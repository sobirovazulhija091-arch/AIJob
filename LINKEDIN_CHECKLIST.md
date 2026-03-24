# LinkedIn-Style Project – Checklist

**Project:** AIJob (FindJob)  
**Status:** API runs OK (tested on http://localhost:5076)  
**Date:** 2 days left until Monday

---

## ✅ DONE (Ready)

| Area | Status | Notes |
|------|--------|-------|
| **Auth** | ✅ | Register, Login, RefreshToken, Logout, ForgotPassword, ResetPassword |
| **Jobs** | ✅ | CRUD, paged, filter, search, by-organization |
| **Job Applications** | ✅ | Apply, status change, by-user, by-job |
| **Job Categories** | ✅ | CRUD |
| **Job Skills** | ✅ | CRUD, skills by job |
| **Organizations** | ✅ | CRUD, paged, search |
| **Organization Members** | ✅ | CRUD, by-organization |
| **User (Admin)** | ✅ | CRUD, paged, change role |
| **UserProfile** | ✅ | CRUD, by-user (CV-style profile) |
| **User Skills** | ✅ | CRUD, skills by user, remove skill |
| **User Education** | ✅ | CRUD, by-user |
| **User Experience** | ✅ | CRUD, by-user |
| **Skills** | ✅ | CRUD, search |
| **Language** | ✅ | CRUD, search by name, Type enum |
| **Profile** | ✅ | CRUD, by-user, one per user |
| **Education** (Profile) | ✅ | CRUD, by-profile |
| **ProfileSkill** | ✅ | CRUD, by-profile, remove skill |
| **ProfileLanguage** | ✅ | CRUD, by-profile |
| **Notifications** | ✅ | CRUD, paged by user, mark as read |
| **Connections** | ✅ | Send request, accept/decline, list, remove |
| **Messaging** | ✅ | Conversation, Message, create chat, get messages |
| **Posts / Feed** | ✅ | Create post, feed (connections + self), CRUD |
| **File upload** | ✅ | CV (PDF, DOC), photo (JPG, PNG) upload endpoints |
| **Swagger** | ✅ | JWT Authorize button |
| **Database** | ✅ | PostgreSQL, migrations applied |

---

## ✅ ENTITIES WITH SERVICE/CONTROLLER (Complete)

All entities in DbContext now have service + controller:

| Entity | Service | Controller | LinkedIn-like feature |
|--------|---------|------------|------------------------|
| **Profile** | ✅ IProfileService | ✅ ProfileController | Rich profile (headline, about, photo, background) |
| **Education** (Profile) | ✅ IEducationService | ✅ EducationController | Profile-linked education, GetByProfileId |
| **ProfileSkill** | ✅ IProfileSkillService | ✅ ProfileSkillController | Skills on Profile, RemoveSkillAsync |
| **ProfileLanguage** | ✅ IProfileLanguageService | ✅ ProfileLanguageController | Languages with level |
| **Language** | ✅ ILanguageService | ✅ LanguageController | Language catalog, SearchByName, Type enum |
| **Connection** | ✅ IConnectionService | ✅ ConnectionController | User-to-user connections |
| **Conversation** | ✅ IConversationService | ✅ ConversationController | Chat conversations |
| **Message** | ✅ IMessageService | ✅ MessageController | Chat messages |
| **Post** | ✅ IPostService | ✅ PostController | Posts, feed |

*Note: RefreshToken is managed by AuthService (no separate controller).*

---

## 🤖 AI PART – DONE

| Item | Status | Notes |
|------|--------|-------|
| **Sort jobs for candidates** | ✅ | `GET /api/JobMatching/recommended-jobs/{userId}` – jobs sorted by match |
| **Matching score for candidates** | ✅ | 0–100 score from skills + experience |
| **Sort applicants for companies** | ✅ | `GET /api/JobMatching/recommended-applicants/{jobId}` – applicants sorted by match |
| **APIs for sorted lists** | ✅ | JobMatchingController with paged results |
| **Use AI for matching** | ✅ | `GET /api/JobMatching/match-explanation/{userId}/{jobId}?useAi=true` – Gemini-generated insight |
| **Cache match scores** | ⏸️ | Optional – can be added later with IMemoryCache |

*Rule-based matching (skills + experience) works without AI. Set `useAi=true` for Gemini-generated match explanations.*

---

## IMPLEMENTED (LinkedIn has these)

| Feature | Status | Notes |
|---------|--------|-------|
| **Connections / Network** | ✅ | Connection entity, send/respond/list/remove |
| **Messaging (Chat)** | ✅ | Conversation, Message, MessageService, ConversationService |
| **Posts / Feed** | ✅ | Post entity, feed from connections + self |
| **Recommendations / Endorsements** | ✅ | Endorsement (skill), Recommendation (written) – POST/GET/DELETE |
| **File upload (CV, photo)** | ✅ | POST /api/upload/cv, POST /api/upload/photo |
| **Organization user seed** | ✅ | Seed has Admin, Candidate, Organization user |
| **CORS** | ⚠️ | May be needed if you add a frontend |

---

## 🔧 SMALL FIXES / IMPROVEMENTS

| Item | Status |
|------|--------|
| **Organization seed** | ✅ Done – `organization@example.com` / `Organization123!` added |
| **Profile vs UserProfile** | ✅ Clarified – comments in entities: Profile = future full profile, UserProfile = CV-style (used now) |
| **Duplicate JobService** | Not found – only one JobService.cs exists |

---

## 📋 PRIORITY FOR 2 DAYS

**Higher priority:**
1. ~~Add **Organization** user to seed~~ ✅ Done
2. ~~Implement **Language** service + controller~~ ✅ Done
3. ~~**Profile** + **ProfileSkill** + **ProfileLanguage** + **Education (Profile)**~~ ✅ All done

**Medium priority:**
4. ~~**Messaging**~~ ✅ Done

**Lower priority (if time allows):**
5. ~~Connections~~ ✅ Done
6. ~~File upload for CV/photo~~ ✅ Done
7. CORS for frontend
8. ~~Posts/Feed~~ ✅ Done

---

## 📁 FOLDER STRUCTURE (Current)

```
AIJob/
├── Domain/
│   ├── DTOs/          ✅
│   ├── Entities/      ✅ (some unused)
│   ├── Enums/         ✅
│   ├── Filters/       ✅
│   └── Seeds/         ✅
├── Infrastructure/
│   ├── Data/          ✅ ApplicationDbContext
│   ├── Interfaces/    ✅
│   ├── Migrations/    ✅
│   ├── Responses/     ✅ Response, PagedResult
│   └── Services/      ✅ (Auth, Email + 23 domain services)
├── WebApi/
│   ├── Controllers/   ✅ 24 controllers
│   ├── Middlware/     (RequestTimeMiddleware)
│   └── Program.cs     ✅
└── appsettings.json   ✅
```

---

## ✅ RUN VERIFICATION

- **Build:** ✅ Success
- **Run:** ✅ API starts on http://localhost:5076
- **Swagger:** ✅ http://localhost:5076/swagger
- **Database:** ✅ Migrations applied
- **Seed:** ✅ Admin + Candidate + Organization users created

---

*Use this list to decide what to finish before Monday.*
