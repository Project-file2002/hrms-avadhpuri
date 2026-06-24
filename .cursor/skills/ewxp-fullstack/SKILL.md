---
name: ewxp-fullstack
description: >-
  Full-stack development guide for EWXP (Enterprise Workforce Experience Platform).
  Use when implementing features, fixing bugs, or reviewing changes in this HRMS repo
  with React 19 + TypeScript frontend and .NET 10 Web API backend. Covers entity-to-UI
  workflow, role-based access, migrations, seeding, and project conventions.
---

# EWXP Full-Stack Skill

## When to Apply

Apply this skill for **any code change** in the EWXP / HRMS repository:
- New modules, pages, API endpoints, migrations
- Approval workflows, AI features, no-code builders
- Bug fixes spanning frontend + backend
- Role/menu/route updates

**Always read** [`agent.md`](../../../agent.md) first for full detail. This skill is the operational checklist.

---

## Stack (Do Not Substitute)

| Layer | Use |
|-------|-----|
| Frontend | React 19, TypeScript, Ant Design 5, Zustand, React Router, Axios |
| Backend | .NET 10 Web API, EF Core 10, JWT |
| DB | MS SQL Server via EF migrations |
| HTTP | `frontend/src/services/api.ts` → `http://localhost:5096/api` |

---

## Full-Stack Implementation Order

Execute in this order — **never skip steps**:

1. **Read similar module** — e.g. `ExpenseController` + `ExpensePage` for CRUD/approval
2. **Entity** — `backend/Models/Entities/`
3. **DbContext** — `DbSet`, relationships, `.HasPrecision(18, 2)`, `DeleteBehavior.Restrict`
4. **Migration** — `dotnet ef migrations add <Name>`
5. **Controller** — `[Authorize]`, DTOs at file bottom, map to DTOs in responses
6. **Seeder** — `DatabaseSeeder.cs` with `AnyAsync()` guard
7. **Page** — `frontend/src/pages/XxxPage.tsx` (PascalCase)
8. **Route** — `App.tsx` → `ProtectedRoute` with roles
9. **Menu** — `DashboardLayout.tsx` → `allMenuItems` + `roleAccess` for all 5 roles
10. **Verify** — `dotnet build`, `npx tsc --noEmit`, `dotnet test` if logic changed

---

## DO

- Match existing naming: PascalCase files, camelCase locals, purple `#6c5ce7` UI
- Wire **route + menu + roleAccess + ProtectedRoute** together
- Use `useAuthStore`, `api.get/post/put/delete`, Ant Design components
- Use `DateTime.UtcNow`, eager `.Include()`, seeder idempotency checks
- Keep diffs minimal; copy patterns from nearest module
- Run build + type-check before finishing

## DO NOT

- Add Redux, TanStack Query, MediatR, AutoMapper, or new CSS frameworks
- Use raw `fetch` or bypass JWT interceptor in `api.ts`
- Skip migrations or hand-edit DB schema without EF migration
- Add routes without menu entries (or reverse)
- Refactor unrelated code, add unsolicited markdown, or commit without user request
- Trust frontend role checks alone — secure sensitive endpoints on server
- Return EF entities with navigation cycles — map to DTOs
- Add code comments unless logic is genuinely non-obvious

---

## Roles

`Administrator` | `HRManager` | `Manager` | `Employee` | `PayrollStaff`

Demo login: see `agent.md` credentials table.

Check access in:
- `frontend/src/App.tsx` — `ProtectedRoute`
- `frontend/src/layouts/DashboardLayout.tsx` — `roleAccess`

---

## Quick Reference Files

| Task | Reference |
|------|-----------|
| CRUD + approval | `ExpenseController.cs`, `ExpensePage.tsx` |
| Multi-stage workflow | `PromotionRequest.cs`, `TransferRequest.cs` |
| AI | `AIService.cs`, `CopilotService.cs` |
| No-code | `FormsController.cs`, `FormBuilderPage.tsx` |
| Notifications | `NotificationsController.cs`, `DashboardLayout.tsx` |
| Milestones | `HRMS-Plan-Tracker.md` |

---

## Status

Phases 1–4 **complete** (138/138). Phase 5 (payroll engine, benefits, mobile) is **future**. Phase 6 (Digital Workplace) is **in progress** — see `agent.md` and `HRMS-Plan-Tracker.md`.
