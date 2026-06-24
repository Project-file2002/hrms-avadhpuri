import { useState, useEffect } from 'react';
import { Card, Table, Tag, Button, Modal, Form, Input, Select, DatePicker, Row, Col, Statistic, Tabs, Typography } from 'antd';
import { PlusOutlined, CheckCircleOutlined, ClockCircleOutlined, WarningOutlined, AuditOutlined, SafetyCertificateOutlined } from '@ant-design/icons';
import type { Dayjs } from 'dayjs';
import api from '../services/api';
import { notifySuccess, notifyError } from '../utils/notification';

const { Title } = Typography;

const statusColor: Record<string, string> = { Pending: 'orange', InProgress: 'blue', Completed: 'green', Overdue: 'red' };

export default function CompliancePage() {
  const [records, setRecords] = useState<any[]>([]);
  const [auditLogs, setAuditLogs] = useState<any[]>([]);
  const [privacyLogs, setPrivacyLogs] = useState<any[]>([]);
  const [dashboard, setDashboard] = useState<any>({});
  const [loading, setLoading] = useState(false);
  const [modal, setModal] = useState(false);
  const [form] = Form.useForm();

  const fetchAll = async () => {
    setLoading(true);
    try {
      const [r, l, p, d] = await Promise.all([
        api.get('/compliance/records'),
        api.get('/compliance/audit-log'),
        api.get('/compliance/privacy-logs'),
        api.get('/compliance/dashboard'),
      ]);
      setRecords(r.data);
      setAuditLogs(l.data.items);
      setPrivacyLogs(p.data);
      setDashboard(d.data);
    } catch { notifyError('Failed to load compliance data'); }
    setLoading(false);
  };

  useEffect(() => { fetchAll(); }, []);

  const handleCreate = async (values: any) => {
    await api.post('/compliance/records', {
      ...values, dueDate: values.dueDate?.toISOString()
    });
    setModal(false); form.resetFields(); fetchAll();
    notifySuccess('Compliance record created');
  };

  const handleStatusUpdate = async (id: number, status: string) => {
    await api.put(`/compliance/records/${id}`, { status });
    fetchAll();
    notifySuccess(`Status updated to ${status}`);
  };

  const recordColumns = [
    { title: 'Title', dataIndex: 'title', key: 'title' },
    { title: 'Category', dataIndex: 'category', key: 'category', render: (c: string) => <Tag color={c === 'GDPR' ? 'purple' : 'blue'}>{c}</Tag> },
    { title: 'Regulation', dataIndex: 'regulation', key: 'regulation' },
    { title: 'Status', dataIndex: 'status', key: 'status', render: (s: string) => <Tag color={statusColor[s]}>{s}</Tag> },
    { title: 'Assigned To', dataIndex: 'assignedToName', key: 'assignedToName' },
    { title: 'Due Date', dataIndex: 'dueDate', key: 'dueDate', render: (d: string) => d ? new Date(d).toLocaleDateString() : '-' },
    { title: 'Completed', dataIndex: 'completedAt', key: 'completedAt', render: (d: string) => d ? new Date(d).toLocaleDateString() : '-' },
    {
      title: 'Action', key: 'action',
      render: (_: any, r: any) => r.status !== 'Completed' ? (
        <Select value={r.status} onChange={v => handleStatusUpdate(r.id, v)} style={{ width: 130 }}
          options={[
            { value: 'Pending', label: 'Pending' },
            { value: 'InProgress', label: 'In Progress' },
            { value: 'Completed', label: 'Completed' },
          ]}
        />
      ) : <Tag color="green">Done</Tag>
    },
  ];

  const auditColumns = [
    { title: 'Action', dataIndex: 'action', key: 'action' },
    { title: 'Entity', dataIndex: 'entityType', key: 'entityType', render: (e: string) => <Tag>{e}</Tag> },
    { title: 'User', dataIndex: 'userName', key: 'userName' },
    { title: 'Date', dataIndex: 'timestamp', key: 'timestamp', render: (d: string) => new Date(d).toLocaleString() },
  ];

  const privacyColumns = [
    { title: 'Action', dataIndex: 'action', key: 'action' },
    { title: 'Data Category', dataIndex: 'dataCategory', key: 'dataCategory', render: (c: string) => <Tag color="purple">{c}</Tag> },
    { title: 'Consent', dataIndex: 'consentStatus', key: 'consentStatus', render: (s: string) => <Tag color={s === 'Granted' ? 'green' : 'red'}>{s}</Tag> },
    { title: 'Employee', dataIndex: 'employeeName', key: 'employeeName' },
    { title: 'Date', dataIndex: 'timestamp', key: 'timestamp', render: (d: string) => new Date(d).toLocaleString() },
  ];

  return (
    <div>
      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        <Col xs={12} sm={6}><Card><Statistic title="Total Records" value={dashboard.totalRecords ?? 0} prefix={<SafetyCertificateOutlined />} /></Card></Col>
        <Col xs={12} sm={6}><Card><Statistic title="Pending" value={dashboard.pending ?? 0} styles={{ content: {  color: '#faad14'  } }} prefix={<ClockCircleOutlined />} /></Card></Col>
        <Col xs={12} sm={6}><Card><Statistic title="Overdue" value={dashboard.overdue ?? 0} styles={{ content: {  color: '#ff4d4f'  } }} prefix={<WarningOutlined />} /></Card></Col>
        <Col xs={12} sm={6}><Card><Statistic title="Audit Events" value={dashboard.auditLogCount ?? 0} prefix={<AuditOutlined />} /></Card></Col>
      </Row>

      <Tabs defaultActiveKey="records" items={[
        { key: 'records', label: 'Compliance Records', children: (
          <Card style={{ borderRadius: 12 }}>
            <div className="page-toolbar" style={{ marginBottom: 16 }}>
              <Title level={5} style={{ margin: 0 }}>Compliance Records</Title>
              <Button type="primary" icon={<PlusOutlined />} onClick={() => setModal(true)}>New Record</Button>
            </div>
            <Table dataSource={records} columns={recordColumns} rowKey="id" loading={loading} scroll={{ x: 'max-content' }} />
          </Card>
        )},
        { key: 'audit', label: 'Audit Log', children: (
          <Card style={{ borderRadius: 12 }}>
            <Title level={5} style={{ marginBottom: 16 }}>Change History</Title>
            <Table dataSource={auditLogs} columns={auditColumns} rowKey="id" loading={loading} pagination={{ pageSize: 20 }} scroll={{ x: 'max-content' }} />
          </Card>
        )},
        { key: 'privacy', label: 'Data Privacy', children: (
          <Card style={{ borderRadius: 12 }}>
            <Title level={5} style={{ marginBottom: 16 }}>Privacy Logs</Title>
            <Table dataSource={privacyLogs} columns={privacyColumns} rowKey="id" loading={loading} pagination={{ pageSize: 20 }} scroll={{ x: 'max-content' }} />
          </Card>
        )},
      ]} />

      <Modal title="New Compliance Record" open={modal} onCancel={() => setModal(false)} onOk={() => form.submit()}>
        <Form form={form} onFinish={handleCreate} layout="vertical">
          <Form.Item name="title" label="Title" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="category" label="Category" rules={[{ required: true }]}>
            <Select options={[
              { value: 'Statutory', label: 'Statutory' },
              { value: 'GDPR', label: 'GDPR / Data Privacy' },
              { value: 'ISO', label: 'ISO Standard' },
              { value: 'Internal', label: 'Internal Policy' },
            ]} />
          </Form.Item>
          <Form.Item name="regulation" label="Regulation">
            <Input />
          </Form.Item>
          <Form.Item name="dueDate" label="Due Date">
            <DatePicker style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="notes" label="Notes">
            <Input.TextArea rows={3} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
