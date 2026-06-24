import { useEffect, useState } from 'react';
import { Card, Table, Tag, Spin, Select, Button, Modal, Form, Input } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import api from '../services/api';
import { notifySuccess, notifyError } from '../utils/notification';
import { logError } from '../utils/errorLogger';

interface UserWithRoles {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  createdAt: string;
  roles: string[];
}

interface Role {
  id: number;
  name: string;
  description: string;
}

export default function AdminSettingsPage() {
  const [users, setUsers] = useState<UserWithRoles[]>([]);
  const [roles, setRoles] = useState<Role[]>([]);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [form] = Form.useForm();

  const fetchData = async () => {
    setLoading(true);
    try {
      const [usersRes, rolesRes] = await Promise.all([
        api.get('/users'),
        api.get('/roles'),
      ]);
      setUsers(usersRes.data);
      setRoles(rolesRes.data);
    } catch (err) {
      logError('Failed to load admin data', 'AdminSettingsPage', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const handleRoleChange = async (userId: number, selectedRoleIds: number[]) => {
    await api.put(`/users/${userId}/roles`, { roleIds: selectedRoleIds });
    fetchData();
  };

  const handleAddUser = async (values: { email: string; password: string; firstName: string; lastName: string }) => {
    setSubmitting(true);
    try {
      await api.post('/auth/register', values);
      notifySuccess('User created');
      setModalOpen(false);
      form.resetFields();
      fetchData();
    } catch {
      notifyError('Failed to create user');
    } finally {
      setSubmitting(false);
    }
  };

  const columns = [
    { title: 'Name', key: 'name', render: (_: unknown, r: UserWithRoles) => `${r.firstName} ${r.lastName}` },
    { title: 'Email', dataIndex: 'email', key: 'email' },
    {
      title: 'Roles', key: 'roles',
      render: (_: unknown, r: UserWithRoles) => (
        <Select
          mode="multiple"
          value={roles.filter(role => r.roles.includes(role.name)).map(r => r.id)}
          onChange={(val) => handleRoleChange(r.id, val)}
          style={{ minWidth: 160, width: '100%', maxWidth: 280 }}
          size="small"
          placeholder="Select roles"
        >
          {roles.map(role => (
            <Select.Option key={role.id} value={role.id}>{role.name}</Select.Option>
          ))}
        </Select>
      ),
    },
    {
      title: 'Status', dataIndex: 'isActive', key: 'isActive',
      render: (v: boolean) => <Tag color={v ? 'green' : 'red'}>{v ? 'Active' : 'Inactive'}</Tag>,
    },
  ];

  return (
    <>
      <div className="page-toolbar">
        <h2 style={{ margin: 0 }}>Settings</h2>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => setModalOpen(true)}>
          Add User
        </Button>
      </div>
      {loading ? (
        <Spin size="large" style={{ display: 'block', marginTop: 80 }} />
      ) : (
        <Card className="content-card" title="User Management" styles={{ body: { padding: 0 } }}>
          <Table dataSource={users} columns={columns} rowKey="id" pagination={false} scroll={{ x: 'max-content' }} />
        </Card>
      )}
      <Modal
        title="Create User Account"
        open={modalOpen}
        onCancel={() => setModalOpen(false)}
        onOk={() => form.submit()}
        confirmLoading={submitting}
      >
        <Form form={form} onFinish={handleAddUser} layout="vertical">
          <Form.Item name="firstName" label="First Name" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="lastName" label="Last Name" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="email" label="Email" rules={[{ required: true, type: 'email' }]}>
            <Input />
          </Form.Item>
          <Form.Item name="password" label="Password" rules={[{ required: true, min: 6 }]}>
            <Input.Password />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}
