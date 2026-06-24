import { useState, useEffect, useCallback, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import dayjs from 'dayjs';
import {
  Layout, Card, Steps, Form, Input, Select, Button, DatePicker, InputNumber,
  Checkbox, Radio, Upload, Tag, Progress, Typography, Divider, Space,
  Row, Col, App as AntApp, Spin, Result, Popconfirm, Tooltip, Empty, Modal, Alert
} from 'antd';
import {
  ArrowLeftOutlined, ArrowRightOutlined, SaveOutlined,
  CheckCircleOutlined, ClockCircleOutlined, InboxOutlined,
  PlusOutlined, DeleteOutlined, FileTextOutlined,
  UserOutlined, BookOutlined, ExperimentOutlined, GlobalOutlined,
  TrophyOutlined, SafetyOutlined, TeamOutlined, SettingOutlined,
  StarOutlined, FormOutlined, AuditOutlined, AimOutlined, ThunderboltOutlined,
  ContactsOutlined, LinkOutlined, FileProtectOutlined, CheckSquareOutlined
} from '@ant-design/icons';
import careerApi from '../../services/careerApi';
import { useCareerAuthStore } from '../../store/careerAuthStore';

const { Title, Text } = Typography;
const { TextArea } = Input;
const { Dragger } = Upload;

// ---------- Types ----------

interface ProfessionalStatusConfig {
  status: string;
  label: string;
  icon: string;
  description: string;
  showSections: string[];
  hideSections: string[];
}

interface WizardSectionDto {
  key: string;
  title: string;
  description?: string;
  stepNumber: number;
  isRequired: boolean;
  isCompleted: boolean;
  fields: WizardFieldDto[];
}

interface WizardFieldDto {
  key: string;
  label: string;
  fieldType: string;
  isRequired: boolean;
  placeholder?: string;
  defaultValue?: string;
  options?: string[];
  helpText?: string;
  hidden: boolean;
}

interface SaveStepResponse {
  success: boolean;
  completedSteps: number;
  totalSteps: number;
  message?: string;
}

interface StepDataResponse {
  stepKey: string;
  isCompleted: boolean;
  data: Record<string, any>;
  items: Record<string, any>[];
}

interface WizardProgressDto {
  profileId: number;
  jobId: number;
  professionalStatus: string;
  completedSteps: number;
  totalSteps: number;
  steps: StepStatusDto[];
}

interface StepStatusDto {
  key: string;
  title: string;
  isCompleted: boolean;
  isCurrent: boolean;
}

// ---------- Status icon mapping ----------

const STATUS_ICONS: Record<string, React.ReactNode> = {
  'Student': <BookOutlined />,
  'Fresher': <UserOutlined />,
  'Working': <ExperimentOutlined />,
  'ServingNotice': <ClockCircleOutlined />,
  'Freelancer': <GlobalOutlined />,
  'CareerBreak': <ClockCircleOutlined />,
  'LookingInternship': <AimOutlined />,
};

const SECTION_ICONS: Record<string, React.ReactNode> = {
  'professional-status': <UserOutlined />,
  'basic': <UserOutlined />,
  'contact': <ContactsOutlined />,
  'professional': <ExperimentOutlined />,
  'education': <BookOutlined />,
  'experience': <ExperimentOutlined />,
  'projects': <SettingOutlined />,
  'internships': <BookOutlined />,
  'skills': <StarOutlined />,
  'certifications': <TrophyOutlined />,
  'training': <BookOutlined />,
  'social': <LinkOutlined />,
  'achievements': <TrophyOutlined />,
  'languages': <GlobalOutlined />,
  'documents': <FileTextOutlined />,
  'references': <TeamOutlined />,
  'work-authorization': <SafetyOutlined />,
  'diversity': <TeamOutlined />,
  'emergency': <SafetyOutlined />,
  'availability': <ClockCircleOutlined />,
  'preferences': <SettingOutlined />,
  'ai-section': <AimOutlined />,
  'questions': <FormOutlined />,
  'consent': <CheckSquareOutlined />,
};

// ---------- Section metadata (frontend-defined, no API needed) ----------

const SECTION_META: { key: string; title: string; stepNumber: number; isRequired: boolean; description?: string }[] = [
  { key: 'professional-status', title: 'Professional Status', stepNumber: 1, isRequired: true },
  { key: 'basic', title: 'Basic Information', stepNumber: 2, isRequired: true, description: 'Tell us about yourself' },
  { key: 'contact', title: 'Contact & Address', stepNumber: 3, isRequired: true, description: 'Your contact details and address' },
  { key: 'professional', title: 'Professional Info', stepNumber: 4, isRequired: true, description: 'Current employment details' },
  { key: 'education', title: 'Education', stepNumber: 5, isRequired: true, description: 'Your academic background' },
  { key: 'experience', title: 'Work Experience', stepNumber: 6, isRequired: false, description: 'Your work history' },
  { key: 'projects', title: 'Projects', stepNumber: 7, isRequired: true, description: 'Notable projects you worked on' },
  { key: 'internships', title: 'Internships', stepNumber: 8, isRequired: false, description: 'Internship experience' },
  { key: 'skills', title: 'Skills & Expertise', stepNumber: 9, isRequired: true, description: 'Your technical and professional skills' },
  { key: 'certifications', title: 'Certifications', stepNumber: 10, isRequired: false, description: 'Professional certifications' },
  { key: 'training', title: 'Training & Courses', stepNumber: 11, isRequired: false, description: 'Courses and training programs' },
  { key: 'social', title: 'Coding Profiles', stepNumber: 12, isRequired: false, description: 'GitHub, LinkedIn, LeetCode, etc.' },
  { key: 'achievements', title: 'Achievements', stepNumber: 13, isRequired: false, description: 'Awards, publications, hackathons' },
  { key: 'languages', title: 'Languages', stepNumber: 14, isRequired: false, description: 'Languages you speak' },
  { key: 'documents', title: 'Resume & Documents', stepNumber: 15, isRequired: true, description: 'Upload your resume and supporting docs' },
  { key: 'references', title: 'References', stepNumber: 16, isRequired: false, description: 'Professional references' },
  { key: 'work-authorization', title: 'Work Authorization', stepNumber: 17, isRequired: false, description: 'Your work eligibility' },
  { key: 'diversity', title: 'Diversity (Optional)', stepNumber: 18, isRequired: false },
  { key: 'emergency', title: 'Emergency Contact', stepNumber: 19, isRequired: false },
  { key: 'availability', title: 'Availability', stepNumber: 20, isRequired: false, description: 'When can you start?' },
  { key: 'preferences', title: 'Job Preferences', stepNumber: 21, isRequired: true, description: 'Your preferred work setup' },
  { key: 'ai-section', title: 'AI Analysis', stepNumber: 22, isRequired: false },
  { key: 'questions', title: 'Job Questions', stepNumber: 23, isRequired: false },
  { key: 'consent', title: 'Review & Submit', stepNumber: 24, isRequired: true, description: 'Review all information before submitting' },
];

const STATUS_HIDDEN_SECTIONS: Record<string, string[]> = {
  'Student': ['professional', 'experience', 'references', 'work-authorization'],
  'Fresher': ['professional', 'experience', 'references', 'work-authorization'],
  'Freelancer': ['experience'],
  'CareerBreak': ['professional', 'experience', 'references', 'work-authorization'],
  'LookingInternship': ['professional', 'experience', 'references', 'work-authorization'],
};

function getSectionsForStatus(status: string): WizardSectionDto[] {
  const hidden = STATUS_HIDDEN_SECTIONS[status] || [];
  return SECTION_META
    .filter(s => !hidden.includes(s.key))
    .map(s => ({
      key: s.key,
      title: s.title,
      description: s.description,
      stepNumber: s.stepNumber,
      isRequired: s.isRequired,
      isCompleted: false,
      fields: [],
    }));
}

// ---------- Field definitions for each step ----------

const STEP_FIELDS: Record<string, WizardFieldDto[]> = {
  'professional-status': [
    { key: 'status', label: 'Professional Status', fieldType: 'select', isRequired: true, placeholder: 'Select your current status', options: [], helpText: 'This determines which sections you need to fill.', hidden: false },
  ],
  'basic': [
    { key: 'firstName', label: 'First Name', fieldType: 'text', isRequired: true, placeholder: 'Enter first name', hidden: false },
    { key: 'lastName', label: 'Last Name', fieldType: 'text', isRequired: true, placeholder: 'Enter last name', hidden: false },
    { key: 'middleName', label: 'Middle Name', fieldType: 'text', isRequired: false, placeholder: 'Enter middle name', hidden: false },
    { key: 'preferredName', label: 'Preferred Name', fieldType: 'text', isRequired: false, placeholder: 'What should we call you?', hidden: false },
    { key: 'gender', label: 'Gender', fieldType: 'select', isRequired: true, options: ['Male', 'Female', 'Non-binary', 'Prefer not to say'], hidden: false },
    { key: 'dateOfBirth', label: 'Date of Birth', fieldType: 'date', isRequired: true, hidden: false },
    { key: 'nationality', label: 'Nationality', fieldType: 'text', isRequired: true, placeholder: 'e.g. Indian', hidden: false },
    { key: 'maritalStatus', label: 'Marital Status', fieldType: 'select', isRequired: false, options: ['Single', 'Married', 'Divorced', 'Widowed', 'Prefer not to say'], hidden: false },
  ],
  'contact': [
    { key: 'email', label: 'Email Address', fieldType: 'email', isRequired: true, placeholder: 'you@example.com', hidden: false },
    { key: 'phone', label: 'Phone Number', fieldType: 'tel', isRequired: true, placeholder: '+91-9876543210', hidden: false },
    { key: 'alternateEmail', label: 'Alternate Email', fieldType: 'email', isRequired: false, placeholder: 'alternate@example.com', hidden: false },
    { key: 'alternatePhone', label: 'Alternate Phone', fieldType: 'tel', isRequired: false, placeholder: '+91-9876543211', hidden: false },
    { key: 'whatsappNumber', label: 'WhatsApp Number', fieldType: 'tel', isRequired: false, placeholder: 'Same as phone if not specified', hidden: false },
    { key: 'currentAddress', label: 'Current Address', fieldType: 'textarea', isRequired: true, placeholder: 'Enter your current address', hidden: false },
    { key: 'permanentAddress', label: 'Permanent Address', fieldType: 'textarea', isRequired: false, placeholder: 'Enter permanent address', hidden: false },
    { key: 'country', label: 'Country', fieldType: 'text', isRequired: true, placeholder: 'e.g. India', hidden: false },
    { key: 'state', label: 'State', fieldType: 'text', isRequired: true, placeholder: 'e.g. Maharashtra', hidden: false },
    { key: 'city', label: 'City', fieldType: 'text', isRequired: true, placeholder: 'e.g. Pune', hidden: false },
    { key: 'zipCode', label: 'Zip / Postal Code', fieldType: 'text', isRequired: true, placeholder: 'e.g. 411001', hidden: false },
    { key: 'nearestLandmark', label: 'Nearest Landmark', fieldType: 'text', isRequired: false, placeholder: 'Optional', hidden: false },
  ],
  'professional': [
    { key: 'currentCompany', label: 'Current / Last Company', fieldType: 'text', isRequired: true, placeholder: 'Company name', hidden: false },
    { key: 'designation', label: 'Designation', fieldType: 'text', isRequired: true, placeholder: 'e.g. Senior Software Engineer', hidden: false },
    { key: 'totalExperienceYears', label: 'Total Experience (Years)', fieldType: 'number', isRequired: true, placeholder: 'e.g. 5', hidden: false },
    { key: 'currentCtc', label: 'Current CTC (₹ LPA)', fieldType: 'number', isRequired: false, placeholder: 'e.g. 12', hidden: false },
    { key: 'expectedCtc', label: 'Expected CTC (₹ LPA)', fieldType: 'number', isRequired: false, placeholder: 'e.g. 18', hidden: false },
    { key: 'noticePeriod', label: 'Notice Period (Days)', fieldType: 'number', isRequired: true, placeholder: 'e.g. 30', hidden: false },
    { key: 'currentLocation', label: 'Current Location', fieldType: 'text', isRequired: true, placeholder: 'e.g. Pune, India', hidden: false },
  ],
  'education': [
    { key: 'degree', label: 'Degree / Diploma', fieldType: 'text', isRequired: true, placeholder: 'e.g. B.Tech Computer Science', hidden: false },
    { key: 'institution', label: 'Institution', fieldType: 'text', isRequired: true, placeholder: 'University / College name', hidden: false },
    { key: 'yearOfPassing', label: 'Year of Passing', fieldType: 'number', isRequired: true, placeholder: 'e.g. 2020', hidden: false },
    { key: 'percentage', label: 'Percentage / CGPA', fieldType: 'number', isRequired: true, placeholder: 'e.g. 85 or 8.5', hidden: false },
    { key: 'specialization', label: 'Specialization', fieldType: 'text', isRequired: false, placeholder: 'e.g. Data Science', hidden: false },
    { key: 'isPursuing', label: 'Currently Pursuing', fieldType: 'checkbox', isRequired: false, hidden: false },
  ],
  'experience': [
    { key: 'company', label: 'Company', fieldType: 'text', isRequired: true, placeholder: 'Company name', hidden: false },
    { key: 'role', label: 'Role / Title', fieldType: 'text', isRequired: true, placeholder: 'e.g. Frontend Developer', hidden: false },
    { key: 'startDate', label: 'Start Date', fieldType: 'date', isRequired: true, hidden: false },
    { key: 'endDate', label: 'End Date', fieldType: 'date', isRequired: false, helpText: 'Leave blank if current', hidden: false },
    { key: 'isCurrent', label: 'I currently work here', fieldType: 'checkbox', isRequired: false, hidden: false },
    { key: 'responsibilities', label: 'Key Responsibilities', fieldType: 'textarea', isRequired: false, placeholder: 'Describe your role and achievements', hidden: false },
  ],
  'projects': [
    { key: 'title', label: 'Project Title', fieldType: 'text', isRequired: true, placeholder: 'e.g. HRMS Portal', hidden: false },
    { key: 'description', label: 'Description', fieldType: 'textarea', isRequired: true, placeholder: 'Brief description of the project', hidden: false },
    { key: 'techStack', label: 'Tech Stack', fieldType: 'text', isRequired: false, placeholder: 'e.g. React, .NET, SQL Server', hidden: false },
    { key: 'url', label: 'Project URL', fieldType: 'url', isRequired: false, placeholder: 'GitHub / live demo link', hidden: false },
    { key: 'duration', label: 'Duration', fieldType: 'text', isRequired: false, placeholder: 'e.g. 3 months', hidden: false },
  ],
  'internships': [
    { key: 'company', label: 'Company / Organization', fieldType: 'text', isRequired: true, placeholder: 'Company name', hidden: false },
    { key: 'role', label: 'Role', fieldType: 'text', isRequired: true, placeholder: 'e.g. Intern', hidden: false },
    { key: 'duration', label: 'Duration', fieldType: 'text', isRequired: true, placeholder: 'e.g. 6 months', hidden: false },
    { key: 'description', label: 'Description', fieldType: 'textarea', isRequired: false, placeholder: 'What you worked on', hidden: false },
  ],
  'skills': [
    { key: 'skill', label: 'Skill Name', fieldType: 'text', isRequired: true, placeholder: 'e.g. React', hidden: false },
    { key: 'proficiency', label: 'Proficiency Level', fieldType: 'select', isRequired: true, options: ['Beginner', 'Intermediate', 'Advanced', 'Expert'], hidden: false },
    { key: 'yearsOfExperience', label: 'Years of Experience', fieldType: 'number', isRequired: false, placeholder: 'e.g. 3', hidden: false },
  ],
  'certifications': [
    { key: 'name', label: 'Certification Name', fieldType: 'text', isRequired: true, placeholder: 'e.g. AWS Certified Developer', hidden: false },
    { key: 'issuer', label: 'Issuing Organization', fieldType: 'text', isRequired: true, placeholder: 'e.g. Amazon', hidden: false },
    { key: 'year', label: 'Year Obtained', fieldType: 'number', isRequired: true, placeholder: 'e.g. 2023', hidden: false },
    { key: 'url', label: 'Credential URL', fieldType: 'url', isRequired: false, placeholder: 'Link to certificate', hidden: false },
  ],
  'training': [
    { key: 'title', label: 'Course / Training Title', fieldType: 'text', isRequired: true, placeholder: 'e.g. Advanced React Patterns', hidden: false },
    { key: 'provider', label: 'Provider', fieldType: 'text', isRequired: true, placeholder: 'e.g. Udemy, Coursera', hidden: false },
    { key: 'year', label: 'Year Completed', fieldType: 'number', isRequired: true, placeholder: 'e.g. 2024', hidden: false },
    { key: 'duration', label: 'Duration', fieldType: 'text', isRequired: false, placeholder: 'e.g. 40 hours', hidden: false },
  ],
  'social': [
    { key: 'linkedin', label: 'LinkedIn URL', fieldType: 'url', isRequired: false, placeholder: 'https://linkedin.com/in/your-profile', hidden: false },
    { key: 'github', label: 'GitHub URL', fieldType: 'url', isRequired: false, placeholder: 'https://github.com/your-username', hidden: false },
    { key: 'leetcode', label: 'LeetCode URL', fieldType: 'url', isRequired: false, placeholder: 'https://leetcode.com/your-username', hidden: false },
    { key: 'hackerrank', label: 'HackerRank URL', fieldType: 'url', isRequired: false, placeholder: 'https://hackerrank.com/your-username', hidden: false },
    { key: 'portfolio', label: 'Portfolio Website', fieldType: 'url', isRequired: false, placeholder: 'https://your-portfolio.com', hidden: false },
    { key: 'stackoverflow', label: 'Stack Overflow URL', fieldType: 'url', isRequired: false, placeholder: 'https://stackoverflow.com/users/your-id', hidden: false },
  ],
  'achievements': [
    { key: 'type', label: 'Type', fieldType: 'select', isRequired: true, options: ['Award', 'Publication', 'Patent', 'Hackathon', 'Open Source', 'Other'], hidden: false },
    { key: 'title', label: 'Title', fieldType: 'text', isRequired: true, placeholder: 'e.g. Best Paper Award 2024', hidden: false },
    { key: 'organization', label: 'Organization', fieldType: 'text', isRequired: false, placeholder: 'e.g. IEEE', hidden: false },
    { key: 'year', label: 'Year', fieldType: 'number', isRequired: false, placeholder: 'e.g. 2024', hidden: false },
    { key: 'description', label: 'Description', fieldType: 'textarea', isRequired: false, placeholder: 'Brief description', hidden: false },
    { key: 'url', label: 'URL', fieldType: 'url', isRequired: false, placeholder: 'Link to proof', hidden: false },
  ],
  'languages': [
    { key: 'language', label: 'Language', fieldType: 'text', isRequired: true, placeholder: 'e.g. English', hidden: false },
    { key: 'proficiency', label: 'Proficiency', fieldType: 'select', isRequired: true, options: ['Basic', 'Conversational', 'Professional', 'Native'], hidden: false },
    { key: 'canRead', label: 'Can Read', fieldType: 'checkbox', isRequired: false, hidden: false },
    { key: 'canWrite', label: 'Can Write', fieldType: 'checkbox', isRequired: false, hidden: false },
    { key: 'canSpeak', label: 'Can Speak', fieldType: 'checkbox', isRequired: false, hidden: false },
  ],
  'documents': [
    { key: 'resume', label: 'Resume / CV', fieldType: 'file', isRequired: true, helpText: 'Upload PDF, DOC, or DOCX (Max 5MB)', hidden: false },
    { key: 'coverLetter', label: 'Cover Letter', fieldType: 'file', isRequired: false, helpText: 'Optional cover letter', hidden: false },
    { key: 'otherDocs', label: 'Other Documents', fieldType: 'file', isRequired: false, helpText: 'Portfolio, certificates, etc.', hidden: false },
  ],
  'references': [
    { key: 'name', label: 'Full Name', fieldType: 'text', isRequired: true, placeholder: 'e.g. John Doe', hidden: false },
    { key: 'company', label: 'Company', fieldType: 'text', isRequired: true, placeholder: 'e.g. Acme Corp', hidden: false },
    { key: 'role', label: 'Role / Title', fieldType: 'text', isRequired: true, placeholder: 'e.g. Engineering Manager', hidden: false },
    { key: 'email', label: 'Email', fieldType: 'email', isRequired: true, placeholder: 'john@example.com', hidden: false },
    { key: 'phone', label: 'Phone', fieldType: 'tel', isRequired: false, placeholder: '+91-9876543210', hidden: false },
  ],
  'work-authorization': [
    { key: 'workAuth', label: 'Work Authorization', fieldType: 'select', isRequired: true, options: ['Citizen', 'Permanent Resident', 'Work Visa', 'Student Visa', 'Other'], hidden: false },
    { key: 'visaStatus', label: 'Visa Status Details', fieldType: 'text', isRequired: false, placeholder: 'e.g. H1-B valid till Dec 2026', hidden: false },
    { key: 'requiresSponsorship', label: 'Requires Visa Sponsorship?', fieldType: 'radio', isRequired: true, options: ['Yes', 'No', 'Not Sure'], hidden: false },
  ],
  'diversity': [
    { key: 'gender', label: 'Gender', fieldType: 'select', isRequired: false, options: ['Male', 'Female', 'Non-binary', 'Prefer not to say'], hidden: false },
    { key: 'veteranStatus', label: 'Veteran Status', fieldType: 'select', isRequired: false, options: ['I am a veteran', 'I am not a veteran', 'Prefer not to say'], hidden: false },
    { key: 'disabilityStatus', label: 'Disability Status', fieldType: 'select', isRequired: false, options: ['Yes, I have a disability', 'No, I do not', 'Prefer not to say'], hidden: false },
  ],
  'emergency': [
    { key: 'emergencyContact', label: 'Emergency Contact Name', fieldType: 'text', isRequired: true, placeholder: 'Full name', hidden: false },
    { key: 'emergencyPhone', label: 'Emergency Contact Phone', fieldType: 'tel', isRequired: true, placeholder: '+91-9876543210', hidden: false },
    { key: 'emergencyRelation', label: 'Relationship', fieldType: 'select', isRequired: true, options: ['Spouse', 'Parent', 'Sibling', 'Friend', 'Other'], hidden: false },
  ],
  'availability': [
    { key: 'startDate', label: 'Earliest Start Date', fieldType: 'date', isRequired: true, hidden: false },
    { key: 'noticePeriod', label: 'Notice Period (Days)', fieldType: 'number', isRequired: true, placeholder: 'e.g. 30', hidden: false },
    { key: 'preferredShift', label: 'Preferred Shift', fieldType: 'select', isRequired: false, options: ['Day Shift', 'Night Shift', 'Flexible', 'Rotational'], hidden: false },
  ],
  'preferences': [
    { key: 'preferredLocation', label: 'Preferred Job Location', fieldType: 'text', isRequired: true, placeholder: 'e.g. Pune, Mumbai, Remote', hidden: false },
    { key: 'workType', label: 'Work Type Preference', fieldType: 'select', isRequired: true, options: ['Remote', 'Hybrid', 'Office', 'Flexible'], hidden: false },
    { key: 'willingToRelocate', label: 'Willing to Relocate?', fieldType: 'radio', isRequired: true, options: ['Yes', 'No', 'Depends on Location'], hidden: false },
  ],
  'ai-section': [],
  'questions': [],
  'consent': [
    { key: 'agree', label: 'I confirm that all the information provided is true and accurate.', fieldType: 'checkbox', isRequired: true, hidden: false },
    { key: 'consent', label: 'I consent to the processing of my personal data for recruitment purposes.', fieldType: 'checkbox', isRequired: true, hidden: false },
    { key: 'communication', label: 'I agree to receive communications about my application via email/phone.', fieldType: 'checkbox', isRequired: false, hidden: false },
  ],
};

// ---------- Multi-entry step keys ----------

const MULTI_ENTRY_STEPS = new Set([
  'education', 'experience', 'projects', 'internships',
  'skills', 'certifications', 'training', 'achievements',
  'languages', 'references'
]);

// ---------- Component ----------

export default function CareerWizardPage() {
  const { message: messageApi } = AntApp.useApp();
  const { jobId } = useParams<{ jobId: string }>();
  const navigate = useNavigate();
  const { isAuthenticated, login, register, loadSession } = useCareerAuthStore();

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [statusConfigs, setStatusConfigs] = useState<ProfessionalStatusConfig[]>([]);
  const [sections, setSections] = useState<WizardSectionDto[]>([]);
  const [currentStep, setCurrentStep] = useState(0);
  const [completedSteps, setCompletedSteps] = useState<Set<string>>(new Set());
  const [progress, setProgress] = useState<WizardProgressDto | null>(null);
  const [submitted, setSubmitted] = useState(false);
  const [authModalOpen, setAuthModalOpen] = useState(false);
  const [authMode, setAuthMode] = useState<'login' | 'register'>('login');
  const [authForm] = Form.useForm();

  // Multi-entry state: items per step key
  const [multiItems, setMultiItems] = useState<Record<string, Record<string, any>[]>>({});
  // Form data per step (single entry)
  const [stepData, setStepData] = useState<Record<string, Record<string, any>>>({});
  // Selected professional status
  const [selectedStatus, setSelectedStatus] = useState<string>('');
  const [profileData, setProfileData] = useState<Record<string, any> | null>(null);
  const [autoFilling, setAutoFilling] = useState(false);

  // Pending step data to save after auth
  const pendingAfterAuth = useRef<(() => Promise<void>) | null>(null);

  // Auth action: try to open auth modal
  const requireAuth = useCallback((action: () => void) => {
    if (isAuthenticated) {
      action();
    } else {
      setAuthMode('login');
      pendingAfterAuth.current = async () => { action(); };
      setAuthModalOpen(true);
    }
  }, [isAuthenticated]);

  // On mount: restore session, fetch status config, show first step
  useEffect(() => {
    loadSession();
    const init = async () => {
      try {
        setLoading(true);
        const res = await careerApi.get('/wizard/status-config');
        setStatusConfigs(res.data);
        setSections(getSectionsForStatus(''));
      } catch (err) {
        messageApi.error('Failed to load wizard configuration');
      } finally {
        setLoading(false);
      }
    };
    init();
  }, []);

  // When status selected, build sections from frontend metadata
  useEffect(() => {
    if (!selectedStatus) return;
    setLoading(true);
    const frontendSections = getSectionsForStatus(selectedStatus);
    setSections(frontendSections);
    setCurrentStep(0);
    setLoading(false);
  }, [selectedStatus]);

  // When authenticated, load profile data and progress
  useEffect(() => {
    if (!isAuthenticated || !jobId || !selectedStatus) return;

    // Fetch profile data for auto-fill
    careerApi.get('/profile').then(res => {
      const p = res.data;
      if (p.resumeFileName) {
        careerApi.post('/profile/parse-resume').then(pr => {
          setProfileData({ ...p, parsed: pr.data });
        }).catch(() => setProfileData(p));
      } else {
        setProfileData(p);
      }
    }).catch(() => {});

    const fetchProgress = async () => {
      try {
        const res = await careerApi.get(`/wizard/progress/${jobId}`);
        const data: WizardProgressDto = res.data;
        setProgress(data);
        const completed = new Set(data.steps.filter(s => s.isCompleted).map(s => s.key));
        setCompletedSteps(completed);
        const currentIdx = data.steps.findIndex(s => s.isCurrent);
        if (currentIdx >= 0) setCurrentStep(currentIdx);

        // Load step data for each completed step
        for (const step of data.steps) {
          if (step.isCompleted) {
            try {
              const stepRes = await careerApi.get(`/wizard/step/${jobId}/${step.key}`);
              const stepDataRes: StepDataResponse = stepRes.data;
              if (stepDataRes.data && Object.keys(stepDataRes.data).length > 0) {
                setStepData(prev => ({ ...prev, [step.key]: stepDataRes.data }));
              }
              if (stepDataRes.items && stepDataRes.items.length > 0) {
                setMultiItems(prev => ({ ...prev, [step.key]: stepDataRes.items }));
              }
            } catch { /* skip failed loads */ }
          }
        }
      } catch { /* no progress yet */ }
    };
    fetchProgress();
  }, [isAuthenticated, jobId, selectedStatus]);

  const currentSection = sections[currentStep];
  const isFirstStep = currentStep === 0;
  const isLastStep = currentStep === sections.length - 1;
  const isMultiEntry = currentSection ? MULTI_ENTRY_STEPS.has(currentSection.key) : false;
  const currentItems = currentSection ? multiItems[currentSection.key] || [] : [];

  const progressPercent = sections.length > 0
    ? Math.round((completedSteps.size / sections.length) * 100)
    : 0;

  const goToStep = (idx: number) => {
    if (idx < 0 || idx >= sections.length) return;
    if (!currentSection) { setCurrentStep(idx); return; }
    if (idx > currentStep && currentSection.key !== 'professional-status') {
      const fieldDefs = STEP_FIELDS[currentSection.key] || [];
      if (MULTI_ENTRY_STEPS.has(currentSection.key)) {
        // For multi-entry steps, check that at least one item exists and each required field is filled in at least one item
        const items = multiItems[currentSection.key] || [];
        if (items.length === 0) {
          messageApi.warning('Please add at least one entry');
          return;
        }
        // All items must have required fields filled
        const hasMissing = items.some(item =>
          fieldDefs.filter(f => f.isRequired).some(f => {
            const v = item[f.key];
            return v === null || v === undefined || v === '';
          })
        );
        if (hasMissing) {
          messageApi.warning('Please fill all required fields in each entry');
          return;
        }
      } else {
        const data = stepData[currentSection.key] || {};
        const missing = fieldDefs.filter(f => f.isRequired && !data[f.key]);
        if (missing.length > 0) {
          messageApi.warning(`Please fill required fields: ${missing.map(f => f.label).join(', ')}`);
          return;
        }
      }
    }
    setCurrentStep(idx);
  };

  // ---------- Auto-fill from profile/resume ----------

  interface AutoFillRule { profileField: string; stepField: string; transform?: (v: any) => any }

  const AUTO_FILL_STEP_KEYS = new Set(['basic', 'contact', 'professional', 'social']);

  /** Single-entry step field mappings (parsed.field → stepData.step.field) */
  const AUTO_FILL_RULES: Record<string, AutoFillRule[]> = {
    'basic': [
      { profileField: 'firstName', stepField: 'firstName' },
      { profileField: 'lastName', stepField: 'lastName' },
      { profileField: 'gender', stepField: 'gender' },
      { profileField: 'dateOfBirth', stepField: 'dateOfBirth' },
      { profileField: 'nationality', stepField: 'nationality' },
    ],
    'contact': [
      { profileField: 'email', stepField: 'email' },
      { profileField: 'phone', stepField: 'phone' },
      { profileField: 'currentAddress', stepField: 'currentAddress' },
      { profileField: 'city', stepField: 'city' },
      { profileField: 'state', stepField: 'state' },
      { profileField: 'country', stepField: 'country' },
      { profileField: 'zipCode', stepField: 'zipCode' },
    ],
    'professional': [
      { profileField: 'currentCompany', stepField: 'currentCompany' },
      { profileField: 'currentDesignation', stepField: 'designation' },
      { profileField: 'totalExperienceMonths', stepField: 'totalExperienceYears', transform: v => Math.round(Number(v) / 12) },
      { profileField: 'currentCtc', stepField: 'currentCtc' },
      { profileField: 'expectedCtc', stepField: 'expectedCtc' },
    ],
    'social': [
      { profileField: 'linkedInUrl', stepField: 'linkedin' },
      { profileField: 'gitHubUrl', stepField: 'github' },
      { profileField: 'portfolioUrl', stepField: 'portfolio' },
    ],
  };

  /** Multi-entry step keys whose arrays come directly from the full parse result */
  const MULTI_ENTRY_AUTO_FILL = new Set([
    'education', 'experience', 'projects', 'internships',
    'skills', 'certifications', 'training', 'achievements',
    'languages', 'references'
  ]);

  /** Fetch profile + full parsed resume and auto-fill EVERYTHING at once */
  const autoFillAllSteps = async () => {
    setAutoFilling(true);
    await new Promise(r => setTimeout(r, 300));
    try {
      const res = await careerApi.get('/profile');
      const p = res.data as Record<string, any>;

      // Try the full parse that includes arrays for multi-entry steps
      let fullParsed: Record<string, any> = {};
      if (p.resumeFileName) {
        try {
          const pr = await careerApi.post('/profile/parse-resume-full');
          fullParsed = pr.data || {};
        } catch {
          // Fallback to simple parse if full fails
          try {
            const pr2 = await careerApi.post('/profile/parse-resume');
            fullParsed = pr2.data || {};
          } catch { /* both parsers failed */ }
        }
      }

      const val = (field: string) => fullParsed[field] ?? p[field];
      const stepUpdates: Record<string, Record<string, any>> = {};
      const multiUpdates: Record<string, Record<string, any>[]> = {};
      let totalFilled = 0;

      // Fill single-entry steps
      for (const [stepKey, rules] of Object.entries(AUTO_FILL_RULES)) {
        const fill: Record<string, any> = {};
        for (const rule of rules) {
          const raw = val(rule.profileField);
          if (raw !== null && raw !== undefined && raw !== '') {
            fill[rule.stepField] = rule.transform ? rule.transform(raw) : raw;
            totalFilled++;
          }
        }
        if (Object.keys(fill).length > 0) {
          stepUpdates[stepKey] = fill;
        }
      }

      // Fill multi-entry steps from full parse arrays
      for (const stepKey of MULTI_ENTRY_AUTO_FILL) {
        const items = fullParsed[stepKey];
        if (Array.isArray(items) && items.length > 0) {
          multiUpdates[stepKey] = items;
          totalFilled += items.length;
        }
      }

      if (totalFilled === 0) {
        setAutoFilling(false);
        messageApi.info('No resume data available to auto-fill');
        return;
      }

      // Apply single-entry updates
      setStepData(prev => {
        const next = { ...prev };
        for (const [stepKey, fill] of Object.entries(stepUpdates)) {
          next[stepKey] = { ...(next[stepKey] || {}), ...fill };
        }
        return next;
      });

      // Apply multi-entry updates
      setMultiItems(prev => {
        const next = { ...prev };
        for (const [stepKey, items] of Object.entries(multiUpdates)) {
          next[stepKey] = items;
        }
        return next;
      });

      setAutoFilling(false);

      const fieldCount = Object.keys(stepUpdates).length;
      const multiCount = Object.keys(multiUpdates).length;
      const parts: string[] = [];
      if (fieldCount > 0) parts.push(`${Object.values(stepUpdates).reduce((s, f) => s + Object.keys(f).length, 0)} field(s)`);
      if (multiCount > 0) parts.push(`${multiCount} section(s)`);
      messageApi.success(`Auto-filled ${parts.join(' & ')} from your resume`);
    } catch {
      setAutoFilling(false);
      messageApi.error('Failed to fetch profile data');
    }
  };

  const saveCurrentStep = async (data: Record<string, any>): Promise<boolean> => {
    if (!isAuthenticated) {
      return new Promise((resolve) => {
        setAuthMode('login');
        pendingAfterAuth.current = async () => {
          const ok = await saveCurrentStep(data);
          if (ok && currentStep < sections.length - 1) {
            goToStep(currentStep + 1);
          }
          resolve(ok);
        };
        setAuthModalOpen(true);
      });
    }
    if (!currentSection || !jobId) return false;
    try {
      setSaving(true);
      const payload = { jobId: parseInt(jobId), stepKey: currentSection.key, data };
      const res = await careerApi.post('/wizard/step', payload);
      const response: SaveStepResponse = res.data;
      if (response.success) {
        setCompletedSteps(prev => new Set([...prev, currentSection.key]));
        setStepData(prev => ({ ...prev, [currentSection.key]: data }));
        messageApi.success(`Step "${currentSection.title}" saved`);
        return true;
      }
      return false;
    } catch (err: any) {
      messageApi.error(err?.response?.data?.message || 'Failed to save step');
      return false;
    } finally {
      setSaving(false);
    }
  };

  const handleNext = async () => {
    if (!currentSection) return;
    if (currentSection.key === 'professional-status') {
      if (!selectedStatus) {
        messageApi.warning('Please select your professional status');
        return;
      }
      // Save status selection and advance
      await saveCurrentStep({ status: selectedStatus });
      if (currentStep < sections.length - 1) {
        goToStep(currentStep + 1);
      }
      return;
    }
    await requireAuth(async () => {
      const formData = stepData[currentSection.key];
      if (formData && Object.keys(formData).length > 0) {
        await saveCurrentStep(formData);
      }
      if (currentStep < sections.length - 1) {
        goToStep(currentStep + 1);
      }
    });
  };

  const validateRequiredFields = (key: string): boolean => {
    const fields = STEP_FIELDS[key] || [];
    const data = stepData[key] || {};
    const missing = fields.filter(f => f.isRequired && !data[f.key]);
    if (missing.length > 0) {
      messageApi.warning(`Please fill: ${missing.map(f => f.label).join(', ')}`);
      return false;
    }
    return true;
  };

  const handleSkip = () => {
    if (currentStep < sections.length - 1) {
      goToStep(currentStep + 1);
    }
  };

  const handleSaveAndNext = async () => {
    if (!currentSection) return;
    if (currentSection.key === 'professional-status') {
      if (!selectedStatus) {
        messageApi.warning('Please select your professional status');
        return;
      }
      await saveCurrentStep({ status: selectedStatus });
      if (currentStep < sections.length - 1) {
        goToStep(currentStep + 1);
      }
      return;
    }
    if (!validateRequiredFields(currentSection.key)) return;
    await requireAuth(async () => {
      const formData = stepData[currentSection.key];
      if (formData && Object.keys(formData).length > 0) {
        await saveCurrentStep(formData);
      }
      if (currentStep < sections.length - 1) {
        goToStep(currentStep + 1);
      }
    });
  };

  const handleSubmit = async () => {
    if (!jobId) return;
    try {
      setSubmitting(true);
      await careerApi.post(`/wizard/submit/${jobId}`);
      setSubmitted(true);
      messageApi.success('Application submitted successfully!');
    } catch (err: any) {
      messageApi.error(err?.response?.data?.message || 'Failed to submit application');
    } finally {
      setSubmitting(false);
    }
  };

  const handleAuth = async (values: { email: string; password: string; firstName?: string; lastName?: string; phone?: string }) => {
    try {
      if (authMode === 'register') {
        await register(values as any);
      } else {
        await login(values.email, values.password);
      }
      setAuthModalOpen(false);
      authForm.resetFields();
      messageApi.success(authMode === 'register' ? 'Account created!' : 'Logged in!');
      const pending = pendingAfterAuth.current;
      pendingAfterAuth.current = null;
      if (pending) await pending();
    } catch (err: any) {
      messageApi.error(err?.response?.data?.message || 'Authentication failed');
    }
  };

  // ---------- Render field ----------

  const renderField = (field: WizardFieldDto, value: any, onChange: (key: string, val: any) => void) => {
    const sharedProps = {
      placeholder: field.placeholder,
      disabled: field.fieldType === 'file',
    };

    switch (field.fieldType) {
      case 'text':
      case 'email':
      case 'tel':
      case 'url':
        return (
          <Input
            {...sharedProps}
            type={field.fieldType === 'email' ? 'email' : field.fieldType === 'tel' ? 'tel' : field.fieldType === 'url' ? 'url' : 'text'}
            value={value || ''}
            onChange={e => onChange(field.key, e.target.value)}
          />
        );
      case 'number':
        return (
          <InputNumber
            style={{ width: '100%' }}
            placeholder={field.placeholder}
            value={value}
            onChange={val => onChange(field.key, val)}
          />
        );
      case 'textarea':
        return (
          <TextArea
            rows={4}
            {...sharedProps}
            value={value || ''}
            onChange={e => onChange(field.key, e.target.value)}
          />
        );
      case 'select':
        if (field.key === 'status') {
          return (
            <Radio.Group
              value={value || selectedStatus}
              onChange={e => {
                const v = e.target.value;
                setSelectedStatus(v);
                onChange(field.key, v);
              }}
              style={{ width: '100%' }}
            >
              <Space orientation="vertical" style={{ width: '100%' }}>
                {statusConfigs.map(cfg => (
                  <Card
                    key={cfg.status}
                    size="small"
                    hoverable
                    style={{
                      border: selectedStatus === cfg.status ? '2px solid #6c5ce7' : '1px solid #f0f0f0',
                      cursor: 'pointer',
                    }}
                    onClick={() => {
                      setSelectedStatus(cfg.status);
                      onChange(field.key, cfg.status);
                    }}
                  >
                    <Space>
                      {STATUS_ICONS[cfg.status] || <UserOutlined />}
                      <div>
                        <Text strong>{cfg.label}</Text>
                        <br />
                        <Text type="secondary" style={{ fontSize: 12 }}>{cfg.description}</Text>
                      </div>
                    </Space>
                  </Card>
                ))}
              </Space>
            </Radio.Group>
          );
        }
        return (
          <Select
            style={{ width: '100%' }}
            placeholder={field.placeholder}
            value={value || undefined}
            onChange={val => onChange(field.key, val)}
            options={field.options?.map(o => ({ label: o, value: o }))}
          />
        );
      case 'radio':
        return (
          <Radio.Group
            value={value || undefined}
            onChange={e => onChange(field.key, e.target.value)}
          >
            <Space orientation="vertical">
              {field.options?.map(o => (
                <Radio key={o} value={o}>{o}</Radio>
              ))}
            </Space>
          </Radio.Group>
        );
      case 'checkbox':
        return (
          <Checkbox
            checked={!!value}
            onChange={e => onChange(field.key, e.target.checked)}
          >
            {field.label}
          </Checkbox>
        );
      case 'date':
        return (
          <DatePicker
            style={{ width: '100%' }}
            value={value ? dayjs(value) : null}
            onChange={date => onChange(field.key, date?.toISOString() || null)}
          />
        );
      case 'file':
        return (
          <Dragger
            name="file"
            multiple={false}
            action={`${careerApi.defaults.baseURL}/wizard/upload`}
            headers={{ Authorization: `Bearer ${localStorage.getItem('career_token')}` }}
            onChange={info => {
              if (info.file.status === 'done') {
                onChange(field.key, info.file.response?.url || info.file.name);
                messageApi.success(`${info.file.name} uploaded`);
              } else if (info.file.status === 'error') {
                messageApi.error(`${info.file.name} upload failed`);
              }
            }}
          >
            <p className="ant-upload-drag-icon"><InboxOutlined /></p>
            <p className="ant-upload-text">Click or drag {field.label} to upload</p>
            <p className="ant-upload-hint">{field.helpText}</p>
          </Dragger>
        );
      default:
        return (
          <Input
            {...sharedProps}
            value={value || ''}
            onChange={e => onChange(field.key, e.target.value)}
          />
        );
    }
  };

  // ---------- Multi-entry item ----------

  const addMultiItem = () => {
    const key = currentSection!.key;
    const newItem: Record<string, any> = {};
    const fields = STEP_FIELDS[key] || [];
    fields.forEach(f => {
      if (f.fieldType === 'checkbox') newItem[f.key] = false;
      else newItem[f.key] = null;
    });
    setMultiItems(prev => ({ ...prev, [key]: [...(prev[key] || []), newItem] }));
  };

  const updateMultiItem = (idx: number, fieldKey: string, value: any) => {
    const key = currentSection!.key;
    setMultiItems(prev => {
      const items = [...(prev[key] || [])];
      if (items[idx]) {
        items[idx] = { ...items[idx], [fieldKey]: value };
      }
      return { ...prev, [key]: items };
    });
  };

  const removeMultiItem = (idx: number) => {
    const key = currentSection!.key;
    setMultiItems(prev => ({
      ...prev,
      [key]: (prev[key] || []).filter((_, i) => i !== idx),
    }));
  };

  const saveMultiItemStep = async () => {
    const key = currentSection!.key;
    const items = multiItems[key] || [];
    if (items.length === 0) {
      messageApi.warning('Please add at least one entry');
      return;
    }
    const data: Record<string, any> = { items };
    const saved = await saveCurrentStep(data);
    if (saved) {
      if (currentStep < sections.length - 1) goToStep(currentStep + 1);
    }
  };

  // ---------- Step content ----------

  const renderStepContent = () => {
    if (!currentSection) return null;
    const key = currentSection.key;

    // Professional status step (step 1)
    if (key === 'professional-status') {
      return (
        <div>
          <Title level={4}>What best describes you?</Title>
          <Text type="secondary">Your status determines which sections you need to complete.</Text>
          <Divider />
          {renderField(STEP_FIELDS[key][0], selectedStatus, (k, v) => setSelectedStatus(v))}
        </div>
      );
    }

    // Consent / Review step
    if (key === 'consent') {
      const allData = { ...stepData };
      const allItems = { ...multiItems };
      return (
        <div>
          <Title level={4}>Review & Submit</Title>
          <Text type="secondary">Please review your application before submitting.</Text>
          <Divider />
          {Object.entries(allData).filter(([k]) => k !== 'consent').map(([sk, sd]) => {
            const section = sections.find(s => s.key === sk);
            return (
              <Card key={sk} size="small" style={{ marginBottom: 12 }}>
                <Space>
                  {SECTION_ICONS[sk]}
                  <Text strong>{section?.title || sk}</Text>
                </Space>
                <Divider style={{ margin: '8px 0' }} />
                <Row gutter={[16, 8]}>
                  {Object.entries(sd).filter(([, v]) => v !== null && v !== undefined && v !== '').map(([fk, fv]) => (
                    <Col span={12} key={fk}>
                      <Text type="secondary" style={{ fontSize: 12 }}>{fk}</Text>
                      <br />
                      <Text>{typeof fv === 'boolean' ? (fv ? 'Yes' : 'No') : String(fv)}</Text>
                    </Col>
                  ))}
                </Row>
                {allItems[sk] && allItems[sk].length > 0 && (
                  <>
                    <Divider style={{ margin: '8px 0' }} />
                    <Text type="secondary">{allItems[sk].length} item(s) added</Text>
                  </>
                )}
              </Card>
            );
          })}
          <Divider />
          {STEP_FIELDS['consent'].map(field => (
            <div key={field.key} style={{ marginBottom: 12 }}>
              {renderField(field, stepData['consent']?.[field.key], (k, v) => {
                setStepData(prev => ({
                  ...prev,
                  'consent': { ...prev['consent'], [k]: v },
                }));
              })}
            </div>
          ))}
        </div>
      );
    }

    // AI section
    if (key === 'ai-section') {
      return (
        <div>
          <Title level={4}>AI Analysis</Title>
          <Text type="secondary">AI-powered analysis of your profile will appear here after submission.</Text>
          <Divider />
          <Empty description="AI analysis will be available after you submit your application." />
        </div>
      );
    }

    // Questions section
    if (key === 'questions') {
      return (
        <div>
          <Title level={4}>Job Questions</Title>
          <Text type="secondary">Additional questions from the employer will appear here.</Text>
          <Divider />
          <Empty description="No additional questions for this position." />
        </div>
      );
    }

    // Documents section (special file upload)
    if (key === 'documents') {
      return (
        <div>
          <Title level={4}>{currentSection.title}</Title>
          <Text type="secondary">{currentSection.description}</Text>
          <Divider />
          {STEP_FIELDS[key].map(field => (
            <div key={field.key} style={{ marginBottom: 24 }}>
              <Text strong>{field.label}</Text>
              {field.isRequired && <Text type="danger"> *</Text>}
              <br />
              {field.helpText && <Text type="secondary" style={{ fontSize: 12 }}>{field.helpText}</Text>}
              <div style={{ marginTop: 8 }}>
                {renderField(field, stepData[key]?.[field.key], (k, v) => {
                  setStepData(prev => ({
                    ...prev,
                    [key]: { ...prev[key], [k]: v },
                  }));
                })}
              </div>
            </div>
          ))}
        </div>
      );
    }

    // Multi-entry steps
    if (isMultiEntry) {
      const fields = STEP_FIELDS[key] || [];
      return (
        <div>
          <Title level={4}>{currentSection.title}</Title>
          {currentSection.description && <Text type="secondary">{currentSection.description}</Text>}
          <Divider />
          <Form layout="vertical">
            {currentItems.map((item, idx) => (
              <Card
                key={idx}
                size="small"
                style={{ marginBottom: 12 }}
                title={<Space><Text strong>#{idx + 1}</Text></Space>}
                extra={
                  <Button
                    type="text"
                    danger
                    icon={<DeleteOutlined />}
                    onClick={() => removeMultiItem(idx)}
                  />
                }
              >
                <Row gutter={[16, 8]}>
                  {fields.filter(f => !['isCurrent', 'isPursuing', 'canRead', 'canWrite', 'canSpeak'].includes(f.key) || (
                    f.key === 'isCurrent' || f.key === 'isPursuing' || f.key === 'canRead' || f.key === 'canWrite' || f.key === 'canSpeak'
                  )).map(field => (
                    <Col span={field.fieldType === 'textarea' || field.fieldType === 'checkbox' ? 24 : 12} key={field.key}>
                      <Form.Item
                        label={field.label}
                        required={field.isRequired}
                      >
                        {renderField(field, item[field.key], (fk, fv) => updateMultiItem(idx, fk, fv))}
                      </Form.Item>
                    </Col>
                  ))}
                </Row>
              </Card>
            ))}
          </Form>
          <Button
            type="dashed"
            icon={<PlusOutlined />}
            onClick={addMultiItem}
            block
            style={{ marginBottom: 16 }}
          >
            Add {currentSection.title}
          </Button>
        </div>
      );
    }

    // Single-entry steps
    const fields = STEP_FIELDS[key] || [];
    return (
      <div>
        <Space style={{ marginBottom: 4 }}>
          <Tag color="#6c5ce7">Step {currentStep + 1} of {sections.length}</Tag>
        </Space>
        <Title level={4}>{currentSection.title}</Title>
        {currentSection.description && <Text type="secondary">{currentSection.description}</Text>}
        {currentStep > 0 && AUTO_FILL_STEP_KEYS.has(key) && profileData && (
          <Alert
            type="info"
            showIcon
            icon={<ThunderboltOutlined />}
            style={{ marginTop: 12, marginBottom: 8, borderRadius: 8 }}
            title="Resume data available"
            description="AI will analyze your resume and fill all sections — basic info, contact, education, experience, skills, and more."
            action={
              <Button size="small" type="primary" ghost onClick={autoFillAllSteps} icon={<ThunderboltOutlined />} loading={autoFilling}>
                {autoFilling ? 'Auto-filling...' : 'Auto-fill All from Resume'}
              </Button>
            }
          />
        )}
        <Divider />
        <Form layout="vertical">
          <Row gutter={[16, 8]}>
            {fields.map(field => (
              <Col span={field.fieldType === 'textarea' ? 24 : field.fieldType === 'checkbox' ? 24 : field.fieldType === 'radio' ? 24 : 12} key={field.key}>
                <Form.Item
                  label={<Space size={4}><span>{field.label}</span>{field.isRequired && <Text type="danger">*</Text>}</Space>}
                  help={field.helpText}
                >
                  {renderField(field, stepData[key]?.[field.key], (k, v) => {
                    setStepData(prev => ({
                      ...prev,
                      [key]: { ...prev[key] || {}, [k]: v },
                    }));
                  })}
                </Form.Item>
              </Col>
            ))}
          </Row>
        </Form>
      </div>
    );
  };

  // ---------- Loading state ----------

  if (loading && statusConfigs.length === 0) {
    return (
    <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '60vh' }}>
      <Spin size="large" description="Loading wizard..." />
    </div>
    );
  }

  // ---------- Submitted state ----------

  if (submitted) {
    return (
      <Layout className="career-wizard-layout" style={{ minHeight: '100vh', background: '#f5f5f5' }}>
        <Layout.Content style={{ maxWidth: 600, margin: '80px auto', padding: 24 }}>
          <Result
            status="success"
            title="Application Submitted!"
            subTitle="Your application has been received. We'll review it and get back to you soon."
            extra={[
              <Button type="primary" key="jobs" onClick={() => navigate('/careers')}>
                Browse More Jobs
              </Button>,
              <Button key="track" onClick={() => navigate('/careers?section=tracker')}>
                Track Application
              </Button>,
            ]}
          />
        </Layout.Content>
      </Layout>
    );
  }

  // ---------- Main render ----------

  return (
    <Layout className="career-wizard-layout" style={{ minHeight: '100vh', background: '#f0f2f5' }}>
      {/* Header */}
      <Layout.Header style={{
        background: '#fff',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: '0 24px',
        borderBottom: '1px solid #f0f0f0',
        position: 'sticky',
        top: 0,
        zIndex: 10,
      }}>
        <Space>
          <Text strong style={{ fontSize: 18, color: '#6c5ce7' }}>EWXP</Text>
          <Text type="secondary">|</Text>
          <Text>Career Application</Text>
        </Space>
        <Space>
          {completedSteps.size > 0 && (
            <Text type="secondary">
              {completedSteps.size} of {sections.length} completed
            </Text>
          )}
          <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/careers')}>
            Back to Jobs
          </Button>
        </Space>
      </Layout.Header>

      <Layout.Content style={{ maxWidth: 1200, margin: '24px auto', padding: '0 24px', width: '100%' }}>
        {/* Progress bar */}
        {sections.length > 0 && (
          <Card size="small" style={{ marginBottom: 16 }}>
            <Row align="middle" gutter={16}>
              <Col flex="auto">
                <Progress percent={progressPercent} strokeColor="#6c5ce7" format={() => `${completedSteps.size}/${sections.length}`} />
              </Col>
              <Col>
                <Text type="secondary">
                  {currentSection?.title || 'Getting started'}
                </Text>
              </Col>
            </Row>
          </Card>
        )}

        <Row gutter={24}>
          {/* Sidebar - Step navigation */}
          {sections.length > 0 && (
            <Col xs={24} md={6}>
              <Card style={{ position: 'sticky', top: 80 }}>
                <Steps
                  orientation="vertical"
                  size="small"
                  current={currentStep}
                  onChange={goToStep}
                  items={sections.map((s, idx) => ({
                    title: (
                      <div style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
                        {SECTION_ICONS[s.key]}
                        <span style={{ fontSize: 13 }}>{s.title}</span>
                        {s.isRequired && <Text type="danger" style={{ fontSize: 10 }}>*</Text>}
                      </div>
                    ),
                    status: completedSteps.has(s.key)
                      ? 'finish'
                      : idx === currentStep
                        ? 'process'
                        : idx < currentStep && !completedSteps.has(s.key)
                          ? 'error'
                          : 'wait',
                    disabled: false,
                  }))}
                />
              </Card>
            </Col>
          )}

          {/* Main content */}
          <Col xs={24} md={sections.length > 0 ? 18 : 24}>
            <Card>
              {renderStepContent()}

              <Divider />

              {/* Navigation buttons */}
              <Row justify="space-between">
                <Col>
                  {currentStep > 0 && (
                    <Button onClick={() => goToStep(currentStep - 1)} icon={<ArrowLeftOutlined />}>
                      Previous
                    </Button>
                  )}
                  {!isFirstStep && currentSection && !currentSection.isRequired && (
                    <Button
                      type="link"
                      onClick={handleSkip}
                      style={{ marginLeft: 8 }}
                    >
                      Skip this step
                    </Button>
                  )}
                </Col>
                <Col>
                  <Space>
                    {isLastStep ? (
                      <Popconfirm
                        title="Submit your application?"
                        description="Make sure all your information is correct."
                        onConfirm={handleSubmit}
                        okText="Submit"
                        cancelText="Review"
                      >
                        <Button
                          type="primary"
                          icon={<CheckCircleOutlined />}
                          loading={submitting}
                          disabled={!isAuthenticated}
                        >
                          Submit Application
                        </Button>
                      </Popconfirm>
                    ) : currentSection?.key === 'professional-status' ? (
                      <Button
                        type="primary"
                        onClick={handleNext}
                        disabled={!selectedStatus}
                        icon={<ArrowRightOutlined />}
                      >
                        Continue
                      </Button>
                    ) : isMultiEntry ? (
                      <Button
                        type="primary"
                        onClick={saveMultiItemStep}
                        loading={saving}
                        icon={<SaveOutlined />}
                      >
                        Save & Next
                      </Button>
                    ) : (
                      <Button
                        type="primary"
                        onClick={handleSaveAndNext}
                        loading={saving}
                        icon={<SaveOutlined />}
                      >
                        Save & Next
                      </Button>
                    )}
                  </Space>
                </Col>
              </Row>
            </Card>
          </Col>
        </Row>
      </Layout.Content>

      {/* Auth Modal */}
      <Modal
        title={authMode === 'login' ? 'Login to Apply' : 'Create Account to Apply'}
        open={authModalOpen}
        onCancel={() => setAuthModalOpen(false)}
        footer={null}
        destroyOnHidden
      >
        <Form form={authForm} layout="vertical" onFinish={handleAuth}>
          {authMode === 'register' && (
            <>
              <Form.Item name="firstName" label="First Name" rules={[{ required: true }]}>
                <Input placeholder="First name" />
              </Form.Item>
              <Form.Item name="lastName" label="Last Name" rules={[{ required: true }]}>
                <Input placeholder="Last name" />
              </Form.Item>
            </>
          )}
          <Form.Item name="email" label="Email" rules={[{ required: true, type: 'email' }]}>
            <Input placeholder="your@email.com" />
          </Form.Item>
          <Form.Item name="password" label="Password" rules={[{ required: true, min: 6 }]}>
            <Input.Password placeholder="Password" />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" block>
              {authMode === 'login' ? 'Login' : 'Create Account'}
            </Button>
          </Form.Item>
          <Form.Item style={{ textAlign: 'center', marginBottom: 0 }}>
            <Button
              type="link"
              onClick={() => setAuthMode(authMode === 'login' ? 'register' : 'login')}
            >
              {authMode === 'login' ? 'Don\'t have an account? Register' : 'Already have an account? Login'}
            </Button>
          </Form.Item>
        </Form>
      </Modal>

      {/* AI auto-fill loading overlay */}
      {autoFilling && (
        <div style={{
          position: 'fixed', inset: 0, zIndex: 9999,
          background: 'rgba(255,255,255,0.92)',
          display: 'flex', flexDirection: 'column',
          alignItems: 'center', justifyContent: 'center',
        }}>
          <div style={{ position: 'relative', width: 56, height: 56, marginBottom: 24 }}>
            {[0,1,2,3].map(i => (
              <div key={i} style={{
                position: 'absolute', inset: 0,
                border: '3px solid transparent',
                borderTopColor: '#6c5ce7',
                borderRadius: '50%',
                animation: `ai-spin ${1.2 + i*0.15}s linear infinite`,
                animationDelay: `${i*0.2}s`,
              }} />
            ))}
          </div>
          <div style={{ fontSize: 20, fontWeight: 600, color: '#6c5ce7', marginBottom: 8 }}>
            AI is analyzing your resume
            <span style={{ animation: 'ai-dots 1.4s infinite' }}>.</span>
            <span style={{ animation: 'ai-dots 1.4s infinite 0.2s' }}>.</span>
            <span style={{ animation: 'ai-dots 1.4s infinite 0.4s' }}>.</span>
          </div>
          <Text type="secondary" style={{ fontSize: 14 }}>
            Extracting and filling your profile data
          </Text>
          <style>{`
            @keyframes ai-spin { to { transform: rotate(360deg); } }
            @keyframes ai-dots { 0%,20% { opacity: 0; } 50% { opacity: 1; } 100% { opacity: 0; } }
          `}</style>
        </div>
      )}
    </Layout>
  );
}
