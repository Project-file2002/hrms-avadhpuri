import { useEffect, useState } from 'react';
import { Card, Button, Modal, Form, Input, Select, Tag, Typography, Space, Spin, Avatar, Divider } from 'antd';
import { PlusOutlined, MessageOutlined, EyeOutlined, PushpinOutlined, SendOutlined } from '@ant-design/icons';
import api from '../services/api';
import { useRoles } from '../utils/roles';

const { Title, Text } = Typography;

const categories = ['All', 'General', 'Tech', 'HR', 'Announcements', 'Ideas', 'Q&A'];

function DiscussionsPage() {
  const { canModerateContent } = useRoles();
  const [threads, setThreads] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [modal, setModal] = useState(false);
  const [detail, setDetail] = useState<any>(null);
  const [reply, setReply] = useState('');
  const [category, setCategory] = useState('All');
  const [form] = Form.useForm();

  useEffect(() => { fetchThreads(); }, [category]);

  const fetchThreads = async () => {
    try { const res = await api.get(`/knowledge/threads${category !== 'All' ? `?category=${category}` : ''}`); setThreads(res.data); }
    finally { setLoading(false); }
  };

  const handleCreate = async (values: any) => {
    await api.post('/knowledge/threads', values);
    setModal(false); form.resetFields(); fetchThreads();
  };

  const openThread = async (id: number) => {
    const res = await api.get(`/knowledge/threads/${id}`);
    setDetail(res.data);
  };

  const handleReply = async () => {
    if (!reply.trim()) return;
    await api.post(`/knowledge/threads/${detail.id}/reply`, { content: reply });
    setReply('');
    const res = await api.get(`/knowledge/threads/${detail.id}`);
    setDetail(res.data);
  };

  const handleDelete = async (id: number) => {
    await api.delete(`/knowledge/threads/${id}`);
    setDetail(null); fetchThreads();
  };

  return (
    <div>
      <div className="page-toolbar">
        <div className="page-toolbar-text">
          <Title level={4} style={{ margin: 0 }}>
            <MessageOutlined style={{ marginRight: 8, color: '#6c5ce7' }} />
            Knowledge Hub
          </Title>
          <Text type="secondary">Share knowledge, ask questions, discuss ideas</Text>
        </div>
        <div className="page-toolbar-actions">
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setModal(true)}>New Thread</Button>
        </div>
      </div>

      <div style={{ marginBottom: 16 }}>
        <Space wrap>
          {categories.map(c => <Tag key={c} color={category === c ? '#6c5ce7' : undefined} style={{ cursor: 'pointer', padding: '4px 12px' }} onClick={() => setCategory(c)}>{c}</Tag>)}
        </Space>
      </div>

      <Card style={{ borderRadius: 12 }}>
        {loading ? (
          <div style={{ textAlign: 'center', padding: 40 }}><Spin /></div>
        ) : (
          threads.map((t: any) => (
            <div key={t.id} style={{ cursor: 'pointer', padding: '16px 0', display: 'flex', alignItems: 'flex-start', gap: 12, borderBottom: '1px solid #f0f0f0' }}
              onClick={() => openThread(t.id)}
            >
              <Avatar style={{ background: '#6c5ce7' }}>{t.createdByName[0]}</Avatar>
              <div style={{ flex: 1 }}>
                <Space>{t.isPinned && <PushpinOutlined style={{ color: '#fa8c16' }} />}<Text strong>{t.title}</Text><Tag>{t.category}</Tag></Space>
                <div><Space><Text type="secondary">{t.createdByName} · {new Date(t.createdAt).toLocaleDateString()}</Text>{t.tags && t.tags.split(',').map((tag: string) => <Tag key={tag}>{tag.trim()}</Tag>)}</Space></div>
              </div>
              <Space size="small" style={{ color: '#999' }}>
                <span><EyeOutlined /> {t.viewCount}</span>
                <span><MessageOutlined /> {t.replyCount}</span>
              </Space>
            </div>
          ))
        )}
      </Card>

      <Modal title="New Discussion" open={modal} onCancel={() => setModal(false)} onOk={() => form.submit()}>
        <Form form={form} onFinish={handleCreate} layout="vertical">
          <Form.Item name="title" label="Title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="category" label="Category" initialValue="General"><Select>{categories.filter(c => c !== 'All').map(c => <Select.Option key={c} value={c}>{c}</Select.Option>)}</Select></Form.Item>
          <Form.Item name="tags" label="Tags (comma-separated)"><Input placeholder="e.g. react, typescript, tips" /></Form.Item>
          <Form.Item name="content" label="Content" rules={[{ required: true }]}><Input.TextArea rows={5} /></Form.Item>
        </Form>
      </Modal>

      <Modal title={detail?.title} open={!!detail} onCancel={() => setDetail(null)} footer={null} width="100%" style={{ maxWidth: 640 }}>
        {detail && (
          <>
            <Space><Tag>{detail.category}</Tag><Text type="secondary">by {detail.createdByName} · {new Date(detail.createdAt).toLocaleDateString()} · {detail.viewCount} views</Text></Space>
            <div style={{ marginTop: 16, whiteSpace: 'pre-wrap', background: '#f9f9f9', padding: 16, borderRadius: 8 }}>{detail.content}</div>
            <Divider /><Text strong>Replies ({detail.replies?.length})</Text>
            {detail.replies?.map((r: any) => (
              <Card key={r.id} size="small" style={{ marginTop: 8, borderRadius: 8 }}>
                <Space><Avatar style={{ background: '#6c5ce7' }} size={24}>{r.createdByName[0]}</Avatar><Text strong>{r.createdByName}</Text><Text type="secondary">{new Date(r.createdAt).toLocaleDateString()}</Text></Space>
                <div style={{ marginTop: 8, whiteSpace: 'pre-wrap' }}>{r.content}</div>
              </Card>
            ))}
            <div className="page-toolbar" style={{ marginTop: 16 }}>
              <Input.TextArea rows={2} value={reply} onChange={e => setReply(e.target.value)} placeholder="Write a reply..." style={{ flex: 1 }} />
              <Button type="primary" icon={<SendOutlined />} onClick={handleReply} disabled={!reply.trim()}>Reply</Button>
            </div>
            {canModerateContent && (
              <Button type="text" danger style={{ marginTop: 8 }} onClick={() => handleDelete(detail.id)}>Delete Thread</Button>
            )}
          </>
        )}
      </Modal>
    </div>
  );
}

export default DiscussionsPage;
