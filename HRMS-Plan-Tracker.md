# EWXP Plan Tracker — Enterprise Workforce Experience Platform

> Agent implementation guide: [`agent.md`](agent.md) · Cursor skill: [`.cursor/skills/ewxp-fullstack/`](.cursor/skills/ewxp-fullstack/SKILL.md)

## Product Vision
AI-Powered Enterprise Workforce Experience Platform (EWXP) — going beyond traditional HRMS record-keeping to deliver intelligent employee experiences, AI-driven automation, and predictive workforce analytics.

---

## Phase 1: Foundation (Current — Implemented)

### Core HR Modules
- [x] Employee Management (CRUD, soft-delete, status tracking)
- [x] Department / Organization Structure
- [x] Leave Management (request, approve/reject, balances)
- [x] Attendance Tracking (daily logs, corrections, edit/delete — edit/delete restricted to Admin/HRManager/Manager)
- [x] Performance Management (reviews, cycles, scores)
- [x] Payroll Setup (structures, salary components)
- [x] Recruitment (candidates, job requisitions, interviews)

### Platform
- [x] JWT Authentication & Role-based Authorization
- [x] Admin User Management (create users, assign roles)
- [x] Role-based Menu & Route Protection (**6 roles** incl. Candidate) — PayrollStaff now has access to Employees & Attendance routes
- [x] Premium UI Theme (Ant Design, purple, dark sidebar)
- [x] **Currency: INR (₹)** — payroll, expense, assets, recruitment, offers ( `en-IN` formatting)
- [x] Public Career Portal (`/careers`) — see Phase 5 partial below + Phase 1 APIs
- [x] Reports Dashboard with CSV export
- [x] Error Logging (localStorage, download/clear)
- [x] Demo Data Seeder (10 employees, 6 workflows seeded)

### Testing
- [x] 9 Unit Tests (EmployeeService, DepartmentService, LeaveService)
- [x] Backend builds with 0 errors
- [x] Frontend TypeScript with 0 errors
- [x] All API endpoints tested

---

## Phase 2: AI & Experience (✅ Complete)

### AI Features
- [x] **AI HR Copilot** — natural language chat interface ✅
  - [x] Employee: "Meri leave balance" → shows leave balances with progress
  - [x] Manager: "Meri team ka attendance" → shows team attendance today
  - [x] CEO: "Company headcount" → shows employee/department counts
  - [x] Any user: "About me" → shows personal profile summary
  - [x] Hinglish support: "Meri team kaun kaun hai?" → lists team members
  - [x] Policy Search — basic keyword-based policy lookup
  - [x] Help system — type "help" for all commands
  - [x] Action buttons — each response has contextual navigation buttons
  - [x] Suggested prompts — quick-start chips on first load
  - [x] Groq AI fallback — if AI:ApiKey configured, uses Groq (LLaMA 3 70B) for general queries
- [x] **AI Organization Brain** — advanced cross-module queries ✅
  - [x] Team comparison: "Compare my attendance with team average"
  - [x] Best performer: "Top performer in my department"
  - [x] Department jobs: "Open positions in Engineering"
  - [x] Leave trends: "Leave trends this quarter"
- [x] **AI Resume Matching** — Screen candidates with AI ✅
  - [x] Action buttons on candidate rows: "Screen" + "Questions"
  - [x] Modal with job description + resume input → AI analysis
  - [x] Uses existing `POST /api/ai/screen` backend endpoint
- [x] **AI Interview Questions** — Generate role-specific questions ✅
  - [x] Modal with role + candidate profile → AI-generated questions
  - [x] Uses existing `POST /api/ai/interview-questions` backend endpoint
- [x] **Predictive HR Analytics** ✅
  - [x] Attrition Risk prediction with reasons & suggested actions
  - [x] Hiring forecast with position-level urgency tracking
  - [x] Burnout detection with indicators & interventions
  - [x] Dashboard overview with summary cards + actionable insights

### Employee Experience
- [x] **Digital Employee Twin** — consolidated profile with tabs ✅
  - [x] Overview tab — all employee details + emergency contact
  - [x] Leave tab — balances with progress bars + statistics cards
  - [x] Attendance tab — monthly stats (present/absent/late) + records table
  - [x] Performance tab — reviews table with expandable score breakdowns
  - [x] **Employee Timeline** — visual journey from joining ✅
    - [x] Joining event marker
    - [x] Performance review milestones with scores
    - [x] Approved leave history
    - [x] Color-coded event types (green/blue/purple)
