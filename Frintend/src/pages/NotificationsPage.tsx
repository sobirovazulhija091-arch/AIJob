import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  CONNECTION_RESPOND_STATUS,
  NOTIFICATION_TYPE,
  getNotifications,
  markNotificationRead,
  respondConnection,
  respondOrganizationMemberInvite,
  type Notification,
} from '../lib/api'
import { getUserId } from '../lib/auth'
import { useI18n } from '../lib/i18n'
import { notifyNavAlertsChanged } from '../lib/navAlerts'
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

function UserPlusIcon() {
  return (
    <svg width="22" height="22" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M16 21v-2a4 4 0 00-4-4H6a4 4 0 00-4 4v2M13 7a4 4 0 11-8 0 4 4 0 018 0zM20 8v6m3-3h-6"
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

function isConnectionRequest(n: Notification): boolean {
  if (n.type === NOTIFICATION_TYPE.connectionRequest) return true
  return n.title.trim().toLowerCase() === 'connection request'
}

function isOrgMemberInvite(n: Notification): boolean {
  return n.type === NOTIFICATION_TYPE.organizationMemberInvite
}

function isActionableInvite(n: Notification): boolean {
  return isConnectionRequest(n) || isOrgMemberInvite(n)
}

export function NotificationsPage() {
  const { t } = useI18n()
  const userId = getUserId()
  const [items, setItems] = useState<Notification[]>([])
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [busyId, setBusyId] = useState<number | null>(null)

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
      notifyNavAlertsChanged()
    }
  }

  useEffect(() => {
    void load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  async function read(id: number) {
    setError('')
    const snapshot = items
    setItems((prev) => prev.map((x) => (x.id === id ? { ...x, isRead: true } : x)))
    notifyNavAlertsChanged()
    try {
      await markNotificationRead(id)
    } catch {
      setItems(snapshot)
      notifyNavAlertsChanged()
      setError(t('notifications.markReadErr'))
    }
  }

  async function respondToInvite(n: Notification, status: number) {
    const rid = n.relatedId
    if (rid == null || rid <= 0) return
    setError('')
    setBusyId(n.id)
    try {
      if (isConnectionRequest(n)) await respondConnection(rid, status)
      else if (isOrgMemberInvite(n)) await respondOrganizationMemberInvite(rid, status)
      else return
      setItems((prev) => prev.map((x) => (x.id === n.id ? { ...x, isRead: true } : x)))
      try {
        await markNotificationRead(n.id)
      } catch {
        /* row already visually read */
      }
      notifyNavAlertsChanged()
    } catch {
      setError(t('notifications.connectionActionErr'))
    } finally {
      setBusyId(null)
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
          Mark items as read when you&apos;ve seen them. <strong>Mark as read</strong> only clears the highlight — use{' '}
          <strong>Accept</strong> or <strong>Decline</strong> to answer a connection or organization invitation.
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

          {error ? <p className="li-notif-inline-err">{error}</p> : null}
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
            {items.map((n) => {
              const inviteUnread = !n.isRead && isActionableInvite(n)
              const hasLink = typeof n.relatedId === 'number' && n.relatedId > 0
              const showConnectStyle = isConnectionRequest(n) || isOrgMemberInvite(n)
              return (
                <article key={n.id} className={`li-notif-card ${n.isRead ? '' : 'li-notif-card--unread'}`}>
                  <div
                    className={`li-notif-icon ${showConnectStyle ? 'li-notif-icon--connect' : ''}`}
                    aria-hidden
                  >
                    {showConnectStyle ? <UserPlusIcon /> : <BellIcon />}
                  </div>
                  <div className="li-notif-body">
                    <h3 className="li-notif-title">{n.title}</h3>
                    <p className="li-notif-msg">{n.message}</p>
                    {inviteUnread && isConnectionRequest(n) && !hasLink ? (
                      <p className="li-notif-msg li-notif-msg--hint">
                        <Link to="/connections">{t('notifications.reviewConnections')}</Link>
                      </p>
                    ) : null}
                  </div>
                  <div className="li-notif-aside">
                    <span className={`li-notif-pill ${n.isRead ? 'li-notif-pill--read' : 'li-notif-pill--new'}`}>
                      {n.isRead ? 'Read' : 'New'}
                    </span>
                    {!n.isRead ? (
                      <div className="li-notif-actions">
                        {inviteUnread && hasLink ? (
                          <div className="li-notif-actions-connect">
                            <button
                              className="li-btn primary li-notif-btn-compact"
                              type="button"
                              disabled={busyId === n.id}
                              onClick={() => void respondToInvite(n, CONNECTION_RESPOND_STATUS.accepted)}
                            >
                              {t('notifications.acceptConnection')}
                            </button>
                            <button
                              className="li-btn li-notif-btn-compact"
                              type="button"
                              disabled={busyId === n.id}
                              onClick={() => void respondToInvite(n, CONNECTION_RESPOND_STATUS.declined)}
                            >
                              {t('notifications.declineConnection')}
                            </button>
                          </div>
                        ) : null}
                        <button
                          className="li-btn li-notif-btn-compact"
                          type="button"
                          disabled={busyId === n.id}
                          title={inviteUnread && hasLink ? t('notifications.markReadHint') : undefined}
                          onClick={() => void read(n.id)}
                        >
                          {t('notifications.markRead')}
                        </button>
                      </div>
                    ) : null}
                  </div>
                </article>
              )
            })}
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
