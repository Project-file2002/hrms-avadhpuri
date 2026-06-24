import { useState, useEffect } from 'react';
import {
  Card, Row, Col, Statistic, Table, Tag, Typography, Spin, Space,
  Progress, Tabs, Alert, Empty, Tooltip
} from 'antd';
import {
  WarningOutlined, FireOutlined, LineChartOutlined,
  UserOutlined, TeamOutlined, RiseOutlined, FallOutlined,
  BulbOutlined, AlertOutlined, ClockCircleOutlined
} from '@ant-design/icons';
import api from '../services/api';

const { Title, Text } = Typography;

interface AttritionRisk {
  employeeId: number;
  employeeName: string;
  department: string;
  position: string;
  riskScore: number;
  riskLevel: string;
  riskFactors: string[];
  suggestedAction: string;
}

interface PositionForecast {
  title: string;
  department: string;
  daysOpen: number;
  candidateCount: number;
  urgency: string;
}

interface HiringForecast {
  openPositions: number;
  totalCandidates: number;
  avgDaysOpen: number;
  positions: PositionForecast[];
}

interface BurnoutRisk {
  employeeId: number;
  employeeName: string;
  department: string;
  burnoutScore: number;
  riskLevel: string;
  indicators: string[];
  suggestedAction: string;
}

interface Summary {
  totalEmployees: number;
  atRiskEmployees: number;
  highBurnoutEmployees: number;
  openPositions: number;
  avgRiskScore: number;
  avgBurnoutScore: number;
}

const riskColor: Record<string, string> = {
  Low: 'green',
  Medium: 'orange',
  High: 'red',
  Critical: '#cf1322',
};

