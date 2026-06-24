import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import dayjs from 'dayjs';
import {
  Layout, Card, Row, Col, Typography, Spin, Button, Space, Divider,
  Input, Form, Select, DatePicker, Upload, Alert, App as AntApp,
} from 'antd';
import {
  ArrowLeftOutlined, UserOutlined, UploadOutlined, SaveOutlined,
  EnvironmentOutlined, LinkOutlined, ToolOutlined, ThunderboltOutlined,
} from '@ant-design/icons';
import careerApi from '../../services/careerApi';
import { useCareerAuthStore } from '../../store/careerAuthStore';

const { Title, Text } = Typography;
const { Content } = Layout;

const PROFESSIONAL_STATUS_OPTIONS = [
  { value: 'Student', label: 'Student' },
  { value: 'Fresher', label: 'Fresher' },
  { value: 'Working', label: 'Working Professional' },
  { value: 'ServingNotice', label: 'Serving Notice Period' },
  { value: 'Freelancer', label: 'Freelancer / Self-Employed' },
  { value: 'CareerBreak', label: 'Career Break' },
  { value: 'LookingInternship', label: 'Looking for Internship' },
];

interface ProfileData {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  gender?: string;
  dateOfBirth?: string;
  nationality?: string;
  currentAddress?: string;
  city?: string;
  state?: string;
  country?: string;
  zipCode?: string;
  professionalStatus?: string;
  currentCompany?: string;
  currentDesignation?: string;
  totalExperienceMonths?: number;
  currentCtc?: number;
  expectedCtc?: number;
  linkedInUrl?: string;
  gitHubUrl?: string;
  portfolioUrl?: string;
  resumePath?: string;
  resumeFileName?: string;
}

