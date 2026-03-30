import { useEffect, useState } from 'react'
import { Link, Navigate, useParams } from 'react-router-dom'
import { getOrganization, type OrganizationRow } from '../lib/api'
import { useI18n } from '../lib/i18n'

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

  return (
    <div className="li-grid">
      <aside className="li-panel">
        <h4 className="li-side-title">{t('directory.section.orgs')}</h4>
        <p className="li-side-text">{t('directory.org.sideHint')}</p>
      </aside>

      <section className="li-card li-card-pad li-stack">
        <div className="li-page-sub">
          <Link to="/directory">{t('directory.org.back')}</Link>
        </div>
        <h2 className="li-page-title">{org.name}</h2>
        <dl className="li-stack" style={{ margin: 0 }}>
          {org.type ? (
            <>
              <dt className="li-side-title" style={{ fontSize: 12, marginTop: 8 }}>
                {t('directory.org.type')}
              </dt>
              <dd style={{ margin: '4px 0 0' }}>{org.type}</dd>
            </>
          ) : null}
          {org.location ? (
            <>
              <dt className="li-side-title" style={{ fontSize: 12, marginTop: 8 }}>
                {t('directory.org.location')}
              </dt>
              <dd style={{ margin: '4px 0 0' }}>{org.location}</dd>
            </>
          ) : null}
          {org.description ? (
            <>
              <dt className="li-side-title" style={{ fontSize: 12, marginTop: 8 }}>
                {t('directory.org.about')}
              </dt>
              <dd style={{ margin: '4px 0 0', whiteSpace: 'pre-wrap' }}>{org.description}</dd>
            </>
          ) : null}
        </dl>
      </section>

      <aside className="li-panel" />
    </div>
  )
}
