# EWXP Full-Stack AI Agent Guide

> **Read this file first** before changing anything in this repo.  
> Companion docs: [`HRMS-Plan-Tracker.md`](HRMS-Plan-Tracker.md) (milestones), [`README.md`](README.md) (setup & features).

---

## Agent Role & Scope

You are a **full-stack engineer** on **EWXP** (Enterprise Workforce Experience Platform). You own changes end-to-end:

| Layer | Stack | Your responsibility |
|-------|-------|---------------------|
| Frontend | React 19, TypeScript, Ant Design 5, Zustand, React Router, Axios, Vite | Pages, layout, role UI, API integration |
| Backend | .NET 10 Web API, EF Core 10, JWT | Controllers, entities, migrations, seed data |
| Database | MS SQL Server (local, Windows Auth) | Schema, relationships, precision, indexes |
| Integration | REST `/api/*`, JWT interceptors | Contract alignment, auth, error handling |

**UI identity**: Ant Design v5, Inter font, primary purple `#6c5ce7`, dark sidebar, `borderRadius: 12` on cards.

**Auth**: JWT + 5 roles — `Administrator`, `HRManager`, `Manager`, `Employee`, `PayrollStaff`.

---

## Full-Stack Skills Checklist

Use this mental model for every task:

### 1. Understand before coding
- Read an **existing similar module** first (e.g. `ExpensePage` + `ExpenseController` for CRUD + approval).
- Check **role access** in `App.tsx` (`ProtectedRoute`) and `DashboardLayout.tsx` (`roleAccess`).
- Check **plan status** in `HRMS-Plan-Tracker.md` — Phases 1–4 are complete; Phase 5 is future work.

### 2. Backend (.NET)
- Entity → `DbSet` → `OnModelCreating` → migration → controller → seeder.
- Controllers: `[ApiController]`, `[Route("api/[controller]")]`, `[Authorize]`.
- Return `Ok()`, `NotFound()`, `CreatedAtAction()`, `BadRequest()` — never leak raw exceptions.
- Eager-load with `.Include().ThenInclude()`; set decimal `.HasPrecision(18, 2)`.
- FK deletes: `OnDelete(DeleteBehavior.Restrict)` for required relationships.
- DTO/request records at **bottom of controller file** (project convention).
- Get current user: `User.FindFirst(ClaimTypes.NameIdentifier)`.

### 3. Frontend (React)
- One page per route in `frontend/src/pages/` — **PascalCase** filename (e.g. `AssetsPage.tsx`).
- HTTP via `import api from '../services/api'` — never raw `fetch` or duplicate axios instances.
- Auth state: `useAuthStore(s => s.user)`; role checks: `user?.roles?.includes('Administrator')`.
- Notifications: `notifySuccess` / `notifyError` from `utils/notification.ts` (or Ant Design `message` where pages already use it).
- Component order: **state → effects → handlers → columns/helpers → render**.
- Reuse Ant Design: `Card`, `Table`, `Button`, `Modal`, `Form`, `Select`, `Tag`, `Typography`, `Space`, `Row`, `Col`, `Statistic`.
- Status tags: inline `Record<string, string>` color maps (see `ExpensePage.tsx`).

### 4. Full-stack wiring
Every new feature must touch **all** of these when applicable:

```
Entity → Migration → Controller → Seeder → Page → App.tsx route → DashboardLayout menu + roleAccess
```

### 5. Verification (always run)
```bash
cd backend && dotnet build
cd frontend && npx tsc --noEmit
cd tests && dotnet test          # when backend logic changed
```

---

## What TO DO

### Planning & scope
- **Match existing patterns** — copy structure from the closest module, then adapt.
- **Minimal diff** — only change files required for the task; no drive-by refactors.
- **Keep frontend and backend in sync** — field names, status strings, and routes must match.
- **Seed demo data** for new entities so UI is testable immediately.
- **Respect roles** — update `ProtectedRoute`, `roleAccess`, and menu together.

