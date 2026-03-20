# AIJob API – Full Step-by-Step Testing Guide

**Base URL:** `http://localhost:5076`  
**Swagger:** `http://localhost:5076/swagger`

---

## Seed Users (created on first run)

| Role         | Email                     | Password         |
|--------------|---------------------------|------------------|
| Admin        | `admin@example.com`       | `Admin123!`      |
| Candidate    | `candidate@example.com`   | `Candidate123!`  |
| Organization | `organization@example.com`| `Organization123!` |

**Token usage:** After login, copy `Token` → Swagger **Authorize** → paste `Bearer {token}`

---

# PART 1: AUTH

## Step 1.1: Register (new user)

```
POST /api/Auth/register
Content-Type: application/json
```

```json
{
  "fullName": "New User",
  "email": "newuser@example.com",
  "password": "User123!",
  "phoneNumber": "+9999999999",
  "role": 0
}
```
*role: 0=Candidate, 1=Organization, 2=Admin*

---

## Step 1.2: Login as Admin

```
POST /api/Auth/login
Content-Type: application/json
```

```json
{
  "email": "admin@example.com",
  "password": "Admin123!"
}
```

---

## Step 1.3: Login as Organization

```
POST /api/Auth/login
Content-Type: application/json
```

```json
{
  "email": "organization@example.com",
  "password": "Organization123!"
}
```

---

## Step 1.4: Login as Candidate

```
POST /api/Auth/login
Content-Type: application/json
```

```json
{
  "email": "candidate@example.com",
  "password": "Candidate123!"
}
```

---

# PART 2: ADMIN SETUP

*Use `Authorization: Bearer {admin-token}` for all steps below.*

## Step 2.1: Create Job Category

```
POST /api/JobCategory
```

```json
{
  "name": "Software Development"
}
```
*Save categoryId (e.g. 1) for jobs.*

---

## Step 2.2: Create Organization

```
POST /api/Organization
```

```json
{
  "name": "Tech Company Inc",
  "description": "We build software",
  "type": "Company",
  "location": "New York"
}
```
*Save organizationId (e.g. 1).*

---

## Step 2.3: Add Organization User to Organization

Get user ID:
```
GET /api/User/by-email?email=organization@example.com
```

Add member:
```
POST /api/OrganizationMember
```

```json
{
  "organizationId": 1,
  "userId": 3,
  "role": "Admin"
}
```

---

## Step 2.4: Create Skill (catalog)

```
POST /api/Skill
```

```json
{
  "name": "C#",
  "description": "C# programming"
}
```
*Save skillId for UserSkill / ProfileSkill.*

---

## Step 2.5: Create Language (catalog)

```
POST /api/Language
```

```json
{
  "name": "English",
  "type": 0
}
```
*type: 0=Natural, 1=Programming. Save languageId for ProfileLanguage.*

---

# PART 3: ORGANIZATION – Add Job

*Login as Organization first, then:*

```
POST /api/Job
Authorization: Bearer {organization-token}
```

```json
{
  "organizationId": 1,
  "title": "Senior Developer",
  "description": "We need a great developer",
  "salaryMin": 50000,
  "salaryMax": 80000,
  "location": "Remote",
  "jobType": 3,
  "experienceLevel": 2,
  "experienceRequired": 3,
  "categoryId": 1
}
```
*jobType: 1=FullTime, 2=PartTime, 3=Remote, 4=Hybrid*  
*experienceLevel: 1=Junior, 2=Middle, 3=Senior*

---

# PART 4: CANDIDATE – Profiles

*Login as Candidate. Get userId from `GET /api/User/paged` (Admin) or JWT `sub` claim. Candidate seed is often userId=2.*

## Step 4.1: Add UserProfile (CV-style)

```
POST /api/UserProfile
Authorization: Bearer {candidate-token}
```

```json
{
  "userId": 2,
  "firstName": "John",
  "lastName": "Doe",
  "aboutMe": "Experienced developer",
  "experienceYears": 5,
  "expectedSalary": 60000,
  "cvFileUrl": null
}
```

