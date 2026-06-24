import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Card, Descriptions, Tag, Spin, Button, Typography, Space, Divider,
  Tabs, Table, Timeline, Statistic, Row, Col, Progress, Empty
} from 'antd';
import {
  ArrowLeftOutlined, MailOutlined, PhoneOutlined, EnvironmentOutlined,
  UserOutlined, CalendarOutlined, ClockCircleOutlined, TrophyOutlined,
  DollarOutlined, TeamOutlined, CrownOutlined, HistoryOutlined
} from '@ant-design/icons';
import type { Employee } from '../types';
import api from '../services/api';

const { Title, Text } = Typography;

const statusColor: Record<string, string> = {
  Active: 'green',
  Probation: 'orange',
  Resigned: 'red',
  Terminated: 'red',
};

interface LeaveBalance {
  leaveTypeName: string;
  totalDays: number;
  usedDays: number;
  remainingDays: number;
}

interface LeaveRequest {
  id: number;
  startDate: string;
  endDate: string;
  reason: string;
  status: string;
  leaveTypeName: string;
}

interface AttendanceRecord {
  id: number;
  date: string;
  checkInTime: string;
  checkOutTime: string;
  status: string;
}

interface PerformanceReview {
  id: number;
  title: string;
  status: string;
  overallScore: number;
  cycleName: string;
  reviewerName: string;
  scores: { criteria: string; score: number; comments: string }[];
}

interface PayrollInfo {
  structureName: string;
  earningsTotal: number;
  deductionsTotal: number;
  netTotal: number;
  components: { name: string; type: string; amount: number }[];
}

