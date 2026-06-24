import { useState, useRef, useEffect } from 'react';
import { Input, Button, Card, Avatar, Typography, Space, Tag, Spin } from 'antd';
import { SendOutlined, RobotOutlined, UserOutlined, BulbOutlined, LockOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';
import { useRoles } from '../utils/roles';
import { canAccessPath } from '../config/navigation';

const { Text, Paragraph } = Typography;

interface Message {
  role: 'user' | 'bot';
  content: string;
  actions?: { label: string; action: string }[];
  intent?: string;
  restricted?: boolean;
  persona?: string;
}

interface WelcomeConfig {
  persona: string;
  title: string;
  subtitle: string;
  welcome: string;
  suggestedPrompts: string[];
}

const personaColors: Record<string, string> = {
  admin: '#722ed1',
  hr: '#eb2f96',
  manager: '#1890ff',
  payroll: '#faad14',
  employee: '#52c41a',
};

const intentColors: Record<string, string> = {
  leave_balance: '#52c41a',
  my_attendance: '#1890ff',
  team_attendance: '#722ed1',
  team_members: '#13c2c2',
  my_info: '#fa8c16',
  employee_count: '#eb2f96',
  performance: '#2f54eb',
  payroll: '#faad14',
  policy: '#a0d911',
  help: '#595959',
};

function AICopilotPage() {
  const navigate = useNavigate();
  const { roles } = useRoles();
  const [welcome, setWelcome] = useState<WelcomeConfig | null>(null);
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState('');
  const [loading, setLoading] = useState(false);
  const [initLoading, setInitLoading] = useState(true);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    api.get('/copilot/welcome')
      .then((res) => {
        const data = res.data as WelcomeConfig;
        setWelcome(data);
        setMessages([{
          role: 'bot',
          content: data.welcome,
          intent: 'help',
          persona: data.persona,
        }]);
      })
      .catch(() => {
        setMessages([{
          role: 'bot',
          content: "Hi! I'm your AI HR Copilot. Ask me about leave, attendance, or your profile.",
          intent: 'help',
          persona: 'employee',
        }]);
      })
      .finally(() => setInitLoading(false));
  }, []);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const handleSend = async (message?: string) => {
    const msg = (message || input).trim();
    if (!msg || loading) return;

    setInput('');
    setMessages((prev) => [...prev, { role: 'user', content: msg }]);
    setLoading(true);

    try {
      const res = await api.post('/copilot/chat', { message: msg });
      const data = res.data;
      setMessages((prev) => [
        ...prev,
        {
          role: 'bot',
          content: data.reply,
          actions: data.actions,
          intent: data.intent,
          restricted: data.restricted,
          persona: data.persona,
        },
      ]);
    } catch {
      setMessages((prev) => [
        ...prev,
        {
          role: 'bot',
          content: 'Sorry, I encountered an error. Please try again.',
          intent: 'error',
        },
      ]);
    } finally {
      setLoading(false);
    }
  };

  const handleAction = (action: string) => {
    if (action === 'help') {
      handleSend('help');
      return;
    }
    if (action.startsWith('navigate:')) {
      const path = action.replace('navigate:', '');
      const basePath = path.startsWith('/employees/') ? '/employees' : path;
      if (canAccessPath(roles, basePath)) {
        navigate(path);
      }
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  const renderBoldText = (text: string, lineKey: number) => {
    const parts = text.split(/(\*\*[^*]+?\*\*)/g);
    return parts.map((part, j) => {
      const match = part.match(/^\*\*(.+?)\*\*$/);
      if (match) {
        return (
          <Text strong key={`${lineKey}-${j}`} style={{ display: 'inline' }}>
            {match[1]}
          </Text>
        );
      }
      return part ? <span key={`${lineKey}-${j}`}>{part}</span> : null;
    });
  };

  const renderMessage = (msg: string) => {
    return msg.split('\n').map((line, i) => {
      if (line.startsWith('•') || line.startsWith('·')) {
        return (
          <div key={i} style={{ paddingLeft: 8, marginBottom: 2 }}>
            {renderBoldText(line, i)}
          </div>
        );
      }
      if (!line.trim()) {
        return <div key={i} style={{ height: 4 }} />;
      }
      return (
        <Paragraph key={i} style={{ marginBottom: 4, whiteSpace: 'pre-wrap' }}>
          {renderBoldText(line, i)}
        </Paragraph>
      );
    });
  };

  const suggestedPrompts = welcome?.suggestedPrompts ?? [];
  const persona = welcome?.persona ?? 'employee';
  const headerColor = personaColors[persona] ?? '#6c5ce7';

  if (initLoading) {
    return <Spin size="large" style={{ display: 'block', marginTop: 80 }} />;
  }

  return (
    <div className="copilot-container">
      <div
        style={{
          padding: '16px 24px',
          background: `linear-gradient(135deg, ${headerColor} 0%, #a29bfe 100%)`,
          color: '#fff',
          display: 'flex',
          alignItems: 'center',
          gap: 12,
        }}
      >
        <Avatar icon={<RobotOutlined />} style={{ background: 'rgba(255,255,255,0.2)' }} />
        <div style={{ flex: 1 }}>
          <Space wrap>
            <Text strong style={{ color: '#fff', fontSize: 16 }}>
              {welcome?.title ?? 'AI HR Copilot'}
            </Text>
            <Tag style={{ background: 'rgba(255,255,255,0.2)', border: 'none', color: '#fff', fontSize: 11 }}>
              {persona.toUpperCase()} MODE
            </Tag>
          </Space>
          <br />
          <Text style={{ color: 'rgba(255,255,255,0.85)', fontSize: 12 }}>
            {welcome?.subtitle ?? 'Role-based HR assistant'}
          </Text>
        </div>
      </div>

      <div
        style={{
          flex: 1,
          overflowY: 'auto',
          padding: 24,
          display: 'flex',
          flexDirection: 'column',
          gap: 16,
        }}
      >
        {messages.map((msg, i) => (
          <div
            key={i}
            style={{
              display: 'flex',
              justifyContent: msg.role === 'user' ? 'flex-end' : 'flex-start',
            }}
          >
            <div
              className="copilot-message"
              style={{
                display: 'flex',
                gap: 8,
                flexDirection: msg.role === 'user' ? 'row-reverse' : 'row',
              }}
            >
              <Avatar
                icon={msg.role === 'user' ? <UserOutlined /> : <RobotOutlined />}
                style={{
                  background: msg.role === 'user' ? '#6c5ce7' : '#a29bfe',
                  flexShrink: 0,
                  marginTop: 4,
                }}
              />
              <div>
                <Card
                  size="small"
                  style={{
                    background: msg.role === 'user' ? '#6c5ce7' : msg.restricted ? '#fff7e6' : '#fff',
                    borderColor: msg.role === 'user' ? '#6c5ce7' : msg.restricted ? '#ffd591' : '#e8e8e8',
                    borderRadius: msg.role === 'user' ? '16px 16px 4px 16px' : '16px 16px 16px 4px',
                  }}
                  styles={{ body: { padding: '12px 16px' } }}
                >
                  <div
                    style={{
                      color: msg.role === 'user' ? '#fff' : '#262626',
                      fontSize: 14,
                      lineHeight: 1.6,
                    }}
                  >
                    {renderMessage(msg.content)}
                  </div>

                  {msg.role === 'bot' && (msg.intent || msg.restricted) && (
                    <div style={{ marginTop: 8, display: 'flex', gap: 6, flexWrap: 'wrap' }}>
                      {msg.intent && msg.intent !== 'help' && (
                        <Tag color={intentColors[msg.intent] || '#8c8c8c'} style={{ fontSize: 11 }}>
                          {msg.intent.replace(/_/g, ' ')}
                        </Tag>
                      )}
                      {msg.restricted && (
                        <Tag icon={<LockOutlined />} color="warning" style={{ fontSize: 11 }}>
                          access restricted
                        </Tag>
                      )}
                    </div>
                  )}
                </Card>

                {msg.actions && msg.actions.length > 0 && (
                  <div style={{ marginTop: 8, display: 'flex', gap: 6, flexWrap: 'wrap' }}>
                    {msg.actions.map((action, j) => (
                      <Button
                        key={j}
                        size="small"
                        type="default"
                        style={{
                          borderColor: '#6c5ce7',
                          color: '#6c5ce7',
                          fontSize: 12,
                          borderRadius: 12,
                        }}
                        onClick={() => handleAction(action.action)}
                      >
                        {action.label}
                      </Button>
                    ))}
                  </div>
                )}
              </div>
            </div>
          </div>
        ))}

        {loading && (
          <div style={{ display: 'flex', justifyContent: 'flex-start', gap: 8, alignItems: 'center' }}>
            <Avatar icon={<RobotOutlined />} style={{ background: '#a29bfe' }} />
            <Card
              size="small"
              style={{
                background: '#fff',
                borderRadius: '16px 16px 16px 4px',
                borderColor: '#e8e8e8',
              }}
              styles={{ body: { padding: '12px 16px' } }}
            >
              <Spin size="small" /> <Text style={{ marginLeft: 8, color: '#8c8c8c' }}>Thinking...</Text>
            </Card>
          </div>
        )}

        <div ref={messagesEndRef} />

        {messages.length === 1 && suggestedPrompts.length > 0 && (
          <div style={{ marginTop: 16 }}>
            <Text type="secondary" style={{ fontSize: 13, display: 'block', marginBottom: 8 }}>
              <BulbOutlined /> Try asking:
            </Text>
            <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
              {suggestedPrompts.map((prompt, i) => (
                <Button
                  key={i}
                  size="small"
                  style={{
                    borderRadius: 16,
                    borderColor: '#d9adff',
                    color: '#6c5ce7',
                    fontSize: 12,
                  }}
                  onClick={() => handleSend(prompt)}
                >
                  {prompt}
                </Button>
              ))}
            </div>
          </div>
        )}
      </div>

      <div
        style={{
          padding: '16px 24px',
          borderTop: '1px solid #e8e8e8',
          background: '#fff',
        }}
      >
        <Space.Compact style={{ width: '100%' }}>
          <Input
            placeholder={
              persona === 'employee'
                ? "Ask about your leave, attendance, salary..."
                : persona === 'manager'
                  ? "Ask about your team, leave, attendance..."
                  : persona === 'payroll'
                    ? "Ask about payroll, headcount, policies..."
                    : "Ask about workforce, recruitment, analytics..."
            }
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={handleKeyDown}
            size="large"
            variant="borderless"
            style={{
              background: '#f5f0ff',
              borderRadius: '24px 0 0 24px',
              paddingLeft: 16,
            }}
          />
          <Button
            type="primary"
            icon={<SendOutlined />}
            onClick={() => handleSend()}
            loading={loading}
            style={{
              borderRadius: '0 24px 24px 0',
              background: '#6c5ce7',
              borderColor: '#6c5ce7',
            }}
          />
        </Space.Compact>
      </div>
    </div>
  );
}

export default AICopilotPage;
