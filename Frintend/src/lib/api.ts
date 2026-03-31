import { clearSession, getRefreshToken, getToken, setSession, SESSION_EXPIRED_EVENT } from './auth'

export type AuthResponse = { token: string; refreshToken: string }

export type ApiError = { message?: string; statusCode?: number }
export type Response<T> = { statusCode: number; description?: string[]; data: T }

async function readApiError(res: globalThis.Response): Promise<string> {
  const text = await res.text()
  if (!text) return `Request failed (${res.status})`
  try {
    const json = JSON.parse(text) as ApiError
    if (json?.message) return json.message
  } catch {
    // ignore parse
  }
  return text
}

let refreshInFlight: Promise<boolean> | null = null

async function tryRefreshSession(): Promise<boolean> {
  if (!refreshInFlight) {
    refreshInFlight = (async () => {
      try {
        const rt = getRefreshToken()
        if (!rt) return false
        const res = await fetch(`/api/Auth/refresh-token`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ refreshToken: rt }),
        })
        if (!res.ok) return false
        const data = (await res.json()) as AuthResponse
        if (!data?.token || !data?.refreshToken) return false
        setSession(data.token, data.refreshToken)
        return true
      } catch {
        return false
      }
    })().finally(() => {
      refreshInFlight = null
    })
  }
  return refreshInFlight
}

async function authedFetch(input: string, init?: RequestInit) {
  const buildHeaders = () => {
    const headers = new Headers(init?.headers ?? {})
    if (!headers.has('Content-Type')) headers.set('Content-Type', 'application/json')
    const token = getToken()
    if (token) headers.set('Authorization', `Bearer ${token}`)
    return headers
  }

  let res = await fetch(input, { ...init, headers: buildHeaders() })
  if (res.status !== 401) return res

  const renewed = await tryRefreshSession()
  if (!renewed) {
    clearSession()
    window.dispatchEvent(new Event(SESSION_EXPIRED_EVENT))
    return res
  }

  return fetch(input, { ...init, headers: buildHeaders() })
}

export async function login(email: string, password: string): Promise<AuthResponse> {
  const res = await fetch(`/api/Auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  })
  if (!res.ok) throw new Error(await readApiError(res))
  return (await res.json()) as AuthResponse
}

export async function register(payload: {
  fullName: string
  email: string
  phoneNumber: string
  password: string
  role: 'Candidate' | 'Organization'
}): Promise<AuthResponse> {
  const res = await fetch(`/api/Auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  if (!res.ok) throw new Error(await readApiError(res))
  return (await res.json()) as AuthResponse
}

export async function forgotPassword(email: string): Promise<void> {
  const res = await fetch(`/api/Auth/forgot-password`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email }),
  })
  if (!res.ok) throw new Error(await readApiError(res))
}

export async function resetPasswordWithToken(payload: {
  email: string
  token: string
  newPassword: string
}): Promise<void> {
  const res = await fetch(`/api/Auth/reset-password`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      email: payload.email,
      token: payload.token,
      newPassword: payload.newPassword,
    }),
  })
  if (!res.ok) throw new Error(await readApiError(res))
}

export type Job = {
  id: number
  title: string
  description?: string
  salaryMin: number
  salaryMax: number
  location?: string
  jobType: string
  experienceLevel: string
}

export type JobCategoryRow = { id: number; name: string }

export async function getJobCategories(): Promise<JobCategoryRow[]> {
  const res = await authedFetch(`/api/JobCategory`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<JobCategoryRow[]>).data ?? []
}

/** POST /api/JobCategory — Organization role; name must be unique (case-insensitive). */
export async function createJobCategory(name: string): Promise<void> {
  const res = await authedFetch(`/api/JobCategory`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name: name.trim() }),
  })
  if (!res.ok) throw new Error(await readApiError(res))
}

/** Payload for POST /api/Job (Organization). Enum fields as server expects (e.g. FullTime, Junior). */
export type CreateJobPayload = {
  organizationId: number
  title: string
  description?: string
  salaryMin: number
  salaryMax: number
  location?: string
  jobType: string
  experienceLevel: string
  experienceRequired: number
  categoryId: number
}