---

## Step 4.2: Add Profile (LinkedIn-style)

```
POST /api/Profile
Authorization: Bearer {candidate-token}
```

```json
{
  "userId": 2,
  "firstName": "John",
  "lastName": "Doe",
  "headline": "Senior Developer",
  "about": "I love building software",
  "location": "Tajikistan",
  "photoUrl": null,
  "backgroundPhotoUrl": null,
  "birthDate": "1990-01-15T00:00:00Z"
}
```
*Save profileId for Education, ProfileSkill, ProfileLanguage.*

---

## Step 4.3: Add UserSkill (to User)

```
POST /api/UserSkill
Authorization: Bearer {candidate-token}
```

```json
{
  "userId": 2,
  "skillId": 1,
  "skillName": "C#"
}
```

---

## Step 4.4: Add UserEducation (to User)

```
POST /api/UserEducation
Authorization: Bearer {candidate-token}
```

```json
{
  "userId": 2,
  "institution": "University of Tech",
  "degree": "Bachelor",
  "startYear": 2015,
  "endYear": 2019
}
```

---

## Step 4.5: Add UserExperience (to User)

```
POST /api/UserExperience
Authorization: Bearer {candidate-token}
```

```json
{
  "userId": 2,
  "companyName": "Previous Corp",
  "position": "Developer",
  "startDate": "2019-06-01T00:00:00Z",
  "endDate": "2024-01-01T00:00:00Z"
}
```

---

## Step 4.6: Add Education (to Profile)

*Needs Profile first. Use profileId from Step 4.2.*

```
POST /api/Education
Authorization: Bearer {candidate-token}
```

```json
{
  "profileId": 1,
  "schoolName": "University of Tech",
  "degree": "Bachelor",
  "fieldOfStudy": "Computer Science",
  "startDate": "2015-09-01T00:00:00Z",
  "endDate": "2019-06-01T00:00:00Z",
  "grade": "A"
}
```

---

## Step 4.7: Add ProfileSkill (to Profile)

```
POST /api/ProfileSkill
Authorization: Bearer {candidate-token}
```

```json
{
  "profileId": 1,
  "skillId": 1,
  "endorsementsCount": 0
}
```

---

## Step 4.8: Add ProfileLanguage (to Profile)

```
POST /api/ProfileLanguage
Authorization: Bearer {candidate-token}
```

```json
{
  "profileId": 1,
  "languageId": 1,
  "level": 2
}
```
*level: 0=Beginner, 1=Intermediate, 2=Advanced, 3=Native*

---

# PART 5: JOB APPLICATION

*Candidate applies for a job.*

```
POST /api/JobApplication
Authorization: Bearer {candidate-token}
```

```json
{
  "jobId": 1,
  "userId": 2
}
```

*Organization/Admin – change application status:*
```
PATCH /api/JobApplication/{id}/status
Authorization: Bearer {organization-token}
Content-Type: application/json
Body: 2
```
*2=Accepted, 3=Rejected, 4=Interview, 1=Pending*

---

# PART 6: CONNECTIONS (Network)

*Candidate/User connects with another user.*

## Step 6.1: Send connection request

```
POST /api/Connection/send/{addresseeId}
Authorization: Bearer {token}
```
*addresseeId = user you want to connect with (e.g. 3 for organization user)*

## Step 6.2: Get pending requests (as addressee)

```
GET /api/Connection/pending
Authorization: Bearer {token}
```

## Step 6.3: Accept or decline request

```
PUT /api/Connection/{connectionId}/respond
Authorization: Bearer {token}
```

```json
{
  "status": 1
}
```
*status: 0=Pending, 1=Accepted, 2=Declined*

## Step 6.4: Get my connections

```
GET /api/Connection/my
Authorization: Bearer {token}
```

## Step 6.5: Remove connection

```
DELETE /api/Connection/{connectionId}
Authorization: Bearer {token}
```

