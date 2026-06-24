import { useState, useEffect } from 'react';
import { Card, Tree, Spin, Tag, Typography, Badge, Empty, Segmented, Avatar, Tooltip } from 'antd';
import {
  ApartmentOutlined,
  UserOutlined,
  CrownOutlined,
  TeamOutlined,
  RightOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';

const { Text, Title } = Typography;

interface OrgEmployee {
  id: number;
  name: string;
  position: string;
  isHead: boolean;
}

interface DepartmentNode {
  id: number;
  name: string;
  headName: string;
  employeeCount: number;
  employees: OrgEmployee[];
}

interface TreeDataItem {
  title: React.ReactNode;
  key: string;
  icon?: React.ReactNode;
  children?: TreeDataItem[];
}

const departmentColors = [
  '#6c5ce7', '#00b894', '#fdcb6e', '#e17055', '#0984e3',
  '#e84393', '#00cec9', '#fd79a8', '#636e72', '#d63031',
];

const viewOptions = [
  { label: 'Tree View', value: 'tree' },
  { label: 'Card View', value: 'card' },
];

function OrganizationChartPage() {
  const [departments, setDepartments] = useState<DepartmentNode[]>([]);
  const [loading, setLoading] = useState(true);
  const [view, setView] = useState<'tree' | 'card'>('card');
  const navigate = useNavigate();

  useEffect(() => {
    fetchOrgChart();
  }, []);

  const fetchOrgChart = async () => {
    try {
      const res = await api.get('/departments/org-chart');
      setDepartments(res.data);
    } catch {
    } finally {
      setLoading(false);
    }
  };

  const getDeptColor = (index: number) => departmentColors[index % departmentColors.length];

  const buildTreeData = (): TreeDataItem[] =>
    departments.map((dept, i) => ({
      key: `dept-${dept.id}`,
      icon: <ApartmentOutlined style={{ color: getDeptColor(i) }} />,
      title: (
        <div style={{ display: 'flex', alignItems: 'center', gap: 12, padding: '2px 0', flexWrap: 'wrap' }}>
          <Text strong style={{ fontSize: 14, color: getDeptColor(i) }}>{dept.name}</Text>
          <Badge count={dept.employeeCount} style={{ backgroundColor: getDeptColor(i), fontSize: 11 }} />
          {dept.headName && (
            <Tag color="purple" style={{ fontSize: 11, marginLeft: 4 }}>
              <CrownOutlined /> {dept.headName}
            </Tag>
          )}
        </div>
      ),
      children: dept.employees.map((emp) => ({
        key: `emp-${emp.id}`,
        icon: emp.isHead ? <CrownOutlined style={{ color: '#faad14' }} /> : <UserOutlined style={{ color: '#8c8c8c' }} />,
        title: (
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, padding: '2px 0', cursor: 'pointer' }}
            onClick={() => navigate(`/employees/${emp.id}`)}
          >
            <Text style={{ fontSize: 13, color: emp.isHead ? '#6c5ce7' : '#595959', fontWeight: emp.isHead ? 600 : 400 }}>
              {emp.name}
            </Text>
            {emp.position && <Text type="secondary" style={{ fontSize: 11, fontStyle: 'italic' }}>{emp.position}</Text>}
            {emp.isHead && <Tag color="gold" style={{ fontSize: 10, lineHeight: '16px', padding: '0 4px' }}>Head</Tag>}
          </div>
        ),
      })),
    }));

  const renderCardView = () => (
    <div style={{ display: 'flex', flexWrap: 'wrap', gap: 24, justifyContent: 'center' }}>
      {departments.map((dept, i) => {
        const color = getDeptColor(i);
        return (
          <Card
            key={dept.id}
            className="org-card-view"
            style={{
              width: 340,
              borderRadius: 16,
              borderTop: `4px solid ${color}`,
              boxShadow: '0 4px 20px rgba(0,0,0,0.06)',
              flexShrink: 0,
            }}
            styles={{ body: { padding: 20 } }}
          >
            <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 16 }}>
              <div style={{
                width: 40, height: 40, borderRadius: 12,
                background: `${color}15`, display: 'flex',
                alignItems: 'center', justifyContent: 'center',
                fontSize: 20, color,
              }}>
                <ApartmentOutlined />
              </div>
              <div style={{ flex: 1 }}>
                <Text strong style={{ fontSize: 15, color }}>{dept.name}</Text>
                <div>
                  <Text type="secondary" style={{ fontSize: 12 }}>{dept.employeeCount} employees</Text>
                </div>
              </div>
            </div>

            {dept.headName && (
              <div style={{
                display: 'flex', alignItems: 'center', gap: 10,
                padding: '10px 12px', background: `${color}08`,
                borderRadius: 10, marginBottom: 12,
              }}>
                <Avatar style={{ background: color, flexShrink: 0 }}>
                  <CrownOutlined />
                </Avatar>
                <div>
                  <Text strong style={{ fontSize: 13 }}>{dept.headName}</Text>
                  <div><Text type="secondary" style={{ fontSize: 11 }}>Department Head</Text></div>
                </div>
              </div>
            )}

            <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
              {dept.employees
                .filter(e => !e.isHead)
                .map(emp => (
                  <div key={emp.id} style={{
                    display: 'flex', alignItems: 'center', gap: 10,
                    padding: '8px 10px', borderRadius: 8, cursor: 'pointer',
                    transition: 'background 0.2s',
                  }}
                    className="org-employee-row"
                    onClick={() => navigate(`/employees/${emp.id}`)}
                  >
                    <Avatar style={{ background: '#e8e8e8', color: '#595959', flexShrink: 0 }} size={32}>
                      {emp.name.charAt(0)}
                    </Avatar>
                    <div style={{ flex: 1, minWidth: 0 }}>
                      <Text style={{ fontSize: 13, display: 'block' }} ellipsis>{emp.name}</Text>
                      {emp.position && (
                        <Text type="secondary" style={{ fontSize: 11, display: 'block' }} ellipsis>{emp.position}</Text>
                      )}
                    </div>
                    <RightOutlined style={{ color: '#d9d9d9', fontSize: 11 }} />
                  </div>
                ))}
            </div>

            {dept.employees.filter(e => !e.isHead).length === 0 && (
              <Text type="secondary" style={{ fontSize: 12, display: 'block', textAlign: 'center', padding: 8 }}>
                No team members
              </Text>
            )}

            <Tooltip title="View department details">
              <div style={{ marginTop: 12, textAlign: 'center' }}>
                <Tag color={color} style={{ cursor: 'pointer', padding: '2px 16px', borderRadius: 20 }}
                  onClick={() => navigate('/departments')}>
                  <TeamOutlined /> View Department
                </Tag>
              </div>
            </Tooltip>
          </Card>
        );
      })}
    </div>
  );

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: 80 }}>
        <Spin size="large" />
      </div>
    );
  }

  if (departments.length === 0) {
    return <Empty description="No departments found" />;
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 12, marginBottom: 20 }}>
        <div>
          <Title level={4} style={{ margin: 0 }}>
            <ApartmentOutlined style={{ marginRight: 8, color: '#6c5ce7' }} />
            Organization Chart
          </Title>
          <Text type="secondary">
            {departments.length} departments, {departments.reduce((s, d) => s + d.employeeCount, 0)} employees
          </Text>
        </div>
        <Segmented options={viewOptions} value={view} onChange={(v) => setView(v as 'tree' | 'card')} />
      </div>

      {view === 'tree' ? (
        <>
          <div style={{ padding: '10px 16px', background: '#f0f5ff', borderRadius: 8, marginBottom: 16, fontSize: 13, color: '#6c5ce7' }}>
            Click on any employee to view their profile
          </div>
          <Card style={{ borderRadius: 12, boxShadow: '0 2px 8px rgba(0,0,0,0.06)' }} className="org-tree-wrap">
            <Tree
              showIcon
              defaultExpandAll
              treeData={buildTreeData()}
              style={{ background: 'transparent', fontSize: 13 }}
              switcherIcon={<TeamOutlined style={{ fontSize: 11, color: '#bfbfbf' }} />}
            />
          </Card>
        </>
      ) : (
        renderCardView()
      )}
    </div>
  );
}

export default OrganizationChartPage;
