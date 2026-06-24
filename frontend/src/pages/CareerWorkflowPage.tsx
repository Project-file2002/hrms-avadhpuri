import { useEffect, useState } from 'react';
import { Table, Tabs, Button, Modal, Form, Input, Select, Tag, Typography, Space, Card, InputNumber, Progress, Tooltip } from 'antd';
import { PlusOutlined, CheckCircleOutlined, CloseCircleOutlined, SwapOutlined, RiseOutlined, SafetyCertificateOutlined } from '@ant-design/icons';
import api from '../services/api';
import { formatINR, formatSalaryChange, INR_PREFIX } from '../utils/currency';
import { useRoles } from '../utils/roles';

const { Title, Text } = Typography;

const statusColor: Record<string, string> = {
  PendingManagerApproval: 'orange', PendingHrbpApproval: 'volcano', PendingDeptHeadApproval: 'gold',
  PendingCeoApproval: 'purple', Approved: 'green', Rejected: 'red',
  PendingHrApproval: 'volcano', PendingDepartmentApproval: 'gold', PendingItApproval: 'cyan',
  PendingPayrollApproval: 'geekblue', PendingEmployeeAcceptance: 'lime', Completed: 'green',
};

const statusLabel = (s: string) => s.replace(/([A-Z])/g, ' $1').trim();

const stageOrder: Record<string, string> = {
  PendingManagerApproval: 'Manager', PendingHrbpApproval: 'HRBP',
  PendingDeptHeadApproval: 'Dept Head', PendingCeoApproval: 'CEO',
  PendingHrApproval: 'HR', PendingDepartmentApproval: 'Department',
  PendingItApproval: 'IT', PendingPayrollApproval: 'Payroll',
  PendingEmployeeAcceptance: 'Employee',
};

