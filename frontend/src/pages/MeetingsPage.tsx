import { useEffect, useState } from 'react';
import {
  Card, Table, Button, Modal, Form, Input, Select, Tag, Typography,
  Space, Row, Col, Statistic, Tabs, DatePicker, message, Empty, Spin, Drawer, Popconfirm
} from 'antd';
import {
  PlusOutlined, ScheduleOutlined, RobotOutlined, CheckCircleOutlined,
  VideoCameraOutlined, TeamOutlined, EditOutlined, DeleteOutlined
} from '@ant-design/icons';
import dayjs from 'dayjs';
import api from '../services/api';
import { useAuthStore } from '../store/authStore';
import { useBreakpoint } from '../hooks/useBreakpoint';

const { Title, Text, Paragraph } = Typography;
const { RangePicker } = DatePicker;

interface Meeting {
  id: number;
  organizerId: number;
  title: string;
  description?: string;
  agenda?: string;
  meetingType: string;
  status: string;
  priority: string;
  startTime: string;
  endTime: string;
  location?: string;
  onlineLink?: string;
  departmentName?: string;
  organizerName: string;
  participantCount: number;
}

interface DashboardData {
  todayMeetings: Meeting[];
  upcomingMeetings: Meeting[];
  pendingInvitations: number;
  totalScheduled: number;
}

const meetingTypes = [
  'Team', 'HR', 'Project', 'Client', 'Interview', 'Training', '1:1',
  'Performance Review', 'Town Hall', 'Sprint', 'Weekly Review', 'Board Meeting',
];

const typeColors: Record<string, string> = {
  HR: 'purple', Team: 'blue', Sprint: 'cyan', '1:1': 'green',
  Client: 'orange', 'Town Hall': 'magenta', Interview: 'gold',
};

const priorityColors: Record<string, string> = { High: 'red', Normal: 'default', Low: 'blue' };

