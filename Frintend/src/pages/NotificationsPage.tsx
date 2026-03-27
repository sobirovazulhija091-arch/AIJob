import { useEffect, useMemo, useState } from 'react'
import { getNotifications, markNotificationRead, type Notification } from '../lib/api'
import { getUserId } from '../lib/auth'
import './notifications.css'

function BellIcon() {
  return (
    <svg width="22" height="22" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M15 17h5l-1.4-1.4A2 2 0 0118 14.2V11a6 6 0 10-12 0v3.2c0 .5-.2 1-.6 1.4L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"
        stroke="currentColor"
        strokeWidth="1.75"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  )
}

function ManageIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M12 3v2M12 19v2M3 12h2M19 12h2M5.6 5.6l1.4 1.4M17 17l1.4 1.4M5.6 18.4l1.4-1.4M17 7l1.4-1.4"
        stroke="currentColor"
        strokeWidth="1.75"
        strokeLinecap="round"
      />
      <circle cx="12" cy="12" r="4" stroke="currentColor" strokeWidth="1.75" />
    </svg>
  )
}

function TipsIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M12 16v-4M12 8h.01M22 12c0 5.523-4.477 10-10 10S2 17.523 2 12 6.477 2 12 2s10 4.477 10 10z"
        stroke="currentColor"
        strokeWidth="1.75"
        strokeLinecap="round"
      />
    </svg>
  )
}

export function NotificationsPage() {
  const userId = getUserId()
  const [items, setItems] = useState<Notification[]>([])
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  const unreadCount = useMemo(() => items.filter((n) => !n.isRead).length, [items])

  async function load() {
    if (!userId) return
    setError('')
    setLoading(true)
    try {
      setItems(await getNotifications(userId))
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load notifications.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  async function read(id: number) {
    setError('')
    try {
      await markNotificationRead(id)
      await load()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to mark as read.')
    }
  }

  return (
    <div className="li-grid">
      <aside className="li-panel">
        <span className="li-notif-aside-icon" aria-hidden>
          <ManageIcon />
        </span>
        <h4 className="li-side-title">Manage</h4>
        <p className="li-side-text">
          Mark items as read when you&apos;ve acted on them. Unread highlights help you see what matters first without
          missing follow-ups.
        </p>
      </aside>

      <section className="li-notif-col">
        <div className="li-notif-hero">
          <h2 className="li-page-title">Notifications</h2>
          <p className="li-page-sub">Everything important in one place—inbox-style updates from your CareerHub activity.</p>

          {!loading ? (
            <div className="li-notif-summary" role="status">
              <span className="li-notif-summary-stat">
                <span className={unreadCount ? 'li-notif-dot' : 'li-notif-dot li-notif-dot--muted'} aria-hidden />
                {unreadCount === 0
                  ? 'All caught up — no unread notifications'
                  : `${unreadCount} unread notification${unreadCount === 1 ? '' : 's'}`}
              </span>
            </div>
          ) : null}

          {error ? <p style={{ color: 'crimson', margin: '14px 0 0', fontSize: 14 }}>{error}</p> : null}
        </div>

        {loading ? <div className="li-notif-loading">Loading notifications…</div> : null}

        {!loading && items.length === 0 ? (
          <div className="li-notif-empty">
            <strong>You&apos;re all set</strong>
            No notifications right now. When something needs your attention—applications, connections, or system
            updates—it will show up here.
          </div>
        ) : null}

        {!loading && items.length > 0 ? (
          <div className="li-notif-list">
            {items.map((n) => (
              <article key={n.id} className={`li-notif-card ${n.isRead ? '' : 'li-notif-card--unread'}`}>
                <div className="li-notif-icon" aria-hidden>
                  <BellIcon />
                </div>
                <div className="li-notif-body">
                  <h3 className="li-notif-title">{n.title}</h3>
                  <p className="li-notif-msg">{n.message}</p>
                </div>
                <div className="li-notif-aside">
                  <span className={`li-notif-pill ${n.isRead ? 'li-notif-pill--read' : 'li-notif-pill--new'}`}>
                    {n.isRead ? 'Read' : 'New'}
                  </span>
                  {!n.isRead ? (
                    <div className="li-notif-actions">
                      <button className="li-btn" type="button" onClick={() => read(n.id)}>
                        Mark read
                      </button>
                    </div>
                  ) : null}
                </div>
              </article>
            ))}
          </div>
        ) : null}
      </section>

      <aside className="li-panel">
        <span className="li-notif-aside-icon" aria-hidden>
          <TipsIcon />
        </span>
        <h4 className="li-side-title">Tips</h4>
        <p className="li-side-text">
          Tackle unread items first. Clearing notifications regularly keeps this list useful and mirrors how strong teams
          handle alerts on LinkedIn-style products.
        </p>
      </aside>
    </div>
  )
}
