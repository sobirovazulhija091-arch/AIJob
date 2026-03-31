import { useCallback, useEffect, useMemo, useRef, useState, type FormEvent } from 'react'
import { Link, NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom'
import {
  getConversations,
  getFeed,
  getMemberDirectory,
  getNotifications,
  getProfileByUser,
  searchJobsByTitle,
  type Job,
  type MemberDirectoryEntry,
  type Post,
} from '../lib/api'
import {
  clearSession,
  getEmail,
  getHeaderDisplayName,
  getRoles,
  getUserId,
  HEADER_DISPLAY_EVENT,
  initialsFromLabel,
  isAuthed,
  SESSION_EXPIRED_EVENT,
  setHeaderDisplayNameFromProfile,
} from '../lib/auth'
import { NAV_ALERTS_EVENT, hasUnreadGeneralAlerts } from '../lib/navAlerts'
import { useI18n } from '../lib/i18n'
import './shell.css'

function primaryRoleLabel(roles: string[], t: (key: string) => string): string | null {
  if (roles.includes('Organization')) return t('role.organization')
  if (roles.includes('Candidate')) return t('role.candidate')
  return null
}

function directoryDisplayName(m: MemberDirectoryEntry): string {
  const n = m.fullName?.trim()
  if (n) return n
  if (m.userName?.trim()) return m.userName.trim()
  const e = m.email?.trim()
  if (e) return e.split('@')[0] ?? e
  return ''
}

function truncateText(s: string, max: number): string {
  const t = s.replace(/\s+/g, ' ').trim()
  if (t.length <= max) return t
  return `${t.slice(0, max)}…`
}

export function Shell() {
  const { t } = useI18n()
  const navigate = useNavigate()
  const location = useLocation()
  const aiWide = location.pathname === '/ai'
  const [searchQ, setSearchQ] = useState('')
  const [debouncedSearchQ, setDebouncedSearchQ] = useState('')
  const [suggestOpen, setSuggestOpen] = useState(false)
  const [suggestLoading, setSuggestLoading] = useState(false)
  const [suggestJobs, setSuggestJobs] = useState<Job[]>([])
  const [suggestPeople, setSuggestPeople] = useState<MemberDirectoryEntry[]>([])
  const [suggestPosts, setSuggestPosts] = useState<Post[]>([])
  const searchWrapRef = useRef<HTMLDivElement>(null)
  const directoryCacheRef = useRef<MemberDirectoryEntry[] | null>(null)
  const feedCacheRef = useRef<Post[] | null>(null)
  const [showMsgPip, setShowMsgPip] = useState(false)
  const [showNotifPip, setShowNotifPip] = useState(false)
  const [headerNameTick, setHeaderNameTick] = useState(0)
  const uid = getUserId()
  const roles = getRoles()
  const showApplications = roles.includes('Candidate')
  const showRecruiting = roles.includes('Organization')

  const display = useMemo(
    () => getHeaderDisplayName() ?? getEmail()?.split('@')[0] ?? t('settings.title.fallback'),
    [headerNameTick, t],
  )
  const roleLabel = primaryRoleLabel(roles, t)

  useEffect(() => {
    const onName = () => setHeaderNameTick((n) => n + 1)
    window.addEventListener(HEADER_DISPLAY_EVENT, onName)
    return () => window.removeEventListener(HEADER_DISPLAY_EVENT, onName)
  }, [])

  useEffect(() => {
    if (uid == null) return
    let cancelled = false
    void getProfileByUser(uid)
      .then((p) => {
        if (cancelled || !p) return
        setHeaderDisplayNameFromProfile(p.firstName, p.lastName)
      })
      .catch(() => {
        /* profile optional */
      })
    return () => {
      cancelled = true
    }
  }, [uid])

  useEffect(() => {
    if (location.pathname !== '/jobs') return
    const q = new URLSearchParams(location.search).get('q') ?? ''
    setSearchQ(q)
  }, [location.pathname, location.search])

  useEffect(() => {
    const id = window.setTimeout(() => setDebouncedSearchQ(searchQ), 280)
    return () => window.clearTimeout(id)
  }, [searchQ])

  useEffect(() => {
    directoryCacheRef.current = null
    feedCacheRef.current = null
  }, [uid])

  useEffect(() => {
    function onDocDown(e: MouseEvent) {
      if (!searchWrapRef.current?.contains(e.target as Node)) setSuggestOpen(false)
    }
    document.addEventListener('mousedown', onDocDown)
    return () => document.removeEventListener('mousedown', onDocDown)
  }, [])

  useEffect(() => {
    const q = debouncedSearchQ.trim()
    if (q.length < 1) {
      setSuggestJobs([])
      setSuggestPeople([])
      setSuggestPosts([])
      setSuggestLoading(false)
      return
    }
    let cancelled = false
    setSuggestLoading(true)
    void (async () => {
      try {
        const jobs = (await searchJobsByTitle(q)).slice(0, 8)
        if (cancelled) return
        setSuggestJobs(jobs)
        let people: MemberDirectoryEntry[] = []
        let posts: Post[] = []
        if (isAuthed()) {
          const qq = q.toLowerCase()
          try {
            if (!directoryCacheRef.current) directoryCacheRef.current = await getMemberDirectory()
            people = directoryCacheRef.current
              .filter((m) => {
                const blob = `${directoryDisplayName(m)} ${m.email ?? ''} ${m.userName ?? ''}`.toLowerCase()
                return blob.includes(qq)
              })
              .slice(0, 8)
          } catch {
            /* ignore */
          }
          try {
            if (!feedCacheRef.current) feedCacheRef.current = await getFeed()
            posts = feedCacheRef.current
              .filter((p) => (p.content ?? '').toLowerCase().includes(qq))
              .slice(0, 8)
          } catch {
            /* ignore */
          }
        }
        if (!cancelled) {
          setSuggestPeople(people)
          setSuggestPosts(posts)
        }
      } finally {
        if (!cancelled) setSuggestLoading(false)
      }
    })()
    return () => {
      cancelled = true
    }
  }, [debouncedSearchQ])

  useEffect(() => {
    if (!searchQ.trim()) setSuggestOpen(false)
  }, [searchQ])

  function runGlobalSearch(e: FormEvent) {
    e.preventDefault()
    setSuggestOpen(false)
    const q = searchQ.trim()
    if (q) navigate(`/jobs?q=${encodeURIComponent(q)}`)
    else navigate('/jobs')
  }

  const showSuggestPanel = suggestOpen && searchQ.trim().length >= 1
  const hasSuggestRows = suggestJobs.length + suggestPeople.length + suggestPosts.length > 0

  const refreshNavAlerts = useCallback(async () => {
    if (uid == null) {
      setShowMsgPip(false)
      setShowNotifPip(false)
      return
    }
    let msgPip = false
    let notifPip = false
    try {
      const convos = await getConversations()
      msgPip = convos.some((c) => (c.unreadCount ?? 0) > 0)
    } catch {
      /* keep msg pip */
    }
    try {
      const list = await getNotifications(uid)
      notifPip = hasUnreadGeneralAlerts(list)
    } catch {
      /* keep notif pip */
    }
    setShowMsgPip(msgPip)
    setShowNotifPip(notifPip)
  }, [uid])

  useEffect(() => {
    void refreshNavAlerts()
  }, [refreshNavAlerts, location.pathname])

  useEffect(() => {
    const id = window.setInterval(() => void refreshNavAlerts(), 45000)
    return () => window.clearInterval(id)
  }, [refreshNavAlerts])

  useEffect(() => {
    const onVis = () => {
      if (document.visibilityState === 'visible') void refreshNavAlerts()
    }
    const onCustom = () => void refreshNavAlerts()
    const onSessionExpired = () => navigate('/auth', { replace: true })
    document.addEventListener('visibilitychange', onVis)
    window.addEventListener(NAV_ALERTS_EVENT, onCustom)
    window.addEventListener(SESSION_EXPIRED_EVENT, onSessionExpired)
    return () => {
      document.removeEventListener('visibilitychange', onVis)
      window.removeEventListener(NAV_ALERTS_EVENT, onCustom)
      window.removeEventListener(SESSION_EXPIRED_EVENT, onSessionExpired)
    }
  }, [refreshNavAlerts, navigate])

  return (
    <div className="li-app-shell">
      <header className="li-topbar">
        <div className="li-topbar-inner">
          <div className="li-topbar-row li-topbar-main">
            <Link className="li-brand" to="/settings" title={t('settings.title')}>
              <span className="li-brand-badge">CH</span>
              {t('brand.title')}
            </Link>

            <Link className="li-user-pill" to="/profile" title={getEmail() ?? display}>
              <span className="li-user-avatar">{initialsFromLabel(display)}</span>
              <span className="li-user-meta">
                <span className="li-user-name">{display}</span>
                {roleLabel ? <span className="li-user-role">{roleLabel}</span> : null}
              </span>
            </Link>

            <div className="li-search-wrap" ref={searchWrapRef}>
              <form className="li-search" role="search" onSubmit={runGlobalSearch} title={t('search.hint')}>
                <svg className="li-search-icon" width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden>
                  <circle cx="11" cy="11" r="7" stroke="currentColor" strokeWidth="2" />
                  <path d="M20 20 16 16" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
                </svg>
                <input
                  type="search"
                  autoComplete="off"
                  name="q"
                  placeholder={t('search.placeholder')}
                  aria-label={t('search.placeholder')}
                  aria-expanded={showSuggestPanel}
                  aria-controls="li-global-search-suggest"
                  value={searchQ}
                  onChange={(e) => setSearchQ(e.target.value)}
                  onFocus={() => setSuggestOpen(true)}
                />
                <button type="submit" className="li-search-submit" aria-label={t('search.submit')}>
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" aria-hidden>
                    <circle cx="11" cy="11" r="7" stroke="currentColor" strokeWidth="2" />
                    <path d="M20 20 16 16" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
                  </svg>
                </button>
              </form>
              {showSuggestPanel ? (
                <div
                  id="li-global-search-suggest"
                  className="li-search-suggest"
                  role="listbox"
                  aria-label={t('search.placeholder')}
                  onMouseDown={(e) => e.preventDefault()}
                >
                  {suggestLoading ? (
                    <div className="li-search-suggest-loading">{t('search.suggest.loading')}</div>
                  ) : !hasSuggestRows ? (
                    <div className="li-search-suggest-empty">{t('search.suggest.empty')}</div>
                  ) : (
                    <>
                      {suggestJobs.length ? (
                        <>
                          <div className="li-search-suggest-section">{t('search.suggest.jobs')}</div>
                          {suggestJobs.map((j) => (
                            <button
                              key={`job-${j.id}`}
                              type="button"
                              className="li-search-suggest-row"
                              role="option"
                              onClick={() => {
                                setSuggestOpen(false)
                                navigate(`/jobs?q=${encodeURIComponent(j.title)}`)
                              }}
                            >
                              <span className="li-search-suggest-title">{j.title}</span>
                              <span className="li-search-suggest-sub">
                                {t('search.suggest.jobSub')}
                                {j.location?.trim() ? ` · ${j.location.trim()}` : ''}
                              </span>
                            </button>
                          ))}
                        </>
                      ) : null}
                      {suggestPeople.length ? (
                        <>
                          <div className="li-search-suggest-section">{t('search.suggest.people')}</div>
                          {suggestPeople.map((m) => (
                            <button
                              key={`mem-${m.id}`}
                              type="button"
                              className="li-search-suggest-row"
                              role="option"
                              onClick={() => {
                                setSuggestOpen(false)
                                navigate(`/people/${m.id}`)
                              }}
                            >
                              <span className="li-search-suggest-title">
                                {directoryDisplayName(m) || t('messages.unnamedMember')}
                              </span>
                              <span className="li-search-suggest-sub">{t('search.suggest.personSub')}</span>
                            </button>
                          ))}
                        </>
                      ) : null}
                      {suggestPosts.length ? (
                        <>
                          <div className="li-search-suggest-section">{t('search.suggest.posts')}</div>
                          {suggestPosts.map((p) => (
                            <button
                              key={`post-${p.id}`}
                              type="button"
                              className="li-search-suggest-row"
                              role="option"
                              onClick={() => {
                                setSuggestOpen(false)
                                navigate(`/?post=${p.id}`)
                              }}
                            >
                              <span className="li-search-suggest-title">{truncateText(p.content, 88)}</span>
                              <span className="li-search-suggest-sub">{t('search.suggest.postSub')}</span>
                            </button>
                          ))}
                        </>
                      ) : null}
                    </>
                  )}
                </div>
              ) : null}
            </div>
          </div>

          <div className="li-topbar-row li-topbar-nav">
            <nav className="li-nav" aria-label="Main">
              <div className="li-nav-links">
                <NavLink to="/">{t('nav.home')}</NavLink>
                <NavLink to="/jobs">{t('nav.jobs')}</NavLink>
                {showApplications ? <NavLink to="/applications">{t('nav.applications')}</NavLink> : null}
                {showRecruiting ? (
                  <>
                    <NavLink to="/company">{t('nav.company')}</NavLink>
                    <NavLink to="/recruiting">{t('nav.recruiting')}</NavLink>
                  </>
                ) : null}
                <NavLink to="/connections">{t('nav.connections')}</NavLink>
                <NavLink to="/directory">{t('nav.directory')}</NavLink>
                <NavLink to="/messages">
                  <span className="li-nav-link-inner">
                    {t('nav.messages')}
                    {showMsgPip ? (
                      <span
                        className="li-nav-pip"
                        role="status"
                        title={t('navAlerts.unreadMessages')}
                        aria-label={t('navAlerts.unreadMessages')}
                      />
                    ) : null}
                  </span>
                </NavLink>
                <NavLink to="/notifications">
                  <span className="li-nav-link-inner">
                    {t('nav.notifications')}
                    {showNotifPip ? (
                      <span
                        className="li-nav-pip"
                        role="status"
                        title={t('navAlerts.unreadNotifications')}
                        aria-label={t('navAlerts.unreadNotifications')}
                      />
                    ) : null}
                  </span>
                </NavLink>
                <NavLink to="/ai">{t('nav.ai')}</NavLink>
                <NavLink to="/settings">{t('nav.settings')}</NavLink>
              </div>
              <button
                type="button"
                className="li-nav-logout"
                onClick={() => {
                  clearSession()
                  window.location.href = '/auth'
                }}
              >
                {t('nav.logout')}
              </button>
            </nav>
          </div>
        </div>
      </header>

      <main className={aiWide ? 'li-wrap li-wrap--ai' : 'li-wrap'}>
        <Outlet />
      </main>
    </div>
  )
}
