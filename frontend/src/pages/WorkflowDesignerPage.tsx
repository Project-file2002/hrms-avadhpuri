import { useEffect, useState } from 'react';
import {
  Card, Table, Button, Modal, Form, Input, Select, Typography,
  Space, Tag, Empty, message, Popconfirm, Steps, Descriptions, Divider, Row, Col, Tabs
} from 'antd';
import {
  PlusOutlined, DeleteOutlined, EditOutlined, PlayCircleOutlined,
  NodeIndexOutlined, BranchesOutlined, ArrowRightOutlined
} from '@ant-design/icons';
import api from '../services/api';

const { Title, Text } = Typography;

interface WorkflowStep {
  name: string;
  assigneeRole: string;
  action: string;
}

interface WorkflowDef {
  id: number;
  name: string;
  description?: string;
  steps: string;
  isActive: boolean;
  createdAt: string;
  instanceCount: number;
}

interface WorkflowInstance {
  id: number;
  recordId: number;
  status: string;
  currentStep: string;
  data?: string;
  createdAt: string;
  completedAt?: string;
  workflowName: string;
}

const roles = ['Administrator', 'HRManager', 'Manager', 'Employee', 'PayrollStaff'];
const actions = ['Approve', 'Review', 'Notify', 'Submit'];

const statusColors: Record<string, string> = {
  Pending: 'orange', InProgress: 'blue', Approved: 'green', Rejected: 'red',
};

const actionColors: Record<string, string> = {
  Approve: 'green', Review: 'blue', Notify: 'cyan', Submit: 'purple',
};

