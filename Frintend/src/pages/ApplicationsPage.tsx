import { useEffect, useState } from 'react'
import { applyToJob, getApplicationsByUser, getJobs, type Job, type JobApplication } from '../lib/api'
import { getUserId } from '../lib/auth'
import { useSearchParams } from 'react-router-dom'

export function ApplicationsPage() {
  const userId = getUserId()
  const [params] = useSearchParams()
  const [jobId, setJobId] = useState<number>(() => parseInt(params.get('jobId') ?? '0', 10) || 0)
  const [jobQuery, setJobQuery] = useState('')
  const [jobs, setJobs] = useState<Job[]>([])
  const [items, setItems] = useState<JobApplication[]>([])
  const [error, setError] = useState('')

  async function load() {
    if (!userId) return
    setError('')
    try {
      setItems(await getApplicationsByUser(userId))
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load applications.')
    }
  }

  useEffect(() => {
    void load()
    ;(async () => {
      try {
        setJobs(await getJobs())
      } catch {
        // ignore
      }
    })()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const jobIdFromUrl = params.get('jobId')

  useEffect(() => {
    const fromUrl = parseInt(jobIdFromUrl ?? '0', 10) || 0
    if (fromUrl && jobs.length > 0 && jobs.some((j) => j.id === fromUrl)) {
      const job = jobs.find((j) => j.id === fromUrl)!
      setJobId(fromUrl)
      setJobQuery(job.title)
      return
    }
    if (!jobs.length) return
    const query = jobQuery.trim().toLowerCase()
    if (!query) return
    const exact = jobs.find((j) => j.title.toLowerCase() === query)
    if (exact) {
      setJobId(exact.id)
      return
    }
    const firstContains = jobs.find((j) => j.title.toLowerCase().includes(query))
    if (firstContains) setJobId(firstContains.id)
  }, [jobQuery, jobs, jobIdFromUrl])

  async function apply() {
    if (!userId || !jobId) {
      setError('Please select a valid job title first.')
      return
    }
    setError('')
    try {
      await applyToJob(jobId, userId)
      await load()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Apply failed.')
    }
  }

  return (
    <div className="li-grid">
      <aside className="li-panel">
        <h4 className="li-side-title">Quick apply</h4>
        <p className="li-side-text">Pick a job title and apply instantly.</p>
      </aside>
      <section className="li-card li-card-pad" style={{ display: 'grid', gap: 12 }}>
        <h2 className="li-page-title">Applications</h2>
        <p className="li-page-sub">Track your applications and status.</p>
        <div style={{ display: 'grid', gridTemplateColumns: '1fr auto', gap: 10 }}>
          <div className="li-stack">
            <input
              className="li-input"
              value={jobQuery}
              onChange={(e) => setJobQuery(e.target.value)}
              placeholder="Type job title (e.g. Senior Developer)"
              list="jobs-list"
            />
            <datalist id="jobs-list">
              {jobs.map((j) => (
                <option key={j.id} value={j.title} />
              ))}
            </datalist>
          </div>
          <button className="li-btn primary" onClick={apply} type="button">
            Apply
          </button>
        </div>
        {error && <p style={{ color: 'crimson' }}>{error}</p>}
        <div className="li-list">
          {items.map((a) => (
            <div key={a.id} className="li-item" style={{ display: 'flex', justifyContent: 'space-between' }}>
              <span>
                Application #{a.id} • {jobs.find((j) => j.id === a.jobId)?.title ?? `Job #${a.jobId}`}
              </span>
              <b className="li-badge">{a.status}</b>
            </div>
          ))}
        </div>
      </section>
      <aside className="li-panel">
        <h4 className="li-side-title">Tips</h4>
        <p className="li-side-text">Follow up after applying to increase response rate.</p>
      </aside>
    </div>
  )
}

