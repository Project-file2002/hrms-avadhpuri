import { useEffect, useState } from 'react';
import { Card, Row, Col, Table, Statistic, Button, Spin } from 'antd';
import { DownloadOutlined, TeamOutlined, ClockCircleOutlined } from '@ant-design/icons';
import api from '../services/api';

interface HeadcountReport {
  total: number;
  byDepartment: { department: string; count: number }[];
  byStatus: { status: string; count: number }[];
}

interface LeaveSummary {
  total: number;
  pending: number;
  approved: number;
  rejected: number;
  byType: { type: string; count: number }[];
}

interface AttendanceSummary {
  date: string;
  todayRecords: number;
  present: number;
  late: number;
}

export default function ReportsPage() {
  const [headcount, setHeadcount] = useState<HeadcountReport | null>(null);
  const [leaveSummary, setLeaveSummary] = useState<LeaveSummary | null>(null);
  const [attendance, setAttendance] = useState<AttendanceSummary | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([
      api.get('/reports/headcount'),
      api.get('/reports/leave-summary'),
      api.get('/reports/attendance-summary'),
    ]).then(([h, l, a]) => {
      setHeadcount(h.data);
      setLeaveSummary(l.data);
      setAttendance(a.data);
    }).finally(() => setLoading(false));
  }, []);

  if (loading) return <Spin size="large" style={{ display: 'block', marginTop: 80 }} />;

  const downloadCSV = (data: unknown[], filename: string) => {
    if (!data.length) return;
    const headers = Object.keys(data[0] as Record<string, unknown>).join(',');
    const rows = data.map(r => Object.values(r as Record<string, unknown>).join(','));
    const csv = [headers, ...rows].join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
  };

  return (
    <>
      <div className="page-header">
        <h2>Reports</h2>
      </div>

      <Row gutter={[20, 20]}>
        <Col xs={24} lg={12}>
          <Card className="content-card" title="Headcount" styles={{ body: { padding: 0 } }}
            extra={
              <Button size="small" icon={<DownloadOutlined />} onClick={() => downloadCSV(headcount?.byDepartment ?? [], 'headcount.csv')}>
                Export
              </Button>
            }
          >
            <div style={{ padding: 20, borderBottom: '1px solid #f0f0f0' }}>
              <Statistic title="Total Employees" value={headcount?.total ?? 0} prefix={<TeamOutlined />} />
            </div>
            <Table
              dataSource={headcount?.byDepartment ?? []}
              columns={[
                { title: 'Department', dataIndex: 'department', key: 'department' },
                { title: 'Count', dataIndex: 'count', key: 'count' },
              ]}
              rowKey="department"
              pagination={false}
              size="small"
              style={{ padding: 8 }}
              scroll={{ x: 'max-content' }}
            />
          </Card>
        </Col>

        <Col xs={24} lg={12}>
          <Card className="content-card" title="Leave Summary" styles={{ body: { padding: 0 } }}
            extra={
              <Button size="small" icon={<DownloadOutlined />} onClick={() => downloadCSV(leaveSummary?.byType ?? [], 'leaves.csv')}>
                Export
              </Button>
            }
          >
            <div style={{ padding: 20, borderBottom: '1px solid #f0f0f0' }}>
              <Row gutter={[8, 8]}>
                <Col xs={8}>
                  <Statistic title="Total" value={leaveSummary?.total ?? 0} styles={{ content: {  fontSize: 20  } }} />
                </Col>
                <Col xs={8}>
                  <Statistic title="Pending" value={leaveSummary?.pending ?? 0} styles={{ content: {  fontSize: 20, color: '#fdcb6e'  } }} />
                </Col>
                <Col xs={8}>
                  <Statistic title="Approved" value={leaveSummary?.approved ?? 0} styles={{ content: {  fontSize: 20, color: '#00b894'  } }} />
                </Col>
              </Row>
            </div>
            <Table
              dataSource={leaveSummary?.byType ?? []}
              columns={[
                { title: 'Leave Type', dataIndex: 'type', key: 'type' },
                { title: 'Count', dataIndex: 'count', key: 'count' },
              ]}
              rowKey="type"
              pagination={false}
              size="small"
              style={{ padding: 8 }}
              scroll={{ x: 'max-content' }}
            />
          </Card>
        </Col>

        <Col xs={24}>
          <Card className="content-card" title="Attendance Summary"
            extra={
              <Button size="small" icon={<DownloadOutlined />} onClick={() => downloadCSV(attendance ? [{ ...attendance }] : [], 'attendance.csv')}>
                Export
              </Button>
            }
          >
            <Row gutter={[16, 16]}>
              <Col xs={12} sm={6}>
                <Statistic title="Date" value={attendance?.date ?? '-'} prefix={<ClockCircleOutlined />} />
              </Col>
              <Col xs={12} sm={6}>
                <Statistic title="Records" value={attendance?.todayRecords ?? 0} />
              </Col>
              <Col xs={12} sm={6}>
                <Statistic title="Present" value={attendance?.present ?? 0} styles={{ content: {  color: '#00b894'  } }} />
              </Col>
              <Col xs={12} sm={6}>
                <Statistic title="Late" value={attendance?.late ?? 0} styles={{ content: {  color: '#e17055'  } }} />
              </Col>
            </Row>
          </Card>
        </Col>
      </Row>
    </>
  );
}
