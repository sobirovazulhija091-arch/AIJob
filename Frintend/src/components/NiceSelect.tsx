import { useEffect, useId, useRef, useState } from 'react'
import './nice-select.css'

export type NiceSelectOption = { value: string; label: string }

type Props = {
  value: string
  onChange: (value: string) => void
  options: NiceSelectOption[]
  disabled?: boolean
  id?: string
  'aria-label'?: string
  'aria-labelledby'?: string
}

export function NiceSelect({
  value,
  onChange,
  options,
  disabled,
  id,
  'aria-label': ariaLabel,
  'aria-labelledby': ariaLabelledBy,
}: Props) {
  const [open, setOpen] = useState(false)
  const rootRef = useRef<HTMLDivElement>(null)
  const listId = useId()
  const selected = options.find((o) => o.value === value)
  const displayLabel = selected?.label ?? options[0]?.label ?? ''

  useEffect(() => {
    if (!open) return
    function onDoc(e: MouseEvent) {
      if (!rootRef.current?.contains(e.target as Node)) setOpen(false)
    }
    function onKey(e: KeyboardEvent) {
      if (e.key === 'Escape') setOpen(false)
    }
    document.addEventListener('mousedown', onDoc)
    document.addEventListener('keydown', onKey)
    return () => {
      document.removeEventListener('mousedown', onDoc)
      document.removeEventListener('keydown', onKey)
    }
  }, [open])

  return (
    <div className={`li-nice-select ${open ? 'is-open' : ''}`} ref={rootRef}>
      <button
        type="button"
        id={id}
        className="li-nice-select-trigger"
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-controls={listId}
        aria-label={ariaLabelledBy ? undefined : ariaLabel}
        aria-labelledby={ariaLabelledBy}
        disabled={disabled}
        onClick={() => !disabled && setOpen((o) => !o)}
      >
        <span className="li-nice-select-value">{displayLabel}</span>
        <span className="li-nice-select-chevron" aria-hidden />
      </button>
      {open ? (
        <ul id={listId} className="li-nice-select-menu" role="listbox">
          {options.map((o) => (
            <li key={o.value} role="presentation">
              <button
                type="button"
                role="option"
                className={`li-nice-select-option ${o.value === value ? 'is-selected' : ''}`}
                aria-selected={o.value === value}
                onClick={() => {
                  onChange(o.value)
                  setOpen(false)
                }}
              >
                <span className="li-nice-select-option-label">{o.label}</span>
              </button>
            </li>
          ))}
        </ul>
      ) : null}
    </div>
  )
}