export async function createJob(body: CreateJobPayload): Promise<void> {
  const res = await authedFetch(`/api/Job`, { method: 'POST', body: JSON.stringify(body) })
  if (!res.ok) throw new Error(await readApiError(res))
}

export async function getJobs(): Promise<Job[]> {
  const res = await authedFetch(`/api/Job`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  const json = (await res.json()) as Response<Job[]>
  return json.data ?? []
}

/** Jobs for organizations the current user belongs to (employer recruiting). */
export async function getMyJobs(): Promise<Job[]> {
  const res = await authedFetch(`/api/Job/mine`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<Job[]>).data ?? []
}

/** Server-side match on title, description, and skills (partial text ok). Returns [] if none or bad request. */
export async function searchJobsByTitle(title: string): Promise<Job[]> {
  const t = title.trim()
  if (!t) return []
  const res = await authedFetch(`/api/Job/search?title=${encodeURIComponent(t)}`, { method: 'GET' })
  const json = (await res.json()) as Response<Job[]>
  return json.data ?? []
}

export type Connection = { id: number; requesterId: number; addresseeId: number; status: string }
export async function getMyConnections(): Promise<Connection[]> {
  const res = await authedFetch(`/api/Connection/my`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<Connection[]>).data ?? []
}
export async function getPendingConnections(): Promise<Connection[]> {
  const res = await authedFetch(`/api/Connection/pending`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<Connection[]>).data ?? []
}
export async function sendConnectionByEmail(email: string): Promise<void> {
  const res = await authedFetch(`/api/Connection/send-by-email`, {
    method: 'POST',
    body: JSON.stringify({ email }),
  })
  if (!res.ok) throw new Error(await readApiError(res))
}
/** Matches backend `ConnectionStatus`: Pending=0, Accepted=1, Declined=2 */
export const CONNECTION_RESPOND_STATUS = { accepted: 1, declined: 2 } as const

export async function respondConnection(connectionId: number, status: number): Promise<void> {
  const res = await authedFetch(`/api/Connection/${connectionId}/respond`, {
    method: 'PUT',
    body: JSON.stringify({ status }),
  })
  if (!res.ok) throw new Error(await readApiError(res))
}

export type Conversation = {
  id: number
  user1Id: number
  user2Id: number
  createdAt?: string
  unreadCount: number
  lastMessagePreview?: string | null
  lastMessageAt?: string | null
}
export async function getConversations(): Promise<Conversation[]> {
  const res = await authedFetch(`/api/Conversation`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  const rows = ((await res.json()) as Response<Conversation[]>).data ?? []
  return rows.map((c) => ({
    ...c,
    unreadCount: c.unreadCount ?? 0,
    lastMessagePreview: c.lastMessagePreview ?? null,
    lastMessageAt: c.lastMessageAt ?? null,
  }))
}
export async function createConversation(otherUserId: number): Promise<Conversation> {
  const res = await authedFetch(`/api/Conversation`, { method: 'POST', body: JSON.stringify({ otherUserId }) })
  if (!res.ok) throw new Error(await readApiError(res))
  const row = (await res.json()) as Response<Conversation>
  const d = row.data
  if (!d) throw new Error('No conversation returned')
  return {
    ...d,
    unreadCount: d.unreadCount ?? 0,
    lastMessagePreview: d.lastMessagePreview ?? null,
    lastMessageAt: d.lastMessageAt ?? null,
  }
}

export async function deleteConversation(conversationId: number): Promise<void> {
  // POST …/delete avoids 405s when DELETE is blocked or not routed (e.g. stale API, picky proxies).
  const res = await authedFetch(`/api/Conversation/${conversationId}/delete`, { method: 'POST' })
  if (!res.ok) throw new Error(await readApiError(res))
}

export type Message = {
  id: number
  conversationId: number
  senderId: number
  content: string
  /** API may return `createdAt` (backend) or `sentAt` */
  createdAt?: string
  sentAt?: string
}
export async function getMessages(conversationId: number): Promise<Message[]> {
  const res = await authedFetch(`/api/Message/by-conversation/${conversationId}`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<Message[]>).data ?? []
}
export async function sendMessage(conversationId: number, content: string): Promise<void> {
  const res = await authedFetch(`/api/Message`, { method: 'POST', body: JSON.stringify({ conversationId, content }) })
  if (!res.ok) throw new Error(await readApiError(res))
}

/** Removes the message for both chat participants (server requires you to be in the conversation). */
export async function deleteMessage(messageId: number): Promise<void> {
  const res = await authedFetch(`/api/Message/${messageId}`, { method: 'DELETE' })
  if (!res.ok) throw new Error(await readApiError(res))
}

/** Matches backend `NotificationType`; `MessageReceived` = 3 */
/** Backend `NotificationType`; connection request = 4; org member invite = 7 */
export const NOTIFICATION_TYPE = {
  connectionRequest: 4,
  connectionAccepted: 5,
  organizationMemberInvite: 7,
} as const

export type Notification = {
  id: number
  title: string
  message: string
  isRead: boolean
  type?: number
  /** e.g. Connection.Id for connection-request notifications */
  relatedId?: number | null
}
export async function getNotifications(userId: number): Promise<Notification[]> {
  const res = await authedFetch(`/api/Notification/by-user/${userId}`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<Notification[]>).data ?? []
}
export async function markNotificationRead(id: number): Promise<void> {
  const res = await authedFetch(`/api/Notification/${id}/read`, { method: 'PATCH' })
  if (!res.ok) throw new Error(await readApiError(res))
}

export type UserSettings = { theme: string; brandColor: string; emailNotifications: boolean; pushNotifications: boolean; language: string }
export async function getMySettings(): Promise<UserSettings> {
  const res = await authedFetch(`/api/UserSettings/me`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<UserSettings>).data
}
export async function updateMySettings(payload: UserSettings): Promise<void> {
  const res = await authedFetch(`/api/UserSettings/me`, { method: 'PUT', body: JSON.stringify(payload) })
  if (!res.ok) throw new Error(await readApiError(res))
}

export type UserProfile = { id: number; userId: number; firstName: string; lastName: string; aboutMe: string; experienceYears: number; expectedSalary: number; cvFileUrl?: string }
export type UserPublicProfile = { userId: number; fullName: string; firstName: string; lastName: string }

export type MemberProfile = {
  userId: number
  firstName: string
  lastName: string
  fullName: string
  aboutMe: string
  experienceYears: number
}
export async function getMemberProfile(userId: number): Promise<MemberProfile | null> {
  const res = await authedFetch(`/api/UserProfile/member/${userId}`, { method: 'GET' })
  if (res.status === 404) return null
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<MemberProfile>).data ?? null
}
export async function getProfileByUser(userId: number): Promise<UserProfile | null> {
  const res = await authedFetch(`/api/UserProfile/by-user/${userId}`, { method: 'GET' })
  if (res.status === 404) return null
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<UserProfile>).data
}
export async function getPublicProfilesByUsers(userIds: number[]): Promise<UserPublicProfile[]> {
  const unique = Array.from(new Set(userIds.filter((x) => Number.isFinite(x) && x > 0)))
  if (!unique.length) return []
  const res = await authedFetch(`/api/UserProfile/public-by-users`, { method: 'POST', body: JSON.stringify(unique) })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<UserPublicProfile[]>).data ?? []
}
export async function createProfile(payload: Omit<UserProfile, 'id'>): Promise<void> {
  const res = await authedFetch(`/api/UserProfile`, { method: 'POST', body: JSON.stringify(payload) })
  if (!res.ok) throw new Error(await readApiError(res))
}
export async function updateProfile(id: number, payload: UserProfile): Promise<void> {
  const res = await authedFetch(`/api/UserProfile/${id}`, { method: 'PUT', body: JSON.stringify(payload) })
  if (!res.ok) throw new Error(await readApiError(res))
}

