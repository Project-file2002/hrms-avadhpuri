import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Layout, Card, Row, Col, Typography, Spin, Button, Tag, Steps, Statistic,
  Space, Divider, Empty, Progress, Table, Result, Timeline
} from 'antd';
import {
  ArrowLeftOutlined, CheckCircleOutlined, CloseCircleOutlined,
  ClockCircleOutlined, UserOutlined, FileTextOutlined,
  ThunderboltOutlined, StarOutlined, RiseOutlined, AimOutlined
} from '@ant-design/icons';
import careerApi from '../../services/careerApi';
import { useCareerAuthStore } from '../../store/careerAuthStore';

const { Title, Text, Paragraph } = Typography;

interface PipelineStage {
  title: string;
  description: string;
  completed: boolean;
  current: boolean;
}

interface ApplicationSummary {
  applicationId: number;
  jobTitle: string;
  department: string;
  status: string;
  statusLabel: string;
  matchScore: number | null;
  appliedAt: string;
  updatedAt: string | null;
  nextStepHint: string | null;
  pipeline: PipelineStage[];
}

interface DashboardData {
  applicationCount: number;
  applications: ApplicationSummary[];
  resumeScore: {
    resumeScore: number;
    atsScore: number;
    missingSkills: string[];
    suggestions: string[];
    aiAnalysis: string | null;
  } | null;
  careerAdvice: {
    recommendedRoles: { role: string; matchPercent: number; reason: string | null }[];
    skillGaps: { skill: string; currentLevel: number; requiredLevel: number; gap: number }[];
    learningRoadmap: { currentSkills: string; targetJob: string; missingSkills: string[]; steps: string[]; resources: string[] } | null;
    interviewReadiness: { technical: number; behavioral: number; communication: number } | null;
  } | null;
}

const STATUS_COLORS: Record<string, string> = {
  'Draft': 'default',
  'New': 'blue',
  'Screening': 'processing',
  'Interviewed': 'orange',
  'Offered': 'green',
  'Hired': 'success',
  'Rejected': 'error',
  'Withdrawn': 'default',
};

