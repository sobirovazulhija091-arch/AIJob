# AIJob – Services Overview

This document describes the purpose of each service in the AIJob API.

---

## Auth

| Service | Purpose |
|---------|---------|
| **AuthService** | Register, login, refresh token, logout, forgot password, reset password. Manages JWT tokens and user authentication. |
| **EmailService** | Sends emails (e.g. password reset tokens). |

---

## Users

| Service | Purpose |
|---------|---------|
| **UserService** | Manage users (Admin only). Create, read, update, delete users. Change user roles. Get by ID, email, or paged list. |
| **UserProfileService** | CV-style profile linked to a user. Store resume data: name, about, salary, CV file URL, etc. One per user. |
| **ProfileService** | LinkedIn-style profile. Headline, about, photo, background, location, birth date. One profile per user. |

---

## User Qualifications (CV / User-level)

| Service | Purpose |
|---------|---------|
| **UserSkillService** | Skills attached to a user (CV-style). Add, remove, list skills for a user. |
| **UserEducationService** | Education records on a user (institution, degree, years). |
| **UserExperienceService** | Work experience on a user (company, position, dates). |

---

## Profile (LinkedIn-style)

| Service | Purpose |
|---------|---------|
| **ProfileSkillService** | Skills on a Profile. Can be endorsed. Linked to Profile and Skill catalog. |
| **ProfileLanguageService** | Languages on a Profile (with level: Beginner, Intermediate, Advanced, Native). |
| **EducationService** | Education records linked to a Profile (school, degree, field, dates, grade). |

---

## Catalogs

| Service | Purpose |
|---------|---------|
| **SkillService** | Skill catalog (C#, Python, etc.). Create, search, manage skills. Used by UserSkill, ProfileSkill, JobSkill. |
| **LanguageService** | Language catalog (English, Russian, etc.) with type (Natural, Programming). Search by name. |
| **JobCategoryService** | Job categories (e.g. Software Development, Marketing). Used when creating jobs. |

---

## Organizations

| Service | Purpose |
|---------|---------|
| **OrganizationService** | Companies/organizations. Create, update, search. Used for job postings. |
| **OrganizationMemberService** | Users who belong to an organization. Link users to organizations with roles (e.g. Admin). |

---

## Jobs

| Service | Purpose |
|---------|---------|
| **JobService** | Job postings. Create, update, delete jobs. Get by organization, search by title, paged list. |
| **JobSkillService** | Skills required for a job. Links jobs to the Skill catalog. |
| **JobApplicationService** | Job applications. Candidates apply; Organization/Admin change status (Pending, Accepted, Rejected, Interview). Get by user or by job. |

---

## Connections (Network)

| Service | Purpose |
|---------|---------|
| **ConnectionService** | User-to-user connections. Send request, accept/decline, list connections, remove. Like LinkedIn connections. |

---

## Messaging

| Service | Purpose |
|---------|---------|
| **ConversationService** | Chat conversations between two users. Get or create conversation by other user ID. List my conversations. |
| **MessageService** | Messages within a conversation. Send message, get messages by conversation. |

---

## Posts & Feed

| Service | Purpose |
|---------|---------|
| **PostService** | Posts (like LinkedIn posts). Create, update, delete. Feed shows posts from you and your accepted connections. |

---

## Endorsements & Recommendations

| Service | Purpose |
|---------|---------|
| **EndorsementService** | Endorse a connection's skill on their Profile (+1). Only connected users can endorse. Updates ProfileSkill.EndorsementsCount. |
| **RecommendationService** | Write a testimonial for a connection. Only connected users can recommend. Author or recipient can delete. |

---

## Notifications

| Service | Purpose |
|---------|---------|
| **NotificationService** | Notifications for users (job matched, application accepted, message received). Create, list by user, mark as read. |

---

## Quick Reference

| Service | Used For |
|---------|----------|
| AuthService | Login, register, tokens |
| UserService | Admin: manage users |
| UserProfileService | CV-style profile |
| ProfileService | LinkedIn-style profile |
| UserSkillService | Skills on user (CV) |
| UserEducationService | Education on user |
| UserExperienceService | Experience on user |
| ProfileSkillService | Skills on Profile (can endorse) |
| ProfileLanguageService | Languages on Profile |
| EducationService | Education on Profile |
| SkillService | Skill catalog |
| LanguageService | Language catalog |
| JobCategoryService | Job categories |
| OrganizationService | Companies |
| OrganizationMemberService | Users in organizations |
| JobService | Job postings |
| JobSkillService | Skills for jobs |
| JobApplicationService | Apply to jobs, change status |
| ConnectionService | Connect with users |
| ConversationService | Chat threads |
| MessageService | Chat messages |
| PostService | Posts and feed |
| EndorsementService | Endorse skills |
| RecommendationService | Write recommendations |
| NotificationService | User notifications |
| EmailService | Send emails |
