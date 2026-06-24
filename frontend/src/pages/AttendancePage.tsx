import { useEffect, useState } from 'react';
import { Table, Button, Modal, Form, Input, DatePicker, TimePicker, Space, Popconfirm, message, Tag } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, ToolOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import type { AttendanceRecord } from '../types';
import api from '../services/api';
import { notifySuccess } from '../utils/notification';
import { useRoles } from '../utils/roles';

export default function AttendancePage() {
  const { user, canViewAllHrData } = useRoles();
  const [records, setRecords] = useState<AttendanceRecord[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [editingRecord, setEditingRecord] = useState<AttendanceRecord | null>(null);
  const [correctionModal, setCorrectionModal] = useState(false);
  const [form] = Form.useForm();
  const [editForm] = Form.useForm();
  const [correctionForm] = Form.useForm();

  const readyToFetch = canViewAllHrData || !!user?.employeeId;

  const fetchRecords = async () => {
    if (!readyToFetch) return;
    setLoading(true);
    try {
      const params = !canViewAllHrData && user?.employeeId
        ? { employeeId: user.employeeId }
        : undefined;
      const res = await api.get('/attendance', { params });
      setRecords(res.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchRecords(); }, [readyToFetch, canViewAllHrData, user?.employeeId]);

  const handleCreate = async (values: { date: unknown; checkInTime: unknown; checkOutTime: unknown }) => {
    await api.post('/attendance', values);
    notifySuccess('Attendance recorded');
    setModalOpen(false);
    form.resetFields();
    fetchRecords();
  };

  const openEditModal = (record: AttendanceRecord) => {
    setEditingRecord(record);
    editForm.setFieldsValue({
      date: record.date ? dayjs(record.date) : null,
      checkInTime: record.checkInTime ? dayjs(record.checkInTime) : null,
      checkOutTime: record.checkOutTime ? dayjs(record.checkOutTime) : null,
    });
    setEditModalOpen(true);
  };

  const handleEdit = async (values: any) => {
    await api.put(`/attendance/${editingRecord?.id}`, values);
    message.success('Attendance updated');
    setEditModalOpen(false);
    setEditingRecord(null);
    editForm.resetFields();
    fetchRecords();
  };

  const handleDelete = async (id: number) => {
    await api.delete(`/attendance/${id}`);
    message.success('Attendance deleted');
    fetchRecords();
  };

  const handleRequestCorrection = async (values: any) => {
    await api.post('/attendance/corrections', values);
    message.success('Correction request submitted');
    setCorrectionModal(false);
    correctionForm.resetFields();
  };

  const isOwnRecord = (r: AttendanceRecord) => !canViewAllHrData || user?.employeeId === r.employeeId;
  const statusColors: Record<string, string> = { Present: 'green', Late: 'orange', Absent: 'red', HalfDay: 'gold' };

  const columns = [
    ...(canViewAllHrData ? [{ title: 'Employee', dataIndex: 'employeeName', key: 'employeeName', render: (v: string) => v || '-' }] : []),
    { title: 'Date', dataIndex: 'date', key: 'date', render: (v: string) => v?.split('T')[0] },
    { title: 'Check In', dataIndex: 'checkInTime', key: 'checkInTime', render: (v: string) => v ? new Date(v).toLocaleTimeString() : '-' },
    { title: 'Check Out', dataIndex: 'checkOutTime', key: 'checkOutTime', render: (v: string) => v ? new Date(v).toLocaleTimeString() : '-' },
    { title: 'Status', dataIndex: 'status', key: 'status', render: (s: string) => <Tag color={statusColors[s]}>{s}</Tag> },
    {
      title: 'Actions', key: 'actions', width: 200,
      render: (_: unknown, r: AttendanceRecord) => (
        <Space>
          <Button size="small" icon={<EditOutlined />} onClick={() => openEditModal(r)}>Edit</Button>
          <Popconfirm title="Delete this record?" onConfirm={() => handleDelete(r.id)} okText="Delete" okButtonProps={{ danger: true }}>
            <Button size="small" danger icon={<DeleteOutlined />}>Delete</Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <>
      <div className="page-toolbar">
        <h2 style={{ margin: 0 }}>Attendance</h2>
        <Space>
          <Button icon={<ToolOutlined />} onClick={() => setCorrectionModal(true)}>Request Correction</Button>
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setModalOpen(true)}>Check In/Out</Button>
        </Space>
      </div>
      <div className="responsive-table-wrap">
      <Table dataSource={records} columns={columns} rowKey="id" loading={loading || !readyToFetch} scroll={{ x: 'max-content' }} />
      </div>
      <Modal title="Record Attendance" open={modalOpen} onCancel={() => setModalOpen(false)} onOk={() => form.submit()}>
        <Form form={form} onFinish={handleCreate} layout="vertical">
          <Form.Item name="date" label="Date" rules={[{ required: true }]}>
            <DatePicker style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="checkInTime" label="Check In">
            <TimePicker style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="checkOutTime" label="Check Out">
            <TimePicker style={{ width: '100%' }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal title="Edit Attendance" open={editModalOpen} onCancel={() => { setEditModalOpen(false); setEditingRecord(null); }} onOk={() => editForm.submit()}>
        <Form form={editForm} onFinish={handleEdit} layout="vertical">
          <Form.Item name="date" label="Date" rules={[{ required: true }]}>
            <DatePicker style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="checkInTime" label="Check In">
            <TimePicker style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="checkOutTime" label="Check Out">
            <TimePicker style={{ width: '100%' }} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal title="Request Correction" open={correctionModal} onCancel={() => setCorrectionModal(false)} onOk={() => correctionForm.submit()}>
        <Form form={correctionForm} onFinish={handleRequestCorrection} layout="vertical">
          <Form.Item name="date" label="Date" rules={[{ required: true }]}>
            <DatePicker style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="correctedCheckIn" label="Corrected Check In">
            <TimePicker style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="correctedCheckOut" label="Corrected Check Out">
            <TimePicker style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="reason" label="Reason" rules={[{ required: true }]}>
            <Input.TextArea rows={2} />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}
