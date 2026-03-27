import { useMemo, useState } from 'react'
import { login, register } from '../lib/api'
import { setSession } from '../lib/auth'
import './auth.css'

type Mode = 'signin' | 'signup'

export function AuthPage() {
  const [mode, setMode] = useState<Mode>('signin')
  const isSignIn = mode === 'signin'

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [fullName, setFullName] = useState('')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [role, setRole] = useState<'Candidate' | 'Organization'>('Candidate')

  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  const title = useMemo(() => (isSignIn ? 'Join the Platform' : 'Join the Platform'), [isSignIn])
  const sub = useMemo(() => 'Driven by data and purpose.', [])

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

  return (
    <div className="auth-bg">
      <div className="auth-card" role="main">
        <h1 className="auth-title">{title}</h1>
        <p className="auth-sub">{sub}</p>

        <div className="seg" role="tablist" aria-label="Auth mode">
          <button
            className={`seg-btn ${isSignIn ? 'active' : ''}`}
            onClick={() => setMode('signin')}
            type="button"
            role="tab"
            aria-selected={isSignIn}
          >
            Sign In
          </button>
          <button
            className={`seg-btn ${!isSignIn ? 'active' : ''}`}
            onClick={() => setMode('signup')}
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

          <button className="link" type="button">
            Forgot your password?
          </button>
        </div>
      </div>
    </div>
  )
}

