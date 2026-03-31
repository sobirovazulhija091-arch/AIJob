import { useCallback, useEffect, useId, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { NiceSelect } from '../components/NiceSelect'
import {
  addOrganizationMember,
  inviteOrganizationMember,
  createOrganization,
  getMemberDirectory,
  getOrganizationMembers,
  getMyOrganizations,
  type MemberDirectoryEntry,
  type OrganizationMemberRow,
  type OrganizationRow,
} from '../lib/api'
import { getEmail, getHeaderDisplayName, getUserId } from '../lib/auth'
import { useI18n } from '../lib/i18n'
import './company.css'

function directoryRoleLabelKey(role: string): string {
  if (role === 'Organization') return 'role.organization'
  if (role === 'Candidate') return 'role.candidate'
  return 'directory.memberFallback'
}

function IconCompanyCreate() {
  return (
    <svg width="22" height="22" viewBox="0 0 24 24" fill="none" aria-hidden>
      <circle cx="12" cy="12" r="9" stroke="currentColor" strokeWidth="1.65" />
      <path d="M12 8v8M8 12h8" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
    </svg>
  )
}

function IconCompanyTeam() {
  return (
    <svg width="22" height="22" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M16 18v-1a4 4 0 00-4-4h-1m-5 4v-1a4 4 0 014-4h1m4 4v-1a4 4 0 00-3-3.87M8 6a3 3 0 106 0 3 3 0 10-6 0zM3 18v-1a4 4 0 014-4h2"
        stroke="currentColor"
        strokeWidth="1.65"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  )
}

export function CompanyPage() {
  const { t } = useI18n()
  const me = getUserId()
  const orgLabelId = useId()
  const typeLabelId = useId()

  const [orgs, setOrgs] = useState<OrganizationRow[]>([])
  const [selectedOrgId, setSelectedOrgId] = useState<number | ''>('')
  const [members, setMembers] = useState<OrganizationMemberRow[]>([])
  const [directory, setDirectory] = useState<MemberDirectoryEntry[]>([])

  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [location, setLocation] = useState('')
  const [logoUrl, setLogoUrl] = useState('')
  const [orgType, setOrgType] = useState('Company')

  const [addUserId, setAddUserId] = useState('')
  const [memberRole, setMemberRole] = useState('Member')

  const [loadingOrgs, setLoadingOrgs] = useState(true)
  const [loadingMembers, setLoadingMembers] = useState(false)
  const [busyCreate, setBusyCreate] = useState(false)
  const [busyAdd, setBusyAdd] = useState(false)
  const [msgCreate, setMsgCreate] = useState('')
  const [errCreate, setErrCreate] = useState('')
  const [msgAdd, setMsgAdd] = useState('')
  const [errAdd, setErrAdd] = useState('')

  const typeOptions = useMemo(
    () => [
      { value: 'Startup', label: t('company.typeStartup') },
      { value: 'Company', label: t('company.typeCompany') },
      { value: 'Agency', label: t('company.typeAgency') },
    ],
    [t],
  )

  const orgSelectOptions = useMemo(() => {
    const sorted = [...orgs].sort((a, b) => a.name.localeCompare(b.name, undefined, { sensitivity: 'base' }))
    return [
      { value: '', label: t('company.selectOrgPh') },
      ...sorted.map((o) => ({ value: String(o.id), label: o.name })),
    ]
  }, [orgs, t])

  const refreshOrgsAndDirectory = useCallback(async () => {
    const [o, d] = await Promise.all([getMyOrganizations(), getMemberDirectory()])
    setOrgs(o)
    setDirectory(d)
  }, [])

  useEffect(() => {
    let cancelled = false
    void (async () => {
      setLoadingOrgs(true)
      try {
        await refreshOrgsAndDirectory()
      } catch {
        if (!cancelled) setOrgs([])
      } finally {
        if (!cancelled) setLoadingOrgs(false)
      }
    })()
    return () => {
      cancelled = true
    }
  }, [refreshOrgsAndDirectory])

  useEffect(() => {
    if (selectedOrgId === '') {
      setMembers([])
      return
    }
    let cancelled = false
    void (async () => {
      setLoadingMembers(true)
      try {
        const list = await getOrganizationMembers(selectedOrgId)
        if (!cancelled) setMembers(list)
      } catch {
        if (!cancelled) setMembers([])
      } finally {
        if (!cancelled) setLoadingMembers(false)
      }
    })()
    return () => {
      cancelled = true
    }
  }, [selectedOrgId])

  const memberUserIds = useMemo(() => new Set(members.map((m) => m.userId)), [members])

  const nameForUser = useCallback(
    (userId: number) => {
      if (me != null && userId === me) {
        const self =
          getHeaderDisplayName()?.trim() || getEmail()?.trim()
        if (self) return self
      }
      const p = directory.find((x) => x.id === userId)
      if (!p) return `User ${userId}`
      return p.fullName?.trim() || p.userName?.trim() || p.email?.trim() || `User ${userId}`
    },
    [directory, me],
  )

  /** Anyone in the directory except you and current members (candidates *and* employers). */
  const peopleAddOptions = useMemo(() => {
    const rows = directory.filter((p) => (me == null || p.id !== me) && !memberUserIds.has(p.id))
    rows.sort((a, b) => nameForUser(a.id).localeCompare(nameForUser(b.id), undefined, { sensitivity: 'base' }))
    return [
      { value: '', label: t('company.pickCandidatePh') },
      ...rows.map((p) => {
        const roleLbl = t(directoryRoleLabelKey(p.role))
        const mail = p.email?.trim()
        return {
          value: String(p.id),
          label: mail ? `${nameForUser(p.id)} · ${roleLbl} · ${mail}` : `${nameForUser(p.id)} · ${roleLbl}`,
        }
      }),
    ]
  }, [directory, me, memberUserIds, nameForUser, t])

  const pickablePeopleCount = Math.max(0, peopleAddOptions.length - 1)

  async function submitCreate() {
    setMsgCreate('')
    setErrCreate('')
    const n = name.trim()
    if (!n) {
      setErrCreate(t('company.createErrName'))
      return
    }
    setBusyCreate(true)
    try {
      const created = await createOrganization({
        name: n,
        description: description.trim() || undefined,
        type: orgType,
        location: location.trim() || undefined,
        logoUrl: logoUrl.trim() || undefined,
      })
      await refreshOrgsAndDirectory()
      setSelectedOrgId(created.id)
      setName('')
      setDescription('')
      setLocation('')
      setLogoUrl('')
      setOrgType('Company')
      setMsgCreate(t('company.createSuccess'))

      if (me != null) {
        try {
          await addOrganizationMember({ organizationId: created.id, userId: me, role: 'Owner' })
          const list = await getOrganizationMembers(created.id)
          setMembers(list)
        } catch {
          /* optional link; user can still use org in Recruiting */
        }
      }
    } catch (e) {
      setErrCreate(e instanceof Error ? e.message : t('company.createErr'))
    } finally {
      setBusyCreate(false)
    }
  }

  async function submitAddMember() {
    setMsgAdd('')
    setErrAdd('')
    if (selectedOrgId === '') {
      setErrAdd(t('company.addErrNoOrg'))
      return
    }
    const uid = parseInt(addUserId, 10)
    if (!Number.isFinite(uid) || uid <= 0) {
      setErrAdd(t('company.addErrPick'))
      return
    }
    const role = memberRole.trim() || 'Member'
    setBusyAdd(true)
    try {
      await inviteOrganizationMember({ organizationId: selectedOrgId, userId: uid, role })
      setAddUserId('')
      setMemberRole('Member')
      setMsgAdd(t('company.addMemberOk'))
    } catch (e) {
      setErrAdd(e instanceof Error ? e.message : t('company.addMemberErr'))
    } finally {
      setBusyAdd(false)
    }
  }

  return (
    <div className="li-company-page">
      <section className="li-card li-card-pad li-company-main">
        <header className="li-company-header">
          <h2 className="li-page-title li-company-title">{t('company.title')}</h2>
          <p className="li-page-sub li-company-intro">{t('company.sub')}</p>
        </header>

        {loadingOrgs ? <p className="li-company-loading">{t('directory.loading')}</p> : null}

        <div className="li-company-grid">
          <div className="li-company-block li-company-block--create">
            <div className="li-company-block-head">
              <span className="li-company-block-icon li-company-block-icon--create" aria-hidden>
                <IconCompanyCreate />
              </span>
              <div className="li-company-block-head-text">
                <h3>{t('company.createTitle')}</h3>
                <span className="li-company-block-badge">{t('company.badgeSetup')}</span>
              </div>
            </div>
            <p className="li-company-lead">{t('company.createSub')}</p>
            <div className="li-company-block-body">
            {msgCreate ? <p className="li-company-ok">{msgCreate}</p> : null}
            {errCreate ? <p className="li-company-err">{errCreate}</p> : null}

            <label className="li-stack li-company-field">
              <span className="li-label">{t('company.name')}</span>
              <input
                className="li-input li-company-input"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder={t('company.namePh')}
                autoComplete="organization"
              />
            </label>
            <label className="li-stack li-company-field">
              <span className="li-label">{t('company.desc')}</span>
              <textarea
                className="li-textarea li-company-textarea"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                rows={5}
                placeholder={t('company.descPh')}
              />
            </label>
            <label className="li-stack li-company-field">
              <span className="li-label">{t('company.location')}</span>
              <input
                className="li-input li-company-input"
                value={location}
                onChange={(e) => setLocation(e.target.value)}
                placeholder={t('company.locationPh')}
              />
            </label>
            <label className="li-stack li-company-field">
              <span className="li-label">{t('company.logoUrl')}</span>
              <input
                className="li-input li-company-input"
                type="url"
                inputMode="url"
                value={logoUrl}
                onChange={(e) => setLogoUrl(e.target.value)}
                placeholder={t('company.logoUrlPh')}
                autoComplete="off"
              />
            </label>
            <p className="li-field-hint li-company-hint">{t('company.logoUrlHint')}</p>
            <div className="li-stack li-company-field">
              <span className="li-label" id={typeLabelId}>
                {t('company.type')}
              </span>
              <NiceSelect
                aria-labelledby={typeLabelId}
                value={orgType}
                onChange={setOrgType}
                options={typeOptions}
              />
            </div>
            <div className="li-company-row-actions">
              <button
                type="button"
                className="li-btn primary li-company-btn-primary"
                disabled={busyCreate}
                onClick={() => void submitCreate()}
              >
                {busyCreate ? t('company.createBusy') : t('company.createSubmit')}
              </button>
            </div>
            <p className="li-field-hint li-company-hint">{t('company.ownerNote')}</p>
            </div>
          </div>

          <div className="li-company-block li-company-block--team">
            <div className="li-company-block-head">
              <span className="li-company-block-icon li-company-block-icon--team" aria-hidden>
                <IconCompanyTeam />
              </span>
              <div className="li-company-block-head-text">
                <h3>{t('company.teamTitle')}</h3>
                <span className="li-company-block-badge li-company-block-badge--muted">{t('company.badgeMembers')}</span>
              </div>
            </div>
            <p className="li-company-lead">{t('company.teamSub')}</p>
            <div className="li-company-block-body">

            <div className="li-stack li-company-field">
              <span className="li-label" id={orgLabelId}>
                {t('company.selectOrg')}
              </span>
              <NiceSelect
                aria-labelledby={orgLabelId}
                value={selectedOrgId === '' ? '' : String(selectedOrgId)}
                onChange={(v) => setSelectedOrgId(v === '' ? '' : parseInt(v, 10))}
                options={orgSelectOptions}
                disabled={!orgs.length}
              />
            </div>

            <div className="li-company-members">
              <h3 className="li-company-subheading">{t('company.membersTitle')}</h3>
              <p className="li-company-lead">{t('company.membersSub')}</p>
              {loadingMembers ? <p className="li-page-sub">{t('directory.loading')}</p> : null}
              {!loadingMembers && selectedOrgId !== '' && !members.length ? (
                <div className="li-company-members-placeholder">
                  <p>{t('company.membersEmpty')}</p>
                </div>
              ) : null}
              {members.length > 0 ? (
                <table className="li-company-table">
                  <thead>
                    <tr>
                      <th>{t('company.tablePerson')}</th>
                      <th>{t('company.tableRole')}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {members.map((m) => (
                      <tr key={m.id}>
                        <td>
                          <Link to={`/people/${m.userId}`}>{nameForUser(m.userId)}</Link>
                          {me != null && m.userId === me ? (
                            <span className="li-company-you-badge">({t('company.youBadge')})</span>
                          ) : null}
                        </td>
                        <td>{m.role}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              ) : null}
            </div>

            <div className="li-company-add-block">
              <h3 className="li-company-subheading">{t('company.addTitle')}</h3>
              <p className="li-company-lead">{t('company.addSub')}</p>
              {msgAdd ? <p className="li-company-ok">{msgAdd}</p> : null}
              {errAdd ? <p className="li-company-err">{errAdd}</p> : null}
              <div className="li-stack li-company-field">
                <span className="li-label">{t('company.pickCandidate')}</span>
                <NiceSelect
                  value={addUserId}
                  onChange={setAddUserId}
                  options={peopleAddOptions}
                  disabled={selectedOrgId === ''}
                />
                {selectedOrgId === '' ? (
                  <p className="li-field-hint li-company-hint" style={{ marginTop: 8 }}>
                    {t('company.pickPersonNeedOrg')}
                  </p>
                ) : null}
                {selectedOrgId !== '' && pickablePeopleCount === 0 ? (
                  <p className="li-company-err" style={{ marginTop: 8, marginBottom: 0 }}>
                    {t('company.personPickerEmpty')}
                  </p>
                ) : null}
              </div>
              <label className="li-stack li-company-field">
                <span className="li-label">{t('company.memberRole')}</span>
                <input
                  className="li-input li-company-input"
                  value={memberRole}
                  onChange={(e) => setMemberRole(e.target.value)}
                  placeholder={t('company.memberRolePh')}
                />
              </label>
              <div className="li-company-row-actions">
                <button
                  type="button"
                  className="li-btn primary li-company-btn-primary"
                  disabled={busyAdd || selectedOrgId === ''}
                  onClick={() => void submitAddMember()}
                >
                  {busyAdd ? t('company.addMemberBusy') : t('company.addMemberSubmit')}
                </button>
              </div>
              <p className="li-field-hint li-company-hint">{t('company.addHint')}</p>
            </div>
            </div>
          </div>
        </div>

        <p className="li-company-footer-link">
          <Link to="/recruiting">{t('company.gotoRecruiting')} →</Link>
        </p>
      </section>

      <div className="li-company-asides">
        <aside className="li-panel li-company-panel">
          <h4 className="li-side-title">{t('company.sideTitle')}</h4>
          <p className="li-side-text">{t('company.sideHint')}</p>
        </aside>
        <aside className="li-panel li-company-panel">
          <h4 className="li-side-title">{t('company.asideTitle')}</h4>
          <p className="li-side-text">{t('company.asideBody')}</p>
        </aside>
      </div>
    </div>
  )
}
