export interface CareerJob {
  id: number;
  title: string;
  description: string | null;
  requirements: string | null;
  department: string;
  location: string;
  workplace: string;
  jobType: string;
  experience: string;
  salaryRange: string;
  skills: string[];
  featured: boolean;
  isRemote: boolean;
  daysOpen: number;
  postedAt: string;
}

export interface CareerMatch {
  matchPercent: number;
  skills: { skill: string; matched: boolean }[];
  missingSkills: string[];
  strongAreas: string[];
  summary: string;
}

export interface CareerExplain {
  summary: string;
  profileMatchPercent: number;
  missingSkills: string[];
  recommendedLearning: string[];
  interviewDifficulty: string;
}

export interface CareerResumeReview {
  atsScore: number;
  grammar: string;
  missingKeywords: string[];
  suggestions: string[];
  aiAnalysis?: string;
}

export interface CareerApplicationStage {
  title: string;
  description: string;
  completed: boolean;
  current: boolean;
}

export interface CareerApplication {
  id: number;
  jobId: number;
  jobTitle: string;
  department: string;
  location: string;
  status: string;
  statusLabel: string;
  matchScore?: number;
  appliedAt: string;
  updatedAt?: string;
  nextStepHint?: string;
  pipeline: CareerApplicationStage[];
}

export const COMPANY_BENEFITS = [
  'Health insurance for you & family',
  'Flexible hybrid & remote options',
  'Learning budget & certifications',
  'AI-powered career development',
  'Performance bonuses',
  'Inclusive culture & ERG groups',
];

export const RECRUITMENT_STEPS = [
  { title: 'Quick Apply', desc: 'Resume upload + AI auto-fill — under 2 minutes' },
  { title: 'AI Screening', desc: 'Semantic match score & shortlist probability' },
  { title: 'HR Review', desc: 'Recruiter validates fit within 2-3 days' },
  { title: 'Interviews', desc: 'Technical + manager + culture rounds' },
  { title: 'Offer & Onboarding', desc: 'Digital offer letter & onboarding checklist' },
];

export const FAQ_ITEMS = [
  { q: 'Is this portal for multiple companies?', a: 'No — this is EWXP\'s dedicated career portal for our single organization. One company, one culture, one team.' },
  { q: 'How does AI Job Match work?', a: 'We compare your resume/skills semantically against the job description — not just keywords. You get a match % with skill breakdown.' },
  { q: 'How long until I hear back?', a: 'AI screening is instant. HR review typically takes 2-3 business days for shortlisted profiles.' },
  { q: 'Can I apply without creating an account?', a: 'Yes — Quick Apply needs only basic details and your resume. No lengthy forms.' },
];

export const PHASE5_FEATURES = [
  { icon: '🎤', title: 'AI Mock Interview', desc: 'Voice + coding + behavioral practice' },
  { icon: '⚖️', title: 'Compare Jobs', desc: 'Side-by-side salary, skills & culture' },
  { icon: '📊', title: 'Application Tracker', desc: 'Live pipeline with AI status updates' },
  { icon: '🔗', title: 'LinkedIn / GitHub Import', desc: 'One-click profile auto-fill' },
  { icon: '🔔', title: 'Job Alerts', desc: 'Email, SMS, WhatsApp notifications' },
  { icon: '🌍', title: 'Multi-language Portal', desc: 'Hindi, Tamil, and more' },
  { icon: '📝', title: 'AI Resume Builder', desc: 'Generate ATS-optimized resumes' },
  { icon: '📈', title: 'Salary Benchmark AI', desc: 'Market salary predictions by role' },
  { icon: '🗺️', title: 'AI Career Roadmap', desc: 'Personalized growth path to lead/architect' },
  { icon: '👥', title: 'Employee Referral Hub', desc: 'Track referrals & bonuses' },
];

export const SEARCH_SUGGESTIONS = [
  'React Developer Pune',
  'Remote frontend jobs',
  'DevOps engineer',
  'Backend .NET',
  'Sales analyst',
  'Mujhe remote react job chahiye',
];
