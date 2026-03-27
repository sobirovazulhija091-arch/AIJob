import { useRef, useState } from 'react'
import { analyzeCv, askAi, type CvAnalysis } from '../lib/api'
import { useI18n } from '../lib/i18n'
import './ai.css'

const MIN_CV_LEN = 80

export function AiPage() {
  const { t } = useI18n()
  const fileRef = useRef<HTMLInputElement>(null)
  const [tab, setTab] = useState<'ask' | 'cv'>('ask')
  const [prompt, setPrompt] = useState('')
  const [cvText, setCvText] = useState('')
  const [cvFileName, setCvFileName] = useState('')
  const [applyToProfile, setApplyToProfile] = useState(false)
  const [syncSkills, setSyncSkills] = useState(false)
  const [answer, setAnswer] = useState('')
  const [cvResult, setCvResult] = useState<CvAnalysis | null>(null)
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  function displayName(r: CvAnalysis): string {
    const raw = r.fullName?.trim()
    if (raw) return raw
    const a = [r.firstName?.trim(), r.lastName?.trim()].filter(Boolean)
    return a.length ? a.join(' ') : '—'
  }

  function onCvFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    const f = e.target.files?.[0]
    e.target.value = ''
    if (!f) return
    const byName = /\.(txt|md)$/i.test(f.name)
    const byType = f.type === 'text/plain' || f.type === 'text/markdown' || f.type === ''
    if (!byName && !byType) {
      setError(t('ai.cv.fileType'))
      return
    }
    const reader = new FileReader()
    reader.onload = () => {
      const text = typeof reader.result === 'string' ? reader.result : ''
      setCvText(text)
      setCvFileName(f.name)
      setCvResult(null)
      setError('')
    }
    reader.onerror = () => setError(t('ai.cv.readError'))
    reader.readAsText(f)
  }

  async function run() {
    setError('')
    setAnswer('')
    setBusy(true)
    try {
      setAnswer(await askAi(prompt))
    } catch (e) {
      setError(e instanceof Error ? e.message : t('ai.error.ask'))
    } finally {
      setBusy(false)
    }
  }

  async function runCv() {
    setError('')
    setCvResult(null)
    const trimmed = cvText.trim()
    if (trimmed.length < MIN_CV_LEN) {
      setError(t('ai.cv.minHint').replace('{n}', String(MIN_CV_LEN)))
      return
    }
    setBusy(true)
    try {
      setCvResult(
        await analyzeCv(trimmed, {
          applyToProfile,
          syncSkills,
        }),
      )
    } catch (e) {
      setError(e instanceof Error ? e.message : t('ai.error.cv'))
    } finally {
      setBusy(false)
    }
  }

  const cvLen = cvText.length

  const replyBodyClass =
    'li-ai-reply-body' + (busy ? ' is-thinking' : !answer ? ' is-empty' : '')

  return (
    <div className="li-grid">
      <aside className="li-panel">
        <h4 className="li-side-title">{t('ai.side.tools')}</h4>
        <p className="li-side-text">{t('ai.side.tools.body')}</p>
      </aside>
      <section className="li-card li-ai-shell">
        <header className="li-ai-hero">
          <div className="li-ai-hero-inner">
            <div className="li-ai-hero-text">
              <p className="li-ai-kicker">{t('ai.kicker')}</p>
              <h2 className="li-page-title">{t('ai.title')}</h2>
              <p className="li-page-sub">{t('ai.sub')}</p>
            </div>
            <div className="li-ai-segment-wrap">
              <div className="li-ai-segment" role="tablist" aria-label={t('ai.title')}>
                <button
                  type="button"
                  role="tab"
                  aria-selected={tab === 'ask'}
                  onClick={() => {
                    setTab('ask')
                    setError('')
                  }}
                >
                  {t('ai.tab.ask')}
                </button>
                <button
                  type="button"
                  role="tab"
                  aria-selected={tab === 'cv'}
                  onClick={() => {
                    setTab('cv')
                    setError('')
                  }}
                >
                  {t('ai.tab.cv')}
                </button>
              </div>
            </div>
          </div>
        </header>

        <div className="li-ai-body">
          {tab === 'ask' ? (
            <div className="li-ai-workspace li-ai-workspace--ask">
              <div className="li-ai-editor-col">
                <p className="li-ai-hint">{t('ai.ask.hint')}</p>
                <textarea
                  className="li-ai-textarea"
                  value={prompt}
                  onChange={(e) => setPrompt(e.target.value)}
                  placeholder={t('ai.ask.placeholder')}
                />
                <div className="li-ai-footer-row">
                  <button
                    type="button"
                    className="li-ai-submit"
                    onClick={() => void run()}
                    disabled={busy || !prompt.trim()}
                  >
                    {busy ? t('ai.ask.thinking') : t('ai.ask.run')}
                  </button>
                </div>
              </div>
              <div className="li-ai-reply-col">
                <div className="li-ai-reply-head">{t('ai.reply.title')}</div>
                <div className={replyBodyClass}>
                  {busy ? t('ai.reply.thinking') : answer || t('ai.reply.empty')}
                </div>
              </div>
            </div>
          ) : (
            <div className="li-ai-cv-stack">
              <div className="li-ai-cv-head">
                <p className="li-ai-hint">{t('ai.cv.hint')}</p>
                <span className="li-ai-meta">{t('ai.cv.chars').replace('{n}', String(cvLen))}</span>
              </div>
              <div className="li-ai-upload-row">
                <input
                  ref={fileRef}
                  type="file"
                  className="li-ai-file"
                  accept=".txt,.md,text/plain"
                  onChange={onCvFileChange}
                />
                <button type="button" className="li-ai-file-label" onClick={() => fileRef.current?.click()}>
                  {t('ai.cv.upload')}
                </button>
                {cvFileName ? <span className="li-ai-file-name">{cvFileName}</span> : null}
              </div>
              <textarea
                className="li-ai-textarea li-ai-textarea--cv"
                value={cvText}
                onChange={(e) => {
                  setCvText(e.target.value)
                  setCvResult(null)
                }}
                placeholder={t('ai.cv.placeholder')}
              />
              <div className="li-ai-options">
                <label className="li-ai-check">
                  <input type="checkbox" checked={applyToProfile} onChange={(e) => setApplyToProfile(e.target.checked)} />
                  <span>
                    {t('ai.cv.applyProfile')}
                    <small>{t('ai.cv.applyProfile.hint')}</small>
                  </span>
                </label>
                <label className="li-ai-check">
                  <input type="checkbox" checked={syncSkills} onChange={(e) => setSyncSkills(e.target.checked)} />
                  <span>
                    {t('ai.cv.syncSkills')}
                    <small>{t('ai.cv.syncSkills.hint')}</small>
                  </span>
                </label>
              </div>
              <div className="li-ai-cv-actions">
                <button type="button" className="li-ai-submit" onClick={() => void runCv()} disabled={busy}>
                  {busy ? t('ai.cv.analyzing') : t('ai.cv.analyze')}
                </button>
              </div>
            </div>
          )}

          {error ? <p className="li-ai-error">{error}</p> : null}

          {tab === 'cv' && cvResult ? (
            <div className="li-ai-results">
              <div className="li-ai-result-card">
                <div className="li-ai-name-row">
                  <h3 className="li-ai-name">{displayName(cvResult)}</h3>
                  {cvResult.experienceYears > 0 ? (
                    <span className="li-ai-exp-pill">{t('ai.exp.years').replace('{n}', String(cvResult.experienceYears))}</span>
                  ) : (
                    <span className="li-ai-exp-pill">{t('ai.exp.unknown')}</span>
                  )}
                </div>
              </div>

              <div className="li-ai-result-card">
                <h4 className="li-ai-result-title">{t('ai.result.summary')}</h4>
                {cvResult.professionalSummary?.trim() ? (
                  <p className="li-ai-prose">{cvResult.professionalSummary}</p>
                ) : (
                  <p className="li-ai-muted">{t('ai.result.emptySection')}</p>
                )}
              </div>

              <div className="li-ai-result-card">
                <h4 className="li-ai-result-title">{t('ai.result.skills')}</h4>
                {cvResult.skills?.length ? (
                  <div className="li-ai-chip-list">
                    {cvResult.skills.map((s, i) => (
                      <span key={`${i}-${s}`} className="li-ai-skill">
                        {s}
                      </span>
                    ))}
                  </div>
                ) : (
                  <p className="li-ai-muted">{t('ai.result.emptySection')}</p>
                )}
              </div>

              <div className="li-ai-result-card">
                <h4 className="li-ai-result-title">{t('ai.result.education')}</h4>
                {cvResult.education?.length ? (
                  <ul className="li-ai-list-plain">
                    {cvResult.education.map((x, i) => (
                      <li key={`${i}-${x}`}>{x}</li>
                    ))}
                  </ul>
                ) : (
                  <p className="li-ai-muted">{t('ai.result.emptySection')}</p>
                )}
              </div>

              <div className="li-ai-result-card">
                <h4 className="li-ai-result-title">{t('ai.result.roles')}</h4>
                {cvResult.recommendedRoles?.length ? (
                  <ul className="li-ai-list-plain">
                    {cvResult.recommendedRoles.map((x, i) => (
                      <li key={`${i}-${x}`}>{x}</li>
                    ))}
                  </ul>
                ) : (
                  <p className="li-ai-muted">{t('ai.result.emptySection')}</p>
                )}
              </div>

              <div className="li-ai-result-card">
                <h4 className="li-ai-result-title">{t('ai.result.notes')}</h4>
                {cvResult.notes?.length ? (
                  <ul className="li-ai-list-plain">
                    {cvResult.notes.map((x, i) => (
                      <li key={`${i}-${x}`}>{x}</li>
                    ))}
                  </ul>
                ) : (
                  <p className="li-ai-muted">{t('ai.result.emptySection')}</p>
                )}
              </div>

              {cvResult.sourceTextPreview?.trim() ? (
                <div className="li-ai-result-card">
                  <h4 className="li-ai-result-title">{t('ai.result.preview')}</h4>
                  <div className="li-ai-preview">{cvResult.sourceTextPreview}</div>
                </div>
              ) : null}
            </div>
          ) : null}
        </div>
      </section>
      <aside className="li-panel">
        <h4 className="li-side-title">{t('ai.side.tips')}</h4>
        <p className="li-side-text">{t('ai.side.tips.body')}</p>
      </aside>
    </div>
  )
}
