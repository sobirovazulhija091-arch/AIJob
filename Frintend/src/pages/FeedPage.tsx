import { useEffect, useState } from 'react'
import { createPost, getFeed, getPublicProfilesByUsers, type Post } from '../lib/api'
import { getDisplayName, getEmail, initialsFromLabel } from '../lib/auth'
import './feed.css'

function formatFeedTime(iso: string): string {
  try {
    const d = new Date(iso)
    return d.toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })
  } catch {
    return iso
  }
}

export function FeedPage() {
  const [items, setItems] = useState<Post[]>([])
  const [text, setText] = useState('')
  const [names, setNames] = useState<Record<number, string>>({})
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)
  const [loading, setLoading] = useState(true)

  const meLabel = getDisplayName() ?? getEmail()?.split('@')[0] ?? 'You'
  const meInitials = initialsFromLabel(meLabel)

  async function load() {
    setError('')
    setLoading(true)
    try {
      const posts = await getFeed()
      setItems(posts)
      const ids = [...new Set(posts.map((p) => p.userId))]
      if (ids.length) {
        const profiles = await getPublicProfilesByUsers(ids)
        const map: Record<number, string> = {}
        for (const p of profiles)
          map[p.userId] = p.fullName || `${p.firstName} ${p.lastName}`.trim() || `User ${p.userId}`
        setNames(map)
      } else {
        setNames({})
      }
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load feed.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void load()
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

  return (
    <div className="li-grid">
      <aside className="li-panel">
        <h4 className="li-side-title">Your space</h4>
        <p className="li-side-text">Share updates, wins, and ideas with your network. Regular posts help you stay visible to recruiters and connections.</p>
      </aside>
      <section className="li-feed-col">
        <div className="li-feed-composer">
          <div className="li-feed-composer-head">
            <h2 className="li-page-title" style={{ marginBottom: 4 }}>
              Feed
            </h2>
            <p className="li-page-sub">What is happening in your professional network?</p>
          </div>
          <p className="li-feed-kicker">Start a post</p>
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
                <p className="li-feed-hint">Visible to your network after you post.</p>
                <div className="li-btn-row" style={{ marginTop: 0 }}>
                  <button className="li-btn primary" onClick={post} disabled={busy || !text.trim()} type="button">
                    {busy ? 'Posting…' : 'Post'}
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        {loading ? <div className="li-feed-loading">Loading feed…</div> : null}

        {!loading && !items.length ? (
          <div className="li-feed-empty">
            <strong>No posts yet</strong>
            Be the first to share something above. Short updates work great—questions, links, or milestones.
          </div>
        ) : null}

        {!loading
          ? items.map((p) => {
              const name = names[p.userId] ?? `User ${p.userId}`
              const initials = initialsFromLabel(name)
              return (
                <article key={p.id} className="li-feed-post">
                  <div className="li-feed-post-header">
                    <div className="li-feed-avatar li-feed-avatar--sm" aria-hidden>
                      {initials}
                    </div>
                    <div className="li-feed-post-who">
                      <p className="li-feed-post-name">{name}</p>
                      <p className="li-feed-post-meta">
                        <span>{formatFeedTime(p.createdAt)}</span>
                        <span className="li-feed-post-dot" aria-hidden />
                        <span>Post</span>
                      </p>
                    </div>
                  </div>
                  <div className="li-feed-post-body">{p.content}</div>
                  <div className="li-feed-actions" aria-label="Reactions (coming soon)">
                    <button type="button" className="li-feed-act" disabled title="Coming soon">
                      Like
                    </button>
                    <button type="button" className="li-feed-act" disabled title="Coming soon">
                      Comment
                    </button>
                    <button type="button" className="li-feed-act" disabled title="Coming soon">
                      Repost
                    </button>
                  </div>
                </article>
              )
            })
          : null}
      </section>
      <aside className="li-panel">
        <h4 className="li-side-title">News &amp; tips</h4>
        <p className="li-side-text">
          Stay visible with consistent, helpful posts. Engage on other people&apos;s updates to grow reach—reactions and threads are on the roadmap.
        </p>
      </aside>
    </div>
  )
}
