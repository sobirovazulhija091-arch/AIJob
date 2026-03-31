import { useEffect, useState } from 'react'
import { Link, Navigate, useParams } from 'react-router-dom'
import { OrgAvatar } from '../components/OrgAvatar'
import { getOrganization, type OrganizationRow } from '../lib/api'
import { useI18n } from '../lib/i18n'
import './organization-profile.css'

export function OrganizationProfilePage() {
  const { orgId: param } = useParams<{ orgId: string }>()
  const { t } = useI18n()
  const id = parseInt(param ?? '', 10)
  const [org, setOrg] = useState<OrganizationRow | null | undefined>(undefined)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!Number.isFinite(id) || id <= 0) {
      setOrg(null)
      return
    }
    void (async () => {
      setError('')
      try {
        const o = await getOrganization(id)
        setOrg(o ?? null)
      } catch (e) {
        setError(e instanceof Error ? e.message : t('directory.error'))
        setOrg(null)
      }
    })()
  }, [id, t])

  if (!Number.isFinite(id) || id <= 0) {
    return <Navigate to="/directory" replace />
  }

  if (org === undefined) {
    return (
      <div className="li-wrap">
        <p className="li-page-sub">{t('directory.loading')}</p>
      </div>
    )
  }

  if (org === null) {
    return (
      <div className="li-grid">
        <aside className="li-panel" />
        <section className="li-card li-card-pad li-stack">
          <p>{error || t('directory.org.notFound')}</p>
          <Link to="/directory">{t('directory.org.back')}</Link>
        </section>
        <aside className="li-panel" />
      </div>
    )
  }

  const about = org.description?.trim()

  return (
    <div className="li-grid">
      <aside className="li-panel">
        <h4 className="li-side-title">{t('directory.section.orgs')}</h4>
        <p className="li-side-text">{t('directory.org.sideHint')}</p>
      </aside>

      <section className="li-org-profile-card">
        <header className="li-org-profile-hero">
          <OrgAvatar id={org.id} name={org.name} logoUrl={org.logoUrl} size="lg" imgAlt={org.name} />
          <div className="li-org-profile-hero-text">
            <p className="li-org-profile-back">
              <Link to="/directory">{t('directory.org.back')}</Link>
            </p>
            <h1 className="li-org-profile-title">{org.name}</h1>
            <div className="li-org-profile-badge-row">
              {org.type ? <span className="li-org-profile-badge">{org.type}</span> : null}
              {org.location ? <span className="li-org-profile-badge">{org.location}</span> : null}
            </div>
          </div>
        </header>

        <div className="li-org-profile-body">
          <dl className="li-org-profile-dl">
            <div>
              <dt className="li-org-profile-dt">{t('directory.org.about')}</dt>
              <dd
                className={
                  about ? 'li-org-profile-dd li-org-profile-dd--about' : 'li-org-profile-dd li-org-profile-dd--empty'
                }
              >
                {about || t('directory.org.noDescription')}
              </dd>
            </div>
          </dl>
        </div>
      </section>

      <aside className="li-panel" />
    </div>
  )
}
