using Microsoft.EntityFrameworkCore;
using HRMS.API.Models.Entities;

namespace HRMS.API.Data;

public class HRMSDbContext : DbContext
{
    public HRMSDbContext(DbContextOptions<HRMSDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<AttendanceCorrection> AttendanceCorrections => Set<AttendanceCorrection>();
    public DbSet<PerformanceReview> PerformanceReviews => Set<PerformanceReview>();
    public DbSet<ReviewCycle> ReviewCycles => Set<ReviewCycle>();
    public DbSet<ReviewScore> ReviewScores => Set<ReviewScore>();
    public DbSet<PayrollStructure> PayrollStructures => Set<PayrollStructure>();
    public DbSet<SalaryComponent> SalaryComponents => Set<SalaryComponent>();
    public DbSet<EmployeePayroll> EmployeePayrolls => Set<EmployeePayroll>();
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<CandidateMasterProfile> CandidateMasterProfiles => Set<CandidateMasterProfile>();
    public DbSet<JobRequisition> JobRequisitions => Set<JobRequisition>();
    public DbSet<InterviewSchedule> InterviewSchedules => Set<InterviewSchedule>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AIUsageLog> AIUsageLogs => Set<AIUsageLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<FormDefinition> FormDefinitions => Set<FormDefinition>();
    public DbSet<FormSubmission> FormSubmissions => Set<FormSubmission>();
    public DbSet<CustomField> CustomFields => Set<CustomField>();
    public DbSet<CustomFieldValue> CustomFieldValues => Set<CustomFieldValue>();
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<ReportDefinition> ReportDefinitions => Set<ReportDefinition>();
    public DbSet<SocialPost> SocialPosts => Set<SocialPost>();
    public DbSet<PostComment> PostComments => Set<PostComment>();
    public DbSet<PostLike> PostLikes => Set<PostLike>();
    public DbSet<ExpenseReport> ExpenseReports => Set<ExpenseReport>();
    public DbSet<ExpenseLineItem> ExpenseLineItems => Set<ExpenseLineItem>();
    public DbSet<HiringRequest> HiringRequests => Set<HiringRequest>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<BackgroundCheck> BackgroundChecks => Set<BackgroundCheck>();
    public DbSet<OnboardingTask> OnboardingTasks => Set<OnboardingTask>();
    public DbSet<PromotionRequest> PromotionRequests => Set<PromotionRequest>();
    public DbSet<TransferRequest> TransferRequests => Set<TransferRequest>();
    public DbSet<Poll> Polls => Set<Poll>();
    public DbSet<PollOption> PollOptions => Set<PollOption>();
    public DbSet<PollVote> PollVotes => Set<PollVote>();
    public DbSet<DiscussionThread> DiscussionThreads => Set<DiscussionThread>();
    public DbSet<DiscussionReply> DiscussionReplies => Set<DiscussionReply>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<EmployeeSkill> EmployeeSkills => Set<EmployeeSkill>();
    public DbSet<TalentPool> TalentPools => Set<TalentPool>();
    public DbSet<TalentPoolCandidate> TalentPoolCandidates => Set<TalentPoolCandidate>();
    public DbSet<PositionSkillRequirement> PositionSkillRequirements => Set<PositionSkillRequirement>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetAllocation> AssetAllocations => Set<AssetAllocation>();
    public DbSet<AssetMaintenance> AssetMaintenances => Set<AssetMaintenance>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<TrainingEnrollment> TrainingEnrollments => Set<TrainingEnrollment>();
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<EmployeeCertification> EmployeeCertifications => Set<EmployeeCertification>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ComplianceRecord> ComplianceRecords => Set<ComplianceRecord>();
    public DbSet<DataPrivacyLog> DataPrivacyLogs => Set<DataPrivacyLog>();
    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<MeetingParticipant> MeetingParticipants => Set<MeetingParticipant>();
    public DbSet<WorkTask> WorkTasks => Set<WorkTask>();
    public DbSet<CollaborationChannel> CollaborationChannels => Set<CollaborationChannel>();
    public DbSet<ChannelMessage> ChannelMessages => Set<ChannelMessage>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<AnnouncementRead> AnnouncementReads => Set<AnnouncementRead>();
    public DbSet<CandidateProfileExtended> CandidateProfileExtendeds => Set<CandidateProfileExtended>();
    public DbSet<CandidateEducation> CandidateEducations => Set<CandidateEducation>();
    public DbSet<CandidateExperience> CandidateExperiences => Set<CandidateExperience>();
    public DbSet<CandidateProject> CandidateProjects => Set<CandidateProject>();
    public DbSet<CandidateSkill> CandidateSkills => Set<CandidateSkill>();
    public DbSet<CandidateCertification> CandidateCertifications => Set<CandidateCertification>();
    public DbSet<CandidateSocialProfile> CandidateSocialProfiles => Set<CandidateSocialProfile>();
    public DbSet<CandidateAchievement> CandidateAchievements => Set<CandidateAchievement>();
    public DbSet<CandidateLanguage> CandidateLanguages => Set<CandidateLanguage>();
    public DbSet<CandidateDocument> CandidateDocuments => Set<CandidateDocument>();
    public DbSet<CandidateReference> CandidateReferences => Set<CandidateReference>();
    public DbSet<CandidateTraining> CandidateTrainings => Set<CandidateTraining>();
    public DbSet<CandidateAnswer> CandidateAnswers => Set<CandidateAnswer>();
    public DbSet<DynamicFormSection> DynamicFormSections => Set<DynamicFormSection>();
    public DbSet<DynamicFormField> DynamicFormFields => Set<DynamicFormField>();
    public DbSet<CandidateStepData> CandidateStepDatas => Set<CandidateStepData>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Employee)
            .WithOne(e => e.User)
            .HasForeignKey<User>(u => u.EmployeeId);

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.EmployeeCode)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Email)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Manager)
            .WithMany()
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Department>()
            .HasOne(d => d.Head)
            .WithMany()
            .HasForeignKey(d => d.HeadId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LeaveRequest>()
            .HasOne(lr => lr.Employee)
            .WithMany()
            .HasForeignKey(lr => lr.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LeaveBalance>()
            .HasIndex(lb => new { lb.EmployeeId, lb.LeaveTypeId, lb.Year })
            .IsUnique();

        modelBuilder.Entity<PerformanceReview>()
            .HasOne(pr => pr.Reviewer)
            .WithMany()
            .HasForeignKey(pr => pr.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CandidateProfile>()
            .HasOne(cp => cp.JobRequisition)
            .WithMany(jr => jr.Candidates)
            .HasForeignKey(cp => cp.JobRequisitionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CandidateProfile>()
            .HasOne(cp => cp.User)
            .WithMany(u => u.CandidateProfiles)
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<SystemSetting>()
            .HasIndex(ss => ss.Key)
            .IsUnique();

        modelBuilder.Entity<CandidateProfile>()
            .Property(cp => cp.MatchScore)
            .HasPrecision(5, 2);

        modelBuilder.Entity<LeaveBalance>()
            .Property(lb => lb.TotalDays)
            .HasPrecision(5, 1);

        modelBuilder.Entity<LeaveBalance>()
            .Property(lb => lb.UsedDays)
            .HasPrecision(5, 1);

        modelBuilder.Entity<PerformanceReview>()
            .Property(pr => pr.OverallScore)
            .HasPrecision(5, 2);

        modelBuilder.Entity<ReviewScore>()
            .Property(rs => rs.Score)
            .HasPrecision(5, 2);

        modelBuilder.Entity<SalaryComponent>()
            .Property(sc => sc.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ExpenseReport>()
            .Property(er => er.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ExpenseLineItem>()
            .Property(eli => eli.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<HiringRequest>()
            .HasOne(hr => hr.RequestedBy)
            .WithMany()
            .HasForeignKey(hr => hr.RequestedById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HiringRequest>()
            .HasOne(hr => hr.Department)
            .WithMany()
            .HasForeignKey(hr => hr.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<HiringRequest>()
            .HasOne(hr => hr.JobRequisition)
            .WithMany()
            .HasForeignKey(hr => hr.JobRequisitionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Offer>()
            .HasOne(o => o.Candidate)
            .WithMany()
            .HasForeignKey(o => o.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Offer>()
            .HasOne(o => o.ApprovedBy)
            .WithMany()
            .HasForeignKey(o => o.ApprovedById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BackgroundCheck>()
            .HasOne(bc => bc.Candidate)
            .WithMany()
            .HasForeignKey(bc => bc.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OnboardingTask>()
            .HasOne(ot => ot.Candidate)
            .WithMany()
            .HasForeignKey(ot => ot.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HiringRequest>()
            .Property(hr => hr.BudgetRangeLow)
            .HasPrecision(18, 2);
        modelBuilder.Entity<HiringRequest>()
            .Property(hr => hr.BudgetRangeHigh)
            .HasPrecision(18, 2);
        modelBuilder.Entity<Offer>()
            .Property(o => o.Salary)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PromotionRequest>()
            .Property(pr => pr.CurrentSalary).HasPrecision(18, 2);
        modelBuilder.Entity<PromotionRequest>()
            .Property(pr => pr.ProposedSalary).HasPrecision(18, 2);

        modelBuilder.Entity<PromotionRequest>()
            .HasOne(pr => pr.Employee).WithMany().HasForeignKey(pr => pr.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<PromotionRequest>()
            .HasOne(pr => pr.RequestedBy).WithMany().HasForeignKey(pr => pr.RequestedById).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<PromotionRequest>()
            .HasOne(pr => pr.ApprovedByManager).WithMany().HasForeignKey(pr => pr.ApprovedByManagerId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<PromotionRequest>()
            .HasOne(pr => pr.ApprovedByHrbp).WithMany().HasForeignKey(pr => pr.ApprovedByHrbpId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<PromotionRequest>()
            .HasOne(pr => pr.ApprovedByDeptHead).WithMany().HasForeignKey(pr => pr.ApprovedByDeptHeadId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<PromotionRequest>()
            .HasOne(pr => pr.ApprovedByCeo).WithMany().HasForeignKey(pr => pr.ApprovedByCeoId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TransferRequest>()
            .HasOne(tr => tr.Employee).WithMany().HasForeignKey(tr => tr.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TransferRequest>()
            .HasOne(tr => tr.RequestedBy).WithMany().HasForeignKey(tr => tr.RequestedById).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TransferRequest>()
            .HasOne(tr => tr.CurrentDepartment).WithMany().HasForeignKey(tr => tr.CurrentDepartmentId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TransferRequest>()
            .HasOne(tr => tr.ProposedDepartment).WithMany().HasForeignKey(tr => tr.ProposedDepartmentId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TransferRequest>()
            .HasOne(tr => tr.ApprovedByManager).WithMany().HasForeignKey(tr => tr.ApprovedByManagerId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TransferRequest>()
            .HasOne(tr => tr.ApprovedByHr).WithMany().HasForeignKey(tr => tr.ApprovedByHrId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TransferRequest>()
            .HasOne(tr => tr.ApprovedByDept).WithMany().HasForeignKey(tr => tr.ApprovedByDeptId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TransferRequest>()
            .HasOne(tr => tr.ApprovedByIt).WithMany().HasForeignKey(tr => tr.ApprovedByItId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TransferRequest>()
            .HasOne(tr => tr.ApprovedByPayroll).WithMany().HasForeignKey(tr => tr.ApprovedByPayrollId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Poll>()
            .HasOne(p => p.CreatedBy).WithMany().HasForeignKey(p => p.CreatedById).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<PollOption>()
            .HasOne(po => po.Poll).WithMany(p => p.Options).HasForeignKey(po => po.PollId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PollVote>()
            .HasOne(pv => pv.PollOption).WithMany(po => po.Votes).HasForeignKey(pv => pv.PollOptionId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PollVote>()
            .HasOne(pv => pv.User).WithMany().HasForeignKey(pv => pv.UserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<PollVote>()
            .HasIndex(pv => new { pv.PollOptionId, pv.UserId }).IsUnique();

        modelBuilder.Entity<SocialPost>()
            .HasOne(sp => sp.Author).WithMany().HasForeignKey(sp => sp.AuthorId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PostComment>()
            .HasOne(pc => pc.Author).WithMany().HasForeignKey(pc => pc.AuthorId).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<PostComment>()
            .HasOne(pc => pc.Post).WithMany(sp => sp.Comments).HasForeignKey(pc => pc.PostId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PostLike>()
            .HasOne(pl => pl.Post).WithMany(sp => sp.Likes).HasForeignKey(pl => pl.PostId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DiscussionThread>()
            .HasOne(dt => dt.CreatedBy).WithMany().HasForeignKey(dt => dt.CreatedById).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<DiscussionReply>()
            .HasOne(dr => dr.Thread).WithMany(t => t.Replies).HasForeignKey(dr => dr.ThreadId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<DiscussionReply>()
            .HasOne(dr => dr.CreatedBy).WithMany().HasForeignKey(dr => dr.CreatedById).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EmployeeSkill>()
            .HasOne(es => es.Employee).WithMany().HasForeignKey(es => es.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<EmployeeSkill>()
            .HasOne(es => es.Skill).WithMany().HasForeignKey(es => es.SkillId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<EmployeeSkill>()
            .HasIndex(es => new { es.EmployeeId, es.SkillId }).IsUnique();

        modelBuilder.Entity<PositionSkillRequirement>()
            .HasOne(psr => psr.Skill).WithMany().HasForeignKey(psr => psr.SkillId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PositionSkillRequirement>()
            .HasIndex(psr => new { psr.Position, psr.SkillId }).IsUnique();

        modelBuilder.Entity<TalentPool>()
            .HasOne(tp => tp.CreatedBy).WithMany().HasForeignKey(tp => tp.CreatedById).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TalentPoolCandidate>()
            .HasOne(tpc => tpc.Pool).WithMany(tp => tp.Candidates).HasForeignKey(tpc => tpc.TalentPoolId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TalentPoolCandidate>()
            .HasOne(tpc => tpc.Employee).WithMany().HasForeignKey(tpc => tpc.EmployeeId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Asset>()
            .HasIndex(a => a.AssetTag).IsUnique();
        modelBuilder.Entity<Asset>()
            .Property(a => a.PurchasePrice).HasPrecision(18, 2);

        modelBuilder.Entity<AssetAllocation>()
            .HasOne(aa => aa.Asset).WithMany(a => a.Allocations).HasForeignKey(aa => aa.AssetId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AssetAllocation>()
            .HasOne(aa => aa.Employee).WithMany().HasForeignKey(aa => aa.EmployeeId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AssetMaintenance>()
            .HasOne(am => am.Asset).WithMany(a => a.MaintenanceRecords).HasForeignKey(am => am.AssetId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AssetMaintenance>()
            .Property(am => am.Cost).HasPrecision(18, 2);

        modelBuilder.Entity<Course>()
            .HasOne(c => c.CreatedBy).WithMany().HasForeignKey(c => c.CreatedById).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<TrainingEnrollment>()
            .HasOne(te => te.Course).WithMany(c => c.Enrollments).HasForeignKey(te => te.CourseId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TrainingEnrollment>()
            .HasOne(te => te.Employee).WithMany().HasForeignKey(te => te.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TrainingEnrollment>()
            .HasIndex(te => new { te.CourseId, te.EmployeeId }).IsUnique();
        modelBuilder.Entity<TrainingEnrollment>()
            .Property(te => te.Score).HasPrecision(5, 2);
        modelBuilder.Entity<EmployeeCertification>()
            .HasOne(ec => ec.Employee).WithMany().HasForeignKey(ec => ec.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<EmployeeCertification>()
            .HasOne(ec => ec.Certification).WithMany().HasForeignKey(ec => ec.CertificationId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<EmployeeCertification>()
            .HasIndex(ec => new { ec.EmployeeId, ec.CertificationId }).IsUnique();

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User).WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.IsRead });

        modelBuilder.Entity<Document>()
            .HasOne(d => d.UploadedBy).WithMany().HasForeignKey(d => d.UploadedById).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Document>()
            .HasOne(d => d.Employee).WithMany().HasForeignKey(d => d.EmployeeId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ComplianceRecord>()
            .HasOne(cr => cr.AssignedTo).WithMany().HasForeignKey(cr => cr.AssignedToId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<ComplianceRecord>()
            .HasOne(cr => cr.CompletedBy).WithMany().HasForeignKey(cr => cr.CompletedById).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DataPrivacyLog>()
            .HasOne(dpl => dpl.Employee).WithMany().HasForeignKey(dpl => dpl.EmployeeId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<DataPrivacyLog>()
            .HasOne(dpl => dpl.User).WithMany().HasForeignKey(dpl => dpl.UserId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Meeting>()
            .HasOne(m => m.Organizer).WithMany().HasForeignKey(m => m.OrganizerId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Meeting>()
            .HasOne(m => m.Department).WithMany().HasForeignKey(m => m.DepartmentId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<MeetingParticipant>()
            .HasOne(mp => mp.Meeting).WithMany(m => m.Participants).HasForeignKey(mp => mp.MeetingId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<MeetingParticipant>()
            .HasOne(mp => mp.Employee).WithMany().HasForeignKey(mp => mp.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<MeetingParticipant>()
            .HasIndex(mp => new { mp.MeetingId, mp.EmployeeId }).IsUnique();

        modelBuilder.Entity<WorkTask>()
            .HasOne(t => t.AssignedTo).WithMany().HasForeignKey(t => t.AssignedToId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<WorkTask>()
            .HasOne(t => t.AssignedBy).WithMany().HasForeignKey(t => t.AssignedById).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<WorkTask>()
            .HasOne(t => t.Meeting).WithMany(m => m.Tasks).HasForeignKey(t => t.MeetingId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CollaborationChannel>()
            .HasOne(c => c.Department).WithMany().HasForeignKey(c => c.DepartmentId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<ChannelMessage>()
            .HasOne(cm => cm.Channel).WithMany(c => c.Messages).HasForeignKey(cm => cm.ChannelId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ChannelMessage>()
            .HasOne(cm => cm.Author).WithMany().HasForeignKey(cm => cm.AuthorId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Announcement>()
            .HasOne(a => a.CreatedBy).WithMany().HasForeignKey(a => a.CreatedById).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Announcement>()
            .HasOne(a => a.Department).WithMany().HasForeignKey(a => a.DepartmentId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<AnnouncementRead>()
            .HasOne(ar => ar.Announcement).WithMany(a => a.Reads).HasForeignKey(ar => ar.AnnouncementId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AnnouncementRead>()
            .HasOne(ar => ar.User).WithMany().HasForeignKey(ar => ar.UserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<AnnouncementRead>()
            .HasIndex(ar => new { ar.AnnouncementId, ar.UserId }).IsUnique();

        // Candidate Master Profile
        modelBuilder.Entity<CandidateMasterProfile>()
            .HasOne(mp => mp.User).WithMany().HasForeignKey(mp => mp.UserId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CandidateMasterProfile>()
            .HasIndex(mp => mp.UserId).IsUnique();
        modelBuilder.Entity<CandidateMasterProfile>()
            .Property(mp => mp.CurrentCtc).HasPrecision(18, 2);
        modelBuilder.Entity<CandidateMasterProfile>()
            .Property(mp => mp.ExpectedCtc).HasPrecision(18, 2);

        // Candidate Extended
        modelBuilder.Entity<CandidateProfileExtended>()
            .HasOne(e => e.Candidate).WithOne().HasForeignKey<CandidateProfileExtended>(e => e.CandidateProfileId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CandidateProfileExtended>()
            .Property(e => e.CurrentCtc).HasPrecision(18, 2);
        modelBuilder.Entity<CandidateProfileExtended>()
            .Property(e => e.ExpectedCtc).HasPrecision(18, 2);
        modelBuilder.Entity<CandidateEducation>()
            .HasOne(e => e.Candidate).WithMany().HasForeignKey(e => e.CandidateProfileId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CandidateEducation>()
            .Property(e => e.Cgpa).HasPrecision(5, 2);
        modelBuilder.Entity<CandidateEducation>()
            .Property(e => e.Percentage).HasPrecision(5, 2);
        modelBuilder.Entity<CandidateExperience>()
            .HasOne(e => e.Candidate).WithMany().HasForeignKey(e => e.CandidateProfileId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CandidateExperience>()
            .Property(e => e.Salary).HasPrecision(18, 2);
        modelBuilder.Entity<CandidateProject>()
            .HasOne(e => e.Candidate).WithMany().HasForeignKey(e => e.CandidateProfileId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CandidateSkill>()
            .HasOne(e => e.Candidate).WithMany().HasForeignKey(e => e.CandidateProfileId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CandidateCertification>()
            .HasOne(e => e.Candidate).WithMany().HasForeignKey(e => e.CandidateProfileId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CandidateSocialProfile>()
            .HasOne(e => e.Candidate).WithMany().HasForeignKey(e => e.CandidateProfileId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CandidateAchievement>()
            .HasOne(e => e.Candidate).WithMany().HasForeignKey(e => e.CandidateProfileId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CandidateLanguage>()
            .HasOne(e => e.Candidate).WithMany().HasForeignKey(e => e.CandidateProfileId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CandidateDocument>()
            .HasOne(e => e.Candidate).WithMany().HasForeignKey(e => e.CandidateProfileId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CandidateReference>()
            .HasOne(e => e.Candidate).WithMany().HasForeignKey(e => e.CandidateProfileId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CandidateTraining>()
            .HasOne(e => e.Candidate).WithMany().HasForeignKey(e => e.CandidateProfileId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CandidateAnswer>()
            .HasOne(e => e.Candidate).WithMany().HasForeignKey(e => e.CandidateProfileId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CandidateAnswer>()
            .HasOne(e => e.Job).WithMany().HasForeignKey(e => e.JobRequisitionId).OnDelete(DeleteBehavior.Cascade);

        // Dynamic Form Config
        modelBuilder.Entity<DynamicFormField>()
            .HasOne(f => f.Section).WithMany().HasForeignKey(f => f.SectionId).OnDelete(DeleteBehavior.Cascade);
    }
}