- [x] **Interactive Organization Chart** — tree view with department hierarchy ✅
  - [x] Each department as expandable node with employee count badge
  - [x] Department head highlighted with crown icon + "Head" tag
  - [x] Click employee → navigate to profile
  - [x] 10 department color coding
- [x] Organization Digital Twin — real-time org structure via API

### No-Code Platform
- [x] **Form Builder** — create dynamic forms without code ✅
  - [x] 8 field types (text, number, email, textarea, date, select, checkbox, phone)
  - [x] Visual form designer with drag-to-reorder
  - [x] Field properties editor (label, required, placeholder, options)
  - [x] Form preview with live submission
  - [x] Form submissions viewer (table per form)
  - [x] Save/edit/delete form definitions
- [x] **Report Builder** — business users create custom reports ✅
  - [x] 6 data sources (Employee, Leave, Attendance, Performance, Department, Recruitment)
  - [x] Column selector with select-all
  - [x] Run reports and view results in table
  - [x] Save/edit/delete report definitions
  - [x] Dynamic column-based table rendering
- [x] **Workflow Designer** — drag-and-drop approval workflows ✅
  - [x] Visual step builder with role assignment
  - [x] 4 step types (Approve, Review, Notify, Submit)
  - [x] Step reordering with arrow connectors
  - [x] Start workflow instances
  - [x] Running instances tracker
- [x] **Custom Fields** — add fields to any module ✅
  - [x] 6 module support (Employee, Leave, Attendance, Performance, Payroll, Recruitment)
  - [x] 6 field types (Text, Number, Date, Select, Boolean, TextArea)
  - [x] Module-tabbed management UI
  - [x] Save/update/delete custom fields
  - [x] CRUD API for custom field values

---

## Phase 3: Workflows & Social (✅ Complete)

### New Workflows
- [x] **Recruitment Full Pipeline** ✅
  - [x] Hiring Request submission with budget/headcount/justification
  - [x] 3-stage approval workflow (Dept → HR → Budget)
  - [x] Auto-creates JobRequisition on full approval
  - [x] Offer management (Draft → Approval → Accept/Reject)
  - [x] Background Check initiation & status tracking
  - [x] Onboarding tasks (add/toggle/delete)
  - [x] Candidate status progression (New → Screening → Interviewed → Offered → Hired/Rejected)
  - [x] Dashboard with pipeline metrics & funnel chart
- [x] **Expense Workflow** ✅
  - [x] Employee submits expense report with line items
  - [x] Admin approves/rejects with notes
  - [x] Multi-item expenses with categories (Travel, Food, Office, Equipment, Other)
  - [x] Status tracking: Pending → Approved/Rejected → Paid
- [x] **Promotion Workflow** ✅ Manager → HRBP → Dept Head → CEO → Payroll Update
  - [x] 4-stage approval chain with notes per stage
  - [x] Auto-updates employee position on final approval
  - [x] PayrollUpdated flag
- [x] **Transfer Workflow** ✅ Manager → HR → Department → IT → Payroll → Employee
  - [x] 6-stage approval chain with notes per stage
  - [x] Employee acceptance step
  - [x] Auto-updates employee department + position on completion

### Social HR
- [x] **Company Feed** ✅ — posts, recognition, announcements, likes, comments
- [x] **Knowledge Hub / Discussions** ✅ — threads, replies, categories, pinned, views
- [x] **Birthday & Anniversary widget** ✅ — dashboard celebrations card with countdown
- [x] **Polls & Surveys** ✅ — create, vote, results, multi/single choice, expiration

### Skills & Talent
- [x] **Skills Graph** ✅ — 14 seeded skills (Technical/Soft/Domain/Language), employee assignments with proficiency 1-5
- [x] **Talent Pools** ✅ — create pools, add/remove candidates
- [x] **Skill Gap Analysis** ✅ — per-employee gap analysis against position skill requirements, org-wide readiness view, training suggestions, promotion readiness check in Career Workflow
- [x] **Position Skill Requirements** ✅ — admin defines required skills + min proficiency per position title; used for gap analysis and promotion readiness

---

## Phase 4: Communication & Assets (✅ Complete)

