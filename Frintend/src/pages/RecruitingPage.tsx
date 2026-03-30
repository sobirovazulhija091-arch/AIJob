import { useEffect, useId, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { NiceSelect } from '../components/NiceSelect'
import {
  changeApplicationStatus,
  getApplicationsByJob,
  getJobs,
  getPublicProfilesByUsers,
  type Job,
  type JobApplication,
} from '../lib/api'
import { initialsFromLabel } from '../lib/auth'
import { useI18n } from '../lib/i18n'
import './recruiting.css'

const STATUS = { pending: 1, accepted: 2, rejected: 3, interview: 4 } as const

function IconPipeline() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M4 19V5M8 19V9m4 10V6m4 13v-7m4 7V9"
        stroke="currentColor"
        strokeWidth="1.75"
        strokeLinecap="round"
      />
    </svg>
  )
}

function IconBriefcase() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M9 8V6a2 2 0 012-2h2a2 2 0 012 2v2M4 12h16v8a1 1 0 01-1 1H5a1 1 0 01-1-1v-8zM4 12V9a2 2 0 012-2h12a2 2 0 012 2v3"
        stroke="currentColor"
        strokeWidth="1.75"
        strokeLinejoin="round"
      />
    </svg>
  )
}

function IconInbox() {
  return (
    <svg width="26" height="26" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M22 12h-6l-2 3H10L8 12H2M5 12H2a2 2 0 00-2 2v6a2 2 0 002 2h20a2 2 0 002-2v-6a2 2 0 00-2-2h-3M5 12V7a2 2 0 012-2h10a2 2 0 012 2v5"
        stroke="currentColor"
        strokeWidth="1.65"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  )
}

function normalizeStatusKey(status: string): 'pending' | 'accepted' | 'rejected' | 'interview' | 'unknown' {
  const n = parseInt(String(status), 10)
  if (n === STATUS.pending) return 'pending'
  if (n === STATUS.accepted) return 'accepted'
  if (n === STATUS.rejected) return 'rejected'
  if (n === STATUS.interview) return 'interview'
  const s = String(status).toLowerCase()
  if (s.includes('pending')) return 'pending'
  if (s.includes('accept')) return 'accepted'
  if (s.includes('reject')) return 'rejected'
  if (s.includes('interview')) return 'interview'
  return 'unknown'
}

function formatJobSalary(job: Job): string {
  const lang = document.documentElement.lang || undefined
  const fmt = new Intl.NumberFormat(lang, { maximumFractionDigits: 0 })
  return `${fmt.format(job.salaryMin)} – ${fmt.format(job.salaryMax)}`
}

