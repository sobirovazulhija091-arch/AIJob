import { NavLink, Outlet } from 'react-router-dom'
import { clearSession, getDisplayName, getEmail, getRoles, initialsFromLabel } from '../lib/auth'
import { useI18n } from '../lib/i18n'
import './shell.css'

function primaryRoleLabel(roles: string[], t: (key: string) => string): string | null {
  if (roles.includes('Admin')) return t('role.admin')
  if (roles.includes('Organization')) return t('role.organization')
  if (roles.includes('Candidate')) return t('role.candidate')
  return null
}

export function Shell() {
  const { t } = useI18n()
  const roles = getRoles()
  const isAdmin = roles.includes('Admin')
  const showApplications = roles.includes('Candidate') || isAdmin
  const showRecruiting = roles.includes('Organization') || isAdmin

  const display = getDisplayName() ?? getEmail()?.split('@')[0] ?? t('settings.title.fallback')
  const roleLabel = primaryRoleLabel(roles, t)

  return (
    <>
      <header className="li-topbar">
        <div className="li-topbar-inner">
          <div className="li-topbar-row li-topbar-main">
            <a className="li-brand" href="/">
              <span className="li-brand-badge">CH</span>
              {t('brand.title')}
            </a>

            <div className="li-user-pill" title={getEmail() ?? display}>
              <span className="li-user-avatar">{initialsFromLabel(display)}</span>
              <span className="li-user-meta">
                <span className="li-user-name">{display}</span>
                {roleLabel ? <span className="li-user-role">{roleLabel}</span> : null}
              </span>
            </div>

            <div className="li-search">
              <svg className="li-search-icon" width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden>
                <circle cx="11" cy="11" r="7" stroke="currentColor" strokeWidth="2" />
                <path d="M20 20 16 16" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
              </svg>
              <input type="search" autoComplete="off" placeholder={t('search.placeholder')} />
            </div>
          </div>

          <div className="li-topbar-row li-topbar-nav">
            <nav className="li-nav" aria-label="Main">
              <div className="li-nav-links">
                <NavLink to="/">{t('nav.home')}</NavLink>
                <NavLink to="/jobs">{t('nav.jobs')}</NavLink>
                <NavLink to="/profile">{t('nav.profile')}</NavLink>
                {showApplications ? <NavLink to="/applications">{t('nav.applications')}</NavLink> : null}
                {showRecruiting ? <NavLink to="/recruiting">{t('nav.recruiting')}</NavLink> : null}
                <NavLink to="/connections">{t('nav.connections')}</NavLink>
                <NavLink to="/messages">{t('nav.messages')}</NavLink>
                <NavLink to="/notifications">{t('nav.notifications')}</NavLink>
                <NavLink to="/ai">{t('nav.ai')}</NavLink>
                {isAdmin ? <NavLink to="/admin">{t('nav.admin')}</NavLink> : null}
                <NavLink to="/settings">{t('nav.settings')}</NavLink>
              </div>
              <button
                type="button"
                className="li-nav-logout"
                onClick={() => {
                  clearSession()
                  window.location.href = '/auth'
                }}
              >
                {t('nav.logout')}
              </button>
            </nav>
          </div>
        </div>
      </header>

      <main className="li-wrap">
        <Outlet />
      </main>
    </>
  )
}