### Backend
- Add `[Authorize]` on controllers; use role attributes when endpoint is role-specific.
- Use `DateTime.UtcNow` for timestamps.
- Initialize navigation props: `= null!` (required), `= new List<T>()` (collections).
- Guard seeders: `if (await context.X.AnyAsync()) return;` before inserting.
- Map entities to DTOs via local `MapXxx` methods or extension mappers in `Models/Mapping/`.
- For approval workflows: state machine + `PUT /{id}/approve` with `{ approved, notes }` body.

### Frontend
- Use TypeScript interfaces for API shapes (prefer typed state over bare `any[]` on new code).
- Handle loading states (`loading` on `Table`, disable buttons while submitting).
- Use `ProtectedRoute` with explicit `roles` when not all authenticated users should access a page.
- Add menu item to `allMenuItems` **and** paths to each role in `roleAccess`.
- Keep API paths lowercase matching controller name: `/expense`, `/compliance`, `/notifications`.

### AI features (optional)
- Groq AI (LLaMA 3 70B) via `AIService.cs`; works offline with rule-based fallbacks when `AI:ApiKey` is empty.
- Log AI usage to `AIUsageLog` when adding new AI endpoints.

### Git & delivery
- **Do not commit** unless the user explicitly asks.
- **Do not push** unless the user explicitly asks.
- After changes, summarize what was built and how to test it (route + demo user).

---

## What NOT TO DO

### Architecture & scope
- **Do not** introduce new frameworks (Redux, TanStack Query, MediatR, AutoMapper) — use Zustand, axios, EF Core as-is.
- **Do not** create separate service layers for simple CRUD if the project uses controller + `_context` directly (see `ExpenseController`).
- **Do not** refactor unrelated files, rename modules, or reformat entire files.
- **Do not** add README/docs/markdown files unless the user asked.
- **Do not** over-engineer: no extra abstractions for one-off helpers, no excessive error handling for impossible paths.

### Backend
- **Do not** skip migrations — never hand-edit production DB without a migration.
- **Do not** use cascade delete on required FKs without checking existing `OnModelCreating` patterns.
- **Do not** return EF entities with circular references — map to DTOs.
- **Do not** hardcode secrets in code — use `appsettings.json` / env (and never commit real API keys).
- **Do not** add comments unless business logic is non-obvious (project convention: minimal comments).

### Frontend
- **Do not** bypass `api.ts` — it handles JWT and 401 redirect.
- **Do not** store tokens outside `localStorage` keys already used (`token`, `user`).
- **Do not** add routes without menu entries (or vice versa) — users will hit 404 or hidden pages.
- **Do not** use inline styles for theme colors — use Ant Design tokens / existing purple theme in `App.tsx`.
- **Do not** create new global CSS files unless the pattern already exists on similar pages.

### Data & security
- **Do not** expose endpoints without `[Authorize]` except public routes (`/careers`, auth login/register).
- **Do not** trust client-side role checks alone — enforce authorization on the server for sensitive actions.
- **Do not** change demo credentials or JWT key in commits meant for shared repos.

### Testing
- **Do not** mark work complete without `dotnet build` and `tsc --noEmit` passing.
- **Do not** add trivial unit tests unless requested or they cover real business rules.

---

## Quick Commands

```bash
# Backend
cd backend && dotnet build
cd backend && dotnet watch run              # http://localhost:5096
cd backend && dotnet ef migrations add <Name>
cd backend && dotnet ef database update

# Frontend
cd frontend && npm run dev                  # http://localhost:5173
cd frontend && npx tsc --noEmit

# Tests
cd tests && dotnet test

# Run both (Windows)
start "Backend" cmd /c "cd backend && dotnet watch run"
start "Frontend" cmd /c "cd frontend && npm run dev"
```

---

## Demo Credentials (dev only)

| Role | Email | Password |
|------|-------|----------|
| Administrator | admin@hrms.com | Admin@123 |
| HRManager | emily@hrms.com | Demo@123 |
| Manager | michael@hrms.com | Demo@123 |
| Employee | priya@hrms.com | Demo@123 |
| PayrollStaff | robert@hrms.com | Demo@123 |

---

## Project Structure

