import { useEffect, useState } from 'react';
import { Table, Tabs, Button, Modal, Form, Input, Select, Tag, Typography, Space, Card, Row, Col, Statistic, Spin, Progress, Drawer, Popconfirm, message } from 'antd';
import { PlusOutlined, RobotOutlined, QuestionCircleOutlined, BulbOutlined, CheckCircleOutlined, CloseCircleOutlined, SendOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import api from '../services/api';
import { formatINR, formatINRRange, INR_PREFIX } from '../utils/currency';
import { useRoles } from '../utils/roles';

const { Title, Text } = Typography;

const statusColor: Record<string, string> = {
  New: 'blue', Screening: 'orange', Interviewed: 'purple', Offered: 'cyan', Hired: 'green', Rejected: 'red',
  Draft: 'default', Approved: 'green', Accepted: 'cyan', Pending: 'orange', Cleared: 'green', Failed: 'red',
  PendingDeptApproval: 'orange', PendingHrApproval: 'volcano', PendingBudgetApproval: 'gold',
  Open: 'green', Closed: 'default',
};

function RecruitmentPage() {

  const { isHr } = useRoles();
  const isAdmin = isHr;

  const [loading, setLoading] = useState(false);
  const [candidates, setCandidates] = useState<any[]>([]);
  const [requisitions, setRequisitions] = useState<any[]>([]);
  const [hiringRequests, setHiringRequests] = useState<any[]>([]);
  const [offers, setOffers] = useState<any[]>([]);
  const [dashboard, setDashboard] = useState<any>({});
  const [departments, setDepartments] = useState<any[]>([]);

  const [candidateModal, setCandidateModal] = useState(false);
  const [jobModal, setJobModal] = useState(false);
  const [editModalVisible, setEditModalVisible] = useState(false);
  const [editingJob, setEditingJob] = useState<any>(null);
  const [detailVisible, setDetailVisible] = useState(false);
  const [selectedJob, setSelectedJob] = useState<any>(null);
  const [hiringModal, setHiringModal] = useState(false);
  const [offerModal, setOfferModal] = useState(false);
  const [screenModal, setScreenModal] = useState(false);
  const [questionsModal, setQuestionsModal] = useState(false);
  const [bgCheckModal, setBgCheckModal] = useState(false);
  const [onboardingModal, setOnboardingModal] = useState(false);
  const [rejectHiringOpen, setRejectHiringOpen] = useState(false);
  const [rejectHiringId, setRejectHiringId] = useState<number | null>(null);
  const [rejectHiringReason, setRejectHiringReason] = useState('');
  const [rejectOfferOpen, setRejectOfferOpen] = useState(false);
  const [rejectOfferId, setRejectOfferId] = useState<number | null>(null);
  const [rejectOfferReason, setRejectOfferReason] = useState('');

  const [screenCandidate, setScreenCandidate] = useState<any>(null);
  const [screenResult, setScreenResult] = useState('');
  const [screenLoading, setScreenLoading] = useState(false);
  const [screenJobDesc, setScreenJobDesc] = useState('');
  const [screenResume, setScreenResume] = useState('');

  const [questionsResult, setQuestionsResult] = useState('');
  const [questionsLoading, setQuestionsLoading] = useState(false);
  const [questionsRole, setQuestionsRole] = useState('');
  const [questionsProfile, setQuestionsProfile] = useState('');

  const [form] = Form.useForm();
  const [jobForm] = Form.useForm();
  const [editForm] = Form.useForm();
  const [hiringForm] = Form.useForm();
  const [offerForm] = Form.useForm();
  const [bgForm] = Form.useForm();
  const [onboardForm] = Form.useForm();

  const fetchData = async () => {
    setLoading(true);
    try {
      const [candRes, reqRes, hiringRes, offerRes, dashRes, deptRes] = await Promise.all([
        api.get('/recruitment/candidates'),
        api.get('/recruitment/requisitions'),
        api.get('/recruitment/hiring-requests'),
        api.get('/recruitment/offers'),
        api.get('/recruitment/dashboard'),
        api.get('/departments'),
      ]);
      setCandidates(candRes.data);
      setRequisitions(reqRes.data);
      setHiringRequests(hiringRes.data);
      setOffers(offerRes.data.filter((o: any) => o.status !== 'Draft'));
      setDashboard(dashRes.data);
      setDepartments(deptRes.data);
    } finally { setLoading(false); }
  };

  useEffect(() => { fetchData(); }, []);

  // Modal handlers
  const handleAddCandidate = async (values: any) => {
    await api.post('/recruitment/candidates', values);
    setCandidateModal(false); form.resetFields(); fetchData();
  };

  const handleAddRequisition = async (values: any) => {
    await api.post('/recruitment/requisitions', values);
    setJobModal(false); jobForm.resetFields(); fetchData();
  };

  const showDetail = async (id: number) => {
    const res = await api.get(`/recruitment/requisitions/${id}`);
    setSelectedJob(res.data);
    setDetailVisible(true);
  };

  const openEditModal = (record: any) => {
    setEditingJob(record);
    editForm.setFieldsValue({
      title: record.title,
      description: record.description,
      requirements: record.requirements,
      departmentId: record.departmentId,
      status: record.status,
    });
    setEditModalVisible(true);
  };

  const handleEditRequisition = async (values: any) => {
    await api.put(`/recruitment/requisitions/${editingJob?.id}`, values);
    setEditModalVisible(false);
    editForm.resetFields();
    setEditingJob(null);
    message.success('Job requisition updated');
    fetchData();
  };

  const handleDeleteRequisition = async (id: number) => {
    await api.delete(`/recruitment/requisitions/${id}`);
    message.success('Job requisition deleted');
    fetchData();
  };

  const handleCreateHiring = async (values: any) => {
    await api.post('/recruitment/hiring-requests', values);
    setHiringModal(false); hiringForm.resetFields(); fetchData();
  };

  const handleApproveHiring = async (id: number, approved: boolean, notes?: string) => {
    await api.put(`/recruitment/hiring-requests/${id}/approve`, { approved, notes: notes ?? '' });
    fetchData();
  };

  const openRejectHiringModal = (id: number) => {
    setRejectHiringId(id);
    setRejectHiringReason('');
    setRejectHiringOpen(true);
  };

  const confirmRejectHiring = async () => {
    if (rejectHiringId == null) return;
    await handleApproveHiring(rejectHiringId, false, rejectHiringReason);
    setRejectHiringOpen(false);
    setRejectHiringId(null);
    setRejectHiringReason('');
  };

  const handleCreateOffer = async (values: any) => {
    await api.post('/recruitment/offers', values);
    setOfferModal(false); offerForm.resetFields(); fetchData();
  };

  const handleApproveOffer = async (id: number) => {
    await api.put(`/recruitment/offers/${id}/approve`);
    fetchData();
  };

  const handleAcceptOffer = async (id: number) => {
    await api.put(`/recruitment/offers/${id}/accept`);
    fetchData();
  };

  const handleRejectOffer = async (id: number, reason?: string) => {
    await api.put(`/recruitment/offers/${id}/reject`, { reason: reason ?? '' });
    fetchData();
  };

  const openRejectOfferModal = (id: number) => {
    setRejectOfferId(id);
    setRejectOfferReason('');
    setRejectOfferOpen(true);
  };

  const confirmRejectOffer = async () => {
    if (rejectOfferId == null) return;
    await handleRejectOffer(rejectOfferId, rejectOfferReason);
    setRejectOfferOpen(false);
    setRejectOfferId(null);
    setRejectOfferReason('');
  };

  const handleStatusChange = async (id: number, status: string) => {
    await api.put(`/recruitment/candidates/${id}/status`, { status });
    fetchData();
  };

  const handleInitiateBgCheck = async (values: any) => {
    await api.post('/recruitment/background-checks', values);
    setBgCheckModal(false); bgForm.resetFields(); fetchData();
  };

  const handleCreateOnboarding = async (values: any) => {
    await api.post('/recruitment/onboarding', values);
    setOnboardingModal(false); onboardForm.resetFields(); fetchData();
  };

  const handleToggleTask = async (id: number) => {
    await api.put(`/recruitment/onboarding/${id}/toggle`);
    fetchData();
  };

  // AI
  const openScreen = (c: any) => { setScreenCandidate(c); setScreenResult(''); setScreenJobDesc(''); setScreenResume(''); setScreenModal(true); };
  const handleScreen = async () => {
    setScreenLoading(true);
    try { const res = await api.post('/ai/screen', { jobDescription: screenJobDesc, resumeText: screenResume }); setScreenResult(res.data.result); }
    catch { setScreenResult('AI screening error.'); }
    setScreenLoading(false);
  };

  const openQuestions = (c: any) => {
    setQuestionsResult(''); setQuestionsRole(c.jobTitle || ''); setQuestionsProfile(`${c.firstName} ${c.lastName} — ${c.email}`); setQuestionsModal(true);
  };
  const handleGenerateQuestions = async () => {
    setQuestionsLoading(true);
    try { const res = await api.post('/ai/interview-questions', { role: questionsRole, candidateProfile: questionsProfile }); setQuestionsResult(res.data.result); }
    catch { setQuestionsResult('AI question generation error.'); }
    setQuestionsLoading(false);
  };

  // Columns
  const candCols = [
    { title: 'Name', render: (_: any, r: any) => `${r.firstName} ${r.lastName}` },
    { title: 'Email', dataIndex: 'email' },
    { title: 'Status', dataIndex: 'status', render: (s: string) => <Tag color={statusColor[s]}>{s}</Tag> },
    { title: 'Match', dataIndex: 'matchScore', render: (v: number) => v != null ? `${v.toFixed(0)}%` : '-' },
    { title: 'Job', dataIndex: 'jobTitle' },
    {
      title: 'Actions', key: 'actions', width: 200,
      render: (_: any, r: any) => (
        <Space>
          <Select value={r.status} onChange={v => handleStatusChange(r.id, v)} size="small" style={{ width: 110 }}>
            {['New', 'Screening', 'Interviewed', 'Hired', 'Rejected'].map(s => <Select.Option key={s} value={s}>{s}</Select.Option>)}
          </Select>
        </Space>
      ),
    },
    {
      title: 'AI', key: 'ai', width: 160,
      render: (_: any, r: any) => (
        <Space>
          <Button size="small" icon={<RobotOutlined />} onClick={() => openScreen(r)}>Screen</Button>
          <Button size="small" icon={<QuestionCircleOutlined />} onClick={() => openQuestions(r)}>Ques.</Button>
        </Space>
      ),
    },
    {
      title: 'Offer', key: 'offer', width: 100,
      render: (_: any, r: any) => (
        r.status === 'Interviewed' ? <Button size="small" type="primary" onClick={() => { offerForm.setFieldsValue({ candidateId: r.id }); setOfferModal(true); }}>Offer</Button> : null
      ),
    },
    {
      title: 'BG Check', key: 'bg', width: 100,
      render: (_: any, r: any) => (
        ['Offered', 'Hired'].includes(r.status) ? <Button size="small" onClick={() => { bgForm.setFieldsValue({ candidateId: r.id }); setBgCheckModal(true); }}>BG</Button> : null
      ),
    },
    {
      title: 'Onboard', key: 'onboard', width: 100,
      render: (_: any, r: any) => (
        r.status === 'Hired' ? <Button size="small" onClick={() => { onboardForm.setFieldsValue({ candidateId: r.id }); setOnboardingModal(true); }}>Tasks</Button> : null
      ),
    },
  ];

  const reqCols = [
    { title: 'Title', dataIndex: 'title', render: (t: string, r: any) => <a onClick={() => showDetail(r.id)}>{t}</a> },
    { title: 'Department', dataIndex: 'department' },
    { title: 'Status', dataIndex: 'status', render: (s: string) => <Tag color={statusColor[s]}>{s}</Tag> },
    { title: 'Candidates', dataIndex: 'candidateCount' },
    { title: 'Created', dataIndex: 'createdAt', render: (d: string) => new Date(d).toLocaleDateString() },
    {
      title: 'Actions', key: 'actions', width: 160,
      render: (_: any, r: any) => (
        <Space>
          <Button size="small" icon={<EditOutlined />} onClick={() => openEditModal(r)}>Edit</Button>
          <Popconfirm title="Delete this job requisition?" onConfirm={() => handleDeleteRequisition(r.id)} okText="Delete" danger>
            <Button size="small" danger icon={<DeleteOutlined />}>Delete</Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  const hiringCols = [
    { title: 'Job Title', dataIndex: 'jobTitle' },
    { title: 'Headcount', dataIndex: 'headcount' },
    { title: 'Budget', render: (_: any, r: any) => r.budgetRangeLow ? formatINRRange(r.budgetRangeLow, r.budgetRangeHigh) : '-' },
    { title: 'Status', dataIndex: 'status', render: (s: string) => <Tag color={statusColor[s]}>{s.replace(/([A-Z])/g, ' $1').trim()}</Tag> },
    { title: 'Requested By', dataIndex: 'requestedByName' },
    { title: 'Date', dataIndex: 'createdAt', render: (d: string) => new Date(d).toLocaleDateString() },
    {
      title: 'Action', key: 'action', width: 180,
      render: (_: any, r: any) => (
        r.status !== 'Approved' && r.status !== 'Rejected' && isAdmin ? (
          <Space>
            <Button size="small" type="primary" icon={<CheckCircleOutlined />} onClick={() => handleApproveHiring(r.id, true)} />
            <Button size="small" danger icon={<CloseCircleOutlined />} onClick={() => openRejectHiringModal(r.id)} />
          </Space>
        ) : <Tag>{r.status}</Tag>
      ),
    },
  ];

  const offerCols = [
    { title: 'Candidate', dataIndex: 'candidateName' },
    { title: 'Salary', dataIndex: 'salary', render: (v: number) => formatINR(v, true) },
    { title: 'Start Date', dataIndex: 'startDate', render: (d: string) => d ? new Date(d).toLocaleDateString() : '-' },
    { title: 'Status', dataIndex: 'status', render: (s: string) => <Tag color={statusColor[s]}>{s}</Tag> },
    { title: 'Approved By', dataIndex: 'approvedByName' },
    {
      title: 'Action', key: 'action',
      render: (_: any, r: any) => (
        <Space>
          {r.status === 'Approved' && <Button size="small" type="primary" icon={<CheckCircleOutlined />} onClick={() => handleAcceptOffer(r.id)}>Accept</Button>}
          {r.status === 'Approved' && <Button size="small" danger onClick={() => openRejectOfferModal(r.id)}>Reject</Button>}
          {r.status === 'Draft' && isAdmin && <Button size="small" onClick={() => handleApproveOffer(r.id)}>Approve</Button>}
        </Space>
      ),
    },
  ];

  return (
    <div>
      <div className="page-intro">
        <Title level={4} style={{ margin: 0 }}>
          <RobotOutlined style={{ marginRight: 8, color: '#6c5ce7' }} />
          Recruitment Pipeline
        </Title>
      </div>

      <Tabs items={[
        {
          key: 'dashboard', label: 'Dashboard',
          children: (
            <Row gutter={[16, 16]}>
              {[
                { title: 'Open Requisitions', value: dashboard.openRequisitions, color: '#6c5ce7' },
                { title: 'Total Candidates', value: dashboard.totalCandidates, color: '#1890ff' },
                { title: 'In Screening', value: dashboard.inScreening, color: '#fa8c16' },
                { title: 'Interviewed', value: dashboard.interviewed, color: '#722ed1' },
                { title: 'Offered', value: dashboard.offered, color: '#13c2c2' },
                { title: 'Hired', value: dashboard.hired, color: '#52c41a' },
                { title: 'Acceptance Rate', value: dashboard.offerAcceptanceRate, color: '#eb2f96' },
                { title: 'Avg Days in Pipeline', value: dashboard.avgDaysSinceCreated, suffix: 'days', color: '#fa541c' },
              ].map(m => (
                <Col xs={12} sm={8} lg={6} key={m.title}>
                  <Card style={{ borderRadius: 12, textAlign: 'center' }}>
                    <Statistic title={m.title} value={m.value || 0} suffix={(m as any).suffix} styles={{ content: {  color: m.color  } }} />
                  </Card>
                </Col>
              ))}
              <Col span={24}>
                <Card title="Pipeline Funnel" style={{ borderRadius: 12 }}>
                  {[
                    { label: 'New', value: dashboard.newCandidates, total: dashboard.totalCandidates },
                    { label: 'Screening', value: dashboard.inScreening, total: dashboard.totalCandidates },
                    { label: 'Interviewed', value: dashboard.interviewed, total: dashboard.totalCandidates },
                    { label: 'Offered', value: dashboard.offered, total: dashboard.totalCandidates },
                    { label: 'Hired', value: dashboard.hired, total: dashboard.totalCandidates },
                  ].map(s => (
                    <div key={s.label} style={{ marginBottom: 12 }}>
                      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                        <Text>{s.label}</Text><Text strong>{s.value} / {s.total}</Text>
                      </div>
                      <Progress percent={s.total ? Math.round(s.value / s.total * 100) : 0} showInfo={false} strokeColor="#6c5ce7" />
                    </div>
                  ))}
                </Card>
              </Col>
            </Row>
          ),
        },
        {
          key: 'candidates', label: `Candidates (${candidates.length})`,
          children: (
            <>
              <div style={{ marginBottom: 16, display: 'flex', gap: 8 }}>
                <Button type="primary" icon={<PlusOutlined />} onClick={() => setCandidateModal(true)}>Add Candidate</Button>
              </div>
              <Table dataSource={candidates} columns={candCols} rowKey="id" loading={loading} scroll={{ x: 'max-content' }} />
            </>
          ),
        },
        {
          key: 'requisitions', label: `Jobs (${requisitions.length})`,
          children: (
            <>
              <div style={{ marginBottom: 16 }}><Button type="primary" icon={<PlusOutlined />} onClick={() => setJobModal(true)}>New Requisition</Button></div>
              <Table dataSource={requisitions} columns={reqCols} rowKey="id" loading={loading} scroll={{ x: 'max-content' }} />
            </>
          ),
        },
        {
          key: 'hiring', label: `Hiring Requests (${hiringRequests.length})`,
          children: (
            <>
              <div style={{ marginBottom: 16 }}><Button type="primary" icon={<PlusOutlined />} onClick={() => setHiringModal(true)}>New Request</Button></div>
              <Table dataSource={hiringRequests} columns={hiringCols} rowKey="id" loading={loading} scroll={{ x: 'max-content' }} />
            </>
          ),
        },
        {
          key: 'offers', label: `Offers (${offers.length})`,
          children: <Table dataSource={offers} columns={offerCols} rowKey="id" loading={loading} scroll={{ x: 'max-content' }} />,
        },
      ]} />

      {/* Hiring Request Modal */}
      <Modal title="New Hiring Request" open={hiringModal} onCancel={() => setHiringModal(false)} onOk={() => hiringForm.submit()}>
        <Form form={hiringForm} onFinish={handleCreateHiring} layout="vertical">
          <Form.Item name="jobTitle" label="Job Title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea rows={2} /></Form.Item>
          <Form.Item name="justification" label="Justification"><Input.TextArea rows={2} /></Form.Item>
          <Form.Item name="headcount" label="Headcount" rules={[{ required: true }]}><Input type="number" min={1} /></Form.Item>
          <Form.Item name="employmentType" label="Employment Type" initialValue="FullTime">
            <Select>{['FullTime', 'PartTime', 'Contract', 'Intern'].map(s => <Select.Option key={s} value={s}>{s}</Select.Option>)}</Select>
          </Form.Item>
          <Space>
            <Form.Item name="budgetRangeLow" label="Budget Min (INR)"><Input type="number" prefix={INR_PREFIX} /></Form.Item>
            <Form.Item name="budgetRangeHigh" label="Budget Max (INR)"><Input type="number" prefix={INR_PREFIX} /></Form.Item>
          </Space>
        </Form>
      </Modal>

      {/* Offer Modal */}
      <Modal title="Create Offer" open={offerModal} onCancel={() => setOfferModal(false)} onOk={() => offerForm.submit()}>
        <Form form={offerForm} onFinish={handleCreateOffer} layout="vertical">
          <Form.Item name="candidateId" label="Candidate" hidden><Input /></Form.Item>
          <Form.Item name="salary" label="Salary (INR)" rules={[{ required: true }]}><Input type="number" prefix={INR_PREFIX} /></Form.Item>
          <Form.Item name="currency" label="Currency" initialValue="INR"><Select><Select.Option value="INR">INR (₹)</Select.Option></Select></Form.Item>
          <Form.Item name="benefits" label="Benefits"><Input.TextArea rows={2} /></Form.Item>
          <Form.Item name="startDate" label="Start Date"><Input type="date" /></Form.Item>
        </Form>
      </Modal>

      {/* BG Check Modal */}
      <Modal title="Initiate Background Check" open={bgCheckModal} onCancel={() => setBgCheckModal(false)} onOk={() => bgForm.submit()}>
        <Form form={bgForm} onFinish={handleInitiateBgCheck} layout="vertical">
          <Form.Item name="candidateId" hidden><Input /></Form.Item>
          <Form.Item name="vendorName" label="Vendor Name"><Input /></Form.Item>
        </Form>
      </Modal>

      {/* Onboarding Modal */}
      <Modal title="Add Onboarding Task" open={onboardingModal} onCancel={() => setOnboardingModal(false)} onOk={() => onboardForm.submit()}>
        <Form form={onboardForm} onFinish={handleCreateOnboarding} layout="vertical">
          <Form.Item name="candidateId" hidden><Input /></Form.Item>
          <Form.Item name="title" label="Task" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea /></Form.Item>
          <Form.Item name="category" label="Category" initialValue="Document">
            <Select>{['Document', 'IT Setup', 'HR Form', 'Training', 'Compliance'].map(s => <Select.Option key={s} value={s}>{s}</Select.Option>)}</Select>
          </Form.Item>
          <Form.Item name="assignedTo" label="Assigned To"><Input /></Form.Item>
        </Form>
      </Modal>

      {/* Detail Drawer */}
      <Drawer title={selectedJob?.title} open={detailVisible} onClose={() => { setDetailVisible(false); setSelectedJob(null); }} styles={{ wrapper: { width: 520 } }}>
        {selectedJob && (
          <Space direction="vertical" style={{ width: '100%' }}>
            <div><Text strong>Status: </Text><Tag color={statusColor[selectedJob.status]}>{selectedJob.status}</Tag></div>
            <div><Text strong>Department: </Text><Text>{selectedJob.department || '-'}</Text></div>
            <div><Text strong>Created: </Text><Text>{new Date(selectedJob.createdAt).toLocaleDateString()}</Text></div>
            <div><Text strong>Candidates: </Text><Text>{selectedJob.candidateCount}</Text></div>
            {selectedJob.closedAt && <div><Text strong>Closed: </Text><Text>{new Date(selectedJob.closedAt).toLocaleDateString()}</Text></div>}
            <Card title="Description" size="small" style={{ borderRadius: 8, width: '100%' }}>
              <Text>{selectedJob.description || 'No description'}</Text>
            </Card>
            <Card title="Requirements" size="small" style={{ borderRadius: 8, width: '100%' }}>
              <Text style={{ whiteSpace: 'pre-wrap' }}>{selectedJob.requirements || 'No requirements specified'}</Text>
            </Card>
          </Space>
        )}
      </Drawer>

      {/* Edit Job Modal */}
      <Modal title="Edit Job Requisition" open={editModalVisible} onCancel={() => { setEditModalVisible(false); setEditingJob(null); }} onOk={() => editForm.submit()}>
        <Form form={editForm} onFinish={handleEditRequisition} layout="vertical">
          <Form.Item name="title" label="Title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea /></Form.Item>
          <Form.Item name="requirements" label="Requirements"><Input.TextArea /></Form.Item>
          <Form.Item name="departmentId" label="Department">
            <Select allowClear placeholder="Select department">
              {departments.map((d: any) => <Select.Option key={d.id} value={d.id}>{d.name}</Select.Option>)}
            </Select>
          </Form.Item>
          <Form.Item name="status" label="Status" rules={[{ required: true }]}>
            <Select>
              <Select.Option value="Open">Open</Select.Option>
              <Select.Option value="Closed">Closed</Select.Option>
            </Select>
          </Form.Item>
        </Form>
      </Modal>

      {/* AI Screen Modal */}
      <Modal title={<><RobotOutlined /> AI Screen: {screenCandidate?.firstName} {screenCandidate?.lastName}</>} open={screenModal} onCancel={() => setScreenModal(false)} footer={null} width={640}>
        <Space orientation="vertical" style={{ width: '100%' }}>
          <Text strong>Job Description</Text>
          <Input.TextArea rows={4} value={screenJobDesc} onChange={e => setScreenJobDesc(e.target.value)} placeholder="Paste job description..." />
          <Text strong>Resume / Summary</Text>
          <Input.TextArea rows={4} value={screenResume} onChange={e => setScreenResume(e.target.value)} placeholder="Paste resume..." />
          <Button type="primary" icon={<RobotOutlined />} onClick={handleScreen} loading={screenLoading} style={{ background: '#6c5ce7' }} disabled={!screenJobDesc || !screenResume}>Analyze</Button>
          {screenLoading && <Spin style={{ display: 'block', margin: 20 }} />}
          {screenResult && <Card size="small" style={{ borderRadius: 8, background: '#f5f0ff' }}><Text style={{ whiteSpace: 'pre-wrap' }}>{screenResult}</Text></Card>}
        </Space>
      </Modal>

      {/* AI Questions Modal */}
      <Modal title={<><QuestionCircleOutlined /> AI Interview Questions</>} open={questionsModal} onCancel={() => setQuestionsModal(false)} footer={null} width={640}>
        <Space orientation="vertical" style={{ width: '100%' }}>
          <Text strong>Role</Text><Input value={questionsRole} onChange={e => setQuestionsRole(e.target.value)} placeholder="e.g., Software Engineer" />
          <Text strong>Profile</Text><Input.TextArea rows={3} value={questionsProfile} onChange={e => setQuestionsProfile(e.target.value)} />
          <Button type="primary" icon={<BulbOutlined />} onClick={handleGenerateQuestions} loading={questionsLoading} style={{ background: '#6c5ce7' }} disabled={!questionsRole}>Generate</Button>
          {questionsLoading && <Spin style={{ display: 'block', margin: 20 }} />}
          {questionsResult && <Card size="small" style={{ borderRadius: 8, background: '#fff7e6' }}><Text style={{ whiteSpace: 'pre-wrap' }}>{questionsResult}</Text></Card>}
        </Space>
      </Modal>

      {/* Reject Hiring Request Modal */}
      <Modal
        title="Reject Hiring Request"
        open={rejectHiringOpen}
        onCancel={() => { setRejectHiringOpen(false); setRejectHiringId(null); setRejectHiringReason(''); }}
        onOk={confirmRejectHiring}
        okText="Reject"
        okButtonProps={{ danger: true }}
      >
        <p style={{ marginBottom: 12 }}>Are you sure you want to reject this hiring request?</p>
        <Input.TextArea
          placeholder="Please provide a reason for rejection..."
          value={rejectHiringReason}
          onChange={e => setRejectHiringReason(e.target.value)}
          rows={3}
        />
      </Modal>

      {/* Reject Offer Modal */}
      <Modal
        title="Reject Offer"
        open={rejectOfferOpen}
        onCancel={() => { setRejectOfferOpen(false); setRejectOfferId(null); setRejectOfferReason(''); }}
        onOk={confirmRejectOffer}
        okText="Reject"
        okButtonProps={{ danger: true }}
      >
        <p style={{ marginBottom: 12 }}>Are you sure you want to reject this offer?</p>
        <Input.TextArea
          placeholder="Please provide a reason for rejection..."
          value={rejectOfferReason}
          onChange={e => setRejectOfferReason(e.target.value)}
          rows={3}
        />
      </Modal>

      {/* Add Candidate Modal */}
      <Modal title="Add Candidate" open={candidateModal} onCancel={() => setCandidateModal(false)} onOk={() => form.submit()}>
        <Form form={form} onFinish={handleAddCandidate} layout="vertical">
          <Form.Item name="firstName" label="First Name" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="lastName" label="Last Name" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="email" label="Email" rules={[{ required: true, type: 'email' }]}><Input /></Form.Item>
          <Form.Item name="phone" label="Phone"><Input /></Form.Item>
          <Form.Item name="source" label="Source"><Input /></Form.Item>
          <Form.Item name="jobRequisitionId" label="Job Requisition"><Select allowClear>{requisitions.map(r => <Select.Option key={r.id} value={r.id}>{r.title}</Select.Option>)}</Select></Form.Item>
        </Form>
      </Modal>

      {/* New Job Modal */}
      <Modal title="New Job Requisition" open={jobModal} onCancel={() => setJobModal(false)} onOk={() => jobForm.submit()}>
        <Form form={jobForm} onFinish={handleAddRequisition} layout="vertical">
          <Form.Item name="title" label="Title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea /></Form.Item>
          <Form.Item name="requirements" label="Requirements"><Input.TextArea /></Form.Item>
          <Form.Item name="departmentId" label="Department">
            <Select allowClear placeholder="Select department">
              {departments.map((d: any) => <Select.Option key={d.id} value={d.id}>{d.name}</Select.Option>)}
            </Select>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}

export default RecruitmentPage;
