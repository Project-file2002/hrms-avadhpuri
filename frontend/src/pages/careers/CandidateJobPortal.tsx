import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Layout, Typography, Input, Button, Row, Col, Card, Tag, Space, Select,
  Drawer, Modal, Form, Progress, Divider, Collapse, FloatButton,
  Empty, Spin, App as AntApp, Tooltip, Steps, Tabs, Alert, Upload,
} from 'antd';
import {
  SearchOutlined, RocketOutlined, EnvironmentOutlined, MoneyCollectOutlined,
  ClockCircleOutlined, StarOutlined, StarFilled, SendOutlined, RobotOutlined,
  ThunderboltOutlined, FileTextOutlined, CheckCircleOutlined, BulbOutlined,
  HeartOutlined, QuestionCircleOutlined, ExperimentOutlined,
  CloseOutlined, MenuOutlined, FilterOutlined, UserOutlined, LogoutOutlined,
  ProfileOutlined, FormOutlined, UploadOutlined,
} from '@ant-design/icons';
import careerApi from '../../services/careerApi';
import { useCareerAuthStore } from '../../store/careerAuthStore';
import { useBreakpoint } from '../../hooks/useBreakpoint';
import type { CareerJob, CareerMatch, CareerExplain, CareerResumeReview, CareerApplication } from './careerConstants';
import {
  COMPANY_BENEFITS, RECRUITMENT_STEPS, FAQ_ITEMS, PHASE5_FEATURES, SEARCH_SUGGESTIONS,
} from './careerConstants';

const { Header, Content, Footer } = Layout;
const { Title, Text, Paragraph } = Typography;

const SAVED_KEY = 'ewxp_saved_jobs';
const PROFILE_KEY = 'ewxp_career_profile';

type Section = 'home' | 'jobs' | 'tracker' | 'culture' | 'faq' | 'roadmap';

const NAV_ITEMS: { key: Section; label: string; auth?: boolean }[] = [
  { key: 'home', label: 'Home' },
  { key: 'jobs', label: 'Jobs' },
  { key: 'tracker', label: 'My Applications', auth: true },
  { key: 'culture', label: 'Culture' },
  { key: 'faq', label: 'FAQ' },
  { key: 'roadmap', label: 'Phase 5' },
];

const WORKPLACE_OPTIONS = ['Remote', 'Hybrid', 'Office'];

interface ChatMsg { role: 'user' | 'bot'; text: string; jobIds?: number[] }

function matchColor(p: number) {
  if (p >= 85) return '#52c41a';
  if (p >= 70) return '#1890ff';
  return '#faad14';
}

