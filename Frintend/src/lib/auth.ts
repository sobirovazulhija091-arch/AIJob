import { applyTheme, clearStoredTheme } from './theme'

const TOKEN_KEY = 'aijob.token'
const REFRESH_KEY = 'aijob.refresh'

export function setSession(token: string, refreshToken: string) {
  localStorage.setItem(TOKEN_KEY, token)
  localStorage.setItem(REFRESH_KEY, refreshToken)
}

export function clearSession() {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(REFRESH_KEY)
  try {
    localStorage.removeItem('aijob.locale')
  } catch {
    // ignore
  }
  clearStoredTheme()
  applyTheme('light')
}

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function isAuthed(): boolean {
  return Boolean(localStorage.getItem(TOKEN_KEY))
}

function decodePayload(): Record<string, unknown> | null {
  const token = getToken()
  if (!token) return null
  try {
    const payload = token.split('.')[1]
    return JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/'))) as Record<string, unknown>
  } catch {
    return null
  }
}

export function getUserId(): number | null {
  const json = decodePayload()
  if (!json) return null
  try {
    const v = (json['UserId'] ?? json['sub']) as string | number | undefined
    const n = typeof v === 'number' ? v : parseInt(String(v ?? ''), 10)
    return Number.isFinite(n) ? n : null
  } catch {
    return null
  }
}

const NAME_CLAIMS = [
  'name',
  'unique_name',
  'given_name',
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name',
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/name',
]

export function getDisplayName(): string | null {
  const json = decodePayload()
  if (!json) return null
  for (const k of NAME_CLAIMS) {
    const v = json[k]
    if (typeof v === 'string' && v.trim()) return v.trim()
  }
  return null
}

export function getEmail(): string | null {
  const json = decodePayload()
  if (!json) return null
  const v = (json['email'] ?? json['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress']) as unknown
  if (typeof v === 'string' && v.trim()) return v.trim()
  return null
}

function collectRoles(value: unknown, into: Set<string>) {
  if (typeof value === 'string' && value) into.add(value)
  if (Array.isArray(value)) value.forEach((x) => collectRoles(x, into))
}

/** Roles from JWT (Admin, Organization, Candidate). */
export function getRoles(): string[] {
  const json = decodePayload()
  if (!json) return []
  const into = new Set<string>()
  for (const [k, v] of Object.entries(json)) {
    const kl = k.toLowerCase()
    if (kl === 'role' || kl.endsWith('/role') || kl.includes('role')) collectRoles(v, into)
  }
  return [...into]
}

export function hasRole(role: string): boolean {
  return getRoles().includes(role)
}

/** Two-letter initials for avatars (e.g. "Ali Karimov" → "AK"). */
export function initialsFromLabel(name: string): string {
  const parts = name.trim().split(/\s+/).filter(Boolean)
  if (!parts.length) return '?'
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase()
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
}

