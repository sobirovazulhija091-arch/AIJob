import { useEffect, useState } from 'react'
import { getUsersAdmin, type AdminUserRow } from '../lib/api'
import { useI18n } from '../lib/i18n'

export function AdminPage() {
  const { t } = useI18n()
  const [rows, setRows] = useState<AdminUserRow[]>([])
  const [error, setError] = useState('')

  useEffect(() => {
    void (async () => {
      setError('')
      try {
        setRows(await getUsersAdmin())
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Failed to load users.')
      }
    })()
  }, [])

  return (
    <div className="li-grid">
      <aside className="li-panel">
        <h4 className="li-side-title">{t('nav.admin')}</h4>
        <p className="li-side-text">{t('admin.sub')}</p>
      </aside>
      <section className="li-card li-card-pad li-stack">
        <h2 className="li-page-title">{t('admin.title')}</h2>
        <p className="li-page-sub">{t('admin.sub')}</p>
        {error && <p style={{ color: 'crimson' }}>{error}</p>}
        <div className="li-list">
          {!rows.length && !error ? (
            <div className="li-item">{t('admin.empty')}</div>
          ) : (
            rows.map((u) => (
              <div key={u.id} className="li-item">
                <div className="li-item-title">{u.fullName ?? u.userName ?? u.email ?? `User ${u.id}`}</div>
                <div className="li-item-meta">
                  {u.email ?? ''} · ID {u.id}
                </div>
              </div>
            ))
          )}
        </div>
      </section>
      <aside className="li-panel" />
    </div>
  )
}
