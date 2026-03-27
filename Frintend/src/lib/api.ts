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

async function authedFetch(input: string, init?: RequestInit) {
  const token = (await import('./auth')).getToken()
  const headers = new Headers(init?.headers ?? {})
  if (!headers.has('Content-Type')) headers.set('Content-Type', 'application/json')
  if (token) headers.set('Authorization', `Bearer ${token}`)
  return fetch(input, { ...init, headers })
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

export async function getJobs(): Promise<Job[]> {
  const res = await authedFetch(`/api/Job`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
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
export async function respondConnection(connectionId: number, status: number): Promise<void> {
  const res = await authedFetch(`/api/Connection/${connectionId}/respond`, {
    method: 'PUT',
    body: JSON.stringify({ status }),
  })
  if (!res.ok) throw new Error(await readApiError(res))
}

export type Conversation = { id: number; user1Id: number; user2Id: number }
export async function getConversations(): Promise<Conversation[]> {
  const res = await authedFetch(`/api/Conversation`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<Conversation[]>).data ?? []
}
export async function createConversation(otherUserId: number): Promise<Conversation> {
  const res = await authedFetch(`/api/Conversation`, { method: 'POST', body: JSON.stringify({ otherUserId }) })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<Conversation>).data
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

export type Notification = { id: number; title: string; message: string; isRead: boolean }
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

export type JobApplication = { id: number; jobId: number; userId: number; status: string }
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

export type AdminUserRow = {
  id: number
  email?: string | null
  userName?: string | null
  fullName?: string | null
}

export async function getUsersAdmin(): Promise<AdminUserRow[]> {
  const res = await authedFetch(`/api/User`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<AdminUserRow[]>).data ?? []
}

export type Post = { id: number; content: string; createdAt: string; userId: number }
export async function getFeed(): Promise<Post[]> {
  const res = await authedFetch(`/api/Post/feed`, { method: 'GET' })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<Post[]>).data ?? []
}
export async function createPost(content: string): Promise<void> {
  const res = await authedFetch(`/api/Post`, { method: 'POST', body: JSON.stringify({ content }) })
  if (!res.ok) throw new Error(await readApiError(res))
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
  sourceTextPreview?: string
}

export async function analyzeCv(
  cvText: string,
  opts?: { applyToProfile?: boolean; syncSkills?: boolean },
): Promise<CvAnalysis> {
  const res = await authedFetch(`/api/Ai/analyze-cv`, {
    method: 'POST',
    body: JSON.stringify({
      cvText,
      applyToProfile: opts?.applyToProfile ?? false,
      syncSkills: opts?.syncSkills ?? false,
    }),
  })
  if (!res.ok) throw new Error(await readApiError(res))
  return ((await res.json()) as Response<CvAnalysis>).data
}

