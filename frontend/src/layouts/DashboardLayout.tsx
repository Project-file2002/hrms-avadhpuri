import { useState, useEffect } from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { Layout, Menu, Button, Avatar, Dropdown, Typography, Badge, Popover, Drawer, Tag } from 'antd';
import { BellOutlined, MenuOutlined, CloseOutlined } from '@ant-design/icons';
import api from '../services/api';
import {
  DashboardOutlined,
  TeamOutlined,
  BankOutlined,
  CalendarOutlined,
  ClockCircleOutlined,
  TrophyOutlined,
  DollarOutlined,
  UserSwitchOutlined,
  BarChartOutlined,
  SettingOutlined,
  LogoutOutlined,
  UserOutlined,
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  RobotOutlined,
  ApartmentOutlined,
  LineChartOutlined,
  FormOutlined,
  FileTextOutlined,
  NodeIndexOutlined,
  FieldBinaryOutlined,
  HeartOutlined,
  MoneyCollectOutlined,
  RiseOutlined,
  MessageOutlined,
  BulbOutlined,
  LaptopOutlined,
  BookOutlined,
  FileOutlined,
  SafetyCertificateOutlined,
  ScheduleOutlined,
  CommentOutlined,
  NotificationOutlined,
  CheckSquareOutlined,
  InboxOutlined,
  AuditOutlined,
} from '@ant-design/icons';
import { useAuthStore } from '../store/authStore';
import { useBreakpoint } from '../hooks/useBreakpoint';
import {
  getAllowedPathsForRoles,
  getPrimaryRole,
  buildGroupedMenuItems,
  getSelectedMenuKey,
  menuLabels,
} from '../config/navigation';
import ErrorLogViewer from '../components/ErrorLogViewer';

const { Header, Content } = Layout;
const { Text } = Typography;

const menuIcons: Record<string, React.ReactNode> = {
  '/': <DashboardOutlined />,
  '/inbox': <InboxOutlined />,
  '/approvals': <AuditOutlined />,
  '/feed': <HeartOutlined />,
  '/copilot': <RobotOutlined />,
  '/org-chart': <ApartmentOutlined />,
  '/analytics': <LineChartOutlined />,
  '/forms': <FormOutlined />,
  '/reports-builder': <FileTextOutlined />,
  '/workflows': <NodeIndexOutlined />,
  '/admin/custom-fields': <FieldBinaryOutlined />,
  '/employees': <TeamOutlined />,
  '/departments': <BankOutlined />,
  '/leave': <CalendarOutlined />,
  '/attendance': <ClockCircleOutlined />,
  '/performance': <TrophyOutlined />,
  '/expense': <MoneyCollectOutlined />,
  '/career-workflows': <RiseOutlined />,
  '/polls': <BarChartOutlined />,
  '/discussions': <MessageOutlined />,
  '/skills': <BulbOutlined />,
  '/assets': <LaptopOutlined />,
  '/training': <BookOutlined />,
  '/documents': <FileOutlined />,
  '/compliance': <SafetyCertificateOutlined />,
  '/payroll': <DollarOutlined />,
  '/recruitment': <UserSwitchOutlined />,
  '/reports': <BarChartOutlined />,
  '/admin': <SettingOutlined />,
  '/meetings': <ScheduleOutlined />,
  '/collaboration': <CommentOutlined />,
  '/announcements': <NotificationOutlined />,
  '/tasks': <CheckSquareOutlined />,
};

const categoryColors: Record<string, string> = {
  Urgent: 'red', Reminder: 'orange', Information: 'blue', Task: 'green', Approval: 'purple',
};

