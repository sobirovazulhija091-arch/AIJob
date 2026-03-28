import { useEffect, useId, useState } from 'react'
import { NiceSelect } from '../components/NiceSelect'
import { getMySettings, updateMySettings, type UserSettings } from '../lib/api'
import { useI18n } from '../lib/i18n'
import { applyTheme, normalizeTheme } from '../lib/theme'
import './settings.css'

function IconSliders() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M4 21v-7M4 10V3M12 21v-9M12 8V3M20 21v-5M20 12V3M9 7h6M17 15h-2M7 11H5"
        stroke="currentColor"
        strokeWidth="1.75"
        strokeLinecap="round"
      />
    </svg>
  )
}

function IconHelp() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M9 9a3 3 0 115.2 2.08A2 2 0 0113 12v1M12 17h.01"
        stroke="currentColor"
        strokeWidth="1.75"
        strokeLinecap="round"
      />
      <circle cx="12" cy="12" r="9" stroke="currentColor" strokeWidth="1.75" />
    </svg>
  )
}

export function SettingsPage() {
  const themeLabelId = useId()
  const languageLabelId = useId()
  const { t, setLocale } = useI18n()
  const [model, setModel] = useState<UserSettings | null>(null)
  const [loading, setLoading] = useState(true)
  const [msg, setMsg] = useState('')
  const [error, setError] = useState('')

  useEffect(() => {
    ;(async () => {
      setLoading(true)
      setError('')
      try {
        const data = await getMySettings()
        setModel({ ...data, theme: normalizeTheme(data.theme) })
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Failed to load settings.')
      } finally {
        setLoading(false)
      }
    })()
  }, [])

  useEffect(() => {
    if (model?.language) setLocale(model.language)
  }, [model?.language, setLocale])

  useEffect(() => {
    if (model?.theme) applyTheme(model.theme)
  }, [model?.theme])

  async function save() {
    if (!model) return
    setMsg('')
    setError('')
    try {
      await updateMySettings(model)
      applyTheme(model.theme)
      setLocale(model.language)
      setMsg(t('settings.saved'))
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Save failed.')
    }
  }

  return (
    <div className="li-grid">
      <aside className="li-panel">
        <span className="li-settings-aside-icon" aria-hidden>
          <IconSliders />
        </span>
        <h4 className="li-side-title">{t('settings.preferences')}</h4>
        <p className="li-side-text">{t('settings.preferences.hint')}</p>
      </aside>

      <div className="li-settings-col">
        <div className="li-settings-hero">
          <h2 className="li-page-title">{t('settings.title')}</h2>
          <p className="li-page-sub">{t('settings.sub')}</p>
          {msg ? <div className="li-settings-banner li-settings-banner--ok">{msg}</div> : null}
          {error ? <div className="li-settings-banner li-settings-banner--err">{error}</div> : null}
        </div>

        {loading ? <div className="li-settings-loading">{t('settings.loading')}</div> : null}

        {!loading && model ? (
          <>
            <div className="li-settings-panel">
              <h3 className="li-settings-panel-title">{t('settings.section.display')}</h3>
              <p className="li-field-hint">{t('settings.section.display.hint')}</p>
              <div className="li-grid-2">
                <div className="li-stack">
                  <span className="li-label" id={themeLabelId}>
                    {t('settings.theme')}
                  </span>
                  <NiceSelect
                    aria-labelledby={themeLabelId}
                    value={model.theme}
                    onChange={(v) => setModel({ ...model, theme: normalizeTheme(v) })}
                    options={[
                      { value: 'light', label: t('theme.light') },
                      { value: 'dark', label: t('theme.dark') },
                    ]}
                  />
                </div>
                <div className="li-stack">
                  <span className="li-label" id={languageLabelId}>
                    {t('settings.language')}
                  </span>
                  <NiceSelect
                    aria-labelledby={languageLabelId}
                    value={model.language}
                    onChange={(v) => setModel({ ...model, language: v })}
                    options={[
                      { value: 'en', label: t('lang.en') },
                      { value: 'ru', label: t('lang.ru') },
                      { value: 'tg', label: t('lang.tg') },
                    ]}
                  />
                </div>
              </div>
            </div>

            <div className="li-settings-panel">
              <h3 className="li-settings-panel-title">{t('settings.section.alerts')}</h3>
              <p className="li-field-hint">{t('settings.section.alerts.hint')}</p>
              <label className="li-settings-check-row">
                <input
                  type="checkbox"
                  checked={model.emailNotifications}
                  onChange={(e) => setModel({ ...model, emailNotifications: e.target.checked })}
                />
                <span className="li-settings-check-text">
                  <strong>{t('settings.emailNotif')}</strong>
                  <span>{t('settings.emailNotif.hint')}</span>
                </span>
              </label>
              <label className="li-settings-check-row">
                <input
                  type="checkbox"
                  checked={model.pushNotifications}
                  onChange={(e) => setModel({ ...model, pushNotifications: e.target.checked })}
                />
                <span className="li-settings-check-text">
                  <strong>{t('settings.pushNotif')}</strong>
                  <span>{t('settings.pushNotif.hint')}</span>
                </span>
              </label>
            </div>

            <div className="li-settings-footer">
              <button className="li-btn primary" onClick={save} type="button">
                {t('settings.save')}
              </button>
            </div>
          </>
        ) : null}
      </div>

      <aside className="li-panel">
        <span className="li-settings-aside-icon" aria-hidden>
          <IconHelp />
        </span>
        <h4 className="li-side-title">{t('settings.help')}</h4>
        <p className="li-side-text">{t('settings.help.hint')}</p>
      </aside>
    </div>
  )
}
