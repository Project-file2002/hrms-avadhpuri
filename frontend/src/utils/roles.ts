import { useAuthStore } from '../store/authStore';

export const ROLES = {
  ADMIN: 'Administrator',
  HR: 'HRManager',
  MANAGER: 'Manager',
  EMPLOYEE: 'Employee',
  PAYROLL: 'PayrollStaff',
} as const;

export type AppRole = (typeof ROLES)[keyof typeof ROLES];

export function hasAnyRole(roles: string[] | undefined, ...check: string[]): boolean {
  if (!roles?.length) return false;
  return check.some((r) => roles.includes(r));
}

export function isHrRole(roles: string[]): boolean {
  return hasAnyRole(roles, ROLES.ADMIN, ROLES.HR);
}

export function isManagerRole(roles: string[]): boolean {
  return hasAnyRole(roles, ROLES.ADMIN, ROLES.HR, ROLES.MANAGER);
}

export function canViewOrgDashboard(roles: string[]): boolean {
  return hasAnyRole(roles, ROLES.ADMIN, ROLES.HR, ROLES.MANAGER);
}

export function canManageEmployees(roles: string[]): boolean {
  return hasAnyRole(roles, ROLES.ADMIN, ROLES.HR);
}

export function canManageDepartments(roles: string[]): boolean {
  return hasAnyRole(roles, ROLES.ADMIN, ROLES.HR);
}

export function canViewAllHrData(roles: string[]): boolean {
  return hasAnyRole(roles, ROLES.ADMIN, ROLES.HR, ROLES.MANAGER);
}

export function canApproveLeave(roles: string[]): boolean {
  return hasAnyRole(roles, ROLES.ADMIN, ROLES.HR, ROLES.MANAGER);
}

export function canApproveExpense(roles: string[]): boolean {
  return hasAnyRole(roles, ROLES.ADMIN, ROLES.HR, ROLES.MANAGER);
}

export function canCreateCareerRequest(roles: string[]): boolean {
  return hasAnyRole(roles, ROLES.ADMIN, ROLES.HR, ROLES.MANAGER);
}

export function canCreatePoll(roles: string[]): boolean {
  return hasAnyRole(roles, ROLES.ADMIN, ROLES.HR, ROLES.MANAGER);
}

export function canManagePolls(roles: string[]): boolean {
  return hasAnyRole(roles, ROLES.ADMIN, ROLES.HR);
}

export function canModerateContent(roles: string[]): boolean {
  return hasAnyRole(roles, ROLES.ADMIN, ROLES.HR);
}

const promotionStepRoles: Record<string, string[]> = {
  PendingManagerApproval: [ROLES.MANAGER, ROLES.ADMIN],
  PendingHrbpApproval: [ROLES.HR, ROLES.ADMIN],
  PendingDeptHeadApproval: [ROLES.MANAGER, ROLES.ADMIN],
  PendingCeoApproval: [ROLES.ADMIN],
};

const transferStepRoles: Record<string, string[]> = {
  PendingManagerApproval: [ROLES.MANAGER, ROLES.ADMIN],
  PendingHrApproval: [ROLES.HR, ROLES.ADMIN],
  PendingDepartmentApproval: [ROLES.MANAGER, ROLES.ADMIN],
  PendingItApproval: [ROLES.ADMIN],
  PendingPayrollApproval: [ROLES.PAYROLL, ROLES.ADMIN],
  PendingEmployeeAcceptance: [ROLES.EMPLOYEE, ROLES.ADMIN],
};

export function canApproveCareerStep(
  roles: string[],
  status: string,
  type: 'promotion' | 'transfer',
): boolean {
  const map = type === 'promotion' ? promotionStepRoles : transferStepRoles;
  const allowed = map[status];
  if (!allowed) return false;
  return hasAnyRole(roles, ...allowed);
}

export function useRoles() {
  const user = useAuthStore((s) => s.user);
  const roles = user?.roles?.length ? user.roles : [ROLES.EMPLOYEE];

  return {
    user,
    roles,
    isHr: isHrRole(roles),
    isManager: isManagerRole(roles),
    canViewOrgDashboard: canViewOrgDashboard(roles),
    canManageEmployees: canManageEmployees(roles),
    canManageDepartments: canManageDepartments(roles),
    canViewAllHrData: canViewAllHrData(roles),
    canApproveLeave: canApproveLeave(roles),
    canApproveExpense: canApproveExpense(roles),
    canCreateCareerRequest: canCreateCareerRequest(roles),
    canCreatePoll: canCreatePoll(roles),
    canManagePolls: canManagePolls(roles),
    canModerateContent: canModerateContent(roles),
    canApproveCareerStep: (status: string, type: 'promotion' | 'transfer') =>
      canApproveCareerStep(roles, status, type),
  };
}