function PredictiveAnalyticsPage() {
  const [loading, setLoading] = useState(true);
  const [attrition, setAttrition] = useState<AttritionRisk[]>([]);
  const [hiring, setHiring] = useState<HiringForecast | null>(null);
  const [burnout, setBurnout] = useState<BurnoutRisk[]>([]);
  const [summary, setSummary] = useState<Summary | null>(null);

  useEffect(() => {
    fetchAll();
  }, []);

  const fetchAll = async () => {
    try {
      const res = await api.get('/predictive/dashboard');
      setAttrition(res.data.attritionRisks);
      setHiring(res.data.hiringForecast);
      setBurnout(res.data.burnoutRisks);
      setSummary(res.data.summary);
    } catch {
      // handled by api interceptor
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <Spin size="large" style={{ display: 'block', margin: 80 }} />;

  const urgencyColor: Record<string, string> = {
    Normal: 'green', Moderate: 'blue', High: 'orange', Critical: 'red',
  };

  const attritionColumns = [
    {
      title: 'Employee', key: 'name', width: 180,
      render: (_: unknown, r: AttritionRisk) => (
        <Text strong>{r.employeeName}</Text>
      ),
    },
    { title: 'Department', dataIndex: 'department', key: 'dept', width: 140 },
    { title: 'Position', dataIndex: 'position', key: 'pos', width: 160 },
    {
      title: 'Risk Score', dataIndex: 'riskScore', key: 'score', width: 120,
      render: (s: number, r: AttritionRisk) => (
        <Tooltip title={r.riskFactors.join(', ')}>
          <Progress
            percent={Math.round(s)}
            size="small"
            strokeColor={s < 20 ? '#52c41a' : s < 40 ? '#faad14' : s < 65 ? '#ff4d4f' : '#cf1322'}
            format={() => `${Math.round(s)}%`}
            style={{ width: 100 }}
          />
        </Tooltip>
      ),
    },
    {
      title: 'Level', dataIndex: 'riskLevel', key: 'level', width: 100,
      render: (l: string) => <Tag color={riskColor[l]}>{l}</Tag>,
    },
    {
      title: 'Factors', key: 'factors', width: 200,
      render: (_: unknown, r: AttritionRisk) => (
        <Space size={4} wrap>
          {r.riskFactors.map((f, i) => (
            <Tag key={i} style={{ fontSize: 10, margin: 0 }}>{f}</Tag>
          ))}
          {r.riskFactors.length === 0 && <Text type="secondary" style={{ fontSize: 12 }}>None</Text>}
        </Space>
      ),
    },
    {
      title: 'Suggested Action', dataIndex: 'suggestedAction', key: 'action',
      render: (a: string) => (
        <Text style={{ fontSize: 12, fontStyle: 'italic', color: '#8c8c8c' }}>{a}</Text>
      ),
    },
  ];

  const burnoutColumns = [
    {
      title: 'Employee', key: 'name', width: 180,
      render: (_: unknown, r: BurnoutRisk) => <Text strong>{r.employeeName}</Text>,
    },
    { title: 'Department', dataIndex: 'department', key: 'dept', width: 140 },
    {
      title: 'Burnout Score', dataIndex: 'burnoutScore', key: 'score', width: 120,
      render: (s: number) => (
        <Progress
          percent={Math.round(s)}
          size="small"
          strokeColor={s < 20 ? '#52c41a' : s < 40 ? '#faad14' : s < 65 ? '#ff4d4f' : '#cf1322'}
          format={() => `${Math.round(s)}%`}
          style={{ width: 100 }}
        />
      ),
    },
    {
      title: 'Level', dataIndex: 'riskLevel', key: 'level', width: 100,
      render: (l: string) => <Tag color={riskColor[l]}>{l}</Tag>,
    },
    {
      title: 'Indicators', key: 'indicators',
      render: (_: unknown, r: BurnoutRisk) => (
        <Space size={4} wrap>
          {r.indicators.map((ind, i) => (
            <Tag key={i} color="volcano" style={{ fontSize: 10, margin: 0 }}>{ind}</Tag>
          ))}
          {r.indicators.length === 0 && <Text type="secondary" style={{ fontSize: 12 }}>None</Text>}
        </Space>
      ),
    },
    {
      title: 'Suggested Action', dataIndex: 'suggestedAction', key: 'action',
      render: (a: string) => (
        <Text style={{ fontSize: 12, fontStyle: 'italic', color: '#8c8c8c' }}>{a}</Text>
      ),
    },
  ];

  const hiringColumns = [
    { title: 'Position', dataIndex: 'title', key: 'title' },
    { title: 'Department', dataIndex: 'department', key: 'dept' },
    {
      title: 'Days Open', dataIndex: 'daysOpen', key: 'days', width: 100,
      render: (d: number) => <Text strong>{d}d</Text>,
    },
    {
      title: 'Candidates', dataIndex: 'candidateCount', key: 'candidates', width: 100,
    },
    {
      title: 'Urgency', dataIndex: 'urgency', key: 'urgency', width: 100,
      render: (u: string) => <Tag color={urgencyColor[u]}>{u}</Tag>,
    },
  ];

  const tabItems = [
    {
      key: 'overview',
      label: <span><LineChartOutlined /> Overview</span>,
      children: (
        <>
          <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
            <Col xs={12} sm={8} md={4}>
              <Card size="small" style={{ borderRadius: 8, textAlign: 'center' }}>
                <Statistic
                  title={<Space><TeamOutlined /> Total Employees</Space>}
                  value={summary?.totalEmployees ?? 0}
                  styles={{ content: {  color: '#6c5ce7', fontSize: 28  } }}
                />
              </Card>
            </Col>
            <Col xs={12} sm={8} md={4}>
              <Card size="small" style={{ borderRadius: 8, textAlign: 'center' }}>
                <Statistic
                  title={<Space><WarningOutlined style={{ color: '#ff4d4f' }} /> At Risk</Space>}
                  value={summary?.atRiskEmployees ?? 0}
                  styles={{ content: {  color: '#ff4d4f', fontSize: 28  } }}
                  suffix={`/ ${summary?.totalEmployees ?? 0}`}
                />
              </Card>
            </Col>
            <Col xs={12} sm={8} md={4}>
              <Card size="small" style={{ borderRadius: 8, textAlign: 'center' }}>
                <Statistic
                  title={<Space><FireOutlined style={{ color: '#cf1322' }} /> Burnout Risk</Space>}
                  value={summary?.highBurnoutEmployees ?? 0}
                  styles={{ content: {  color: '#cf1322', fontSize: 28  } }}
                />
              </Card>
            </Col>
            <Col xs={12} sm={8} md={4}>
              <Card size="small" style={{ borderRadius: 8, textAlign: 'center' }}>
                <Statistic
                  title={<Space><RiseOutlined /> Open Positions</Space>}
                  value={summary?.openPositions ?? 0}
                  styles={{ content: {  color: '#1890ff', fontSize: 28  } }}
                />
              </Card>
            </Col>
            <Col xs={12} sm={8} md={4}>
              <Card size="small" style={{ borderRadius: 8, textAlign: 'center' }}>
                <Statistic
                  title={<Space><FallOutlined /> Avg Risk</Space>}
                  value={summary?.avgRiskScore ?? 0}
                  precision={1}
                  suffix="%"
                  styles={{ content: {  color: '#faad14', fontSize: 28  } }}
                />
              </Card>
            </Col>
            <Col xs={12} sm={8} md={4}>
              <Card size="small" style={{ borderRadius: 8, textAlign: 'center' }}>
                <Statistic
                  title={<Space><AlertOutlined /> Avg Burnout</Space>}
                  value={summary?.avgBurnoutScore ?? 0}
                  precision={1}
                  suffix="%"
                  styles={{ content: {  color: '#ff4d4f', fontSize: 28  } }}
                />
              </Card>
            </Col>
          </Row>

          <Row gutter={[16, 16]}>
            <Col xs={24} lg={12}>
              <Card
                title={<Space><WarningOutlined style={{ color: '#ff4d4f' }} /> Hiring Forecast</Space>}
                size="small"
                style={{ borderRadius: 8 }}
              >
                  {hiring && hiring.positions.length > 0 ? (
                  <Table
                    dataSource={hiring.positions}
                    columns={hiringColumns}
                    rowKey="title"
                    pagination={false}
                    size="small"
                    scroll={{ x: 'max-content' }}
                  />
                ) : (
                  <Empty description="No open positions" />
                )}
              </Card>
            </Col>
            <Col xs={24} lg={12}>
              <Card
                title={<Space><BulbOutlined /> Insights</Space>}
                size="small"
                style={{ borderRadius: 8 }}
              >
                {summary && (
                  <Space orientation="vertical" style={{ width: '100%' }}>
                    {summary.atRiskEmployees > 0 && (
                      <Alert
                        type="warning"
                        showIcon
                        title={`${summary.atRiskEmployees} employee(s) at high attrition risk — consider retention programs.`}
                        style={{ borderRadius: 6 }}
                      />
                    )}
                    {summary.highBurnoutEmployees > 0 && (
                      <Alert
                        type="error"
                        showIcon
                        title={`${summary.highBurnoutEmployees} employee(s) showing burnout signs — review workload distribution.`}
                        style={{ borderRadius: 6 }}
                      />
                    )}
                    {summary.openPositions > 0 && (
                      <Alert
                        type="info"
                        showIcon
                        icon={<RiseOutlined />}
                        title={`${summary.openPositions} positions open (avg ${hiring?.avgDaysOpen.toFixed(0) ?? 0} days) — review recruitment pipeline.`}
                        style={{ borderRadius: 6 }}
                      />
                    )}
                    {summary.atRiskEmployees === 0 && summary.highBurnoutEmployees === 0 && (
                      <Alert
                        type="success"
                        showIcon
                        title="All indicators look healthy. No major risks detected."
                        style={{ borderRadius: 6 }}
                      />
                    )}
                    <Card size="small" style={{ borderRadius: 6, background: '#f5f0ff' }}>
                      <Text strong style={{ color: '#6c5ce7' }}>
                        <ClockCircleOutlined /> Next recommended actions:
                      </Text>
                      <div style={{ marginTop: 8 }}>
                        {summary.atRiskEmployees > 0 && (
                          <div style={{ marginBottom: 4 }}>• Schedule retention 1:1s with at-risk employees</div>
                        )}
                        {summary.highBurnoutEmployees > 0 && (
                          <div style={{ marginBottom: 4 }}>• Review workload for burnout-risk employees</div>
                        )}
                        {summary.openPositions > 0 && (
                          <div style={{ marginBottom: 4 }}>• Accelerate hiring for critical/aged positions</div>
                        )}
                        <div>• Review department-level metrics monthly</div>
                      </div>
                    </Card>
                  </Space>
                )}
              </Card>
            </Col>
          </Row>
        </>
      ),
    },
    {
      key: 'attrition',
      label: <span><WarningOutlined /> Attrition Risk</span>,
      children: attrition.length > 0 ? (
        <Table
          dataSource={attrition}
          columns={attritionColumns}
          rowKey="employeeId"
          pagination={false}
          size="small"
          scroll={{ x: 'max-content' }}
        />
      ) : <Empty description="No attrition risk data" />,
    },
    {
      key: 'burnout',
      label: <span><FireOutlined /> Burnout Detection</span>,
      children: burnout.length > 0 ? (
        <Table
          dataSource={burnout}
          columns={burnoutColumns}
          rowKey="employeeId"
          pagination={false}
          size="small"
          scroll={{ x: 'max-content' }}
        />
      ) : <Empty description="No burnout risk data" />,
    },
    {
      key: 'positions',
      label: <span><RiseOutlined /> Hiring Forecast</span>,
      children: hiring && hiring.positions.length > 0 ? (
        <>
          <Row gutter={16} style={{ marginBottom: 24 }}>
            <Col xs={24} sm={8}>
              <Card size="small" style={{ borderRadius: 8, textAlign: 'center' }}>
                <Statistic title="Open Positions" value={hiring.openPositions} styles={{ content: {  color: '#1890ff'  } }} />
              </Card>
            </Col>
            <Col xs={24} sm={8}>
              <Card size="small" style={{ borderRadius: 8, textAlign: 'center' }}>
                <Statistic title="Total Candidates" value={hiring.totalCandidates} styles={{ content: {  color: '#52c41a'  } }} />
              </Card>
            </Col>
            <Col xs={24} sm={8}>
              <Card size="small" style={{ borderRadius: 8, textAlign: 'center' }}>
                <Statistic
                  title="Avg Days Open"
                  value={hiring.avgDaysOpen.toFixed(0)}
                  suffix="days"
                  styles={{ content: {  color: hiring.avgDaysOpen > 30 ? '#ff4d4f' : '#faad14'  } }}
                />
              </Card>
            </Col>
          </Row>
          <Table
            dataSource={hiring.positions}
            columns={hiringColumns}
            rowKey="title"
            pagination={false}
            size="small"
            scroll={{ x: 'max-content' }}
          />
        </>
      ) : <Empty description="No open positions" />,
    },
  ];

  return (
    <div>
      <div className="page-intro">
        <Title level={4} style={{ margin: 0 }}>
          <LineChartOutlined style={{ marginRight: 8, color: '#6c5ce7' }} />
          Predictive HR Analytics
        </Title>
        <Text type="secondary">
          AI-driven workforce insights — attrition risk, burnout detection, and hiring forecasts
        </Text>
      </div>

      <Card
        style={{ borderRadius: 12, overflow: 'hidden' }}
        styles={{ body: { padding: 0 } }}
      >
        <Tabs
          defaultActiveKey="overview"
          items={tabItems}
          size="large"
          tabBarStyle={{ paddingLeft: 24, marginBottom: 0 }}
        />
      </Card>
    </div>
  );
}

export default PredictiveAnalyticsPage;
