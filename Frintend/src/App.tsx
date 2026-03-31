import { Link, Navigate, Route, Routes } from 'react-router-dom'
import { AuthPage } from './pages/AuthPage'
import { hasRole, isAuthed } from './lib/auth'
import { useI18n } from './lib/i18n'
import { Shell } from './app/Shell'
import { JobsPage } from './pages/JobsPage'
import { FeedPage } from './pages/FeedPage'
import { ProfilePage } from './pages/ProfilePage'
import { MemberProfilePage } from './pages/MemberProfilePage'
import { ApplicationsPage } from './pages/ApplicationsPage'
import { ConnectionsPage } from './pages/ConnectionsPage'
import { MessagesPage } from './pages/MessagesPage'
import { NotificationsPage } from './pages/NotificationsPage'
import { AiPage } from './pages/AiPage'
import { SettingsPage } from './pages/SettingsPage'
import { DirectoryPage } from './pages/DirectoryPage'
import { OrganizationProfilePage } from './pages/OrganizationProfilePage'
import { RecruitingPage } from './pages/RecruitingPage'
import { CompanyPage } from './pages/CompanyPage'
import type { ReactNode } from 'react'

function RoleGate({ allow, children }: { allow: string[]; children: ReactNode }) {
  const ok = allow.some((r) => hasRole(r))
  if (!ok) return <Navigate to="/" replace />
  return <>{children}</>
}

function HomePublic() {
  const { t } = useI18n()
  return (
    <div className="li-grid">
      <div />
      <section className="li-card li-home-hero" aria-labelledby="home-public-title">
        <h1 id="home-public-title" className="li-home-title">
          {t('home.public.title')}
        </h1>
        <p className="li-home-sub">{t('home.public.sub')}</p>
        <div className="li-home-cta">
          <Link className="li-btn li-btn--hero primary" to="/auth">
            {t('home.public.signin')}
          </Link>
          <Link className="li-btn li-btn--hero li-btn--outline" to="/auth">
            {t('home.public.join')}
          </Link>
        </div>
      </section>
      <div />
    </div>
  )
}

export default function App() {
  const authed = isAuthed()
  return (
    <Routes>
      <Route path="/auth" element={<AuthPage />} />
      <Route element={<Shell />}>
        <Route path="/" element={authed ? <FeedPage /> : <HomePublic />} />
        <Route path="/jobs" element={authed ? <JobsPage /> : <Navigate to="/auth" replace />} />
        <Route path="/profile" element={authed ? <ProfilePage /> : <Navigate to="/auth" replace />} />
        <Route path="/people/:userId" element={authed ? <MemberProfilePage /> : <Navigate to="/auth" replace />} />
        <Route path="/directory" element={authed ? <DirectoryPage /> : <Navigate to="/auth" replace />} />
        <Route
          path="/organizations/:orgId"
          element={authed ? <OrganizationProfilePage /> : <Navigate to="/auth" replace />}
        />
        <Route
          path="/applications"
          element={
            authed ? (
              <RoleGate allow={['Candidate']}>
                <ApplicationsPage />
              </RoleGate>
            ) : (
              <Navigate to="/auth" replace />
            )
          }
        />
        <Route
          path="/recruiting"
          element={
            authed ? (
              <RoleGate allow={['Organization']}>
                <RecruitingPage />
              </RoleGate>
            ) : (
              <Navigate to="/auth" replace />
            )
          }
        />
        <Route
          path="/company"
          element={
            authed ? (
              <RoleGate allow={['Organization']}>
                <CompanyPage />
              </RoleGate>
            ) : (
              <Navigate to="/auth" replace />
            )
          }
        />
        <Route path="/connections" element={authed ? <ConnectionsPage /> : <Navigate to="/auth" replace />} />
        <Route path="/messages" element={authed ? <MessagesPage /> : <Navigate to="/auth" replace />} />
        <Route path="/notifications" element={authed ? <NotificationsPage /> : <Navigate to="/auth" replace />} />
        <Route path="/ai" element={authed ? <AiPage /> : <Navigate to="/auth" replace />} />
        <Route path="/settings" element={authed ? <SettingsPage /> : <Navigate to="/auth" replace />} />
      </Route>
      <Route path="*" element={<Navigate to={authed ? '/' : '/auth'} replace />} />
    </Routes>
  )
}
