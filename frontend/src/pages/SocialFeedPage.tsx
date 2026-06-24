import { useEffect, useState } from 'react';
import {
  Card, Input, Button, Typography, Space, Avatar, Tag, Divider,
  message, Spin, Empty, Modal, Select, Form, List, Popconfirm,
} from 'antd';
import {
  HeartOutlined, HeartFilled, MessageOutlined, SendOutlined,
  TrophyOutlined, BellOutlined, TeamOutlined, DeleteOutlined,
} from '@ant-design/icons';
import api from '../services/api';
import { useRoles } from '../utils/roles';

const { Text, Title, Paragraph } = Typography;
const { TextArea } = Input;

interface PostComment {
  id: number;
  content: string;
  createdAt: string;
  author: string;
}

interface Post {
  id: number;
  content: string;
  type: string;
  imageUrl?: string;
  tags?: string;
  createdAt: string;
  author: string;
  authorAvatar: string;
  likeCount: number;
  isLiked: boolean;
  comments: PostComment[];
}

const typeConfig: Record<string, { icon: React.ReactNode; color: string; label: string }> = {
  Post: { icon: <TeamOutlined />, color: '#6c5ce7', label: 'Post' },
  Recognition: { icon: <TrophyOutlined />, color: '#52c41a', label: 'Recognition' },
  Announcement: { icon: <BellOutlined />, color: '#faad14', label: 'Announcement' },
};