### Asset & IT Management ✅
- [x] **Asset Management** — 6 demo assets (Laptops, Monitor, Phone, Accessory) across 4 categories
- [x] **Asset allocation & return** — allocate to employee, return with automatic status update
- [x] **Maintenance tracking** — repair/upgrade/inspection records with cost tracking
- [x] **Status lifecycle** — Available → Allocated → Maintenance → Available/Retired
- [x] **Dashboard stats** — total/available/allocated/maintenance counters

### Training & Learning ✅
- [x] **Course catalog** — 4 courses across Technical/Leadership/Compliance categories with instructors, capacity, durations
- [x] **Enrollment tracking** — enroll employees, track status (Enrolled → InProgress → Completed), scores
- [x] **Certification management** — 5 certifications (AWS, Azure, PMP, CISSP, PHR) with employee assignments, expiry tracking

### Advanced Notifications ✅
- [x] **In-app notification center** — bell icon in header with unread badge count
- [x] **Notification popover** — list of recent notifications with read/unread, mark all read
- [x] **Auto-notifications** — document uploads trigger notifications for linked employees

### Document Management ✅
- [x] **Document repository** — upload with title, category, file metadata, employee linkage
- [x] **5 seeded documents** — offer letters, contracts, NDA, policy handbook with expiry dates
- [x] **Document expiry tracking** — expiry date column with color-coded tags (red = expired, orange = expiring)

### Compliance ✅
- [x] **Compliance Center** — configurable statutory packs, 5 seeded records (Tax Filing, Safety Inspection, Data Privacy Audit, Training, Data Retention), status tracking (Pending/InProgress/Completed), category breakdown
- [x] **Audit Logging** — full change history with EntityType/EntityId/Details, timestamped, linked to users, 6 seeded audit events (employee create, leave approve, payroll update, promotion, training, config)
- [x] **GDPR / Data Privacy controls** — DataPrivacyLog with consent tracking, Action/DataCategory/Details/IP address, employee linkage

---

## Phase 5: Enterprise Scale & Advanced CX (Partial — In Progress)

### Career Portal — Advanced Candidate Experience
- [x] **AI Job Match %** — semantic resume vs JD scoring with skill breakdown
- [x] **Natural Language Job Search** — Hinglish supported (`POST /api/careers/search`)
- [x] **Explain Job & Resume Review** — AI summaries before apply
- [x] **Quick Apply** — minimal form, duplicate apply prevention
- [x] **17 seeded open jobs** — idempotent seeder across Engineering, HR, Sales, Marketing, Finance
- [x] **Candidate Register / Login** — `Candidate` role JWT (`candidate@demo.com` / `Demo@123`)
- [x] **My Applications tracker** — 6-step pipeline UI synced with HR candidate status
- [x] **Resume persistence** — `ResumeText` + `ScreeningSummary` saved on `CandidateProfile`
- [x] **Auto AI Screening** — apply sets status `Screening` + `MatchScore` (HR sees in Recruitment)
- [x] **Responsive career portal UI** — mobile nav drawer, filter panel, hero layout
- [x] **Login required to apply** — auth gate on Quick Apply; redirects to sign-in/register modal; **no longer auto-opens apply after auth** (user clicks Quick Apply again)
- [x] **AI Match gated behind login** — AI Match score hidden for anonymous users; shows "Sign in" prompt instead
- [x] **Career portal converted to light theme** — matches main HRMS site color scheme (previously dark theme)
- [x] **Full Apply Wizard** — 24-step dynamic form with professional status routing, auto-save, multi-entry item collections (education, experience, projects, etc.), review & submit
- [x] **Candidate Dashboard** — pipeline stages, AI resume score gauges, career advice (recommended roles, skill gaps, learning roadmap, interview readiness), applications table
- [x] **Resume Upload & AI Parse** — PDF/DOCX file upload with text extraction (iText7 + OpenXML), Groq-based structured data extraction, auto-populate profile
- [ ] AI Mock Interview (voice + coding + behavioral)
- [ ] Job Comparison (side-by-side salary, skills, culture)
- [ ] Application Health Score (beyond match %)
- [ ] LinkedIn / GitHub / Portfolio auto-import
- [ ] AI Resume Builder (generate ATS-optimized CV)
- [ ] Job Alerts (email, SMS, WhatsApp, push)
- [ ] Multi-language portal (Hindi, Tamil, etc.)
- [ ] AI Salary Benchmark & Career Roadmap
- [ ] Employee Referral Hub with bonus tracking
- [ ] Accessibility suite (voice nav, high contrast, screen reader)

