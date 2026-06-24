export interface User {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  employeeId?: number;
  roles: string[];
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

export interface Employee {
  id: number;
  employeeCode: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  position?: string;
  dateOfBirth?: string;
  dateOfJoining?: string;
  status: string;
  gender?: string;
  address?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  departmentName?: string;
  managerName?: string;
}

export interface CreateEmployeeRequest {
  employeeCode: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  position?: string;
  dateOfBirth?: string;
  dateOfJoining?: string;
  gender?: string;
  address?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  departmentId?: number;
  managerId?: number;
}

export interface Department {
  id: number;
  name: string;
  description?: string;
  headName?: string;
  employeeCount: number;
}

export interface LeaveRequest {
  id: number;
  employeeId: number;
  startDate: string;
  endDate: string;
  reason: string;
  status: string;
  createdAt: string;
  employeeName: string;
  leaveTypeName: string;
  leaveTypeId: number;
}

export interface LeaveType {
  id: number;
  name: string;
  description?: string;
  defaultDays: number;
}

export interface LeaveBalance {
  leaveTypeName: string;
  totalDays: number;
  usedDays: number;
  remainingDays: number;
}

export interface AttendanceRecord {
  id: number;
  employeeId: number;
  date: string;
  checkInTime?: string;
  checkOutTime?: string;
  status: string;
  employeeName?: string;
}

export interface Candidate {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  status: string;
  matchScore?: number;
  createdAt: string;
  jobTitle?: string;
}

export interface JobRequisition {
  id: number;
  title: string;
  description?: string;
  requirements?: string;
  status: string;
  candidateCount: number;
}
