import { useEffect, useState } from 'react';
import { Card, Button, Modal, Form, Input, Select, Tag, Typography, Space, Table, Row, Col, Statistic, message, InputNumber, Tabs, Checkbox, Popconfirm } from 'antd';
import { PlusOutlined, DeleteOutlined, LaptopOutlined, SwapOutlined, ToolOutlined } from '@ant-design/icons';
import api from '../services/api';
import { formatINR, INR_PREFIX } from '../utils/currency';

const { Title, Text } = Typography;

const statusColor: Record<string, string> = { Available: 'green', Allocated: 'blue', Maintenance: 'orange', Retired: 'red' };
const categories = ['Laptop', 'Monitor', 'Phone', 'Accessory', 'Other'];

function AssetsPage() {
  const [assets, setAssets] = useState<any[]>([]);
  const [employees, setEmployees] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [modal, setModal] = useState(false);
  const [allocModal, setAllocModal] = useState<any>(null);
  const [maintModal, setMaintModal] = useState<any>(null);
  const [maintRecords, setMaintRecords] = useState<any[]>([]);
  const [detailAsset, setDetailAsset] = useState<any>(null);
  const [form] = Form.useForm();
  const [editId, setEditId] = useState<number | null>(null);

  useEffect(() => {
    Promise.all([
      api.get('/assets'),
      api.get('/employees'),
    ]).then(([a, e]) => { setAssets(a.data); setEmployees(e.data); })
    .finally(() => setLoading(false));
  }, []);

  const fetchAssets = async () => {
    const res = await api.get('/assets'); setAssets(res.data);
  };

  const handleCreate = async (values: any) => {
    if (editId) {
      await api.put(`/assets/${editId}`, values);
      message.success('Asset updated');
    } else {
      await api.post('/assets', values);
      message.success('Asset created');
    }
    setModal(false); form.resetFields(); setEditId(null); fetchAssets();
  };

  const handleAllocate = async (values: any) => {
    await api.post(`/assets/${allocModal}/allocate`, values);
    message.success('Asset allocated');
    setAllocModal(null); fetchAssets();
  };

  const handleReturn = async (id: number) => {
    await api.post(`/assets/${id}/return`);
    message.success('Asset returned');
    fetchAssets();
  };

  const handleDeleteAsset = async (id: number) => {
    await api.delete(`/assets/${id}`);
    message.success('Asset deleted');
    fetchAssets();
  };

  const handleAddMaint = async (values: any) => {
    await api.post(`/assets/${maintModal}/maintenance`, values);
    message.success('Maintenance record added');
    setMaintModal(null); fetchAssets();
  };

  const openEdit = (asset: any) => {
    setEditId(asset.id);
    form.setFieldsValue(asset);
    setModal(true);
  };

  const openMaint = async (assetId: number) => {
    setMaintModal(assetId);
    const res = await api.get(`/assets/${assetId}/maintenance`);
    setMaintRecords(res.data);
  };

  const columns = [
    { title: 'Tag', dataIndex: 'assetTag' },
    { title: 'Name', dataIndex: 'name' },
    { title: 'Category', dataIndex: 'category', render: (c: string) => <Tag>{c}</Tag> },
    { title: 'Model', dataIndex: 'model' },
    { title: 'Status', dataIndex: 'status', render: (s: string) => <Tag color={statusColor[s]}>{s}</Tag> },
    { title: 'Allocated To', render: (_: any, r: any) => r.currentAllocation?.employeeName || '-' },
    { title: '', key: 'action', width: 240,
      render: (_: any, r: any) => (
        <Space>
          {r.status === 'Available' && <Button size="small" type="primary" icon={<SwapOutlined />} onClick={() => setAllocModal(r.id)}>Allocate</Button>}
          {r.status === 'Allocated' && <Button size="small" onClick={() => handleReturn(r.id)}>Return</Button>}
          <Button size="small" icon={<ToolOutlined />} onClick={() => openMaint(r.id)}>Maint.</Button>
          <Button size="small" onClick={() => openEdit(r)}>Edit</Button>
          <Popconfirm title="Delete this asset?" onConfirm={() => handleDeleteAsset(r.id)} okText="Delete" okButtonProps={{ danger: true }}>
            <Button size="small" danger icon={<DeleteOutlined />}>Delete</Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  const maintCols = [
    { title: 'Date', dataIndex: 'scheduledDate', render: (d: string) => new Date(d).toLocaleDateString() },
    { title: 'Type', dataIndex: 'type', render: (t: string) => <Tag>{t}</Tag> },
    { title: 'Description', dataIndex: 'description' },
    { title: 'Cost', dataIndex: 'cost', render: (c: number) => c ? formatINR(c) : '-' },
    { title: 'Status', dataIndex: 'status', render: (s: string) => <Tag color={statusColor[s] || 'default'}>{s}</Tag> },
  ];

  const stats = {
    total: assets.length,
    available: assets.filter(a => a.status === 'Available').length,
    allocated: assets.filter(a => a.status === 'Allocated').length,
    maintenance: assets.filter(a => a.status === 'Maintenance').length,
  };

  return (
    <div>
      <div className="page-toolbar">
        <div className="page-toolbar-text">
          <Title level={4} style={{ margin: 0 }}>
            <LaptopOutlined style={{ marginRight: 8, color: '#6c5ce7' }} />
            Asset Management
          </Title>
          <Text type="secondary">Track company assets, allocations, and maintenance</Text>
        </div>
        <div className="page-toolbar-actions">
          <Button type="primary" icon={<PlusOutlined />} onClick={() => { setEditId(null); form.resetFields(); setModal(true); }}>Add Asset</Button>
        </div>
      </div>

      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        <Col xs={12} sm={6}><Card style={{ borderRadius: 12, textAlign: 'center' }}><Statistic title="Total" value={stats.total} styles={{ content: {  color: '#6c5ce7'  } }} /></Card></Col>
        <Col xs={12} sm={6}><Card style={{ borderRadius: 12, textAlign: 'center' }}><Statistic title="Available" value={stats.available} styles={{ content: {  color: '#52c41a'  } }} /></Card></Col>
        <Col xs={12} sm={6}><Card style={{ borderRadius: 12, textAlign: 'center' }}><Statistic title="Allocated" value={stats.allocated} styles={{ content: {  color: '#1890ff'  } }} /></Card></Col>
        <Col xs={12} sm={6}><Card style={{ borderRadius: 12, textAlign: 'center' }}><Statistic title="Maintenance" value={stats.maintenance} styles={{ content: {  color: '#fa8c16'  } }} /></Card></Col>
      </Row>

      <Card style={{ borderRadius: 12 }}>
        <Table dataSource={assets} columns={columns} rowKey="id" loading={loading} scroll={{ x: 'max-content' }} />
      </Card>

      <Modal title={editId ? 'Edit Asset' : 'Add Asset'} open={modal} onCancel={() => { setModal(false); setEditId(null); }} onOk={() => form.submit()} width={500}>
        <Form form={form} onFinish={handleCreate} layout="vertical">
          <Space style={{ width: '100%' }} size={12}>
            <Form.Item name="name" label="Name" rules={[{ required: true }]} style={{ flex: 1 }}><Input /></Form.Item>
            <Form.Item name="assetTag" label="Asset Tag" rules={[{ required: true }]} style={{ flex: 1 }}><Input /></Form.Item>
          </Space>
          <Space style={{ width: '100%' }} size={12}>
            <Form.Item name="category" label="Category" rules={[{ required: true }]} initialValue="Laptop" style={{ flex: 1 }}>
              <Select>{categories.map(c => <Select.Option key={c} value={c}>{c}</Select.Option>)}</Select>
            </Form.Item>
            <Form.Item name="model" label="Model" style={{ flex: 1 }}><Input /></Form.Item>
          </Space>
          <Form.Item name="serialNumber" label="Serial Number"><Input /></Form.Item>
          <Space style={{ width: '100%' }} size={12}>
            <Form.Item name="purchaseDate" label="Purchase Date" style={{ flex: 1 }}><Input type="date" /></Form.Item>
            <Form.Item name="purchasePrice" label="Price (INR)" style={{ flex: 1 }}><InputNumber prefix={INR_PREFIX} style={{ width: '100%' }} min={0} /></Form.Item>
          </Space>
          <Form.Item name="warrantyExpiry" label="Warranty Expiry"><Input type="date" /></Form.Item>
          <Form.Item name="notes" label="Notes"><Input.TextArea /></Form.Item>
        </Form>
      </Modal>

      <Modal title="Allocate Asset" open={!!allocModal} onCancel={() => setAllocModal(null)} onOk={() => form.submit()}>
        <Form onFinish={handleAllocate} layout="vertical">
          <Form.Item name="employeeId" label="Employee" rules={[{ required: true }]}>
            <Select showSearch placeholder="Search employee..."
              filterOption={(i, o) => (o?.label as string)?.toLowerCase()?.includes(i?.toLowerCase() ?? '')}
              options={employees.map((e: any) => ({ label: `${e.firstName} ${e.lastName} — ${e.position}`, value: e.id }))}
            />
          </Form.Item>
          <Form.Item name="notes" label="Notes"><Input.TextArea /></Form.Item>
        </Form>
      </Modal>

      <Modal title="Maintenance Records" open={!!maintModal} onCancel={() => setMaintModal(null)} footer={null} width={600}>
        <Tabs items={[
          { key: 'records', label: 'Records', children: <Table dataSource={maintRecords} columns={maintCols} rowKey="id" pagination={false} scroll={{ x: 'max-content' }} /> },
          { key: 'add', label: 'New Record',
            children: (
              <Form onFinish={handleAddMaint} layout="vertical">
                <Form.Item name="description" label="Description" rules={[{ required: true }]}><Input /></Form.Item>
                <Space style={{ width: '100%' }} size={12}>
                  <Form.Item name="type" label="Type" initialValue="Repair" style={{ flex: 1 }}>
                    <Select>{['Repair', 'Upgrade', 'Inspection'].map(t => <Select.Option key={t} value={t}>{t}</Select.Option>)}</Select>
                  </Form.Item>
                  <Form.Item name="cost" label="Cost (INR)" style={{ flex: 1 }}><InputNumber prefix={INR_PREFIX} style={{ width: '100%' }} min={0} /></Form.Item>
                </Space>
                <Form.Item name="scheduledDate" label="Scheduled Date"><Input type="date" /></Form.Item>
                <Form.Item name="notes" label="Notes"><Input.TextArea /></Form.Item>
                <Form.Item name="startMaintenance" valuePropName="checked"><Checkbox>Set asset to Maintenance status</Checkbox></Form.Item>
                <Button type="primary" htmlType="submit">Add Record</Button>
              </Form>
            ),
          },
        ]} />
      </Modal>
    </div>
  );
}

export default AssetsPage;
