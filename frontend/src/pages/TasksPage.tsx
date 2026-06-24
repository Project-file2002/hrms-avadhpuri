import { useEffect, useState } from 'react';
import {
  Card, Table, Button, Modal, Form, Input, Select, Tag, Typography, Space, message, Popconfirm
} from 'antd';
import { PlusOutlined, CheckCircleOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import api from '../services/api';
import { useAuthStore } from '../store/authStore';

const { Title, Text } = Typography;

interface WorkTask {
  id: number;
  title: string;
  description?: string;
  status: string;
  priority: string;
  dueDate?: string;
  meetingTitle?: string;
  assignedToName: string;
  assignedByName?: string;
  createdAt: string;
}

const statusColors: Record<string, string> = {
  Pending: 'orange', InProgress: 'blue', Completed: 'green',
};

export default function TasksPage() {
  const [tasks, setTasks] = useState<WorkTask[]>([]);
  const [employees, setEmployees] = useState<{ id: number; firstName: string; lastName: string }[]>([]);
  const [loading, setLoading] = useState(true);
  const [modal, setModal] = useState(false);
  const [editModal, setEditModal] = useState(false);
  const [editingTask, setEditingTask] = useState<WorkTask | null>(null);
  const [form] = Form.useForm();
  const [editForm] = Form.useForm();
  const user = useAuthStore(s => s.user);

  const fetchTasks = async () => {
    setLoading(true);
    try {
      const params = user?.employeeId ? { employeeId: user.employeeId } : undefined;
      const [t, e] = await Promise.all([
        api.get('/tasks', { params }),
        api.get('/employees'),
      ]);
      setTasks(t.data);
      setEmployees(e.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchTasks(); }, [user?.employeeId]);

  const handleCreate = async (values: Record<string, unknown>) => {
    await api.post('/tasks', values);
    message.success('Task created');
    setModal(false);
    form.resetFields();
    fetchTasks();
  };

  const openEditModal = (record: WorkTask) => {
    setEditingTask(record);
    editForm.setFieldsValue({
      title: record.title,
      description: record.description,
      assignedToId: record.assignedToName,
      priority: record.priority,
    });
    setEditModal(true);
  };

  const handleEdit = async (values: Record<string, unknown>) => {
    await api.put(`/tasks/${editingTask?.id}`, values);
    message.success('Task updated');
    setEditModal(false);
    setEditingTask(null);
    editForm.resetFields();
    fetchTasks();
  };

  const handleDelete = async (id: number) => {
    await api.delete(`/tasks/${id}`);
    message.success('Task deleted');
    fetchTasks();
  };

  const completeTask = async (id: number) => {
    await api.put(`/tasks/${id}/complete`);
    message.success('Task completed');
    fetchTasks();
  };

  const columns = [
    { title: 'Task', dataIndex: 'title', key: 'title' },
    { title: 'Assigned To', dataIndex: 'assignedToName', key: 'assignedToName' },
    {
      title: 'Priority', dataIndex: 'priority', key: 'priority',
      render: (p: string) => <Tag color={p === 'High' ? 'red' : 'default'}>{p}</Tag>,
    },
    {
      title: 'Status', dataIndex: 'status', key: 'status',
      render: (s: string) => <Tag color={statusColors[s]}>{s}</Tag>,
    },
    {
      title: 'Due', dataIndex: 'dueDate', key: 'dueDate',
      render: (v?: string) => v ? new Date(v).toLocaleDateString() : '—',
    },
    { title: 'From Meeting', dataIndex: 'meetingTitle', key: 'meetingTitle', render: (v?: string) => v ?? '—' },
    {
      title: 'Actions', key: 'actions', width: 220,
      render: (_: unknown, r: WorkTask) => (
        <Space>
          <Button size="small" icon={<EditOutlined />} onClick={() => openEditModal(r)}>Edit</Button>
          <Popconfirm title="Delete this task?" onConfirm={() => handleDelete(r.id)} okText="Delete" okButtonProps={{ danger: true }}>
            <Button size="small" danger icon={<DeleteOutlined />}>Delete</Button>
          </Popconfirm>
          {r.status !== 'Completed' && (
            <Popconfirm title="Mark complete?" onConfirm={() => completeTask(r.id)}>
              <Button size="small" type="primary" icon={<CheckCircleOutlined />} />
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  return (
    <div>
      <div className="page-header">
        <div>
          <Title level={4} style={{ margin: 0 }}>Tasks</Title>
          <Text type="secondary">Meeting action items and assigned work with accountability tracking</Text>
        </div>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => setModal(true)}>New Task</Button>
      </div>

      <Card className="content-card">
        <div className="responsive-table-wrap">
          <Table rowKey="id" loading={loading} dataSource={tasks} columns={columns}
            pagination={{ pageSize: 10 }} scroll={{ x: 800 }} />
        </div>
      </Card>

      <Modal title="Create Task" open={modal} onCancel={() => setModal(false)} onOk={() => form.submit()}>
        <Form form={form} layout="vertical" onFinish={handleCreate} initialValues={{ priority: 'Normal' }}>
          <Form.Item name="title" label="Title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea rows={2} /></Form.Item>
          <Form.Item name="assignedToId" label="Assign To" rules={[{ required: true }]}>
            <Select options={employees.map(e => ({ value: e.id, label: `${e.firstName} ${e.lastName}` }))} />
          </Form.Item>
          <Form.Item name="priority" label="Priority">
            <Select options={['Low', 'Normal', 'High'].map(p => ({ value: p, label: p }))} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal title="Edit Task" open={editModal} onCancel={() => { setEditModal(false); setEditingTask(null); }} onOk={() => editForm.submit()}>
        <Form form={editForm} layout="vertical" onFinish={handleEdit} initialValues={{ priority: 'Normal' }}>
          <Form.Item name="title" label="Title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea rows={2} /></Form.Item>
          <Form.Item name="assignedToId" label="Assign To" rules={[{ required: true }]}>
            <Select options={employees.map(e => ({ value: e.id, label: `${e.firstName} ${e.lastName}` }))} />
          </Form.Item>
          <Form.Item name="priority" label="Priority">
            <Select options={['Low', 'Normal', 'High'].map(p => ({ value: p, label: p }))} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
