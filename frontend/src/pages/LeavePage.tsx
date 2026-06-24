import { useEffect, useState } from 'react';
import { Table, Button, Modal, Form, Input, DatePicker, Select, Tag, Space, Popconfirm, message, Drawer } from 'antd';
import { PlusOutlined, CheckCircleOutlined, CloseCircleOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import type { LeaveRequest, LeaveType } from '../types';
import api from '../services/api';
import { notifySuccess } from '../utils/notification';
import { useRoles } from '../utils/roles';

export default function LeavePage() {
  const { user, canViewAllHrData, canApproveLeave } = useRoles();
  const [leaves, setLeaves] = useState<LeaveRequest[]>([]);
  const [leaveTypes, setLeaveTypes] = useState<LeaveType[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [editingLeave, setEditingLeave] = useState<LeaveRequest | null>(null);
  const [detailVisible, setDetailVisible] = useState(false);
  const [selectedLeave, setSelectedLeave] = useState<LeaveRequest | null>(null);
  const [form] = Form.useForm();
  const [editForm] = Form.useForm();
  const [rejectOpen, setRejectOpen] = useState(false);
  const [rejectLeaveId, setRejectLeaveId] = useState<number | null>(null);
  const [rejectReason, setRejectReason] = useState('');

  const fetchLeaves = async () => {
    setLoading(true);
    try {
      const params = !canViewAllHrData && user?.employeeId
        ? { employeeId: user.employeeId }
        : undefined;
      const res = await api.get('/leave', { params });
      setLeaves(res.data);
    } finally {
      setLoading(false);
    }
  };

  const fetchLeaveTypes = async () => {
    const res = await api.get('/leave/types');
    setLeaveTypes(res.data);
  };

  useEffect(() => { fetchLeaves(); fetchLeaveTypes(); }, [canViewAllHrData, user?.employeeId]);

  const handleCreate = async (values: { leaveTypeId: number; startDate: unknown; endDate: unknown; reason: string }) => {
    await api.post('/leave', {
      ...values,
      startDate: values.startDate,
      endDate: values.endDate,
    });
    notifySuccess('Leave request submitted');
    setModalOpen(false);
    form.resetFields();
    fetchLeaves();
  };

  const isOwnLeave = (leave: LeaveRequest) => user?.employeeId === leave.employeeId;

  const showDetail = (record: LeaveRequest) => {
    setSelectedLeave(record);
    setDetailVisible(true);
  };

  const openEditModal = (record: LeaveRequest) => {
    setEditingLeave(record);
    editForm.setFieldsValue({
      leaveTypeId: record.leaveTypeId,
      startDate: null,
      endDate: null,
      reason: record.reason,
    });
    setEditModalOpen(true);
  };

  const handleEdit = async (values: any) => {
    await api.put(`/leave/${editingLeave?.id}`, {
      ...values,
      startDate: values.startDate?.toISOString?.(),
      endDate: values.endDate?.toISOString?.(),
    });
    message.success('Leave request updated');
    setEditModalOpen(false);
    setEditingLeave(null);
    editForm.resetFields();
    fetchLeaves();
  };

  const handleDelete = async (id: number) => {
    await api.delete(`/leave/${id}`);
    message.success('Leave request cancelled');
    fetchLeaves();
  };

  const handleApprove = async (id: number, status: 'Approved' | 'Rejected', reason?: string) => {
    await api.put(`/leave/${id}/approve`, { status, reviewNotes: reason ?? '' });
    notifySuccess(status === 'Approved' ? 'Leave approved' : 'Leave rejected');
    fetchLeaves();
  };

  const openRejectModal = (id: number) => {
    setRejectLeaveId(id);
    setRejectReason('');
    setRejectOpen(true);
  };

  const confirmReject = async () => {
    if (rejectLeaveId == null) return;
    await handleApprove(rejectLeaveId, 'Rejected', rejectReason);
    setRejectOpen(false);
    setRejectLeaveId(null);
    setRejectReason('');
  };

  const statusColor: Record<string, string> = {
    Pending: 'orange',
    Approved: 'green',
    Rejected: 'red',
    Canceled: 'default',
  };

  const columns = [
    ...(canViewAllHrData ? [{ title: 'Employee', dataIndex: 'employeeName', key: 'employeeName' }] : []),
    { title: 'Type', dataIndex: 'leaveTypeName', key: 'leaveTypeName' },
    { title: 'Start', dataIndex: 'startDate', key: 'startDate', render: (v: string) => new Date(v).toLocaleDateString() },
    { title: 'End', dataIndex: 'endDate', key: 'endDate', render: (v: string) => new Date(v).toLocaleDateString() },
    { title: 'Reason', dataIndex: 'reason', key: 'reason' },
    {
      title: 'Status', dataIndex: 'status', key: 'status',
      render: (s: string) => <Tag color={statusColor[s]}>{s}</Tag>,
    },
    {
      title: 'Actions', key: 'actions', width: 200,
      render: (_: unknown, r: LeaveRequest) => (
        <Space>
          <Button size="small" onClick={() => showDetail(r)}>View</Button>
          {r.status === 'Pending' && isOwnLeave(r) && (
            <Button size="small" icon={<EditOutlined />} onClick={() => openEditModal(r)}>Edit</Button>
          )}
          {r.status === 'Pending' && isOwnLeave(r) && (
            <Popconfirm title="Cancel this leave request?" onConfirm={() => handleDelete(r.id)} okText="Cancel" okButtonProps={{ danger: true }}>
              <Button size="small" danger icon={<DeleteOutlined />}>Cancel</Button>
            </Popconfirm>
          )}
          {r.status === 'Pending' && canApproveLeave && !isOwnLeave(r) && (
            <Space>
              <Button size="small" type="primary" icon={<CheckCircleOutlined />} onClick={() => handleApprove(r.id, 'Approved')} />
              <Button size="small" danger icon={<CloseCircleOutlined />} onClick={() => openRejectModal(r.id)} />
            </Space>
          )}
        </Space>
      ),
    },
  ];

  return (
    <>
      <div className="page-toolbar">
        <h2 style={{ margin: 0 }}>Leave Requests</h2>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => setModalOpen(true)}>
          New Request
        </Button>
      </div>
      <div className="responsive-table-wrap">
      <Table dataSource={leaves} columns={columns} rowKey="id" loading={loading} scroll={{ x: 'max-content' }} />
      </div>
      <Modal title="New Leave Request" open={modalOpen} onCancel={() => setModalOpen(false)} onOk={() => form.submit()}>
        <Form form={form} onFinish={handleCreate} layout="vertical">
          <Form.Item name="leaveTypeId" label="Leave Type" rules={[{ required: true }]}>
            <Select>
              {leaveTypes.map((lt) => (
                <Select.Option key={lt.id} value={lt.id}>{lt.name}</Select.Option>
              ))}
            </Select>
          </Form.Item>
          <Form.Item name="startDate" label="Start Date" rules={[{ required: true }]}>
            <DatePicker style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="endDate" label="End Date" rules={[{ required: true }]}>
            <DatePicker style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="reason" label="Reason" rules={[{ required: true }]}>
            <Input.TextArea />
          </Form.Item>
        </Form>
      </Modal>

      <Drawer title="Leave Details" open={detailVisible} onClose={() => { setDetailVisible(false); setSelectedLeave(null); }} styles={{ wrapper: { width: 480 } }}>
        {selectedLeave && (
          <Space direction="vertical" style={{ width: '100%' }}>
            <div><strong>Employee:</strong> {selectedLeave.employeeName}</div>
            <div><strong>Leave Type:</strong> {selectedLeave.leaveTypeName}</div>
            <div><strong>Start:</strong> {new Date(selectedLeave.startDate).toLocaleDateString()}</div>
            <div><strong>End:</strong> {new Date(selectedLeave.endDate).toLocaleDateString()}</div>
            <div><strong>Status:</strong> <Tag color={statusColor[selectedLeave.status]}>{selectedLeave.status}</Tag></div>
            <div><strong>Reason:</strong></div>
            <div style={{ whiteSpace: 'pre-wrap' }}>{selectedLeave.reason || 'No reason provided'}</div>
          </Space>
        )}
      </Drawer>

      <Modal title="Edit Leave Request" open={editModalOpen} onCancel={() => { setEditModalOpen(false); setEditingLeave(null); }} onOk={() => editForm.submit()}>
        <Form form={editForm} onFinish={handleEdit} layout="vertical">
          <Form.Item name="leaveTypeId" label="Leave Type" rules={[{ required: true }]}>
            <Select>
              {leaveTypes.map((lt) => (
                <Select.Option key={lt.id} value={lt.id}>{lt.name}</Select.Option>
              ))}
            </Select>
          </Form.Item>
          <Form.Item name="startDate" label="Start Date" rules={[{ required: true }]}>
            <DatePicker style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="endDate" label="End Date" rules={[{ required: true }]}>
            <DatePicker style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="reason" label="Reason" rules={[{ required: true }]}>
            <Input.TextArea />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title="Reject Leave Request"
        open={rejectOpen}
        onCancel={() => { setRejectOpen(false); setRejectLeaveId(null); setRejectReason(''); }}
        onOk={confirmReject}
        okText="Reject"
        okButtonProps={{ danger: true }}
      >
        <p style={{ marginBottom: 12 }}>Are you sure you want to reject this leave request?</p>
        <Input.TextArea
          placeholder="Please provide a reason for rejection..."
          value={rejectReason}
          onChange={e => setRejectReason(e.target.value)}
          rows={3}
        />
      </Modal>
    </>
  );
}