```
backend/
  Controllers/           # REST endpoints (most CRUD lives here)
  Models/
    Entities/            # EF Core entities
    DTOs/                  # Shared DTOs (NoCode, etc.)
    Mapping/               # Extension mappers
  Services/                # Auth, AI, Copilot, Predictive, domain services
  Repositories/            # Generic IRepository<T> (legacy modules)
  Data/
    HRMSDbContext.cs       # DbSets + OnModelCreating
    DatabaseSeeder.cs      # Demo data
  Program.cs               # DI, JWT, CORS, migrate + seed on startup

frontend/src/
  pages/                   # Route-level pages (PascalCase)
  components/              # Reusable UI
  layouts/DashboardLayout.tsx   # Sidebar, notifications, roleAccess
  services/api.ts          # Axios + JWT interceptor
  store/authStore.ts       # Zustand auth
  utils/notification.ts    # Toast helpers
  types/index.ts           # Shared TS types
  App.tsx                  # Routes, theme, ProtectedRoute
```

---

## Standard Page Pattern

```tsx
function XxxPage() {
  const [data, setData] = useState<Xxx[]>([]);
  const [loading, setLoading] = useState(false);
  const [modal, setModal] = useState(false);
  const [form] = Form.useForm();
  const user = useAuthStore(s => s.user);

  const fetchData = async () => {
    setLoading(true);
    try {
      const res = await api.get('/xxx');
      setData(res.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const handleCreate = async (values: XxxForm) => {
    await api.post('/xxx', values);
    setModal(false);
    form.resetFields();
    fetchData();
  };

  const columns = [/* ... */];

  return (
    <div>
      <Title level={4}>Title</Title>
      <Button type="primary" icon={<PlusOutlined />} onClick={() => setModal(true)}>New</Button>
      <Card style={{ borderRadius: 12 }}>
        <Table rowKey="id" loading={loading} dataSource={data} columns={columns} />
      </Card>
      <Modal title="Create" open={modal} onCancel={() => setModal(false)} onOk={() => form.submit()}>
        <Form form={form} layout="vertical" onFinish={handleCreate}>{/* fields */}</Form>
      </Modal>
    </div>
  );
}
```

---

## Feature Implementation Runbook

### New CRUD module
1. Create entity in `Models/Entities/`.
2. Add `DbSet<T>`, relationships, decimal precision in `HRMSDbContext.OnModelCreating`.
3. `dotnet ef migrations add AddXxxModule`.
4. Create `XxxController` with GET list/detail, POST, PUT, DELETE as needed.
5. Add seed data in `DatabaseSeeder.cs`.
6. Create `XxxPage.tsx` following standard pattern.
7. Register route in `App.tsx` with `ProtectedRoute` + roles.
8. Add menu item + `roleAccess` paths in `DashboardLayout.tsx`.
9. Build backend + type-check frontend + smoke-test with demo user.

### Approval workflow module
1. Entity with `Status` string field and audit fields (`ReviewedById`, `ReviewNotes`, `ReviewedAt`).
2. `PUT /{id}/approve` endpoint with role check.
3. Frontend: approve/reject buttons visible only for allowed roles; `Popconfirm` on destructive actions.
4. Optional: multi-stage chain (see `PromotionRequest`, `TransferRequest`).

### AI-powered feature
1. Add endpoint under `Controllers/` or extend `AIService`.
2. Frontend modal or copilot action button calling `/api/ai/*`.
3. Graceful fallback when API key missing.

---

## Key Domain Patterns

| Pattern | Where to look |
|---------|----------------|
| CRUD + approval | `ExpenseController`, `ExpensePage` |
| Multi-stage workflow | `WorkflowController`, `PromotionRequest`, `TransferRequest` |
| No-code JSON config | `FormsController`, `ReportsBuilderController`, `WorkflowsController` |
| AI copilot intents | `CopilotService.cs`, `AICopilotPage` |
| Notifications | `NotificationsController`, bell in `DashboardLayout` |
| Role-filtered sidebar | `roleAccess` + `allMenuItems` in `DashboardLayout.tsx` |
| Generic repository modules | `EmployeeService`, `LeaveService` (older Phase 1 code) |

