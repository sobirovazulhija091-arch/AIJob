import { useEffect, useState } from 'react'
import { getUsersAdmin, type AdminUserRow } from '../lib/api'
import { useI18n } from '../lib/i18n'
import { initialsFromLabel } from '../lib/auth'
import './admin.css'

function roleLabelKey(role: string | null | undefined): string {
  if (role === 'Admin') return 'role.platformTeam'
  if (role === 'Organization') return 'role.organization'
  if (role === 'Candidate') return 'role.candidate'
  return 'role.candidate'
}

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
        setError(e instanceof Error ? e.message : t('admin.loadError'))
      }
    })()
  }, [t])

  return (
    <div className="li-grid">
      <aside className="li-panel">
        <h4 className="li-side-title">{t('admin.sideTitle')}</h4>
        <p className="li-side-text">{t('admin.sideHint')}</p>
      </aside>
      <section className="li-card li-card-pad li-stack">
        <h2 className="li-page-title">{t('admin.pageTitle')}</h2>
        <p className="li-page-sub">{t('admin.lead')}</p>
        {error ? <p className="li-admin-error">{error}</p> : null}
        <h3 className="li-subsection-title">{t('admin.accountsHeading')}</h3>
        <div className="li-admin-list">
          {!rows.length && !error ? (
            <div className="li-feed-empty">{t('admin.empty')}</div>
          ) : (
            rows.map((u) => {
              const title = u.fullName?.trim() || u.userName?.trim() || u.email?.trim() || t('admin.unnamedUser')
              const role = u.accountRole ? t(roleLabelKey(u.accountRole)) : null
              const emailLine = u.email?.trim()
              const initials = initialsFromLabel(title)
              return (
                <div key={u.id} className="li-admin-card">
                  <div className="li-admin-avatar" aria-hidden>
                    {initials}
                  </div>
                  <div className="li-admin-body">
                    <p className="li-admin-name">{title}</p>
                    <div className="li-admin-meta">
                      {role ? <span className="li-admin-badge">{role}</span> : null}
                      {emailLine ? <span>{emailLine}</span> : null}
                    </div>
                  </div>
                </div>
              )
            })
          )}
        </div>
      </section>
      <aside className="li-panel" />
    </div>
  )
}
