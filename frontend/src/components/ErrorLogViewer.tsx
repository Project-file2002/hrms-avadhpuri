import { useState, useEffect } from 'react';
import { Modal, Table, Button, Tag, Space } from 'antd';
import { DownloadOutlined, DeleteOutlined, BugOutlined } from '@ant-design/icons';
import { getLogs, clearLogs, downloadLogs } from '../utils/errorLogger';

interface LogEntry {
  id: string;
  timestamp: string;
  level: string;
  message: string;
  source: string;
  stack?: string;
  metadata?: Record<string, unknown>;
}

export default function ErrorLogViewer() {
  const [open, setOpen] = useState(false);
  const [logs, setLogs] = useState<LogEntry[]>([]);

  const refresh = () => setLogs(getLogs());

  useEffect(() => {
    if (open) refresh();
  }, [open]);

  const columns = [
    {
      title: 'Time', dataIndex: 'timestamp', key: 'timestamp', width: 180,
      render: (v: string) => new Date(v).toLocaleString(),
    },
    {
      title: 'Level', dataIndex: 'level', key: 'level', width: 80,
      render: (v: string) => (
        <Tag color={v === 'error' ? 'red' : v === 'warn' ? 'orange' : 'blue'}>{v.toUpperCase()}</Tag>
      ),
    },
    { title: 'Source', dataIndex: 'source', key: 'source', width: 150 },
    { title: 'Message', dataIndex: 'message', key: 'message' },
  ];

  return (
    <>
      <Button icon={<BugOutlined />} onClick={() => setOpen(true)} size="small">
        Error Logs
      </Button>
      <Modal
        title="Error Log Viewer"
        open={open}
        onCancel={() => setOpen(false)}
        width={1000}
        footer={
          <Space>
            <Button icon={<DownloadOutlined />} onClick={() => { downloadLogs(); refresh(); }}>
              Download Logs
            </Button>
            <Button icon={<DeleteOutlined />} danger onClick={() => { clearLogs(); refresh(); }}>
              Clear Logs
            </Button>
          </Space>
        }
      >
        <Table
          dataSource={logs}
          columns={columns}
          rowKey="id"
          size="small"
          pagination={{ pageSize: 10 }}
          expandable={{
            expandedRowRender: (r: LogEntry) => (
              <pre style={{ fontSize: 12, maxHeight: 200, overflow: 'auto' }}>
                {r.stack && <><strong>Stack:</strong> {r.stack}<br /></>}
                {r.metadata && <><strong>Metadata:</strong> {JSON.stringify(r.metadata, null, 2)}</>}
              </pre>
            ),
          }}
        />
      </Modal>
    </>
  );
}
