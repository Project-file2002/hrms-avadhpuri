import { useEffect, useState } from 'react';
import {
  Card, Table, Button, Modal, Form, Input, Select, Switch, Typography,
  Space, Tabs, Tag, Empty, Popconfirm, Alert, Divider, Row, Col, message
} from 'antd';
import {
  PlusOutlined, DeleteOutlined, EditOutlined, EyeOutlined,
  CodeOutlined, UpOutlined, DownOutlined, FormOutlined, FileTextOutlined
} from '@ant-design/icons';
import api from '../services/api';

const { Title, Text } = Typography;

interface FormField {
  id: string;
  label: string;
  type: string;
  required: boolean;
  placeholder?: string;
  options?: string[];
}

interface FormDefinition {
  id: number;
  title: string;
  description?: string;
  schema: string;
  isActive: boolean;
  createdAt: string;
  submissionCount: number;
}

interface Submission {
  id: number;
  data: string;
  submittedBy: string;
  submittedAt: string;
}

const fieldTypes = [
  { value: 'text', label: 'Text Input' },
  { value: 'number', label: 'Number Input' },
  { value: 'email', label: 'Email' },
  { value: 'textarea', label: 'Text Area' },
  { value: 'date', label: 'Date Picker' },
  { value: 'select', label: 'Dropdown Select' },
  { value: 'checkbox', label: 'Checkbox' },
  { value: 'phone', label: 'Phone Number' },
];

const typeComponents: Record<string, React.ReactNode> = {
  text: <Input placeholder="Text input" />,
  number: <Input type="number" placeholder="Number" />,
  email: <Input type="email" placeholder="email@example.com" />,
  textarea: <Input.TextArea rows={3} placeholder="Long text" />,
  date: <Input type="date" />,
  select: <Select placeholder="Select option" />,
  checkbox: <Switch />,
  phone: <Input placeholder="+1 234 567 890" />,
};

