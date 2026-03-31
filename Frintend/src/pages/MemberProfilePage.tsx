import { useEffect, useState } from 'react'
import { Link, Navigate, useParams } from 'react-router-dom'
import { getMemberProfile, type MemberProfile } from '../lib/api'
import { getUserId, initialsFromLabel } from '../lib/auth'
import { useI18n } from '../lib/i18n'
import './member-profile.css'

export function MemberProfilePage() {
  const { userId: param } = useParams<{ userId: string }>()
  const { t } = useI18n()
  const me = getUserId()
  const targetId = parseInt(param ?? '', 10)

  const [profile, setProfile] = useState<MemberProfile | null | undefined>(undefined)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!Number.isFinite(targetId) || targetId <= 0) {
      setProfile(null)
      return
    }
    if (me != null && targetId === me) {
      setProfile(undefined)
      return
    }
    void (async () => {
      setError('')
      try {
        const p = await getMemberProfile(targetId)
        setProfile(p ?? null)
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Failed to load profile.')
        setProfile(null)
      }
    })()
  }, [targetId, me])

  if (!Number.isFinite(targetId) || targetId <= 0) {
    return <Navigate to="/" replace />
  }

  if (me != null && targetId === me) {
    return <Navigate to="/profile" replace />
  }

  const displayName =
    profile?.fullName?.trim() ||
    `${profile?.firstName ?? ''} ${profile?.lastName ?? ''}`.trim() ||
    `User ${targetId}`
  const initials = initialsFromLabel(displayName)

  return (
    <div className="li-grid">
      <aside className="li-panel">
        <h4 className="li-side-title">{t('nav.connections')}</h4>
        <p className="li-side-text">{t('member.sidebarHint')}</p>
      </aside>

      <section className="li-card li-member">
        {error ? <p className="li-member-err">{error}</p> : null}

        {profile === undefined ? (
          <div className="li-member-loading">Loading…</div>
        ) : profile === null ? (
          <div className="li-member-empty li-member-empty--enhanced">
            <div className="li-member-empty-avatar" aria-hidden>
              {initialsFromLabel(`U${targetId}`)}
            </div>
            <h2 className="li-page-title">{t('member.notFound')}</h2>
            <p className="li-member-empty-lead">{t('member.emptyProfileLead')}</p>
            <p className="li-member-empty-id">
              {t('member.userIdLabel')}: {targetId}
            </p>
            <div className="li-member-empty-actions">
              <Link className="li-btn primary" to={`/messages?with=${targetId}`}>
                {t('member.sendMessage')}
              </Link>
              <Link className="li-btn li-btn--outline" to="/connections">
                {t('member.backToNetwork')}
              </Link>
            </div>
          </div>
        ) : (
          <>
            <header className="li-member-hero">
              <div className="li-member-avatar" aria-hidden>
                {initials}
              </div>
              <div className="li-member-hero-text">
                <h1 className="li-member-name">{displayName}</h1>
                <p className="li-member-headline">
                  {profile.experienceYears > 0
                    ? t('member.years').replace('{{n}}', String(profile.experienceYears))
                    : t('member.years0')}
                </p>
              </div>
            </header>

            <div className="li-member-section">
              <h2 className="li-member-section-title">{t('member.about')}</h2>
              {profile.aboutMe?.trim() ? (
                <p className="li-member-about">{profile.aboutMe}</p>
              ) : (
                <p className="li-member-muted">{t('member.emptyAbout')}</p>
              )}
            </div>

            <div className="li-member-section">
              <h2 className="li-member-section-title">{t('member.experience')}</h2>
              <p className="li-member-exp">
                {profile.experienceYears > 0
                  ? t('member.years').replace('{{n}}', String(profile.experienceYears))
                  : t('member.years0')}
              </p>
            </div>

            <div className="li-member-footer">
              <Link className="li-btn li-btn--outline" to="/messages">
                {t('nav.messages')}
              </Link>
              <Link className="li-btn" to="/connections">
                {t('nav.connections')}
              </Link>
            </div>
          </>
        )}
      </section>

      <aside className="li-panel">
        <h4 className="li-side-title">{t('member.connectAsideTitle')}</h4>
        <p className="li-side-text">{t('member.connectAsideHint')}</p>
        <Link className="li-btn primary" to={`/messages?with=${targetId}`} style={{ marginTop: 12, display: 'inline-flex' }}>
          {t('member.sendMessage')}
        </Link>
      </aside>
    </div>
  )
}
