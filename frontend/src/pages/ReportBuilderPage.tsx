import { useEffect, useState } from 'react';
import {
  Card, Table, Button, Modal, Form, Input, Select, Typography,
  Space, Tabs, Tag, Empty, message, Spin, Alert, Checkbox, Divider, Row, Col
} from 'antd';
import {
  PlusOutlined, PlayCircleOutlined, DeleteOutlined,
  BarChartOutlined, DatabaseOutlined
} from '@ant-design/icons';
import api from '../services/api';

const { Title, Text } = Typography;

interface ReportDef {
  id: number;
  title: string;
  dataSource: string;
  columns: string;
  filters?: string;
  groupBy?: string;
  createdAt: string;
}

interface ReportResult {
  title: string;
  dataSource: string;
  columnNames: string[];
  rows: Record<string, any>[];
}

const dataSourceColors: Record<string, string> = {
  Employee: 'blue', Leave: 'green', Attendance: 'orange',
  Performance: 'purple', Department: 'cyan', Recruitment: 'red',
};

function ReportBuilderPage() {
  const [reports, setReports] = useState<ReportDef[]>([]);
  const [dataSources, setDataSources] = useState<Record<string, string[]>>({});
  const [createModal, setCreateModal] = useState(false);
  const [result, setResult] = useState<ReportResult | null>(null);
  const [running, setRunning] = useState(false);
  const [form] = Form.useForm();

  const [selectedDS, setSelectedDS] = useState('');
  const [availableCols, setAvailableCols] = useState<string[]>([]);
  const [selectedCols, setSelectedCols] = useState<string[]>([]);

  useEffect(() => {
    fetchReports();
    api.get('/reports-builder/datasources').then(r => setDataSources(r.data));
  }, []);

  const fetchReports = async () => {
    const res = await api.get('/reports-builder');
    setReports(res.data);
  };

  const handleDSChange = (ds: string) => {
    setSelectedDS(ds);
    setAvailableCols(dataSources[ds] || []);
    setSelectedCols([]);
  };

  const handleCreate = async (values: any) => {
    const payload = {
      title: values.title,
      dataSource: selectedDS,
      columns: JSON.stringify(selectedCols),
    };
    await api.post('/reports-builder', payload);
    message.success('Report created!');
    setCreateModal(false);
    form.resetFields();
    setSelectedDS('');
    setSelectedCols([]);
    fetchReports();
  };

  const handleRun = async (report: ReportDef) => {
    setRunning(true);
    setResult(null);
    try {
      const res = await api.post(`/reports-builder/${report.id}/run`);
      setResult(res.data);
    } catch {
      message.error('Error running report');
    }
    setRunning(false);
  };

  const columns = [
    { title: 'Title', dataIndex: 'title', key: 'title' },
    {
      title: 'Data Source', dataIndex: 'dataSource', key: 'ds',
      render: (ds: string) => <Tag color={dataSourceColors[ds]}>{ds}</Tag>,
    },
    {
      title: 'Action', key: 'action', width: 200,
      render: (_: any, r: ReportDef) => (
        <Space>
          <Button type="primary" size="small" icon={<PlayCircleOutlined />} onClick={() => handleRun(r)}>
            Run
          </Button>
          <Button size="small" danger icon={<DeleteOutlined />} onClick={async () => { await api.delete(`/reports-builder/${r.id}`); fetchReports(); }} />
        </Space>
      ),
    },
  ];

  return (
    <div>
      <div className="page-intro">
        <Title level={4} style={{ margin: 0 }}>
          <BarChartOutlined style={{ marginRight: 8, color: '#6c5ce7' }} />
          Report Builder
        </Title>
        <Text type="secondary">Create custom reports by selecting data sources and columns</Text>
      </div>

      <Row gutter={[16, 16]}>
        <Col xs={24} xl={result ? 12 : 24}>
          <Card
            style={{ borderRadius: 12 }}
            title="Saved Reports"
            extra={<Button type="primary" icon={<PlusOutlined />} onClick={() => { setCreateModal(true); setSelectedDS(''); setSelectedCols([]); form.resetFields(); }}>New Report</Button>}
          >
            <Table dataSource={reports} columns={columns} rowKey="id" pagination={false} scroll={{ x: 'max-content' }} />
          </Card>
        </Col>

        {result && (
          <Col xs={24} xl={12}>
            <Card
              style={{ borderRadius: 12 }}
              title={<><DatabaseOutlined /> {result.title} ({result.dataSource})</>}
              extra={<Button size="small" onClick={() => setResult(null)}>Close</Button>}
            >
              <Table
                dataSource={result.rows}
                columns={result.columnNames.map(col => ({
                  title: col,
                  dataIndex: col,
                  key: col,
                  ellipsis: true,
                  render: (v: any) => {
                    if (v === null || v === undefined) return '-';
                    if (typeof v === 'boolean') return v ? 'Yes' : 'No';
                    if (typeof v === 'object') return JSON.stringify(v);
                    return String(v);
                  }
                }))}
                rowKey={(_, i) => String(i)}
                pagination={false}
                scroll={{ x: 'max-content' }}
                size="small"
              />
            </Card>
          </Col>
        )}
      </Row>

      {running && <Spin style={{ display: 'block', margin: 40 }} />}

      <Modal
        title="Create Report"
        open={createModal}
        onCancel={() => setCreateModal(false)}
        onOk={() => form.submit()}
        width={500}
      >
        <Form form={form} onFinish={handleCreate} layout="vertical">
          <Form.Item name="title" label="Report Title" rules={[{ required: true }]}>
            <Input placeholder="e.g., Employee Directory" />
          </Form.Item>
          <Form.Item label="Data Source" required>
            <Select value={selectedDS || undefined} onChange={handleDSChange} placeholder="Select data source">
              {Object.keys(dataSources).map(ds => (
                <Select.Option key={ds} value={ds}>
                  <Tag color={dataSourceColors[ds]}>{ds}</Tag>
                </Select.Option>
              ))}
            </Select>
          </Form.Item>

          {availableCols.length > 0 && (
            <>
              <Text strong style={{ display: 'block', marginBottom: 8 }}>Select Columns</Text>
              <div style={{ maxHeight: 200, overflowY: 'auto', border: '1px solid #f0f0f0', borderRadius: 8, padding: 12 }}>
                <Checkbox
                  checked={selectedCols.length === availableCols.length}
                  indeterminate={selectedCols.length > 0 && selectedCols.length < availableCols.length}
                  onChange={(e) => setSelectedCols(e.target.checked ? [...availableCols] : [])}
                  style={{ marginBottom: 8 }}
                >
                  Select All
                </Checkbox>
                <Divider style={{ margin: '4px 0' }} />
                {availableCols.map(col => (
                  <div key={col} style={{ padding: '4px 0' }}>
                    <Checkbox
                      checked={selectedCols.includes(col)}
                      onChange={(e) => {
                        setSelectedCols(e.target.checked
                          ? [...selectedCols, col]
                          : selectedCols.filter(c => c !== col));
                      }}
                    >
                      {col}
                    </Checkbox>
                  </div>
                ))}
              </div>
            </>
          )}
        </Form>
      </Modal>
    </div>
  );
}

export default ReportBuilderPage;
