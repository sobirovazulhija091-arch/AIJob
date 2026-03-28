import { useEffect, useId, useMemo, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { NiceSelect } from '../components/NiceSelect'
import { getJobs, type Job } from '../lib/api'
import './jobs.css'

function IconPin() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M12 22s7-5.2 7-12a7 7 0 10-14 0c0 6.8 7 12 7 12z"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinejoin="round"
      />
      <circle cx="12" cy="10" r="2" fill="currentColor" />
    </svg>
  )
}

function IconBriefcase() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M9 8V6a2 2 0 012-2h2a2 2 0 012 2v2M4 12h16v8a1 1 0 01-1 1H5a1 1 0 01-1-1v-8zM4 12V9a2 2 0 012-2h12a2 2 0 012 2v3"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinejoin="round"
      />
    </svg>
  )
}

function IconLevel() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M4 20h4l10-10-4-4L4 16v4z"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinejoin="round"
      />
      <path d="M13.5 6.5l4 4" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  )
}

function formatJobType(raw: string): string {
  const t = raw.replace(/([a-z])([A-Z])/g, '$1 $2')
  return t.charAt(0).toUpperCase() + t.slice(1).replace(/_/g, ' ')
}

function formatSalaryRange(min: number, max: number): string {
  const lang = document.documentElement.lang || undefined
  const fmt = new Intl.NumberFormat(lang, { maximumFractionDigits: 0 })
  return `${fmt.format(min)} – ${fmt.format(max)}`
}

