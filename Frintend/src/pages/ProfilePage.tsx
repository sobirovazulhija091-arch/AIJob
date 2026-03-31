import { useEffect, useState } from 'react'
import { createProfile, getProfileByUser, updateProfile, type UserProfile } from '../lib/api'
import { getUserId, hasRole, setHeaderDisplayNameFromProfile } from '../lib/auth'
import { useI18n } from '../lib/i18n'

function sanitizeExperienceInput(raw: string): string {
  return raw.replace(/\D/g, '').slice(0, 3)
}

function sanitizeSalaryInput(raw: string): string {
  const t = raw.replace(/[^\d.]/g, '')
  const dot = t.indexOf('.')
  if (dot === -1) return t
  return t.slice(0, dot + 1) + t.slice(dot + 1).replace(/\./g, '')
}

function parseExperienceYears(s: string): number {
  const n = parseInt(s.trim() || '0', 10)
  if (!Number.isFinite(n) || n < 0) return 0
  return Math.min(n, 100)
}

function parseExpectedSalary(s: string): number {
  const cleaned = s.replace(/,/g, '').trim()
  if (cleaned === '' || cleaned === '.') return 0
  const n = parseFloat(cleaned)
  if (!Number.isFinite(n) || n < 0) return 0
  return n
}

export function ProfilePage() {
  const { t } = useI18n()
  const userId = getUserId()
  const [model, setModel] = useState<UserProfile | null>(null)
  const [profileId, setProfileId] = useState<number>(0)
  const [experienceStr, setExperienceStr] = useState('')
  const [salaryStr, setSalaryStr] = useState('')
  const [msg, setMsg] = useState('')
  const [error, setError] = useState('')

  useEffect(() => {
    ;(async () => {
      if (!userId) return
      setError('')
      try {
        const p = await getProfileByUser(userId)
        if (p) {
          setHeaderDisplayNameFromProfile(p.firstName, p.lastName)
          setModel(p)
          setProfileId(p.id)
          setExperienceStr(p.experienceYears > 0 ? String(p.experienceYears) : '')
          setSalaryStr(p.expectedSalary > 0 ? String(p.expectedSalary) : '')
        } else {
          setModel({
            id: 0,
            userId,
            firstName: '',
            lastName: '',
            aboutMe: '',
            experienceYears: 0,
            expectedSalary: 0,
          })
          setExperienceStr('')
          setSalaryStr('')
        }
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Failed to load profile.')
      }
    })()
  }, [userId])

  async function save() {
    if (!model || !userId) return
    setMsg('')
    setError('')
    const experienceYears = parseExperienceYears(experienceStr)
    const expectedSalary = parseExpectedSalary(salaryStr)
    const payload: UserProfile = { ...model, experienceYears, expectedSalary }
    try {
      if (profileId) {
        await updateProfile(profileId, payload)
      } else {
        const { id: _id, ...body } = payload
        await createProfile(body)
      }
      const fresh = await getProfileByUser(userId)
      if (fresh) {
        setHeaderDisplayNameFromProfile(fresh.firstName, fresh.lastName)
        setProfileId(fresh.id)
        setModel(fresh)
        setExperienceStr(fresh.experienceYears > 0 ? String(fresh.experienceYears) : '')
        setSalaryStr(fresh.expectedSalary > 0 ? String(fresh.expectedSalary) : '')
      } else {
        setHeaderDisplayNameFromProfile(payload.firstName, payload.lastName)
        setModel(payload)
        setExperienceStr(experienceYears > 0 ? String(experienceYears) : '')
        setSalaryStr(expectedSalary > 0 ? String(expectedSalary) : '')
      }
      setMsg('Saved.')
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Save failed.')
    }
  }

  return (
    <div className="li-grid">
      <aside className="li-panel">
        <h4 className="li-side-title">Profile</h4>
        <p className="li-side-text">Keep this updated so recruiters can find you.</p>
      </aside>
      <section className="li-card li-card-pad li-stack">
        <h2 className="li-page-title">Profile</h2>
        <p className="li-page-sub">Your professional identity and preferences.</p>
        {error && <p style={{ color: 'crimson' }}>{error}</p>}
        {msg && <p style={{ color: 'green' }}>{msg}</p>}
        {model && (
          <>
            <div className="li-form-section" role="group" aria-labelledby="profile-your-name">
              <h3 id="profile-your-name" className="li-form-section-title">
                Your name
              </h3>
              <p className="li-field-hint">This is how you appear to recruiters and people in your network. Use the name you use professionally.</p>
              <div className="li-grid-2">
                <label className="li-stack">
                  <span className="li-label">First name</span>
                  <input
                    className="li-input"
                    placeholder="e.g. Sano"
                    autoComplete="given-name"
                    value={model.firstName}
                    onChange={(e) => setModel({ ...model, firstName: e.target.value })}
                  />
                </label>
                <label className="li-stack">
                  <span className="li-label">Last name</span>
                  <input
                    className="li-input"
                    placeholder="e.g. Karimov"
                    autoComplete="family-name"
                    value={model.lastName}
                    onChange={(e) => setModel({ ...model, lastName: e.target.value })}
                  />
                </label>
              </div>
            </div>

            <div className="li-form-section">
              <h3 id="profile-about-you" className="li-form-section-title">
                About you
              </h3>
              <p className="li-field-hint">
                Summarize your background in a few sentences: current role or focus, key skills, and what roles or opportunities you are open to.
              </p>
              <textarea
                id="profile-about-field"
                className="li-textarea"
                placeholder="Example: Software developer with 5 years in backend APIs. Interested in remote product teams and mentorship-heavy cultures."
                value={model.aboutMe}
                onChange={(e) => setModel({ ...model, aboutMe: e.target.value })}
                rows={5}
                aria-labelledby="profile-about-you"
              />
            </div>

            <div className="li-form-section" role="group" aria-labelledby="profile-expectations">
              <h3 id="profile-expectations" className="li-form-section-title">
                Experience & expectations
              </h3>
              {hasRole('Candidate') ? (
                <p className="li-field-hint">{t('profile.candidateExpLead')}</p>
              ) : null}
              <p className="li-field-hint">Rough numbers are fine; you can update these anytime.</p>
              <div className="li-grid-2">
                <label className="li-stack">
                  <span className="li-label">Years of experience</span>
                  <p className="li-field-hint">Total years in your field (full-time and relevant work).</p>
                  <input
                    className="li-input"
                    placeholder="e.g. 5"
                    type="text"
                    inputMode="numeric"
                    autoComplete="off"
                    value={experienceStr}
                    onChange={(e) => setExperienceStr(sanitizeExperienceInput(e.target.value))}
                  />
                </label>
                <label className="li-stack">
                  <span className="li-label">Expected salary</span>
                  <p className="li-field-hint">Annual amount you’re aiming for; type the number yourself (no currency stored here).</p>
                  <input
                    className="li-input"
                    placeholder="e.g. 85000 or 1200000"
                    type="text"
                    inputMode="decimal"
                    autoComplete="off"
                    value={salaryStr}
                    onChange={(e) => setSalaryStr(sanitizeSalaryInput(e.target.value))}
                  />
                </label>
              </div>
            </div>
            <button className="li-btn primary" onClick={save} type="button">
              Save profile
            </button>
          </>
        )}
      </section>
      <aside className="li-panel">
        <h4 className="li-side-title">Tips</h4>
        <p className="li-side-text">
          Use your real professional name. In About you, lead with your headline role, add 2–3 proof points (projects, stack, impact), and mention what you want next.
        </p>
      </aside>
    </div>
  )
}

