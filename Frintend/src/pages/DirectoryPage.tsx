import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getMemberDirectory, getOrganizations, type MemberDirectoryEntry, type OrganizationRow } from '../lib/api'
import { getUserId } from '../lib/auth'
import { useI18n } from '../lib/i18n'

function roleLabelKey(role: string): string {
  if (role === 'Admin') return 'role.platformTeam'
  if (role === 'Organization') return 'role.organization'
  if (role === 'Candidate') return 'role.candidate'
  return 'role.candidate'
}

function personTitle(p: MemberDirectoryEntry): string {
  const n = p.fullName?.trim()
  if (n) return n
  if (p.userName?.trim()) return p.userName.trim()
  if (p.email?.trim()) return p.email.trim()
  return ''
}

export function DirectoryPage() {
  const { t } = useI18n()
  const me = getUserId()
  const [people, setPeople] = useState<MemberDirectoryEntry[]>([])
  const [orgs, setOrgs] = useState<OrganizationRow[]>([])
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    void (async () => {
      setError('')
      setLoading(true)
      try {
        const [p, o] = await Promise.all([getMemberDirectory(), getOrganizations()])
        setPeople(p)
        setOrgs(o)
      } catch (e) {
        setError(e instanceof Error ? e.message : t('directory.error'))
      } finally {
        setLoading(false)
      }
    })()
  }, [t])

  return (
    <div className="li-grid">
      <aside className="li-panel">
        <h4 className="li-side-title">{t('directory.sideTitle')}</h4>
        <p className="li-side-text">{t('directory.sideHint')}</p>
      </aside>

      <section className="li-card li-card-pad li-stack">
        <h2 className="li-page-title">{t('directory.title')}</h2>
        <p className="li-page-sub">{t('directory.sub')}</p>
        {loading ? <p className="li-page-sub">{t('directory.loading')}</p> : null}
        {error ? <p style={{ color: 'crimson' }}>{error}</p> : null}

        <h3 className="li-side-title" style={{ marginTop: 8 }}>
          {t('directory.section.people')}
        </h3>
        <div className="li-list">
          {!loading && !people.length && !error ? (
            <div className="li-item">{t('directory.empty.people')}</div>
          ) : null}
          {people.map((p) => {
            const isSelf = me != null && p.id === me
            const secondary = [p.email, isSelf ? `(${t('directory.you')})` : null].filter(Boolean).join(' ')
            return (
              <Link key={p.id} to={`/people/${p.id}`} className="li-item" style={{ textDecoration: 'none', color: 'inherit' }}>
                <div className="li-item-title">{personTitle(p) || t('messages.unnamedMember')}</div>
                <div className="li-item-meta">
                  {t(roleLabelKey(p.role))}
                  {secondary ? ` · ${secondary}` : ''}
                </div>
              </Link>
            )
          })}
        </div>

        <h3 className="li-side-title" style={{ marginTop: 20 }}>
          {t('directory.section.orgs')}
        </h3>
        <div className="li-list">
          {!loading && !orgs.length && !error ? (
            <div className="li-item">{t('directory.empty.orgs')}</div>
          ) : null}
          {orgs.map((o) => (
            <Link
              key={o.id}
              to={`/organizations/${o.id}`}
              className="li-item"
              style={{ textDecoration: 'none', color: 'inherit' }}
            >
              <div className="li-item-title">{o.name}</div>
              <div className="li-item-meta">
                {[o.location, o.type].filter(Boolean).join(' · ') || t('directory.org.noExtra')}
              </div>
            </Link>
          ))}
        </div>
      </section>

      <aside className="li-panel" />
    </div>
  )
}
