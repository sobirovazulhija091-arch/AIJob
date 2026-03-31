import { useCallback, useState } from 'react'
import { initialsFromLabel } from '../lib/auth'
import './orgAvatar.css'

function hueFromOrg(id: number, name: string): number {
  let h = id * 47
  const s = name.trim()
  for (let i = 0; i < s.length; i++) h += s.charCodeAt(i) * (i + 13)
  return ((h % 360) + 360) % 360
}

export type OrgAvatarSize = 'sm' | 'md' | 'lg'

type Props = {
  id: number
  name: string
  logoUrl?: string | null
  size?: OrgAvatarSize
  className?: string
  /** Accessible name for the image when showing a URL logo. */
  imgAlt?: string
}

export function OrgAvatar({ id, name, logoUrl, size = 'sm', className = '', imgAlt }: Props) {
  const [imgFailed, setImgFailed] = useState(false)
  const onError = useCallback(() => setImgFailed(true), [])
  const trimmed = logoUrl?.trim()
  const showImg = Boolean(trimmed && !imgFailed)

  const h = hueFromOrg(id, name)
  const h2 = (h + 38) % 360
  const style = showImg
    ? undefined
    : {
        background: `linear-gradient(145deg, hsl(${h} 58% 42%) 0%, hsl(${h2} 52% 32%) 100%)`,
      }

  const sz = size === 'lg' ? 'li-org-avatar--lg' : size === 'md' ? 'li-org-avatar--md' : 'li-org-avatar--sm'

  return (
    <div className={`li-org-avatar ${sz} ${className}`.trim()} style={style} aria-hidden={showImg ? undefined : true}>
      {showImg ? (
        <img
          className="li-org-avatar__img"
          src={trimmed!}
          alt={imgAlt ?? ''}
          onError={onError}
          loading="lazy"
        />
      ) : (
        <span>{initialsFromLabel(name || '?')}</span>
      )}
    </div>
  )
}
