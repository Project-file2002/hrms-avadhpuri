import { useEffect, useState } from 'react';
import {
  Card, Typography, Tag, Space, Button, Tabs, Badge, Empty, Spin
} from 'antd';
import { StarOutlined, StarFilled, InboxOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';

const { Title, Text } = Typography;

interface Notification {
  id: number;
  title: string;
  message: string;
  category: string;
  source: string;
  priority: string;
  link?: string;
  isRead: boolean;
  isStarred: boolean;
  createdAt: string;
}

interface InboxData {
  unreadCount: number;
  starredCount: number;
  today: Notification[];
  yesterday: Notification[];
  earlier: Notification[];
  categories: { category: string; count: number }[];
}

const categoryColors: Record<string, string> = {
  Urgent: 'red', Reminder: 'orange', Information: 'blue', Task: 'green', Approval: 'purple',
};

function NotificationItem({ item, onRead, onStar }: {
  item: Notification;
  onRead: (id: number) => void;
  onStar: (id: number) => void;
}) {
  const navigate = useNavigate();
  return (
    <div
      className={`inbox-item${item.isRead ? '' : ' unread'}`}
      onClick={() => { if (!item.isRead) onRead(item.id); if (item.link) navigate(item.link); }}
      style={{ cursor: 'pointer', padding: '12px 16px', display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', borderBottom: '1px solid #f0f0f0' }}
    >
      <div style={{ flex: 1, minWidth: 0 }}>
        <Space wrap>
          {!item.isRead && <Badge status="processing" />}
          <Text strong={!item.isRead}>{item.title}</Text>
          <Tag color={categoryColors[item.category] ?? 'default'}>{item.category}</Tag>
          <Tag>{item.source}</Tag>
        </Space>
        <div>
          <Text type="secondary">{item.message}</Text>
          <br />
          <Text type="secondary" style={{ fontSize: 11 }}>
            {new Date(item.createdAt).toLocaleString()}
          </Text>
        </div>
      </div>
      <Button type="text" size="small" icon={item.isStarred ? <StarFilled style={{ color: '#fdcb6e' }} /> : <StarOutlined />}
        onClick={(e) => { e.stopPropagation(); onStar(item.id); }} />
    </div>
  );
}

export default function InboxPage() {
  const [inbox, setInbox] = useState<InboxData | null>(null);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<string>('all');

  const fetchInbox = async () => {
    setLoading(true);
    try {
      const res = await api.get('/notifications/inbox');
      setInbox(res.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchInbox(); }, []);

  const markRead = async (id: number) => {
    await api.put(`/notifications/${id}/read`);
    fetchInbox();
  };

  const toggleStar = async (id: number) => {
    await api.put(`/notifications/${id}/star`);
    fetchInbox();
  };

  const markAllRead = async () => {
    await api.put('/notifications/read-all');
    fetchInbox();
  };

  const allItems = [
    ...(inbox?.today ?? []),
    ...(inbox?.yesterday ?? []),
    ...(inbox?.earlier ?? []),
  ];

  const filtered = filter === 'unread'
    ? allItems.filter(n => !n.isRead)
    : filter === 'starred'
      ? allItems.filter(n => n.isStarred)
      : allItems;

  const renderSection = (title: string, items: Notification[]) =>
    items.length > 0 ? (
      <div key={title} style={{ marginBottom: 24 }}>
        <Title level={5} style={{ marginBottom: 8 }}>{title}</Title>
        <Card className="content-card" styles={{ body: { padding: 0 } }}>
          {items.map((item) => (
            <NotificationItem key={item.id} item={item} onRead={markRead} onStar={toggleStar} />
          ))}
        </Card>
      </div>
    ) : null;

  if (loading) return <div style={{ textAlign: 'center', padding: 48 }}><Spin size="large" /></div>;

  return (
    <div>
      <div className="page-header">
        <div>
          <Title level={4} style={{ margin: 0 }}><InboxOutlined /> Smart Inbox</Title>
          <Text type="secondary">All notifications in one place — leave, meetings, payroll, tasks, approvals</Text>
        </div>
        <Button onClick={markAllRead}>Mark all read</Button>
      </div>

      <Space wrap style={{ marginBottom: 16 }}>
        <Tag color="blue">Unread: {inbox?.unreadCount ?? 0}</Tag>
        <Tag color="gold">Starred: {inbox?.starredCount ?? 0}</Tag>
        {inbox?.categories.map(c => (
          <Tag key={c.category} color={categoryColors[c.category]}>{c.category}: {c.count}</Tag>
        ))}
      </Space>

      <Tabs
        activeKey={filter}
        onChange={setFilter}
        items={[
          { key: 'all', label: 'All' },
          { key: 'unread', label: `Unread (${inbox?.unreadCount ?? 0})` },
          { key: 'starred', label: 'Starred' },
          {
            key: 'grouped',
            label: 'By Day',
            children: filtered.length === 0 ? (
              <Empty description="No notifications" />
            ) : (
              <>
                {renderSection('Today', inbox?.today ?? [])}
                {renderSection('Yesterday', inbox?.yesterday ?? [])}
                {renderSection('Earlier', inbox?.earlier ?? [])}
              </>
            ),
          },
        ]}
      />

      {filter !== 'grouped' && (
        <Card className="content-card" styles={{ body: { padding: 0 } }}>
          {filtered.length === 0 ? (
            <Empty description="No notifications" style={{ padding: 48 }} />
          ) : (
            filtered.map((item) => (
              <NotificationItem key={item.id} item={item} onRead={markRead} onStar={toggleStar} />
            ))
          )}
        </Card>
      )}
    </div>
  );
}
