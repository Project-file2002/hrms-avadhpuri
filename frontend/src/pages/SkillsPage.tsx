import { useEffect, useState } from 'react';
import { Card, Button, Modal, Form, Input, Select, Tag, Typography, Space, Row, Col, Statistic, Progress, Table, message, Tabs, Tooltip } from 'antd';
import { PlusOutlined, DeleteOutlined, BulbOutlined, TeamOutlined, TrophyOutlined, SafetyCertificateOutlined, CheckCircleOutlined, CloseCircleOutlined } from '@ant-design/icons';
import api from '../services/api';

const { Title, Text } = Typography;

const catColors: Record<string, string> = {
  Technical: 'blue', Soft: 'green', Domain: 'purple', Language: 'orange',
};

function SkillsPage() {
  const [skills, setSkills] = useState<any[]>([]);
  const [employees, setEmployees] = useState<any[]>([]);
  const [empSkills, setEmpSkills] = useState<any[]>([]);
  const [pools, setPools] = useState<any[]>([]);
  const [analytics, setAnalytics] = useState<any>({});
  const [loading, setLoading] = useState(true);
  const [skillModal, setSkillModal] = useState(false);
  const [empSkillModal, setEmpSkillModal] = useState(false);
  const [poolModal, setPoolModal] = useState(false);
  const [selectedEmp, setSelectedEmp] = useState<number | null>(null);
  const [allSkills, setAllSkills] = useState<any[]>([]);
  const [form] = Form.useForm();
  const [empSkillForm] = Form.useForm();
  const [poolForm] = Form.useForm();

  // Gap analysis state
  const [gapEmployeeId, setGapEmployeeId] = useState<number | null>(null);
  const [gapData, setGapData] = useState<any>(null);
  const [orgGaps, setOrgGaps] = useState<any[]>([]);
  const [positionReqs, setPositionReqs] = useState<any[]>([]);
  const [posReqModal, setPosReqModal] = useState(false);
  const [posReqForm] = Form.useForm();

  useEffect(() => {
    Promise.all([
      api.get('/skills'),
      api.get('/employees'),
      api.get('/skills/analytics'),
      api.get('/skills/pools'),
      api.get('/skills/employee-gap'),
      api.get('/skills/position-requirements'),
    ]).then(([s, e, a, p, og, pr]) => {
      setSkills(s.data); setEmployees(e.data); setAnalytics(a.data); setPools(p.data); setOrgGaps(og.data); setPositionReqs(pr.data);
      setAllSkills(s.data);
    }).finally(() => setLoading(false));
  }, []);

  const fetchEmpSkills = async (empId: number) => {
    setSelectedEmp(empId);
    const res = await api.get(`/skills/employee/${empId}`);
    setEmpSkills(res.data);
  };

  const fetchGapData = async (empId: number) => {
    setGapEmployeeId(empId);
    const res = await api.get(`/skills/employee-gap/${empId}`);
    setGapData(res.data);
  };

  const fetchOrgGaps = async () => {
    const res = await api.get('/skills/employee-gap');
    setOrgGaps(res.data);
  };

  const fetchPositionReqs = async () => {
    const res = await api.get('/skills/position-requirements');
    setPositionReqs(res.data);
  };

  const handleCreateSkill = async (values: any) => {
    await api.post('/skills', values);
    setSkillModal(false); form.resetFields();
    const res = await api.get('/skills'); setSkills(res.data); setAllSkills(res.data);
  };

  const handleDeleteSkill = async (id: number) => {
    await api.delete(`/skills/${id}`);
    setSkills(skills.filter(s => s.id !== id)); setAllSkills(allSkills.filter(s => s.id !== id));
  };

  const handleAddEmpSkill = async (values: any) => {
    await api.post('/skills/employee', values);
    setEmpSkillModal(false); empSkillForm.resetFields();
    if (selectedEmp) fetchEmpSkills(selectedEmp);
  };

  const handleRemoveEmpSkill = async (id: number) => {
    await api.delete(`/skills/employee/${id}`);
    if (selectedEmp) fetchEmpSkills(selectedEmp);
  };

  const handleCreatePool = async (values: any) => {
    await api.post('/skills/pools', values);
    setPoolModal(false); poolForm.resetFields();
    const res = await api.get('/skills/pools'); setPools(res.data);
  };

  const handleAddToPool = async (poolId: number, employeeId: number) => {
    await api.post(`/skills/pools/${poolId}/candidates`, { employeeId });
    const res = await api.get('/skills/pools'); setPools(res.data);
  };

  const handleRemoveFromPool = async (id: number) => {
    await api.delete(`/skills/pools/candidates/${id}`);
    const res = await api.get('/skills/pools'); setPools(res.data);
  };

  const handleAddPositionReq = async (values: any) => {
    await api.post('/skills/position-requirements', values);
    setPosReqModal(false); posReqForm.resetFields();
    fetchPositionReqs();
  };

  const handleDeletePositionReq = async (id: number) => {
    await api.delete(`/skills/position-requirements/${id}`);
    fetchPositionReqs();
  };

  const skillCols = [
    { title: 'Name', dataIndex: 'name' },
    { title: 'Category', dataIndex: 'category', render: (c: string) => <Tag color={catColors[c] || 'default'}>{c}</Tag> },
    { title: '', key: 'action', render: (_: any, r: any) => <Button type="text" danger icon={<DeleteOutlined />} onClick={() => handleDeleteSkill(r.id)} /> },
  ];

  const empSkillCols = [
    { title: 'Skill', render: (_: any, r: any) => r.skill.name },
    { title: 'Category', render: (_: any, r: any) => <Tag color={catColors[r.skill.category]}>{r.skill.category}</Tag> },
    { title: 'Level', dataIndex: 'proficiencyLevel', render: (v: number) => <Progress percent={v * 20} size="small" format={() => `${v}/5`} /> },
    { title: '', render: (_: any, r: any) => <Button type="text" danger icon={<DeleteOutlined />} onClick={() => handleRemoveEmpSkill(r.id)} /> },
  ];

  return (
    <div>
      <div className="page-intro">
        <Title level={4} style={{ margin: 0 }}>
          <BulbOutlined style={{ marginRight: 8, color: '#6c5ce7' }} />
          Skills & Talent
        </Title>
      </div>

      <Tabs items={[
        {
          key: 'overview', label: 'Overview',
          children: (
            <Row gutter={[16, 16]}>
              <Col xs={12} sm={6}>
                <Card style={{ borderRadius: 12, textAlign: 'center' }}>
                  <Statistic title="Skills" value={analytics.totalSkills} styles={{ content: {  color: '#6c5ce7'  } }} />
                </Card>
              </Col>
              <Col xs={12} sm={6}>
                <Card style={{ borderRadius: 12, textAlign: 'center' }}>
                  <Statistic title="Employees w/ Skills" value={analytics.employeesWithSkills} styles={{ content: {  color: '#1890ff'  } }} />
                </Card>
              </Col>
              <Col xs={12} sm={6}>
                <Card style={{ borderRadius: 12, textAlign: 'center' }}>
                  <Statistic title="Coverage" value={analytics.skillCoverage} styles={{ content: {  color: '#52c41a'  } }} />
                </Card>
              </Col>
              <Col xs={12} sm={6}>
                <Card style={{ borderRadius: 12, textAlign: 'center' }}>
                  <Statistic title="Talent Pools" value={pools.length} styles={{ content: {  color: '#fa8c16'  } }} />
                </Card>
              </Col>
              <Col xs={24} sm={12}>
                <Card title="Skill Categories" style={{ borderRadius: 12 }}>
                  {analytics.categories?.map((c: any) => (
                    <div key={c.category} style={{ marginBottom: 8 }}>
                      <Text>{c.category}</Text>
                      <Progress percent={Math.round(c.count / Math.max(...analytics.categories.map((x: any) => x.count)) * 100)} size="small" />
                    </div>
                  ))}
                </Card>
              </Col>
              <Col xs={24} sm={12}>
                <Card title="Scarce Skills (Lowest Coverage)" style={{ borderRadius: 12 }}>
                  {analytics.scarceSkills?.map((s: any) => (
                    <div key={s.name} style={{ marginBottom: 8, display: 'flex', justifyContent: 'space-between' }}>
                      <Text>{s.name}</Text>
                      <Tag>{s.count} employee{s.count !== 1 ? 's' : ''}</Tag>
                    </div>
                  ))}
                </Card>
              </Col>
            </Row>
          ),
        },
        {
          key: 'skills', label: 'Skills',
          children: (
            <>
              <div style={{ marginBottom: 16 }}><Button type="primary" icon={<PlusOutlined />} onClick={() => setSkillModal(true)}>Add Skill</Button></div>
              <Card style={{ borderRadius: 12 }}>
                <Table dataSource={skills} columns={skillCols} rowKey="id" loading={loading} pagination={false} scroll={{ x: 'max-content' }} />
              </Card>
            </>
          ),
        },
        {
          key: 'employees', label: 'Employee Skills',
          children: (
            <Row gutter={16}>
              <Col xs={24} lg={8}>
                <Card title="Select Employee" style={{ borderRadius: 12 }}>
                  <Select showSearch style={{ width: '100%' }} placeholder="Search employee..."
                    filterOption={(i, o) => (o?.label as string)?.toLowerCase()?.includes(i?.toLowerCase() ?? '')}
                    onChange={fetchEmpSkills}
                    options={employees.map((e: any) => ({ label: `${e.firstName} ${e.lastName} — ${e.position}`, value: e.id }))}
                  />
                  <div style={{ marginTop: 16 }}>
                    <Button type="dashed" icon={<PlusOutlined />} onClick={() => { setEmpSkillModal(true); empSkillForm.setFieldsValue({ employeeId: selectedEmp }); }} block disabled={!selectedEmp}>Add Skill</Button>
                  </div>
                </Card>
              </Col>
              <Col xs={24} lg={16}>
                <Card style={{ borderRadius: 12 }}>
                  <Table dataSource={empSkills} columns={empSkillCols} rowKey="id" pagination={false} locale={{ emptyText: 'Select an employee to view skills' }} scroll={{ x: 'max-content' }} />
                </Card>
              </Col>
            </Row>
          ),
        },
        {
          key: 'pools', label: <><TrophyOutlined /> Talent Pools ({pools.length})</>,
          children: (
            <>
              <div style={{ marginBottom: 16 }}><Button type="primary" icon={<PlusOutlined />} onClick={() => setPoolModal(true)}>New Pool</Button></div>
              <Row gutter={[16, 16]}>
                {pools.map((pool: any) => (
                  <Col xs={24} sm={12} lg={8} key={pool.id}>
                    <Card style={{ borderRadius: 12 }} title={pool.name} extra={<Tag>{pool.candidateCount} candidates</Tag>}>
                      <Text type="secondary">{pool.description}</Text>
                      <div style={{ marginTop: 12 }}>
                        {pool.candidates?.map((c: any) => (
                          <div key={c.id} style={{ display: 'flex', justifyContent: 'space-between', padding: '4px 0', borderBottom: '1px solid #f0f0f0' }}>
                            <div><Text>{c.employeeName}</Text><br /><Text type="secondary" style={{ fontSize: 12 }}>{c.position}</Text></div>
                            <Button type="text" danger size="small" icon={<DeleteOutlined />} onClick={() => handleRemoveFromPool(c.id)} />
                          </div>
                        ))}
                        {pool.candidates?.length === 0 && <Text type="secondary">No candidates yet</Text>}
                      </div>
                      <Select showSearch style={{ width: '100%', marginTop: 8 }} placeholder="Add employee..."
                        filterOption={(i, o) => (o?.label as string)?.toLowerCase()?.includes(i?.toLowerCase() ?? '')}
                        onChange={empId => handleAddToPool(pool.id, empId)}
                        notFoundContent={null}
                        options={employees.map((e: any) => ({ label: `${e.firstName} ${e.lastName}`, value: e.id }))}
                      />
                    </Card>
                  </Col>
                ))}
              </Row>
            </>
          ),
        },
        {
          key: 'gap', label: <><SafetyCertificateOutlined /> Gap Analysis</>,
          children: (
            <Row gutter={[16, 16]}>
              <Col xs={24} lg={10}>
                <Card title="Skill Readiness by Position" style={{ borderRadius: 12 }}>
                  <Table dataSource={orgGaps} rowKey="employeeId" pagination={false} size="small"
                    columns={[
                      { title: 'Employee', dataIndex: 'employeeName', ellipsis: true },
                      { title: 'Position', dataIndex: 'position', ellipsis: true },
                      { title: 'Ready', render: (_: any, r: any) => `${r.metSkills}/${r.requiredSkills}` },
                      { title: '', render: (_: any, r: any) => <Progress percent={r.coveragePercent} size="small" format={() => `${r.coveragePercent}%`} /> },
                    ]}
                    onRow={(r) => ({ onClick: () => fetchGapData(r.employeeId), style: { cursor: 'pointer', background: gapEmployeeId === r.employeeId ? '#f0f5ff' : undefined } })}
                  />
                </Card>
              </Col>
              <Col xs={24} lg={14}>
                <Card title="Employee Skill Gap" style={{ borderRadius: 12 }} extra={
                  <Select showSearch style={{ minWidth: 220 }} placeholder="Select employee..."
                    filterOption={(i, o) => (o?.label as string)?.toLowerCase()?.includes(i?.toLowerCase() ?? '')}
                    onChange={fetchGapData}
                    value={gapEmployeeId}
                    options={employees.map((e: any) => ({ label: `${e.firstName} ${e.lastName} — ${e.position || 'No position'}`, value: e.id }))}
                  />
                }>
                  {!gapData ? (
                    <Text type="secondary">Select an employee to view skill gaps</Text>
                  ) : !gapData.hasPosition ? (
                    <Text type="secondary">This employee has no position assigned. Set a position first.</Text>
                  ) : (
                    <>
                      <div style={{ marginBottom: 16, display: 'flex', alignItems: 'center', gap: 16, flexWrap: 'wrap' }}>
                        <Tag color="blue" style={{ fontSize: 13, padding: '2px 12px' }}>{gapData.position}</Tag>
                        <Progress type="circle" percent={gapData.coveragePercent} size={60} strokeColor={gapData.coveragePercent >= 80 ? '#52c41a' : gapData.coveragePercent >= 50 ? '#faad14' : '#ff4d4f'} format={() => `${gapData.coveragePercent}%`} />
                        <Text type="secondary">{gapData.gaps.filter((g: any) => !g.met).length} skill{gapData.gaps.filter((g: any) => !g.met).length !== 1 ? 's' : ''} to improve</Text>
                      </div>
                      <Table dataSource={gapData.gaps} rowKey="skillId" pagination={false} size="small"
                        columns={[
                          { title: 'Skill', dataIndex: 'skillName' },
                          { title: 'Category', dataIndex: 'skillCategory', render: (c: string) => <Tag color={catColors[c] || 'default'} style={{ fontSize: 11 }}>{c}</Tag> },
                          { title: 'Required', dataIndex: 'requiredProficiency', render: (v: number) => <Tag color="orange">{v}/5</Tag> },
                          { title: 'Current', dataIndex: 'currentProficiency', render: (v: number) => v > 0 ? <Tag color="blue">{v}/5</Tag> : <Tag>—</Tag> },
                          {
                            title: 'Status', dataIndex: 'met', render: (met: boolean, r: any) => met
                              ? <Tag icon={<CheckCircleOutlined />} color="success">Met</Tag>
                              : <Tooltip title={`Gap: ${r.gap} level${r.gap > 1 ? 's' : ''}`}><Tag icon={<CloseCircleOutlined />} color="error">Gap ({r.gap})</Tag></Tooltip>
                          },
                        ]}
                      />
                      {gapData.gaps.filter((g: any) => !g.met).length > 0 && (
                        <Card size="small" style={{ marginTop: 12, background: '#fffbe6', border: '1px solid #ffe58f', borderRadius: 8 }}>
                          <div style={{ display: 'flex', alignItems: 'flex-start', gap: 8 }}>
                            <BulbOutlined style={{ color: '#faad14', fontSize: 18, marginTop: 2 }} />
                            <div>
                              <Text strong style={{ fontSize: 13 }}>Suggested Focus Areas</Text>
                              <ul style={{ margin: '4px 0 0', paddingLeft: 20 }}>
                                {gapData.gaps.filter((g: any) => !g.met).slice(0, 5).map((g: any) => (
                                  <li key={g.skillId}><Text style={{ fontSize: 12 }}>Upgrade <strong>{g.skillName}</strong> from level {g.currentProficiency} → {g.requiredProficiency} (needs +{g.gap})</Text></li>
                                ))}
                              </ul>
                            </div>
                          </div>
                        </Card>
                      )}
                    </>
                  )}
                </Card>
              </Col>
            </Row>
          ),
        },
        {
          key: 'posreq', label: 'Position Requirements',
          children: (
            <>
              <div style={{ marginBottom: 16 }}>
                <Button type="primary" icon={<PlusOutlined />} onClick={() => { setPosReqModal(true); fetchPositionReqs(); }}>Add Requirement</Button>
              </div>
              <Card style={{ borderRadius: 12 }}>
                <Table dataSource={positionReqs} rowKey="id" pagination={false} size="small"
                  columns={[
                    { title: 'Position', dataIndex: 'position', ellipsis: true },
                    { title: 'Skill', render: (_: any, r: any) => r.skill.name },
                    { title: 'Category', render: (_: any, r: any) => <Tag color={catColors[r.skill.category]}>{r.skill.category}</Tag> },
                    { title: 'Min Proficiency', dataIndex: 'minimumProficiency', render: (v: number) => <Tag color="orange">{v}/5</Tag> },
                    { title: '', render: (_: any, r: any) => <Button type="text" danger icon={<DeleteOutlined />} onClick={() => handleDeletePositionReq(r.id)} /> },
                  ]}
                />
              </Card>
            </>
          ),
        },
      ]} />

      <Modal title="Add Skill" open={skillModal} onCancel={() => setSkillModal(false)} onOk={() => form.submit()}>
        <Form form={form} onFinish={handleCreateSkill} layout="vertical">
          <Form.Item name="name" label="Name" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="category" label="Category" initialValue="Technical">
            <Select>{['Technical', 'Soft', 'Domain', 'Language'].map(c => <Select.Option key={c} value={c}>{c}</Select.Option>)}</Select>
          </Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea /></Form.Item>
        </Form>
      </Modal>

      <Modal title="Add Employee Skill" open={empSkillModal} onCancel={() => setEmpSkillModal(false)} onOk={() => empSkillForm.submit()}>
        <Form form={empSkillForm} onFinish={handleAddEmpSkill} layout="vertical">
          <Form.Item name="employeeId" hidden><Input /></Form.Item>
          <Form.Item name="skillId" label="Skill" rules={[{ required: true }]}>
            <Select>{allSkills.map(s => <Select.Option key={s.id} value={s.id}>{s.name} ({s.category})</Select.Option>)}</Select>
          </Form.Item>
          <Form.Item name="proficiencyLevel" label="Proficiency (1-5)" initialValue={3}>
            <Select>{[1,2,3,4,5].map(n => <Select.Option key={n} value={n}>{n} - {['Beginner','Elementary','Intermediate','Advanced','Expert'][n-1]}</Select.Option>)}</Select>
          </Form.Item>
        </Form>
      </Modal>

      <Modal title="New Talent Pool" open={poolModal} onCancel={() => setPoolModal(false)} onOk={() => poolForm.submit()}>
        <Form form={poolForm} onFinish={handleCreatePool} layout="vertical">
          <Form.Item name="name" label="Name" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea /></Form.Item>
        </Form>
      </Modal>

      <Modal title="Add Position Skill Requirement" open={posReqModal} onCancel={() => setPosReqModal(false)} onOk={() => posReqForm.submit()}>
        <Form form={posReqForm} onFinish={handleAddPositionReq} layout="vertical">
          <Form.Item name="position" label="Position Title" rules={[{ required: true }]}><Input placeholder="e.g. Software Engineer" /></Form.Item>
          <Form.Item name="skillId" label="Required Skill" rules={[{ required: true }]}>
            <Select>{allSkills.map(s => <Select.Option key={s.id} value={s.id}>{s.name} ({s.category})</Select.Option>)}</Select>
          </Form.Item>
          <Form.Item name="minimumProficiency" label="Minimum Proficiency (1-5)" initialValue={3}>
            <Select>{[1,2,3,4,5].map(n => <Select.Option key={n} value={n}>{n} - {['Beginner','Elementary','Intermediate','Advanced','Expert'][n-1]}</Select.Option>)}</Select>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}

export default SkillsPage;