export default function CareerProfilePage() {
  const navigate = useNavigate();
  const { message: msg, modal } = AntApp.useApp();
  const { isAuthenticated, loadSession } = useCareerAuthStore();
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [profile, setProfile] = useState<ProfileData | null>(null);
  const [uploading, setUploading] = useState(false);
  const [parsing, setParsing] = useState(false);
  const [testing, setTesting] = useState(false);

  useEffect(() => {
    loadSession();
    if (!isAuthenticated) {
      navigate('/careers', { replace: true });
      return;
    }
    fetchProfile();
  }, [isAuthenticated]);

  const fetchProfile = async () => {
    try {
      setLoading(true);
      const res = await careerApi.get('/profile');
      setProfile(res.data);
    } catch {
      msg.error('Failed to load profile');
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async (values: Record<string, any>) => {
    setSaving(true);
    try {
      const payload = {
        phone: values.phone || null,
        gender: values.gender || null,
        dateOfBirth: values.dateOfBirth?.toISOString() || null,
        nationality: values.nationality || null,
        currentAddress: values.currentAddress || null,
        city: values.city || null,
        state: values.state || null,
        country: values.country || null,
        zipCode: values.zipCode || null,
        professionalStatus: values.professionalStatus || null,
        currentCompany: values.currentCompany || null,
        currentDesignation: values.currentDesignation || null,
        totalExperienceMonths: values.totalExperienceMonths ? Number(values.totalExperienceMonths) : null,
        currentCtc: values.currentCtc ? Number(values.currentCtc) : null,
        expectedCtc: values.expectedCtc ? Number(values.expectedCtc) : null,
        linkedInUrl: values.linkedInUrl || null,
        gitHubUrl: values.gitHubUrl || null,
        portfolioUrl: values.portfolioUrl || null,
      };
      const res = await careerApi.put('/profile', payload);
      setProfile(res.data);
      msg.success('Profile saved');
    } catch {
      msg.error('Failed to save profile');
    } finally {
      setSaving(false);
    }
  };

  const handleUpload = async (file: File): Promise<false> => {
    setUploading(true);
    try {
      const formData = new FormData();
      formData.append('file', file);
      const res = await careerApi.post('/profile/upload-resume', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      setProfile(res.data);
      msg.success('Resume uploaded');
    } catch {
      msg.error('Failed to upload resume');
    } finally {
      setUploading(false);
    }
    return false;
  };

  const handleParseResume = async () => {
    setParsing(true);
    try {
      const res = await careerApi.post('/profile/parse-resume');
      const data = res.data;
      form.setFieldsValue({
        phone: data.phone || undefined,
        gender: data.gender || undefined,
        nationality: data.nationality || undefined,
        currentAddress: data.currentAddress || undefined,
        city: data.city || undefined,
        state: data.state || undefined,
        country: data.country || undefined,
        zipCode: data.zipCode || undefined,
        professionalStatus: data.professionalStatus || undefined,
        currentCompany: data.currentCompany || undefined,
        currentDesignation: data.currentDesignation || undefined,
        totalExperienceMonths: data.totalExperienceMonths?.toString() || undefined,
        currentCtc: data.currentCtc?.toString() || undefined,
        expectedCtc: data.expectedCtc?.toString() || undefined,
        linkedInUrl: data.linkedInUrl || undefined,
        gitHubUrl: data.gitHubUrl || undefined,
        portfolioUrl: data.portfolioUrl || undefined,
      });
      msg.success('Resume parsed successfully — fields populated');
    } catch {
      msg.error('Failed to parse resume. Check your Groq API key.');
    } finally {
      setParsing(false);
    }
  };

  const handleTestGroq = async () => {
    setTesting(true);
    try {
      const res = await careerApi.post('/profile/test-groq');
      const data = res.data;
      modal.info({
        title: 'Groq API Test Result',
        width: 640,
        content: (
          <div>
            <p><strong>Message:</strong> {data.message}</p>
            {data.statusCode !== undefined && <p><strong>HTTP Status:</strong> {data.statusCode}</p>}
            <p><strong>API Key Prefix:</strong> {data.apiKeyPrefix}</p>
            {data.hasResumeText !== undefined && (
              <p><strong>Has Resume Text:</strong> {data.hasResumeText ? 'Yes' : 'No'} ({data.resumeTextLength} chars)</p>
            )}
            {data.responseBody && (
              <>
                <p><strong>Groq Response:</strong></p>
                <pre style={{ fontSize: 11, maxHeight: 300, overflow: 'auto', background: '#f5f5f5', padding: 8, borderRadius: 4, whiteSpace: 'pre-wrap' }}>
                  {data.responseBody}
                </pre>
              </>
            )}
          </div>
        ),
      });
    } catch (err: any) {
      const detail = err?.response?.data?.message || err.message || 'Unknown error';
      modal.error({
        title: 'Groq API Test Failed',
        content: detail,
      });
    } finally {
      setTesting(false);
    }
  };

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '60vh' }}>
        <Spin size="large" />
      </div>
    );
  }

  const initialValues = profile ? {
    phone: profile.phone,
    gender: profile.gender,
    dateOfBirth: profile.dateOfBirth ? dayjs(profile.dateOfBirth) : undefined,
    nationality: profile.nationality,
    currentAddress: profile.currentAddress,
    city: profile.city,
    state: profile.state,
    country: profile.country,
    zipCode: profile.zipCode,
    professionalStatus: profile.professionalStatus,
    currentCompany: profile.currentCompany,
    currentDesignation: profile.currentDesignation,
    totalExperienceMonths: profile.totalExperienceMonths?.toString(),
    currentCtc: profile.currentCtc?.toString(),
    expectedCtc: profile.expectedCtc?.toString(),
    linkedInUrl: profile.linkedInUrl,
    gitHubUrl: profile.gitHubUrl,
    portfolioUrl: profile.portfolioUrl,
  } : {};

  return (
    <Layout style={{ minHeight: '100vh', background: '#f0f2f5' }}>
      <Content style={{ maxWidth: 800, margin: '0 auto', padding: '24px 16px', width: '100%' }}>
        <Space style={{ marginBottom: 24 }}>
          <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/careers')}>Back to Jobs</Button>
        </Space>

        <Title level={3} style={{ margin: 0 }}>
          <UserOutlined style={{ marginRight: 8 }} />My Profile
        </Title>
        <Text type="secondary" style={{ display: 'block', marginBottom: 8 }}>
          Manage your personal info, professional details, and resume
        </Text>

        {!profile?.resumeFileName && (
          <Alert
            type="info"
            showIcon
            icon={<UploadOutlined />}
            title="Upload your resume to auto-fill your profile with AI"
            description="Upload a PDF resume above, then click 'Parse Resume with AI' to extract your details and populate the fields below automatically."
            style={{ marginBottom: 16, borderRadius: 8 }}
          />
        )}

        <Form form={form} layout="vertical" onFinish={handleSave} initialValues={initialValues}>
          {/* Resume — top so user uploads first */}
          <Card
            title={<><UploadOutlined /> Resume</>}
            style={{ marginBottom: 16, borderColor: !profile?.resumeFileName ? '#6c5ce7' : undefined }}
          >
            <Upload
              accept=".pdf"
              maxCount={1}
              beforeUpload={handleUpload as (file: File) => false}
              fileList={profile?.resumeFileName ? [{ uid: '-1', name: profile.resumeFileName, status: 'done' as const }] : []}
              onRemove={() => {}}
            >
              <Button icon={<UploadOutlined />} loading={uploading}>
                {profile?.resumeFileName ? 'Replace Resume (PDF)' : 'Upload Resume (PDF)'}
              </Button>
            </Upload>
            {profile?.resumeFileName && (
              <>
                <Text type="secondary" style={{ display: 'block', marginTop: 8 }}>
                  Current: {profile.resumeFileName}
                </Text>
                <Button
                  type="primary"
                  ghost
                  icon={<ThunderboltOutlined />}
                  loading={parsing}
                  onClick={handleParseResume}
                  style={{ marginTop: 12 }}
                >
                  Parse Resume with AI
                </Button>
                <Button
                  danger
                  size="small"
                  loading={testing}
                  onClick={handleTestGroq}
                  style={{ marginTop: 8 }}
                >
                  Test AI Connection
                </Button>
              </>
            )}
          </Card>

          {/* Personal Information */}
          <Card title={<><UserOutlined /> Personal Information</>} style={{ marginBottom: 16 }}>
            <Row gutter={16}>
              <Col xs={24} sm={8}>
                <Form.Item label="First Name">
                  <Input value={profile?.firstName} disabled />
                </Form.Item>
              </Col>
              <Col xs={24} sm={8}>
                <Form.Item label="Last Name">
                  <Input value={profile?.lastName} disabled />
                </Form.Item>
              </Col>
              <Col xs={24} sm={8}>
                <Form.Item label="Email">
                  <Input value={profile?.email} disabled />
                </Form.Item>
              </Col>
            </Row>
            <Row gutter={16}>
              <Col xs={24} sm={8}>
                <Form.Item name="phone" label="Phone">
                  <Input placeholder="+91 98765 43210" />
                </Form.Item>
              </Col>
              <Col xs={24} sm={8}>
                <Form.Item name="gender" label="Gender">
                  <Select allowClear placeholder="Select">
                    <Select.Option value="Male">Male</Select.Option>
                    <Select.Option value="Female">Female</Select.Option>
                    <Select.Option value="Other">Other</Select.Option>
                    <Select.Option value="PreferNotToSay">Prefer not to say</Select.Option>
                  </Select>
                </Form.Item>
              </Col>
              <Col xs={24} sm={8}>
                <Form.Item name="nationality" label="Nationality">
                  <Input placeholder="e.g. Indian" />
                </Form.Item>
              </Col>
            </Row>
          </Card>

          {/* Address */}
          <Card title={<><EnvironmentOutlined /> Address</>} style={{ marginBottom: 16 }}>
            <Row gutter={16}>
              <Col xs={24}>
                <Form.Item name="currentAddress" label="Current Address">
                  <Input.TextArea rows={2} placeholder="Street, area, landmark" />
                </Form.Item>
              </Col>
            </Row>
            <Row gutter={16}>
              <Col xs={24} sm={8}>
                <Form.Item name="city" label="City">
                  <Input placeholder="e.g. Pune" />
                </Form.Item>
              </Col>
              <Col xs={24} sm={8}>
                <Form.Item name="state" label="State">
                  <Input placeholder="e.g. Maharashtra" />
                </Form.Item>
              </Col>
              <Col xs={24} sm={4}>
                <Form.Item name="zipCode" label="Zip Code">
                  <Input placeholder="411001" />
                </Form.Item>
              </Col>
              <Col xs={24} sm={4}>
                <Form.Item name="country" label="Country">
                  <Input placeholder="India" />
                </Form.Item>
              </Col>
            </Row>
          </Card>

          {/* Professional Details */}
          <Card title={<><ToolOutlined /> Professional Details</>} style={{ marginBottom: 16 }}>
            <Row gutter={16}>
              <Col xs={24} sm={12}>
                <Form.Item name="professionalStatus" label="Professional Status">
                  <Select allowClear placeholder="Select" options={PROFESSIONAL_STATUS_OPTIONS} />
                </Form.Item>
              </Col>
              <Col xs={24} sm={12}>
                <Form.Item name="currentCompany" label="Current Company">
                  <Input placeholder="Company name" />
                </Form.Item>
              </Col>
            </Row>
            <Row gutter={16}>
              <Col xs={24} sm={12}>
                <Form.Item name="currentDesignation" label="Designation">
                  <Input placeholder="e.g. Software Engineer" />
                </Form.Item>
              </Col>
              <Col xs={24} sm={12}>
                <Form.Item name="totalExperienceMonths" label="Total Experience (months)">
                  <Input type="number" placeholder="e.g. 36" />
                </Form.Item>
              </Col>
            </Row>
            <Row gutter={16}>
              <Col xs={24} sm={12}>
                <Form.Item name="currentCtc" label="Current CTC (LPA)">
                  <Input type="number" placeholder="e.g. 12" />
                </Form.Item>
              </Col>
              <Col xs={24} sm={12}>
                <Form.Item name="expectedCtc" label="Expected CTC (LPA)">
                  <Input type="number" placeholder="e.g. 18" />
                </Form.Item>
              </Col>
            </Row>
          </Card>

          {/* Social Links */}
          <Card title={<><LinkOutlined /> Social Links</>} style={{ marginBottom: 16 }}>
            <Row gutter={16}>
              <Col xs={24}>
                <Form.Item name="linkedInUrl" label="LinkedIn URL">
                  <Input placeholder="https://linkedin.com/in/username" />
                </Form.Item>
              </Col>
            </Row>
            <Row gutter={16}>
              <Col xs={24} sm={12}>
                <Form.Item name="gitHubUrl" label="GitHub URL">
                  <Input placeholder="https://github.com/username" />
                </Form.Item>
              </Col>
              <Col xs={24} sm={12}>
                <Form.Item name="portfolioUrl" label="Portfolio URL">
                  <Input placeholder="https://your-portfolio.com" />
                </Form.Item>
              </Col>
            </Row>
          </Card>

          <div style={{ textAlign: 'right' }}>
            <Button type="primary" htmlType="submit" loading={saving} icon={<SaveOutlined />} size="large">
              Save Profile
            </Button>
          </div>
        </Form>
      </Content>
    </Layout>
  );
}
