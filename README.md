# EWXP — Enterprise Workforce Experience Platform

A full-stack AI-powered HR platform built with **React 19 + TypeScript** (frontend), **.NET 10 Web API** (backend), and **MS SQL Server**.

---

## Quick Start

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/) (local instance with Windows Authentication)

### 1. Backend
```bash
cd backend
dotnet run --urls "http://localhost:5096"
```
Auto-creates `HRMSDb`, runs all 12 migrations, seeds ~50 tables of demo data.

### 2. Frontend
```bash
cd frontend
npm install
npm run dev
```
Opens at `http://localhost:5173`.

---

## Demo Credentials

| Role | Email | Password |
|------|-------|----------|
| **Administrator** | `admin@hrms.com` | `Admin@123` |
| **HR Manager** | `emily@hrms.com` | `Demo@123` |
| **Manager** | `michael@hrms.com` | `Demo@123` |
| **Employee** | `priya@hrms.com` | `Demo@123` |
| **Payroll Staff** | `robert@hrms.com` | `Demo@123` |
| **Candidate** (Career Portal) | `candidate@demo.com` | `Demo@123` |

> **Candidate login** is for the public career portal at `/careers` only (separate from employee HRMS login). You can also register a new candidate account from the portal.

---

## Features by Phase

### Phase 1 — Foundation
- Employee Management (CRUD, soft-delete, status)
- Departments with heads + hierarchy
- Leave Management (request, approve/reject with rejection reason, balances)
- Attendance Tracking (daily logs, corrections, edit/delete — edit/delete restricted to Admin/HRManager/Manager)
- Performance Reviews (cycles, scores, breakdowns)
- Payroll Setup (structures, salary components) — **amounts in INR (₹)**
- Recruitment (candidates, jobs, interviews, offers) — **INR salaries & budgets**
- JWT Auth with **6 roles**, role-based menus + route protection
- **Public Career Portal** (`/careers`) — single-company EWXP portal:
  - **17 open jobs** (Engineering, HR, Sales, Marketing, Finance)
  - AI job match %, natural language search (Hinglish), explain job, resume review
  - Quick apply with **login required** — candidates must register/sign in before applying
  - **Candidate register/login** + **My Applications** pipeline tracker
  - Responsive header, filters (workplace, department, experience, location)
  - Light theme matching main HRMS site (previously dark theme)
- **AI Match gated behind login** — score hidden for anonymous users, shows "Sign in" prompt
- Expense, Assets, Payroll UI — **INR (₹) formatting** via `en-IN` locale
- Reports with CSV export
- Error Logging (localStorage)

### Phase 2 — AI & Experience
- **AI HR Copilot** — natural language chat (English + Hinglish) with 16 intent handlers
- **AI Resume Screening & Interview Questions** — per-candidate
- **Predictive Analytics** — attrition risk, burnout detection, hiring forecast
- **Digital Employee Twin** — 5-tab profile (overview, leave, attendance, performance, timeline)
- **Interactive Org Chart** — color-coded departments, click-to-profile
- **No-Code Form Builder** — 8 field types, visual designer, submissions
- **No-Code Report Builder** — 6 data sources, dynamic columns
- **No-Code Workflow Designer** — visual step builder, role assignment, instances

### Phase 3 — Workflows & Social
- **Social HR Feed** — posts (Update/Recognition/Announcement), likes, comments
- **Expense Workflow** — multi-item reports, categories, approval flow
- **Full Recruitment Pipeline** — Hiring Request → 3-stage approval → auto JobRequisition → Offer → Background Check → Onboarding
- **Promotion Workflow** — 4-stage (Manager → HRBP → Dept Head → CEO), auto-updates position
- **Transfer Workflow** — 6-stage (Manager → HR → Dept → IT → Payroll → Employee)
- **Polls & Surveys** — create, vote (single/multi), expiry, live results
- **Knowledge Hub / Discussions** — threads, replies, categories, pinned, views
- **Birthdays & Anniversaries** — dashboard widget with countdown
- **Skills Graph & Talent Marketplace** — 14 skills across 4 categories, proficiency, talent pools, analytics