export default function MeetingsPage() {
  const [dashboard, setDashboard] = useState<DashboardData | null>(null);
  const [meetings, setMeetings] = useState<Meeting[]>([]);
  const [employees, setEmployees] = useState<{ id: number; firstName: string; lastName: string }[]>([]);
  const [departments, setDepartments] = useState<{ id: number; name: string }[]>([]);
  const [loading, setLoading] = useState(true);
  const [modal, setModal] = useState(false);
  const [editModal, setEditModal] = useState(false);
  const [editingMeeting, setEditingMeeting] = useState<Meeting | null>(null);
  const [detailVisible, setDetailVisible] = useState(false);
  const [selectedMeeting, setSelectedMeeting] = useState<Meeting | null>(null);
  const [aiLoading, setAiLoading] = useState(false);
  const [form] = Form.useForm();
  const [editForm] = Form.useForm();
  const user = useAuthStore(s => s.user);
  const { isMobile } = useBreakpoint();

  const fetchAll = async () => {
    setLoading(true);
    try {
      const [dash, all, emps, depts] = await Promise.all([
        api.get('/meetings/dashboard'),
        api.get('/meetings'),
        api.get('/employees'),
        api.get('/departments'),
      ]);
      setDashboard(dash.data);
      setMeetings(all.data);
      setEmployees(emps.data);
      setDepartments(depts.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchAll(); }, []);

  const handleCreate = async (values: Record<string, unknown>) => {
    const range = values.timeRange as [dayjs.Dayjs, dayjs.Dayjs];
    await api.post('/meetings', {
      title: values.title,
      description: values.description,
      agenda: values.agenda,
      meetingType: values.meetingType,
      priority: values.priority,
      location: values.location,
      onlineLink: values.onlineLink,
      departmentId: values.departmentId,
      startTime: range[0].toISOString(),
      endTime: range[1].toISOString(),
      participantIds: values.participantIds ?? [],
    });
    message.success('Meeting created');
    setModal(false);
    form.resetFields();
    fetchAll();
  };

  const showDetail = (record: Meeting) => {
    setSelectedMeeting(record);
    setDetailVisible(true);
  };

  const openEditModal = (record: Meeting) => {
    setEditingMeeting(record);
    editForm.setFieldsValue({
      title: record.title,
      description: record.description,
      meetingType: record.meetingType,
      priority: record.priority,
      location: record.location,
      onlineLink: record.onlineLink,
      departmentId: record.departmentId,
      agenda: record.agenda,
    });
    setEditModal(true);
  };

  const handleEdit = async (values: Record<string, unknown>) => {
    const range = values.timeRange as [dayjs.Dayjs, dayjs.Dayjs];
    await api.put(`/meetings/${editingMeeting?.id}`, {
      title: values.title,
      description: values.description,
      agenda: values.agenda,
      meetingType: values.meetingType,
      priority: values.priority,
      location: values.location,
      onlineLink: values.onlineLink,
      departmentId: values.departmentId,
      startTime: range[0].toISOString(),
      endTime: range[1].toISOString(),
      participantIds: values.participantIds ?? [],
    });
    message.success('Meeting updated');
    setEditModal(false);
    setEditingMeeting(null);
    editForm.resetFields();
    fetchAll();
  };

  const handleDelete = async (id: number) => {
    await api.delete(`/meetings/${id}`);
    message.success('Meeting deleted');
    fetchAll();
  };

  const generateAgenda = async () => {
    const title = form.getFieldValue('title');
    const meetingType = form.getFieldValue('meetingType') ?? 'Team';
    const description = form.getFieldValue('description');
    if (!title) { message.warning('Enter meeting title first'); return; }
    setAiLoading(true);
    try {
      const res = await api.post('/meetings/ai/agenda', { title, meetingType, description });
      form.setFieldValue('agenda', res.data.agenda);
      message.success('AI agenda generated');
    } finally {
      setAiLoading(false);
    }
  };

  const checkAvailability = async () => {
    const range = form.getFieldValue('timeRange') as [dayjs.Dayjs, dayjs.Dayjs] | undefined;
    const participantIds = form.getFieldValue('participantIds') as number[] | undefined;
    if (!range?.[0] || !participantIds?.length) {
      message.warning('Select time and participants first');
      return;
    }
    setAiLoading(true);
    try {
      const res = await api.post('/meetings/ai/suggest-time', {
        meetingType: form.getFieldValue('meetingType') ?? 'Team',
        proposedStart: range[0].toISOString(),
        proposedEnd: range[1].toISOString(),
        participantIds,
      });
      if (res.data.hasConflicts) {
        message.warning(`Conflicts found for ${res.data.conflicts.length} participant(s)`);
      } else {
        message.success(`No conflicts. Suggested duration: ${res.data.suggestedDurationMinutes} min`);
      }
    } finally {
      setAiLoading(false);
    }
  };

  const formatTime = (iso: string) => dayjs(iso).format('HH:mm');
  const formatDate = (iso: string) => dayjs(iso).format('MMM D, YYYY');

  const renderMeetingList = (items: Meeting[]) => (
    items.length === 0 ? <Empty description="No meetings" /> : (
      items.map((m) => (
        <div key={m.id} className="meeting-list-item" style={{ padding: '12px 16px', borderBottom: '1px solid #f0f0f0' }}>
          <Space wrap>
            <Text strong>{formatTime(m.startTime)}</Text>
            <Text>{m.title}</Text>
            <Tag color={typeColors[m.meetingType] ?? 'default'}>{m.meetingType}</Tag>
            {m.priority === 'High' && <Tag color="red">High</Tag>}
          </Space>
          <div>
            <Space wrap size={4}>
              <Text type="secondary">{m.organizerName}</Text>
              {m.location && <Text type="secondary">· {m.location}</Text>}
              {m.onlineLink && <VideoCameraOutlined style={{ color: '#6c5ce7' }} />}
              <TeamOutlined /> {m.participantCount}
            </Space>
          </div>
        </div>
      ))
    )
  );

  const isOwnMeeting = (m: Meeting) => user?.employeeId === m.organizerId;

  const columns = [
    {
      title: 'When', key: 'when', width: 140,
      render: (_: unknown, m: Meeting) => (
        <div>
          <div>{formatDate(m.startTime)}</div>
          <Text type="secondary">{formatTime(m.startTime)} – {formatTime(m.endTime)}</Text>
        </div>
      ),
    },
    {
      title: 'Title', dataIndex: 'title', key: 'title',
      render: (t: string, m: Meeting) => <a onClick={() => showDetail(m)}>{t}</a>,
    },
    {
      title: 'Type', dataIndex: 'meetingType', key: 'meetingType',
      render: (t: string) => <Tag color={typeColors[t]}>{t}</Tag>,
    },
    { title: 'Organizer', dataIndex: 'organizerName', key: 'organizerName' },
    {
      title: 'Priority', dataIndex: 'priority', key: 'priority',
      render: (p: string) => <Tag color={priorityColors[p]}>{p}</Tag>,
    },
    { title: 'Status', dataIndex: 'status', key: 'status' },
    {
      title: 'Actions', key: 'actions', width: 200,
      render: (_: unknown, m: Meeting) => (
        <Space>
          <Button size="small" onClick={() => showDetail(m)}>View</Button>
          {m.status === 'Scheduled' && isOwnMeeting(m) && (
            <>
              <Button size="small" icon={<EditOutlined />} onClick={() => openEditModal(m)}>Edit</Button>
              <Popconfirm title="Delete this meeting?" onConfirm={() => handleDelete(m.id)} okText="Delete" okButtonProps={{ danger: true }}>
                <Button size="small" danger icon={<DeleteOutlined />}>Delete</Button>
              </Popconfirm>
            </>
          )}
        </Space>
      ),
    },
  ];

  if (loading && !dashboard) {
    return <div style={{ textAlign: 'center', padding: 48 }}><Spin size="large" /></div>;
  }

  return (
    <div>
      <div className="page-header">
        <div>
          <Title level={4} style={{ margin: 0 }}>Meetings & Calendar</Title>
          <Text type="secondary">Schedule, collaborate, and track accountability</Text>
        </div>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => setModal(true)}>
          Create Meeting
        </Button>
      </div>

      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={12} sm={6}>
          <Card className="stat-card"><Statistic title="Today" value={dashboard?.todayMeetings.length ?? 0} prefix={<ScheduleOutlined />} /></Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card className="stat-card"><Statistic title="Upcoming" value={dashboard?.totalScheduled ?? 0} /></Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card className="stat-card"><Statistic title="Pending Invites" value={dashboard?.pendingInvitations ?? 0} styles={{ content: { color: '#fdcb6e' } }} /></Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card className="stat-card"><Statistic title="This Week" value={meetings.filter(m => dayjs(m.startTime).isBefore(dayjs().add(7, 'day'))).length} /></Card>
        </Col>
      </Row>

      <Tabs
        items={[
          {
            key: 'today',
            label: "Today's Meetings",
            children: (
              <Card className="content-card">
                {renderMeetingList(dashboard?.todayMeetings ?? [])}
              </Card>
            ),
          },
          {
            key: 'upcoming',
            label: 'Upcoming',
            children: (
              <Card className="content-card">
                {renderMeetingList(dashboard?.upcomingMeetings ?? [])}
              </Card>
            ),
          },
          {
            key: 'all',
            label: 'All Meetings',
            children: (
              <Card className="content-card">
                <div className="responsive-table-wrap">
                  <Table rowKey="id" loading={loading} dataSource={meetings} columns={columns}
                    scroll={isMobile ? { x: 700 } : undefined} pagination={{ pageSize: 10 }} />
                </div>
              </Card>
            ),
          },
        ]}
      />

      <Modal
        title="Create Meeting"
        open={modal}
        onCancel={() => setModal(false)}
        onOk={() => form.submit()}
        width={isMobile ? '95vw' : 640}
        confirmLoading={aiLoading}
      >
        <Form form={form} layout="vertical" onFinish={handleCreate}
          initialValues={{ meetingType: 'Team', priority: 'Normal' }}>
          <Form.Item name="title" label="Meeting Title" rules={[{ required: true }]}>
            <Input placeholder="Weekly team meeting" />
          </Form.Item>
          <Row gutter={12}>
            <Col xs={24} sm={12}>
              <Form.Item name="meetingType" label="Type" rules={[{ required: true }]}>
                <Select options={meetingTypes.map(t => ({ value: t, label: t }))} />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12}>
              <Form.Item name="priority" label="Priority">
                <Select options={['Low', 'Normal', 'High'].map(p => ({ value: p, label: p }))} />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="timeRange" label="Date & Time" rules={[{ required: true }]}>
            <RangePicker showTime style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="participantIds" label="Participants">
            <Select mode="multiple" placeholder="Select team members"
              options={employees.map(e => ({ value: e.id, label: `${e.firstName} ${e.lastName}` }))} />
          </Form.Item>
          <Form.Item name="departmentId" label="Department Calendar">
            <Select allowClear placeholder="Link to department calendar"
              options={departments.map(d => ({ value: d.id, label: d.name }))} />
          </Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea rows={2} /></Form.Item>
          <Form.Item name="agenda" label="Agenda"
            extra={
              <Space style={{ marginTop: 4 }}>
                <Button size="small" icon={<RobotOutlined />} loading={aiLoading} onClick={generateAgenda}>AI Generate Agenda</Button>
                <Button size="small" onClick={checkAvailability}>Check Availability</Button>
              </Space>
            }>
            <Input.TextArea rows={4} placeholder="Objectives, talking points, action items..." />
          </Form.Item>
          <Row gutter={12}>
            <Col xs={24} sm={12}>
              <Form.Item name="location" label="Location"><Input placeholder="Conference Room A" /></Form.Item>
            </Col>
            <Col xs={24} sm={12}>
              <Form.Item name="onlineLink" label="Online Link"><Input placeholder="https://meet..." /></Form.Item>
            </Col>
          </Row>
        </Form>
      </Modal>

      <Drawer title={selectedMeeting?.title} open={detailVisible} onClose={() => { setDetailVisible(false); setSelectedMeeting(null); }} styles={{ wrapper: { width: 520 } }}>
        {selectedMeeting && (
          <Space direction="vertical" style={{ width: '100%' }}>
            <div><strong>Organizer:</strong> {selectedMeeting.organizerName}</div>
            <div><strong>Type:</strong> <Tag color={typeColors[selectedMeeting.meetingType]}>{selectedMeeting.meetingType}</Tag></div>
            <div><strong>Status:</strong> {selectedMeeting.status}</div>
            <div><strong>When:</strong> {formatDate(selectedMeeting.startTime)} {formatTime(selectedMeeting.startTime)} – {formatTime(selectedMeeting.endTime)}</div>
            {selectedMeeting.departmentName && <div><strong>Department:</strong> {selectedMeeting.departmentName}</div>}
            {selectedMeeting.location && <div><strong>Location:</strong> {selectedMeeting.location}</div>}
            {selectedMeeting.onlineLink && <div><strong>Online:</strong> <a href={selectedMeeting.onlineLink} target="_blank">{selectedMeeting.onlineLink}</a></div>}
            {selectedMeeting.description && <Card title="Description" size="small" style={{ borderRadius: 8, width: '100%' }}><Text>{selectedMeeting.description}</Text></Card>}
            {selectedMeeting.agenda && <Card title="Agenda" size="small" style={{ borderRadius: 8, width: '100%' }}><Text style={{ whiteSpace: 'pre-wrap' }}>{selectedMeeting.agenda}</Text></Card>}
          </Space>
        )}
      </Drawer>

      <Modal
        title="Edit Meeting"
        open={editModal}
        onCancel={() => { setEditModal(false); setEditingMeeting(null); }}
        onOk={() => editForm.submit()}
        width={isMobile ? '95vw' : 640}
      >
        <Form form={editForm} layout="vertical" onFinish={handleEdit}
          initialValues={{ meetingType: 'Team', priority: 'Normal' }}>
          <Form.Item name="title" label="Meeting Title" rules={[{ required: true }]}>
            <Input placeholder="Weekly team meeting" />
          </Form.Item>
          <Row gutter={12}>
            <Col xs={24} sm={12}>
              <Form.Item name="meetingType" label="Type" rules={[{ required: true }]}>
                <Select options={meetingTypes.map(t => ({ value: t, label: t }))} />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12}>
              <Form.Item name="priority" label="Priority">
                <Select options={['Low', 'Normal', 'High'].map(p => ({ value: p, label: p }))} />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="timeRange" label="Date & Time" rules={[{ required: true }]}>
            <RangePicker showTime style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="participantIds" label="Participants">
            <Select mode="multiple" placeholder="Select team members"
              options={employees.map(e => ({ value: e.id, label: `${e.firstName} ${e.lastName}` }))} />
          </Form.Item>
          <Form.Item name="departmentId" label="Department Calendar">
            <Select allowClear placeholder="Link to department calendar"
              options={departments.map(d => ({ value: d.id, label: d.name }))} />
          </Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea rows={2} /></Form.Item>
          <Form.Item name="agenda" label="Agenda"><Input.TextArea rows={4} placeholder="Objectives, talking points, action items..." /></Form.Item>
          <Row gutter={12}>
            <Col xs={24} sm={12}>
              <Form.Item name="location" label="Location"><Input placeholder="Conference Room A" /></Form.Item>
            </Col>
            <Col xs={24} sm={12}>
              <Form.Item name="onlineLink" label="Online Link"><Input placeholder="https://meet..." /></Form.Item>
            </Col>
          </Row>
        </Form>
      </Modal>
    </div>
  );
}
