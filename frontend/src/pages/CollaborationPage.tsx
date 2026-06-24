import { useEffect, useState, useRef } from 'react';
import {
  Card, List, Input, Button, Typography, Tag, Space, Row, Col, Empty, Spin, Badge, Modal, Form, message
} from 'antd';
import { SendOutlined, PushpinOutlined, TeamOutlined, PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import api from '../services/api';
import { useAuthStore } from '../store/authStore';
import { useBreakpoint } from '../hooks/useBreakpoint';

const { Title, Text } = Typography;

interface Channel {
  id: number;
  name: string;
  description?: string;
  channelType: string;
  departmentName?: string;
  messageCount: number;
  lastMessage?: { content: string; createdAt: string; authorName: string };
}

interface ChannelMessage {
  id: number;
  content: string;
  messageType: string;
  isPinned: boolean;
  createdAt: string;
  authorName: string;
  authorId: number;
}

export default function CollaborationPage() {
  const [channels, setChannels] = useState<Channel[]>([]);
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [messages, setMessages] = useState<ChannelMessage[]>([]);
  const [loading, setLoading] = useState(true);
  const [msgLoading, setMsgLoading] = useState(false);
  const [draft, setDraft] = useState('');
  const [channelModal, setChannelModal] = useState(false);
  const [editMsgModal, setEditMsgModal] = useState(false);
  const [editingMessage, setEditingMessage] = useState<ChannelMessage | null>(null);
  const [editContent, setEditContent] = useState('');
  const user = useAuthStore(s => s.user);
  const { isMobile } = useBreakpoint();
  const listRef = useRef<HTMLDivElement>(null);
  const showChannelList = !isMobile || selectedId === null;
  const showChat = !isMobile || selectedId !== null;
  const [channelForm] = Form.useForm();

  useEffect(() => {
    api.get('/collaboration/channels').then(res => {
      setChannels(res.data);
      if (res.data.length && !isMobile) setSelectedId(res.data[0].id);
    }).finally(() => setLoading(false));
  }, [isMobile]);

  useEffect(() => {
    if (!selectedId) return;
    setMsgLoading(true);
    api.get(`/collaboration/channels/${selectedId}/messages`)
      .then(res => setMessages(res.data))
      .finally(() => setMsgLoading(false));
  }, [selectedId]);

  useEffect(() => {
    listRef.current?.scrollTo({ top: listRef.current.scrollHeight });
  }, [messages]);

  const sendMessage = async () => {
    if (!draft.trim() || !selectedId) return;
    const res = await api.post(`/collaboration/channels/${selectedId}/messages`, { content: draft });
    setMessages(prev => [...prev, res.data]);
    setDraft('');
  };

  const handleCreateChannel = async (values: any) => {
    await api.post('/collaboration/channels', values);
    message.success('Channel created');
    setChannelModal(false);
    channelForm.resetFields();
    const res = await api.get('/collaboration/channels');
    setChannels(res.data);
  };

  const openEditMsg = (m: ChannelMessage) => {
    setEditingMessage(m);
    setEditContent(m.content);
    setEditMsgModal(true);
  };

  const handleEditMsg = async () => {
    if (!editingMessage || !selectedId) return;
    await api.put(`/collaboration/channels/${selectedId}/messages/${editingMessage.id}`, { content: editContent });
    message.success('Message updated');
    setEditMsgModal(false);
    setEditingMessage(null);
    setMessages(prev => prev.map(m => m.id === editingMessage.id ? { ...m, content: editContent } : m));
  };

  const handleDeleteMsg = async (msgId: number) => {
    await api.delete(`/collaboration/channels/${selectedId}/messages/${msgId}`);
    message.success('Message deleted');
    setMessages(prev => prev.filter(m => m.id !== msgId));
  };

  const selected = channels.find(c => c.id === selectedId);

  if (loading) return <div style={{ textAlign: 'center', padding: 48 }}><Spin size="large" /></div>;

  return (
    <div>
      <div className="page-header">
        <div>
          <Title level={4} style={{ margin: 0 }}>Collaboration Hub</Title>
          <Text type="secondary">Department channels — messages, announcements, and team coordination</Text>
        </div>
      </div>

      <Row gutter={16} className="collab-layout">
        {showChannelList && (
          <Col xs={24} md={8} lg={7}>
            <Card
              className="content-card collab-channel-list"
              title="Channels"
              extra={<Button size="small" icon={<PlusOutlined />} onClick={() => setChannelModal(true)}>New</Button>}
            >
              <List
                dataSource={channels}
                renderItem={(c) => (
                  <List.Item
                    className={`collab-channel-item${selectedId === c.id ? ' active' : ''}`}
                    onClick={() => setSelectedId(c.id)}
                    style={{ cursor: 'pointer', padding: '10px 8px', borderRadius: 8 }}
                  >
                    <List.Item.Meta
                      title={
                        <Space>
                          <Text strong={selectedId === c.id}>#{c.name}</Text>
                          {c.channelType === 'Department' && <Tag color="blue">Dept</Tag>}
                        </Space>
                      }
                      description={
                        c.lastMessage
                          ? <Text type="secondary" ellipsis>{c.lastMessage.authorName}: {c.lastMessage.content}</Text>
                          : c.description
                      }
                    />
                    <Badge count={c.messageCount} style={{ backgroundColor: '#6c5ce7' }} />
                  </List.Item>
                )}
              />
            </Card>
          </Col>
        )}

        {showChat && (
          <Col xs={24} md={16} lg={17}>
            <Card
              className="content-card collab-chat-panel"
              title={
                selected ? (
                  <Space>
                    {isMobile && (
                      <Button type="text" size="small" onClick={() => setSelectedId(null)}>←</Button>
                    )}
                    <TeamOutlined />
                    <span>#{selected.name}</span>
                    {selected.departmentName && <Tag>{selected.departmentName}</Tag>}
                  </Space>
                ) : 'Select a channel'
              }
            >
              {!selectedId ? (
                <Empty description="Select a channel to start collaborating" />
              ) : (
                <>
                  <div ref={listRef} className="collab-messages">
                    {msgLoading ? <Spin /> : messages.length === 0 ? (
                      <Empty description="No messages yet" />
                    ) : messages.map(m => {
                      const canEdit = m.authorId === user?.employeeId;
                      return (
                        <div key={m.id} className={`collab-message${m.messageType === 'Announcement' ? ' announcement' : ''}`}>
                          <div className="collab-message-head">
                            <Text strong>{m.authorName}</Text>
                            <Text type="secondary" style={{ fontSize: 11, marginLeft: 8 }}>
                              {new Date(m.createdAt).toLocaleString()}
                            </Text>
                            {m.isPinned && <PushpinOutlined style={{ marginLeft: 6, color: '#6c5ce7' }} />}
                            {m.messageType === 'Announcement' && <Tag color="purple" style={{ marginLeft: 6 }}>Announcement</Tag>}
                            {canEdit && (
                              <Space style={{ marginLeft: 'auto' }}>
                                <Button type="text" size="small" icon={<EditOutlined />} onClick={() => openEditMsg(m)} />
                                <Button type="text" size="small" danger icon={<DeleteOutlined />} onClick={() => handleDeleteMsg(m.id)} />
                              </Space>
                            )}
                          </div>
                          <Text>{m.content}</Text>
                        </div>
                      );
                    })}
                  </div>
                  <div className="collab-compose">
                    <Input.TextArea
                      value={draft}
                      onChange={e => setDraft(e.target.value)}
                      placeholder={`Message #${selected?.name}`}
                      autoSize={{ minRows: 1, maxRows: 4 }}
                      onPressEnter={e => { if (!e.shiftKey) { e.preventDefault(); sendMessage(); } }}
                    />
                    <Button type="primary" icon={<SendOutlined />} onClick={sendMessage} disabled={!draft.trim()}>
                      Send
                    </Button>
                  </div>
                </>
              )}
            </Card>
          </Col>
        )}
      </Row>

      <Modal title="Create Channel" open={channelModal} onCancel={() => setChannelModal(false)} onOk={() => channelForm.submit()}>
        <Form form={channelForm} onFinish={handleCreateChannel} layout="vertical">
          <Form.Item name="name" label="Name" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="description" label="Description">
            <Input.TextArea rows={2} />
          </Form.Item>
          <Form.Item name="channelType" label="Type">
            <Input placeholder="General, Department, Announcements" />
          </Form.Item>
        </Form>
      </Modal>

      <Modal title="Edit Message" open={editMsgModal} onCancel={() => { setEditMsgModal(false); setEditingMessage(null); }} onOk={handleEditMsg}>
        <Input.TextArea value={editContent} onChange={e => setEditContent(e.target.value)} rows={3} />
      </Modal>
    </div>
  );
}
