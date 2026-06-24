import { useEffect, useState } from 'react';
import { Card, Table, Tag, Typography, Row, Col, Statistic, Button, Space } from 'antd';
import { AuditOutlined, ArrowRightOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';

const { Title, Text } = Typography;

interface ApprovalItem {
  id: number;
  type: string;
  title: string;
  subtitle: string;
  status: string;
  createdAt: string;
  link: string;
}

interface ApprovalSummary {
  leave: number;
  expense: number;
  promotion: number;
  transfer: number;
  recruitment: number;
}

const typeColors: Record<string, string> = {
  Leave: 'blue', Expense: 'green', Promotion: 'purple', Transfer: 'cyan', Recruitment: 'orange',
};

export default function ApprovalsPage() {
  const [items, setItems] = useState<ApprovalItem[]>([]);
  const [summary, setSummary] = useState<ApprovalSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    api.get('/approvals/pending').then(res => {
      setItems(res.data.items);
      setSummary(res.data.summary);
    }).finally(() => setLoading(false));
  }, []);

  const columns = [
    {
      title: 'Type', dataIndex: 'type', key: 'type',
      render: (t: string) => <Tag color={typeColors[t]}>{t}</Tag>,
    },
    { title: 'Request', dataIndex: 'title', key: 'title' },
    { title: 'Details', dataIndex: 'subtitle', key: 'subtitle', ellipsis: true },
    {
      title: 'Status', dataIndex: 'status', key: 'status',
      render: (s: string) => <Tag>{s}</Tag>,
    },
    {
      title: 'Submitted', dataIndex: 'createdAt', key: 'createdAt',
      render: (v: string) => new Date(v).toLocaleDateString(),
    },
    {
      title: 'Action', key: 'action', width: 100,
      render: (_: unknown, r: ApprovalItem) => (
        <Button type="link" icon={<ArrowRightOutlined />} onClick={() => navigate(r.link)}>
          Review
        </Button>
      ),
    },
  ];

  return (
    <div>
      <div className="page-header">
        <div>
          <Title level={4} style={{ margin: 0 }}><AuditOutlined /> Approval Center</Title>
          <Text type="secondary">One screen for all pending approvals — leave, expense, recruitment, promotions</Text>
        </div>
      </div>

      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        {[
          { label: 'Leave', key: 'leave', color: '#74b9ff' },
          { label: 'Expense', key: 'expense', color: '#00b894' },
          { label: 'Promotion', key: 'promotion', color: '#6c5ce7' },
          { label: 'Transfer', key: 'transfer', color: '#0984e3' },
          { label: 'Recruitment', key: 'recruitment', color: '#fdcb6e' },
        ].map(s => (
          <Col xs={12} sm={8} md={4} key={s.key}>
            <Card className="stat-card">
              <Statistic
                title={s.label}
                value={summary ? (summary as Record<string, number>)[s.key] : 0}
                valueStyle={{ color: s.color }}
              />
            </Card>
          </Col>
        ))}
      </Row>

      <Card className="content-card" title={<Space>Pending Approvals <Tag>{items.length}</Tag></Space>}>
        <div className="responsive-table-wrap">
          <Table rowKey={(r) => `${r.type}-${r.id}`} loading={loading} dataSource={items}
            columns={columns} pagination={{ pageSize: 10 }} scroll={{ x: 700 }} />
        </div>
      </Card>
    </div>
  );
}