export default function CareerDashboardPage() {
  const navigate = useNavigate();
  const { loadSession } = useCareerAuthStore();
  const [loading, setLoading] = useState(true);
  const [data, setData] = useState<DashboardData | null>(null);

  useEffect(() => {
    loadSession();
    const fetchDashboard = async () => {
      try {
        setLoading(true);
        const res = await careerApi.get('/wizard/dashboard');
        setData(res.data);
      } catch {
        // Not authenticated or no data
      } finally {
        setLoading(false);
      }
    };
    fetchDashboard();
  }, []);

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '60vh' }}>
        <Spin size="large" description="Loading dashboard..." />
      </div>
    );
  }

  if (!data) {
    return (
      <Layout className="career-dashboard-layout" style={{ minHeight: '100vh', background: '#f0f2f5' }}>
        <Layout.Content style={{ maxWidth: 600, margin: '80px auto', padding: 24 }}>
          <Result
            status="info"
            title="No applications yet"
            subTitle="Start by applying to a job that matches your skills."
            extra={
              <Button type="primary" onClick={() => navigate('/careers')}>
                Browse Jobs
              </Button>
            }
          />
        </Layout.Content>
      </Layout>
    );
  }

  const appColumns = [
    {
      title: 'Job Title',
      dataIndex: 'jobTitle',
      key: 'jobTitle',
      render: (title: string, record: ApplicationSummary) => (
        <Space>
          <FileTextOutlined />
          <Text strong>{title}</Text>
          <Tag color="default">{record.department}</Tag>
        </Space>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'statusLabel',
      key: 'status',
      render: (label: string, record: ApplicationSummary) => (
        <Tag color={STATUS_COLORS[record.status] || 'default'}>{label}</Tag>
      ),
    },
    {
      title: 'Match Score',
      dataIndex: 'matchScore',
      key: 'matchScore',
      render: (score: number | null) => {
        if (score == null) return <Text type="secondary">--</Text>;
        const color = score >= 70 ? 'green' : score >= 40 ? 'orange' : 'red';
        return <Tag color={color}>{score}%</Tag>;
      },
    },
    {
      title: 'Applied',
      dataIndex: 'appliedAt',
      key: 'appliedAt',
      render: (date: string) => new Date(date).toLocaleDateString(),
    },
    {
      title: 'Next Step',
      dataIndex: 'nextStepHint',
      key: 'nextStepHint',
      render: (hint: string | null) => hint || <Text type="secondary">--</Text>,
    },
    {
      title: '',
      key: 'actions',
      render: (_: any, record: ApplicationSummary) => (
        <Button size="small" type="link" onClick={() => navigate(`/careers`)}>
          View
        </Button>
      ),
    },
  ];

  return (
    <Layout className="career-dashboard-layout" style={{ minHeight: '100vh', background: '#f0f2f5' }}>
      <Layout.Header style={{
        background: '#fff',
        display: 'flex',
        alignItems: 'center',
        padding: '0 24px',
        borderBottom: '1px solid #f0f0f0',
        position: 'sticky',
        top: 0,
        zIndex: 10,
      }}>
        <Space>
          <Text strong style={{ fontSize: 18, color: '#6c5ce7' }}>EWXP</Text>
          <Text type="secondary">|</Text>
          <Text>My Dashboard</Text>
        </Space>
        <div style={{ flex: 1 }} />
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/careers')}>
          Back to Jobs
        </Button>
      </Layout.Header>

      <Layout.Content style={{ maxWidth: 1200, margin: '24px auto', padding: '0 24px', width: '100%' }}>
        {/* Stats Cards */}
        <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
          <Col xs={24} sm={8}>
            <Card hoverable>
              <Statistic
                title="Total Applications"
                value={data.applicationCount}
                prefix={<FileTextOutlined style={{ color: '#6c5ce7' }} />}
              />
            </Card>
          </Col>
          <Col xs={24} sm={8}>
            <Card hoverable>
              <Statistic
                title="Active"
                value={data.applications.filter(a => !['Rejected', 'Withdrawn', 'Hired'].includes(a.status)).length}
                prefix={<ClockCircleOutlined style={{ color: '#1890ff' }} />}
              />
            </Card>
          </Col>
          <Col xs={24} sm={8}>
            <Card hoverable>
              <Statistic
                title="Interviews"
                value={data.applications.filter(a => a.status === 'Interviewed').length}
                prefix={<UserOutlined style={{ color: '#fa8c16' }} />}
              />
            </Card>
          </Col>
        </Row>

        {/* Resume Score + Career Advice */}
        <Row gutter={16} style={{ marginBottom: 24 }}>
          {data.resumeScore && (
            <Col xs={24} md={12}>
              <Card title={<Space><StarOutlined /> AI Resume Score</Space>}>
                <Row gutter={16}>
                  <Col span={12} style={{ textAlign: 'center' }}>
                    <Progress
                      type="dashboard"
                      percent={data.resumeScore.resumeScore}
                      strokeColor={{
                        '0%': '#6c5ce7',
                        '100%': '#00b894',
                      }}
                      size={120}
                    />
                    <Text strong>Overall</Text>
                  </Col>
                  <Col span={12} style={{ textAlign: 'center' }}>
                    <Progress
                      type="dashboard"
                      percent={data.resumeScore.atsScore}
                      strokeColor="#fa8c16"
                      size={120}
                    />
                    <Text strong>ATS Score</Text>
                  </Col>
                </Row>
                {data.resumeScore.missingSkills.length > 0 && (
                  <>
                    <Divider />
                    <Text type="secondary">Missing Skills:</Text>
                    <div style={{ marginTop: 4 }}>
                      {data.resumeScore.missingSkills.map(s => (
                        <Tag key={s} color="warning">{s}</Tag>
                      ))}
                    </div>
                  </>
                )}
                {data.resumeScore.suggestions.length > 0 && (
                  <>
                    <Divider />
                    <Text type="secondary">Suggestions:</Text>
                    <ul style={{ margin: '4px 0 0', paddingLeft: 20 }}>
                      {data.resumeScore.suggestions.map((s, i) => (
                        <li key={i}><Text style={{ fontSize: 13 }}>{s}</Text></li>
                      ))}
                    </ul>
                  </>
                )}
                {data.resumeScore.aiAnalysis && (
                  <>
                    <Divider />
                    <Text type="secondary">AI Analysis:</Text>
                    <Paragraph style={{ margin: '4px 0 0', fontSize: 13, whiteSpace: 'pre-line' }}>
                      {data.resumeScore.aiAnalysis}
                    </Paragraph>
                  </>
                )}
              </Card>
            </Col>
          )}

          {data.careerAdvice && (
            <Col xs={24} md={12}>
              <Card title={<Space><RiseOutlined /> Career Advice</Space>}>
                {data.careerAdvice.recommendedRoles.length > 0 && (
                  <>
                    <Text strong>Recommended Roles</Text>
                    {data.careerAdvice.recommendedRoles.map((r, i) => (
                      <Card key={i} size="small" style={{ marginTop: 8 }}>
                        <Row align="middle" justify="space-between">
                          <Col>
                            <Text>{r.role}</Text>
                            {r.reason && <br />}
                            {r.reason && <Text type="secondary" style={{ fontSize: 12 }}>{r.reason}</Text>}
                          </Col>
                          <Col>
                            <Tag color={r.matchPercent >= 70 ? 'success' : 'warning'}>{r.matchPercent}%</Tag>
                          </Col>
                        </Row>
                      </Card>
                    ))}
                  </>
                )}

                {data.careerAdvice.skillGaps.length > 0 && (
                  <>
                    <Divider />
                    <Text strong>Skill Gaps</Text>
                    {data.careerAdvice.skillGaps.map((g, i) => (
                      <div key={i} style={{ marginTop: 8 }}>
                        <Space style={{ width: '100%', justifyContent: 'space-between' }}>
                          <Text>{g.skill}</Text>
                          <Tag color={g.gap <= 0 ? 'success' : 'error'}>
                            {g.currentLevel} → {g.requiredLevel}
                          </Tag>
                        </Space>
                      </div>
                    ))}
                  </>
                )}

                {data.careerAdvice.learningRoadmap && (
                  <>
                    <Divider />
                    <Text strong>Learning Roadmap</Text>
                    <Timeline
                      style={{ marginTop: 8 }}
                      items={data.careerAdvice.learningRoadmap.steps.map(s => ({
                        children: <Text style={{ fontSize: 13 }}>{s}</Text>,
                        color: 'blue',
                      }))}
                    />
                    {data.careerAdvice.learningRoadmap.resources.length > 0 && (
                      <>
                        <Divider />
                        <Text type="secondary">Resources:</Text>
                        <ul style={{ margin: '4px 0 0', paddingLeft: 20 }}>
                          {data.careerAdvice.learningRoadmap.resources.map((r, i) => (
                            <li key={i}><Text style={{ fontSize: 13 }}>{r}</Text></li>
                          ))}
                        </ul>
                      </>
                    )}
                  </>
                )}

                {data.careerAdvice.interviewReadiness && (
                  <>
                    <Divider />
                    <Text strong>Interview Readiness</Text>
                    <Row gutter={8} style={{ marginTop: 8 }}>
                      {Object.entries(data.careerAdvice.interviewReadiness).map(([key, val]) => (
                        <Col span={8} key={key} style={{ textAlign: 'center' }}>
                          <Progress type="circle" percent={val} size={60} strokeColor="#6c5ce7" />
                          <Text style={{ fontSize: 11, display: 'block' }}>{key}</Text>
                        </Col>
                      ))}
                    </Row>
                  </>
                )}
              </Card>
            </Col>
          )}
        </Row>

        {/* Pipeline for latest application */}
        {data.applications.length > 0 && (
          <Card title={<Space><AimOutlined /> Application Pipeline</Space>} style={{ marginBottom: 24 }}>
            <Steps
              orientation="horizontal"
              current={data.applications[0].pipeline.findIndex(s => s.current)}
              size="small"
              items={data.applications[0].pipeline.map(s => ({
                title: s.title,
                content: s.description,
                status: s.completed ? 'finish' : s.current ? 'process' : 'wait',
              }))}
            />
          </Card>
        )}

        {/* Applications Table */}
        <Card title={<Space><FileTextOutlined /> All Applications</Space>}>
          <Table
            dataSource={data.applications}
            columns={appColumns}
            rowKey="applicationId"
            pagination={{ pageSize: 10, size: 'small' }}
            locale={{ emptyText: <Empty description="No applications yet" /> }}
          />
        </Card>
      </Layout.Content>
    </Layout>
  );
}