export type JobApplication = { id: number; jobId: number; userId: number; status: string | number }
export async function getApplicationsByUser(userId: number): Promise<JobApplication[]> {
  const res = await authedFetch(`/api/JobApplication/by-user/${userId}`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<JobApplication[]>).data ?? []
}
export async function applyToJob(jobId: number, userId: number): Promise<void> {
  const res = await authedFetch(`/api/JobApplication`, { method: 'POST', body: JSON.stringify({ jobId, userId }) })
  if (!res.ok) throw new Error(await readApiError(res))
}

export async function getApplicationsByJob(jobId: number): Promise<JobApplication[]> {
  const res = await authedFetch(`/api/JobApplication/by-job/${jobId}`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<JobApplication[]>).data ?? []
}

/** ApplicationStatus enum on server: Pending=1, Accepted=2, Rejected=3, Interview=4 */
export async function changeApplicationStatus(applicationId: number, status: number): Promise<void> {
  const res = await authedFetch(`/api/JobApplication/${applicationId}/status`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(status),
  })
  if (!res.ok) throw new Error(await readApiError(res))
}

export type MemberDirectoryEntry = {
  id: number
  fullName?: string | null
  userName?: string | null
  email?: string | null
  role: string
}

export async function getMemberDirectory(): Promise<MemberDirectoryEntry[]> {
  const res = await authedFetch(`/api/User/directory`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<MemberDirectoryEntry[]>).data ?? []
}

