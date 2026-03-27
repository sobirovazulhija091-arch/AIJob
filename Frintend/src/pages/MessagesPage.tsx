import { useEffect, useMemo, useRef, useState } from 'react'
import {
  createConversation,
  getConversations,
  getMessages,
  getMyConnections,
  getPublicProfilesByUsers,
  sendMessage,
  type Connection,
  type Conversation,
  type Message,
} from '../lib/api'
import { getUserId } from '../lib/auth'
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

function formatMsgTime(m: Message): string {
  const raw = m.createdAt ?? m.sentAt
  if (!raw) return ''
  try {
    return new Date(raw).toLocaleString(undefined, { dateStyle: 'short', timeStyle: 'short' })
  } catch {
    return ''
  }
}

export function MessagesPage() {
  const me = getUserId()
  const [convos, setConvos] = useState<Conversation[]>([])
  const [connections, setConnections] = useState<Connection[]>([])
  const [names, setNames] = useState<Record<number, string>>({})
  const [startUserId, setStartUserId] = useState<number>(0)
  const [activeId, setActiveId] = useState<number>(0)
  const [messages, setMessages] = useState<Message[]>([])
  const [text, setText] = useState('')
  const [error, setError] = useState('')
  const chatEndRef = useRef<HTMLDivElement>(null)
  const chatAreaRef = useRef<HTMLDivElement>(null)

  const active = useMemo(() => convos.find((c) => c.id === activeId), [convos, activeId])
  const activeOtherUserId = active ? (active.user1Id === me ? active.user2Id : active.user1Id) : 0
  const activeName = activeOtherUserId ? names[activeOtherUserId] ?? `User ${activeOtherUserId}` : 'Select a chat'
  const activeInitials = initialsFromName(activeName)

  async function loadConvos() {
    setError('')
    try {
      const cs = await getConversations()
      setConvos(cs)
      const ids = cs.flatMap((c) => [c.user1Id, c.user2Id])
      const profiles = await getPublicProfilesByUsers(ids)
      const map: Record<number, string> = {}
      for (const p of profiles) map[p.userId] = p.fullName || `${p.firstName} ${p.lastName}`.trim() || `User ${p.userId}`
      setNames((prev) => ({ ...prev, ...map }))
      if (!activeId && cs.length) setActiveId(cs[0].id)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load conversations.')
    }
  }

  async function loadConnections() {
    try {
      const all = await getMyConnections()
      const accepted = all.filter((c) => String(c.status).toLowerCase().includes('accept'))
      setConnections(accepted)
      const ids = accepted.flatMap((c) => [c.requesterId, c.addresseeId])
      const profiles = await getPublicProfilesByUsers(ids)
      const map: Record<number, string> = {}
      for (const p of profiles) map[p.userId] = p.fullName || `${p.firstName} ${p.lastName}`.trim() || `User ${p.userId}`
      setNames((prev) => ({ ...prev, ...map }))
    } catch {
      // ignore
    }
  }

  async function loadMessages(id: number) {
    setError('')
    try {
      setMessages(await getMessages(id))
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load messages.')
    }
  }

  useEffect(() => {
    void loadConvos()
    void loadConnections()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  useEffect(() => {
    if (activeId) void loadMessages(activeId)
  }, [activeId])

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
      setError(e instanceof Error ? e.message : 'Failed to send.')
    }
  }

  async function startConversation() {
    if (!startUserId) return
    setError('')
    try {
      const c = await createConversation(startUserId)
      await loadConvos()
      setActiveId(c.id)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to start conversation.')
    }
  }

  return (
    <div className="li-msg-app">
      <aside className="li-msg-sidebar">
        <div className="li-msg-sidebar-head">
          <h3>Messages</h3>
          <p>Open a thread or start one from your connections</p>
        </div>
        <div className="li-msg-start">
          <select className="li-select" value={startUserId || ''} onChange={(e) => setStartUserId(parseInt(e.target.value || '0', 10))}>
            <option value="">Start chat from connection</option>
            {connections.map((c) => {
              const other = c.requesterId === me ? c.addresseeId : c.requesterId
              return (
                <option key={c.id} value={other}>
                  {names[other] ?? `User ${other}`}
                </option>
              )
            })}
          </select>
          <button className="li-btn primary" onClick={startConversation} type="button">
            New chat
          </button>
        </div>
        <div className="li-msg-list">
          {convos.length === 0 ? (
            <div className="li-msg-list-empty">
              <span className="li-msg-list-empty-ic" aria-hidden>
                <PlaceholderChatIcon />
              </span>
              <p className="li-msg-list-empty-title">No conversations yet</p>
              <p className="li-msg-list-empty-hint">Choose an accepted connection above to start your first chat.</p>
            </div>
          ) : (
            convos.map((c) => {
              const otherId = c.user1Id === me ? c.user2Id : c.user1Id
              const label = names[otherId] ?? `Conversation #${c.id}`
              return (
                <button
                  key={c.id}
                  className={`li-msg-convo-btn ${c.id === activeId ? 'active' : ''}`}
                  type="button"
                  onClick={() => setActiveId(c.id)}
                >
                  <span className="li-msg-avatar" aria-hidden>
                    {initialsFromName(label)}
                  </span>
                  <span className="li-msg-convo-text">
                    <span className="li-msg-convo-name">{label}</span>
                    <span className="li-msg-convo-sub">Direct message</span>
                  </span>
                </button>
              )
            })
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
            <div className="li-msg-thread-title">{activeName}</div>
            <div className="li-msg-thread-sub">
              {activeId > 0 ? 'Active conversation' : 'Choose someone from the list to open messages'}
            </div>
          </div>
        </div>
        {error ? <div className="li-msg-err">{error}</div> : null}
        <div className="li-msg-chat-area" ref={chatAreaRef}>
          {activeId === 0 ? (
            <div className="li-msg-emptyplate">
              <div className="li-msg-emptyplate-orb" aria-hidden />
              <div className="li-msg-emptyplate-iconwrap">
                <PlaceholderChatIcon />
              </div>
              <h5 className="li-msg-emptyplate-title">Your messages live here</h5>
              <p className="li-msg-emptyplate-text">Pick a conversation on the left, or start a new one from your connections.</p>
            </div>
          ) : null}
          {activeId > 0 && messages.length === 0 ? (
            <div className="li-msg-emptyplate li-msg-emptyplate--compact">
              <div className="li-msg-emptyplate-iconwrap li-msg-emptyplate-iconwrap--sm">
                <PlaceholderChatIcon />
              </div>
              <h5 className="li-msg-emptyplate-title">No messages yet</h5>
              <p className="li-msg-emptyplate-text">Send a short hello below to open the thread.</p>
            </div>
          ) : null}
          {messages.map((m) => {
            const mine = m.senderId === me
            return (
              <div key={m.id} className={`li-msg-row ${mine ? 'me' : ''}`}>
                <div>
                  <div className={`li-msg-bubble ${mine ? 'me' : 'them'}`}>{m.content}</div>
                  {formatMsgTime(m) ? <div className="li-msg-time">{formatMsgTime(m)}</div> : null}
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
            placeholder={activeId > 0 ? 'Write a message…' : 'Select a chat to type…'}
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
            Send
          </button>
        </div>
      </div>

      <aside className="li-msg-aside">
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
            <h4>Messaging tips</h4>
          </div>
          <ul className="li-msg-tips">
            <li>Start chats only with connections you have accepted.</li>
            <li>Keep messages clear and short; follow up in a friendly tone.</li>
            <li>Use Enter to send, Shift+Enter for a new line.</li>
          </ul>
        </div>
      </aside>
    </div>
  )
}
