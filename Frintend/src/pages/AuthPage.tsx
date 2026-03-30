import { useLayoutEffect, useMemo, useState } from 'react'
import { forgotPassword, login, register, resetPasswordWithToken } from '../lib/api'
import { setSession } from '../lib/auth'
import { useI18n } from '../lib/i18n'
import './auth.css'

type Mode = 'signin' | 'signup'

const PWRESET_SESSION = 'aijob.pwreset.v1'

export function AuthPage() {
  const { t } = useI18n()
  const [mode, setMode] = useState<Mode>('signin')
  const isSignIn = mode === 'signin'

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [fullName, setFullName] = useState('')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [role, setRole] = useState<'Candidate' | 'Organization'>('Candidate')

  const [showForgot, setShowForgot] = useState(false)
  const [resetToken, setResetToken] = useState('')
  const [newPasswordReset, setNewPasswordReset] = useState('')
  const [forgotInfo, setForgotInfo] = useState('')
  const [tokenFromLink, setTokenFromLink] = useState(false)
  const [showTokenField, setShowTokenField] = useState(false)

  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  const title = useMemo(() => (isSignIn ? 'Join the Platform' : 'Join the Platform'), [isSignIn])
  const sub = useMemo(() => 'Driven by data and purpose.', [])

  function goSignIn() {
    try {
      sessionStorage.removeItem(PWRESET_SESSION)
    } catch {
      /* ignore */
    }
    setShowForgot(false)
    setForgotInfo('')
    setError('')
    setResetToken('')
    setNewPasswordReset('')
    setTokenFromLink(false)
    setShowTokenField(false)
    setMode('signin')
  }

  useLayoutEffect(() => {
    const qs = new URLSearchParams(window.location.search)
    if (qs.get('reset') === '1') {
      const tok = qs.get('token')
      const em = qs.get('email')
      if (tok && em) {
        try {
          sessionStorage.setItem(PWRESET_SESSION, JSON.stringify({ token: tok, email: em }))
        } catch {
          /* ignore */
        }
        window.history.replaceState({}, document.title, `${window.location.pathname}${window.location.hash}`)
      }
    }

    let raw: string | null = null
    try {
      raw = sessionStorage.getItem(PWRESET_SESSION)
    } catch {
      raw = null
    }
    if (!raw) return

    let o: { token: string; email: string }
    try {
      o = JSON.parse(raw) as { token: string; email: string }
    } catch {
      try {
        sessionStorage.removeItem(PWRESET_SESSION)
      } catch {
        /* ignore */
      }
      return
    }
    if (!o.token || !o.email) return

    setShowForgot(true)
    setResetToken(o.token)
    setEmail(o.email)
    setTokenFromLink(true)
    setShowTokenField(false)
    setForgotInfo(t('auth.resetFromLink'))
  }, [t])

  async function submit() {
    setError('')
    setBusy(true)
    try {
      if (isSignIn) {
        const res = await login(email.trim(), password)
        setSession(res.token, res.refreshToken)
      } else {
        const res = await register({
          fullName: fullName.trim(),
          email: email.trim(),
          phoneNumber: phoneNumber.trim(),
          password,
          role,
        })
        setSession(res.token, res.refreshToken)
      }
      window.location.href = '/'
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Something went wrong.')
    } finally {
      setBusy(false)
    }
  }

  async function sendForgotEmail() {
    setError('')
    setForgotInfo('')
    const em = email.trim()
    if (!em) {
      setError('Enter your email address.')
      return
    }
    setBusy(true)
    try {
      await forgotPassword(em)
      setForgotInfo('If that email is registered, we sent instructions (check your inbox or spam folder).')
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Could not send reset email.')
    } finally {
      setBusy(false)
    }
  }

  async function submitNewPassword() {
    setError('')
    setForgotInfo('')
    const em = email.trim()
    const tok = resetToken.trim()
    const np = newPasswordReset
    if (!em || !tok || !np) {
      setError('Fill in email, reset token from the email, and a new password.')
      return
    }
    if (np.length < 6) {
      setError('Use a password with at least 6 characters.')
      return
    }
    setBusy(true)
    try {
      await resetPasswordWithToken({ email: em, token: tok, newPassword: np })
      try {
        sessionStorage.removeItem(PWRESET_SESSION)
      } catch {
        /* ignore */
      }
      setForgotInfo('Password updated. You can sign in below.')
      setShowForgot(false)
      setResetToken('')
      setNewPasswordReset('')
      setTokenFromLink(false)
      setShowTokenField(false)
      setPassword('')
      setMode('signin')
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Reset failed.')
    } finally {
      setBusy(false)
    }
  }

  if (showForgot) {
    return (
      <div className="auth-bg">
        <div className="auth-card" role="main">
          <h1 className="auth-title">Reset password</h1>
          <p className="auth-sub">We’ll email a reset code to your account (if it exists).</p>

          <button className="auth-back" type="button" onClick={goSignIn}>
            ← Back to sign in
          </button>

          <div className="pane">
            <div className="field">
              <label>Email address</label>
              <input value={email} onChange={(e) => setEmail(e.target.value)} placeholder="name@company.com" />
            </div>

            {error ? <div className="error">{error}</div> : null}
            {forgotInfo ? <div className="auth-info">{forgotInfo}</div> : null}

            <button className="cta cta--outline" onClick={() => void sendForgotEmail()} disabled={busy} type="button">
              {busy ? 'Sending…' : 'Send reset email'}
              <span className="arrow" aria-hidden="true">
                →
              </span>
            </button>

            <p className="auth-forgot-divider">{t('auth.forgotDivider')}</p>

            {tokenFromLink && !showTokenField ? (
              <div className="auth-token-applied">
                <span className="auth-token-applied-ic" aria-hidden>
                  ✓
                </span>
                <div>
                  <p className="auth-token-applied-title">{t('auth.tokenFromEmailLink')}</p>
                  <button className="auth-token-edit" type="button" onClick={() => setShowTokenField(true)}>
                    {t('auth.editCodeManually')}
                  </button>
                </div>
              </div>
            ) : (
              <div className="field">
                <label>{t('auth.resetTokenLabel')}</label>
                <textarea
                  className="auth-token-area"
                  value={resetToken}
                  onChange={(e) => {
                    setResetToken(e.target.value)
                    setTokenFromLink(false)
                  }}
                  placeholder={t('auth.resetTokenPlaceholder')}
                  rows={tokenFromLink ? 2 : 3}
                  readOnly={tokenFromLink && !showTokenField}
                />
              </div>
            )}
            <div className="field">
              <label>{t('auth.newPasswordLabel')}</label>
              <input
                value={newPasswordReset}
                onChange={(e) => setNewPasswordReset(e.target.value)}
                placeholder="••••••••"
                type="password"
                autoComplete="new-password"
              />
            </div>
            <button className="cta cta--reset-submit" onClick={() => void submitNewPassword()} disabled={busy} type="button">
              {busy ? t('auth.resetSaving') : t('auth.setNewPassword')}
              <span className="arrow" aria-hidden="true">
                →
              </span>
            </button>
            <p className="auth-muted">{t('auth.resetEmailHint')}</p>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="auth-bg">
      <div className="auth-card" role="main">
        <h1 className="auth-title">{title}</h1>
        <p className="auth-sub">{sub}</p>

        <div className="seg" role="tablist" aria-label="Auth mode">
          <button
            className={`seg-btn ${isSignIn ? 'active' : ''}`}
            onClick={() => {
              setMode('signin')
              setShowForgot(false)
            }}
            type="button"
            role="tab"
            aria-selected={isSignIn}
          >
            Sign In
          </button>
          <button
            className={`seg-btn ${!isSignIn ? 'active' : ''}`}
            onClick={() => {
              setMode('signup')
              setShowForgot(false)
            }}
            type="button"
            role="tab"
            aria-selected={!isSignIn}
          >
            Create Account
          </button>
          <div className={`seg-pill ${isSignIn ? 'left' : 'right'}`} aria-hidden="true" />
        </div>

        <div className={`pane ${isSignIn ? 'pane-in' : 'pane-up'}`}>
          {!isSignIn && (
            <div className="row2">
              <div className="field">
                <label>Full name</label>
                <input value={fullName} onChange={(e) => setFullName(e.target.value)} placeholder="Your name" />
              </div>
              <div className="field">
                <label>Phone</label>
                <input value={phoneNumber} onChange={(e) => setPhoneNumber(e.target.value)} placeholder="+998..." />
              </div>
            </div>
          )}

          <div className="field">
            <label>Email address</label>
            <input value={email} onChange={(e) => setEmail(e.target.value)} placeholder="name@company.com" />
          </div>

          <div className="field">
            <label>Password</label>
            <input
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              type="password"
            />
          </div>

          {!isSignIn && (
            <div className="field">
              <label>Role</label>
              <div className="role-toggle" role="group" aria-label="Choose role">
                <button
                  type="button"
                  className={`role-btn ${role === 'Candidate' ? 'active' : ''}`}
                  onClick={() => setRole('Candidate')}
                >
                  Candidate
                </button>
                <button
                  type="button"
                  className={`role-btn ${role === 'Organization' ? 'active' : ''}`}
                  onClick={() => setRole('Organization')}
                >
                  Organization
                </button>
                <div className={`role-pill ${role === 'Candidate' ? 'left' : 'right'}`} aria-hidden="true" />
              </div>
            </div>
          )}

          {error && <div className="error">{error}</div>}

          <button className="cta" onClick={submit} disabled={busy} type="button">
            {busy ? (isSignIn ? 'Signing In…' : 'Creating…') : isSignIn ? 'Sign In' : 'Create account'}
            <span className="arrow" aria-hidden="true">
              →
            </span>
          </button>

          {isSignIn ? (
            <button
              className="link link--accent"
              type="button"
              onClick={() => {
                setError('')
                setForgotInfo('')
                setTokenFromLink(false)
                setShowTokenField(false)
                setShowForgot(true)
              }}
            >
              Forgot your password?
            </button>
          ) : null}
        </div>
      </div>
    </div>
  )
}
