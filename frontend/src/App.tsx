import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ConfigProvider, App as AntApp, theme } from 'antd';
import { useEffect } from 'react';
import { useAuthStore } from './store/authStore';
import { getRolesForPath } from './config/navigation';
import { initNotification } from './utils/notification';
import DashboardLayout from './layouts/DashboardLayout';
import LoginPage from './pages/LoginPage';
import CandidateJobPortal from './pages/careers/CandidateJobPortal';
import CareerWizardPage from './pages/careers/CareerWizardPage';
import CareerDashboardPage from './pages/careers/CareerDashboardPage';
import CareerProfilePage from './pages/careers/CareerProfilePage';
import DashboardPage from './pages/DashboardPage';
import EmployeeListPage from './pages/EmployeeListPage';
import EmployeeProfilePage from './pages/EmployeeProfilePage';
import DepartmentListPage from './pages/DepartmentListPage';
import LeavePage from './pages/LeavePage';
import AttendancePage from './pages/AttendancePage';
import PerformancePage from './pages/PerformancePage';
import PayrollPage from './pages/PayrollPage';
import RecruitmentPage from './pages/RecruitmentPage';
import ReportsPage from './pages/ReportsPage';
import AdminSettingsPage from './pages/AdminSettingsPage';
import AICopilotPage from './pages/AICopilotPage';
import OrganizationChartPage from './pages/OrganizationChartPage';
import PredictiveAnalyticsPage from './pages/PredictiveAnalyticsPage';
import CustomFieldsPage from './pages/CustomFieldsPage';
import FormBuilderPage from './pages/FormBuilderPage';
import ReportBuilderPage from './pages/ReportBuilderPage';
import WorkflowDesignerPage from './pages/WorkflowDesignerPage';
import SocialFeedPage from './pages/SocialFeedPage';
import ExpensePage from './pages/ExpensePage';
import CareerWorkflowPage from './pages/CareerWorkflowPage';
import PollsPage from './pages/PollsPage';
import DiscussionsPage from './pages/DiscussionsPage';
import SkillsPage from './pages/SkillsPage';
import AssetsPage from './pages/AssetsPage';
import TrainingPage from './pages/TrainingPage';
import DocumentsPage from './pages/DocumentsPage';
import CompliancePage from './pages/CompliancePage';
import MeetingsPage from './pages/MeetingsPage';
import CollaborationPage from './pages/CollaborationPage';
import InboxPage from './pages/InboxPage';
import ApprovalsPage from './pages/ApprovalsPage';
import AnnouncementsPage from './pages/AnnouncementsPage';
import TasksPage from './pages/TasksPage';

function ProtectedRoute({ children, roles }: { children: React.ReactNode; roles?: string[] }) {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const user = useAuthStore((state) => state.user);
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (roles && user) {
    const hasRole = user.roles.some(r => roles.includes(r));
    if (!hasRole) return <Navigate to="/" replace />;
  }
  return <>{children}</>;
}

function RoleRoute({ path, children }: { path: string; children: React.ReactNode }) {
  return <ProtectedRoute roles={getRolesForPath(path)}>{children}</ProtectedRoute>;
}