export default function DashboardLayout() {
  const navigate = useNavigate();
  const location = useLocation();
  const { user, logout } = useAuthStore();
  const { isMobileNav, isCompactSider } = useBreakpoint();
  const [collapsed, setCollapsed] = useState(false);
  const [mobileDrawer, setMobileDrawer] = useState(false);
  const [notifications, setNotifications] = useState<any[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);

  useEffect(() => {
    if (isCompactSider) setCollapsed(true);
  }, [isCompactSider]);

  const fetchNotifications = async () => {
    try {
      const [n, c] = await Promise.all([
        api.get('/notifications'),
        api.get('/notifications/unread-count'),
      ]);
      setNotifications(n.data);
      setUnreadCount(c.data.count);
    } catch {}
  };

  useEffect(() => { fetchNotifications(); }, []);

  const markRead = async (id: number) => {
    await api.put(`/notifications/${id}/read`);
    fetchNotifications();
  };

  const markAllRead = async () => {
    await api.put('/notifications/read-all');
    fetchNotifications();
  };

  const userMenu = {
    items: [
      { key: 'profile', icon: <UserOutlined />, label: 'Profile' },
      { type: 'divider' as const },
      { key: 'logout', icon: <LogoutOutlined />, label: 'Logout', danger: true },
    ],
    onClick: ({ key }: { key: string }) => {
      if (key === 'logout') logout();
    },
  };

  const userRoles = user?.roles?.length ? user.roles : ['Employee'];
  const allowedKeys = getAllowedPathsForRoles(userRoles);
  const menuItems = buildGroupedMenuItems(allowedKeys, menuIcons);
  const selectedKey = getSelectedMenuKey(location.pathname);

  const handleNavigate = (key: string) => {
    navigate(key);
    if (isMobileNav) setMobileDrawer(false);
  };

  const siderWidth = collapsed ? 64 : 240;
  const showDesktopSider = !isMobileNav;

  const renderMenu = (isDark: boolean) => (
    <Menu
      theme={isDark ? 'dark' : 'light'}
      mode="inline"
      selectedKeys={[selectedKey]}
      items={menuItems}
      onClick={({ key }) => handleNavigate(key)}
      style={{
        borderInlineEnd: 'none',
        padding: isDark ? '8px' : '4px',
        marginTop: isDark ? 4 : 0,
      }}
    />
  );

  const brandLabel = collapsed ? 'E' : 'EWXP';

  return (
    <Layout style={{ minHeight: '100vh' }}>
      {showDesktopSider && (
        <Layout.Sider
          trigger={null}
          collapsible
          collapsed={collapsed}
          width={240}
          collapsedWidth={64}
          className="desktop-sider"
          style={{ borderRight: '1px solid rgba(255,255,255,0.06)' }}
        >
          <div style={{
            height: 64,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            borderBottom: '1px solid rgba(255,255,255,0.06)',
            flexDirection: 'column',
            gap: 2,
          }}>
            <Text strong style={{ color: '#fff', fontSize: collapsed ? 16 : 20, letterSpacing: '-0.5px' }}>
              {brandLabel}
            </Text>
            {!collapsed && (
              <Text style={{ color: 'rgba(255,255,255,0.5)', fontSize: 10 }}>Workforce Platform</Text>
            )}
          </div>
          <div className="sidebar-menu-scroll">
            {renderMenu(true)}
          </div>
        </Layout.Sider>
      )}

      <Drawer
        title={null}
        placement="left"
        closable={false}
        onClose={() => setMobileDrawer(false)}
        open={isMobileNav && mobileDrawer}
        size={Math.min(280, window.innerWidth * 0.85)}
        styles={{ body: { padding: 0, background: '#1a1a2e' } }}
        className="mobile-drawer-menu"
        extra={<Button type="text" icon={<CloseOutlined style={{ color: '#fff' }} />} onClick={() => setMobileDrawer(false)} />}
      >
        <div style={{
          height: 64,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          padding: '0 16px',
          borderBottom: '1px solid rgba(255,255,255,0.06)',
        }}>
          <div>
            <Text strong style={{ color: '#fff', fontSize: 20, letterSpacing: '-0.5px' }}>EWXP</Text>
            <Text style={{ color: 'rgba(255,255,255,0.5)', fontSize: 10, display: 'block' }}>Workforce Platform</Text>
          </div>
          <Text style={{ color: 'rgba(255,255,255,0.6)', fontSize: 11 }}>
            {getPrimaryRole(userRoles)}
          </Text>
        </div>
        <div className="sidebar-menu-scroll">
          {renderMenu(true)}
        </div>
      </Drawer>

      <Layout style={{
        marginLeft: showDesktopSider ? siderWidth : 0,
        transition: 'margin-left 0.2s',
        minHeight: '100vh',
        width: showDesktopSider ? `calc(100% - ${siderWidth}px)` : '100%',
      }}>
        <Header style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          padding: '0 24px',
          borderBottom: '1px solid #f0f0f0',
          height: 64,
          position: 'sticky',
          top: 0,
          zIndex: 50,
          background: '#fff',
          width: '100%',
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, minWidth: 0 }}>
            {isMobileNav ? (
              <Button
                type="text"
                icon={<MenuOutlined />}
                onClick={() => setMobileDrawer(true)}
                aria-label="Open menu"
                style={{ fontSize: 18, color: '#6b7280', flexShrink: 0 }}
              />
            ) : (
              <Button
                type="text"
                icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
                onClick={() => setCollapsed(!collapsed)}
                style={{ fontSize: 16, color: '#6b7280', flexShrink: 0 }}
              />
            )}
            {isMobileNav && (
              <Text strong ellipsis className="show-on-mobile-only" style={{ fontSize: 14, maxWidth: 140 }}>
                {menuLabels[selectedKey] ?? 'EWXP'}
              </Text>
            )}
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: 12, flexShrink: 0 }}>
            <Popover
              trigger="click"
              placement="bottomRight"
              title={
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 8, flexWrap: 'wrap' }}>
                  <span>Notifications</span>
                  <Button type="link" size="small" onClick={() => navigate('/inbox')}>Smart Inbox</Button>
                  <Button type="link" size="small" onClick={markAllRead}>Mark all read</Button>
                </div>
              }
              content={
                <div style={{ width: 'min(320px, calc(100vw - 48px))', maxHeight: 400, overflow: 'auto' }}>
                  {notifications.slice(0, 10).map((n: any) => (
                    <div key={n.id} style={{ cursor: 'pointer', padding: '8px 12px', background: n.isRead ? 'transparent' : '#f5f0ff' }}
                      onClick={() => { if (!n.isRead) markRead(n.id); if (n.link) navigate(n.link); }}
                    >
                      <div style={{ fontSize: 13, fontWeight: n.isRead ? 400 : 600 }}>
                        {n.title}{' '}
                        {n.category && <Tag color={categoryColors[n.category] ?? 'default'} style={{ fontSize: 10 }}>{n.category}</Tag>}
                      </div>
                      <div><span style={{ fontSize: 12 }}>{n.message}</span><br /><span style={{ fontSize: 11, color: '#999' }}>{new Date(n.createdAt).toLocaleDateString()}</span></div>
                    </div>
                  ))}
                </div>
              }
            >
              <Badge count={unreadCount} size="small" style={{ cursor: 'pointer' }}>
                <BellOutlined style={{ fontSize: 18, color: '#6b7280', cursor: 'pointer' }} />
              </Badge>
            </Popover>
            <ErrorLogViewer />
            <Dropdown menu={userMenu} placement="bottomRight">
              <div style={{ display: 'flex', alignItems: 'center', gap: 8, cursor: 'pointer', padding: '4px 8px', borderRadius: 8, transition: 'background 0.2s' }}
                onMouseEnter={e => (e.currentTarget.style.background = '#f5f5f5')}
                onMouseLeave={e => (e.currentTarget.style.background = 'transparent')}
              >
                <Avatar size={32} style={{ background: '#6c5ce7' }}>
                  {user?.firstName?.charAt(0)}{user?.lastName?.charAt(0)}
                </Avatar>
                <div className="hide-on-mobile" style={{ lineHeight: 1.2 }}>
                  <Text strong style={{ fontSize: 13, display: 'block' }}>{user?.firstName} {user?.lastName}</Text>
                  <Text type="secondary" style={{ fontSize: 11 }}>{getPrimaryRole(userRoles)}</Text>
                </div>
              </div>
            </Dropdown>
          </div>
        </Header>

        <Content style={{ overflow: 'auto', minHeight: 'calc(100vh - 64px)' }}>
          <div className="page-container">
            <Outlet />
          </div>
        </Content>
      </Layout>
    </Layout>
  );
}
