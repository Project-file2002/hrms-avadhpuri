import { useEffect, useState } from 'react';
import { Table, Button, Modal, Form, Input, Select, Space, Popconfirm, message, Drawer, Tag } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import type { Department } from '../types';
import api from '../services/api';
import { notifySuccess } from '../utils/notification';
import { useRoles } from '../utils/roles';

export default function DepartmentListPage() {
  const { canManageDepartments } = useRoles();
  const [departments, setDepartments] = useState<Department[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [editingDepartment, setEditingDepartment] = useState<Department | null>(null);
  const [detailVisible, setDetailVisible] = useState(false);
  const [selectedDept, setSelectedDept] = useState<Department | null>(null);
  const [form] = Form.useForm();
  const [editForm] = Form.useForm();

  const fetchDepartments = async () => {
    setLoading(true);
    try {
      const res = await api.get('/departments');
      setDepartments(res.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchDepartments(); }, []);

  const handleCreate = async (values: { name: string; description?: string }) => {
    await api.post('/departments', values);
    notifySuccess('Department created');
    setModalOpen(false);
    form.resetFields();
    fetchDepartments();
  };

  const showDetail = (record: Department) => {
    setSelectedDept(record);
    setDetailVisible(true);
  };

  const openEditModal = (record: Department) => {
    setEditingDepartment(record);
    editForm.setFieldsValue({
      name: record.name,
      description: record.description,
    });
    setEditModalOpen(true);
  };

  const handleEdit = async (values: any) => {
    await api.put(`/departments/${editingDepartment?.id}`, values);
    message.success('Department updated');
    setEditModalOpen(false);
    setEditingDepartment(null);
    editForm.resetFields();
    fetchDepartments();
  };

  const handleDelete = async (id: number) => {
    await api.delete(`/departments/${id}`);
    message.success('Department deleted');
    fetchDepartments();
  };

  const columns = [
    { title: 'Name', dataIndex: 'name', key: 'name', render: (t: string, r: Department) => <a onClick={() => showDetail(r)}>{t}</a> },
    { title: 'Description', dataIndex: 'description', key: 'description' },
    { title: 'Head', dataIndex: 'headName', key: 'headName' },
    { title: 'Employees', dataIndex: 'employeeCount', key: 'employeeCount' },
    ...(canManageDepartments ? [{
      title: 'Actions', key: 'actions', width: 160,
      render: (_: unknown, r: Department) => (
        <Space>
          <Button size="small" icon={<EditOutlined />} onClick={() => openEditModal(r)}>Edit</Button>
          <Popconfirm title="Delete this department?" onConfirm={() => handleDelete(r.id)} okText="Delete" okButtonProps={{ danger: true }}>
            <Button size="small" danger icon={<DeleteOutlined />}>Delete</Button>
          </Popconfirm>
        </Space>
      ),
    }] : []),
  ];

  return (
    <>
      <div className="page-toolbar">
        <h2 style={{ margin: 0 }}>Departments</h2>
        {canManageDepartments && (
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setModalOpen(true)}>
            Add Department
          </Button>
        )}
      </div>
      <div className="responsive-table-wrap">
      <Table dataSource={departments} columns={columns} rowKey="id" loading={loading} scroll={{ x: 'max-content' }} />
      </div>
      <Modal title="Add Department" open={modalOpen} onCancel={() => setModalOpen(false)} onOk={() => form.submit()}>
        <Form form={form} onFinish={handleCreate} layout="vertical">
          <Form.Item name="name" label="Name" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="description" label="Description">
            <Input.TextArea />
          </Form.Item>
        </Form>
      </Modal>

      <Drawer title={selectedDept?.name} open={detailVisible} onClose={() => { setDetailVisible(false); setSelectedDept(null); }} styles={{ wrapper: { width: 480 } }}>
        {selectedDept && (
          <Space direction="vertical" style={{ width: '100%' }}>
            <div><strong>Head:</strong> {selectedDept.headName || '-'}</div>
            <div><strong>Employees:</strong> <Tag color="blue">{selectedDept.employeeCount}</Tag></div>
            <div><strong>Description:</strong></div>
            <div style={{ whiteSpace: 'pre-wrap' }}>{selectedDept.description || 'No description'}</div>
          </Space>
        )}
      </Drawer>

      <Modal title="Edit Department" open={editModalOpen} onCancel={() => { setEditModalOpen(false); setEditingDepartment(null); }} onOk={() => editForm.submit()}>
        <Form form={editForm} onFinish={handleEdit} layout="vertical">
          <Form.Item name="name" label="Name" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="description" label="Description">
            <Input.TextArea />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}
