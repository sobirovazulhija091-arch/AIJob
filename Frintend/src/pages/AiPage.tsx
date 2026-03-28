import { useEffect, useId, useMemo, useRef, useState, type ReactNode } from 'react'
import {
  analyzeCv,
  askAi,
  draftCoverLetter,
  getJobs,
  getSkillGap,
  improveJob,
  type CvAnalysis,
  type DraftLetterResult,
  type Job,
  type JobImproveResult,
  type SkillGapResult,
} from '../lib/api'
import { getUserId, hasRole } from '../lib/auth'
import { NiceSelect } from '../components/NiceSelect'
import { useI18n } from '../lib/i18n'
import './ai.css'

const MIN_CV_LEN = 80

type AiTab = 'ask' | 'cv' | 'skill' | 'cover' | 'improve'

function IconChat() {
  return (
    <svg className="li-ai-tab-ic" width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M8 10h8M8 14h5"
        stroke="currentColor"
        strokeWidth="1.75"
        strokeLinecap="round"
      />
      <path
        d="M4 6a2 2 0 012-2h12a2 2 0 012 2v9a2 2 0 01-2 2h-6.35L8 21v-4H6a2 2 0 01-2-2V6z"
        stroke="currentColor"
        strokeWidth="1.65"
        strokeLinejoin="round"
      />
    </svg>
  )
}

function IconDoc() {
  return (
    <svg className="li-ai-tab-ic" width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8l-6-6z"
        stroke="currentColor"
        strokeWidth="1.65"
        strokeLinejoin="round"
      />
      <path d="M14 2v6h6M8 13h8M8 17h6" stroke="currentColor" strokeWidth="1.65" strokeLinecap="round" />
    </svg>
  )
}

function IconBolt() {
  return (
    <svg className="li-ai-tab-ic" width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M13 2L3 14h8l-1 8 10-12h-8l1-8z"
        stroke="currentColor"
        strokeWidth="1.65"
        strokeLinejoin="round"
      />
    </svg>
  )
}

function IconMail() {
  return (
    <svg className="li-ai-tab-ic" width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M4 6h16v12H4V6z"
        stroke="currentColor"
        strokeWidth="1.65"
        strokeLinejoin="round"
      />
      <path d="M4 7l8 6 8-6" stroke="currentColor" strokeWidth="1.65" strokeLinecap="round" />
    </svg>
  )
}

function IconSpark() {
  return (
    <svg className="li-ai-tab-ic" width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M12 3l1.2 4.2L17 8l-3.8 1.8L12 14l-1.2-4.2L7 8l3.8-1.8L12 3z" fill="currentColor" opacity="0.9" />
      <path d="M19 15l.6 1.9L21 17l-1.4.7L19 20l-.6-2.3L17 17l2-.9.9-1.1.1 0z" fill="currentColor" opacity="0.65" />
    </svg>
  )
}

function IconRobot() {
  return (
    <svg className="li-ai-empty-icon" width="48" height="48" viewBox="0 0 24 24" fill="none" aria-hidden>
      <rect x="5" y="8" width="14" height="12" rx="2" stroke="currentColor" strokeWidth="1.5" />
      <path d="M9 8V6a3 3 0 016 0v2" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
      <circle cx="9.5" cy="13" r="1.25" fill="currentColor" />
      <circle cx="14.5" cy="13" r="1.25" fill="currentColor" />
      <path d="M10 16h4" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
    </svg>
  )
}

function IconPlane() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M22 2L11 13M22 2l-7 20-4-9-9-4L22 2z"
        stroke="currentColor"
        strokeWidth="1.75"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  )
}

function IconReset() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M4 12a8 8 0 1116 0 8 8 0 01-16 0z"
        stroke="currentColor"
        strokeWidth="1.65"
        strokeLinejoin="round"
      />
      <path d="M8 12h6M12 9l3 3-3 3" stroke="currentColor" strokeWidth="1.65" strokeLinecap="round" />
    </svg>
  )
}