---

## API Conventions

- Base URL: `http://localhost:5096/api`
- Auth header: `Authorization: Bearer <token>` (via `api.ts` interceptor)
- JSON property naming: camelCase in JSON (ASP.NET default)
- Common response codes: 200 OK, 201 Created, 400 Bad Request, 401 Unauthorized, 404 Not Found

---

## Current Project Status

| Phase | Status |
|-------|--------|
| Phase 1: Foundation | Complete |
| Phase 2: AI & Experience | Complete |
| Phase 3: Workflows & Social | Complete |
| Phase 4: Communication & Assets | Complete |
| Phase 5: Enterprise Scale | Partial (career portal, INR) |
| **Phase 6: Digital Workplace** | **20/37 — In Progress** |

**Phases 1–4: 138/138 complete.** Phase 6 adds unified workplace modules (Meetings, Collaboration Hub, Tasks, Smart Inbox, Announcements, Approval Center). All modules now have full CRUD with role-based access — see [HRMS-Plan-Tracker.md](HRMS-Plan-Tracker.md) for details.

---

## EWXP Product Pillars (Phase 6 Vision)

Positioning: **One Employee → One Login → Complete Workplace** (HRMS + Collaboration + Communication + Productivity + AI).

| Pillar | Modules |
|--------|---------|
| **People** | Employees, Org, Recruitment, Payroll, Performance, Learning |
| **Work** | Meetings, Tasks, Calendar, Approvals, Workflows |
| **Communicate** | Announcements, Collaboration Hub, Smart Inbox, Notifications |
| **Intelligence** | AI Copilot, AI Analytics, Meeting AI (agenda, MoM, conflicts) |
| **Platform** | Admin, Security, Integrations, Audit, Workflow Builder |

### Phase 6 Navigation (grouped sidebar)

Configured in `frontend/src/config/navigation.ts` → `menuGroups` + `buildGroupedMenuItems()`.

Key new routes: `/meetings`, `/collaboration`, `/announcements`, `/tasks`, `/inbox`, `/approvals`

### Responsive UI (mandatory)

- Breakpoints: `frontend/src/utils/breakpoints.ts` — mobile `<768px`, tablet, small desktop, standard
- Hook: `useBreakpoint()` — `isMobileNav`, `isCompactSider`, `isMobile`
- Mobile: drawer sidebar, stacked layouts, horizontal scroll tables, full-width modals
- CSS utilities: `index.css` — `.page-header`, `.responsive-table-wrap`, `.collab-*`, `.inbox-item`
- **Always test** Meetings, Collaboration, Inbox on mobile width before marking complete

### Meeting Lifecycle Engine (Phase 6)

```
Create → AI Agenda → Availability Check → Invitations → Conduct → AI MoM → Tasks → Reminders → Follow-up
```

Backend: `MeetingsController`, entities `Meeting`, `MeetingParticipant`, `WorkTask`  
Frontend: `MeetingsPage.tsx` — dashboard, create modal, AI agenda/conflict check

### Role-Based Workspaces (planned)

Administrator / HR Manager / Manager / Employee / Payroll Staff — filter dashboard widgets by role (extend `DashboardPage.tsx` next).

---

## Phase 5 Guidance (Future)

When implementing Phase 5 items:
- Prefer **incremental modules** (one vertical slice at a time: entity → API → UI).
- Payroll processing needs careful decimal handling and audit trails — extend `Compliance` / `AuditLog` patterns.
- Multi-tenant / SSO are **not** in the current architecture — discuss design before coding.
- Mobile apps are out of scope for this web repo unless explicitly requested.

---

## Agent Workflow Summary

```
1. Read agent.md + check HRMS-Plan-Tracker.md
2. Find similar existing module → copy patterns
3. Implement backend (entity → migration → controller → seed)
4. Implement frontend (page → route → menu → roles)
5. dotnet build && npx tsc --noEmit && dotnet test (if applicable)
6. Smoke-test with appropriate demo role
7. Report changes + test steps to user
```

Do not commit, push, or add unsolicited documentation.
