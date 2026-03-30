import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  getMyConnections,
  getPendingConnections,
  getPublicProfilesByUsers,
  respondConnection,
  sendConnectionByEmail,
  type Connection,
} from '../lib/api'
import { getUserId, initialsFromLabel } from '../lib/auth'
import './connections.css'

function isValidEmail(raw: string): boolean {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(raw.trim())
}

export function ConnectionsPage() {
  const me = getUserId()
  const [mine, setMine] = useState<Connection[]>([])
  const [pending, setPending] = useState<Connection[]>([])
  const [names, setNames] = useState<Record<number, string>>({})
  const [email, setEmail] = useState('')
  const [msg, setMsg] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  async function load() {
    setError('')
    setLoading(true)
    try {
      const my = await getMyConnections()
      const pd = await getPendingConnections()
      setMine(my)
      setPending(pd)
      const ids = [...new Set([...my, ...pd].flatMap((c) => [c.requesterId, c.addresseeId]))]
      if (ids.length) {
        const profiles = await getPublicProfilesByUsers(ids)
        const map: Record<number, string> = {}
        for (const p of profiles)
          map[p.userId] = p.fullName || `${p.firstName} ${p.lastName}`.trim() || `User ${p.userId}`
        setNames(map)
      } else {
        setNames({})
      }
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load connections.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void load()
  }, [])

  async function send() {
    setMsg('')
    setError('')
    const trimmed = email.trim()
    if (!trimmed) return
    if (!isValidEmail(trimmed)) {
      setError('Enter a valid email address.')
      return
    }
    try {
      await sendConnectionByEmail(trimmed)
      setEmail('')
      setMsg('Request sent.')
      await load()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to send.')
    }
  }

  async function respond(id: number, status: number) {
    setMsg('')
    setError('')
    try {
      await respondConnection(id, status)
      await load()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to respond.')
    }
  }

  function otherUserId(c: Connection): number {
    if (!me) return c.requesterId
    return c.requesterId === me ? c.addresseeId : c.requesterId
  }

  return (
    <div className="li-grid">
      <aside className="li-panel">
        <h4 className="li-side-title">People</h4>
        <p className="li-side-text">
          Grow your network with people you know or want to collaborate with. Accepted connections can unlock messaging
          and richer discovery in your feed.
        </p>
      </aside>

      <section className="li-conn-col">
        <div className="li-conn-hero">
          <h2 className="li-page-title">Connections</h2>
          <p className="li-page-sub">Invite by email and manage incoming requests.</p>

          <div className="li-conn-invite">
            <label className="li-stack" style={{ gap: 0 }}>
              <span className="li-label">Invite someone</span>
              <span className="li-field-hint" style={{ marginBottom: 10 }}>
                Enter their CareerHub account email. They&apos;ll get a request they can accept or decline.
              </span>
            </label>
            <div className="li-conn-invite-row">
              <input
                className="li-input"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="name@company.com"
                autoComplete="email"
              />
              <button
                className="li-btn primary"
                onClick={send}
                type="button"
                disabled={!email.trim() || !isValidEmail(email)}
              >
                Send request
              </button>
            </div>
            {msg ? <p style={{ color: 'green', margin: '12px 0 0', fontSize: 14 }}>{msg}</p> : null}
            {error ? <p style={{ color: 'crimson', margin: '12px 0 0', fontSize: 14 }}>{error}</p> : null}
          </div>
        </div>

        {loading ? <div className="li-conn-loading">Loading connections…</div> : null}

        {!loading ? (
          <>
            <div className="li-conn-section">
              <div className="li-conn-section-head">
                <h3>My connections</h3>
                <span className="li-conn-count">{mine.length}</span>
              </div>
              {mine.length === 0 ? (
                <div className="li-conn-empty">
                  <strong>No connections yet</strong>
                  When someone accepts your invite—or you accept theirs—they will appear here.
                </div>
              ) : (
                <div className="li-conn-list">
                  {mine.map((c) => {
                    const oid = otherUserId(c)
                    const label = names[oid] ?? `User ${oid}`
                    return (
                      <div key={c.id} className="li-conn-card">
                        <div className="li-conn-avatar" aria-hidden>
                          {initialsFromLabel(label)}
                        </div>
                        <div className="li-conn-body">
                          <p className="li-conn-name">
                            <Link className="li-conn-name-link" to={`/people/${oid}`}>
                              {label}
                            </Link>
                          </p>
                          <p className="li-conn-meta">
                            {me
                              ? c.requesterId === me
                                ? 'You sent the invitation · accepted'
                                : 'They reached out · you accepted'
                              : 'Member of your network'}
                          </p>
                          <span className="li-conn-badge">Connected</span>
                        </div>
                      </div>
                    )
                  })}
                </div>
              )}
            </div>

            <div className="li-conn-section">
              <div className="li-conn-section-head">
                <h3>Incoming requests</h3>
                <span className="li-conn-count">{pending.length}</span>
              </div>
              {pending.length === 0 ? (
                <div className="li-conn-empty">
                  <strong>All caught up</strong>
                  No pending invitations. New requests from others will show up here for you to accept or decline.
                </div>
              ) : (
                <div className="li-conn-list">
                  {pending.map((c) => {
                    const oid = c.requesterId === me ? c.addresseeId : c.requesterId
                    const label = names[oid] ?? `User ${oid}`
                    return (
                      <div key={c.id} className="li-conn-card">
                        <div className="li-conn-avatar" aria-hidden>
                          {initialsFromLabel(label)}
                        </div>
                        <div className="li-conn-body">
                          <p className="li-conn-name">
                            <Link className="li-conn-name-link" to={`/people/${oid}`}>
                              {label}
                            </Link>
                          </p>
                          <p className="li-conn-meta">Wants to connect with you on CareerHub.</p>
                          <span className="li-conn-badge li-conn-badge--pending">Pending</span>
                        </div>
                        <div className="li-conn-pending-actions">
                          <button className="li-btn primary" type="button" onClick={() => respond(c.id, 1)}>
                            Accept
                          </button>
                          <button className="li-btn" type="button" onClick={() => respond(c.id, 2)}>
                            Decline
                          </button>
                        </div>
                      </div>
                    )
                  })}
                </div>
              )}
            </div>
          </>
        ) : null}
      </section>

      <aside className="li-panel">
        <h4 className="li-side-title">Tips</h4>
        <p className="li-side-text">
          Short, personal notes (when your product supports them) improve acceptance. Connect with people you’ve actually
          interacted with so your network stays trustworthy.
        </p>
      </aside>
    </div>
  )
}