function CareerWorkflowPage() {
  const { canCreateCareerRequest, canApproveCareerStep } = useRoles();

  const [employees, setEmployees] = useState<any[]>([]);
  const [departments, setDepartments] = useState<any[]>([]);
  const [promotions, setPromotions] = useState<any[]>([]);
  const [transfers, setTransfers] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);

  const [promModal, setPromModal] = useState(false);
  const [transferModal, setTransferModal] = useState(false);
  const [detailModal, setDetailModal] = useState<any>(null);
  const [detailType, setDetailType] = useState<'promotion' | 'transfer'>('promotion');
  const [rejectOpen, setRejectOpen] = useState(false);
  const [rejectType, setRejectType] = useState<'promotions' | 'transfers'>('promotions');
  const [rejectId, setRejectId] = useState<number | null>(null);
  const [rejectReason, setRejectReason] = useState('');

  const [skillReadiness, setSkillReadiness] = useState<any>(null);
  const [skillLoading, setSkillLoading] = useState(false);

  const [promForm] = Form.useForm();
  const [transferForm] = Form.useForm();

  const fetchData = async () => {
    setLoading(true);
    try {
      const requests = [
        api.get('/workflow/promotions'),
        api.get('/workflow/transfers'),
      ];
      if (canCreateCareerRequest) {
        requests.unshift(api.get('/employees'), api.get('/departments'));
      }
      const results = await Promise.all(requests);
      if (canCreateCareerRequest) {
        setEmployees(results[0].data);
        setDepartments(results[1].data);
        setPromotions(results[2].data);
        setTransfers(results[3].data);
      } else {
        setPromotions(results[0].data);
        setTransfers(results[1].data);
      }
    } finally { setLoading(false); }
  };

  useEffect(() => { fetchData(); }, [canCreateCareerRequest]);

  const handleCreatePromotion = async (values: any) => {
    await api.post('/workflow/promotions', values);
    setPromModal(false); promForm.resetFields(); fetchData();
  };

  const handleCreateTransfer = async (values: any) => {
    await api.post('/workflow/transfers', values);
    setTransferModal(false); transferForm.resetFields(); fetchData();
  };

  const handleApprove = async (type: string, id: number, approved: boolean, notes?: string) => {
    await api.put(`/workflow/${type}/${id}/approve`, { approved, notes: notes ?? '' });
    fetchData();
  };

  const openRejectModal = (type: 'promotions' | 'transfers', id: number) => {
    setRejectType(type);
    setRejectId(id);
    setRejectReason('');
    setRejectOpen(true);
  };

  const confirmReject = async () => {
    if (rejectId == null) return;
    await handleApprove(rejectType, rejectId, false, rejectReason);
    setRejectOpen(false);
    setRejectId(null);
    setRejectReason('');
  };

  const showDetail = async (type: 'promotion' | 'transfer', item: any) => {
    setDetailType(type);
    setDetailModal(item);
    if (type === 'promotion') {
      setSkillLoading(true);
      try {
        const res = await api.get(`/skills/employee-gap/${item.employeeId}`);
        setSkillReadiness(res.data);
      } catch { setSkillReadiness(null); }
      setSkillLoading(false);
    }
  };

  const editablePromStatuses = ['PendingManagerApproval', 'PendingHrbpApproval', 'PendingDeptHeadApproval', 'PendingCeoApproval'];
  const editableTransStatuses = ['PendingManagerApproval', 'PendingHrApproval', 'PendingDepartmentApproval', 'PendingItApproval', 'PendingPayrollApproval', 'PendingEmployeeAcceptance'];

  const promCols = [
    { title: 'Employee', dataIndex: 'employeeName' },
    { title: 'Current', dataIndex: 'currentPosition' },
    { title: 'Proposed', dataIndex: 'proposedPosition' },
    { title: 'Salary', render: (_: any, r: any) => formatSalaryChange(r.currentSalary, r.proposedSalary) },
    { title: 'Status', dataIndex: 'status', render: (s: string) => <Tag color={statusColor[s]}>{statusLabel(s)}</Tag> },
    { title: '', key: 'action', width: 150,
      render: (_: any, r: any) => (
        <Space>
          <Button size="small" onClick={() => showDetail('promotion', r)}>View</Button>
          {editablePromStatuses.includes(r.status) && canApproveCareerStep(r.status, 'promotion') && (
            <>
              <Button size="small" type="primary" icon={<CheckCircleOutlined />} onClick={() => handleApprove('promotions', r.id, true)} />
              <Button size="small" danger icon={<CloseCircleOutlined />} onClick={() => openRejectModal('promotions', r.id)} />
            </>
          )}
        </Space>
      ),
    },
  ];

  const transCols = [
    { title: 'Employee', dataIndex: 'employeeName' },
    { title: 'From', dataIndex: 'currentDepartmentName' },
    { title: 'To', dataIndex: 'proposedDepartmentName' },
    { title: 'Position', dataIndex: 'proposedPosition' },
    { title: 'Status', dataIndex: 'status', render: (s: string) => <Tag color={statusColor[s]}>{statusLabel(s)}</Tag> },
    { title: '', key: 'action', width: 150,
      render: (_: any, r: any) => (
        <Space>
          <Button size="small" onClick={() => showDetail('transfer', r)}>View</Button>
          {editableTransStatuses.includes(r.status) && canApproveCareerStep(r.status, 'transfer') && (
            <>
              <Button size="small" type="primary" icon={<CheckCircleOutlined />} onClick={() => handleApprove('transfers', r.id, true)} />
              <Button size="small" danger icon={<CloseCircleOutlined />} onClick={() => openRejectModal('transfers', r.id)} />
            </>
          )}
        </Space>
      ),
    },
  ];

  const statusSteps = (status: string, isPromotion: boolean) => {
    const steps = isPromotion
      ? ['PendingManagerApproval', 'PendingHrbpApproval', 'PendingDeptHeadApproval', 'PendingCeoApproval', 'Approved']
      : ['PendingManagerApproval', 'PendingHrApproval', 'PendingDepartmentApproval', 'PendingItApproval', 'PendingPayrollApproval', 'PendingEmployeeAcceptance', 'Completed'];
    const idx = steps.indexOf(status);
    return (
      <div style={{ display: 'flex', gap: 0, marginTop: 8, flexWrap: 'wrap' }}>
        {steps.map((s, i) => (
          <div key={s} style={{
            padding: '2px 8px', borderRadius: 4, marginRight: 4, marginBottom: 4,
            fontSize: 11, background: i <= idx ? '#6c5ce7' : '#f0f0f0',
            color: i <= idx ? '#fff' : '#999',
          }}>
            {statusLabel(s)}
          </div>
        ))}
      </div>
    );
  };

  return (
    <div>
      <div className="page-intro">
        <Title level={4} style={{ margin: 0 }}>
          <RiseOutlined style={{ marginRight: 8, color: '#6c5ce7' }} />
          Career Workflows
        </Title>
      </div>

      <Tabs items={[
        {
          key: 'promotions', label: <><RiseOutlined /> Promotions ({promotions.length})</>,
          children: (
            <>
              {canCreateCareerRequest && (
                <div style={{ marginBottom: 16 }}><Button type="primary" icon={<PlusOutlined />} onClick={() => setPromModal(true)}>New Promotion</Button></div>
              )}
              <Table dataSource={promotions} columns={promCols} rowKey="id" loading={loading} scroll={{ x: 'max-content' }} />
            </>
          ),
        },
        {
          key: 'transfers', label: <><SwapOutlined /> Transfers ({transfers.length})</>,
          children: (
            <>
              {canCreateCareerRequest && (
                <div style={{ marginBottom: 16 }}><Button type="primary" icon={<PlusOutlined />} onClick={() => setTransferModal(true)}>New Transfer</Button></div>
              )}
              <Table dataSource={transfers} columns={transCols} rowKey="id" loading={loading} scroll={{ x: 'max-content' }} />
            </>
          ),
        },
      ]} />

      {/* Promotion Modal */}
      <Modal title="New Promotion Request" open={promModal} onCancel={() => setPromModal(false)} onOk={() => promForm.submit()}>
        <Form form={promForm} onFinish={handleCreatePromotion} layout="vertical">
          <Form.Item name="employeeId" label="Employee" rules={[{ required: true }]}>
            <Select showSearch filterOption={(i, o) => (o?.label as string)?.toLowerCase()?.includes(i?.toLowerCase() ?? '')}>
              {employees.map((e: any) => <Select.Option key={e.id} value={e.id} label={`${e.firstName} ${e.lastName}`}>{e.firstName} {e.lastName} — {e.position}</Select.Option>)}
            </Select>
          </Form.Item>
          <Form.Item name="currentPosition" label="Current Position" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="currentSalary" label="Current Salary (INR)" rules={[{ required: true }]}><InputNumber prefix={INR_PREFIX} style={{ width: '100%' }} min={0} /></Form.Item>
          <Form.Item name="proposedPosition" label="Proposed Position" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="proposedSalary" label="Proposed Salary (INR)" rules={[{ required: true }]}><InputNumber prefix={INR_PREFIX} style={{ width: '100%' }} min={0} /></Form.Item>
          <Form.Item name="justification" label="Justification"><Input.TextArea rows={2} /></Form.Item>
        </Form>
      </Modal>

      {/* Transfer Modal */}
      <Modal title="New Transfer Request" open={transferModal} onCancel={() => setTransferModal(false)} onOk={() => transferForm.submit()}>
        <Form form={transferForm} onFinish={handleCreateTransfer} layout="vertical">
          <Form.Item name="employeeId" label="Employee" rules={[{ required: true }]}>
            <Select showSearch filterOption={(i, o) => (o?.label as string)?.toLowerCase()?.includes(i?.toLowerCase() ?? '')}>
              {employees.map((e: any) => <Select.Option key={e.id} value={e.id} label={`${e.firstName} ${e.lastName}`}>{e.firstName} {e.lastName} — {e.position}</Select.Option>)}
            </Select>
          </Form.Item>
          <Form.Item name="proposedPosition" label="Proposed Position" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="proposedDepartmentId" label="Target Department" rules={[{ required: true }]}>
            <Select>{departments.map((d: any) => <Select.Option key={d.id} value={d.id}>{d.name}</Select.Option>)}</Select>
          </Form.Item>
          <Form.Item name="reason" label="Reason"><Input.TextArea rows={2} /></Form.Item>
        </Form>
      </Modal>

      {/* Rejection Reason Modal */}
      <Modal
        title={`Reject ${rejectType === 'promotions' ? 'Promotion' : 'Transfer'} Request`}
        open={rejectOpen}
        onCancel={() => { setRejectOpen(false); setRejectId(null); setRejectReason(''); }}
        onOk={confirmReject}
        okText="Reject"
        okButtonProps={{ danger: true }}
      >
        <p style={{ marginBottom: 12 }}>Are you sure you want to reject this request?</p>
        <Input.TextArea
          placeholder="Please provide a reason for rejection..."
          value={rejectReason}
          onChange={e => setRejectReason(e.target.value)}
          rows={3}
        />
      </Modal>

      {/* Detail Modal */}
      <Modal title={detailType === 'promotion' ? 'Promotion Details' : 'Transfer Details'} open={!!detailModal} onCancel={() => setDetailModal(null)} footer={null} width={520}>
        {detailModal && detailType === 'promotion' && (
          <>
            <div><Text strong>Employee:</Text> <Text>{detailModal.employeeName}</Text></div>
            <div style={{ marginTop: 8 }}><Text strong>Current:</Text> <Text>{detailModal.currentPosition} ({formatINR(detailModal.currentSalary, true)})</Text></div>
            <div style={{ marginTop: 4 }}><Text strong>Proposed:</Text> <Text>{detailModal.proposedPosition} ({formatINR(detailModal.proposedSalary, true)})</Text></div>
            <div style={{ marginTop: 4 }}><Text strong>Justification:</Text> <Text>{detailModal.justification || '-'}</Text></div>
            <div style={{ marginTop: 8 }}><Text strong>Status:</Text> <Tag color={statusColor[detailModal.status]}>{statusLabel(detailModal.status)}</Tag></div>
            {statusSteps(detailModal.status, true)}
            <Card size="small" style={{ marginTop: 12, borderRadius: 8 }} title={<><SafetyCertificateOutlined style={{ marginRight: 6 }} />Skill Readiness for {detailModal.proposedPosition}</>}>
              {skillLoading ? <Text type="secondary">Loading...</Text> : !skillReadiness ? (
                <Text type="secondary">No skill requirements defined for this position.</Text>
              ) : !skillReadiness.hasPosition ? (
                <Text type="secondary">Employee has no position set.</Text>
              ) : (
                <>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 8 }}>
                    <Progress type="circle" percent={skillReadiness.coveragePercent} size={50} strokeColor={skillReadiness.coveragePercent >= 80 ? '#52c41a' : skillReadiness.coveragePercent >= 50 ? '#faad14' : '#ff4d4f'} format={() => `${skillReadiness.coveragePercent}%`} />
                    <div>
                      <Text strong>{skillReadiness.gaps.filter((g: any) => g.met).length}/{skillReadiness.gaps.length}</Text> skills met
                      {skillReadiness.gaps.filter((g: any) => !g.met).length > 0 && (
                        <Text type="secondary" style={{ display: 'block', fontSize: 12 }}>{skillReadiness.gaps.filter((g: any) => !g.met).length} skill gap{skillReadiness.gaps.filter((g: any) => !g.met).length > 1 ? 's' : ''} — consider upskilling</Text>
                      )}
                    </div>
                  </div>
                  {skillReadiness.gaps.filter((g: any) => !g.met).length > 0 && (
                    <div style={{ fontSize: 12 }}>
                      {skillReadiness.gaps.filter((g: any) => !g.met).slice(0, 4).map((g: any) => (
                        <Tag key={g.skillId} color="error" style={{ marginBottom: 4 }}>{g.skillName} ({g.currentProficiency}→{g.requiredProficiency})</Tag>
                      ))}
                    </div>
                  )}
                </>
              )}
            </Card>
          </>
        )}
        {detailModal && detailType === 'transfer' && (
          <>
            <div><Text strong>Employee:</Text> <Text>{detailModal.employeeName}</Text></div>
            <div style={{ marginTop: 8 }}><Text strong>From:</Text> <Text>{detailModal.currentDepartmentName} — {detailModal.currentPosition}</Text></div>
            <div style={{ marginTop: 4 }}><Text strong>To:</Text> <Text>{detailModal.proposedDepartmentName} — {detailModal.proposedPosition}</Text></div>
            <div style={{ marginTop: 4 }}><Text strong>Reason:</Text> <Text>{detailModal.reason || '-'}</Text></div>
            <div style={{ marginTop: 8 }}><Text strong>Status:</Text> <Tag color={statusColor[detailModal.status]}>{statusLabel(detailModal.status)}</Tag></div>
            {statusSteps(detailModal.status, false)}
          </>
        )}
      </Modal>
    </div>
  );
}

export default CareerWorkflowPage;