export type OrganizationRow = {
  id: number
  name: string
  description?: string | null
  type: string
  location?: string | null
  /** Public logo HTTPS URL when set; otherwise UI uses generated initials. */
  logoUrl?: string | null
}

export async function getOrganizations(): Promise<OrganizationRow[]> {
  const res = await authedFetch(`/api/Organization`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<OrganizationRow[]>).data ?? []
}

/** Organizations the current user is a member of (Company / Recruiting). */
export async function getMyOrganizations(): Promise<OrganizationRow[]> {
  const res = await authedFetch(`/api/Organization/mine`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<OrganizationRow[]>).data ?? []
}

export async function getOrganization(id: number): Promise<OrganizationRow | null> {
  const res = await authedFetch(`/api/Organization/${id}`, { method: 'GET' })
  if (res.status === 404) return null
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<OrganizationRow>).data ?? null
}

export type CreateOrganizationPayload = {
  name: string
  description?: string | null
  type?: string | null
  location?: string | null
  logoUrl?: string | null
}

/** POST /api/Organization — returns created organization (employer role). */
export async function createOrganization(payload: CreateOrganizationPayload): Promise<OrganizationRow> {
  const res = await authedFetch(`/api/Organization`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      name: payload.name,
      description: payload.description ?? undefined,
      type: payload.type ?? 'Company',
      location: payload.location ?? undefined,
      logoUrl: payload.logoUrl?.trim() || undefined,
    }),
  })
  if (!res.ok) throw new Error(await readApiError(res))
  const raw = (await res.json()) as Response<Record<string, unknown>>
  const o = raw.data as Record<string, unknown> | undefined
  if (!o || typeof o.id !== 'number') throw new Error('Invalid organization response')
  return {
    id: o.id as number,
    name: String(o.name ?? ''),
    description: (o.description as string | null) ?? null,
    type: String(o.type ?? 'Company'),
    location: (o.location as string | null) ?? null,
    logoUrl: (o.logoUrl as string | null | undefined) ?? null,
  }
}

export type OrganizationMemberRow = {
  id: number
  organizationId: number
  userId: number
  role: string
}

export async function getOrganizationMembers(organizationId: number): Promise<OrganizationMemberRow[]> {
  const res = await authedFetch(`/api/OrganizationMember/by-organization/${organizationId}`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<OrganizationMemberRow[]>).data ?? []
}

export async function addOrganizationMember(payload: {
  organizationId: number
  userId: number
  role: string
}): Promise<void> {
  const res = await authedFetch(`/api/OrganizationMember`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  if (!res.ok) throw new Error(await readApiError(res))
}

/** Sends an in-app notification; the invitee must accept before they appear in the org. */
export async function inviteOrganizationMember(payload: {
  organizationId: number
  userId: number
  role: string
}): Promise<void> {
  const res = await authedFetch(`/api/OrganizationMember/invite`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  if (!res.ok) throw new Error(await readApiError(res))
}

export async function respondOrganizationMemberInvite(invitationId: number, status: number): Promise<void> {
  const res = await authedFetch(`/api/OrganizationMember/invitation/${invitationId}/respond`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ status }),
  })
  if (!res.ok) throw new Error(await readApiError(res))
}

