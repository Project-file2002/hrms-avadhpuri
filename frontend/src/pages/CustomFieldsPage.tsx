import { useEffect, useState } from 'react';
import {
  Card, Table, Button, Modal, Form, Input, Select, Switch, Tag,
  Typography, Space, Popconfirm, Tabs, message
} from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, FieldBinaryOutlined } from '@ant-design/icons';
import api from '../services/api';

const { Title, Text } = Typography;

interface CustomField {
  id: number;
  module: string;
  fieldName: string;
  fieldType: string;
  options?: string;
  isRequired: boolean;
  sortOrder: number;
  isActive: boolean;
}

const fieldTypes = ['Text', 'Number', 'Date', 'Select', 'Boolean', 'TextArea'];
const modules = ['Employee', 'Leave', 'Attendance', 'Performance', 'Payroll', 'Recruitment'];

const typeColors: Record<string, string> = {
  Text: 'blue', Number: 'green', Date: 'orange',
  Select: 'purple', Boolean: 'cyan', TextArea: 'geekblue',
};

function CustomFieldsPage() {
  const [fields, setFields] = useState<CustomField[]>([]);
  const [activeModule, setActiveModule] = useState('Employee');
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<CustomField | null>(null);
  const [form] = Form.useForm();

  const fetchFields = async (module: string) => {
    const res = await api.get(`/customfields?module=${module}`);
    setFields(res.data);
  };

  useEffect(() => { fetchFields(activeModule); }, [activeModule]);

  const handleSave = async (values: any) => {
    const payload = { ...values, module: activeModule };
    if (editing) {
      await api.put(`/customfields/${editing.id}`, payload);
    } else {
      await api.post('/customfields', payload);
    }
    setModal(false);
    setEditing(null);
    form.resetFields();
    fetchFields(activeModule);
  };

  const handleDelete = async (id: number) => {
    await api.delete(`/customfields/${id}`);
    fetchFields(activeModule);
  };

  const openEdit = (field: CustomField) => {
    setEditing(field);
    form.setFieldsValue(field);
    setModal(true);
  };

  const columns = [
    { title: 'Field Name', dataIndex: 'fieldName', key: 'name' },
    {
      title: 'Type', dataIndex: 'fieldType', key: 'type',
      render: (t: string) => <Tag color={typeColors[t]}>{t}</Tag>,
    },
    {
      title: 'Required', dataIndex: 'isRequired', key: 'required',
      render: (v: boolean) => v ? <Tag color="red">Yes</Tag> : <Tag>No</Tag>,
    },
    { title: 'Order', dataIndex: 'sortOrder', key: 'order', width: 80 },
    {
      title: 'Active', dataIndex: 'isActive', key: 'active',
      render: (v: boolean) => <Switch size="small" checked={v} disabled />,
    },
    {
      title: 'Action', key: 'action', width: 120,
      render: (_: any, r: CustomField) => (
        <Space>
          <Button size="small" icon={<EditOutlined />} onClick={() => openEdit(r)} />
          <Popconfirm title="Delete this field?" onConfirm={() => handleDelete(r.id)}>
            <Button size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div>
      <div className="page-intro">
        <Title level={4} style={{ margin: 0 }}>
          <FieldBinaryOutlined style={{ marginRight: 8, color: '#6c5ce7' }} />
          Custom Fields
        </Title>
        <Text type="secondary">Add custom fields to any module without code</Text>
      </div>

      <Card style={{ borderRadius: 12 }}>
        <Tabs
          activeKey={activeModule}
          onChange={(k) => { setActiveModule(k); setEditing(null); }}
          tabBarExtraContent={
            <Button type="primary" icon={<PlusOutlined />} onClick={() => { setEditing(null); form.resetFields(); setModal(true); }}>
              Add Field
            </Button>
          }
          items={modules.map(m => ({ key: m, label: m }))}
        />

        <Table
          dataSource={fields}
          columns={columns}
          rowKey="id"
          pagination={false}
          style={{ marginTop: 16 }}
          scroll={{ x: 'max-content' }}
        />
      </Card>

      <Modal
        title={editing ? 'Edit Custom Field' : 'Add Custom Field'}
        open={modal}
        onCancel={() => { setModal(false); setEditing(null); }}
        onOk={() => form.submit()}
      >
        <Form form={form} onFinish={handleSave} layout="vertical">
          <Form.Item name="fieldName" label="Field Name" rules={[{ required: true }]}>
            <Input placeholder="e.g., LinkedIn Profile" />
          </Form.Item>
          <Form.Item name="fieldType" label="Field Type" rules={[{ required: true }]}>
            <Select>
              {fieldTypes.map(t => <Select.Option key={t} value={t}>{t}</Select.Option>)}
            </Select>
          </Form.Item>
          <Form.Item name="options" label="Options (for Select type)" dependencies={['fieldType']}>
            <Input.TextArea rows={3} placeholder='["Option 1", "Option 2"]' />
          </Form.Item>
          <Form.Item name="isRequired" label="Required" valuePropName="checked">
            <Switch />
          </Form.Item>
          <Form.Item name="sortOrder" label="Sort Order">
            <Input type="number" min={0} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}

export default CustomFieldsPage;