export function JobsPage() {
  const [searchParams] = useSearchParams()
  const jobTypeLabelId = useId()
  const jobLevelLabelId = useId()
  const [items, setItems] = useState<Job[]>([])
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [typeFilter, setTypeFilter] = useState('')
  const [levelFilter, setLevelFilter] = useState('')
  const [locationFilter, setLocationFilter] = useState('')
  const [expanded, setExpanded] = useState<Record<number, boolean>>({})

  useEffect(() => {
    ;(async () => {
      setLoading(true)
      setError('')
      try {
        setItems(await getJobs())
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Failed to load jobs.')
      } finally {
        setLoading(false)
      }
    })()
  }, [])

  useEffect(() => {
    setSearch(searchParams.get('q') ?? '')
  }, [searchParams])

  const jobTypes = useMemo(() => [...new Set(items.map((j) => j.jobType))].sort(), [items])
  const levels = useMemo(() => [...new Set(items.map((j) => j.experienceLevel))].sort(), [items])
  const typeOptions = useMemo(
    () => [
      { value: '', label: 'All types' },
      ...jobTypes.map((ty) => ({ value: ty, label: formatJobType(ty) })),
    ],
    [jobTypes],
  )
  const levelOptions = useMemo(
    () => [{ value: '', label: 'All levels' }, ...levels.map((l) => ({ value: l, label: l }))],
    [levels],
  )

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase()
    return items.filter((j) => {
      if (q) {
        const inTitle = j.title.toLowerCase().includes(q)
        const inDesc = (j.description ?? '').toLowerCase().includes(q)
        if (!inTitle && !inDesc) return false
      }
      if (typeFilter && j.jobType !== typeFilter) return false
      if (levelFilter && j.experienceLevel !== levelFilter) return false
      if (locationFilter.trim()) {
        const loc = (j.location ?? '').toLowerCase()
        if (!loc.includes(locationFilter.trim().toLowerCase())) return false
      }
      return true
    })
  }, [items, search, typeFilter, levelFilter, locationFilter])

  function clearFilters() {
    setSearch('')
    setTypeFilter('')
    setLevelFilter('')
    setLocationFilter('')
  }

  const hasActiveFilters = Boolean(search.trim() || typeFilter || levelFilter || locationFilter.trim())

  return (
    <div className="li-grid">
      <aside className="li-panel">
        <h4 className="li-side-title">Filters</h4>
        <p className="li-side-text">Narrow listings by keyword, location, job type, and seniority.</p>
        <div className="li-jobs-filters" style={{ marginTop: 14 }}>
          <label className="li-stack">
            <span className="li-label">Search</span>
            <input
              className="li-input"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Title or description"
              type="search"
              autoComplete="off"
            />
          </label>
          <label className="li-stack">
            <span className="li-label">Location</span>
            <input
              className="li-input"
              value={locationFilter}
              onChange={(e) => setLocationFilter(e.target.value)}
              placeholder="e.g. Remote, Dushanbe"
              autoComplete="off"
            />
          </label>
          <div className="li-stack">
            <span className="li-label" id={jobTypeLabelId}>
              Job type
            </span>
            <NiceSelect
              aria-labelledby={jobTypeLabelId}
              value={typeFilter}
              onChange={setTypeFilter}
              options={typeOptions}
            />
          </div>
          <div className="li-stack">
            <span className="li-label" id={jobLevelLabelId}>
              Experience
            </span>
            <NiceSelect
              aria-labelledby={jobLevelLabelId}
              value={levelFilter}
              onChange={setLevelFilter}
              options={levelOptions}
            />
          </div>
          <div className="li-jobs-filter-actions">
            <button className="li-job-ghost" type="button" onClick={clearFilters} disabled={!hasActiveFilters}>
              Clear filters
            </button>
          </div>
        </div>
      </aside>

      <section className="li-jobs-col">
        <div className="li-jobs-hero">
          <h2 className="li-page-title">Jobs</h2>
          <p className="li-page-sub">Discover roles that match your goals.</p>
          {error ? <p style={{ color: 'crimson', margin: '8px 0 0', fontSize: 14 }}>{error}</p> : null}
        </div>

        {loading ? <div className="li-jobs-loading">Loading openings…</div> : null}

        {!loading && !filtered.length ? (
          <div className="li-jobs-empty">
            <strong>{items.length ? 'No matches' : 'No jobs yet'}</strong>
            {items.length
              ? 'Try clearing filters or searching with different keywords.'
              : 'Check back later—or ask your admin to publish roles.'}
          </div>
        ) : null}

        {!loading && filtered.length ? (
          <div className="li-jobs-list">
            {filtered.map((j) => {
              const loc = j.location?.trim() || 'Location TBD'
              const desc = j.description?.trim()
              const isOpen = expanded[j.id]
              const showToggle = Boolean(desc && desc.length > 160)
              return (
                <article key={j.id} className="li-job-card">
                  <div className="li-job-card-body">
                    <h3 className="li-job-title">{j.title}</h3>
                    <div className="li-job-chips">
                      <span className="li-job-chip">
                        <IconPin />
                        {loc}
                      </span>
                      <span className="li-job-chip">
                        <IconBriefcase />
                        {formatJobType(j.jobType)}
                      </span>
                      <span className="li-job-chip li-job-chip--accent">
                        <IconLevel />
                        {j.experienceLevel}
                      </span>
                    </div>
                    <div className="li-job-salary">
                      <p className="li-job-salary-label">Compensation range</p>
                      <p className="li-job-salary-range">{formatSalaryRange(j.salaryMin, j.salaryMax)}</p>
                      <p className="li-job-salary-hint">
                        Formatted for your locale. Pay period (monthly vs yearly) is set by the employer—confirm when
                        you apply.
                      </p>
                    </div>
                    {desc ? (
                      <p className={`li-job-desc ${!isOpen && showToggle ? 'li-job-desc--clamp' : ''}`}>{desc}</p>
                    ) : (
                      <p className="li-job-desc" style={{ color: 'var(--li-muted)', fontStyle: 'italic' }}>
                        No full description provided.
                      </p>
                    )}
                  </div>
                  <div className="li-job-actions">
                    <Link className="li-btn primary" to={`/applications?jobId=${j.id}`}>
                      Apply
                    </Link>
                    {showToggle ? (
                      <button
                        type="button"
                        className="li-job-ghost"
                        onClick={() => setExpanded((prev) => ({ ...prev, [j.id]: !prev[j.id] }))}
                      >
                        {isOpen ? 'Show less' : 'Full description'}
                      </button>
                    ) : null}
                  </div>
                </article>
              )
            })}
          </div>
        ) : null}
      </section>

      <aside className="li-panel">
        <h4 className="li-side-title">Tips</h4>
        <p className="li-side-text">
          Apply quickly from each card. Align your profile and expected salary with the role before you submit—employers
          see your CareerHub profile.
        </p>
      </aside>
    </div>
  )
}