export type Post = {
  id: number
  content: string
  createdAt: string
  userId: number
  likeCount: number
  likedByMe: boolean
  repostOfPostId?: number | null
  /** Present when this card is a repost: author of the original post */
  repostSourceUserId?: number | null
  repostCount: number
  imageUrl?: string | null
}

export type PostLikeState = { postId: number; likeCount: number; likedByMe: boolean }

export async function getFeed(): Promise<Post[]> {
  const res = await authedFetch(`/api/Post/feed`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  const rows = ((await res.json()) as Response<Post[]>).data ?? []
  return rows.map((p) => ({
    ...p,
    likeCount: p.likeCount ?? 0,
    likedByMe: p.likedByMe ?? false,
    repostOfPostId: p.repostOfPostId ?? null,
    repostSourceUserId: p.repostSourceUserId ?? null,
    repostCount: p.repostCount ?? 0,
  }))
}

export async function togglePostLike(postId: number): Promise<PostLikeState> {
  const res = await authedFetch(`/api/Post/${postId}/like`, { method: 'POST' })
  if (!res.ok) throw new Error(await readApiError(res))
  const body = (await res.json()) as Response<PostLikeState>
  const row = body.data
  if (!row) throw new Error('No like state returned')
  return row
}

export async function repostPost(postId: number): Promise<void> {
  const res = await authedFetch(`/api/Post/${postId}/repost`, { method: 'POST' })
  if (!res.ok) throw new Error(await readApiError(res))
}
export async function createPost(content: string): Promise<void> {
  const res = await authedFetch(`/api/Post`, { method: 'POST', body: JSON.stringify({ content }) })
  if (!res.ok) throw new Error(await readApiError(res))
}

export async function deletePost(postId: number): Promise<void> {
  const res = await authedFetch(`/api/Post/${postId}`, { method: 'DELETE' })
  if (!res.ok) throw new Error(await readApiError(res))
}

export type PostComment = {
  id: number
  postId: number
  userId: number
  content: string
  createdAt: string
}

export async function getPostComments(postId: number): Promise<PostComment[]> {
  const res = await authedFetch(`/api/Post/${postId}/comments`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<PostComment[]>).data ?? []
}

export async function addPostComment(postId: number, content: string): Promise<PostComment> {
  const res = await authedFetch(`/api/Post/${postId}/comments`, {
    method: 'POST',
    body: JSON.stringify({ content }),
  })
  if (!res.ok) throw new Error(await readApiError(res))
  const body = (await res.json()) as Response<PostComment>
  const row = body.data
  if (!row) throw new Error('No comment returned')
  return row
}

export async function askAi(prompt: string): Promise<string> {
  const res = await authedFetch(`/api/Ai/ask`, { method: 'POST', body: JSON.stringify({ prompt }) })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<string>).data ?? ''
}

export type CvAnalysis = {
  fullName: string
  firstName: string
  lastName: string
  professionalSummary: string
  experienceYears: number
  skills: string[]
  education: string[]
  recommendedRoles: string[]
  notes: string[]
  missingOrWeakSections: string[]
  howToImprove: string[]
  helpfulResources: string[]
  sourceTextPreview?: string
}

function stringList(v: unknown): string[] {
  if (!Array.isArray(v)) return []
  return v.filter((x): x is string => typeof x === 'string' && x.trim().length > 0)
}

/** Handles camelCase / PascalCase payloads and `data` vs `Data` envelope. */
function normalizeCvAnalysisPayload(raw: unknown): CvAnalysis | null {
  if (!raw || typeof raw !== 'object') return null
  const envelope = raw as Record<string, unknown>
  const d = (envelope.data ?? envelope.Data) as Record<string, unknown> | undefined
  if (!d || typeof d !== 'object' || Array.isArray(d)) return null

  const exp = d.experienceYears ?? d.ExperienceYears
  const years = typeof exp === 'number' && Number.isFinite(exp) ? Math.max(0, Math.floor(exp)) : parseInt(String(exp ?? '0'), 10) || 0

  return {
    fullName: String(d.fullName ?? d.FullName ?? ''),
    firstName: String(d.firstName ?? d.FirstName ?? ''),
    lastName: String(d.lastName ?? d.LastName ?? ''),
    professionalSummary: String(d.professionalSummary ?? d.ProfessionalSummary ?? ''),
    experienceYears: years,
    skills: stringList(d.skills ?? d.Skills),
    education: stringList(d.education ?? d.Education),
    recommendedRoles: stringList(d.recommendedRoles ?? d.RecommendedRoles),
    notes: stringList(d.notes ?? d.Notes),
    missingOrWeakSections: stringList(d.missingOrWeakSections ?? d.MissingOrWeakSections),
    howToImprove: stringList(d.howToImprove ?? d.HowToImprove),
    helpfulResources: stringList(d.helpfulResources ?? d.HelpfulResources),
    sourceTextPreview:
      typeof d.sourceTextPreview === 'string'
        ? d.sourceTextPreview
        : typeof d.SourceTextPreview === 'string'
          ? d.SourceTextPreview
          : undefined,
  }
}