function WorkflowDesignerPage() {
  const [workflows, setWorkflows] = useState<WorkflowDef[]>([]);
  const [instances, setInstances] = useState<WorkflowInstance[]>([]);
  const [activeTab, setActiveTab] = useState('workflows');

  // Designer
  const [selectedWf, setSelectedWf] = useState<WorkflowDef | null>(null);
  const [wfName, setWfName] = useState('');
  const [wfDesc, setWfDesc] = useState('');
  const [steps, setSteps] = useState<WorkflowStep[]>([]);
  const [stepModal, setStepModal] = useState(false);
  const [editingStep, setEditingStep] = useState<number | null>(null);
  const [stepForm] = Form.useForm();

  useEffect(() => { fetchWorkflows(); fetchInstances(); }, []);

  const fetchWorkflows = async () => {
    const res = await api.get('/workflows');
    setWorkflows(res.data);
  };

  const fetchInstances = async () => {
    const res = await api.get('/workflows/instances');
    setInstances(res.data);
  };

  const selectWorkflow = (wf: WorkflowDef) => {
    setSelectedWf(wf);
    setWfName(wf.name);
    setWfDesc(wf.description || '');
    setSteps(JSON.parse(wf.steps));
    setActiveTab('designer');
  };

  const newWorkflow = () => {
    setSelectedWf(null);
    setWfName('');
    setWfDesc('');
    setSteps([]);
    setActiveTab('designer');
  };

  const addStep = () => {
    setEditingStep(null);
    stepForm.resetFields();
    setStepModal(true);
  };

  const editStep = (index: number) => {
    setEditingStep(index);
    stepForm.setFieldsValue(steps[index]);
    setStepModal(true);
  };

  const saveStep = (values: any) => {
    const step: WorkflowStep = {
      name: values.name,
      assigneeRole: values.assigneeRole,
      action: values.action,
    };

    let newSteps: WorkflowStep[];
    if (editingStep !== null) {
      newSteps = steps.map((s, i) => i === editingStep ? step : s);
    } else {
      newSteps = [...steps, step];
    }
    setSteps(newSteps);
    setStepModal(false);
  };

  const removeStep = (index: number) => {
    setSteps(steps.filter((_, i) => i !== index));
  };

  const saveWorkflow = async () => {
    if (!wfName.trim()) { message.warning('Workflow name is required'); return; }
    if (steps.length === 0) { message.warning('Add at least one step'); return; }

    const payload = { name: wfName, description: wfDesc, steps: JSON.stringify(steps) };

    if (selectedWf) {
      await api.put(`/workflows/${selectedWf.id}`, payload);
    } else {
      await api.post('/workflows', payload);
    }
    message.success('Workflow saved!');
    fetchWorkflows();
  };

  const startWorkflow = async (wf: WorkflowDef) => {
    const stepsArr = JSON.parse(wf.steps);
    const payload = {
      recordId: Date.now(),
      data: JSON.stringify({ startedBy: 'Admin', startedAt: new Date().toISOString() }),
    };
    await api.post(`/workflows/${wf.id}/start`, payload);
    message.success('Workflow started!');
    fetchInstances();
  };

  const workflowColumns = [
    { title: 'Name', dataIndex: 'name', key: 'name' },
    { title: 'Steps', key: 'steps', render: (_: any, r: WorkflowDef) => {
      const s = JSON.parse(r.steps);
      return <Tag>{s.length} step(s)</Tag>;
    }},
    { title: 'Active', dataIndex: 'isActive', key: 'active', render: (v: boolean) => v ? <Tag color="green">Yes</Tag> : <Tag>No</Tag> },
    {
      title: 'Action', key: 'action', width: 240,
      render: (_: any, r: WorkflowDef) => (
        <Space>
          <Button size="small" type="primary" onClick={() => selectWorkflow(r)}>Edit</Button>
          <Button size="small" icon={<PlayCircleOutlined />} onClick={() => startWorkflow(r)}>Start</Button>
          <Popconfirm title="Delete?" onConfirm={async () => { await api.delete(`/workflows/${r.id}`); fetchWorkflows(); }}>
            <Button size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  const instanceColumns = [
    { title: 'Workflow', dataIndex: 'workflowName', key: 'wf' },
    { title: 'Record ID', dataIndex: 'recordId', key: 'record' },
    {
      title: 'Status', dataIndex: 'status', key: 'status',
      render: (s: string) => <Tag color={statusColors[s]}>{s}</Tag>,
    },
    { title: 'Current Step', dataIndex: 'currentStep', key: 'step' },
    { title: 'Started', dataIndex: 'createdAt', key: 'created', render: (d: string) => new Date(d).toLocaleString() },
  ];

  return (
    <div>
      <div className="page-toolbar">
        <div className="page-toolbar-text">
          <Title level={4} style={{ margin: 0 }}>
            <NodeIndexOutlined style={{ marginRight: 8, color: '#6c5ce7' }} />
            Workflow Designer
          </Title>
          <Text type="secondary">Design multi-step approval workflows with role-based routing</Text>
        </div>
      </div>

      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        items={[
          {
            key: 'workflows',
            label: 'Workflows',
            children: (
              <Card style={{ borderRadius: 12 }}>
                <div style={{ marginBottom: 16 }}>
                  <Button type="primary" icon={<PlusOutlined />} onClick={newWorkflow}>Create Workflow</Button>
                </div>
                <Table dataSource={workflows} columns={workflowColumns} rowKey="id" pagination={false} scroll={{ x: 'max-content' }} />
              </Card>
            ),
          },
          {
            key: 'designer',
            label: 'Designer',
            children: (
              <Row gutter={[16, 16]}>
                <Col xs={24} lg={16}>
                  <Card
                    title={
                      <Input
                        value={wfName}
                        onChange={e => setWfName(e.target.value)}
                        placeholder="Workflow Name"
                        style={{ width: 300, fontWeight: 600, border: 'none', fontSize: 16 }}
                        variant="borderless"
                      />
                    }
                    style={{ borderRadius: 12, minHeight: 400 }}
                    extra={<Button type="primary" onClick={saveWorkflow}>Save</Button>}
                  >
                    <Input.TextArea
                      value={wfDesc}
                      onChange={e => setWfDesc(e.target.value)}
                      placeholder="Description (optional)"
                      style={{ marginBottom: 16 }}
                      rows={2}
                    />

                    {steps.length === 0 && <Empty description="No steps defined. Click 'Add Step' to start building your workflow." />}

                    <div style={{ position: 'relative' }}>
                      {steps.map((step, i) => (
                        <div key={i}>
                          <Card
                            size="small"
                            style={{
                              borderRadius: 8, marginBottom: 0,
                              borderLeft: `4px solid ${actionColors[step.action] || '#6c5ce7'}`,
                              position: 'relative',
                            }}
                          >
                            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                              <Space orientation="vertical" size={2}>
                                <Space>
                                  <Text strong>Step {i + 1}: {step.name}</Text>
                                  <Tag color={actionColors[step.action]}>{step.action}</Tag>
                                </Space>
                                <Text type="secondary" style={{ fontSize: 12 }}>Assigned to: {step.assigneeRole || 'Any'}</Text>
                              </Space>
                              <Space>
                                <Button size="small" icon={<EditOutlined />} onClick={() => editStep(i)} />
                                <Button size="small" danger icon={<DeleteOutlined />} onClick={() => removeStep(i)} />
                              </Space>
                            </div>
                          </Card>
                          {i < steps.length - 1 && (
                            <div style={{ textAlign: 'center', padding: '4px 0' }}>
                              <ArrowRightOutlined style={{ color: '#bfbfbf', fontSize: 16 }} />
                            </div>
                          )}
                        </div>
                      ))}
                    </div>

                    <Divider />
                    <Button type="dashed" icon={<PlusOutlined />} onClick={addStep} block>
                      Add Step
                    </Button>
                  </Card>
                </Col>

                <Col xs={24} lg={8}>
                  <Card title="Step Types" size="small" style={{ borderRadius: 12 }}>
                    <Space orientation="vertical" style={{ width: '100%' }}>
                      {actions.map(action => (
                        <div
                          key={action}
                          style={{
                            padding: '8px 12px', borderRadius: 6,
                            border: '1px solid #f0f0f0',
                            cursor: 'pointer',
                          }}
                          onClick={() => {
                            stepForm.setFieldsValue({ action, name: '', assigneeRole: '' });
                            setEditingStep(null);
                            setStepModal(true);
                          }}
                        >
                          <Space>
                            <Tag color={actionColors[action]}>{action}</Tag>
                            <Text>{action} Step</Text>
                          </Space>
                        </div>
                      ))}
                    </Space>
                  </Card>
                </Col>
              </Row>
            ),
          },
          {
            key: 'instances',
            label: 'Running Instances',
            children: (
              <Card style={{ borderRadius: 12 }}>
                <Table dataSource={instances} columns={instanceColumns} rowKey="id" pagination={false} scroll={{ x: 'max-content' }} />
              </Card>
            ),
          },
        ]}
      />

      <Modal
        title={editingStep !== null ? 'Edit Step' : 'Add Step'}
        open={stepModal}
        onCancel={() => setStepModal(false)}
        onOk={() => stepForm.submit()}
      >
        <Form form={stepForm} onFinish={saveStep} layout="vertical">
          <Form.Item name="name" label="Step Name" rules={[{ required: true }]}>
            <Input placeholder="e.g., Manager Approval" />
          </Form.Item>
          <Form.Item name="assigneeRole" label="Assigned Role">
            <Select placeholder="Any role" allowClear>
              {roles.map(r => <Select.Option key={r} value={r}>{r}</Select.Option>)}
            </Select>
          </Form.Item>
          <Form.Item name="action" label="Action" rules={[{ required: true }]}>
            <Select>
              {actions.map(a => <Select.Option key={a} value={a}>{a}</Select.Option>)}
            </Select>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}

export default WorkflowDesignerPage;
