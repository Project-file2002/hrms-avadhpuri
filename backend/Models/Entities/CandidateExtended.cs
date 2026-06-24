namespace HRMS.API.Models.Entities;

public class CandidateProfileExtended
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }
    public CandidateProfile Candidate { get; set; } = null!;

    public string? ProfessionalStatus { get; set; }

    // Step 1 — Basic
    public string? MiddleName { get; set; }
    public string? PreferredName { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string? MaritalStatus { get; set; }
    public string? ProfilePhotoPath { get; set; }
    public string? AlternateEmail { get; set; }
    public string? AlternatePhone { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? CurrentAddress { get; set; }
    public string? PermanentAddress { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
    public string? NearestLandmark { get; set; }

    // Step 2 — Professional
    public string? CurrentCompany { get; set; }
    public string? EmployeeId { get; set; }
    public string? CurrentDesignation { get; set; }
    public string? Department { get; set; }
    public string? EmploymentType { get; set; }
    public decimal? CurrentCtc { get; set; }
    public decimal? ExpectedCtc { get; set; }
    public bool? Negotiable { get; set; }
    public string? NoticePeriod { get; set; }
    public bool? ImmediateJoiner { get; set; }
    public DateTime? LastWorkingDay { get; set; }
    public string? CurrentLocation { get; set; }
    public string? PreferredLocation { get; set; }
    public string? PreferredWorkMode { get; set; }
    public int? TotalExperienceMonths { get; set; }
    public int? RelevantExperienceMonths { get; set; }
    public int? LeadershipExperienceMonths { get; set; }
    public int? TeamSizeManaged { get; set; }
    public string? EmploymentTypePreferred { get; set; }
    public string? ShiftPreference { get; set; }
    public bool? WillingToRelocate { get; set; }
    public string? PreferredCities { get; set; }
    public string? WillingToTravel { get; set; }

    // Step 6 — Resume
    public string? ResumeOriginalPath { get; set; }
    public string? ResumeVersion { get; set; }
    public string? CoverLetterText { get; set; }

    // Step 8 — Work Authorization
    public string? CitizenshipStatus { get; set; }
    public string? VisaType { get; set; }
    public string? WorkPermit { get; set; }
    public bool? SponsorshipRequired { get; set; }
    public string? PassportNumber { get; set; }
    public DateTime? PassportExpiry { get; set; }

    // Step 10 — Emergency Contact
    public string? EmergencyName { get; set; }
    public string? EmergencyRelationship { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? EmergencyEmail { get; set; }
    public string? EmergencyAddress { get; set; }

    // Step 11 — Availability
    public DateTime? AvailableFrom { get; set; }
    public string? InterviewAvailability { get; set; }
    public string? PreferredTime { get; set; }
    public string? Timezone { get; set; }

    // Step 12 — Consent
    public bool? ConsentInfoAccurate { get; set; }
    public bool? ConsentPrivacyPolicy { get; set; }
    public bool? ConsentResumeProcessing { get; set; }
    public bool? ConsentBackgroundVerification { get; set; }
    public bool? ConsentCommunication { get; set; }
    public string? DigitalSignature { get; set; }
    public string? DiversityDisability { get; set; }
    public string? DiversityVeteran { get; set; }
    public string? DiversityGenderIdentity { get; set; }
    public string? DiversityPronouns { get; set; }
    public string? DiversityEthnicity { get; set; }
    public string? DiversityReligion { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class CandidateEducation
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }
    public CandidateProfile Candidate { get; set; } = null!;
    public string Level { get; set; } = string.Empty; // 10th, 12th, Diploma, Graduation, Masters, PhD
    public string? Institute { get; set; }
    public string? Degree { get; set; }
    public string? Specialization { get; set; }
    public string? BoardOrUniversity { get; set; }
    public int? PassingYear { get; set; }
    public decimal? Cgpa { get; set; }
    public decimal? Percentage { get; set; }
    public string? Grade { get; set; }
    public bool? IsCurrent { get; set; }
    public string? Achievements { get; set; }
    public string? Certificates { get; set; }
    public bool? HasGap { get; set; }
    public string? GapReason { get; set; }
}

public class CandidateExperience
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }
    public CandidateProfile Candidate { get; set; } = null!;
    public string CompanyName { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public DateTime? JoiningDate { get; set; }
    public DateTime? LeavingDate { get; set; }
    public bool IsCurrent { get; set; }
    public string? Location { get; set; }
    public string? EmploymentType { get; set; }
    public string? Industry { get; set; }
    public string? ManagerName { get; set; }
    public int? TeamSize { get; set; }
    public decimal? Salary { get; set; }
    public string? ReasonForLeaving { get; set; }
    public string? Projects { get; set; }
    public string? Responsibilities { get; set; }
    public string? Achievements { get; set; }
    public string? TechnologiesUsed { get; set; }
}

public class CandidateProject
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }
    public CandidateProfile Candidate { get; set; } = null!;
    public string ProjectName { get; set; } = string.Empty;
    public string? Client { get; set; }
    public string? Description { get; set; }
    public string? Duration { get; set; }
    public string? Role { get; set; }
    public string? Responsibilities { get; set; }
    public int? TeamSize { get; set; }
    public string? TechnologyStack { get; set; }
    public string? GitHubUrl { get; set; }
    public string? LiveDemoUrl { get; set; }
    public string? Achievements { get; set; }
    public string? Challenges { get; set; }
    public string? Outcome { get; set; }
    public string? ScreenshotsPath { get; set; }
    public string? DemoVideoUrl { get; set; }
}