### Phase 4 — Communication & Assets ✅ Complete
- **Asset Management** — lifecycle (Available→Allocated→Maintenance→Retired), allocation/return, maintenance records
- **Training & Learning** — course catalog, enrollments, certifications with expiry
- **In-App Notifications** — bell icon with unread badge, popover, mark-read/mark-all-read
- **Document Management** — upload with categories, employee linkage, expiry tracking
- **Compliance Center** — statutory records with status tracking, audit log viewer, GDPR/data privacy logs
- **Tasks** — full CRUD (assign, edit, delete, complete, priority, employee/meeting links)

### Phase 6 — Digital Workplace Platform ✅ In Progress
- **Meetings & Calendar** — create, edit, complete, respond; AI agenda, conflict detection, duration suggestions; meeting → task action items
- **Collaboration Hub** — department channels, channel creation, messages with edit/delete, responsive split layout
- **Smart Inbox** — unified notifications with Today/Yesterday/Earlier grouping, unread/starred, category tags
- **Announcements** — scope, priority, acknowledgement, read tracking
- **Tasks & Accountability** — linked to meetings, full CRUD with assign/complete/priority
- **Approval Center** — unified pending view across Leave, Expense, Promotion, Transfer, Recruitment

### RBAC Security Hardening (Complete ✅)
- **18 backend controllers** updated with proper `[Authorize(Roles = "...")]` attributes matching frontend UI gating
- Previously under-protected endpoints now role-restricted: Employees, Departments, Payroll, Performance, Compliance, Assets, Skills, Training, Reports, Predictive, ReportsBuilder, Workflow
- **RecruitmentController** — class-level auth added (was missing entirely, leaving internal endpoints publicly accessible)
- **Delete endpoints** on Social posts, Polls, and Discussions now restricted to Admin/HR only
- **Leave/Expense approve** now restricted to Admin/HR/Manager (was any authenticated user)
- **Rejection reason prompts** — all HR rejections now require a reason:
  - Leave rejection → modal with reason textarea (sends `reviewNotes`)
  - Expense rejection → modal with reason textarea (sends `notes`)
  - Promotion/Transfer rejection → modal with reason textarea (sends `notes`)
  - Offer rejection → modal with reason textarea (sends `reason`)
  - Hiring Request rejection → modal with reason textarea (sends `notes`)

### Phase 5 — Advanced CX (Partial ✅)
- **Candidate Auth** — register/login on `/careers` with `Candidate` role JWT
- **Application Tracker** — My Applications tab with 6-step pipeline synced to HR recruitment status
- **Resume persistence** — `ResumeText` + AI `ScreeningSummary` stored on apply
- **Auto AI Screening** — apply sets status `Screening` (not `New`) with match score
- **Login required to apply** — candidates can browse jobs anonymously, but must authenticate before applying
  - Clicking "Quick Apply" without a session opens the auth modal
  - After login/register, user clicks Quick Apply again to open form (no auto-open)
  - **AI Match score gated behind login** — hidden for anonymous users, shows "Sign in" prompt instead
  - Removed anonymous application path from both frontend and backend
- **Rejection reason prompts** — all HR rejections now prompt for a reason before confirming
- **RBAC Security Hardening** — all 18 vulnerable backend controllers now properly role-restricted
- 🔮 Still planned: mock interview, job comparison, LinkedIn import, job alerts, multi-language

---

## Role Access Matrix

| Menu | Admin | HR Mgr | Manager | Employee | Payroll | Candidate |
|------|:-----:|:------:|:-------:|:--------:|:-------:|:---------:|
| Dashboard | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| Company Feed | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| AI Copilot | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| Org Chart | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| Analytics | ✅ | ✅ | ✅ | — | — | — |
| Form Builder | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| Report Builder | ✅ | ✅ | ✅ | — | ✅ | — |
| Workflows | ✅ | ✅ | — | — | — | — |
| Custom Fields | ✅ | — | — | — | — | — |
| Employees | ✅ | ✅ | ✅ | — | ✅ | — |
| Departments | ✅ | ✅ | ✅ | — | — | — |
| Leave | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| Attendance | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| Performance | ✅ | ✅ | ✅ | — | — | — |
| Expense | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| Career Workflows | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| Polls | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| Discussions | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| Skills | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| Assets | ✅ | ✅ | — | — | — | — |
| Training | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| Documents | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| Compliance | ✅ | ✅ | ✅ | — | — | — |
| Payroll | ✅ | ✅ | ✅ | — | ✅ | — |
| Recruitment | ✅ | ✅ | ✅ | — | — | — |
| Reports | ✅ | ✅ | ✅ | — | ✅ | — |
| Settings | ✅ | — | — | — | — | — |
| Career Portal | — | — | — | — | — | ✅ |
| Meetings | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| Collaboration | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| Smart Inbox | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| Announcements | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| Tasks | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| Approvals | ✅ | ✅ | ✅ | — | ✅ | — |