function AppContent() {
  const notification = AntApp.useApp();
  useEffect(() => {
    initNotification(notification);
  }, [notification]);

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/careers" element={<CandidateJobPortal />} />
        <Route path="/careers/apply/:jobId" element={<CareerWizardPage />} />
        <Route path="/careers/dashboard" element={<CareerDashboardPage />} />
        <Route path="/careers/profile" element={<CareerProfilePage />} />
        <Route path="/" element={<ProtectedRoute><DashboardLayout /></ProtectedRoute>}>
          <Route index element={<RoleRoute path="/"><DashboardPage /></RoleRoute>} />
          <Route path="employees" element={<RoleRoute path="/employees"><EmployeeListPage /></RoleRoute>} />
          <Route path="employees/:id" element={<RoleRoute path="/employees"><EmployeeProfilePage /></RoleRoute>} />
          <Route path="departments" element={<RoleRoute path="/departments"><DepartmentListPage /></RoleRoute>} />
          <Route path="leave" element={<RoleRoute path="/leave"><LeavePage /></RoleRoute>} />
          <Route path="attendance" element={<RoleRoute path="/attendance"><AttendancePage /></RoleRoute>} />
          <Route path="performance" element={<RoleRoute path="/performance"><PerformancePage /></RoleRoute>} />
          <Route path="payroll" element={<RoleRoute path="/payroll"><PayrollPage /></RoleRoute>} />
          <Route path="recruitment" element={<RoleRoute path="/recruitment"><RecruitmentPage /></RoleRoute>} />
          <Route path="copilot" element={<RoleRoute path="/copilot"><AICopilotPage /></RoleRoute>} />
          <Route path="org-chart" element={<RoleRoute path="/org-chart"><OrganizationChartPage /></RoleRoute>} />
          <Route path="analytics" element={<RoleRoute path="/analytics"><PredictiveAnalyticsPage /></RoleRoute>} />
          <Route path="reports" element={<RoleRoute path="/reports"><ReportsPage /></RoleRoute>} />
          <Route path="admin" element={<RoleRoute path="/admin"><AdminSettingsPage /></RoleRoute>} />
          <Route path="admin/custom-fields" element={<RoleRoute path="/admin/custom-fields"><CustomFieldsPage /></RoleRoute>} />
          <Route path="forms" element={<RoleRoute path="/forms"><FormBuilderPage /></RoleRoute>} />
          <Route path="reports-builder" element={<RoleRoute path="/reports-builder"><ReportBuilderPage /></RoleRoute>} />
          <Route path="workflows" element={<RoleRoute path="/workflows"><WorkflowDesignerPage /></RoleRoute>} />
          <Route path="feed" element={<RoleRoute path="/feed"><SocialFeedPage /></RoleRoute>} />
          <Route path="expense" element={<RoleRoute path="/expense"><ExpensePage /></RoleRoute>} />
          <Route path="career-workflows" element={<RoleRoute path="/career-workflows"><CareerWorkflowPage /></RoleRoute>} />
          <Route path="polls" element={<RoleRoute path="/polls"><PollsPage /></RoleRoute>} />
          <Route path="discussions" element={<RoleRoute path="/discussions"><DiscussionsPage /></RoleRoute>} />
          <Route path="skills" element={<RoleRoute path="/skills"><SkillsPage /></RoleRoute>} />
          <Route path="assets" element={<RoleRoute path="/assets"><AssetsPage /></RoleRoute>} />
          <Route path="training" element={<RoleRoute path="/training"><TrainingPage /></RoleRoute>} />
          <Route path="documents" element={<RoleRoute path="/documents"><DocumentsPage /></RoleRoute>} />
          <Route path="compliance" element={<RoleRoute path="/compliance"><CompliancePage /></RoleRoute>} />
          <Route path="meetings" element={<RoleRoute path="/meetings"><MeetingsPage /></RoleRoute>} />
          <Route path="collaboration" element={<RoleRoute path="/collaboration"><CollaborationPage /></RoleRoute>} />
          <Route path="inbox" element={<RoleRoute path="/inbox"><InboxPage /></RoleRoute>} />
          <Route path="approvals" element={<RoleRoute path="/approvals"><ApprovalsPage /></RoleRoute>} />
          <Route path="announcements" element={<RoleRoute path="/announcements"><AnnouncementsPage /></RoleRoute>} />
          <Route path="tasks" element={<RoleRoute path="/tasks"><TasksPage /></RoleRoute>} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default function App() {
  const loadUser = useAuthStore((state) => state.loadUser);

  useEffect(() => {
    loadUser();
  }, [loadUser]);

  return (
    <ConfigProvider
      theme={{
        algorithm: theme.defaultAlgorithm,
        token: {
          colorPrimary: '#6c5ce7',
          colorSuccess: '#00b894',
          colorWarning: '#fdcb6e',
          colorError: '#e17055',
          colorInfo: '#74b9ff',
          borderRadius: 8,
          fontFamily: 'Inter, -apple-system, BlinkMacSystemFont, sans-serif',
          fontSize: 14,
          colorBgContainer: '#ffffff',
          colorBgLayout: '#f5f5f5',
          colorBorder: '#e8e8e8',
          colorText: '#1a1a2e',
          colorTextSecondary: '#6b7280',
          boxShadow: '0 1px 3px rgba(0,0,0,0.04)',
        },
        components: {
          Layout: {
            headerBg: '#ffffff',
            siderBg: '#1a1a2e',
            bodyBg: '#f5f5f5',
          },
          Menu: {
            darkItemBg: '#1a1a2e',
            darkItemColor: '#a0a0b8',
            darkItemSelectedBg: 'rgba(108, 92, 231, 0.15)',
            darkItemSelectedColor: '#ffffff',
            itemBorderRadius: 8,
            darkSubMenuItemBg: '#151528',
          },
          Card: {
            padding: 20,
            borderRadius: 12,
          },
          Table: {
            headerBg: '#fafafa',
            headerColor: '#6b7280',
            borderColor: '#f0f0f0',
            borderRadius: 8,
            scrollToFirstRowOnChange: true,
          },
          Button: {
            borderRadius: 8,
            controlHeight: 36,
          },
          Input: {
            borderRadius: 8,
            controlHeight: 36,
          },
          Select: {
            borderRadius: 8,
            controlHeight: 36,
          },
          Modal: {
            borderRadius: 12,
          },
          Tag: {
            borderRadius: 6,
          },
        },
      }}
    >
      <AntApp>
        <AppContent />
      </AntApp>
    </ConfigProvider>
  );
}
