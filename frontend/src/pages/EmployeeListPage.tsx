import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Table, Button, Modal, Form, Input, Select, Space, Popconfirm, message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import type { Employee, CreateEmployeeRequest } from '../types';
import api from '../services/api';
import { notifySuccess } from '../utils/notification';
import { useRoles } from '../utils/roles';

export default function EmployeeListPage() {
  const { canManageEmployees } = useRoles();
  const navigate = useNavigate();
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [editingEmployee, setEditingEmployee] = useState<Employee | null>(null);
  const [departments, setDepartments] = useState<any[]>([]);
  const [form] = Form.useForm();
  const [editForm] = Form.useForm();

  const fetchEmployees = async () => {
    setLoading(true);
    try {
      const [empRes, deptRes] = await Promise.all([
        api.get('/employees'),
        api.get('/departments'),
      ]);
      setEmployees(empRes.data);
      setDepartments(deptRes.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchEmployees(); }, []);

  const handleCreate = async (values: CreateEmployeeRequest) => {
    await api.post('/employees', values);
    notifySuccess('Employee created');
    setModalOpen(false);
    form.resetFields();
    fetchEmployees();
  };

  const openEditModal = (record: Employee) => {
    setEditingEmployee(record);
    editForm.setFieldsValue({
      firstName: record.firstName,
      lastName: record.lastName,
      phone: record.phone,
      position: record.position,
      status: record.status,
      dateOfBirth: record.dateOfBirth,
      dateOfJoining: record.dateOfJoining,
      gender: record.gender,
      address: record.address,
      emergencyContactName: record.emergencyContactName,
      emergencyContactPhone: record.emergencyContactPhone,
    });
    setEditModalOpen(true);
  };

  const handleEdit = async (values: any) => {
    await api.put(`/employees/${editingEmployee?.id}`, values);
    message.success('Employee updated');
    setEditModalOpen(false);
    setEditingEmployee(null);
    editForm.resetFields();
    fetchEmployees();
  };

  const handleDelete = async (id: number) => {
    await api.delete(`/employees/${id}`);
    message.success('Employee deleted');
    fetchEmployees();
  };

  const columns = [
    { title: 'Code', dataIndex: 'employeeCode', key: 'employeeCode' },
    { title: 'Name', key: 'name', render: (_: unknown, r: Employee) => `${r.firstName} ${r.lastName}` },
    { title: 'Email', dataIndex: 'email', key: 'email' },
    { title: 'Position', dataIndex: 'position', key: 'position' },
    { title: 'Department', dataIndex: 'departmentName', key: 'departmentName' },
    { title: 'Status', dataIndex: 'status', key: 'status' },
    ...(canManageEmployees ? [{
      title: 'Actions', key: 'actions', width: 160,
      render: (_: unknown, r: Employee) => (
        <Space>
          <Button size="small" icon={<EditOutlined />} onClick={(e) => { e.stopPropagation(); openEditModal(r); }}>Edit</Button>
          <Popconfirm title="Delete this employee?" description="This action cannot be undone." onConfirm={() => handleDelete(r.id)} okText="Delete" okButtonProps={{ danger: true }} onCancel={(e?: any) => e?.stopPropagation?.()}>
            <Button size="small" danger icon={<DeleteOutlined />} onClick={(e) => e.stopPropagation()}>Delete</Button>
          </Popconfirm>
        </Space>
      ),
    }] : []),
  ];

  return (
    <>
      <div className="page-toolbar">
        <h2 style={{ margin: 0 }}>Employees</h2>
        {canManageEmployees && (
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setModalOpen(true)}>
            Add Employee
          </Button>
        )}
      </div>
      <div className="responsive-table-wrap">
      <Table
        dataSource={employees}
        columns={columns}
        rowKey="id"
        loading={loading}
        scroll={{ x: 'max-content' }}
        onRow={(record) => ({
          onClick: () => navigate(`/employees/${record.id}`),
          style: { cursor: 'pointer' },
        })}
      />
      </div>
      <Modal title="Add Employee" open={modalOpen} onCancel={() => setModalOpen(false)} onOk={() => form.submit()}>
        <Form form={form} onFinish={handleCreate} layout="vertical">
          <Space style={{ width: '100%' }} orientation="vertical">
            <Form.Item name="employeeCode" label="Code" rules={[{ required: true }]}>
              <Input />
            </Form.Item>
            <Space>
              <Form.Item name="firstName" label="First Name" rules={[{ required: true }]}>
                <Input />
              </Form.Item>
              <Form.Item name="lastName" label="Last Name" rules={[{ required: true }]}>
                <Input />
              </Form.Item>
            </Space>
            <Form.Item name="email" label="Email" rules={[{ required: true, type: 'email' }]}>
              <Input />
            </Form.Item>
            <Form.Item name="position" label="Position">
              <Input />
            </Form.Item>
            <Form.Item name="status" label="Status" initialValue="Active">
              <Select>
                <Select.Option value="Active">Active</Select.Option>
                <Select.Option value="Probation">Probation</Select.Option>
                <Select.Option value="Resigned">Resigned</Select.Option>
              </Select>
            </Form.Item>
            <Form.Item name="departmentId" label="Department">
              <Select allowClear placeholder="Select department">
                {departments.map((d: any) => <Select.Option key={d.id} value={d.id}>{d.name}</Select.Option>)}
              </Select>
            </Form.Item>
          </Space>
        </Form>
      </Modal>
      <Modal title="Edit Employee" open={editModalOpen} onCancel={() => { setEditModalOpen(false); setEditingEmployee(null); }} onOk={() => editForm.submit()}>
        <Form form={editForm} onFinish={handleEdit} layout="vertical">
          <Space style={{ width: '100%' }} orientation="vertical">
            <Space>
              <Form.Item name="firstName" label="First Name" rules={[{ required: true }]}>
                <Input />
              </Form.Item>
              <Form.Item name="lastName" label="Last Name" rules={[{ required: true }]}>
                <Input />
              </Form.Item>
            </Space>
            <Form.Item name="phone" label="Phone">
              <Input />
            </Form.Item>
            <Form.Item name="position" label="Position">
              <Input />
            </Form.Item>
            <Form.Item name="status" label="Status">
              <Select>
                <Select.Option value="Active">Active</Select.Option>
                <Select.Option value="Probation">Probation</Select.Option>
                <Select.Option value="Resigned">Resigned</Select.Option>
              </Select>
            </Form.Item>
            <Form.Item name="departmentId" label="Department">
              <Select allowClear placeholder="Select department">
                {departments.map((d: any) => <Select.Option key={d.id} value={d.id}>{d.name}</Select.Option>)}
              </Select>
            </Form.Item>
          </Space>
        </Form>
      </Modal>
    </>
  );
}