export default function CandidateJobPortal() {
  const { message } = AntApp.useApp();
  const { isMobileNav } = useBreakpoint();
  const navigate = useNavigate();
  const { user: careerUser, isAuthenticated, login, register, logout, loadSession } = useCareerAuthStore();
  const [config, setConfig] = useState({ companyName: 'EWXP Technologies', tagline: '' });
  const [jobs, setJobs] = useState<CareerJob[]>([]);
  const [loading, setLoading] = useState(true);
  const [section, setSection] = useState<Section>('home');
  const [search, setSearch] = useState('');
  const [profileText, setProfileText] = useState(() => localStorage.getItem(PROFILE_KEY) ?? 'React TypeScript Redux frontend developer 3 years experience');
  const [workplace, setWorkplace] = useState<string | undefined>();
  const [department, setDepartment] = useState<string | undefined>();
  const [filteredIds, setFilteredIds] = useState<number[] | null>(null);
  const [matches, setMatches] = useState<Record<number, CareerMatch>>({});
  const [saved, setSaved] = useState<number[]>(() => JSON.parse(localStorage.getItem(SAVED_KEY) ?? '[]'));
  const [selectedJob, setSelectedJob] = useState<CareerJob | null>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [applyOpen, setApplyOpen] = useState(false);
  const [explain, setExplain] = useState<CareerExplain | null>(null);
  const [resumeReview, setResumeReview] = useState<CareerResumeReview | null>(null);
  const [reviewOpen, setReviewOpen] = useState(false);
  const [applyDone, setApplyDone] = useState<{ match: number; probability: number } | null>(null);
  const [chatOpen, setChatOpen] = useState(false);
  const [chatInput, setChatInput] = useState('');
  const [chatMsgs, setChatMsgs] = useState<ChatMsg[]>([{
    role: 'bot',
    text: 'Hi! I\'m your **AI Job Assistant**. Ask things like:\n• "Remote React jobs"\n• "Freshers ke liye openings"\n• "Salary 10 LPA se upar"',
  }]);
  const [chatLoading, setChatLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [matchesLoading, setMatchesLoading] = useState(false);
  const [jobType, setJobType] = useState<string | undefined>();
  const [experience, setExperience] = useState<string | undefined>();
  const [location, setLocation] = useState<string | undefined>();
  const [savedOnly, setSavedOnly] = useState(false);
  const [navOpen, setNavOpen] = useState(false);
  const [filtersOpen, setFiltersOpen] = useState(false);
  const [authOpen, setAuthOpen] = useState(false);
  const [authTab, setAuthTab] = useState<'login' | 'register'>('login');
  const [applications, setApplications] = useState<CareerApplication[]>([]);
  const [appsLoading, setAppsLoading] = useState(false);
  const [pendingApply, setPendingApply] = useState(false);
  const [resumeFile, setResumeFile] = useState<{ name: string; text: string } | null>(null);
  const [profileData, setProfileData] = useState<{ phone?: string; resumeText?: string; resumeFileName?: string } | null>(null);
  const [form] = Form.useForm();
  const [authForm] = Form.useForm();

  useEffect(() => {
    loadSession();
    const onAuthChange = () => loadSession();
    window.addEventListener('career-auth-changed', onAuthChange);
    return () => window.removeEventListener('career-auth-changed', onAuthChange);
  }, [loadSession]);

  useEffect(() => {
    Promise.all([
      careerApi.get('/config'),
      careerApi.get('/jobs'),
    ]).then(([cfg, j]) => {
      setConfig(cfg.data);
      setJobs(j.data);
    }).finally(() => setLoading(false));
  }, []);

  const fetchApplications = useCallback(async () => {
    if (!isAuthenticated) return;
    setAppsLoading(true);
    try {
      const res = await careerApi.get('/applications');
      setApplications(res.data);
    } catch {
      setApplications([]);
    } finally {
      setAppsLoading(false);
    }
  }, [isAuthenticated]);

  useEffect(() => {
    if (isAuthenticated) fetchApplications();
    else setApplications([]);
  }, [isAuthenticated, fetchApplications]);

  const computeMatches = useCallback(async (jobList: CareerJob[], text: string) => {
    if (!text.trim()) return;
    setMatchesLoading(true);
    const results: Record<number, CareerMatch> = {};
    try {
      await Promise.all(jobList.map(async (job) => {
        try {
          const res = await careerApi.post('/match', { jobId: job.id, resumeText: text, skillsText: text });
          results[job.id] = res.data;
        } catch { /* */ }
      }));
      setMatches(results);
    } finally {
      setMatchesLoading(false);
    }
  }, []);

  useEffect(() => {
    if (!jobs.length || !profileText.trim()) return;
    const timer = setTimeout(() => {
      localStorage.setItem(PROFILE_KEY, profileText);
      computeMatches(jobs, profileText);
    }, 700);
    return () => clearTimeout(timer);
  }, [jobs, profileText, computeMatches]);

  const departments = useMemo(() => [...new Set(jobs.map(j => j.department))].sort(), [jobs]);
  const jobTypes = useMemo(() => [...new Set(jobs.map(j => j.jobType))].sort(), [jobs]);
  const experienceLevels = useMemo(() => [...new Set(jobs.map(j => j.experience))].sort(), [jobs]);
  const locations = useMemo(() => [...new Set(jobs.map(j => j.location))].sort(), [jobs]);

  const hasActiveFilters = Boolean(
    search.trim() || workplace || department || jobType || experience || location || filteredIds || savedOnly,
  );

  useEffect(() => {
    setFiltersOpen(!isMobileNav);
  }, [isMobileNav]);

  const visibleJobs = useMemo(() => {
    let list = [...jobs];
    if (filteredIds) list = list.filter(j => filteredIds.includes(j.id));
    if (workplace) list = list.filter(j => j.workplace === workplace || (workplace === 'Remote' && j.isRemote));
    if (department) list = list.filter(j => j.department === department);
    if (jobType) list = list.filter(j => j.jobType === jobType);
    if (experience) list = list.filter(j => j.experience === experience);
    if (location) list = list.filter(j => j.location === location);
    if (savedOnly) list = list.filter(j => saved.includes(j.id));
    if (search.trim()) {
      const q = search.toLowerCase();
      list = list.filter(j =>
        j.title.toLowerCase().includes(q) ||
        j.skills.some(s => s.toLowerCase().includes(q)) ||
        j.department.toLowerCase().includes(q));
    }
    return list.sort((a, b) => a.daysOpen - b.daysOpen);
  }, [jobs, filteredIds, workplace, department, jobType, experience, location, savedOnly, saved, search]);

  const featuredJobs = useMemo(() => {
    const featured = jobs.filter(j => j.featured);
    return (featured.length ? featured : jobs).slice(0, 4);
  }, [jobs]);

  const latestJobs = useMemo(() => [...jobs].sort((a, b) => a.daysOpen - b.daysOpen).slice(0, 6), [jobs]);

  const clearFilters = () => {
    setSearch('');
    setWorkplace(undefined);
    setDepartment(undefined);
    setJobType(undefined);
    setExperience(undefined);
    setLocation(undefined);
    setFilteredIds(null);
    setSavedOnly(false);
  };

  const goToSection = (next: Section) => {
    if (next === 'tracker' && !isAuthenticated) {
      setAuthTab('login');
      setAuthOpen(true);
      message.info('Sign in to track your applications');
      return;
    }
    setSection(next);
    setNavOpen(false);
  };

  const handleAuth = async (values: Record<string, string>) => {
    try {
      if (authTab === 'login') {
        await login(values.email, values.password);
        message.success('Welcome back!');
      } else {
        await register({
          firstName: values.firstName,
          lastName: values.lastName,
          email: values.email,
          password: values.password,
          phone: values.phone,
        });
        message.success('Account created — your applications are now tracked here.');
      }
      setAuthOpen(false);
      authForm.resetFields();
      fetchApplications();
      if (pendingApply) {
        setPendingApply(false);
        openApplyModal();
      } else if (section === 'home') {
        setSection('tracker');
      }
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        ?? 'Authentication failed';
      message.error(msg);
    }
  };

  const handleSearch = async (q?: string) => {
    const query = q ?? search;
    if (!query.trim()) { setFilteredIds(null); return; }
    try {
      const res = await careerApi.post('/search', { query });
      setFilteredIds(res.data.jobIds);
      setSearch(query);
      message.info(res.data.interpretedAs);
      setSection('jobs');
    } catch {
      setSection('jobs');
    }
  };

  const toggleSave = (id: number) => {
    setSaved(prev => {
      const next = prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id];
      localStorage.setItem(SAVED_KEY, JSON.stringify(next));
      return next;
    });
  };

  const openJob = (job: CareerJob) => {
    setSelectedJob(job);
    setDrawerOpen(true);
    setExplain(null);
  };

  const loadExplain = async () => {
    if (!selectedJob) return;
    const res = await careerApi.post('/explain', { jobId: selectedJob.id, resumeText: profileText });
    setExplain(res.data);
  };

  const handleResumeUpload = async (file: File): Promise<false> => {
    try {
      const formData = new FormData();
      formData.append('file', file);
      const res = await careerApi.post('/extract-text', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      const text = res.data.text;
      if (!text || text.length < 50) {
        message.warning('Could not extract enough text from file. Try a clearer PDF.');
        return false;
      }
      setResumeFile({ name: file.name, text });
      message.success(`Extracted ${text.length} characters from ${file.name}`);
    } catch {
      message.error('Failed to process resume file');
    }
    return false;
  };

  const runResumeReview = async (text: string) => {
    const res = await careerApi.post('/resume-review', {
      jobId: selectedJob?.id,
      resumeText: text,
    });
    setResumeReview(res.data);
    setReviewOpen(true);
  };

  const handleApply = async (values: Record<string, string>) => {
    if (!selectedJob) return;
    const resumeText = resumeFile?.text || profileData?.resumeText;
    if (!resumeText || resumeText.length < 50) {
      message.warning('Please upload a resume file or add one in your Profile');
      return;
    }
    setSubmitting(true);
    try {
      const res = await careerApi.post('/apply', {
        ...values,
        jobRequisitionId: selectedJob.id,
        resumeText: resumeText,
        skillsText: profileText,
      });
      setApplyDone({ match: res.data.matchScore, probability: res.data.shortlistProbability });
      fetchApplications();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { status?: number; data?: { message?: string } } };
      if (axiosErr.response?.status === 401) {
        setApplyOpen(false);
        setAuthTab('login');
        setPendingApply(true);
        setAuthOpen(true);
        message.info('Please sign in to apply');
      } else {
        const msg = axiosErr.response?.data?.message ?? 'Application failed';
        message.error(msg);
      }
    } finally {
      setSubmitting(false);
    }
  };

  const openApplyModal = () => {
    if (!isAuthenticated) {
      setAuthTab('login');
      setAuthOpen(true);
      return;
    }
    setApplyOpen(true);
    setApplyDone(null);
    setResumeFile(null);
    form.resetFields();
    if (careerUser) {
      form.setFieldsValue({
        firstName: careerUser.firstName,
        lastName: careerUser.lastName,
        email: careerUser.email,
      });
    }
    careerApi.get('/profile').then(res => {
      const data = res.data;
      setProfileData(data);
      if (data.phone) form.setFieldsValue({ phone: data.phone });
    }).catch(() => {});
  };

  const sendChat = async (text?: string) => {
    const msg = (text ?? chatInput).trim();
    if (!msg) return;
    setChatInput('');
    setChatMsgs(prev => [...prev, { role: 'user', text: msg }]);
    setChatLoading(true);
    try {
      const res = await careerApi.post('/assistant', { message: msg });
      setChatMsgs(prev => [...prev, { role: 'bot', text: res.data.reply, jobIds: res.data.jobIds }]);
      if (res.data.jobIds?.length) {
        setFilteredIds(res.data.jobIds);
        setSection('jobs');
      }
    } catch {
      setChatMsgs(prev => [...prev, { role: 'bot', text: 'Sorry, try again.' }]);
    } finally {
      setChatLoading(false);
    }
  };

  const renderJobCard = (job: CareerJob) => {
    const match = matches[job.id];
    const pct = match?.matchPercent ?? 0;
    const isSaved = saved.includes(job.id);

    return (
      <Col xs={24} md={12} key={job.id}>
        <Card className="career-job-card" hoverable onClick={() => openJob(job)}>
          <div style={{ display: 'flex', justifyContent: 'space-between', gap: 8, flexWrap: 'wrap' }}>
            <div style={{ flex: 1, minWidth: 200 }}>
              <Space wrap style={{ marginBottom: 8 }}>
                {job.featured && <Tag color="purple">Featured</Tag>}
                <Tag color="green">Open</Tag>
                {job.isRemote && <Tag color="blue">Remote</Tag>}
              </Space>
              <Title level={5} style={{ margin: '0 0 8px' }}>{job.title}</Title>
              <Space wrap size={[8, 4]} style={{ fontSize: 13, color: '#6b7280' }}>
                <span><EnvironmentOutlined /> {job.location}</span>
                <span>{job.workplace}</span>
                <span><MoneyCollectOutlined /> {job.salaryRange}</span>
                <span><ClockCircleOutlined /> {job.experience}</span>
              </Space>
              <div style={{ marginTop: 10 }}>
                {job.skills.slice(0, 4).map(s => <Tag key={s} style={{ fontSize: 11 }}>{s}</Tag>)}
              </div>
            </div>
            <div style={{ textAlign: 'center', minWidth: 72 }}>
              {!isAuthenticated ? (
                <Button type="link" size="small" style={{ padding: 0, height: 'auto' }} onClick={(e) => { e.stopPropagation(); setAuthTab('login'); setAuthOpen(true); }}>
                  <Text type="secondary" style={{ fontSize: 11 }}>Sign in</Text>
                </Button>
              ) : matchesLoading && !match ? (
                <Spin size="small" />
              ) : (
                <Progress type="circle" percent={pct} size={64} strokeColor={matchColor(pct)} format={p => `${p}%`} />
              )}
              <Text type="secondary" style={{ fontSize: 11, display: 'block' }}>AI Match</Text>
            </div>
          </div>
          <Divider style={{ margin: '12px 0' }} />
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 8 }} onClick={e => e.stopPropagation()}>
            <Text type="secondary" style={{ fontSize: 12 }}>{job.department} · Posted {job.daysOpen}d ago</Text>
            <Space>
              <Tooltip title={isSaved ? 'Unsave' : 'Save job'}>
                <Button type="text" icon={isSaved ? <StarFilled style={{ color: '#faad14' }} /> : <StarOutlined />} onClick={() => toggleSave(job.id)} />
              </Tooltip>
              <Button type="primary" size="small" icon={<ThunderboltOutlined />} onClick={() => { openJob(job); openApplyModal(); }}>
                Quick Apply
              </Button>
              <Button size="small" icon={<FormOutlined />} onClick={() => navigate(`/careers/apply/${job.id}`)}>
                Full Apply
              </Button>
            </Space>
          </div>
        </Card>
      </Col>
    );
  };

  if (loading) return <Spin size="large" style={{ display: 'block', marginTop: 120 }} />;

  return (
    <Layout className="career-portal">
      <Header className="career-portal-header">
        <div className="career-portal-header-inner page-container">
          <div className="career-portal-header-start">
            {isMobileNav && (
              <Button
                type="text"
                className="career-nav-toggle"
                icon={<MenuOutlined />}
                aria-label="Open navigation"
                onClick={() => setNavOpen(true)}
              />
            )}
            <div className="career-portal-brand" onClick={() => goToSection('home')}>
              <div className="career-portal-logo">E</div>
              <div className="career-portal-brand-text">
                <Text strong className="career-brand-name">{config.companyName}</Text>
                <Text className="career-brand-tagline">Careers Portal</Text>
              </div>
            </div>
          </div>

          {!isMobileNav && (
            <div className="career-portal-header-end">
              <nav className="career-portal-nav" aria-label="Career portal sections">
                {NAV_ITEMS.map(item => (
                  <Button
                    key={item.key}
                    type={section === item.key ? 'primary' : 'text'}
                    className={`career-nav-btn${section === item.key ? ' career-nav-btn-active' : ''}`}
                    onClick={() => goToSection(item.key)}
                  >
                    {item.label}
                  </Button>
                ))}
              </nav>
              <Space className="career-portal-auth" wrap>
                {isAuthenticated ? (
                  <>
                    <Text className="career-user-name">{careerUser?.firstName}</Text>
                    <Button size="small" icon={<ProfileOutlined />} onClick={() => navigate('/careers/profile')}>
                      Profile
                    </Button>
                    <Button size="small" icon={<ProfileOutlined />} onClick={() => navigate('/careers/dashboard')}>
                      Dashboard
                    </Button>
                    <Button size="small" icon={<ProfileOutlined />} onClick={() => goToSection('tracker')}>
                      My Applications{applications.length ? ` (${applications.length})` : ''}
                    </Button>
                    <Button size="small" icon={<LogoutOutlined />} onClick={logout}>Logout</Button>
                  </>
                ) : (
                  <>
                    <Button size="small" onClick={() => { setAuthTab('login'); setAuthOpen(true); }}>Sign In</Button>
                    <Button size="small" type="primary" onClick={() => { setAuthTab('register'); setAuthOpen(true); }}>Register</Button>
                  </>
                )}
              </Space>
            </div>
          )}
        </div>
      </Header>

      <Drawer
        title={config.companyName}
        placement="left"
        open={navOpen}
        onClose={() => setNavOpen(false)}
        className="career-mobile-drawer"
        styles={{ wrapper: { width: Math.min(280, typeof window !== 'undefined' ? window.innerWidth * 0.85 : 280) } }}
      >
        <div className="career-mobile-nav">
          {NAV_ITEMS.map(item => (
            <Button
              key={item.key}
              block
              type={section === item.key ? 'primary' : 'text'}
              className="career-mobile-nav-btn"
              onClick={() => goToSection(item.key)}
            >
              {item.label}
            </Button>
          ))}
          <Divider style={{ margin: '12px 0' }} />
          {isAuthenticated ? (
            <Button block icon={<LogoutOutlined />} onClick={() => { logout(); setNavOpen(false); }}>Logout</Button>
          ) : (
            <>
              <Button block type="primary" onClick={() => { setAuthTab('login'); setAuthOpen(true); setNavOpen(false); }}>Sign In</Button>
              <Button block style={{ marginTop: 8 }} onClick={() => { setAuthTab('register'); setAuthOpen(true); setNavOpen(false); }}>Register</Button>
            </>
          )}
        </div>
      </Drawer>

      <Content className="career-portal-content">
        {(section === 'home' || section === 'jobs') && (
          <div className="career-hero">
            <div className="career-hero-inner page-container">
              <Title level={isMobileNav ? 2 : 1} className="career-hero-title">
                Find Your Dream Career
              </Title>
              <Text className="career-hero-subtitle">
                {config.tagline || 'AI-powered job matching — easier than LinkedIn, built for our team.'}
              </Text>

              <div className="career-search-box">
                <Input.Search
                  size="large"
                  placeholder='Try "Remote React Pune" or "Mujhe DevOps job chahiye"'
                  value={search}
                  onChange={e => setSearch(e.target.value)}
                  onSearch={handleSearch}
                  enterButton={<span className="career-search-btn"><SearchOutlined /> AI Search</span>}
                />
                <div className="career-suggestion-row">
                  {SEARCH_SUGGESTIONS.slice(0, 4).map(s => (
                    <Tag key={s} className="career-suggestion-tag" onClick={() => handleSearch(s)}>{s}</Tag>
                  ))}
                </div>
              </div>

              <div className="career-filter-panel">
                <div className="career-filter-panel-head">
                  <div>
                    <Text strong className="career-filter-title">
                      <FilterOutlined /> Refine your search
                    </Text>
                    <Text className="career-filter-subtitle">Filter roles published by our HR team</Text>
                  </div>
                  <Space wrap size={8}>
                    {isMobileNav && (
                      <Button
                        size="small"
                        type="default"
                        className="career-filter-toggle"
                        onClick={() => setFiltersOpen(v => !v)}
                      >
                        {filtersOpen ? 'Hide filters' : 'Show filters'}
                      </Button>
                    )}
                    {hasActiveFilters && (
                      <Button size="small" type="link" className="career-clear-filters" onClick={clearFilters}>
                        Clear all
                      </Button>
                    )}
                  </Space>
                </div>

                {filtersOpen && (
                  <Row gutter={[12, 12]} className="career-hero-filters">
                    <Col xs={24} lg={12}>
                      <label className="career-filter-label">Skills / resume summary</label>
                      <Input
                        className="career-hero-input"
                        placeholder="For AI match % — e.g. React, TypeScript, 3 years"
                        value={profileText}
                        onChange={e => setProfileText(e.target.value)}
                        prefix={<BulbOutlined />}
                      />
                    </Col>
                    <Col xs={12} sm={8} lg={4}>
                      <label className="career-filter-label">Workplace</label>
                      <Select
                        allowClear
                        placeholder="Any"
                        className="career-hero-select"
                        style={{ width: '100%' }}
                        value={workplace}
                        onChange={setWorkplace}
                        options={WORKPLACE_OPTIONS.map(v => ({ value: v, label: v }))}
                      />
                    </Col>
                    <Col xs={12} sm={8} lg={4}>
                      <label className="career-filter-label">Department</label>
                      <Select
                        allowClear
                        placeholder="Any"
                        className="career-hero-select"
                        style={{ width: '100%' }}
                        value={department}
                        onChange={setDepartment}
                        options={departments.map(d => ({ value: d, label: d }))}
                      />
                    </Col>
                    <Col xs={12} sm={8} lg={4}>
                      <label className="career-filter-label">Job type</label>
                      <Select
                        allowClear
                        placeholder="Any"
                        className="career-hero-select"
                        style={{ width: '100%' }}
                        value={jobType}
                        onChange={setJobType}
                        options={jobTypes.map(t => ({ value: t, label: t }))}
                      />
                    </Col>
                    <Col xs={12} sm={8} lg={4}>
                      <label className="career-filter-label">Experience</label>
                      <Select
                        allowClear
                        placeholder="Any"
                        className="career-hero-select"
                        style={{ width: '100%' }}
                        value={experience}
                        onChange={setExperience}
                        options={experienceLevels.map(v => ({ value: v, label: v }))}
                      />
                    </Col>
                    <Col xs={12} sm={8} lg={4}>
                      <label className="career-filter-label">Location</label>
                      <Select
                        allowClear
                        placeholder="Any"
                        className="career-hero-select"
                        style={{ width: '100%' }}
                        value={location}
                        onChange={setLocation}
                        options={locations.map(v => ({ value: v, label: v }))}
                      />
                    </Col>
                  </Row>
                )}

                <div className="career-hero-stats">
                  <Tag className="career-stat-tag">{jobs.length} open roles</Tag>
                  <Tag className="career-stat-tag">{jobs.filter(j => j.isRemote).length} remote/hybrid</Tag>
                  <Tag className="career-stat-tag">{departments.length} departments</Tag>
                  {hasActiveFilters && (
                    <Tag className="career-stat-tag career-stat-tag-active">{visibleJobs.length} matching</Tag>
                  )}
                  {matchesLoading && (
                    <Tag className="career-stat-tag"><Spin size="small" /> Updating AI match…</Tag>
                  )}
                </div>
              </div>
            </div>
          </div>
        )}

        {section === 'home' && (
          <div className="career-section">
            <div className="career-section-header">
              <Title level={3} style={{ margin: 0 }}><RocketOutlined /> Featured Openings</Title>
              <Button type="link" onClick={() => setSection('jobs')}>View all {jobs.length} jobs →</Button>
            </div>
            <Row gutter={[16, 16]}>{featuredJobs.map(renderJobCard)}</Row>

            <Divider />
            <div className="career-section-header">
              <Title level={4} style={{ margin: 0 }}>Latest from HR</Title>
              <Text type="secondary">Posted by our Talent team at {config.companyName}</Text>
            </div>
            <Row gutter={[16, 16]}>{latestJobs.map(renderJobCard)}</Row>

            <Divider />
            <Row gutter={16}>
              <Col xs={24} md={8}><Card className="career-feature-card"><StatisticLike icon="🧠" title="AI Job Match" desc="Semantic resume vs JD scoring with skill breakdown" /></Card></Col>
              <Col xs={24} md={8}><Card className="career-feature-card"><StatisticLike icon="💬" title="Natural Language Search" desc="Search like ChatGPT — Hinglish supported" /></Card></Col>
              <Col xs={24} md={8}><Card className="career-feature-card"><StatisticLike icon="⚡" title="One-Click Apply" desc="Minimal form + AI auto-screening in seconds" /></Card></Col>
            </Row>
          </div>
        )}

        {section === 'jobs' && (
          <div className="career-section">
            <div className="career-section-header">
              <div>
                <Title level={3} style={{ margin: 0 }}>{visibleJobs.length} Open Positions</Title>
                <Text type="secondary">All roles published by HR at {config.companyName}</Text>
              </div>
              <Space wrap>
                {saved.length > 0 && (
                  <Button type={savedOnly ? 'primary' : 'default'} icon={<StarFilled />} onClick={() => setSavedOnly(v => !v)}>
                    Saved ({saved.length})
                  </Button>
                )}
                <Button onClick={clearFilters}>Clear filters</Button>
              </Space>
            </div>
            {visibleJobs.length === 0 ? (
              <Empty description="No jobs match your filters">
                <Button type="primary" onClick={clearFilters}>Reset filters</Button>
              </Empty>
            ) : (
              <Row gutter={[16, 16]}>{visibleJobs.map(renderJobCard)}</Row>
            )}
          </div>
        )}

        {section === 'tracker' && (
          <div className="career-section">
            <div className="career-section-header">
              <div>
                <Title level={3} style={{ margin: 0 }}><ProfileOutlined /> My Applications</Title>
                <Text type="secondary">Track every role you applied for — status updates sync with HR recruitment pipeline</Text>
              </div>
              <Button onClick={fetchApplications} loading={appsLoading}>Refresh</Button>
            </div>

            {!isAuthenticated ? (
              <Card className="career-tracker-empty">
                <Empty description="Sign in to view your application pipeline">
                  <Space>
                    <Button type="primary" onClick={() => { setAuthTab('login'); setAuthOpen(true); }}>Sign In</Button>
                    <Button onClick={() => { setAuthTab('register'); setAuthOpen(true); }}>Create Account</Button>
                  </Space>
                </Empty>
              </Card>
            ) : appsLoading ? (
              <Spin size="large" style={{ display: 'block', margin: '48px auto' }} />
            ) : applications.length === 0 ? (
              <Card className="career-tracker-empty">
                <Empty description="No applications yet">
                  <Button type="primary" onClick={() => goToSection('jobs')}>Browse Open Jobs</Button>
                </Empty>
              </Card>
            ) : (
              <div className="career-tracker-list">
                {applications.map(app => (
                  <Card key={app.id} className="career-tracker-card">
                    <div className="career-tracker-card-head">
                      <div>
                        <Title level={5} style={{ margin: 0 }}>{app.jobTitle}</Title>
                        <Space wrap size={[8, 4]} style={{ marginTop: 6 }}>
                          <Tag>{app.department}</Tag>
                          <Tag icon={<EnvironmentOutlined />}>{app.location}</Tag>
                          <Tag color={app.status === 'Rejected' ? 'red' : app.status === 'Hired' ? 'green' : 'blue'}>
                            {app.statusLabel}
                          </Tag>
                          {app.matchScore != null && <Tag color="purple">AI Match {app.matchScore}%</Tag>}
                        </Space>
                      </div>
                      <Text type="secondary" style={{ fontSize: 12 }}>
                        Applied {new Date(app.appliedAt).toLocaleDateString()}
                      </Text>
                    </div>
                    <Alert type="info" showIcon title={app.nextStepHint} style={{ margin: '12px 0' }} />
                    <Steps
                      size="small"
                      orientation={isMobileNav ? 'vertical' : 'horizontal'}
                      current={Math.max(0, app.pipeline.findIndex(s => s.current))}
                      status={app.status === 'Rejected' ? 'error' : 'process'}
                      items={app.pipeline.map(stage => ({
                        title: stage.title,
                        description: isMobileNav ? stage.description : undefined,
                        status: stage.completed ? 'finish' : stage.current ? 'process' : app.status === 'Rejected' ? 'error' : 'wait',
                      }))}
                    />
                  </Card>
                ))}
              </div>
            )}
          </div>
        )}

        {section === 'culture' && (
          <div className="career-section">
            <Title level={2}><HeartOutlined /> Life at {config.companyName}</Title>
            <Paragraph type="secondary">One company. One mission. Not a multi-tenant job board — this portal is exclusively for joining our team.</Paragraph>
            <Row gutter={[24, 24]}>
              <Col xs={24} md={14}>
                <Card title="Our Culture" style={{ borderRadius: 12 }}>
                  <Paragraph>We believe work should feel like a career companion, not paperwork. Our AI-first approach helps employees and candidates move faster — from application to onboarding.</Paragraph>
                  <Space wrap>{['Innovation', 'Transparency', 'Growth', 'Diversity', 'Work-Life Balance'].map(t => <Tag key={t} color="purple">{t}</Tag>)}</Space>
                </Card>
                <Card title="Recruitment Process" style={{ borderRadius: 12, marginTop: 16 }}>
                  {RECRUITMENT_STEPS.map((s, i) => (
                    <div key={s.title} style={{ display: 'flex', gap: 12, marginBottom: 16 }}>
                      <div className="career-step-num">{i + 1}</div>
                      <div><Text strong>{s.title}</Text><br /><Text type="secondary">{s.desc}</Text></div>
                    </div>
                  ))}
                </Card>
              </Col>
              <Col xs={24} md={10}>
                <Card title="Benefits" style={{ borderRadius: 12 }}>
                  {COMPANY_BENEFITS.map(b => <div key={b} style={{ marginBottom: 8 }}><CheckCircleOutlined style={{ color: '#52c41a', marginRight: 8 }} />{b}</div>)}
                </Card>
              </Col>
            </Row>
          </div>
        )}

        {section === 'faq' && (
          <div className="career-section" style={{ maxWidth: 720, margin: '0 auto' }}>
            <Title level={2}><QuestionCircleOutlined /> FAQs</Title>
            <Collapse items={FAQ_ITEMS.map((f, i) => ({ key: i, label: f.q, children: <Paragraph>{f.a}</Paragraph> }))} />
          </div>
        )}

        {section === 'roadmap' && (
          <div className="career-section">
            <Title level={2}><ExperimentOutlined /> Phase 5 — Coming Soon</Title>
            <Paragraph type="secondary">These advanced CX features are on our roadmap. The demo focuses on AI match, smart search, explain job, resume review, and quick apply.</Paragraph>
            <Row gutter={[16, 16]}>
              {PHASE5_FEATURES.map(f => (
                <Col xs={24} sm={12} md={8} lg={6} key={f.title}>
                  <Card size="small" style={{ borderRadius: 12, opacity: 0.92 }}>
                    <div style={{ fontSize: 28, marginBottom: 8 }}>{f.icon}</div>
                    <Text strong>{f.title}</Text>
                    <Paragraph type="secondary" style={{ fontSize: 13, marginBottom: 0 }}>{f.desc}</Paragraph>
                    <Tag style={{ marginTop: 8 }}>Phase 5</Tag>
                  </Card>
                </Col>
              ))}
            </Row>
          </div>
        )}
      </Content>

      <Footer className="career-portal-footer">
        <Text type="secondary">© {new Date().getFullYear()} {config.companyName} · EWXP Career Portal · Single-company HRMS demo</Text>
      </Footer>

      {/* Job Detail Drawer */}
      <Drawer
        title={selectedJob?.title}
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        size="large"
        extra={
          <Space>
            <Button icon={<BulbOutlined />} onClick={loadExplain}>Explain this Job</Button>
            <Button type="primary" icon={<ThunderboltOutlined />} onClick={openApplyModal}>Quick Apply</Button>
            <Button icon={<FormOutlined />} onClick={() => navigate(`/careers/apply/${selectedJob.id}`)}>Full Apply</Button>
          </Space>
        }
      >
        {selectedJob && (
          <>
            <Space wrap style={{ marginBottom: 16 }}>
              <Tag>{selectedJob.department}</Tag>
              <Tag icon={<EnvironmentOutlined />}>{selectedJob.location}</Tag>
              <Tag>{selectedJob.workplace}</Tag>
              <Tag icon={<MoneyCollectOutlined />}>{selectedJob.salaryRange}</Tag>
            </Space>
            {isAuthenticated && matches[selectedJob.id] && (
              <Card size="small" style={{ marginBottom: 16, background: '#f5f0ff' }}>
                <Text strong>AI Match: {matches[selectedJob.id].matchPercent}%</Text>
                <div style={{ marginTop: 8 }}>
                  {matches[selectedJob.id].skills.map(s => (
                    <Tag key={s.skill} color={s.matched ? 'success' : 'default'}>{s.skill} {s.matched ? '✔' : '—'}</Tag>
                  ))}
                </div>
              </Card>
            )}
            <Title level={5}>Overview</Title>
            <Paragraph>{selectedJob.description}</Paragraph>
            <Title level={5}>Requirements</Title>
            <Paragraph style={{ whiteSpace: 'pre-line' }}>{selectedJob.requirements}</Paragraph>
            {explain && (
              <Card title="AI Job Summary" style={{ marginTop: 16, borderColor: '#6c5ce7' }}>
                <Paragraph>{explain.summary.replace(/\*\*(.+?)\*\*/g, '$1')}</Paragraph>
                <Text>Profile match: <Text strong>{explain.profileMatchPercent}%</Text> · Interview difficulty: {explain.interviewDifficulty}</Text>
                {explain.missingSkills.length > 0 && (
                  <div style={{ marginTop: 8 }}>
                    <Text type="secondary">Missing: </Text>
                    {explain.missingSkills.map(s => <Tag key={s}>{s}</Tag>)}
                  </div>
                )}
              </Card>
            )}
          </>
        )}
      </Drawer>

      {/* Quick Apply Modal */}
      <Modal
        title={applyDone ? 'Application Submitted' : `Quick Apply — ${selectedJob?.title ?? ''}`}
        open={applyOpen}
        onCancel={() => setApplyOpen(false)}
        footer={applyDone ? <Button type="primary" onClick={() => setApplyOpen(false)}>Done</Button> : null}
        width={480}
      >
        {applyDone ? (
          <div style={{ textAlign: 'center', padding: 16 }}>
            <CheckCircleOutlined style={{ fontSize: 48, color: '#52c41a' }} />
            <Title level={4}>You're in the pipeline!</Title>
            <Paragraph>Status: <Tag color="blue">AI Screening Complete</Tag></Paragraph>
            <Paragraph>AI Match: <Text strong>{applyDone.match}%</Text></Paragraph>
            <Paragraph>Shortlisting probability: <Text strong style={{ color: '#6c5ce7' }}>{applyDone.probability}%</Text></Paragraph>
            <Text type="secondary">Expected HR response: 2-3 business days</Text>
          </div>
        ) : (
          <Form form={form} layout="vertical" onFinish={handleApply}>
            <Form.Item name="firstName" label="First Name" rules={[{ required: true }]}><Input /></Form.Item>
            <Form.Item name="lastName" label="Last Name" rules={[{ required: true }]}><Input /></Form.Item>
            <Form.Item name="email" label="Email" rules={[{ required: true, type: 'email' }]}><Input /></Form.Item>
            <Form.Item name="phone" label="Phone"><Input /></Form.Item>
            <Form.Item label="Upload Resume (PDF only)" required={!profileData?.resumeText}>
              <Upload
                accept=".pdf"
                maxCount={1}
                beforeUpload={handleResumeUpload as (file: File) => false}
                onRemove={() => { setResumeFile(null); }}
                fileList={resumeFile ? [{ uid: '-1', name: resumeFile.name, status: 'done' }] : []}
              >
                <Button icon={<UploadOutlined />}>{resumeFile ? 'Replace File' : 'Select Resume File'}</Button>
              </Upload>
              {!resumeFile && profileData?.resumeFileName && (
                <Text type="secondary" style={{ display: 'block', marginTop: 4 }}>
                  Using profile resume: {profileData.resumeFileName} (<a onClick={() => navigate('/careers/profile')}>edit</a>)
                </Text>
              )}
            </Form.Item>
            <Space>
              <Button onClick={() => runResumeReview(resumeFile?.text || profileData?.resumeText || profileText)} icon={<FileTextOutlined />} disabled={!resumeFile?.text && !profileData?.resumeText && !profileText}>
                AI Resume Review
              </Button>
              <Button type="primary" htmlType="submit" loading={submitting} icon={<SendOutlined />}>Submit — One Click</Button>
            </Space>
          </Form>
        )}
      </Modal>

      <Modal
        title={authTab === 'login' ? 'Candidate Sign In' : 'Create Candidate Account'}
        open={authOpen}
        onCancel={() => setAuthOpen(false)}
        footer={null}
        width={440}
        destroyOnHidden
      >
        <Tabs
          activeKey={authTab}
          onChange={k => setAuthTab(k as 'login' | 'register')}
          items={[
            { key: 'login', label: 'Sign In' },
            { key: 'register', label: 'Register' },
          ]}
        />
        <Form form={authForm} layout="vertical" onFinish={handleAuth}>
          {authTab === 'register' && (
            <>
              <Row gutter={12}>
                <Col span={12}>
                  <Form.Item name="firstName" label="First Name" rules={[{ required: true }]}><Input prefix={<UserOutlined />} /></Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item name="lastName" label="Last Name" rules={[{ required: true }]}><Input /></Form.Item>
                </Col>
              </Row>
              <Form.Item name="phone" label="Phone"><Input /></Form.Item>
            </>
          )}
          <Form.Item name="email" label="Email" rules={[{ required: true, type: 'email' }]}><Input /></Form.Item>
          <Form.Item name="password" label="Password" rules={[{ required: true, min: 6 }]}><Input.Password /></Form.Item>
          <Button type="primary" htmlType="submit" block>
            {authTab === 'login' ? 'Sign In' : 'Create Account'}
          </Button>
        </Form>
        <Paragraph type="secondary" style={{ marginTop: 12, marginBottom: 0, fontSize: 12 }}>
          Demo: candidate@demo.com / Demo@123
        </Paragraph>
      </Modal>

      {/* Resume Review Modal */}
      <Modal title="AI Resume Review" open={reviewOpen} onCancel={() => setReviewOpen(false)} footer={null} width={520}>
        {resumeReview && (
          <>
            <div style={{ textAlign: 'center', marginBottom: 16 }}>
              <Progress type="dashboard" percent={resumeReview.atsScore} strokeColor="#6c5ce7" />
              <Text type="secondary">ATS Score · Grammar: {resumeReview.grammar}</Text>
            </div>
            {resumeReview.missingKeywords.length > 0 && (
              <><Text strong>Missing keywords</Text><div style={{ margin: '8px 0' }}>{resumeReview.missingKeywords.map(k => <Tag key={k}>{k}</Tag>)}</div></>
            )}
            <Text strong>Suggestions</Text>
            <ul>{resumeReview.suggestions.map(s => <li key={s}><Text>{s}</Text></li>)}</ul>
            {resumeReview.aiAnalysis && (
              <Card size="small" title="AI Deep Analysis" style={{ marginTop: 12, maxHeight: 200, overflow: 'auto' }}>
                <Paragraph style={{ fontSize: 13, whiteSpace: 'pre-wrap' }}>{resumeReview.aiAnalysis}</Paragraph>
              </Card>
            )}
          </>
        )}
      </Modal>

      {/* Floating AI Assistant */}
      {chatOpen && (
        <Card className="career-chat-panel" title={<><RobotOutlined /> AI Job Assistant</>} extra={<Button type="text" icon={<CloseOutlined />} onClick={() => setChatOpen(false)} />}>
          <div className="career-chat-messages">
            {chatMsgs.map((m, i) => (
              <div key={i} className={`career-chat-msg career-chat-msg-${m.role}`}>
                <Paragraph style={{ margin: 0, whiteSpace: 'pre-wrap' }}>{m.text.replace(/\*\*(.+?)\*\*/g, '$1')}</Paragraph>
              </div>
            ))}
            {chatLoading && <Spin size="small" />}
          </div>
          <Input.Search placeholder="Remote React jobs? Freshers openings?" value={chatInput}
            onChange={e => setChatInput(e.target.value)} onSearch={sendChat} enterButton={<SendOutlined />} />
        </Card>
      )}
      <FloatButton icon={<RobotOutlined />} type="primary" style={{ right: 24, bottom: 24, background: '#6c5ce7' }}
        badge={{ dot: chatOpen }} onClick={() => setChatOpen(v => !v)} tooltip="AI Job Assistant" />
    </Layout>
  );
}

function StatisticLike({ icon, title, desc }: { icon: string; title: string; desc: string }) {
  return (
    <div style={{ textAlign: 'center', padding: 8 }}>
      <div className="career-feature-icon">{icon}</div>
      <Text strong style={{ display: 'block' }}>{title}</Text>
      <Text type="secondary" style={{ fontSize: 13 }}>{desc}</Text>
    </div>
  );
}
