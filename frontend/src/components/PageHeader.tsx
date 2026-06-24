import type { ReactNode } from 'react';
import { Typography } from 'antd';

const { Title, Text } = Typography;

interface PageHeaderProps {
  title: ReactNode;
  subtitle?: ReactNode;
  extra?: ReactNode;
}

export default function PageHeader({ title, subtitle, extra }: PageHeaderProps) {
  return (
    <div className="page-toolbar">
      <div className="page-toolbar-text">
        <Title level={4} className="page-toolbar-title" style={{ margin: 0 }}>
          {title}
        </Title>
        {subtitle && (
          <Text type="secondary" className="page-toolbar-subtitle">
            {subtitle}
          </Text>
        )}
      </div>
      {extra && <div className="page-toolbar-actions">{extra}</div>}
    </div>
  );
}