function SocialFeedPage() {
  const { canModerateContent } = useRoles();
  const [posts, setPosts] = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);
  const [createModal, setCreateModal] = useState(false);
  const [postContent, setPostContent] = useState('');
  const [postType, setPostType] = useState('Post');
  const [postTags, setPostTags] = useState('');
  const [commentInputs, setCommentInputs] = useState<Record<number, string>>({});

  useEffect(() => { fetchFeed(); }, []);

  const fetchFeed = async () => {
    try {
      const res = await api.get('/social/feed');
      setPosts(res.data);
    } catch { /* */ }
    setLoading(false);
  };

  const handleCreate = async () => {
    if (!postContent.trim()) { message.warning('Write something!'); return; }
    await api.post('/social', {
      content: postContent,
      type: postType,
      tags: postTags || null,
    });
    message.success('Posted!');
    setCreateModal(false);
    setPostContent('');
    setPostType('Post');
    setPostTags('');
    fetchFeed();
  };

  const handleLike = async (postId: number) => {
    await api.post(`/social/${postId}/like`);
    setPosts(posts.map(p => p.id === postId ? { ...p, isLiked: !p.isLiked, likeCount: p.isLiked ? p.likeCount - 1 : p.likeCount + 1 } : p));
  };

  const handleComment = async (postId: number) => {
    const content = commentInputs[postId]?.trim();
    if (!content) return;
    const res = await api.post(`/social/${postId}/comments`, { content });
    setPosts(posts.map(p => p.id === postId ? { ...p, comments: [...p.comments, res.data] } : p));
    setCommentInputs({ ...commentInputs, [postId]: '' });
  };

  const handleDelete = async (postId: number) => {
    await api.delete(`/social/${postId}`);
    message.success('Deleted');
    fetchFeed();
  };

  if (loading) return <Spin size="large" style={{ display: 'block', margin: 80 }} />;

  return (
    <div className="feed-container">
      <div className="page-toolbar">
        <div className="page-toolbar-text">
          <Title level={4} style={{ margin: 0 }}>Company Feed</Title>
          <Text type="secondary">Share updates, recognize peers, and stay informed</Text>
        </div>
        <div className="page-toolbar-actions">
          <Button type="primary" icon={<SendOutlined />} onClick={() => setCreateModal(true)}>
            Create Post
          </Button>
        </div>
      </div>

      {posts.length === 0 && <Empty description="No posts yet. Be the first to share!" />}

      <Space orientation="vertical" style={{ width: '100%' }} size={16}>
        {posts.map(post => {
          const cfg = typeConfig[post.type] || typeConfig.Post;
          return (
            <Card
              key={post.id}
              style={{ borderRadius: 12, borderLeft: `4px solid ${cfg.color}` }}
              styles={{ body: { padding: 20 } }}
            >
              <div style={{ display: 'flex', gap: 12 }}>
                <Avatar style={{ background: cfg.color, flexShrink: 0 }}>
                  {post.authorAvatar}
                </Avatar>
                <div style={{ flex: 1 }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', flexWrap: 'wrap', gap: 8 }}>
                    <Space wrap>
                      <Text strong>{post.author}</Text>
                      <Tag color={cfg.color} style={{ fontSize: 10, lineHeight: '18px' }}>
                        {cfg.icon} {cfg.label}
                      </Tag>
                    </Space>
                    <Space size={4} wrap>
                      <Text type="secondary" style={{ fontSize: 12 }}>
                        {new Date(post.createdAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })}
                      </Text>
                      {canModerateContent && (
                        <Popconfirm title="Delete this post?" onConfirm={() => handleDelete(post.id)}>
                          <Button type="text" size="small" danger icon={<DeleteOutlined />} />
                        </Popconfirm>
                      )}
                    </Space>
                  </div>

                  <Paragraph style={{ margin: '8px 0', whiteSpace: 'pre-wrap' }}>{post.content}</Paragraph>

                  {post.tags && (
                    <div style={{ marginBottom: 8 }}>
                      {JSON.parse(post.tags).map((tag: string, i: number) => (
                        <Tag key={i} style={{ fontSize: 11 }}>{tag}</Tag>
                      ))}
                    </div>
                  )}

                  <div style={{ display: 'flex', gap: 16, marginTop: 8 }}>
                    <Button
                      type="text"
                      size="small"
                      icon={post.isLiked ? <HeartFilled style={{ color: '#ff4d4f' }} /> : <HeartOutlined />}
                      onClick={() => handleLike(post.id)}
                    >
                      {post.likeCount}
                    </Button>
                    <Button type="text" size="small" icon={<MessageOutlined />}>
                      {post.comments.length}
                    </Button>
                  </div>

                  {post.comments.length > 0 && (
                    <div style={{ marginTop: 12, padding: 12, background: '#fafafa', borderRadius: 8 }}>
                      {post.comments.map(c => (
                        <div key={c.id} style={{ display: 'flex', gap: 8, marginBottom: 8 }}>
                          <Text strong style={{ fontSize: 12, flexShrink: 0 }}>{c.author}:</Text>
                          <Text style={{ fontSize: 12 }}>{c.content}</Text>
                        </div>
                      ))}
                    </div>
                  )}

                  <div style={{ marginTop: 8, display: 'flex', gap: 8 }}>
                    <Input
                      size="small"
                      placeholder="Write a comment..."
                      value={commentInputs[post.id] || ''}
                      onChange={e => setCommentInputs({ ...commentInputs, [post.id]: e.target.value })}
                      onPressEnter={() => handleComment(post.id)}
                      style={{ borderRadius: 16, flex: 1 }}
                    />
                    <Button size="small" type="primary" icon={<SendOutlined />} onClick={() => handleComment(post.id)} />
                  </div>
                </div>
              </div>
            </Card>
          );
        })}
      </Space>

      <Modal
        title="Create Post"
        open={createModal}
        onCancel={() => setCreateModal(false)}
        onOk={handleCreate}
        okText="Post"
      >
        <Space orientation="vertical" style={{ width: '100%' }}>
          <Select value={postType} onChange={setPostType} style={{ width: '100%' }}>
            <Select.Option value="Post">📝 Post</Select.Option>
            <Select.Option value="Recognition">🏆 Recognition</Select.Option>
            <Select.Option value="Announcement">🔔 Announcement</Select.Option>
          </Select>
          <TextArea
            rows={4}
            placeholder={postType === 'Recognition' ? 'Recognize a colleague... @mention their name and why' : "What's on your mind?"}
            value={postContent}
            onChange={e => setPostContent(e.target.value)}
          />
          <Input placeholder="Tags (comma separated)" value={postTags} onChange={e => setPostTags(e.target.value)} />
        </Space>
      </Modal>
    </div>
  );
}

export default SocialFeedPage;
