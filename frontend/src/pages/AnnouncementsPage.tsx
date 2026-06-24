import { useEffect, useState } from 'react';
import {
  Card, Button, Modal, Form, Input, Select, Tag, Typography, Space, Switch, Spin, Empty, message
} from 'antd';
import { PlusOutlined, NotificationOutlined, CheckOutlined } from '@ant-design/icons';
import api from '../services/api';
import { useRoles } from '../utils/roles';

const { Title, Text, Paragraph } = Typography;

interface Announcement {
  id: number;
  title: string;
  content: string;
  scope: string;
  priority: string;
  acknowledgementRequired: boolean;
  createdAt: string;
  createdByName: string;
  departmentName?: string;
  isRead: boolean;
  isAcknowledged: boolean;
  readCount: number;
}

const priorityColors: Record<string, string> = {
  Urgent: 'red', High: 'orange', Normal: 'blue', Low: 'default',
};

export default function AnnouncementsPage() {
  const [items, setItems] = useState<Announcement[]>([]);
  const [loading, setLoading] = useState(true);
  const [modal, setModal] = useState(false);
  const [form] = Form.useForm();
  const { isHr } = useRoles();

  const fetchAll = async () => {
    setLoading(true);
    try {
      const res = await api.get('/announcements');
      setItems(res.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchAll(); }, []);

  const handleCreate = async (values: Record<string, unknown>) => {
    await api.post('/announcements', values);
    message.success('Announcement published');
    setModal(false);
    form.resetFields();
    fetchAll();
  };

  const acknowledge = async (id: number) => {
    await api.put(`/announcements/${id}/acknowledge`);
    message.success('Acknowledged');
    fetchAll();
  };

  const markRead = async (id: number) => {
    await api.put(`/announcements/${id}/read`);
    fetchAll();
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <Title level={4} style={{ margin: 0 }}><NotificationOutlined /> Announcements</Title>
          <Text type="secondary">Company, department, and role-based announcements with read tracking</Text>
        </div>
        {isHr && (
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setModal(true)}>New Announcement</Button>
        )}
      </div>

      {loading ? (
        <Spin size="large" style={{ display: 'block', marginTop: 80 }} />
      ) : items.length === 0 ? (
        <Empty description="No announcements" />
      ) : items.map((a) => (
        <Card key={a.id} className="content-card" style={{ marginBottom: 16 }} onClick={() => !a.isRead && markRead(a.id)}>
          <Space direction="vertical" style={{ width: '100%' }} size={8}>
            <Space wrap>
              <Title level={5} style={{ margin: 0 }}>{a.title}</Title>
              <Tag color={priorityColors[a.priority]}>{a.priority}</Tag>
              <Tag>{a.scope}</Tag>
              {a.departmentName && <Tag color="blue">{a.departmentName}</Tag>}
              {!a.isRead && <Tag color="processing">New</Tag>}
            </Space>
            <Paragraph style={{ margin: 0 }}>{a.content}</Paragraph>
            <Space wrap>
              <Text type="secondary">{a.createdByName} · {new Date(a.createdAt).toLocaleDateString()}</Text>
              <Text type="secondary">· {a.readCount} read</Text>
              {a.acknowledgementRequired && !a.isAcknowledged && (
                <Button size="small" type="primary" icon={<CheckOutlined />} onClick={(e) => { e.stopPropagation(); acknowledge(a.id); }}>
                  Acknowledge Required
                </Button>
              )}
              {a.isAcknowledged && <Tag color="green">Acknowledged</Tag>}
            </Space>
          </Space>
        </Card>
      ))}

      <Modal title="Publish Announcement" open={modal} onCancel={() => setModal(false)} onOk={() => form.submit()}>
        <Form form={form} layout="vertical" onFinish={handleCreate}
          initialValues={{ scope: 'Company', priority: 'Normal' }}>
          <Form.Item name="title" label="Title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="content" label="Content" rules={[{ required: true }]}><Input.TextArea rows={4} /></Form.Item>
          <Form.Item name="scope" label="Scope">
            <Select options={['Company', 'Department', 'Branch'].map(s => ({ value: s, label: s }))} />
          </Form.Item>
          <Form.Item name="priority" label="Priority">
            <Select options={['Low', 'Normal', 'High', 'Urgent'].map(p => ({ value: p, label: p }))} />
          </Form.Item>
          <Form.Item name="acknowledgementRequired" label="Acknowledgement Required" valuePropName="checked">
            <Switch />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
