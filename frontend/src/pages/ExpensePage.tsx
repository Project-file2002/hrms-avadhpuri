import { useEffect, useState } from 'react';
import {
  Card, Table, Button, Modal, Form, Input, Select, Tag, Typography,
  Space, Popconfirm, Statistic, Row, Col, message, DatePicker, Descriptions
} from 'antd';
import { formatINR, INR_PREFIX } from '../utils/currency';
import { PlusOutlined, MoneyCollectOutlined, CheckCircleOutlined, CloseCircleOutlined, DeleteOutlined, EditOutlined } from '@ant-design/icons';
import api from '../services/api';
import { useRoles } from '../utils/roles';

const { Title, Text } = Typography;

interface LineItem {
  category: string;
  description: string;
  amount: number;
  expenseDate: string;
}

interface ExpenseReport {
  id: number;
  employeeId: number;
  title: string;
  description?: string;
  totalAmount: number;
  status: string;
  createdAt: string;
  employeeName: string;
  reviewedByName?: string;
  reviewNotes?: string;
  lineItems: { id: number; category: string; description: string; amount: number; expenseDate: string }[];
}

const statusColors: Record<string, string> = {
  Pending: 'orange', Approved: 'green', Rejected: 'red', Paid: 'blue',
};

const categories = ['Travel', 'Food', 'Office', 'Equipment', 'Other'];

