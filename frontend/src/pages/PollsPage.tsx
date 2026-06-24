import { useEffect, useState } from 'react';
import { Card, Button, Modal, Form, Input, Select, Tag, Typography, Space, Row, Col, Progress, Checkbox, Radio, message } from 'antd';
import { PlusOutlined, DeleteOutlined, BarChartOutlined } from '@ant-design/icons';
import api from '../services/api';
import { useRoles } from '../utils/roles';

const { Title, Text } = Typography;

function PollsPage() {
  const { canCreatePoll, canManagePolls } = useRoles();
  const [polls, setPolls] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [modal, setModal] = useState(false);
  const [form] = Form.useForm();
  const [options, setOptions] = useState<string[]>(['', '']);

  useEffect(() => { fetchPolls(); }, []);

  const fetchPolls = async () => {
    try { const res = await api.get('/knowledge/polls'); setPolls(res.data); }
    finally { setLoading(false); }
  };

  const handleCreate = async (values: any) => {
    const filtered = options.filter(o => o.trim());
    if (filtered.length < 2) { message.warning('Add at least 2 options'); return; }
    await api.post('/knowledge/polls', { ...values, options: filtered });
    message.success('Poll created!');
    setModal(false); form.resetFields(); setOptions(['', '']); fetchPolls();
  };

  const handleVote = async (pollId: number, optionIds: number[]) => {
    await api.post(`/knowledge/polls/${pollId}/vote`, { optionIds });
    fetchPolls();
  };

  const handleDelete = async (id: number) => {
    await api.delete(`/knowledge/polls/${id}`);
    fetchPolls();
  };

  const addOption = () => setOptions([...options, '']);

  return (
    <div>
      <div className="page-toolbar">
        <div className="page-toolbar-text">
          <Title level={4} style={{ margin: 0 }}>
            <BarChartOutlined style={{ marginRight: 8, color: '#6c5ce7' }} />
            Polls & Surveys
          </Title>
          <Text type="secondary">Create and participate in company polls</Text>
        </div>
        <div className="page-toolbar-actions">
          {canCreatePoll && (
            <Button type="primary" icon={<PlusOutlined />} onClick={() => setModal(true)}>New Poll</Button>
          )}
        </div>
      </div>

      <Row gutter={[16, 16]}>
        {polls.map(poll => {
          const total = poll.totalVotes || 1;
          return (
            <Col xs={24} sm={12} lg={8} key={poll.id}>
              <Card style={{ borderRadius: 12, height: '100%' }}
                title={<Text strong>{poll.question}</Text>}
                extra={poll.userVoted ? <Tag color="green">Voted</Tag> : null}
                actions={[
                  !poll.userVoted ? (
                    poll.multiVote
                      ? <Checkbox.Group onChange={vals => vals.length && handleVote(poll.id, vals as number[])}>
                          <Button type="link" size="small">Submit Vote</Button>
                        </Checkbox.Group>
                      : null
                  ) : null,
                  canManagePolls ? (
                    <Button type="text" danger icon={<DeleteOutlined />} onClick={() => handleDelete(poll.id)} />
                  ) : null,
                ].filter(Boolean)}
              >
                <div style={{ marginTop: 8 }}>
                  <Text type="secondary" style={{ fontSize: 12 }}>by {poll.createdByName} · {new Date(poll.createdAt).toLocaleDateString()}</Text>
                  {poll.expiresAt && <Tag style={{ marginLeft: 8 }}>{new Date(poll.expiresAt).toLocaleDateString()}</Tag>}
                </div>
                {poll.options.map((opt: any) => {
                  const pct = total > 0 ? Math.round(opt.voteCount / total * 100) : 0;
                  return (
                    <div key={opt.id} style={{ marginTop: 12, cursor: poll.userVoted ? 'default' : 'pointer' }}
                      onClick={() => !poll.userVoted && handleVote(poll.id, [opt.id])}>
                      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                        <Text>{opt.text}</Text>
                        <Text type="secondary">{opt.voteCount} ({pct}%)</Text>
                      </div>
                      <Progress percent={pct} showInfo={false} strokeColor={opt.userVotedThis ? '#6c5ce7' : '#d9d9d9'} />
                    </div>
                  );
                })}
                <Text type="secondary" style={{ fontSize: 11, marginTop: 8, display: 'block' }}>
                  Total: {total} vote{total !== 1 ? 's' : ''} · {poll.multiVote ? 'Multiple choice' : 'Single choice'}
                </Text>
              </Card>
            </Col>
          );
        })}
      </Row>

      <Modal title="Create Poll" open={modal} onCancel={() => setModal(false)} onOk={() => form.submit()}>
        <Form form={form} onFinish={handleCreate} layout="vertical">
          <Form.Item name="question" label="Question" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="multiVote" label="Allow multiple choices" valuePropName="checked">
            <Checkbox />
          </Form.Item>
          <Form.Item name="expiresAt" label="Expires"><Input type="date" /></Form.Item>
        </Form>
        <Text strong>Options</Text>
        {options.map((opt, i) => (
          <Input key={i} value={opt} onChange={e => { const o = [...options]; o[i] = e.target.value; setOptions(o); }}
            placeholder={`Option ${i + 1}`} style={{ marginTop: 8 }} />
        ))}
        <Button type="dashed" onClick={addOption} block style={{ marginTop: 8 }}>+ Add Option</Button>
      </Modal>
    </div>
  );
}

export default PollsPage;
