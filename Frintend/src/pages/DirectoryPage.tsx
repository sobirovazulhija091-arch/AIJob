import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { OrgAvatar } from '../components/OrgAvatar'
import { getMemberDirectory, getOrganizations, type MemberDirectoryEntry, type OrganizationRow } from '../lib/api'
import { getUserId, initialsFromLabel } from '../lib/auth'
import { useI18n } from '../lib/i18n'
import './directory.css'

function IconChevron() {
  return (
    <svg className="li-dir-card-chevron" width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M9 6l6 6-6 6" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  )
}

function roleLabelKey(role: string): string {
  if (role === 'Organization') return 'role.organization'
  if (role === 'Candidate') return 'role.candidate'
  return 'directory.memberFallback'
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

  const sortedOrgs = useMemo(
    () => [...orgs].sort((a, b) => a.name.localeCompare(b.name, undefined, { sensitivity: 'base' })),
    [orgs],
  )

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
    <div className="li-dir-page">
      <section className="li-card li-card-pad li-dir-main">
        <header className="li-dir-hero">
          <h2 className="li-page-title">{t('directory.title')}</h2>
          <p className="li-page-sub">{t('directory.sub')}</p>
          {loading ? <p className="li-dir-status">{t('directory.loading')}</p> : null}
          {error ? <p className="li-dir-error">{error}</p> : null}
        </header>

        <div className="li-dir-split">
          <div className="li-dir-column li-dir-column--orgs" aria-labelledby="dir-orgs-heading">
            <div className="li-dir-section li-dir-section--orgs">
              <div className="li-dir-section-head">
                <h3 className="li-dir-section-title" id="dir-orgs-heading">
                  {t('directory.section.orgs')}
                </h3>
                {!loading && !error ? <span className="li-dir-count">{sortedOrgs.length}</span> : null}
              </div>
              <p className="li-dir-section-lead">{t('directory.section.orgs.lead')}</p>
              <div className="li-dir-grid li-dir-grid--orgs">
                {!loading && !sortedOrgs.length && !error ? (
                  <div className="li-dir-empty">{t('directory.empty.orgs')}</div>
                ) : null}
                {sortedOrgs.map((o) => (
                  <Link key={o.id} to={`/organizations/${o.id}`} className="li-dir-card li-dir-card--org">
                    <OrgAvatar id={o.id} name={o.name} logoUrl={o.logoUrl} size="sm" className="li-dir-org-avatar" />
                    <div className="li-dir-card-body">
                      <div className="li-dir-card-title">{o.name}</div>
                      <div className="li-dir-card-meta">
                        {[o.location, o.type].filter(Boolean).join(' · ') || t('directory.org.noExtra')}
                      </div>
                    </div>
                    <IconChevron />
                  </Link>
                ))}
              </div>
            </div>
          </div>

          <div className="li-dir-column li-dir-column--people" aria-labelledby="dir-people-heading">
            <div className="li-dir-section li-dir-section--people">
              <div className="li-dir-section-head">
                <h3 className="li-dir-section-title" id="dir-people-heading">
                  {t('directory.section.people')}
                </h3>
                {!loading && !error ? <span className="li-dir-count">{people.length}</span> : null}
              </div>
              <p className="li-dir-section-lead">{t('directory.section.people.lead')}</p>
              <div className="li-dir-grid li-dir-grid--people">
                {!loading && !people.length && !error ? (
                  <div className="li-dir-empty">{t('directory.empty.people')}</div>
                ) : null}
                {people.map((p) => {
                  const isSelf = me != null && p.id === me
                  const title = personTitle(p) || t('messages.unnamedMember')
                  const secondary = [p.email, isSelf ? `(${t('directory.you')})` : null].filter(Boolean).join(' ')
                  const initials = initialsFromLabel(title)
                  return (
                    <Link key={p.id} to={`/people/${p.id}`} className="li-dir-card li-dir-card--person">
                      <span className="li-dir-card-icon li-dir-card-icon--person" aria-hidden>
                        {initials || '?'}
                      </span>
                      <div className="li-dir-card-body">
                        <div className="li-dir-card-title">{title}</div>
                        <div className="li-dir-card-meta">
                          {t(roleLabelKey(p.role))}
                          {secondary ? ` · ${secondary}` : ''}
                        </div>
                      </div>
                      <IconChevron />
                    </Link>
                  )
                })}
              </div>
            </div>
          </div>
        </div>
      </section>

      <div className="li-dir-page-asides">
        <aside className="li-panel li-dir-tip">
          <h4 className="li-side-title">{t('directory.sideTitle')}</h4>
          <p className="li-side-text">{t('directory.sideHint')}</p>
        </aside>
        <aside className="li-panel li-dir-tip">
          <h4 className="li-side-title">{t('directory.asideMoreTitle')}</h4>
          <p className="li-side-text">{t('directory.asideMoreBody')}</p>
        </aside>
      </div>
    </div>
  )
}
