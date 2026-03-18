## Project: Build a LinkedIn‑Style Web App

### 1. Choose Tech Stack

- **Frontend**
  - **React + TypeScript** (or Next.js if you want SSR/SEO)
  - UI: **Tailwind CSS** or **Material UI**
- **Backend**
  - **Node.js + Express** (or NestJS)
- **Database**
  - **PostgreSQL** (via **Prisma** or **TypeORM**)
- **Auth**
  - **JWT** (access + refresh tokens)
  - Password hashing with **bcrypt**

### 2. Core Domain Entities (Database Models)

At minimum, design these entities:

- **User**
  - `id`, `email`, `passwordHash`, `role` (user/admin), `createdAt`, `updatedAt`
- **Profile**
  - `id`, `userId`, `firstName`, `lastName`, `headline`, `about`, `location`, `photoUrl`, `backgroundPhotoUrl`
- **Experience**
  - `id`, `profileId`, `companyName`, `title`, `startDate`, `endDate`, `isCurrent`, `description`
- **Education**
  - `id`, `profileId`, `schoolName`, `degree`, `fieldOfStudy`, `startDate`, `endDate`, `grade`
- **Skill**
  - `id`, `name`
- **ProfileSkill**
  - `profileId`, `skillId`, `endorsementsCount`
- **Company**
  - `id`, `name`, `logoUrl`, `headline`, `industry`, `location`, `about`
- **Job**
  - `id`, `companyId`, `title`, `description`, `location`, `employmentType`, `seniority`, `createdAt`
- **Connection**
  - `id`, `requesterId`, `addresseeId`, `status` (`PENDING`, `ACCEPTED`, `REJECTED`, `BLOCKED`), `createdAt`
- **Post**
  - `id`, `authorId`, `content`, `mediaUrl`, `createdAt`, `updatedAt`, `visibility`
- **PostReaction**
  - `id`, `postId`, `userId`, `type` (`LIKE`, `CELEBRATE`, etc.), `createdAt`
- **Comment**
  - `id`, `postId`, `authorId`, `content`, `createdAt`, `parentCommentId?`
- **MessageThread**
  - `id`, `createdAt`
- **ThreadParticipant**
  - `threadId`, `userId`, `joinedAt`
- **Message**
  - `id`, `threadId`, `senderId`, `content`, `createdAt`, `isRead`
- **Notification**
  - `id`, `userId`, `type`, `payloadJson`, `isRead`, `createdAt`

### 3. Main Features to Implement (Step by Step)

#### 3.1 Auth & Users

- **Backend endpoints**
  - `POST /api/auth/register` – create `User` + `Profile`
  - `POST /api/auth/login` – email + password → JWT tokens
  - `POST /api/auth/refresh` – refresh token → new access token
  - `POST /api/auth/logout`
- **Frontend flows**
  - Register page (like “Join now”)
  - Login page (like “Sign in”)

#### 3.2 Profile Page

- **Backend**
  - `GET /api/profiles/:userId`
  - `PUT /api/profiles/:userId` – update profile
  - `POST /api/profiles/:userId/experience`
  - `POST /api/profiles/:userId/education`
  - `POST /api/profiles/:userId/skills`
- **Frontend**
  - Public profile view (name, headline, experience, education, skills)
  - Edit profile form

#### 3.3 Connections (Network)

- **Backend**
  - `POST /api/connections/request` – send request
  - `POST /api/connections/:id/accept`
  - `POST /api/connections/:id/reject`
  - `GET /api/connections/mine` – my connections + status
- **Frontend**
  - “Connect” button on profile page
  - List of my connections
  - List of pending requests

#### 3.4 Feed (Home Page)

- **Backend**
  - `GET /api/feed` – posts from user + connections (pagination)
  - `POST /api/posts` – create post
  - `POST /api/posts/:id/reactions`
  - `POST /api/posts/:id/comments`
- **Frontend**
  - Feed page with:
    - Create post form (text + image)
    - List posts, reactions count, comments

#### 3.5 Messaging

- **Backend**
  - `GET /api/threads` – list threads
  - `POST /api/threads` – start new thread
  - `GET /api/threads/:id/messages`
  - `POST /api/threads/:id/messages`
- **Frontend**
  - Simple inbox:
    - Left: list of threads
    - Right: messages in selected thread + input

#### 3.6 Jobs

- **Backend**
  - `GET /api/jobs` – search/filter jobs
  - `POST /api/jobs` – create job (as company/admin)
  - `GET /api/companies/:id`
- **Frontend**
  - Jobs listing page (search, filters)
  - Job details page
  - Simple job post form (for admin / company users)

### 4. Backend Architecture (How It Works)

- **Layers**
  - **Routes / Controllers** – handle HTTP requests, validation, map to services
  - **Services** – business logic (create connection, accept request, create post, etc.)
  - **Repositories** – DB access (for each entity)
  - **Middlewares**
    - `authMiddleware` – read JWT, attach `req.user`
    - `errorHandler` – send consistent error responses
    - `rateLimiter` (later) for security
- **Typical method flow (example: send connection request)**
  1. `POST /api/connections/request`
  2. Validate body: `targetUserId`
  3. Check: not already connected, not pending, not self
  4. Create `Connection` with `status = PENDING`
  5. Create `Notification` for target user
  6. Return new connection info

### 5. Frontend Architecture (How It Works)

- **Pages**
  - `/login`, `/register`
  - `/feed`
  - `/profile/:id`
  - `/network`
  - `/jobs`
  - `/messages`
- **State management**
  - **React Query** (or Redux Toolkit) for:
    - auth user
    - feed, profile, connections, messages
- **Components (examples)**
  - `Navbar`, `Sidebar`, `Feed`, `PostCard`, `ProfileHeader`, `ProfileAbout`, `ExperienceList`, `ConnectionsList`, `ChatSidebar`, `ChatWindow`

### 6. Non‑Functional Requirements

- **Security**
  - Hash passwords, never store plain text
  - Validate all inputs (backend and frontend)
  - Use `HTTPS` in production
- **Performance**
  - Pagination everywhere (feed, jobs, messages)
  - Index DB columns used in `WHERE`/`JOIN` (`userId`, `createdAt`, etc.)
- **Scalability (future)**
  - Move heavy tasks (emails, notifications) to background jobs
  - Consider microservices later (auth, feed, messaging, jobs)

### 7. Suggested Implementation Order

1. Set up repo + basic Express server + DB connection
2. Implement Auth (register / login / JWT)
3. Implement Profile (view + edit)
4. Implement Connections
5. Implement Feed (posts + comments + reactions)
6. Implement Messaging
7. Implement Jobs + Companies
8. Add Notifications, better UI, and polish

