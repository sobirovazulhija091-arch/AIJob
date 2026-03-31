import { useEffect, useMemo, useRef, useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import {
  createConversation,
  deleteConversation,
  deleteMessage,
  getConversations,
  getMemberDirectory,
  getMessages,
  getPublicProfilesByUsers,
  sendMessage,
  type Conversation,
  type MemberDirectoryEntry,
  type Message,
} from '../lib/api'
import { getUserId } from '../lib/auth'
import { useI18n } from '../lib/i18n'
import { notifyNavAlertsChanged } from '../lib/navAlerts'
import './messages.css'

function PlaceholderChatIcon() {
  return (
    <svg className="li-msg-ph-icon" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M8 10h8M8 14h5"
        stroke="currentColor"
        strokeWidth="1.75"
        strokeLinecap="round"
      />
      <path
        d="M4 6a2 2 0 012-2h12a2 2 0 012 2v9a2 2 0 01-2 2h-6.35L8 21v-4H6a2 2 0 01-2-2V6z"
        stroke="currentColor"
        strokeWidth="1.65"
        strokeLinejoin="round"
      />
    </svg>
  )
}

function initialsFromName(name: string): string {
  const p = name.trim().split(/\s+/).filter(Boolean)
  if (!p.length) return '?'
  if (p.length === 1) return p[0].slice(0, 2).toUpperCase()
  return (p[0][0] + p[p.length - 1][0]).toUpperCase()
}

function formatBubbleTime(m: Message): string {
  const raw = m.createdAt ?? m.sentAt
  if (!raw) return ''
  try {
    return new Date(raw).toLocaleTimeString(undefined, { hour: '2-digit', minute: '2-digit' })
  } catch {
    return ''
  }
}

function formatConvoListTime(iso: string | null | undefined, t: (key: string) => string): string {
  if (!iso) return ''
  try {
    const d = new Date(iso)
    const now = new Date()
    const sod = (x: Date) => new Date(x.getFullYear(), x.getMonth(), x.getDate()).getTime()
    const diffDays = Math.round((sod(now) - sod(d)) / 86400000)
    if (diffDays === 0) return d.toLocaleTimeString(undefined, { hour: '2-digit', minute: '2-digit' })
    if (diffDays === 1) return t('messages.yesterday')
    if (diffDays > 0 && diffDays < 7) return d.toLocaleDateString(undefined, { weekday: 'short' })
    return d.toLocaleDateString(undefined, { month: 'short', day: 'numeric' })
  } catch {
    return ''
  }
}

function truncatePreview(s: string, max: number): string {
  const x = s.replace(/\s+/g, ' ').trim()
  if (x.length <= max) return x
  return `${x.slice(0, max)}…`
}

function directoryDisplayName(m: MemberDirectoryEntry): string {
  const n = m.fullName?.trim()
  if (n) return n
  if (m.userName?.trim()) return m.userName.trim()
  const e = m.email?.trim()
  if (e) return e.split('@')[0] ?? e
  return ''
}

function convoMatchesQuery(
  c: Conversation,
  me: number | null,
  names: Record<number, string>,
  q: string,
): boolean {
  if (!q.trim()) return true
  if (me == null) return true
  const needle = q.trim().toLowerCase()
  const otherId = c.user1Id === me ? c.user2Id : c.user1Id
  const name = (names[otherId] ?? '').toLowerCase()
  const preview = (c.lastMessagePreview ?? '').toLowerCase()
  return name.includes(needle) || preview.includes(needle)
}

function memberMatchesQuery(m: MemberDirectoryEntry, q: string): boolean {
  if (!q.trim()) return false
  const needle = q.trim().toLowerCase()
  const label = directoryDisplayName(m).toLowerCase()
  const email = (m.email ?? '').toLowerCase()
  const un = (m.userName ?? '').toLowerCase()
  return label.includes(needle) || email.includes(needle) || un.includes(needle)
}

const TIPS_PREF_KEY = 'aijob.messages.showTips'

export function MessagesPage() {
  const { t } = useI18n()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const me = getUserId()
  const [tipsOpen, setTipsOpen] = useState(() => {
    try {
      return localStorage.getItem(TIPS_PREF_KEY) === '1'
    } catch {
      return false
    }
  })
  const [convos, setConvos] = useState<Conversation[]>([])
  const [directory, setDirectory] = useState<MemberDirectoryEntry[]>([])
  const [names, setNames] = useState<Record<number, string>>({})
  const [sidebarQuery, setSidebarQuery] = useState('')
  const [activeId, setActiveId] = useState<number>(0)
  const [messages, setMessages] = useState<Message[]>([])
  const [text, setText] = useState('')
  const [error, setError] = useState('')
  const chatEndRef = useRef<HTMLDivElement>(null)
  const chatAreaRef = useRef<HTMLDivElement>(null)

  const roleShort = (role: string) => {
    const r = role?.trim()
    if (r === 'Organization') return t('role.organization')
    if (r === 'Candidate') return t('role.candidate')
    return t('directory.memberFallback')
  }

  const sidebarQueryTrim = sidebarQuery.trim()

  const filteredConvos = useMemo(() => {
    if (!sidebarQueryTrim) return convos
    return convos.filter((c) => convoMatchesQuery(c, me, names, sidebarQueryTrim))
  }, [convos, me, names, sidebarQueryTrim])

  const directoryMatches = useMemo(() => {
    if (!sidebarQueryTrim || me == null) return []
    const partnerIds = new Set(convos.map((c) => (c.user1Id === me ? c.user2Id : c.user1Id)))
    return directory
      .filter((m) => m.id !== me && memberMatchesQuery(m, sidebarQueryTrim))
      .filter((m) => !partnerIds.has(m.id))
      .slice(0, 30)
  }, [convos, directory, me, sidebarQueryTrim])

  const active = useMemo(() => convos.find((c) => c.id === activeId), [convos, activeId])
  const activeOtherUserId = active ? (active.user1Id === me ? active.user2Id : active.user1Id) : 0
  const activeName = activeOtherUserId
    ? names[activeOtherUserId] ?? t('messages.unnamedMember')
    : t('messages.selectChat')
  const activeInitials = initialsFromName(activeName)

  async function loadConvos(opts?: { skipAutoPick?: boolean; silent?: boolean }) {
    if (!opts?.silent) setError('')
    try {
      const cs = await getConversations()
      setConvos(cs)
      const ids = cs.flatMap((c) => [c.user1Id, c.user2Id])
      const profiles = await getPublicProfilesByUsers(ids)
      const map: Record<number, string> = {}
      for (const p of profiles) map[p.userId] = p.fullName || `${p.firstName} ${p.lastName}`.trim() || `User ${p.userId}`
      setNames((prev) => ({ ...prev, ...map }))
      if (!opts?.skipAutoPick && !activeId && cs.length) setActiveId(cs[0].id)
    } catch (e) {
      if (!opts?.silent) setError(e instanceof Error ? e.message : t('messages.error.loadConvos'))
    }
  }

  async function loadDirectory() {
    try {
      const entries = await getMemberDirectory()
      setDirectory(entries)
      const map: Record<number, string> = {}
      for (const m of entries) {
        const d = directoryDisplayName(m)
        if (d) map[m.id] = d
      }
      setNames((prev) => ({ ...prev, ...map }))
    } catch {
      // ignore — picker stays empty; user may retry by refresh
    }
  }

  async function loadMessages(id: number) {
    setError('')
    try {
      setMessages(await getMessages(id))
    } catch (e) {
      setError(e instanceof Error ? e.message : t('messages.error.loadMsgs'))
      return
    }
    await loadConvos({ skipAutoPick: true, silent: true })
    notifyNavAlertsChanged()
  }

  useEffect(() => {
    void loadConvos()
    void loadDirectory()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const tipsVisible = tipsOpen

  function persistTipsPref(open: boolean) {
    setTipsOpen(open)
    try {
      localStorage.setItem(TIPS_PREF_KEY, open ? '1' : '0')
    } catch {
      /* ignore */
    }
  }

  useEffect(() => {
    const raw = searchParams.get('with')
    if (raw == null || raw === '') return
    const withId = parseInt(raw, 10)
    if (!Number.isFinite(withId) || withId <= 0 || (me != null && withId === me)) {
      navigate('/messages', { replace: true })
      return
    }
    let cancelled = false
    void (async () => {
      setError('')
      try {
        const c = await createConversation(withId)
        if (cancelled) return
        await loadConvos({ skipAutoPick: true })
        notifyNavAlertsChanged()
        setActiveId(c.id)
        navigate('/messages', { replace: true })
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : t('messages.error.start'))
          navigate('/messages', { replace: true })
        }
      }
    })()
    return () => {
      cancelled = true
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchParams, me])

  useEffect(() => {
    if (activeId) void loadMessages(activeId)
  }, [activeId])

  useEffect(() => {
    if (!activeId || convos.some((c) => c.id === activeId)) return
    const next = convos[0]?.id ?? 0
    setActiveId(next)
  }, [convos, activeId])

  useEffect(() => {
    chatEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages, activeId])

  async function send() {
    if (!activeId || !text.trim()) return
    setError('')
    try {
      await sendMessage(activeId, text.trim())
      setText('')
      await loadMessages(activeId)
    } catch (e) {
      setError(e instanceof Error ? e.message : t('messages.error.send'))
    }
  }

  async function removeMessage(m: Message) {
    if (!activeId) return
    if (!window.confirm(t('messages.deleteConfirm'))) return
    setError('')
    try {
      await deleteMessage(m.id)
      await loadMessages(activeId)
      await loadConvos({ skipAutoPick: true, silent: true })
      notifyNavAlertsChanged()
    } catch (e) {
      setError(e instanceof Error ? e.message : t('messages.error.delete'))
    }
  }

  async function removeConversation() {
    if (!activeId) return
    if (!window.confirm(t('messages.deleteConversationConfirm'))) return
    setError('')
    try {
      await deleteConversation(activeId)
      setMessages([])
      await loadConvos({ skipAutoPick: true })
      notifyNavAlertsChanged()
    } catch (e) {
      setError(e instanceof Error ? e.message : t('messages.error.deleteConversation'))
    }
  }

  async function openOrStartChat(withUserId: number) {
    if (me == null || withUserId === me) return
    setError('')
    setSidebarQuery('')
    const existing = convos.find(
      (c) =>
        (c.user1Id === me && c.user2Id === withUserId) || (c.user2Id === me && c.user1Id === withUserId),
    )
    if (existing) {
      setActiveId(existing.id)
      return
    }
    try {
      const c = await createConversation(withUserId)
      await loadConvos({ skipAutoPick: true })
      notifyNavAlertsChanged()
      setActiveId(c.id)
    } catch (e) {
      setError(e instanceof Error ? e.message : t('messages.error.start'))
    }
  }

  function renderConvoRows(list: Conversation[]) {
    return list.map((c) => {
      const otherId = me != null && c.user1Id === me ? c.user2Id : c.user1Id
      const label = names[otherId] ?? t('messages.unnamedMember')
      const unread = (c.unreadCount ?? 0) > 0
      const previewRaw = c.lastMessagePreview?.trim()
      const preview = previewRaw
        ? truncatePreview(previewRaw, 52)
        : t('messages.threadEmptyTitle')
      return (
        <div
          key={c.id}
          className={`li-msg-convo-row ${c.id === activeId ? 'active' : ''} ${unread ? 'unread' : ''}`}
        >
          <button className="li-msg-convo-btn" type="button" onClick={() => setActiveId(c.id)}>
            <span className="li-msg-avatar" aria-hidden>
              {initialsFromName(label)}
            </span>
            <span className="li-msg-convo-main">
              <span className="li-msg-convo-top">
                <span className="li-msg-convo-name">{label}</span>
                <span className="li-msg-convo-time">{formatConvoListTime(c.lastMessageAt, t)}</span>
              </span>
              <span className="li-msg-convo-bottom">
                <span className={`li-msg-convo-preview ${previewRaw ? '' : 'li-msg-convo-preview--muted'}`}>
                  {preview}
                </span>
                {(c.unreadCount ?? 0) > 0 ? (
                  <span className="li-msg-unread-badge" aria-label={t('nav.messages')}>
                    {(c.unreadCount ?? 0) > 99 ? '99+' : c.unreadCount}
                  </span>
                ) : null}
              </span>
            </span>
          </button>
          <Link className="li-msg-convo-profile" to={`/people/${otherId}`} title={t('member.viewProfile')}>
            ↗
          </Link>
        </div>
      )
    })
  }

  return (
    <div
      className={`li-msg-app ${tipsVisible ? 'li-msg-app--with-tips' : 'li-msg-app--tips-collapsed'}`}
    >
      <aside className="li-msg-sidebar">
        <div className="li-msg-sidebar-head li-msg-sidebar-head--compact">
          <h3>{t('nav.messages')}</h3>
        </div>
        <div className="li-msg-search-wrap">
          <label className="li-msg-search-label" htmlFor="li-msg-sidebar-search">
            <span className="li-msg-search-ic" aria-hidden>
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none">
                <path
                  d="M10.5 18a7.5 7.5 0 1 1 0-15 7.5 7.5 0 0 1 0 15Z"
                  stroke="currentColor"
                  strokeWidth="1.7"
                />
                <path d="M16 16l5 5" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" />
              </svg>
            </span>
            <input
              id="li-msg-sidebar-search"
              className="li-msg-search"
              type="search"
              autoComplete="off"
              placeholder={t('messages.searchPlaceholder')}
              value={sidebarQuery}
              onChange={(e) => setSidebarQuery(e.target.value)}
            />
          </label>
        </div>
        <div className="li-msg-list">
          {sidebarQueryTrim ? (
            <>
              {filteredConvos.length > 0 ? (
                <>
                  {directoryMatches.length > 0 ? (
                    <div className="li-msg-list-section">{t('messages.searchChats')}</div>
                  ) : null}
                  {renderConvoRows(filteredConvos)}
                </>
              ) : null}
              {directoryMatches.length > 0 ? (
                <>
                  <div className="li-msg-list-section">{t('messages.searchPeople')}</div>
                  {directoryMatches.map((m) => {
                    const label = directoryDisplayName(m) || t('messages.unnamedMember')
                    return (
                      <div key={`dir-${m.id}`} className="li-msg-convo-row li-msg-convo-row--person">
                        <button
                          className="li-msg-convo-btn"
                          type="button"
                          onClick={() => void openOrStartChat(m.id)}
                        >
                          <span className="li-msg-avatar" aria-hidden>
                            {initialsFromName(label)}
                          </span>
                          <span className="li-msg-convo-main">
                            <span className="li-msg-convo-top">
                              <span className="li-msg-convo-name">{label}</span>
                              <span className="li-msg-person-role">{roleShort(m.role)}</span>
                            </span>
                            <span className="li-msg-convo-bottom">
                              <span className="li-msg-convo-preview li-msg-convo-preview--muted">
                                {t('messages.newChat')}
                              </span>
                            </span>
                          </span>
                        </button>
                        <Link className="li-msg-convo-profile" to={`/people/${m.id}`} title={t('member.viewProfile')}>
                          ↗
                        </Link>
                      </div>
                    )
                  })}
                </>
              ) : null}
              {filteredConvos.length === 0 && directoryMatches.length === 0 ? (
                <div className="li-msg-list-empty li-msg-list-empty--search">
                  <p className="li-msg-list-empty-title">{t('messages.searchNoResults')}</p>
                </div>
              ) : null}
            </>
          ) : convos.length === 0 ? (
            <div className="li-msg-list-empty">
              <span className="li-msg-list-empty-ic" aria-hidden>
                <PlaceholderChatIcon />
              </span>
              <p className="li-msg-list-empty-title">{t('messages.emptyListTitle')}</p>
              <p className="li-msg-list-empty-hint">{t('messages.emptyListHint')}</p>
            </div>
          ) : (
            renderConvoRows(convos)
          )}
        </div>
      </aside>

      <div className="li-msg-thread-wrap">
        <div className="li-msg-thread-header">
          {activeId > 0 ? (
            <span className="li-msg-avatar" aria-hidden>
              {activeInitials}
            </span>
          ) : (
            <span className="li-msg-avatar li-msg-avatar--placeholder" aria-hidden>
              <PlaceholderChatIcon />
            </span>
          )}
          <div className="li-msg-thread-heading">
            <div className="li-msg-thread-title">
              {activeId > 0 && activeOtherUserId ? (
                <Link className="li-msg-thread-title-link" to={`/people/${activeOtherUserId}`}>
                  {activeName}
                </Link>
              ) : (
                activeName
              )}
            </div>
            <div className="li-msg-thread-sub">
              {activeId > 0 ? t('messages.threadActiveSub') : t('messages.threadIdleSub')}
            </div>
          </div>
          {activeId > 0 ? (
            <button type="button" className="li-msg-thread-del" onClick={() => void removeConversation()}>
              {t('messages.deleteConversation')}
            </button>
          ) : null}
        </div>
        {error ? <div className="li-msg-err">{error}</div> : null}
        <div className="li-msg-chat-area" ref={chatAreaRef}>
          {activeId === 0 ? (
            <div className="li-msg-emptyplate">
              <div className="li-msg-emptyplate-orb" aria-hidden />
              <div className="li-msg-emptyplate-iconwrap">
                <PlaceholderChatIcon />
              </div>
              <h5 className="li-msg-emptyplate-title">{t('messages.threadIdleTitle')}</h5>
              <p className="li-msg-emptyplate-text">{t('messages.emptyPlateHint')}</p>
            </div>
          ) : null}
          {activeId > 0 && messages.length === 0 ? (
            <div className="li-msg-emptyplate li-msg-emptyplate--compact">
              <div className="li-msg-emptyplate-iconwrap li-msg-emptyplate-iconwrap--sm">
                <PlaceholderChatIcon />
              </div>
              <h5 className="li-msg-emptyplate-title">{t('messages.threadEmptyTitle')}</h5>
              <p className="li-msg-emptyplate-text">{t('messages.threadEmptyHint')}</p>
            </div>
          ) : null}
          {messages.map((m) => {
            const mine = m.senderId === me
            const bubbleTime = formatBubbleTime(m)
            return (
              <div key={m.id} className={`li-msg-row ${mine ? 'me' : ''}`}>
                <div className={`li-msg-bubble-wrap ${mine ? 'me' : 'them'}`}>
                  <div className={`li-msg-bubble ${mine ? 'me' : 'them'}`}>
                    <span className="li-msg-bubble-text">{m.content}</span>
                    {bubbleTime ? <span className="li-msg-bubble-meta">{bubbleTime}</span> : null}
                    <button
                      type="button"
                      className="li-msg-bubble-del"
                      title={t('messages.deleteMessage')}
                      aria-label={t('messages.deleteMessage')}
                      onClick={() => void removeMessage(m)}
                    >
                      ×
                    </button>
                  </div>
                </div>
              </div>
            )
          })}
          <div ref={chatEndRef} />
        </div>
        <div className={`li-msg-composer ${activeId === 0 ? 'li-msg-composer--disabled' : ''}`}>
          <textarea
            value={text}
            onChange={(e) => setText(e.target.value)}
            placeholder={activeId > 0 ? t('messages.composerPlaceholder') : t('messages.composerDisabled')}
            rows={2}
            disabled={activeId === 0}
            onKeyDown={(e) => {
              if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault()
                void send()
              }
            }}
          />
          <button
            className="li-btn primary"
            onClick={send}
            type="button"
            disabled={activeId === 0 || !text.trim()}
          >
            {t('messages.send')}
          </button>
        </div>
      </div>

      {tipsVisible ? (
        <aside className="li-msg-aside" aria-label={t('messages.tipsTitle')}>
          <div className="li-msg-aside-inner">
            <div className="li-msg-aside-head">
              <span className="li-msg-aside-badge" aria-hidden>
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none">
                  <path
                    d="M12 16v-4M12 8h.01M22 12c0 5.523-4.477 10-10 10S2 17.523 2 12 6.477 2 12 2s10 4.477 10 10z"
                    stroke="currentColor"
                    strokeWidth="1.8"
                    strokeLinecap="round"
                  />
                </svg>
              </span>
              <h4>{t('messages.tipsTitle')}</h4>
              <button
                type="button"
                className="li-msg-aside-close"
                onClick={() => persistTipsPref(false)}
                aria-label={t('messages.hideTips')}
              >
                ×
              </button>
            </div>
            <ul className="li-msg-tips">
              <li>{t('messages.tip1')}</li>
              <li>{t('messages.tip2')}</li>
              <li>{t('messages.tip3')}</li>
            </ul>
          </div>
        </aside>
      ) : null}
    </div>
  )
}
