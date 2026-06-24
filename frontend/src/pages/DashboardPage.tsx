import { useEffect, useState } from 'react';
import { Row, Col, Card, Statistic, Spin, Avatar, Tag, Typography, Space } from 'antd';
import { TeamOutlined, CalendarOutlined, ClockCircleOutlined, UserSwitchOutlined, GiftOutlined } from '@ant-design/icons';
import api from '../services/api';
import { useRoles } from '../utils/roles';

const { Text } = Typography;

interface DashboardStats {
  totalEmployees: number;
  pendingLeaves: number;
  todayAttendance: number;
  openPositions: number;
}

export default function DashboardPage() {
  const { canViewOrgDashboard } = useRoles();
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [celebrations, setCelebrations] = useState<any>({ birthdays: [], anniversaries: [] });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([
      canViewOrgDashboard ? api.get('/reports/dashboard') : Promise.resolve({ data: null }),
      api.get('/knowledge/celebrations'),
    ]).then(([s, c]) => {
      setStats(s.data);
      setCelebrations(c.data);
    }).finally(() => setLoading(false));
  }, [canViewOrgDashboard]);

  if (loading) return <Spin size="large" style={{ display: 'block', marginTop: 80 }} />;

  const cards = [
    { title: 'Total Employees', value: stats?.totalEmployees ?? 0, icon: <TeamOutlined />, color: '#6c5ce7' },
    { title: 'Pending Leaves', value: stats?.pendingLeaves ?? 0, icon: <CalendarOutlined />, color: '#fdcb6e' },
    { title: 'Today Attendance', value: stats?.todayAttendance ?? 0, icon: <ClockCircleOutlined />, color: '#00b894' },
    { title: 'Open Positions', value: stats?.openPositions ?? 0, icon: <UserSwitchOutlined />, color: '#e17055' },
  ];

  const allCelebrations = [
    ...(celebrations.birthdays || []).map((c: any) => ({ ...c, icon: '🎂', typeLabel: 'Birthday' })),
    ...(celebrations.anniversaries || []).map((c: any) => ({ ...c, icon: '🎉', typeLabel: `${c.years} Year Anniversary` })),
  ].sort((a, b) => new Date(a.upcoming).getTime() - new Date(b.upcoming).getTime());

  return (
    <>
      <div className="page-header">
        <h2>Dashboard</h2>
      </div>
      {canViewOrgDashboard ? (
        <Row gutter={[20, 20]}>
          {cards.map(c => (
            <Col xs={24} sm={12} lg={6} key={c.title}>
              <Card className="stat-card" styles={{ body: { padding: 24 } }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
                  <div style={{
                    width: 48, height: 48, borderRadius: 12, background: `${c.color}15`,
                    display: 'flex', alignItems: 'center', justifyContent: 'center',
                    fontSize: 22, color: c.color,
                  }}>
                    {c.icon}
                  </div>
                  <Statistic title={c.title} value={c.value} styles={{ content: { fontSize: 28, fontWeight: 600 } }} />
                </div>
              </Card>
            </Col>
          ))}
        </Row>
      ) : (
        <Card className="content-card" style={{ borderRadius: 12, marginBottom: 24 }}>
          <Text type="secondary">
            Welcome to your HR self-service hub. Use the menu to manage leave, attendance, expenses, training, and more.
          </Text>
        </Card>
      )}

      <Row gutter={[20, 20]} style={{ marginTop: 24 }}>
        {canViewOrgDashboard && (
          <Col xs={24} lg={16}>
            <Card className="content-card" title="Recent Activity" style={{ borderRadius: 12 }}>
              <p style={{ color: '#6b7280', textAlign: 'center', padding: '40px 0' }}>
                Activity feed will appear here as actions are logged.
              </p>
            </Card>
          </Col>
        )}
        <Col xs={24} lg={canViewOrgDashboard ? 8 : 24}>
          <Card className="content-card"
            title={<span><GiftOutlined style={{ color: '#eb2f96', marginRight: 8 }} />Upcoming Celebrations</span>}
            style={{ borderRadius: 12 }}
          >
            {allCelebrations.length === 0 ? (
              <p style={{ color: '#6b7280', textAlign: 'center', padding: 20 }}>No celebrations this month</p>
            ) : (
              allCelebrations.map((item: any) => (
                <div key={item.name + item.upcoming} style={{ display: 'flex', alignItems: 'flex-start', gap: 12, padding: '8px 0' }}>
                  <Avatar style={{ background: item.type === 'Birthday' ? '#eb2f96' : '#fa8c16' }}>{item.icon}</Avatar>
                  <div>
                    <Text strong>{item.name}</Text>
                    <div>
                      <Space>
                        <Tag color={item.type === 'Birthday' ? 'magenta' : 'orange'}>{item.typeLabel}</Tag>
                        <Text type="secondary">{item.position}</Text>
                        <Text type="secondary">{new Date(item.upcoming).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}</Text>
                      </Space>
                    </div>
                  </div>
                </div>
              )))}
          </Card>
        </Col>
      </Row>
    </>
  );
}