export default function EmployeeProfilePage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [employee, setEmployee] = useState<Employee | null>(null);
  const [loading, setLoading] = useState(true);
  const [leaveBalances, setLeaveBalances] = useState<LeaveBalance[]>([]);
  const [leaveRequests, setLeaveRequests] = useState<LeaveRequest[]>([]);
  const [attendance, setAttendance] = useState<AttendanceRecord[]>([]);
  const [reviews, setReviews] = useState<PerformanceReview[]>([]);

  const [tabLoading, setTabLoading] = useState<string | null>(null);
  const [timeline, setTimeline] = useState<any[]>([]);

  useEffect(() => {
    api.get(`/employees/${id}`)
      .then(res => setEmployee(res.data))
      .finally(() => setLoading(false));
  }, [id]);

  const fetchLeaveData = async () => {
    if (leaveBalances.length > 0) return;
    setTabLoading('leave');
    try {
      const [balRes, reqRes] = await Promise.all([
        api.get(`/leave/balances/${id}`),
        api.get(`/leave?employeeId=${id}`),
      ]);
      setLeaveBalances(balRes.data.map((b: LeaveBalance) => ({ ...b, remainingDays: b.totalDays - b.usedDays })));
      setLeaveRequests(reqRes.data);
    } catch { /* ignore */ }
    setTabLoading(null);
  };

  const fetchAttendance = async () => {
    if (attendance.length > 0) return;
    setTabLoading('attendance');
    try {
      const from = new Date();
      from.setMonth(from.getMonth() - 1);
      const res = await api.get(`/attendance/${id}?from=${from.toISOString()}&to=${new Date().toISOString()}`);
      setAttendance(res.data);
    } catch { /* ignore */ }
    setTabLoading(null);
  };

  const fetchPerformance = async () => {
    if (reviews.length > 0) return;
    setTabLoading('performance');
    try {
      const res = await api.get('/performance/reviews');
      setReviews(res.data.filter((r: PerformanceReview) => r.employeeName?.includes(employee?.firstName || '')));
    } catch { /* ignore */ }
    setTabLoading(null);
  };

  const fetchTimeline = async () => {
    if (timeline.length > 0) return;
    setTabLoading('timeline');
    try {
      const res = await api.get(`/employees/${id}/timeline`);
      setTimeline(res.data);
    } catch { /* ignore */ }
    setTabLoading(null);
  };

  if (loading) return <Spin size="large" style={{ display: 'block', marginTop: 80 }} />;
  if (!employee) return <p>Employee not found.</p>;

  const leaveColumns = [
    { title: 'Leave Type', dataIndex: 'leaveTypeName', key: 'type' },
    { title: 'Total', dataIndex: 'totalDays', key: 'total', width: 80 },
    { title: 'Used', dataIndex: 'usedDays', key: 'used', width: 80 },
    {
      title: 'Remaining', key: 'remaining', width: 120,
      render: (_: unknown, r: LeaveBalance) => (
        <Space>
          <Progress
            type="circle"
            percent={Math.round((r.usedDays / r.totalDays) * 100)}
            size={32}
            format={() => `${r.remainingDays}`}
            strokeColor={r.remainingDays > 5 ? '#52c41a' : r.remainingDays > 2 ? '#faad14' : '#ff4d4f'}
          />
        </Space>
      ),
    },
  ];

  const attendanceColumns = [
    { title: 'Date', dataIndex: 'date', key: 'date', render: (d: string) => new Date(d).toLocaleDateString() },
    { title: 'Check In', dataIndex: 'checkInTime', key: 'in', render: (t: string) => t ? new Date(t).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : '-' },
    { title: 'Check Out', dataIndex: 'checkOutTime', key: 'out', render: (t: string) => t ? new Date(t).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : '-' },
    {
      title: 'Status', dataIndex: 'status', key: 'status',
      render: (s: string) => (
        <Tag color={s === 'Present' ? 'green' : s === 'Late' ? 'orange' : 'red'}>{s}</Tag>
      ),
    },
  ];

  const reviewColumns = [
    { title: 'Title', dataIndex: 'title', key: 'title' },
    { title: 'Cycle', dataIndex: 'cycleName', key: 'cycle' },
    {
      title: 'Score', dataIndex: 'overallScore', key: 'score',
      render: (s: number) => s != null ? <Text strong>{s.toFixed(1)}</Text> : '-',
    },
    {
      title: 'Status', dataIndex: 'status', key: 'status',
      render: (s: string) => (
        <Tag color={s === 'Completed' ? 'green' : s === 'In Progress' ? 'blue' : 'orange'}>{s}</Tag>
      ),
    },
  ];

  const presentCount = attendance.filter(a => a.status === 'Present').length;
  const absentCount = attendance.filter(a => a.status === 'Absent').length;

  const tabItems = [
    {
      key: 'overview',
      label: <span><UserOutlined /> Overview</span>,
      children: (
        <>
          <Card style={{ borderRadius: 12, marginBottom: 16 }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 20, marginBottom: 24 }}>
              <div style={{
                width: 72, height: 72, borderRadius: 20,
                background: 'linear-gradient(135deg, #6c5ce7, #a29bfe)',
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                fontSize: 28, fontWeight: 600, color: '#fff',
              }}>
                {employee.firstName.charAt(0)}{employee.lastName.charAt(0)}
              </div>
              <div>
                <Title level={4} style={{ margin: 0 }}>{employee.firstName} {employee.lastName}</Title>
                <Space style={{ marginTop: 4 }}>
                  <Text type="secondary">{employee.position ?? 'No position'}</Text>
                  <Tag color={statusColor[employee.status] ?? 'default'}>{employee.status}</Tag>
                </Space>
              </div>
            </div>

            <Descriptions column={{ xs: 1, sm: 2 }} bordered size="small">
              <Descriptions.Item label="Employee Code">{employee.employeeCode}</Descriptions.Item>
              <Descriptions.Item label="Department">{employee.departmentName ?? '-'}</Descriptions.Item>
              <Descriptions.Item label={<><MailOutlined /> Email</>}>{employee.email}</Descriptions.Item>
              <Descriptions.Item label={<><PhoneOutlined /> Phone</>}>{employee.phone ?? '-'}</Descriptions.Item>
              <Descriptions.Item label={<><CrownOutlined /> Manager</>}>{employee.managerName ?? '-'}</Descriptions.Item>
              <Descriptions.Item label={<><CalendarOutlined /> Date Joined</>}>
                {employee.dateOfJoining ? new Date(employee.dateOfJoining).toLocaleDateString() : '-'}
              </Descriptions.Item>
              <Descriptions.Item label={<><CalendarOutlined /> DOB</>}>
                {employee.dateOfBirth ? new Date(employee.dateOfBirth).toLocaleDateString() : '-'}
              </Descriptions.Item>
              <Descriptions.Item label="Gender">{employee.gender ?? '-'}</Descriptions.Item>
              <Descriptions.Item label={<><EnvironmentOutlined /> Address</>} span={2}>{employee.address ?? '-'}</Descriptions.Item>
            </Descriptions>
          </Card>

          <Card title="Emergency Contact" style={{ borderRadius: 12 }}>
            <Descriptions column={{ xs: 1, sm: 2 }} bordered size="small">
              <Descriptions.Item label="Name">{employee.emergencyContactName ?? '-'}</Descriptions.Item>
              <Descriptions.Item label={<><PhoneOutlined /> Phone</>}>{employee.emergencyContactPhone ?? '-'}</Descriptions.Item>
            </Descriptions>
          </Card>
        </>
      ),
    },
    {
      key: 'leave',
      label: <span><CalendarOutlined /> Leave ({leaveBalances.length})</span>,
      children: tabLoading === 'leave' ? <Spin style={{ display: 'block', margin: 40 }} /> : (
        <Spin spinning={false}>
          {leaveBalances.length > 0 ? (
            <>
              <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
                {leaveBalances.map(lb => (
                  <Col xs={12} sm={8} key={lb.leaveTypeName}>
                    <Card size="small" style={{ borderRadius: 8, textAlign: 'center' }}>
                      <Statistic
                        title={lb.leaveTypeName}
                        value={lb.remainingDays}
                        suffix={`/ ${lb.totalDays}`}
                        styles={{ content: {  color: lb.remainingDays > 5 ? '#52c41a' : '#faad14', fontWeight: 600  } }}
                      />
                      <Text type="secondary" style={{ fontSize: 11 }}>{lb.usedDays} used</Text>
                    </Card>
                  </Col>
                ))}
              </Row>
              <Table
                dataSource={leaveBalances}
                columns={leaveColumns}
                rowKey="leaveTypeName"
                pagination={false}
                size="small"
                scroll={{ x: 'max-content' }}
              />
            </>
          ) : <Empty description="No leave data found" />}
        </Spin>
      ),
    },
    {
      key: 'attendance',
      label: <span><ClockCircleOutlined /> Attendance ({attendance.length})</span>,
      children: tabLoading === 'attendance' ? <Spin style={{ display: 'block', margin: 40 }} /> : (
        <Spin spinning={false}>
          {attendance.length > 0 ? (
            <>
              <Row gutter={16} style={{ marginBottom: 24 }}>
                <Col xs={24} sm={8}>
                  <Card size="small" style={{ borderRadius: 8, textAlign: 'center' }}>
                    <Statistic title="Present" value={presentCount} styles={{ content: {  color: '#52c41a'  } }} />
                  </Card>
                </Col>
                <Col xs={24} sm={8}>
                  <Card size="small" style={{ borderRadius: 8, textAlign: 'center' }}>
                    <Statistic title="Absent" value={absentCount} styles={{ content: {  color: '#ff4d4f'  } }} />
                  </Card>
                </Col>
                <Col xs={24} sm={8}>
                  <Card size="small" style={{ borderRadius: 8, textAlign: 'center' }}>
                    <Statistic title="Total" value={attendance.length} />
                  </Card>
                </Col>
              </Row>
              <Table
                dataSource={attendance}
                columns={attendanceColumns}
                rowKey="id"
                pagination={false}
                size="small"
                scroll={{ x: 'max-content' }}
              />
            </>
          ) : <Empty description="No attendance records found" />}
        </Spin>
      ),
    },
    {
      key: 'performance',
      label: <span><TrophyOutlined /> Performance</span>,
      children: tabLoading === 'performance' ? <Spin style={{ display: 'block', margin: 40 }} /> : (
        <Spin spinning={false}>
          {reviews.length > 0 ? (
            <Table
              dataSource={reviews}
              columns={reviewColumns}
              rowKey="id"
              pagination={false}
              size="small"
              scroll={{ x: 'max-content' }}
              expandable={{
                expandedRowRender: (r) => (
                  <div style={{ padding: 16 }}>
                    {r.scores?.map((s, i) => (
                      <div key={i} style={{ marginBottom: 12 }}>
                        <Space style={{ width: '100%', justifyContent: 'space-between' }}>
                          <Text strong>{s.criteria}</Text>
                          <Text>{s.score.toFixed(1)} / 5.0</Text>
                        </Space>
                        <Progress percent={Math.round((s.score / 5) * 100)} size="small" strokeColor="#6c5ce7" />
                        {s.comments && <Text type="secondary" style={{ fontSize: 12 }}>{s.comments}</Text>}
                      </div>
                    ))}
                  </div>
                ),
              }}
            />
          ) : <Empty description="No performance reviews found" />}
        </Spin>
      ),
    },
    {
      key: 'timeline',
      label: <span><HistoryOutlined /> Timeline</span>,
      children: tabLoading === 'timeline' ? <Spin style={{ display: 'block', margin: 40 }} /> : (
        <Spin spinning={false}>
          {timeline.length > 0 ? (
            <Timeline
              mode="left"
              items={timeline.map((evt: any) => ({
                color: evt.color,
                label: <Text type="secondary" style={{ fontSize: 12 }}>{new Date(evt.date).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })}</Text>,
                children: (
                  <div>
                    <Text strong style={{ fontSize: 14 }}>{evt.title}</Text>
                    <br />
                    <Text type="secondary" style={{ fontSize: 12 }}>{evt.description}</Text>
                  </div>
                ),
              }))}
            />
          ) : <Empty description="No timeline events found" />}
        </Spin>
      ),
    },
  ];

  return (
    <>
      <div className="page-header">
        <Space>
          <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/employees')}>Back</Button>
          <Title level={4} style={{ margin: 0 }}>
            <TeamOutlined style={{ marginRight: 8, color: '#6c5ce7' }} />
            Digital Employee Twin
          </Title>
        </Space>
      </div>

      <Card
        className="content-card"
        styles={{ body: { padding: 0 } }}
        style={{ borderRadius: 12, overflow: 'hidden' }}
      >
        <Tabs
          defaultActiveKey="overview"
          items={tabItems}
          size="large"
          tabBarStyle={{ paddingLeft: 24, marginBottom: 0 }}
          onTabClick={(key) => {
            if (key === 'leave') fetchLeaveData();
            if (key === 'attendance') fetchAttendance();
            if (key === 'performance') fetchPerformance();
            if (key === 'timeline') fetchTimeline();
          }}
        />
      </Card>
    </>
  );
}