---

# PART 7: MESSAGING (Chat)

## Step 7.1: Get or create conversation

```
POST /api/Conversation
Authorization: Bearer {token}
```

```json
{
  "otherUserId": 3
}
```
*Returns conversation. Save conversationId.*

## Step 7.2: Send message

```
POST /api/Message
Authorization: Bearer {token}
```

```json
{
  "conversationId": 1,
  "content": "Hello!"
}
```

## Step 7.3: Get messages in conversation

```
GET /api/Message/by-conversation/{conversationId}
Authorization: Bearer {token}
```

## Step 7.4: Get my conversations

```
GET /api/Conversation
Authorization: Bearer {token}
```

---

# PART 8: POSTS / FEED

## Step 8.1: Create post

```
POST /api/Post
Authorization: Bearer {token}
```

```json
{
  "content": "Excited to start my new job!",
  "imageUrl": null
}
```

## Step 8.2: Get feed (connections + your posts)

```
GET /api/Post/feed
Authorization: Bearer {token}
```

## Step 8.3: Get all posts

```
GET /api/Post
```

## Step 8.4: Update post

```
PUT /api/Post/{id}
Authorization: Bearer {token}
```

```json
{
  "content": "Updated content",
  "imageUrl": null
}
```

## Step 8.5: Delete post

```
DELETE /api/Post/{id}
Authorization: Bearer {token}
```

---

# PART 9: NOTIFICATIONS

*Admin creates notification for a user. (POST is Admin only.)*

```
POST /api/Notification
Authorization: Bearer {admin-token}
```

```json
{
  "userId": 2,
  "type": 1,
  "title": "Application Update",
  "message": "Your application was reviewed"
}
```
*type: 1=JobMatched, 2=ApplicationAccepted, 3=MessageReceived*

Get user notifications:
```
GET /api/Notification/by-user/{userId}
Authorization: Bearer {token}
```

Get paged notifications:
```
GET /api/Notification/paged?userId=2&page=1&pageSize=10
Authorization: Bearer {token}
```

Mark as read:
```
PATCH /api/Notification/{id}/read
Authorization: Bearer {token}
```

---

# PART 10: FILE UPLOAD

## Step 10.1: Upload CV

```
POST /api/Upload/cv
Authorization: Bearer {token}
Content-Type: multipart/form-data
Body: file = (PDF, DOC, or DOCX, max 5 MB)
```
*Returns URL like `/uploads/cv/xxx.pdf`. Use in UserProfile.cvFileUrl.*

## Step 10.2: Upload Photo

```
POST /api/Upload/photo
Authorization: Bearer {token}
Content-Type: multipart/form-data
Body: file = (JPG, PNG, GIF, WEBP, max 5 MB)
```
*Returns URL like `/uploads/photos/xxx.jpg`. Use in Profile.photoUrl or Post.imageUrl.*

---

# QUICK TEST ORDER

1. **Admin:** Login → JobCategory → Organization → OrganizationMember → Skill → Language  
2. **Organization:** Login → Job  
3. **Candidate:** Login → UserProfile → Profile → UserSkill → UserEducation → UserExperience  
4. **Candidate:** Education → ProfileSkill → ProfileLanguage (need profileId)  
5. **Candidate:** JobApplication  
6. **Connections:** Send request → (other user) Accept → Get my connections  
7. **Messaging:** Create conversation → Send message → Get messages  
8. **Post:** Create post → Get feed  
9. **Upload:** CV / Photo  

---

# USEFUL GET ENDPOINTS (no auth or AllowAnonymous)

- `GET /api/Job` – all jobs  
- `GET /api/Job/{id}` – job by id  
- `GET /api/JobCategory` – all categories  
- `GET /api/Organization` – all organizations  
- `GET /api/Skill` – all skills  
- `GET /api/Language` – all languages  
- `GET /api/Skill/search?name=c` – search skills  
- `GET /api/Language/search?name=eng` – search languages  