public class CandidateSkill
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }
    public CandidateProfile Candidate { get; set; } = null!;
    public string SkillName { get; set; } = string.Empty;
    public string Category { get; set; } = "Technical"; // Technical, Soft, Language, Framework, Database, Cloud, DevOps, Testing, AI, VersionControl, OS, Methodology
    public int ProficiencyLevel { get; set; } = 1; // 1-5
    public int? ExperienceMonths { get; set; }
    public int? ProjectCount { get; set; }
    public int? SelfRating { get; set; }
    public bool? Certified { get; set; }
    public string? LastUsed { get; set; }
}

public class CandidateCertification
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }
    public CandidateProfile Candidate { get; set; } = null!;
    public string CertificationName { get; set; } = string.Empty;
    public string? Issuer { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? CredentialId { get; set; }
    public string? CredentialUrl { get; set; }
    public string? CertificatePath { get; set; }
}

public class CandidateSocialProfile
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }
    public CandidateProfile Candidate { get; set; } = null!;
    public string Platform { get; set; } = string.Empty; // LinkedIn, GitHub, GitLab, LeetCode, etc.
    public string Url { get; set; } = string.Empty;
    public string? Username { get; set; }
}

public class CandidateAchievement
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }
    public CandidateProfile Candidate { get; set; } = null!;
    public string Type { get; set; } = string.Empty; // Award, Hackathon, Competition, Paper, Patent, Scholarship, Olympiad, Publication
    public string? Title { get; set; }
    public string? Organization { get; set; }
    public int? Year { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
}

public class CandidateLanguage
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }
    public CandidateProfile Candidate { get; set; } = null!;
    public string Language { get; set; } = string.Empty;
    public bool? CanRead { get; set; }
    public bool? CanWrite { get; set; }
    public bool? CanSpeak { get; set; }
    public string? Proficiency { get; set; } // Beginner, Intermediate, Advanced, Native
}

public class CandidateDocument
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }
    public CandidateProfile Candidate { get; set; } = null!;
    public string DocType { get; set; } = string.Empty; // Resume, PAN, Aadhaar, Passport, DL, VoterID, Certificate
    public string FileName { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? DocNumber { get; set; }
    public bool? Verified { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

public class CandidateReference
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }
    public CandidateProfile Candidate { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Designation { get; set; }
    public string? Relationship { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int? YearsKnown { get; set; }
}

public class CandidateTraining
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }
    public CandidateProfile Candidate { get; set; } = null!;
    public string TrainingName { get; set; } = string.Empty;
    public string? Provider { get; set; } // Coursera, Udemy, NPTEL, Bootcamp, etc.
    public string? Duration { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? CertificatePath { get; set; }
    public string? SkillsLearned { get; set; }
}

public class CandidateAnswer
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }
    public CandidateProfile Candidate { get; set; } = null!;
    public int JobRequisitionId { get; set; }
    public JobRequisition Job { get; set; } = null!;
    public string QuestionText { get; set; } = string.Empty;
    public string? Answer { get; set; }
    public string? QuestionType { get; set; } // Text, YesNo, Select, Number
}

public class CandidateAvailability
{
    public int Id { get; set; }
    public int CandidateProfileId { get; set; }
    public CandidateProfile Candidate { get; set; } = null!;
    public string? PreferredRoles { get; set; }
    public string? PreferredTechnology { get; set; }
    public string? ExpectedSalaryRange { get; set; }
    public DateTime? JoiningDate { get; set; }
    public string? InterviewAvailability { get; set; }
    public string? PreferredTime { get; set; }
    public string? Timezone { get; set; }
}

// ======== DYNAMIC FORM CONFIG ========

public class DynamicFormSection
{
    public int Id { get; set; }
    public string SectionKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string ApplicableStatuses { get; set; } = "all"; // comma-delimited: Student,Fresher,Working
    public string? ConditionalLogic { get; set; } // JSON: depends on which field has what value
}

public class DynamicFormField
{
    public int Id { get; set; }
    public int SectionId { get; set; }
    public DynamicFormSection Section { get; set; } = null!;
    public string FieldKey { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string FieldType { get; set; } = "text"; // text, number, email, select, textarea, date, file, checkbox, radio, tel
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public string? Options { get; set; } // JSON array for select/radio
    public string? Placeholder { get; set; }
    public string? ValidationRegex { get; set; }
    public string? DefaultValue { get; set; }
    public string? ConditionalShow { get; set; } // JSON: depends on fieldKey + value
    public string? ApplicableStatuses { get; set; } = "all";
}

public class CandidateStepData
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    public string StepKey { get; set; } = string.Empty;
    public string JsonData { get; set; } = "{}";
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}