export function RecruitingPage() {
  const { t } = useI18n()
  const pickJobLabelId = useId()
  const [jobs, setJobs] = useState<Job[]>([])
  const [jobsLoading, setJobsLoading] = useState(true)
  const [jobId, setJobId] = useState(0)
  const [apps, setApps] = useState<JobApplication[]>([])
  const [appsLoading, setAppsLoading] = useState(false)
  const [names, setNames] = useState<Record<number, string>>({})
  const [error, setError] = useState('')

  const selectedJob = useMemo(() => jobs.find((j) => j.id === jobId), [jobs, jobId])
  const jobOptions = useMemo(
    () => [
      { value: '', label: t('recruiting.selectPlaceholder') },
      ...jobs.map((j) => ({ value: String(j.id), label: j.title })),
    ],
    [jobs, t],
  )

  useEffect(() => {
    void (async () => {
      setJobsLoading(true)
      try {
        setJobs(await getJobs())
      } catch {
        // ignore
      } finally {
        setJobsLoading(false)
      }
    })()
  }, [])

  useEffect(() => {
    if (!jobId) {
      setApps([])
      setNames({})
      return
    }
    void (async () => {
      setAppsLoading(true)
      setError('')
      try {
        const list = await getApplicationsByJob(jobId)
        setApps(list)
        const userIds = [...new Set(list.map((a) => a.userId))]
        if (userIds.length) {
          const profiles = await getPublicProfilesByUsers(userIds)
          const map: Record<number, string> = {}
          for (const p of profiles)
            map[p.userId] = p.fullName || `${p.firstName} ${p.lastName}`.trim() || `User ${p.userId}`
          setNames(map)
        } else {
          setNames({})
        }
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Failed to load applications.')
      } finally {
        setAppsLoading(false)
      }
    })()
  }, [jobId])

  async function setStatus(id: number, status: number) {
    setError('')
    try {
      await changeApplicationStatus(id, status)
      if (jobId) {
        const list = await getApplicationsByJob(jobId)
        setApps(list)
        const userIds = [...new Set(list.map((a) => a.userId))]
        if (userIds.length) {
          const profiles = await getPublicProfilesByUsers(userIds)
          const map: Record<number, string> = {}
          for (const p of profiles)
            map[p.userId] = p.fullName || `${p.firstName} ${p.lastName}`.trim() || `User ${p.userId}`
          setNames((prev) => ({ ...prev, ...map }))
        }
      }
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Update failed.')
    }
  }

  function statusBadgeLabel(key: ReturnType<typeof normalizeStatusKey>): string {
    switch (key) {
      case 'pending':
        return t('recruiting.pending')
      case 'accepted':
        return t('recruiting.accepted')
      case 'rejected':
        return t('recruiting.rejected')
      case 'interview':
        return t('recruiting.interview')
      default:
        return String(t('recruiting.status'))
    }
  }

  return (
    <div className="li-grid li-recruit-page">
      <aside className="li-panel li-recruit-side">
        <span className="li-recruit-aside-ic" aria-hidden>
          <IconPipeline />
        </span>
        <h4 className="li-side-title">{t('recruiting.sidebarTitle')}</h4>
        <p className="li-side-text">{t('recruiting.sidebarHint')}</p>
      </aside>

      <section className="li-recruit-col">
        <div className="li-recruit-hero">
          <h2 className="li-page-title">{t('recruiting.title')}</h2>
          <p className="li-page-sub">{t('recruiting.sub')}</p>

          {jobsLoading ? (
            <p className="li-recruit-loading" style={{ margin: '12px 0 0', padding: 0 }}>
              {t('recruiting.loadingJobs')}
            </p>
          ) : null}

          {!jobsLoading && !jobs.length ? (
            <div className="li-recruit-empty" style={{ marginTop: 14 }}>
              <strong>{t('recruiting.noJobsEmployer')}</strong>
            </div>
          ) : null}

          {!jobsLoading && jobs.length > 0 ? (
            <div className="li-recruit-picker">
              <div className="li-stack">
                <span className="li-label" id={pickJobLabelId}>
                  {t('recruiting.pickJob')}
                </span>
                <NiceSelect
                  aria-labelledby={pickJobLabelId}
                  value={jobId ? String(jobId) : ''}
                  onChange={(v) => setJobId(parseInt(v || '0', 10))}
                  options={jobOptions}
                />
              </div>
              {selectedJob ? (
                <div className="li-recruit-job-summary">
                  <strong>{selectedJob.title}</strong>
                  {' · '}
                  {selectedJob.location?.trim() || '—'} · {formatJobSalary(selectedJob)}
                </div>
              ) : null}
            </div>
          ) : null}

          {error ? <p style={{ color: 'crimson', margin: '12px 0 0', fontSize: 14 }}>{error}</p> : null}
        </div>

        {!jobsLoading && jobs.length > 0 ? (
          <div className="li-recruit-workspace">
            <div className="li-recruit-workspace-head">
              <h3>{t('recruiting.workspaceTitle')}</h3>
              <div className="li-recruit-workspace-meta">
                {jobId > 0 && selectedJob ? (
                  <>
                    <span className="li-recruit-job-pill" title={selectedJob.title}>
                      {selectedJob.title}
                    </span>
                    {!appsLoading ? (
                      <span className="li-recruit-count" title={t('recruiting.applicants')}>
                        {apps.length}
                      </span>
                    ) : null}
                  </>
                ) : (
                  <span className="li-recruit-job-pill li-recruit-job-pill--ghost">{t('recruiting.selectPlaceholder')}</span>
                )}
              </div>
            </div>
            <div className="li-recruit-workspace-body">
              {jobId === 0 ? (
                <div className="li-recruit-prompt">
                  <div className="li-recruit-prompt-ic" aria-hidden>
                    <IconInbox />
                  </div>
                  <p className="li-recruit-prompt-title">{t('recruiting.applicants')}</p>
                  <p className="li-recruit-prompt-text">{t('recruiting.workspaceEmpty')}</p>
                </div>
              ) : null}

              {jobId > 0 && appsLoading ? (
                <div className="li-recruit-loading">{t('recruiting.loadingApplicants')}</div>
              ) : null}

              {jobId > 0 && !appsLoading && apps.length === 0 ? (
                <div className="li-recruit-empty" style={{ padding: '28px 18px' }}>
                  <strong>{t('recruiting.applicants')}</strong>
                  <span>{t('recruiting.emptyApplicants')}</span>
                </div>
              ) : null}

              {jobId > 0 && !appsLoading && apps.length > 0 ? (
                <div className="li-recruit-list">
                  {apps.map((a) => {
                    const name = names[a.userId] ?? `User ${a.userId}`
                    const initials = initialsFromLabel(name)
                    const sk = normalizeStatusKey(a.status)
                    const badgeClass =
                      sk === 'pending'
                        ? 'li-recruit-badge--pending'
                        : sk === 'accepted'
                          ? 'li-recruit-badge--accepted'
                          : sk === 'rejected'
                            ? 'li-recruit-badge--rejected'
                            : sk === 'interview'
                              ? 'li-recruit-badge--interview'
                              : 'li-recruit-badge--pending'
                    return (
                      <article key={a.id} className="li-recruit-card">
                        <div className="li-recruit-card-top">
                          <div className="li-recruit-avatar" aria-hidden>
                            {initials}
                          </div>
                          <div className="li-recruit-who">
                            <p className="li-recruit-name">
                              <Link className="li-recruit-name-link" to={`/people/${a.userId}`}>
                                {name}
                              </Link>
                            </p>
                            <p className="li-recruit-meta">{t('recruiting.applicationRef')} #{a.id}</p>
                            <span className={`li-recruit-badge ${badgeClass}`}>{statusBadgeLabel(sk)}</span>
                          </div>
                        </div>
                        <div className="li-recruit-actions">
                          <p className="li-recruit-act-label">{t('recruiting.statusActions')}</p>
                          <div className="li-recruit-row-actions">
                            <button className="li-btn" type="button" onClick={() => setStatus(a.id, STATUS.pending)}>
                              {t('recruiting.pending')}
                            </button>
                            <button className="li-btn primary" type="button" onClick={() => setStatus(a.id, STATUS.accepted)}>
                              {t('recruiting.accepted')}
                            </button>
                            <button className="li-btn" type="button" onClick={() => setStatus(a.id, STATUS.rejected)}>
                              {t('recruiting.rejected')}
                            </button>
                            <button className="li-btn" type="button" onClick={() => setStatus(a.id, STATUS.interview)}>
                              {t('recruiting.interview')}
                            </button>
                          </div>
                        </div>
                      </article>
                    )
                  })}
                </div>
              ) : null}
            </div>
          </div>
        ) : null}
      </section>

      <aside className="li-panel li-recruit-side">
        <span className="li-recruit-aside-ic" aria-hidden>
          <IconBriefcase />
        </span>
        <h4 className="li-side-title">{t('recruiting.tipsTitle')}</h4>
        <p className="li-side-text">{t('recruiting.tipsBody')}</p>
        <p className="li-recruit-legend-title">{t('recruiting.legendTitle')}</p>
        <ul className="li-recruit-legend">
          <li>
            <span className="li-recruit-legend-dot li-recruit-legend-dot--pending" aria-hidden />
            <span>
              <strong>{t('recruiting.pending')}</strong>
            </span>
          </li>
          <li>
            <span className="li-recruit-legend-dot li-recruit-legend-dot--interview" aria-hidden />
            <span>
              <strong>{t('recruiting.interview')}</strong>
            </span>
          </li>
          <li>
            <span className="li-recruit-legend-dot li-recruit-legend-dot--accepted" aria-hidden />
            <span>
              <strong>{t('recruiting.accepted')}</strong>
            </span>
          </li>
          <li>
            <span className="li-recruit-legend-dot li-recruit-legend-dot--rejected" aria-hidden />
            <span>
              <strong>{t('recruiting.rejected')}</strong>
            </span>
          </li>
        </ul>
      </aside>
    </div>
  )
}