export function AiPage() {
  const { t } = useI18n()
  const skillJobLbl = useId()
  const coverJobLbl = useId()
  const improveJobLbl = useId()
  const fileRef = useRef<HTMLInputElement>(null)

  const canImprove = hasRole('Organization') || hasRole('Admin')

  const [tab, setTab] = useState<AiTab>('ask')
  const [jobs, setJobs] = useState<Job[]>([])

  const [prompt, setPrompt] = useState('')
  const [answer, setAnswer] = useState('')

  const [cvText, setCvText] = useState('')
  const [cvFileName, setCvFileName] = useState('')
  const [applyToProfile, setApplyToProfile] = useState(false)
  const [syncSkills, setSyncSkills] = useState(false)
  const [cvResult, setCvResult] = useState<CvAnalysis | null>(null)

  const [skillJobId, setSkillJobId] = useState('')
  const [skillResult, setSkillResult] = useState<SkillGapResult | null>(null)

  const [coverJobId, setCoverJobId] = useState('')
  const [tone, setTone] = useState('')
  const [extraContext, setExtraContext] = useState('')
  const [coverResult, setCoverResult] = useState<DraftLetterResult | null>(null)

  const [improveJobId, setImproveJobId] = useState('')
  const [applyImprove, setApplyImprove] = useState(false)
  const [improveResult, setImproveResult] = useState<JobImproveResult | null>(null)

  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    void getJobs()
      .then(setJobs)
      .catch(() => {})
  }, [])

  const jobOptions = useMemo(
    () => [
      { value: '', label: t('recruiting.selectPlaceholder') },
      ...jobs.map((j) => ({ value: String(j.id), label: j.title })),
    ],
    [jobs, t],
  )

  const navItems = useMemo(
    () =>
      [
        { id: 'ask' as const, label: t('ai.nav.ask'), icon: <IconChat /> },
        { id: 'cv' as const, label: t('ai.nav.cv'), icon: <IconDoc /> },
        { id: 'skill' as const, label: t('ai.nav.skill'), icon: <IconBolt /> },
        { id: 'cover' as const, label: t('ai.nav.cover'), icon: <IconMail /> },
        canImprove ? { id: 'improve' as const, label: t('ai.nav.improve'), icon: <IconSpark /> } : null,
      ].filter(Boolean) as { id: AiTab; label: string; icon: ReactNode }[],
    [t, canImprove],
  )

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

  function resetAsk() {
    setPrompt('')
    setAnswer('')
    setError('')
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

  async function runSkill() {
    setError('')
    setSkillResult(null)
    const jid = parseInt(skillJobId || '0', 10)
    const uid = getUserId()
    if (!jid) {
      setError(t('ai.error.job'))
      return
    }
    if (uid == null) {
      setError(t('ai.error.cv'))
      return
    }
    setBusy(true)
    try {
      setSkillResult(await getSkillGap(uid, jid))
    } catch (e) {
      setError(e instanceof Error ? e.message : t('ai.error.ask'))
    } finally {
      setBusy(false)
    }
  }

  async function runCover() {
    setError('')
    setCoverResult(null)
    const jid = parseInt(coverJobId || '0', 10)
    if (!jid) {
      setError(t('ai.error.job'))
      return
    }
    setBusy(true)
    try {
      setCoverResult(
        await draftCoverLetter({
          jobId: jid,
          tone: tone.trim() || undefined,
          extraContext: extraContext.trim() || undefined,
        }),
      )
    } catch (e) {
      setError(e instanceof Error ? e.message : t('ai.error.ask'))
    } finally {
      setBusy(false)
    }
  }

  async function runImprove() {
    setError('')
    setImproveResult(null)
    const jid = parseInt(improveJobId || '0', 10)
    if (!jid) {
      setError(t('ai.error.job'))
      return
    }
    setBusy(true)
    try {
      setImproveResult(await improveJob({ jobId: jid, applyToJob: applyImprove }))
    } catch (e) {
      setError(e instanceof Error ? e.message : t('ai.error.ask'))
    } finally {
      setBusy(false)
    }
  }

  function onAskKeyDown(e: React.KeyboardEvent<HTMLTextAreaElement>) {
    if (e.key !== 'Enter' || !(e.ctrlKey || e.metaKey)) return
    e.preventDefault()
    if (!busy && prompt.trim()) void run()
  }

  const cvLen = cvText.length

  function Thinking() {
    return (
      <span className="li-ai-thinking" aria-live="polite">
        <span className="li-ai-dots" aria-hidden>
          <span />
          <span />
          <span />
        </span>
        {t('ai.reply.thinking')}
      </span>
    )
  }

  function EmptyOut({ message }: { message: string }) {
    return (
      <div className="li-ai-empty-out">
        <IconRobot />
        <p className="li-ai-empty-title">{t('ai.panel.response')}</p>
        <p className="li-ai-empty-msg">{message}</p>
      </div>
    )
  }

  function tipsKeys(): string[] {
    switch (tab) {
      case 'ask':
        return ['ai.tips.ask.1', 'ai.tips.ask.2', 'ai.tips.ask.3']
      case 'cv':
        return ['ai.tips.cv.1', 'ai.tips.cv.2']
      case 'skill':
        return ['ai.tips.skill.1']
      case 'cover':
        return ['ai.tips.cover.1']
      case 'improve':
        return ['ai.tips.improve.1']
      default:
        return []
    }
  }

  function renderCvResults(r: CvAnalysis) {
    return (
      <div className="li-ai-results li-ai-results--in-panel">
        <div className="li-ai-result-card">
          <div className="li-ai-name-row">
            <h3 className="li-ai-name">{displayName(r)}</h3>
            {r.experienceYears > 0 ? (
              <span className="li-ai-exp-pill">{t('ai.exp.years').replace('{n}', String(r.experienceYears))}</span>
            ) : (
              <span className="li-ai-exp-pill">{t('ai.exp.unknown')}</span>
            )}
          </div>
        </div>
        <div className="li-ai-result-card">
          <h4 className="li-ai-result-title">{t('ai.result.summary')}</h4>
          {r.professionalSummary?.trim() ? (
            <p className="li-ai-prose">{r.professionalSummary}</p>
          ) : (
            <p className="li-ai-muted">{t('ai.result.emptySection')}</p>
          )}
        </div>
        <div className="li-ai-result-card">
          <h4 className="li-ai-result-title">{t('ai.result.skills')}</h4>
          {r.skills?.length ? (
            <div className="li-ai-chip-list">
              {r.skills.map((s, i) => (
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
          {r.education?.length ? (
            <ul className="li-ai-list-plain">
              {r.education.map((x, i) => (
                <li key={`${i}-${x}`}>{x}</li>
              ))}
            </ul>
          ) : (
            <p className="li-ai-muted">{t('ai.result.emptySection')}</p>
          )}
        </div>
        <div className="li-ai-result-card">
          <h4 className="li-ai-result-title">{t('ai.result.roles')}</h4>
          {r.recommendedRoles?.length ? (
            <ul className="li-ai-list-plain">
              {r.recommendedRoles.map((x, i) => (
                <li key={`${i}-${x}`}>{x}</li>
              ))}
            </ul>
          ) : (
            <p className="li-ai-muted">{t('ai.result.emptySection')}</p>
          )}
        </div>
        <div className="li-ai-result-card">
          <h4 className="li-ai-result-title">{t('ai.result.notes')}</h4>
          {r.notes?.length ? (
            <ul className="li-ai-list-plain">
              {r.notes.map((x, i) => (
                <li key={`${i}-${x}`}>{x}</li>
              ))}
            </ul>
          ) : (
            <p className="li-ai-muted">{t('ai.result.emptySection')}</p>
          )}
        </div>
        {r.sourceTextPreview?.trim() ? (
          <div className="li-ai-result-card">
            <h4 className="li-ai-result-title">{t('ai.result.preview')}</h4>
            <div className="li-ai-preview">{r.sourceTextPreview}</div>
          </div>
        ) : null}
      </div>
    )
  }

  function panelInputTitle(): string {
    switch (tab) {
      case 'ask':
        return t('ai.panel.question')
      case 'cv':
        return t('ai.panel.resume')
      case 'skill':
        return t('ai.skill.job')
      case 'cover':
        return t('ai.cover.job')
      case 'improve':
        return t('ai.improve.job')
      default:
        return ''
    }
  }

  function renderLeftColumn() {
    if (tab === 'ask') {
      return (
        <>
          <label className="li-ai-hint li-ai-hint--label" htmlFor="ai-ask-input">
            {t('ai.ask.hint')}
          </label>
          <textarea
            id="ai-ask-input"
            className="li-ai-textarea"
            value={prompt}
            onChange={(e) => setPrompt(e.target.value)}
            onKeyDown={onAskKeyDown}
            placeholder={t('ai.ask.placeholder')}
            rows={6}
          />
          <div className="li-ai-footer-block">
            <button type="button" className="li-ai-submit li-ai-submit--with-icon" onClick={() => void run()} disabled={busy || !prompt.trim()}>
              <IconPlane />
              {busy ? t('ai.ask.thinking') : t('ai.ask.run')}
            </button>
            <p className="li-ai-shortcut-hint">{t('ai.ask.shortcut')}</p>
          </div>
        </>
      )
    }

    if (tab === 'cv') {
      return (
        <>
          <div className="li-ai-cv-head">
            <p className="li-ai-hint" style={{ margin: 0 }}>
              {t('ai.cv.hint')}
            </p>
            <span className="li-ai-meta">{t('ai.cv.chars').replace('{n}', String(cvLen))}</span>
          </div>
          <div className="li-ai-upload-row">
            <input ref={fileRef} type="file" className="li-ai-file" accept=".txt,.md,text/plain" onChange={onCvFileChange} />
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
            <button type="button" className="li-ai-submit li-ai-submit--with-icon" onClick={() => void runCv()} disabled={busy}>
              {busy ? t('ai.cv.analyzing') : t('ai.cv.analyze')}
            </button>
          </div>
        </>
      )
    }

    if (tab === 'skill') {
      return (
        <>
          <p className="li-ai-hint li-ai-hint--label" id={skillJobLbl}>
            {t('ai.skill.job')}
          </p>
          <NiceSelect aria-labelledby={skillJobLbl} value={skillJobId} onChange={setSkillJobId} options={jobOptions} />
          <div className="li-ai-cv-actions" style={{ marginTop: 16 }}>
            <button type="button" className="li-ai-submit" onClick={() => void runSkill()} disabled={busy}>
              {busy ? t('ai.reply.thinking') : t('ai.skill.run')}
            </button>
          </div>
        </>
      )
    }

    if (tab === 'cover') {
      return (
        <>
          <p className="li-ai-hint li-ai-hint--label" id={coverJobLbl}>
            {t('ai.cover.job')}
          </p>
          <NiceSelect aria-labelledby={coverJobLbl} value={coverJobId} onChange={setCoverJobId} options={jobOptions} />
          <label className="li-stack" style={{ marginTop: 12 }}>
            <span className="li-label">{t('ai.cover.tone')}</span>
            <input className="li-input" value={tone} onChange={(e) => setTone(e.target.value)} placeholder={t('ai.cover.tonePh')} />
          </label>
          <label className="li-stack">
            <span className="li-label">{t('ai.cover.extra')}</span>
            <textarea
              className="li-ai-textarea"
              style={{ minHeight: 100 }}
              value={extraContext}
              onChange={(e) => setExtraContext(e.target.value)}
              placeholder={t('ai.cover.extraPh')}
            />
          </label>
          <div className="li-ai-cv-actions">
            <button type="button" className="li-ai-submit" onClick={() => void runCover()} disabled={busy}>
              {busy ? t('ai.reply.thinking') : t('ai.cover.run')}
            </button>
          </div>
        </>
      )
    }

    if (tab === 'improve') {
      if (!canImprove) {
        return <p className="li-ai-muted">{t('ai.improve.denied')}</p>
      }
      return (
        <>
          <p className="li-ai-hint li-ai-hint--label" id={improveJobLbl}>
            {t('ai.improve.job')}
          </p>
          <NiceSelect aria-labelledby={improveJobLbl} value={improveJobId} onChange={setImproveJobId} options={jobOptions} />
          <label className="li-ai-check" style={{ marginTop: 14 }}>
            <input type="checkbox" checked={applyImprove} onChange={(e) => setApplyImprove(e.target.checked)} />
            <span>{t('ai.improve.apply')}</span>
          </label>
          <div className="li-ai-cv-actions">
            <button type="button" className="li-ai-submit" onClick={() => void runImprove()} disabled={busy}>
              {busy ? t('ai.reply.thinking') : t('ai.improve.run')}
            </button>
          </div>
        </>
      )
    }

    return null
  }

  function renderRightColumn() {
    if (tab === 'ask') {
      return (
        <div className={'li-ai-reply-body' + (busy ? ' is-thinking' : !answer ? ' is-empty' : '')}>
          {busy ? <Thinking /> : answer ? answer : <span className="li-ai-empty-hint">{t('ai.empty.prompt')}</span>}
        </div>
      )
    }

    if (tab === 'cv') {
      if (busy) {
        return (
          <div className="li-ai-reply-body is-thinking">
            <Thinking />
          </div>
        )
      }
      if (cvResult) {
        return <div className="li-ai-reply-body li-ai-reply-body--scroll">{renderCvResults(cvResult)}</div>
      }
      return (
        <div className="li-ai-reply-body is-empty">
          <EmptyOut message={t('ai.reply.empty')} />
        </div>
      )
    }

    if (tab === 'skill') {
      if (busy) {
        return (
          <div className="li-ai-reply-body is-thinking">
            <Thinking />
          </div>
        )
      }
      if (skillResult) {
        return (
          <div className="li-ai-reply-body li-ai-reply-body--scroll">
            <div className="li-ai-skill-score">{skillResult.matchScore}%</div>
            <p className="li-ai-skill-summary">{skillResult.fitSummary}</p>
            <h4 className="li-ai-result-title">{t('ai.skill.strengths')}</h4>
            <ul className="li-ai-list-plain">
              {skillResult.strengths?.map((x, i) => (
                <li key={`${i}-${x}`}>{x}</li>
              ))}
            </ul>
            <h4 className="li-ai-result-title">{t('ai.skill.gaps')}</h4>
            <ul className="li-ai-list-plain">
              {skillResult.missingSkills?.map((x, i) => (
                <li key={`${i}-${x}`}>{x}</li>
              ))}
            </ul>
            <h4 className="li-ai-result-title">{t('ai.skill.next')}</h4>
            <ul className="li-ai-list-plain">
              {skillResult.nextSteps?.map((x, i) => (
                <li key={`${i}-${x}`}>{x}</li>
              ))}
            </ul>
          </div>
        )
      }
      return (
        <div className="li-ai-reply-body is-empty">
          <EmptyOut message={t('ai.skill.empty')} />
        </div>
      )
    }

    if (tab === 'cover') {
      if (busy) {
        return (
          <div className="li-ai-reply-body is-thinking">
            <Thinking />
          </div>
        )
      }
      if (coverResult) {
        return (
          <div className="li-ai-reply-body li-ai-reply-body--scroll">
            <h4 className="li-ai-result-title">{t('ai.cover.subject')}</h4>
            <p className="li-ai-prose li-ai-prose--strong">{coverResult.subject}</p>
            <h4 className="li-ai-result-title">{t('ai.panel.response')}</h4>
            <div className="li-ai-letter">{coverResult.content}</div>
          </div>
        )
      }
      return (
        <div className="li-ai-reply-body is-empty">
          <EmptyOut message={t('ai.cover.empty')} />
        </div>
      )
    }

    if (tab === 'improve') {
      if (!canImprove) {
        return (
          <div className="li-ai-reply-body is-empty">
            <EmptyOut message={t('ai.improve.denied')} />
          </div>
        )
      }
      if (busy) {
        return (
          <div className="li-ai-reply-body is-thinking">
            <Thinking />
          </div>
        )
      }
      if (improveResult) {
        return (
          <div className="li-ai-reply-body li-ai-reply-body--scroll">
            <h4 className="li-ai-result-title">{t('ai.improve.title')}</h4>
            <p className="li-ai-prose li-ai-prose--strong">{improveResult.improvedTitle}</p>
            <h4 className="li-ai-result-title">{t('ai.improve.desc')}</h4>
            <div className="li-ai-letter">{improveResult.improvedDescription}</div>
            <h4 className="li-ai-result-title">{t('ai.result.skills')}</h4>
            <div className="li-ai-chip-list">
              {improveResult.suggestedSkills?.map((s, i) => (
                <span key={`${i}-${s}`} className="li-ai-skill">
                  {s}
                </span>
              ))}
            </div>
            <h4 className="li-ai-result-title">{t('ai.improve.responsibilities')}</h4>
            <ul className="li-ai-list-plain">
              {improveResult.suggestedResponsibilities?.map((x, i) => (
                <li key={`r-${i}-${x}`}>{x}</li>
              ))}
            </ul>
            <h4 className="li-ai-result-title">{t('ai.improve.benefits')}</h4>
            <ul className="li-ai-list-plain">
              {improveResult.suggestedBenefits?.map((x, i) => (
                <li key={`b-${i}-${x}`}>{x}</li>
              ))}
            </ul>
          </div>
        )
      }
      return (
        <div className="li-ai-reply-body is-empty">
          <EmptyOut message={t('ai.improve.empty')} />
        </div>
      )
    }

    return null
  }

  return (
    <div className="li-grid li-grid--ai">
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
            <div className="li-ai-toolbar" role="tablist" aria-label={t('ai.title')}>
              {navItems.map((item) => (
                <button
                  key={item.id}
                  type="button"
                  role="tab"
                  className="li-ai-tab"
                  aria-selected={tab === item.id}
                  onClick={() => {
                    setTab(item.id)
                    setError('')
                  }}
                >
                  {item.icon}
                  <span>{item.label}</span>
                </button>
              ))}
            </div>
          </div>
        </header>

        <div className="li-ai-body">
          <div className="li-ai-workspace li-ai-workspace--split">
            <div className="li-ai-panel li-ai-panel--in">
              <div className="li-ai-panel-head">
                <h3 className="li-ai-panel-title">{panelInputTitle()}</h3>
                {tab === 'ask' ? (
                  <button type="button" className="li-ai-reset" onClick={resetAsk}>
                    <IconReset />
                    {t('ai.reset')}
                  </button>
                ) : null}
              </div>
              <div className="li-ai-panel-inner">{renderLeftColumn()}</div>
            </div>
            <div className="li-ai-panel li-ai-panel--out">
              <div className="li-ai-panel-head">
                <h3 className="li-ai-panel-title">{t('ai.panel.response')}</h3>
              </div>
              <div className="li-ai-panel-out-body">{renderRightColumn()}</div>
            </div>
          </div>

          {error ? <p className="li-ai-error">{error}</p> : null}

          <div className="li-ai-tips-bar">
            <h4 className="li-ai-tips-title">{t('ai.tips.title')}</h4>
            <ol className="li-ai-tips-list">
              {tipsKeys().map((k) => (
                <li key={k}>{t(k)}</li>
              ))}
            </ol>
          </div>
        </div>
      </section>

      <aside className="li-panel">
        <h4 className="li-side-title">{t('ai.side.tips')}</h4>
        <p className="li-side-text">{t('ai.side.tips.body')}</p>
      </aside>
    </div>
  )
}