function FormBuilderPage() {
  const [forms, setForms] = useState<FormDefinition[]>([]);
  const [activeTab, setActiveTab] = useState('designer');
  const [selectedForm, setSelectedForm] = useState<FormDefinition | null>(null);

  // Designer state
  const [formTitle, setFormTitle] = useState('');
  const [formDesc, setFormDesc] = useState('');
  const [fields, setFields] = useState<FormField[]>([]);
  const [showFieldModal, setShowFieldModal] = useState(false);
  const [editingField, setEditingField] = useState<FormField | null>(null);
  const [fieldForm] = Form.useForm();

  // Submissions state
  const [submissions, setSubmissions] = useState<Submission[]>([]);
  const [previewData, setPreviewData] = useState<Record<string, any>>({});
  const [showPreview, setShowPreview] = useState(false);

  useEffect(() => { fetchForms(); }, []);

  const fetchForms = async () => {
    const res = await api.get('/forms');
    setForms(res.data);
  };

  const fetchSubmissions = async (formId: number) => {
    const res = await api.get(`/forms/${formId}/submissions`);
    setSubmissions(res.data);
  };

  const selectForm = (form: FormDefinition) => {
    setSelectedForm(form);
    setFormTitle(form.title);
    setFormDesc(form.description || '');
    setFields(JSON.parse(form.schema));
    fetchSubmissions(form.id);
    setActiveTab('designer');
  };

  const newForm = () => {
    setSelectedForm(null);
    setFormTitle('');
    setFormDesc('');
    setFields([]);
    setSubmissions([]);
    setActiveTab('designer');
  };

  const addField = () => {
    setEditingField(null);
    fieldForm.resetFields();
    setShowFieldModal(true);
  };

  const editField = (field: FormField) => {
    setEditingField(field);
    fieldForm.setFieldsValue({ ...field, optionsStr: field.options?.join('\n') || '' });
    setShowFieldModal(true);
  };

  const saveField = (values: any) => {
    const field: FormField = {
      id: editingField?.id || Date.now().toString(),
      label: values.label,
      type: values.type,
      required: values.required,
      placeholder: values.placeholder,
      options: values.type === 'select' ? (values.optionsStr || '').split('\n').filter(Boolean) : undefined,
    };

    if (editingField) {
      setFields(fields.map(f => f.id === editingField.id ? field : f));
    } else {
      setFields([...fields, field]);
    }
    setShowFieldModal(false);
  };

  const removeField = (id: string) => {
    setFields(fields.filter(f => f.id !== id));
  };

  const moveField = (index: number, direction: 'up' | 'down') => {
    const newFields = [...fields];
    const target = direction === 'up' ? index - 1 : index + 1;
    if (target < 0 || target >= newFields.length) return;
    [newFields[index], newFields[target]] = [newFields[target], newFields[index]];
    setFields(newFields);
  };

  const saveForm = async () => {
    if (!formTitle.trim()) { message.warning('Form title is required'); return; }
    const payload = { title: formTitle, description: formDesc, schema: JSON.stringify(fields) };

    if (selectedForm) {
      await api.put(`/forms/${selectedForm.id}`, payload);
    } else {
      await api.post('/forms', payload);
    }
    message.success('Form saved!');
    fetchForms();
  };

  const handlePreviewSubmit = async () => {
    if (!selectedForm) return;
    await api.post(`/forms/${selectedForm.id}/submit`, { data: JSON.stringify(previewData) });
    message.success('Form submitted!');
    setShowPreview(false);
    setPreviewData({});
    fetchSubmissions(selectedForm.id);
  };

  const renderPreview = () => (
    <div style={{ maxWidth: 500, margin: '0 auto' }}>
      {fields.map(f => (
        <div key={f.id} style={{ marginBottom: 16 }}>
          <Text strong>{f.label}{f.required && <Text type="danger"> *</Text>}</Text>
          <div style={{ marginTop: 4 }}>
            {f.type === 'select' ? (
              <Select
                style={{ width: '100%' }}
                placeholder={f.placeholder}
                value={previewData[f.id]}
                onChange={v => setPreviewData({ ...previewData, [f.id]: v })}
              >
                {(f.options || []).map(o => <Select.Option key={o} value={o}>{o}</Select.Option>)}
              </Select>
            ) : f.type === 'checkbox' ? (
              <Switch
                checked={previewData[f.id]}
                onChange={v => setPreviewData({ ...previewData, [f.id]: v })}
              />
            ) : f.type === 'textarea' ? (
              <Input.TextArea
                rows={3}
                placeholder={f.placeholder}
                value={previewData[f.id]}
                onChange={e => setPreviewData({ ...previewData, [f.id]: e.target.value })}
              />
            ) : (
              <Input
                type={f.type}
                placeholder={f.placeholder}
                value={previewData[f.id]}
                onChange={e => setPreviewData({ ...previewData, [f.id]: e.target.value })}
              />
            )}
          </div>
        </div>
      ))}
      {fields.length > 0 && (
        <Button type="primary" onClick={handlePreviewSubmit} style={{ marginTop: 8 }}>
          Submit
        </Button>
      )}
    </div>
  );

  const listColumns = [
    { title: 'Title', dataIndex: 'title', key: 'title' },
    { title: 'Submissions', dataIndex: 'submissionCount', key: 'count', width: 120 },
    { title: 'Created', dataIndex: 'createdAt', key: 'created', render: (d: string) => new Date(d).toLocaleDateString() },
    {
      title: 'Action', key: 'action', width: 200,
      render: (_: any, r: FormDefinition) => (
        <Space>
          <Button size="small" type="primary" onClick={() => selectForm(r)}>Edit</Button>
          <Button size="small" icon={<FileTextOutlined />} onClick={() => { selectForm(r); setActiveTab('submissions'); }}>
            Submissions
          </Button>
        </Space>
      ),
    },
  ];

  const submissionColumns = (fields: FormField[]) =>
    fields.filter(f => f.type !== 'checkbox').map(f => ({
      title: f.label, key: f.id,
      render: (_: any, r: Submission) => {
        const data = JSON.parse(r.data);
        return data[f.id]?.toString() || '-';
      },
    }));

  const isFieldModalOpen = showFieldModal;

  return (
    <div>
      <div className="page-toolbar">
        <div className="page-toolbar-text">
          <Title level={4} style={{ margin: 0 }}>
            <FormOutlined style={{ marginRight: 8, color: '#6c5ce7' }} />
            No-Code Form Builder
          </Title>
          <Text type="secondary">Create dynamic forms without writing any code</Text>
        </div>
      </div>

      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        items={[
          {
            key: 'forms',
            label: 'My Forms',
            children: (
              <Card style={{ borderRadius: 12 }}>
                <div style={{ marginBottom: 16 }}>
                  <Button type="primary" icon={<PlusOutlined />} onClick={newForm}>Create New Form</Button>
                </div>
                <Table dataSource={forms} columns={listColumns} rowKey="id" pagination={false} scroll={{ x: 'max-content' }} />
              </Card>
            ),
          },
          {
            key: 'designer',
            label: 'Form Designer',
            children: (
              <Row gutter={[16, 16]}>
                <Col xs={24} lg={14}>
                  <Card
                    title={
                      <Space>
                        <Input
                          value={formTitle}
                          onChange={e => setFormTitle(e.target.value)}
                          placeholder="Form Title"
                          style={{ width: 300, fontWeight: 600, border: 'none', fontSize: 16 }}
                          variant="borderless"
                        />
                        <Button size="small" icon={<EyeOutlined />} onClick={() => setShowPreview(true)}>
                          Preview
                        </Button>
                      </Space>
                    }
                    style={{ borderRadius: 12, minHeight: 400 }}
                    extra={
                      <Space>
                        <Button type="primary" onClick={saveForm}>Save Form</Button>
                      </Space>
                    }
                  >
                    <Input.TextArea
                      value={formDesc}
                      onChange={e => setFormDesc(e.target.value)}
                      placeholder="Form description (optional)"
                      style={{ marginBottom: 16 }}
                      rows={2}
                    />

                    {fields.length === 0 && (
                      <Empty description="Add your first form field" />
                    )}

                    {fields.map((f, i) => (
                      <Card
                        key={f.id}
                        size="small"
                        style={{ marginBottom: 8, borderRadius: 8, borderLeft: '3px solid #6c5ce7' }}
                        onClick={() => editField(f)}
                      >
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                          <Space>
                            <Tag color="purple">{f.type}</Tag>
                            <Text strong>{f.label}</Text>
                            {f.required && <Text type="danger">*</Text>}
                          </Space>
                          <Space>
                            <Button size="small" icon={<UpOutlined />} onClick={(e) => { e.stopPropagation(); moveField(i, 'up'); }} disabled={i === 0} />
                            <Button size="small" icon={<DownOutlined />} onClick={(e) => { e.stopPropagation(); moveField(i, 'down'); }} disabled={i === fields.length - 1} />
                            <Button size="small" danger icon={<DeleteOutlined />} onClick={(e) => { e.stopPropagation(); removeField(f.id); }} />
                          </Space>
                        </div>
                      </Card>
                    ))}

                    <Divider />
                    <Button type="dashed" icon={<PlusOutlined />} onClick={addField} block>
                      Add Field
                    </Button>
                  </Card>
                </Col>

                <Col xs={24} lg={10}>
                  <Card title="Field Types" size="small" style={{ borderRadius: 12 }}>
                    <Space orientation="vertical" style={{ width: '100%' }}>
                      {fieldTypes.map(ft => (
                        <div
                          key={ft.value}
                          style={{
                            padding: '8px 12px', borderRadius: 6,
                            border: '1px solid #f0f0f0', cursor: 'pointer',
                            display: 'flex', justifyContent: 'space-between', alignItems: 'center'
                          }}
                          onClick={() => {
                            fieldForm.setFieldsValue({ type: ft.value, required: false, placeholder: '', label: ft.label, optionsStr: '' });
                            setEditingField(null);
                            setShowFieldModal(true);
                          }}
                        >
                          <Text>{ft.label}</Text>
                          <Text type="secondary" style={{ fontSize: 12 }}>{typeComponents[ft.value]}</Text>
                        </div>
                      ))}
                    </Space>
                  </Card>
                </Col>
              </Row>
            ),
          },
          {
            key: 'submissions',
            label: 'Submissions',
            children: (
              <Card style={{ borderRadius: 12 }}>
                {selectedForm ? (
                  <>
                    <Text strong style={{ fontSize: 16 }}>{selectedForm.title}</Text>
                    <Text type="secondary" style={{ display: 'block', marginBottom: 16 }}>
                      {submissions.length} submission(s)
                    </Text>
                    {submissions.length > 0 ? (
                      <Table
                        dataSource={submissions}
                        columns={[
                          { title: '#', key: 'index', render: (_: any, __: any, i: number) => i + 1, width: 60 },
                          ...submissionColumns(fields),
                          { title: 'Submitted By', dataIndex: 'submittedBy', key: 'by' },
                          { title: 'Date', dataIndex: 'submittedAt', key: 'date', render: (d: string) => new Date(d).toLocaleString() },
                        ]}
                        rowKey="id"
                        pagination={false}
                        scroll={{ x: 'max-content' }}
                      />
                    ) : <Empty description="No submissions yet" />}
                  </>
                ) : <Empty description="Select a form to view submissions" />}
              </Card>
            ),
          },
        ]}
      />

      {/* Field Editor Modal */}
      <Modal
        title={editingField ? 'Edit Field' : 'Add Field'}
        open={isFieldModalOpen}
        onCancel={() => setShowFieldModal(false)}
        onOk={() => fieldForm.submit()}
      >
        <Form form={fieldForm} onFinish={saveField} layout="vertical">
          <Form.Item name="label" label="Label" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="type" label="Type" rules={[{ required: true }]}>
            <Select>
              {fieldTypes.map(t => <Select.Option key={t.value} value={t.value}>{t.label}</Select.Option>)}
            </Select>
          </Form.Item>
          <Form.Item name="placeholder" label="Placeholder">
            <Input />
          </Form.Item>
          <Form.Item name="required" label="Required" valuePropName="checked">
            <Switch />
          </Form.Item>
          <Form.Item noStyle dependencies={['type']}>
            {({ getFieldValue }) =>
              getFieldValue('type') === 'select' ? (
                <Form.Item name="optionsStr" label="Options (one per line)">
                  <Input.TextArea rows={4} placeholder="Option 1&#10;Option 2&#10;Option 3" />
                </Form.Item>
              ) : null
            }
          </Form.Item>
        </Form>
      </Modal>

      {/* Preview Modal */}
      <Modal
        title="Form Preview"
        open={showPreview}
        onCancel={() => setShowPreview(false)}
        footer={null}
        width={600}
      >
        {renderPreview()}
      </Modal>
    </div>
  );
}

export default FormBuilderPage;