### RBAC Security Hardening & Rejection Prompts
- [x] **Login required to apply** — `[Authorize(Roles = "Candidate")]` on POST /api/careers/apply (was `[AllowAnonymous]`)
- [x] **Rejection reason prompts** — 5 HR flows now prompt for reason before rejecting:
  - [x] Leave rejection — modal with reason textarea
  - [x] Expense rejection — modal with reason textarea
  - [x] Promotion/Transfer rejection — modal with reason textarea
  - [x] Offer rejection — modal with reason textarea
  - [x] Hiring Request rejection — modal with reason textarea
- [x] **Backend role authorization audit** — fixed 18 controllers:
  - [x] EmployeesController → `[Authorize(Roles = "Administrator,HRManager,Manager")]`
  - [x] DepartmentsController → `[Authorize(Roles = "Administrator,HRManager,Manager")]`
  - [x] PayrollController → `[Authorize(Roles = "Administrator,HRManager,Manager,PayrollStaff")]`
  - [x] PerformanceController → `[Authorize(Roles = "Administrator,HRManager,Manager")]`
  - [x] ComplianceController → `[Authorize(Roles = "Administrator,HRManager,Manager")]`
  - [x] AssetsController → `[Authorize(Roles = "Administrator,HRManager")]`
  - [x] SkillsController → `[Authorize(Roles = "Administrator,HRManager,Manager")]`
  - [x] TrainingController → `[Authorize(Roles = "Administrator,HRManager,Manager")]`
  - [x] ReportsController → `[Authorize(Roles = "Administrator,HRManager,Manager,PayrollStaff")]`
  - [x] PredictiveController → `[Authorize(Roles = "Administrator,HRManager,Manager")]`
  - [x] ReportsBuilderController → `[Authorize(Roles = "Administrator,HRManager,Manager,PayrollStaff")]`
  - [x] WorkflowController → `[Authorize(Roles = "Administrator,HRManager,Manager")]`
  - [x] RecruitmentController → class-level `[Authorize(Roles = "Administrator,HRManager,Manager")]` + `[AllowAnonymous]` on public endpoints
  - [x] SocialController delete → `[Authorize(Roles = "Administrator,HRManager")]`
  - [x] KnowledgeController poll/thread delete → `[Authorize(Roles = "Administrator,HRManager")]`
  - [x] LeaveController approve → `[Authorize(Roles = "Administrator,HRManager,Manager")]`
  - [x] ExpenseController approve → `[Authorize(Roles = "Administrator,HRManager,Manager")]`

### Platform & Currency
- [x] **INR default currency** — offers, payroll display, expense, assets, recruitment budgets
- [ ] Multi-language UI
- [ ] Multi-currency (beyond INR-first)

### Enterprise HR
- [ ] Payroll Processing Engine (actual computation, payslips, tax filing)
- [ ] Benefits Administration (health insurance, retirement, enrollment)
- [ ] Learning Management System (LMS) with course authoring
- [ ] Grievance Management
- [ ] Loan & Advances Management
- [ ] Shift Scheduling & Workforce Planning
- [ ] Contractor & Freelancer Management
- [ ] Employee Super App (attendance, leave, payroll, chat, AI, learning, expense, travel, recognition, survey, community, QR, visitor, assets)
- [ ] Mobile Native Apps (iOS / Android)
- [ ] Multi-language & Multi-currency (INR done; other currencies & locales pending)
- [ ] Third-party integrations (ERP, CRM, payroll systems)

---

## Phase 6: Digital Workplace Platform (🚧 In Progress)

> Vision: One Employee → One Login → Complete Workplace. Consolidate HRMS + Teams/Slack/Outlook into EWXP.

### 5 Pillars
- **People** — HR, Recruitment, Employee, Payroll, Performance, Learning
- **Work** — Meetings, Tasks, Calendar, Approvals, Workflow
- **Communicate** — Announcements, Department Hubs, Smart Inbox, Notifications
- **Intelligence** — AI Copilot, Meeting AI, Analytics
- **Platform** — Admin, Security, Integrations, Audit

