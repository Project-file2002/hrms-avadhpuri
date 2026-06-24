import { useEffect, useState } from 'react';
import { Table, Card, Row, Col, Button, Modal, Form, Input, Select, Space, Popconfirm, message, Drawer, Tag } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import api from '../services/api';
import { useRoles } from '../utils/roles';

interface ReviewItem {
  id: number;
  title: string;
  status: string;
  overallScore?: number;
  employeeId: number;
  employeeName?: string;
  reviewerName?: string;
  cycleId: number;
  cycleName?: string;
  comments?: string;
  scores?: { criteria: string; score: number; comments?: string }[];
}

interface CycleItem {
  id: number;
  name: string;
  startDate: string;
  endDate: string;
  status: string;
}

export default function PerformancePage() {
  const { isHr } = useRoles();
  const [reviews, setReviews] = useState<ReviewItem[]>([]);
  const [cycles, setCycles] = useState<CycleItem[]>([]);
  const [employees, setEmployees] = useState<{ id: number; firstName: string; lastName: string }[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [editingReview, setEditingReview] = useState<ReviewItem | null>(null);
  const [detailVisible, setDetailVisible] = useState(false);
  const [selectedReview, setSelectedReview] = useState<ReviewItem | null>(null);
  const [form] = Form.useForm();
  const [editForm] = Form.useForm();

  const fetchData = async () => {
    setLoading(true);
    try {
      const [reviewsRes, cyclesRes, empRes] = await Promise.all([
        api.get('/performance/reviews'),
        api.get('/performance/cycles'),
        api.get('/employees'),
      ]);
      setReviews(reviewsRes.data);
      setCycles(cyclesRes.data);
      setEmployees(empRes.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const handleCreate = async (values: any) => {
    await api.post('/performance/reviews', values);
    message.success('Review created');
    setModalOpen(false);
    form.resetFields();
    fetchData();
  };

  const showDetail = (record: ReviewItem) => {
    setSelectedReview(record);
    setDetailVisible(true);
  };

  const openEditModal = (record: ReviewItem) => {
    setEditingReview(record);
    editForm.setFieldsValue({
      title: record.title,
      comments: record.comments,
      status: record.status,
      cycleId: record.cycleId,
    });
    setEditModalOpen(true);
  };

  const handleEdit = async (values: any) => {
    await api.put(`/performance/reviews/${editingReview?.id}`, values);
    message.success('Review updated');
    setEditModalOpen(false);
    setEditingReview(null);
    editForm.resetFields();
    fetchData();
  };

  const handleDelete = async (id: number) => {
    await api.delete(`/performance/reviews/${id}`);
    message.success('Review deleted');
    fetchData();
  };

  const statusColors: Record<string, string> = {
    Pending: 'orange', 'In Progress': 'blue', Completed: 'green',
  };

  const reviewColumns = [
    { title: 'Title', dataIndex: 'title', key: 'title', render: (t: string, r: ReviewItem) => <a onClick={() => showDetail(r)}>{t}</a> },
    { title: 'Employee', dataIndex: 'employeeName', key: 'employeeName' },
    { title: 'Reviewer', dataIndex: 'reviewerName', key: 'reviewerName' },
    { title: 'Status', dataIndex: 'status', key: 'status', render: (s: string) => <Tag color={statusColors[s]}>{s}</Tag> },
    { title: 'Score', dataIndex: 'overallScore', key: 'overallScore', render: (v: number | null) => v?.toFixed(1) ?? '-' },
    ...(isHr ? [{
      title: 'Actions', key: 'actions', width: 160,
      render: (_: unknown, r: ReviewItem) => (
        <Space>
          <Button size="small" icon={<EditOutlined />} onClick={() => openEditModal(r)}>Edit</Button>
          <Popconfirm title="Delete this review?" onConfirm={() => handleDelete(r.id)} okText="Delete" okButtonProps={{ danger: true }}>
            <Button size="small" danger icon={<DeleteOutlined />}>Delete</Button>
          </Popconfirm>
        </Space>
      ),
    }] : []),
  ];

  return (
    <>
      <div className="page-toolbar">
        <h2 style={{ margin: 0 }}>Performance Management</h2>
        {isHr && (
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setModalOpen(true)}>New Review</Button>
        )}
      </div>
      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        {cycles.map((c) => (
          <Col xs={24} sm={12} lg={6} key={c.id}>
            <Card size="small" title={c.name}>
              <p>Status: {c.status}</p>
              <p>{c.startDate?.split('T')[0]} - {c.endDate?.split('T')[0]}</p>
            </Card>
          </Col>
        ))}
      </Row>
      <div className="responsive-table-wrap">
      <Table dataSource={reviews} columns={reviewColumns} rowKey="id" loading={loading} scroll={{ x: 'max-content' }} />
      </div>

      <Modal title="New Performance Review" open={modalOpen} onCancel={() => setModalOpen(false)} onOk={() => form.submit()}>
        <Form form={form} onFinish={handleCreate} layout="vertical">
          <Form.Item name="title" label="Title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="employeeId" label="Employee" rules={[{ required: true }]}>
            <Select showSearch placeholder="Select employee"
              options={employees.map(e => ({ value: e.id, label: `${e.firstName} ${e.lastName}` }))} />
          </Form.Item>
          <Form.Item name="cycleId" label="Review Cycle" rules={[{ required: true }]}>
            <Select placeholder="Select cycle"
              options={cycles.map(c => ({ value: c.id, label: c.name }))} />
          </Form.Item>
        </Form>
      </Modal>

      <Drawer title={selectedReview?.title} open={detailVisible} onClose={() => { setDetailVisible(false); setSelectedReview(null); }} styles={{ wrapper: { width: 520 } }}>
        {selectedReview && (
          <Space direction="vertical" style={{ width: '100%' }}>
            <div><strong>Employee:</strong> {selectedReview.employeeName}</div>
            <div><strong>Reviewer:</strong> {selectedReview.reviewerName}</div>
            <div><strong>Cycle:</strong> {selectedReview.cycleName}</div>
            <div><strong>Status:</strong> <Tag color={statusColors[selectedReview.status]}>{selectedReview.status}</Tag></div>
            <div><strong>Score:</strong> {selectedReview.overallScore?.toFixed(1) ?? '-'}</div>
            {selectedReview.comments && <div><strong>Comments:</strong></div>}
            {selectedReview.comments && <div style={{ whiteSpace: 'pre-wrap' }}>{selectedReview.comments}</div>}
            {selectedReview.scores && selectedReview.scores.length > 0 && (
              <>
                <div><strong>Scores:</strong></div>
                {selectedReview.scores.map((s, i) => (
                  <div key={i} style={{ display: 'flex', justifyContent: 'space-between', padding: '2px 0' }}>
                    <span>{s.criteria}</span><Tag color="blue">{s.score}</Tag>
                  </div>
                ))}
              </>
            )}
          </Space>
        )}
      </Drawer>

      <Modal title="Edit Performance Review" open={editModalOpen} onCancel={() => { setEditModalOpen(false); setEditingReview(null); }} onOk={() => editForm.submit()}>
        <Form form={editForm} onFinish={handleEdit} layout="vertical">
          <Form.Item name="title" label="Title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="status" label="Status">
            <Select>
              <Select.Option value="Pending">Pending</Select.Option>
              <Select.Option value="In Progress">In Progress</Select.Option>
              <Select.Option value="Completed">Completed</Select.Option>
            </Select>
          </Form.Item>
          <Form.Item name="cycleId" label="Review Cycle">
            <Select placeholder="Select cycle"
              options={cycles.map(c => ({ value: c.id, label: c.name }))} />
          </Form.Item>
          <Form.Item name="comments" label="Comments"><Input.TextArea rows={4} /></Form.Item>
        </Form>
      </Modal>
    </>
  );
}