/** Upload PDF/DOC/DOCX for CV analysis; returns relative path e.g. /uploads/cv/…. Pass to analyzeCv as cvFileUrl. */
export async function uploadCvFile(file: File): Promise<string> {
  const doPost = () => {
    const token = getToken()
    const headers = new Headers()
    if (token) headers.set('Authorization', `Bearer ${token}`)
    const form = new FormData()
    form.append('file', file)
    return fetch('/api/Upload/cv', { method: 'POST', body: form, headers })
  }

  let res = await doPost()
  if (res.status === 401) {
    const renewed = await tryRefreshSession()
    if (!renewed) {
      clearSession()
      window.dispatchEvent(new Event(SESSION_EXPIRED_EVENT))
      throw new Error('Session expired')
    }
    res = await doPost()
  }
  if (!res.ok) throw new Error(await readApiError(res))
  const json = (await res.json()) as Response<string>
  const url = json.data?.trim()
  if (!url) throw new Error('No file URL returned')
  return url
}

export async function analyzeCv(opts: {
  cvText?: string
  cvFileUrl?: string
  applyToProfile?: boolean
  syncSkills?: boolean
}): Promise<CvAnalysis> {
  const body: Record<string, unknown> = {
    applyToProfile: opts.applyToProfile ?? false,
    syncSkills: opts.syncSkills ?? false,
  }
  if (opts.cvFileUrl?.trim()) body.cvFileUrl = opts.cvFileUrl.trim()
  if (opts.cvText != null && opts.cvText.trim() !== '') body.cvText = opts.cvText.trim()

  const res = await authedFetch(`/api/Ai/analyze-cv`, {
    method: 'POST',
    body: JSON.stringify(body),
  })
  if (!res.ok) throw new Error(await readApiError(res))
  let json: unknown
  try {
    json = await res.json()
  } catch {
    throw new Error('Invalid response from CV analysis')
  }
  const row = normalizeCvAnalysisPayload(json)
  if (!row) throw new Error('No analysis returned from server')
  return row
}

export type SkillGapResult = {
  matchScore: number
  fitSummary: string
  strengths: string[]
  missingSkills: string[]
  nextSteps: string[]
}

export async function getSkillGap(userId: number, jobId: number): Promise<SkillGapResult> {
  const res = await authedFetch(`/api/Ai/skill-gap/${userId}/${jobId}`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<SkillGapResult>).data
}

export type DraftLetterResult = { subject: string; content: string }

export async function draftCoverLetter(payload: {
  jobId: number
  tone?: string
  extraContext?: string
}): Promise<DraftLetterResult> {
  const res = await authedFetch(`/api/Ai/draft-cover-letter`, {
    method: 'POST',
    body: JSON.stringify({
      jobId: payload.jobId,
      tone: payload.tone || undefined,
      extraContext: payload.extraContext || undefined,
    }),
  })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<DraftLetterResult>).data
}

export type JobImproveResult = {
  improvedTitle: string
  improvedDescription: string
  suggestedSkills: string[]
  suggestedResponsibilities: string[]
  suggestedBenefits: string[]
}

export async function improveJob(payload: { jobId: number; applyToJob?: boolean }): Promise<JobImproveResult> {
  const res = await authedFetch(`/api/Ai/improve-job`, {
    method: 'POST',
    body: JSON.stringify({
      jobId: payload.jobId,
      applyToJob: payload.applyToJob ?? false,
    }),
  })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<JobImproveResult>).data
}