### Meetings & Calendar ⭐
- [x] Meeting entity + participants + department calendar link
- [x] Meetings dashboard (today, upcoming, pending invites)
- [x] Create meeting with type, priority, location, online link
- [x] AI agenda generation (with offline fallback)
- [x] Participant conflict detection + duration suggestion
- [x] Meeting → task action items on complete
- [x] Demo seed: 5 meetings across HR/Engineering/Client/Town Hall
- [ ] Full calendar view (month/week grid)
- [ ] Recurrence engine
- [ ] Teams/Google Calendar sync
- [ ] Meeting recording & AI live summary

### Collaboration Hub ⭐
- [x] Department channels (HR, Engineering, Finance, Sales, Marketing, General)
- [x] Create new channels from UI (name, description, type)
- [x] Channel messages + announcements + pinned
- [x] Message edit/delete (author only; admin/HR for delete)
- [x] Responsive split layout (mobile: channel list ↔ chat)
- [ ] Polls in channels, file sharing, tasks in channel

### Smart Inbox & Notifications ⭐
- [x] Enhanced notification model (Category, Source, Priority, Starred)
- [x] Smart Inbox page (Today / Yesterday / Earlier, unread, starred)
- [x] Category tags (Urgent, Reminder, Information, Task, Approval)
- [ ] Notification preferences per channel (Email, SMS, WhatsApp, Push)
- [ ] Delivery to Teams/Slack

### Announcements ⭐
- [x] Dedicated announcements with scope, priority, acknowledgement
- [x] Read tracking + read count
- [ ] Role/branch targeting, attachments, comments

### Tasks & Accountability ⭐
- [x] WorkTask entity linked to meetings
- [x] Tasks page with assign, complete, priority
- [x] Task edit/delete (full CRUD with update DTO)
- [ ] Manager review workflow, automatic reminders

### Approval Center ⭐
- [x] Unified pending view: Leave, Expense, Promotion, Transfer, Recruitment
- [ ] Inline approve/reject from approval center

### Navigation & UX
- [x] EWXP branding (sidebar)
- [x] 5-pillar grouped navigation (`navigation.ts` menuGroups)
- [x] Responsive CSS for workplace modules
- [ ] Role-based department dashboards (HR, Engineering, Finance homepages)
- [ ] AI Daily Brief on login

---

## Current Status Summary

| Phase | Status | Completion |
|---|---|---|---|
| Phase 1: Foundation | ✅ Complete | 22/22 |
| Phase 2: AI & Experience | ✅ Complete | 54/54 |
| Phase 3: Workflows & Social | ✅ Complete | 34/34 |
| Phase 4: Communication & Assets | ✅ Complete | 18/18 |
| Phase 5: Enterprise Scale | 🚧 Partial | 16/30 |
| Phase 6: Digital Workplace | 🚧 In Progress | 20/37 |
| **RBAC Security Hardening** | ✅ Complete | **18/18** |
| **Rejection Reason Prompts** | ✅ Complete | **5/5** |

**Overall: Phases 1–4 — 140/140 (100%) · Phase 5 — 16/30 · Phase 6 — 20/37 · RBAC — 18/18 · Rejection prompts — 5/5**

---

## Key Differentiators vs Competition
- ✅ AI HR Copilot (natural language)
- ✅ Digital Employee Twin
- ✅ No-Code Builder (forms, reports, workflows)
- ✅ Predictive HR Analytics (attrition, hiring, burnout)
- ✅ Organization Digital Twin
- ✅ Social HR
- ✅ Plugin Marketplace
- ✅ Compliance Center
- ✅ Skill Gap Analysis & Career Path
- ✅ Public Career Portal with candidate auth & application tracker (Phase 5 partial)
- ✅ Full Apply Wizard (24-step dynamic form)
- ✅ Candidate Dashboard (pipeline, AI scores, career advice)
- ✅ Resume Upload & AI Parse (PDF/DOCX + Groq)
- ✅ INR-first currency across HRMS UI
- 🚧 Meetings & Calendar (Phase 6)
- ✅ Collaboration & Tasks (Phase 6)
- 🚧 Smart Inbox & Approval Center (Phase 6)

## Client Impressions (Top Features)
⭐ AI HR Copilot
⭐ Dynamic Workflow Designer
⭐ Digital Employee Journey
⭐ Skills Graph & Internal Talent Marketplace
⭐ Skill Gap & Career Path Recommendations
⭐ Predictive HR Analytics
⭐ Organization Digital Twin
⭐ Unified Communication Hub
⭐ Plugin Marketplace
⭐ Form & Report Builder
⭐ Compliance Center
