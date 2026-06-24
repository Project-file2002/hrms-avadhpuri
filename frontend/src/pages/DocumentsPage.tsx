import { useEffect, useState } from 'react';
import { Card, Button, Modal, Form, Input, Select, Tag, Typography, Space, Table, message, DatePicker } from 'antd';
import { PlusOutlined, DeleteOutlined, FileOutlined, DownloadOutlined } from '@ant-design/icons';
import api from '../services/api';

const { Title, Text } = Typography;

const categories = ['Contract', 'ID Proof', 'Offer Letter', 'Tax Form', 'Policy', 'Other'];

function DocumentsPage() {
  const [docs, setDocs] = useState<any[]>([]);
  const [employees, setEmployees] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [modal, setModal] = useState(false);
  const [form] = Form.useForm();

  useEffect(() => {
    Promise.all([
      api.get('/documents'),
      api.get('/employees'),
    ]).then(([d, e]) => { setDocs(d.data); setEmployees(e.data); })
    .finally(() => setLoading(false));
  }, []);

  const fetchDocs = async () => { const res = await api.get('/documents'); setDocs(res.data); };

  const handleUpload = async (values: any) => {
    await api.post('/documents', { ...values, fileSize: values.fileSize || 0, fileType: values.fileName?.split('.').pop() || 'pdf' });
    message.success('Document uploaded');
    setModal(false); form.resetFields(); fetchDocs();
  };

  const handleDelete = async (id: number) => {
    await api.delete(`/documents/${id}`);
    fetchDocs();
  };

  const formatSize = (bytes: number) => {
    if (!bytes) return '-';
    const k = 1024; const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return `${parseFloat((bytes / Math.pow(k, i)).toFixed(1))} ${sizes[i]}`;
  };

  const columns = [
    { title: 'Title', dataIndex: 'title' },
    { title: 'Category', dataIndex: 'category', render: (c: string) => <Tag>{c}</Tag> },
    { title: 'File', render: (_: any, r: any) => <Space><FileOutlined /><Text>{r.fileName}</Text><Text type="secondary" style={{ fontSize: 12 }}>{formatSize(r.fileSize)}</Text></Space> },
    { title: 'Employee', dataIndex: 'employeeName', render: (v: string) => v || '-' },
    { title: 'Uploaded By', dataIndex: 'uploadedByName' },
    { title: 'Date', dataIndex: 'uploadedAt', render: (d: string) => new Date(d).toLocaleDateString() },
    { title: 'Expiry', dataIndex: 'expiryDate', render: (d: string) => d ? <Tag color={new Date(d) < new Date() ? 'red' : 'orange'}>{new Date(d).toLocaleDateString()}</Tag> : '-' },
    { title: 'Status', dataIndex: 'status', render: (s: string) => <Tag color={s === 'Active' ? 'green' : 'default'}>{s}</Tag> },
    {
      title: '', key: 'action', width: 100,
      render: (_: any, r: any) => (
        <Space>
          {r.filePath && <Button size="small" icon={<DownloadOutlined />} onClick={() => window.open(r.filePath)} />}
          <Button size="small" danger icon={<DeleteOutlined />} onClick={() => handleDelete(r.id)} />
        </Space>
      ),
    },
  ];

  return (
    <div>
      <div className="page-toolbar">
        <div className="page-toolbar-text">
          <Title level={4} style={{ margin: 0 }}>
            <FileOutlined style={{ marginRight: 8, color: '#6c5ce7' }} />
            Document Management
          </Title>
          <Text type="secondary">Upload and manage employee documents</Text>
        </div>
        <div className="page-toolbar-actions">
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setModal(true)}>Upload Document</Button>
        </div>
      </div>

      <Card style={{ borderRadius: 12 }}>
        <Table dataSource={docs} columns={columns} rowKey="id" loading={loading} scroll={{ x: 'max-content' }} />
      </Card>

      <Modal title="Upload Document" open={modal} onCancel={() => setModal(false)} onOk={() => form.submit()}>
        <Form form={form} onFinish={handleUpload} layout="vertical">
          <Form.Item name="title" label="Title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea rows={2} /></Form.Item>
          <Space style={{ width: '100%' }} size={12}>
            <Form.Item name="category" label="Category" initialValue="Contract" style={{ flex: 1 }}>
              <Select>{categories.map(c => <Select.Option key={c} value={c}>{c}</Select.Option>)}</Select>
            </Form.Item>
            <Form.Item name="fileName" label="File Name" rules={[{ required: true }]} style={{ flex: 1 }}><Input placeholder="e.g. offer-letter.pdf" /></Form.Item>
          </Space>
          <Form.Item name="employeeId" label="Employee">
            <Select allowClear showSearch placeholder="Link to employee (optional)"
              filterOption={(i, o) => (o?.label as string)?.toLowerCase()?.includes(i?.toLowerCase() ?? '')}
              options={employees.map((e: any) => ({ label: `${e.firstName} ${e.lastName}`, value: e.id }))}
            />
          </Form.Item>
          <Form.Item name="expiryDate" label="Expiry Date"><Input type="date" /></Form.Item>
          <Form.Item name="filePath" label="File Path / URL"><Input placeholder="e.g. /uploads/doc.pdf" /></Form.Item>
        </Form>
      </Modal>
    </div>
  );
}

export default DocumentsPage;
