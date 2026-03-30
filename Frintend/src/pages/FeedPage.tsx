import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  addPostComment,
  createPost,
  getFeed,
  getPostComments,
  getPublicProfilesByUsers,
  type Post,
  type PostComment,
} from '../lib/api'
import { getDisplayName, getEmail, getUserId, initialsFromLabel } from '../lib/auth'
import { useI18n } from '../lib/i18n'
import './feed.css'

function formatFeedTime(iso: string): string {
  try {
    const d = new Date(iso)
    return d.toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })
  } catch {
    return iso
  }
}

function formatCommentTime(iso: string): string {
  try {
    return new Date(iso).toLocaleString(undefined, { dateStyle: 'short', timeStyle: 'short' })
  } catch {
    return ''
  }
}

export function FeedPage() {
  const { t, locale } = useI18n()
  const [items, setItems] = useState<Post[]>([])
  const [text, setText] = useState('')
  const [names, setNames] = useState<Record<number, string>>({})
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)
  const [loading, setLoading] = useState(true)

  const [openComments, setOpenComments] = useState<Record<number, boolean>>({})
  const [commentsByPost, setCommentsByPost] = useState<Record<number, PostComment[]>>({})
  const [commentLoadingId, setCommentLoadingId] = useState<number | null>(null)
  const [commentDrafts, setCommentDrafts] = useState<Record<number, string>>({})
  const [commentBusy, setCommentBusy] = useState<Record<number, boolean>>({})
  const [commentError, setCommentError] = useState<Record<number, string>>({})

  const meLabel = getDisplayName() ?? getEmail()?.split('@')[0] ?? 'You'
  const meInitials = initialsFromLabel(meLabel)
  const meId = getUserId()

  const mergeNames = useCallback((ids: number[], prev: Record<number, string>) => {
    const unique = [...new Set(ids.filter((x) => Number.isFinite(x) && x > 0))]
    if (!unique.length) return Promise.resolve(prev)
    return getPublicProfilesByUsers(unique).then((profiles) => {
      const next = { ...prev }
      for (const p of profiles) {
        next[p.userId] = p.fullName || `${p.firstName} ${p.lastName}`.trim() || next[p.userId] || ''
      }
      return next
    })
  }, [])

  async function load() {
    setError('')
    setLoading(true)
    try {
      const posts = await getFeed()
      setItems(posts)
      const ids = [...new Set(posts.map((p) => p.userId))]
      if (ids.length) {
        const profiles = await getPublicProfilesByUsers(ids)
        setNames((prev) => {
          const next = { ...prev }
          for (const p of profiles) {
            next[p.userId] =
              p.fullName || `${p.firstName} ${p.lastName}`.trim() || next[p.userId] || t('messages.unnamedMember')
          }
          return next
        })
      }
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load feed.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  async function post() {
    if (!text.trim()) return
    setBusy(true)
    setError('')
    try {
      await createPost(text.trim())
      setText('')
      await load()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to post.')
    } finally {
      setBusy(false)
    }
  }

  async function ensureCommentsLoaded(postId: number) {
    if (commentsByPost[postId]) return
    setCommentLoadingId(postId)
    setCommentError((c) => ({ ...c, [postId]: '' }))
    try {
      const list = await getPostComments(postId)
      setCommentsByPost((c) => ({ ...c, [postId]: list }))
      const authorIds = [...new Set(list.map((x) => x.userId))]
      if (authorIds.length) {
        const next = await mergeNames(authorIds, names)
        setNames(next)
      }
    } catch (e) {
      setCommentError((c) => ({
        ...c,
        [postId]: e instanceof Error ? e.message : t('feed.loadCommentsError'),
      }))
    } finally {
      setCommentLoadingId(null)
    }
  }

  function toggleComments(postId: number) {
    setOpenComments((o) => {
      const next = !o[postId]
      if (next) void ensureCommentsLoaded(postId)
      return { ...o, [postId]: next }
    })
  }

  async function submitComment(postId: number) {
    const raw = (commentDrafts[postId] ?? '').trim()
    if (!raw) return
    setCommentBusy((b) => ({ ...b, [postId]: true }))
    setCommentError((c) => ({ ...c, [postId]: '' }))
    try {
      const row = await addPostComment(postId, raw)
      setCommentsByPost((c) => ({
        ...c,
        [postId]: [...(c[postId] ?? []), row],
      }))
      setCommentDrafts((d) => ({ ...d, [postId]: '' }))
      const next = await mergeNames([row.userId], names)
      setNames(next)
    } catch (e) {
      setCommentError((c) => ({
        ...c,
        [postId]: e instanceof Error ? e.message : t('feed.commentError'),
      }))
    } finally {
      setCommentBusy((b) => ({ ...b, [postId]: false }))
    }
  }

  function commentCountLabel(n: number): string {
    if (locale === 'en') {
      return n === 1
        ? t('feed.commentCount').replace('{{n}}', String(n))
        : t('feed.commentCount_plural').replace('{{n}}', String(n))
    }
    return t('feed.commentCount_plural').replace('{{n}}', String(n))
  }

  return (
    <div className="li-grid">
      <aside className="li-panel">
        <h4 className="li-side-title">{t('feed.spaceTitle')}</h4>
        <p className="li-side-text">{t('feed.spaceHint')}</p>
      </aside>
      <section className="li-feed-col">
        <div className="li-feed-composer">
          <div className="li-feed-composer-head">
            <h2 className="li-page-title" style={{ marginBottom: 4 }}>
              {t('feed.title')}
            </h2>
            <p className="li-page-sub">{t('feed.sub')}</p>
          </div>
          <p className="li-feed-kicker">{t('feed.startPost')}</p>
          <div className="li-feed-composer-row">
            <div className="li-feed-avatar" aria-hidden>
              {meInitials}
            </div>
            <div className="li-feed-composer-body">
              <textarea
                className="li-textarea"
                value={text}
                onChange={(e) => setText(e.target.value)}
                placeholder={`Hi ${meLabel.split(/\s+/)[0] || 'there'}, share an update, question, or win…`}
                rows={4}
                aria-label="Post content"
              />
              {error ? <p style={{ color: 'crimson', margin: 0, fontSize: 13 }}>{error}</p> : null}
              <div className="li-feed-composer-meta">
                <p className="li-feed-hint">{t('feed.postVisible')}</p>
                <div className="li-btn-row" style={{ marginTop: 0 }}>
                  <button className="li-btn primary" onClick={post} disabled={busy || !text.trim()} type="button">
                    {busy ? t('feed.posting') : t('feed.post')}
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        {loading ? <div className="li-feed-loading">{t('feed.loading')}</div> : null}

        {!loading && !items.length ? (
          <div className="li-feed-empty">
            <strong>{t('feed.emptyTitle')}</strong>
            {t('feed.emptyHint')}
          </div>
        ) : null}

        {!loading
          ? items.map((p) => {
              const name = names[p.userId] ?? t('messages.unnamedMember')
              const initials = initialsFromLabel(name)
              const nameEl =
                meId != null && p.userId === meId ? (
                  <Link className="li-feed-post-name li-feed-post-name--link" to="/profile">
                    {name}
                  </Link>
                ) : (
                  <Link className="li-feed-post-name li-feed-post-name--link" to={`/people/${p.userId}`}>
                    {name}
                  </Link>
                )
              const nComments = commentsByPost[p.id]?.length
              const commentsOpen = !!openComments[p.id]
              return (
                <article key={p.id} className="li-feed-post">
                  <div className="li-feed-post-header">
                    <div className="li-feed-avatar li-feed-avatar--sm" aria-hidden>
                      {initials}
                    </div>
                    <div className="li-feed-post-who">
                      <p className="li-feed-post-name-wrap">{nameEl}</p>
                      <p className="li-feed-post-meta">
                        <span>{formatFeedTime(p.createdAt)}</span>
                        <span className="li-feed-post-dot" aria-hidden />
                        <span>{t('feed.postLabel')}</span>
                      </p>
                    </div>
                  </div>
                  <div className="li-feed-post-body">{p.content}</div>
                  <div className="li-feed-actions" aria-label="Post actions">
                    <button type="button" className="li-feed-act li-feed-act--muted" disabled title="Coming soon">
                      Like
                    </button>
                    <button
                      type="button"
                      className="li-feed-act li-feed-act--primary"
                      onClick={() => toggleComments(p.id)}
                    >
                      {commentsOpen ? t('feed.hideComments') : t('feed.showComments')}
                      {typeof nComments === 'number' ? ` · ${commentCountLabel(nComments)}` : ''}
                    </button>
                    <button type="button" className="li-feed-act li-feed-act--muted" disabled title="Coming soon">
                      Repost
                    </button>
                  </div>
                  {commentsOpen ? (
                    <div className="li-feed-comments">
                      <p className="li-feed-comments-title">{t('feed.commentsTitle')}</p>
                      {commentLoadingId === p.id ? (
                        <p className="li-feed-comments-hint">{t('feed.loading')}</p>
                      ) : null}
                      {commentError[p.id] ? (
                        <p className="li-feed-comments-err">{commentError[p.id]}</p>
                      ) : null}
                      <ul className="li-feed-comment-list">
                        {(commentsByPost[p.id] ?? []).length === 0 && commentLoadingId !== p.id ? (
                          <li className="li-feed-comment-empty">{t('feed.noComments')}</li>
                        ) : null}
                        {(commentsByPost[p.id] ?? []).map((c) => {
                          const cname = names[c.userId] ?? t('messages.unnamedMember')
                          const cinitials = initialsFromLabel(cname)
                          return (
                            <li key={c.id} className="li-feed-comment-row">
                              <div className="li-feed-comment-avatar" aria-hidden>
                                {cinitials}
                              </div>
                              <div className="li-feed-comment-body">
                                <div className="li-feed-comment-head">
                                  <span className="li-feed-comment-name">{cname}</span>
                                  <span className="li-feed-comment-time">{formatCommentTime(c.createdAt)}</span>
                                </div>
                                <p className="li-feed-comment-text">{c.content}</p>
                              </div>
                            </li>
                          )
                        })}
                      </ul>
                      <div className="li-feed-comment-compose">
                        <textarea
                          className="li-feed-comment-input"
                          rows={2}
                          value={commentDrafts[p.id] ?? ''}
                          onChange={(e) => setCommentDrafts((d) => ({ ...d, [p.id]: e.target.value }))}
                          placeholder={t('feed.commentPlaceholder')}
                        />
                        <button
                          type="button"
                          className="li-btn primary li-feed-comment-btn"
                          disabled={!!commentBusy[p.id] || !(commentDrafts[p.id] ?? '').trim()}
                          onClick={() => void submitComment(p.id)}
                        >
                          {commentBusy[p.id] ? t('feed.commentBusy') : t('feed.commentSubmit')}
                        </button>
                      </div>
                    </div>
                  ) : null}
                </article>
              )
            })
          : null}
      </section>
      <aside className="li-panel">
        <h4 className="li-side-title">{t('feed.newsTitle')}</h4>
        <p className="li-side-text">{t('feed.newsHint')}</p>
      </aside>
    </div>
  )
}