---

## Seed Data

| Data | Count |
|------|-------|
| Roles | 6 (+ Candidate portal role) |
| Leave Types | 6 |
| Departments | 5 |
| Employees | 10 |
| Leave Requests | 6 |
| Attendance Logs | ~180 |
| Review Cycles | 2 |
| Performance Reviews | 6 |
| Payroll Structure | 1 with 8 components |
| Job Requisitions | 17 (open, multi-department) |
| Candidates | 5+ (Career Portal applies add more) |
| Skills | 14 (4 categories) |
| Courses | 4 |
| Certifications | 5 |
| Assets | 6 |
| Documents | 5 |
| Compliance Records | 5 |
| Audit Log Events | 6 |
| Social Posts | 5 with likes/comments |
| Polls | 3 |
| Discussion Threads | 4 |

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | React 19, TypeScript, Ant Design 5, Zustand, React Router, Axios, Vite |
| Backend | .NET 10 Web API, Entity Framework Core 10, JWT Auth, Swagger |
| Database | MS SQL Server (local, Windows Auth) |
| Testing | xUnit, EF Core InMemory (9 tests) |
| AI | Groq AI (LLaMA 3 70B) via OpenAI-compatible API (optional, config in appsettings.json) |

---

## AI Features (Optional)

Set `AI:ApiKey` in `backend/appsettings.json` to enable (uses Groq API with LLaMA 3 70B):
- AI HR Copilot with Groq fallback for general queries
- AI Resume Screening & Interview Questions
- Predictive HR Analytics

Without the key, AI features work in offline mode with rule-based responses.

---

## Key Files

| File | Purpose |
|------|---------|
| `backend/Program.cs` | DI, JWT, CORS, auto-migration + seed on startup |
| `backend/Data/DatabaseSeeder.cs` | All demo data seeding |
| `backend/Data/HRMSDbContext.cs` | EF Core context (32+ DbSets) |
| `backend/Controllers/ComplianceController.cs` | Compliance, Audit Log, Data Privacy endpoints |
| `frontend/src/App.tsx` | Routes, theme, role-based route protection (29+ routes) |
| `frontend/src/layouts/DashboardLayout.tsx` | Sidebar with 25+ role-filtered menu items, notification bell |
| `frontend/src/pages/CompliancePage.tsx` | Compliance Center dashboard + 3-tab UI |
| `frontend/src/pages/careers/CandidateJobPortal.tsx` | Public career portal — jobs, AI match, apply, candidate auth, application tracker |
| `frontend/src/utils/currency.ts` | INR (₹) formatting helpers for all money displays |
| `backend/Controllers/CareersController.cs` | Public career API — jobs, match, apply, register, login, applications |
| `backend/Services/CareerPortalService.cs` | Career portal logic, candidate auth, application pipeline DTOs |
| `HRMS-Plan-Tracker.md` | Full milestone tracking (169 tasks across 6 phases) |
| `agent.md` | Full-stack AI agent guide — skills, do's/don'ts, runbooks, conventions |
| `.cursor/skills/ewxp-fullstack/` | Cursor skill for EWXP full-stack development |

---

## Project Status

**Phases 1–4 complete — 138/138 tasks (100%)**

**RBAC Security Hardening — 18/18 controllers fixed (100%)** — all backend endpoints now match frontend role-based UI gating.

**Rejection Reason Prompts — 5/5 flows implemented (100%)** — Leave, Expense, Promotion/Transfer, Offer, Hiring Request rejections now require a reason.

**Phase 5 (Advanced CX): partially implemented** — candidate login, login-required-to-apply, application tracker, resume persistence, auto screening, INR currency.

**Phase 6 (Digital Workplace): 20/37 workplace items complete** — meetings, collaboration hub, smart inbox, announcements, tasks, approval center. See [HRMS-Plan-Tracker.md](HRMS-Plan-Tracker.md) for full breakdown.

Remaining Phase 5: mock interview, job comparison, LinkedIn/GitHub import, job alerts, multi-language portal, employee referral hub, mobile apps, benefits admin, LMS authoring.