function ExpensePage() {
  const [reports, setReports] = useState<ExpenseReport[]>([]);
  const [loading, setLoading] = useState(true);
  const [modal, setModal] = useState(false);
  const [detailModal, setDetailModal] = useState<ExpenseReport | null>(null);
  const [editModal, setEditModal] = useState(false);
  const [editingReport, setEditingReport] = useState<ExpenseReport | null>(null);
  const [lineItems, setLineItems] = useState<LineItem[]>([]);
  const [editLineItems, setEditLineItems] = useState<LineItem[]>([]);
  const [form] = Form.useForm();
  const [editForm] = Form.useForm();
  const [rejectOpen, setRejectOpen] = useState(false);
  const [rejectExpenseId, setRejectExpenseId] = useState<number | null>(null);
  const [rejectReason, setRejectReason] = useState('');
  const { user, canApproveExpense } = useRoles();

  useEffect(() => { fetchReports(); }, []);

  const fetchReports = async () => {
    try {
      const res = await api.get('/expense');
      setReports(res.data);
    } finally { setLoading(false); }
  };

  const addLineItem = () => {
    setLineItems([...lineItems, { category: 'Other', description: '', amount: 0, expenseDate: new Date().toISOString() }]);
  };

  const updateLineItem = (i: number, field: string, value: any) => {
    const items = [...lineItems];
    (items[i] as any)[field] = value;
    setLineItems(items);
  };

  const removeLineItem = (i: number) => setLineItems(lineItems.filter((_, idx) => idx !== i));

  const handleCreate = async (values: any) => {
    if (lineItems.length === 0) { message.warning('Add at least one line item'); return; }
    await api.post('/expense', {
      title: values.title, description: values.description, lineItems
    });
    message.success('Expense report submitted!');
    setModal(false); form.resetFields(); setLineItems([]);
    fetchReports();
  };

  const isOwnReport = (r: ExpenseReport) => user?.employeeId === r.employeeId;

  const openEditModal = (r: ExpenseReport) => {
    setEditingReport(r);
    editForm.setFieldsValue({ title: r.title, description: r.description });
    setEditLineItems(r.lineItems.map(li => ({
      category: li.category, description: li.description,
      amount: li.amount, expenseDate: li.expenseDate,
    })));
    setEditModal(true);
  };

  const handleEdit = async (values: any) => {
    if (editLineItems.length === 0) { message.warning('Add at least one line item'); return; }
    await api.put(`/expense/${editingReport?.id}`, {
      title: values.title, description: values.description, lineItems: editLineItems,
    });
    message.success('Expense report updated');
    setEditModal(false); setEditingReport(null); editForm.resetFields(); setEditLineItems([]);
    fetchReports();
  };

  const handleDeleteReport = async (id: number) => {
    await api.delete(`/expense/${id}`);
    message.success('Expense report deleted');
    fetchReports();
  };

  const handleApprove = async (id: number, approved: boolean, notes?: string) => {
    await api.put(`/expense/${id}/approve`, { approved, notes: notes ?? '' });
    message.success(approved ? 'Approved!' : 'Rejected');
    fetchReports();
  };

  const openRejectModal = (id: number) => {
    setRejectExpenseId(id);
    setRejectReason('');
    setRejectOpen(true);
  };

  const confirmReject = async () => {
    if (rejectExpenseId == null) return;
    await handleApprove(rejectExpenseId, false, rejectReason);
    setRejectOpen(false);
    setRejectExpenseId(null);
    setRejectReason('');
  };

  const total = lineItems.reduce((s, li) => s + (li.amount || 0), 0);

  const columns = [
    { title: 'Title', dataIndex: 'title', key: 'title' },
    { title: 'Employee', dataIndex: 'employeeName', key: 'emp' },
    { title: 'Amount', dataIndex: 'totalAmount', key: 'amount', render: (v: number) => formatINR(v) },
    { title: 'Status', dataIndex: 'status', key: 'status', render: (s: string) => <Tag color={statusColors[s]}>{s}</Tag> },
    { title: 'Date', dataIndex: 'createdAt', key: 'date', render: (d: string) => new Date(d).toLocaleDateString() },
    {
      title: 'Action', key: 'action', width: 220,
      render: (_: any, r: ExpenseReport) => (
        <Space>
          <Button size="small" onClick={() => setDetailModal(r)}>View</Button>
          {r.status === 'Pending' && isOwnReport(r) && (
            <>
              <Button size="small" icon={<EditOutlined />} onClick={() => openEditModal(r)}>Edit</Button>
              <Popconfirm title="Delete this expense report?" onConfirm={() => handleDeleteReport(r.id)} okText="Delete" okButtonProps={{ danger: true }}>
                <Button size="small" danger icon={<DeleteOutlined />}>Delete</Button>
              </Popconfirm>
            </>
          )}
          {r.status === 'Pending' && canApproveExpense && !isOwnReport(r) && (
            <>
              <Button size="small" type="primary" icon={<CheckCircleOutlined />} onClick={() => handleApprove(r.id, true)} />
              <Button size="small" danger icon={<CloseCircleOutlined />} onClick={() => openRejectModal(r.id)} />
            </>
          )}
        </Space>
      ),
    },
  ];

  return (
    <div>
      <div className="page-toolbar">
        <div className="page-toolbar-text">
          <Title level={4} style={{ margin: 0 }}>
            <MoneyCollectOutlined style={{ marginRight: 8, color: '#6c5ce7' }} />
            Expense Management
          </Title>
          <Text type="secondary">Submit and manage expense reports</Text>
        </div>
        <div className="page-toolbar-actions">
          <Button type="primary" icon={<PlusOutlined />} onClick={() => { setModal(true); setLineItems([]); }}>New Report</Button>
        </div>
      </div>

      <Card style={{ borderRadius: 12 }}>
        <Table dataSource={reports} columns={columns} rowKey="id" loading={loading} pagination={false} scroll={{ x: 'max-content' }} />
      </Card>

      <Modal title="New Expense Report" open={modal} onCancel={() => setModal(false)} onOk={() => form.submit()} width="100%" style={{ maxWidth: 600 }}>
        <Form form={form} onFinish={handleCreate} layout="vertical">
          <Form.Item name="title" label="Title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea rows={2} /></Form.Item>
        </Form>
        <Text strong>Line Items</Text>
        {lineItems.map((li, i) => (
          <Card key={i} size="small" style={{ marginTop: 8, borderRadius: 8 }} extra={
            <Button type="text" danger icon={<DeleteOutlined />} onClick={() => removeLineItem(i)} />
          }>
            <Space orientation="vertical" style={{ width: '100%' }}>
              <Select value={li.category} onChange={v => updateLineItem(i, 'category', v)} style={{ width: '100%' }}>
                {categories.map(c => <Select.Option key={c} value={c}>{c}</Select.Option>)}
              </Select>
              <Input placeholder="Description" value={li.description} onChange={e => updateLineItem(i, 'description', e.target.value)} />
              <Space>
                <Input type="number" placeholder="Amount" value={li.amount || ''} onChange={e => updateLineItem(i, 'amount', parseFloat(e.target.value) || 0)} style={{ width: 150 }} />
                <DatePicker value={li.expenseDate ? new Date(li.expenseDate) : null} onChange={d => updateLineItem(i, 'expenseDate', d?.toISOString() || new Date().toISOString())} />
              </Space>
            </Space>
          </Card>
        ))}
        <div style={{ marginTop: 8 }}>
          <Text strong>Total: {formatINR(total)}</Text>
        </div>
        <Button type="dashed" onClick={addLineItem} block style={{ marginTop: 8 }}>+ Add Line Item</Button>
      </Modal>

      <Modal title="Edit Expense Report" open={editModal} onCancel={() => { setEditModal(false); setEditingReport(null); setEditLineItems([]); }} onOk={() => editForm.submit()} width="100%" style={{ maxWidth: 600 }}>
        <Form form={editForm} onFinish={handleEdit} layout="vertical">
          <Form.Item name="title" label="Title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea rows={2} /></Form.Item>
        </Form>
        <Text strong>Line Items</Text>
        {editLineItems.map((li, i) => (
          <Card key={i} size="small" style={{ marginTop: 8, borderRadius: 8 }} extra={
            <Button type="text" danger icon={<DeleteOutlined />} onClick={() => setEditLineItems(editLineItems.filter((_, idx) => idx !== i))} />
          }>
            <Space orientation="vertical" style={{ width: '100%' }}>
              <Select value={li.category} onChange={v => { const items = [...editLineItems]; items[i].category = v; setEditLineItems(items); }} style={{ width: '100%' }}>
                {categories.map(c => <Select.Option key={c} value={c}>{c}</Select.Option>)}
              </Select>
              <Input placeholder="Description" value={li.description} onChange={e => { const items = [...editLineItems]; items[i].description = e.target.value; setEditLineItems(items); }} />
              <Space>
                <Input type="number" placeholder="Amount" value={li.amount || ''} onChange={e => { const items = [...editLineItems]; items[i].amount = parseFloat(e.target.value) || 0; setEditLineItems(items); }} style={{ width: 150 }} />
                <DatePicker value={li.expenseDate ? new Date(li.expenseDate) : null} onChange={d => { const items = [...editLineItems]; items[i].expenseDate = d?.toISOString() || new Date().toISOString(); setEditLineItems(items); }} />
              </Space>
            </Space>
          </Card>
        ))}
        <div style={{ marginTop: 8 }}>
          <Text strong>Total: {formatINR(editLineItems.reduce((s, li) => s + (li.amount || 0), 0))}</Text>
        </div>
        <Button type="dashed" onClick={() => setEditLineItems([...editLineItems, { category: 'Other', description: '', amount: 0, expenseDate: new Date().toISOString() }])} block style={{ marginTop: 8 }}>+ Add Line Item</Button>
      </Modal>

      <Modal title="Expense Details" open={!!detailModal} onCancel={() => setDetailModal(null)} footer={null} width={500}>
        {detailModal && (
          <>
            <Descriptions column={1} bordered size="small">
              <Descriptions.Item label="Title">{detailModal.title}</Descriptions.Item>
              <Descriptions.Item label="Employee">{detailModal.employeeName}</Descriptions.Item>
              <Descriptions.Item label="Status"><Tag color={statusColors[detailModal.status]}>{detailModal.status}</Tag></Descriptions.Item>
              <Descriptions.Item label="Total">{formatINR(detailModal.totalAmount)}</Descriptions.Item>
              <Descriptions.Item label="Reviewed By">{detailModal.reviewedByName || '-'}</Descriptions.Item>
              <Descriptions.Item label="Notes">{detailModal.reviewNotes || '-'}</Descriptions.Item>
            </Descriptions>
            <Divider />
            <Text strong>Line Items</Text>
            {detailModal.lineItems.map((li, i) => (
              <div key={li.id} style={{ display: 'flex', justifyContent: 'space-between', padding: '4px 0' }}>
                <Space>
                  <Tag color="purple">{li.category}</Tag>
                  <Text>{li.description}</Text>
                </Space>
                <Text strong>{formatINR(li.amount)}</Text>
              </div>
            ))}
          </>
        )}
      </Modal>

      <Modal
        title="Reject Expense Report"
        open={rejectOpen}
        onCancel={() => { setRejectOpen(false); setRejectExpenseId(null); setRejectReason(''); }}
        onOk={confirmReject}
        okText="Reject"
        okButtonProps={{ danger: true }}
      >
        <p style={{ marginBottom: 12 }}>Are you sure you want to reject this expense report?</p>
        <Input.TextArea
          placeholder="Please provide a reason for rejection..."
          value={rejectReason}
          onChange={e => setRejectReason(e.target.value)}
          rows={3}
        />
      </Modal>
    </div>
  );
}

export default ExpensePage;

const Divider = ({ children }: { children?: React.ReactNode }) => (
  <div style={{ borderTop: '1px solid #f0f0f0', margin: '16px 0' }} />
);
