/** Persisted so the shell matches before settings fetch runs. */
const THEME_KEY = 'aijob.theme'

export type ThemeMode = 'light' | 'dark'

export function normalizeTheme(raw: string | null | undefined): ThemeMode {
  return raw === 'dark' ? 'dark' : 'light'
}

export function readStoredTheme(): ThemeMode | null {
  try {
    const s = localStorage.getItem(THEME_KEY)
    if (s === 'dark' || s === 'light') return s
  } catch {
    // ignore
  }
  return null
}

export function applyTheme(raw: string | null | undefined) {
  const mode = normalizeTheme(raw)
  document.documentElement.dataset.theme = mode
  try {
    localStorage.setItem(THEME_KEY, mode)
  } catch {
    // ignore
  }
}

export function clearStoredTheme() {
  try {
    localStorage.removeItem(THEME_KEY)
  } catch {
    // ignore
  }
}
