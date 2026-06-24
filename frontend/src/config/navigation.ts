import type { MenuProps } from 'antd';
import type { ReactNode } from 'react';

export const roleAccess: Record<string, string[]> = {
  Administrator: ['/', '/inbox', '/approvals', '/feed', '/copilot', '/org-chart', '/analytics', '/employees', '/departments', '/leave', '/attendance', '/performance', '/expense', '/career-workflows', '/polls', '/discussions', '/skills', '/assets', '/training', '/documents', '/compliance', '/payroll', '/recruitment', '/reports', '/forms', '/reports-builder', '/workflows', '/admin', '/admin/custom-fields', '/meetings', '/collaboration', '/announcements', '/tasks'],
  HRManager: ['/', '/inbox', '/approvals', '/feed', '/copilot', '/org-chart', '/analytics', '/employees', '/departments', '/leave', '/attendance', '/performance', '/expense', '/career-workflows', '/polls', '/discussions', '/skills', '/assets', '/training', '/documents', '/compliance', '/recruitment', '/reports', '/forms', '/reports-builder', '/workflows', '/meetings', '/collaboration', '/announcements', '/tasks'],
  Manager: ['/', '/inbox', '/approvals', '/feed', '/copilot', '/org-chart', '/analytics', '/employees', '/departments', '/leave', '/attendance', '/performance', '/expense', '/career-workflows', '/polls', '/discussions', '/skills', '/training', '/documents', '/forms', '/meetings', '/collaboration', '/announcements', '/tasks'],
  Employee: ['/', '/inbox', '/feed', '/copilot', '/org-chart', '/leave', '/attendance', '/expense', '/career-workflows', '/polls', '/discussions', '/skills', '/training', '/documents', '/forms', '/meetings', '/collaboration', '/announcements', '/tasks'],
  PayrollStaff: ['/', '/inbox', '/approvals', '/feed', '/copilot', '/org-chart', '/employees', '/attendance', '/payroll', '/reports', '/forms', '/reports-builder', '/meetings', '/announcements', '/tasks'],
};

export const menuLabels: Record<string, string> = {
  '/': 'Dashboard',
  '/inbox': 'Smart Inbox',
  '/approvals': 'Approval Center',
  '/feed': 'Company Feed',
  '/copilot': 'AI Assistant',
  '/org-chart': 'Org Chart',
  '/analytics': 'Analytics',
  '/forms': 'Form Builder',
  '/reports-builder': 'Report Builder',
  '/workflows': 'Workflows',
  '/admin/custom-fields': 'Custom Fields',
  '/employees': 'Employees',
  '/departments': 'Departments',
  '/leave': 'Leave',
  '/attendance': 'Attendance',
  '/performance': 'Performance',
  '/expense': 'Expense',
  '/career-workflows': 'Career',
  '/polls': 'Polls',
  '/discussions': 'Discussions',
  '/skills': 'Skills',
  '/assets': 'Assets',
  '/training': 'Learning',
  '/documents': 'Documents',
  '/compliance': 'Compliance',
  '/payroll': 'Payroll',
  '/recruitment': 'Recruitment',
  '/reports': 'Reports',
  '/admin': 'Settings',
  '/meetings': 'Meetings & Calendar',
  '/collaboration': 'Collaboration',
  '/announcements': 'Announcements',
  '/tasks': 'Tasks',
};

/** EWXP 5-pillar grouped navigation */
export const menuGroups: { key: string; label: string; paths: string[] }[] = [
  { key: 'home', label: 'Home', paths: ['/', '/inbox', '/approvals', '/copilot'] },
  { key: 'people', label: 'People', paths: ['/employees', '/departments', '/org-chart', '/skills', '/recruitment'] },
  { key: 'workforce', label: 'Workforce', paths: ['/leave', '/attendance', '/performance', '/career-workflows', '/expense'] },
  { key: 'payroll', label: 'Payroll', paths: ['/payroll'] },
  { key: 'work', label: 'Work', paths: ['/meetings', '/tasks', '/collaboration', '/announcements'] },
  { key: 'learning', label: 'Learning', paths: ['/training'] },
  { key: 'communicate', label: 'Communicate', paths: ['/feed', '/discussions', '/polls'] },
  { key: 'intelligence', label: 'Intelligence', paths: ['/analytics', '/reports', '/reports-builder'] },
  { key: 'platform', label: 'Platform', paths: ['/documents', '/assets', '/compliance', '/forms', '/workflows', '/admin/custom-fields', '/admin'] },
];

export function getAllowedPathsForRoles(roles: string[]): string[] {
  const paths = new Set<string>();
  for (const role of roles) {
    const allowed = roleAccess[role] ?? roleAccess.Employee;
    allowed.forEach((path) => paths.add(path));
  }
  return Array.from(paths);
}

export function getRolesForPath(path: string): string[] {
  const roles: string[] = [];
  for (const [role, paths] of Object.entries(roleAccess)) {
    if (paths.includes(path)) roles.push(role);
  }
  return roles.length ? roles : ['Employee'];
}

export function canAccessPath(roles: string[], path: string): boolean {
  return getAllowedPathsForRoles(roles).includes(path);
}

export function getPrimaryRole(roles: string[]): string {
  const priority = ['Administrator', 'HRManager', 'PayrollStaff', 'Manager', 'Employee'];
  return priority.find((role) => roles.includes(role)) ?? roles[0] ?? 'Employee';
}

export function buildGroupedMenuItems(
  allowedPaths: string[],
  iconMap: Record<string, ReactNode>
): MenuProps['items'] {
  const items: MenuProps['items'] = [];
  for (const group of menuGroups) {
    const children = group.paths
      .filter((p) => allowedPaths.includes(p))
      .map((p) => ({ key: p, icon: iconMap[p], label: menuLabels[p] }));
    if (children.length === 0) continue;
    items.push({ key: group.key, label: group.label, type: 'group', children });
  }
  return items;
}

export function getSelectedMenuKey(pathname: string): string {
  if (pathname.startsWith('/admin/custom-fields')) return '/admin/custom-fields';
  if (pathname.startsWith('/employees/')) return '/employees';
  const segment = '/' + pathname.split('/').filter(Boolean)[0];
  return segment === '/' ? '/' : segment;
}
