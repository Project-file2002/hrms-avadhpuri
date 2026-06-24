import { useEffect, useState } from 'react';
import { Card, Button, Modal, Form, Input, Select, Tag, Typography, Space, Table, Row, Col, Statistic, message, InputNumber, Tabs, Progress } from 'antd';
import { PlusOutlined, BookOutlined, TrophyOutlined, TeamOutlined } from '@ant-design/icons';
import api from '../services/api';

const { Title, Text } = Typography;

function TrainingPage() {
  const [courses, setCourses] = useState<any[]>([]);
  const [employees, setEmployees] = useState<any[]>([]);
  const [certs, setCerts] = useState<any[]>([]);
  const [empCerts, setEmpCerts] = useState<any[]>([]);
  const [dashboard, setDashboard] = useState<any>({});
  const [loading, setLoading] = useState(true);
  const [courseModal, setCourseModal] = useState(false);
  const [certModal, setCertModal] = useState(false);
  const [empCertModal, setEmpCertModal] = useState(false);
  const [enrollModal, setEnrollModal] = useState<number | null>(null);
  const [selectedEmp, setSelectedEmp] = useState<number | null>(null);
  const [form] = Form.useForm();
  const [certForm] = Form.useForm();
  const [empCertForm] = Form.useForm();

  useEffect(() => {
    Promise.all([
      api.get('/training/courses'),
      api.get('/training/certifications'),
      api.get('/employees'),
      api.get('/training/dashboard'),
    ]).then(([c, cert, e, d]) => {
      setCourses(c.data); setCerts(cert.data); setEmployees(e.data); setDashboard(d.data);
    }).finally(() => setLoading(false));
  }, []);

  const fetchCourses = async () => { const res = await api.get('/training/courses'); setCourses(res.data); };
  const fetchCerts = async () => { const res = await api.get('/training/certifications'); setCerts(res.data); };
  const fetchEmpCerts = async (id: number) => { setSelectedEmp(id); const res = await api.get(`/training/employee-certifications/${id}`); setEmpCerts(res.data); };

  const handleCreateCourse = async (values: any) => {
    await api.post('/training/courses', values);
    message.success('Course created'); setCourseModal(false); form.resetFields(); fetchCourses();
  };

  const handleEnroll = async (values: any) => {
    await api.post(`/training/courses/${enrollModal}/enroll`, values);
    message.success('Enrolled!'); setEnrollModal(null); fetchCourses();
  };

  const handleUpdateEnroll = async (id: number, status: string, score?: number) => {
    await api.put(`/training/enrollments/${id}`, { status, score: score ?? null });
    fetchCourses();
  };

  const handleCreateCert = async (values: any) => {
    await api.post('/training/certifications', values);
    message.success('Certification created'); setCertModal(false); certForm.resetFields(); fetchCerts();
  };

  const handleAddEmpCert = async (values: any) => {
    await api.post('/training/employee-certifications', values);
    message.success('Certification added'); setEmpCertModal(false); empCertForm.resetFields();
    if (selectedEmp) fetchEmpCerts(selectedEmp);
  };

  const courseCols = [
    { title: 'Title', dataIndex: 'title' },
    { title: 'Category', dataIndex: 'category', render: (c: string) => <Tag>{c}</Tag> },
    { title: 'Duration', dataIndex: 'durationHours', render: (v: number) => `${v}h` },
    { title: 'Capacity', render: (_: any, r: any) => `${r.enrolledCount} / ${r.maxCapacity}` },
    { title: 'Completed', render: (_: any, r: any) => r.completedCount > 0 ? <Tag color="green">{r.completedCount}</Tag> : '-' },
    { title: 'Status', dataIndex: 'status', render: (s: string) => <Tag color={s === 'Active' ? 'green' : 'default'}>{s}</Tag> },
    {
      title: '', key: 'action', width: 200,
      render: (_: any, r: any) => (
        <Space>
          <Button size="small" type="primary" onClick={() => setEnrollModal(r.id)} disabled={r.enrolledCount >= r.maxCapacity}>Enroll</Button>
          <Select size="small" style={{ width: 120 }} placeholder="Status" onChange={v => handleUpdateEnroll(r.enrollments?.[0]?.id, v)}
            options={r.enrollments?.filter((e: any) => e.status !== 'Completed').map((e: any) => ({ label: `${e.employeeName}: ${e.status}`, value: '' })).concat(
              ['Enrolled', 'InProgress', 'Completed'].map(s => ({ label: `Set ${s}`, value: s }))
            )} />
        </Space>
      ),
    },
  ];

  const certCols = [
    { title: 'Name', dataIndex: 'name' },
    { title: 'Issuer', dataIndex: 'issuer' },
    { title: 'Expiry', dataIndex: 'expiryDays', render: (v: number) => `${v} days` },
  ];

  return (
    <div>
      <div className="page-intro">
        <Title level={4} style={{ margin: 0 }}>
          <BookOutlined style={{ marginRight: 8, color: '#6c5ce7' }} />
          Training & Learning
        </Title>
      </div>

      <Tabs items={[
        {
          key: 'overview', label: 'Overview',
          children: (
            <Row gutter={[16, 16]}>
              {[
                { title: 'Courses', value: dashboard.totalCourses, color: '#6c5ce7', icon: <BookOutlined /> },
                { title: 'Active Courses', value: dashboard.activeCourses, color: '#52c41a', icon: <BookOutlined /> },
                { title: 'Enrollments', value: dashboard.totalEnrollments, color: '#1890ff', icon: <TeamOutlined /> },
                { title: 'Completed', value: dashboard.completedEnrollments, color: '#13c2c2', icon: <TrophyOutlined /> },
                { title: 'Certifications', value: dashboard.totalCertifications, color: '#fa8c16', icon: <TrophyOutlined /> },
                { title: 'Employee Certs', value: dashboard.employeeCertifications, color: '#eb2f96', icon: <TrophyOutlined /> },
              ].map(m => (
                <Col xs={12} sm={8} lg={4} key={m.title}>
                  <Card style={{ borderRadius: 12, textAlign: 'center' }}>
                    <Statistic title={m.title} value={m.value || 0} styles={{ content: {  color: m.color  } }} prefix={m.icon} />
                  </Card>
                </Col>
              ))}
            </Row>
          ),
        },
        {
          key: 'courses', label: `Courses (${courses.length})`,
          children: (
            <>
              <div style={{ marginBottom: 16 }}><Button type="primary" icon={<PlusOutlined />} onClick={() => setCourseModal(true)}>New Course</Button></div>
              <Card style={{ borderRadius: 12 }}><Table dataSource={courses} columns={courseCols} rowKey="id" loading={loading} scroll={{ x: 'max-content' }} /></Card>
            </>
          ),
        },
        {
          key: 'certifications', label: `Certifications (${certs.length})`,
          children: (
            <Row gutter={[16, 16]}>
              <Col xs={24} lg={12}>
                <Card title="Certifications" style={{ borderRadius: 12 }} extra={<Button type="primary" size="small" icon={<PlusOutlined />} onClick={() => setCertModal(true)}>Add</Button>}>
                  <Table dataSource={certs} columns={certCols} rowKey="id" pagination={false} scroll={{ x: 'max-content' }} />
                </Card>
              </Col>
              <Col xs={24} lg={12}>
                <Card title="Employee Certifications" style={{ borderRadius: 12 }}>
                  <Select showSearch style={{ width: '100%', marginBottom: 16 }} placeholder="Select employee..."
                    filterOption={(i, o) => (o?.label as string)?.toLowerCase()?.includes(i?.toLowerCase() ?? '')}
                    onChange={fetchEmpCerts}
                    options={employees.map((e: any) => ({ label: `${e.firstName} ${e.lastName}`, value: e.id }))}
                  />
                  <Button type="dashed" icon={<PlusOutlined />} onClick={() => { setEmpCertModal(true); empCertForm.setFieldsValue({ employeeId: selectedEmp }); }} block disabled={!selectedEmp}>Add Certification</Button>
                  {empCerts.map(ec => (
                    <Card key={ec.id} size="small" style={{ marginTop: 8 }}>
                      <Text strong>{ec.certification.name}</Text>
                      <Tag color={ec.status === 'Active' ? 'green' : 'orange'}>{ec.status}</Tag>
                      <br /><Text type="secondary">{ec.certification.issuer} · {new Date(ec.obtainedAt).toLocaleDateString()}</Text>
                      {ec.expiryDate && <><br /><Text type="secondary">Expires: {new Date(ec.expiryDate).toLocaleDateString()}</Text></>}
                    </Card>
                  ))}
                </Card>
              </Col>
            </Row>
          ),
        },
      ]} />

      <Modal title="New Course" open={courseModal} onCancel={() => setCourseModal(false)} onOk={() => form.submit()}>
        <Form form={form} onFinish={handleCreateCourse} layout="vertical">
          <Form.Item name="title" label="Title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea rows={2} /></Form.Item>
          <Space style={{ width: '100%' }} size={12}>
            <Form.Item name="category" label="Category" initialValue="Technical" style={{ flex: 1 }}>
              <Select>{['Technical', 'Soft Skills', 'Compliance', 'Leadership'].map(c => <Select.Option key={c} value={c}>{c}</Select.Option>)}</Select>
            </Form.Item>
            <Form.Item name="instructor" label="Instructor" style={{ flex: 1 }}><Input /></Form.Item>
          </Space>
          <Space style={{ width: '100%' }} size={12}>
            <Form.Item name="durationHours" label="Duration (hrs)" rules={[{ required: true }]} style={{ flex: 1 }}>
              <InputNumber min={0.5} step={0.5} style={{ width: '100%' }} />
            </Form.Item>
            <Form.Item name="maxCapacity" label="Max Capacity" initialValue={20} style={{ flex: 1 }}>
              <InputNumber min={1} style={{ width: '100%' }} />
            </Form.Item>
          </Space>
        </Form>
      </Modal>

      <Modal title="Enroll Employee" open={!!enrollModal} onCancel={() => setEnrollModal(null)} onOk={() => form.submit()}>
        <Form onFinish={handleEnroll} layout="vertical">
          <Form.Item name="employeeId" label="Employee" rules={[{ required: true }]}>
            <Select showSearch filterOption={(i, o) => (o?.label as string)?.toLowerCase()?.includes(i?.toLowerCase() ?? '')}
              options={employees.map((e: any) => ({ label: `${e.firstName} ${e.lastName}`, value: e.id }))} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal title="New Certification" open={certModal} onCancel={() => setCertModal(false)} onOk={() => certForm.submit()}>
        <Form form={certForm} onFinish={handleCreateCert} layout="vertical">
          <Form.Item name="name" label="Name" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea /></Form.Item>
          <Form.Item name="issuer" label="Issuer"><Input /></Form.Item>
          <Form.Item name="expiryDays" label="Expiry (days)" initialValue={365}><InputNumber min={0} style={{ width: '100%' }} /></Form.Item>
        </Form>
      </Modal>

      <Modal title="Add Employee Certification" open={empCertModal} onCancel={() => setEmpCertModal(false)} onOk={() => empCertForm.submit()}>
        <Form form={empCertForm} onFinish={handleAddEmpCert} layout="vertical">
          <Form.Item name="employeeId" hidden><Input /></Form.Item>
          <Form.Item name="certificationId" label="Certification" rules={[{ required: true }]}>
            <Select>{certs.map(c => <Select.Option key={c.id} value={c.id}>{c.name}</Select.Option>)}</Select>
          </Form.Item>
          <Form.Item name="obtainedAt" label="Obtained Date"><Input type="date" /></Form.Item>
          <Form.Item name="expiryDate" label="Expiry Date"><Input type="date" /></Form.Item>
          <Form.Item name="credentialUrl" label="Credential URL"><Input /></Form.Item>
        </Form>
      </Modal>
    </div>
  );
}

export default TrainingPage;
