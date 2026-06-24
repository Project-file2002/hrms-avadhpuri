using HRMS.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(HRMSDbContext context)
    {
        await SeedRoles(context);
        await EnsureCandidateRole(context);
        await EnsureDemoCandidateUser(context);
        await SeedLeaveTypes(context);
        await SeedDepartments(context);
        await SeedEmployees(context);
        await SeedLeaveBalances(context);
        await SeedLeaveRequests(context);
        await SeedAttendanceRecords(context);
        await SeedReviewCycles(context);
        await SeedPayroll(context);
        await SeedJobRequisitions(context);
        await SeedCandidates(context);
        await SeedUsers(context);
        await SeedSocialPosts(context);
        await SeedPromotions(context);
        await SeedTransfers(context);
        await SeedPolls(context);
        await SeedDiscussions(context);
        await SeedSkills(context);
        await SeedPositionSkillRequirements(context);
        await SeedAssets(context);
        await SeedTraining(context);
        await SeedNotifications(context);
        await SeedDocuments(context);
        await SeedCompliance(context);
        await SeedExpenseReports(context);
        await SeedNoCodePlatform(context);
        await SeedRecruitmentPipeline(context);
        await SeedTalentPools(context);
        await SeedReviewScores(context);
        await SeedDataPrivacyLogs(context);
        await SeedSystemSettings(context);
        await SeedWorkplacePlatform(context);
    }

    private static async Task SeedRoles(HRMSDbContext context)
    {
        if (await context.Roles.AnyAsync()) return;
        context.Roles.AddRange(
            new Role { Name = "Administrator", Description = "Full system access" },
            new Role { Name = "HRManager", Description = "HR management access" },
            new Role { Name = "Manager", Description = "Team management access" },
            new Role { Name = "Employee", Description = "Basic employee access" },
            new Role { Name = "PayrollStaff", Description = "Payroll data access" },
            new Role { Name = "Candidate", Description = "External job applicant portal access" }
        );
        await context.SaveChangesAsync();
    }

    private static async Task EnsureCandidateRole(HRMSDbContext context)
    {
        if (await context.Roles.AnyAsync(r => r.Name == "Candidate")) return;
        context.Roles.Add(new Role { Name = "Candidate", Description = "External job applicant portal access" });
        await context.SaveChangesAsync();
    }

    private static async Task EnsureDemoCandidateUser(HRMSDbContext context)
    {
        const string email = "candidate@demo.com";
        if (await context.Users.AnyAsync(u => u.Email == email)) return;

        var candidateRole = await context.Roles.FirstAsync(r => r.Name == "Candidate");
        var pwd = BCrypt.Net.BCrypt.HashPassword("Demo@123");

        context.Users.Add(new User
        {
            Email = email,
            PasswordHash = pwd,
            FirstName = "Alex",
            LastName = "Applicant",
            IsActive = true,
            UserRoles = new List<UserRole> { new() { Role = candidateRole } }
        });
        await context.SaveChangesAsync();
    }

    private static async Task SeedLeaveTypes(HRMSDbContext context)
    {
        if (await context.LeaveTypes.AnyAsync()) return;
        context.LeaveTypes.AddRange(
            new LeaveType { Name = "Annual Leave", Description = "Paid annual leave", DefaultDays = 15, IsActive = true },
            new LeaveType { Name = "Sick Leave", Description = "Medical leave", DefaultDays = 10, IsActive = true },
            new LeaveType { Name = "Personal Leave", Description = "Personal time off", DefaultDays = 5, IsActive = true },
            new LeaveType { Name = "Maternity Leave", Description = "Maternity leave", DefaultDays = 90, IsActive = true },
            new LeaveType { Name = "Paternity Leave", Description = "Paternity leave", DefaultDays = 10, IsActive = true },
            new LeaveType { Name = "Compensatory Off", Description = "Comp off for overtime", DefaultDays = 0, IsActive = true }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedSystemSettings(HRMSDbContext context)
    {
        if (await context.SystemSettings.AnyAsync()) return;
        context.SystemSettings.AddRange(
            new SystemSetting { Key = "CompanyName", Value = "EWXP Technologies", Description = "Company display name (single-company HRMS)" },
            new SystemSetting { Key = "WorkDaysPerWeek", Value = "5", Description = "Standard working days per week" },
            new SystemSetting { Key = "WorkHoursPerDay", Value = "8", Description = "Standard working hours per day" }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedDepartments(HRMSDbContext context)
    {
        if (await context.Departments.AnyAsync()) return;
        context.Departments.AddRange(
            new Department { Name = "Engineering", Description = "Software development and infrastructure" },
            new Department { Name = "Human Resources", Description = "People operations and recruitment" },
            new Department { Name = "Sales", Description = "Revenue and client acquisition" },
            new Department { Name = "Marketing", Description = "Brand and communications" },
            new Department { Name = "Finance", Description = "Accounting and financial planning" }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedEmployees(HRMSDbContext context)
    {
        if (await context.Employees.AnyAsync()) return;

        var depts = await context.Departments.ToListAsync();
        var eng = depts.First(d => d.Name == "Engineering");
        var hr = depts.First(d => d.Name == "Human Resources");
        var sales = depts.First(d => d.Name == "Sales");
        var mktg = depts.First(d => d.Name == "Marketing");
        var fin = depts.First(d => d.Name == "Finance");

        var today = DateTime.UtcNow;

        var sarah = new Employee
        {
            EmployeeCode = "EMP001", FirstName = "Sarah", LastName = "Johnson",
            Email = "sarah@hrms.com", Phone = "+1-555-0101", Position = "Chief Executive Officer",
            DateOfBirth = new DateTime(1980, 3, 15), DateOfJoining = new DateTime(2018, 6, 1),
            Status = "Active", Gender = "Female", Address = "100 Executive Blvd, Suite 400, New York, NY",
            EmergencyContactName = "Mark Johnson", EmergencyContactPhone = "+1-555-0199"
        };

        var michael = new Employee
        {
            EmployeeCode = "EMP002", FirstName = "Michael", LastName = "Chen",
            Email = "michael@hrms.com", Phone = "+1-555-0102", Position = "Engineering Manager",
            DateOfBirth = new DateTime(1985, 7, 22), DateOfJoining = new DateTime(2019, 3, 15),
            Status = "Active", Gender = "Male", Address = "200 Tech Park Dr, San Francisco, CA",
            DepartmentId = eng.Id, EmergencyContactName = "Lisa Chen", EmergencyContactPhone = "+1-555-0198"
        };

        var emily = new Employee
        {
            EmployeeCode = "EMP003", FirstName = "Emily", LastName = "Rodriguez",
            Email = "emily@hrms.com", Phone = "+1-555-0103", Position = "HR Manager",
            DateOfBirth = new DateTime(1988, 11, 8), DateOfJoining = new DateTime(2020, 1, 10),
            Status = "Active", Gender = "Female", Address = "45 People Plaza, Chicago, IL",
            DepartmentId = hr.Id, EmergencyContactName = "Carlos Rodriguez", EmergencyContactPhone = "+1-555-0197"
        };

        var david = new Employee
        {
            EmployeeCode = "EMP004", FirstName = "David", LastName = "Kim",
            Email = "david@hrms.com", Phone = "+1-555-0104", Position = "Sr. Software Engineer",
            DateOfBirth = new DateTime(1990, 2, 14), DateOfJoining = new DateTime(2021, 6, 1),
            Status = "Active", Gender = "Male", Address = "55 Code St, San Francisco, CA",
            DepartmentId = eng.Id, EmergencyContactName = "Grace Kim", EmergencyContactPhone = "+1-555-0196"
        };

        var priya = new Employee
        {
            EmployeeCode = "EMP005", FirstName = "Priya", LastName = "Patel",
            Email = "priya@hrms.com", Phone = "+1-555-0105", Position = "Software Engineer",
            DateOfBirth = new DateTime(1993, 9, 30), DateOfJoining = new DateTime(2022, 4, 5),
            Status = "Active", Gender = "Female", Address = "120 Dev Lane, San Francisco, CA",
            DepartmentId = eng.Id, EmergencyContactName = "Raj Patel", EmergencyContactPhone = "+1-555-0195"
        };

        var james = new Employee
        {
            EmployeeCode = "EMP006", FirstName = "James", LastName = "Wilson",
            Email = "james@hrms.com", Phone = "+1-555-0106", Position = "Sales Manager",
            DateOfBirth = new DateTime(1986, 5, 18), DateOfJoining = new DateTime(2019, 8, 20),
            Status = "Active", Gender = "Male", Address = "88 Revenue Rd, Austin, TX",
            DepartmentId = sales.Id, EmergencyContactName = "Anna Wilson", EmergencyContactPhone = "+1-555-0194"
        };

        var lisa = new Employee
        {
            EmployeeCode = "EMP007", FirstName = "Lisa", LastName = "Thompson",
            Email = "lisa@hrms.com", Phone = "+1-555-0107", Position = "Marketing Manager",
            DateOfBirth = new DateTime(1987, 12, 3), DateOfJoining = new DateTime(2020, 3, 1),
            Status = "Active", Gender = "Female", Address = "12 Brand Ave, Austin, TX",
            DepartmentId = mktg.Id, EmergencyContactName = "Tom Thompson", EmergencyContactPhone = "+1-555-0193"
        };

        var robert = new Employee
        {
            EmployeeCode = "EMP008", FirstName = "Robert", LastName = "Martinez",
            Email = "robert@hrms.com", Phone = "+1-555-0108", Position = "Finance Manager",
            DateOfBirth = new DateTime(1984, 8, 25), DateOfJoining = new DateTime(2019, 11, 15),
            Status = "Active", Gender = "Male", Address = "300 Money St, New York, NY",
            DepartmentId = fin.Id, EmergencyContactName = "Sofia Martinez", EmergencyContactPhone = "+1-555-0192"
        };

        var amanda = new Employee
        {
            EmployeeCode = "EMP009", FirstName = "Amanda", LastName = "Lee",
            Email = "amanda@hrms.com", Phone = "+1-555-0109", Position = "Jr. Software Engineer",
            DateOfBirth = new DateTime(1997, 6, 25), DateOfJoining = today.AddMonths(-3),
            Status = "Probation", Gender = "Female", Address = "55 Code St, San Francisco, CA",
            DepartmentId = eng.Id, EmergencyContactName = "Steven Lee", EmergencyContactPhone = "+1-555-0191"
        };

        var daniel = new Employee
        {
            EmployeeCode = "EMP010", FirstName = "Daniel", LastName = "Brown",
            Email = "daniel@hrms.com", Phone = "+1-555-0110", Position = "Sales Executive",
            DateOfBirth = new DateTime(1995, 1, 20), DateOfJoining = new DateTime(2023, 2, 1),
            Status = "Active", Gender = "Male", Address = "88 Revenue Rd, Austin, TX",
            DepartmentId = sales.Id, EmergencyContactName = "Karen Brown", EmergencyContactPhone = "+1-555-0190"
        };

        context.Employees.AddRange(sarah, michael, emily, david, priya, james, lisa, robert, amanda, daniel);
        await context.SaveChangesAsync();

        // Set manager references (need IDs after save)
        michael.ManagerId = sarah.Id;
        emily.ManagerId = sarah.Id;
        james.ManagerId = sarah.Id;
        lisa.ManagerId = sarah.Id;
        robert.ManagerId = sarah.Id;
        david.ManagerId = michael.Id;
        priya.ManagerId = michael.Id;
        amanda.ManagerId = michael.Id;
        daniel.ManagerId = james.Id;

        // Set department heads
        eng.HeadId = michael.Id;
        hr.HeadId = emily.Id;
        sales.HeadId = james.Id;
        mktg.HeadId = lisa.Id;
        fin.HeadId = robert.Id;

        await context.SaveChangesAsync();
    }

    private static async Task SeedLeaveBalances(HRMSDbContext context)
    {
        if (await context.LeaveBalances.AnyAsync()) return;
        var employees = await context.Employees.ToListAsync();
        var leaveTypes = await context.LeaveTypes.ToListAsync();
        var currentYear = DateTime.UtcNow.Year;

        foreach (var emp in employees)
        {
            foreach (var lt in leaveTypes.Where(lt => lt.DefaultDays > 0))
            {
                var used = lt.Name switch
                {
                    "Annual Leave" => Random.Shared.Next(2, 8),
                    "Sick Leave" => Random.Shared.Next(0, 4),
                    "Personal Leave" => Random.Shared.Next(0, 2),
                    _ => 0
                };
                context.LeaveBalances.Add(new LeaveBalance
                {
                    EmployeeId = emp.Id, LeaveTypeId = lt.Id,
                    TotalDays = lt.DefaultDays, UsedDays = used,
                    Year = currentYear
                });
            }
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedLeaveRequests(HRMSDbContext context)
    {
        if (await context.LeaveRequests.AnyAsync()) return;
        var employees = await context.Employees.ToListAsync();
        var leaveTypes = await context.LeaveTypes.ToListAsync();
        var annual = leaveTypes.First(l => l.Name == "Annual Leave");
        var sick = leaveTypes.First(l => l.Name == "Sick Leave");
        var personal = leaveTypes.First(l => l.Name == "Personal Leave");
        var today = DateTime.UtcNow;
        var sarah = employees.First(e => e.Email == "sarah@hrms.com");
        var emily = employees.First(e => e.Email == "emily@hrms.com");
        var michael = employees.First(e => e.Email == "michael@hrms.com");

        var requests = new List<LeaveRequest>
        {
            // Approved leave
            new() { EmployeeId = employees.First(e => e.Email == "david@hrms.com").Id, LeaveTypeId = annual.Id,
                StartDate = today.AddDays(-30), EndDate = today.AddDays(-28), Reason = "Family vacation",
                Status = "Approved", ReviewedBy = michael.Id, ReviewNotes = "Enjoy your time off!", ReviewedAt = today.AddDays(-31) },
            new() { EmployeeId = employees.First(e => e.Email == "priya@hrms.com").Id, LeaveTypeId = sick.Id,
                StartDate = today.AddDays(-15), EndDate = today.AddDays(-15), Reason = "Feeling unwell",
                Status = "Approved", ReviewedBy = michael.Id, ReviewNotes = "Get well soon", ReviewedAt = today.AddDays(-16) },

            // Pending leave
            new() { EmployeeId = employees.First(e => e.Email == "amanda@hrms.com").Id, LeaveTypeId = personal.Id,
                StartDate = today.AddDays(5), EndDate = today.AddDays(5), Reason = "Personal appointment",
                Status = "Pending", CreatedAt = today.AddDays(-1) },
            new() { EmployeeId = employees.First(e => e.Email == "daniel@hrms.com").Id, LeaveTypeId = annual.Id,
                StartDate = today.AddDays(10), EndDate = today.AddDays(14), Reason = "Annual trip to Europe",
                Status = "Pending", CreatedAt = today.AddDays(-2) },
            new() { EmployeeId = employees.First(e => e.Email == "priya@hrms.com").Id, LeaveTypeId = annual.Id,
                StartDate = today.AddDays(20), EndDate = today.AddDays(22), Reason = "Wedding anniversary getaway",
                Status = "Pending", CreatedAt = today.AddDays(-5) },

            // Rejected leave
            new() { EmployeeId = employees.First(e => e.Email == "lisa@hrms.com").Id, LeaveTypeId = annual.Id,
                StartDate = today.AddDays(-10), EndDate = today.AddDays(-8), Reason = "Personal day",
                Status = "Rejected", ReviewedBy = sarah.Id, ReviewNotes = "Team needs coverage during product launch", ReviewedAt = today.AddDays(-11) },
        };
        context.LeaveRequests.AddRange(requests);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAttendanceRecords(HRMSDbContext context)
    {
        if (await context.AttendanceRecords.AnyAsync()) return;
        var employees = await context.Employees.ToListAsync();
        var today = DateTime.UtcNow.Date;
        var records = new List<AttendanceRecord>();

        foreach (var emp in employees)
        {
            for (int i = 0; i < 20; i++)
            {
                var date = today.AddDays(-i);
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) continue;

                var status = Random.Shared.NextDouble() switch
                {
                    < 0.75 => "Present",
                    < 0.88 => "Late",
                    < 0.95 => "Absent",
                    _ => "Present"
                };

                records.Add(new AttendanceRecord
                {
                    EmployeeId = emp.Id, Date = date,
                    CheckInTime = status == "Present" ? date.AddHours(8).AddMinutes(Random.Shared.Next(-10, 15)) : status == "Late" ? date.AddHours(9).AddMinutes(Random.Shared.Next(5, 30)) : null,
                    CheckOutTime = status == "Present" || status == "Late" ? date.AddHours(17).AddMinutes(Random.Shared.Next(-30, 10)) : null,
                    Status = status
                });
            }
        }
        context.AttendanceRecords.AddRange(records);
        await context.SaveChangesAsync();
    }

    private static async Task SeedReviewCycles(HRMSDbContext context)
    {
        if (await context.ReviewCycles.AnyAsync()) return;
        var today = DateTime.UtcNow;
        context.ReviewCycles.AddRange(
            new ReviewCycle { Name = "Q1 2026 Performance Review", StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 3, 31), Status = "Completed" },
            new ReviewCycle { Name = "Q2 2026 Performance Review", StartDate = new DateTime(2026, 4, 1), EndDate = new DateTime(2026, 6, 30), Status = "Active" }
        );
        await context.SaveChangesAsync();

        var cycle1 = await context.ReviewCycles.FirstAsync(c => c.Name.StartsWith("Q1"));
        var cycle2 = await context.ReviewCycles.FirstAsync(c => c.Name.StartsWith("Q2"));
        var employees = await context.Employees.ToListAsync();
        var sarah = employees.First(e => e.Email == "sarah@hrms.com");
        var michael = employees.First(e => e.Email == "michael@hrms.com");
        var emily = employees.First(e => e.Email == "emily@hrms.com");
        var james = employees.First(e => e.Email == "james@hrms.com");
        var robert = employees.First(e => e.Email == "robert@hrms.com");
        var david = employees.First(e => e.Email == "david@hrms.com");
        var priya = employees.First(e => e.Email == "priya@hrms.com");

        // Completed reviews from Q1
        context.PerformanceReviews.AddRange(
            new PerformanceReview { Title = "Michael Chen - Q1 Review", EmployeeId = michael.Id, ReviewerId = sarah.Id, CycleId = cycle1.Id,
                StartDate = new DateTime(2026, 3, 15), EndDate = new DateTime(2026, 3, 25), Status = "Completed",
                OverallScore = 4.5m, Comments = "Excellent leadership. Team delivered all Q1 milestones on time." },
            new PerformanceReview { Title = "David Kim - Q1 Review", EmployeeId = david.Id, ReviewerId = michael.Id, CycleId = cycle1.Id,
                StartDate = new DateTime(2026, 3, 15), EndDate = new DateTime(2026, 3, 25), Status = "Completed",
                OverallScore = 4.2m, Comments = "Strong technical contributions. Mentored junior developers effectively." },
            new PerformanceReview { Title = "Priya Patel - Q1 Review", EmployeeId = priya.Id, ReviewerId = michael.Id, CycleId = cycle1.Id,
                StartDate = new DateTime(2026, 3, 15), EndDate = new DateTime(2026, 3, 25), Status = "Completed",
                OverallScore = 3.8m, Comments = "Good progress. Needs to work on system design skills." },

            // In-progress reviews for Q2
            new PerformanceReview { Title = "Michael Chen - Q2 Review", EmployeeId = michael.Id, ReviewerId = sarah.Id, CycleId = cycle2.Id,
                StartDate = new DateTime(2026, 6, 1), EndDate = new DateTime(2026, 6, 30), Status = "InProgress",
                OverallScore = null, Comments = null },
            new PerformanceReview { Title = "David Kim - Q2 Review", EmployeeId = david.Id, ReviewerId = michael.Id, CycleId = cycle2.Id,
                StartDate = new DateTime(2026, 6, 1), EndDate = new DateTime(2026, 6, 30), Status = "InProgress",
                OverallScore = null, Comments = null },
            new PerformanceReview { Title = "Priya Patel - Q2 Review", EmployeeId = priya.Id, ReviewerId = michael.Id, CycleId = cycle2.Id,
                StartDate = new DateTime(2026, 6, 1), EndDate = new DateTime(2026, 6, 30), Status = "Pending",
                OverallScore = null, Comments = null }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedPayroll(HRMSDbContext context)
    {
        if (await context.PayrollStructures.AnyAsync()) return;
        var structure = new PayrollStructure
        {
            Name = "Standard Monthly Salary",
            Description = "Standard pay structure for full-time employees",
            Components = new List<SalaryComponent>
            {
                new() { Name = "Basic Salary", Type = "Earning", Amount = 5000, IsRecurring = true },
                new() { Name = "Housing Allowance (HRA)", Type = "Earning", Amount = 2500, IsRecurring = true },
                new() { Name = "Transport Allowance", Type = "Earning", Amount = 800, IsRecurring = true },
                new() { Name = "Medical Allowance", Type = "Earning", Amount = 500, IsRecurring = true },
                new() { Name = "Performance Bonus", Type = "Earning", Amount = 1000, IsRecurring = false },
                new() { Name = "Provident Fund (PF)", Type = "Deduction", Amount = 600, IsRecurring = true },
                new() { Name = "Professional Tax", Type = "Deduction", Amount = 200, IsRecurring = true },
                new() { Name = "Income Tax (TDS)", Type = "Deduction", Amount = 1200, IsRecurring = true }
            }
        };
        context.PayrollStructures.Add(structure);
        await context.SaveChangesAsync();
    }

    private static async Task SeedJobRequisitions(HRMSDbContext context)
    {
        var depts = await context.Departments.ToListAsync();
        var definitions = BuildCareerJobDefinitions(depts);

        if (!await context.JobRequisitions.AnyAsync())
        {
            context.JobRequisitions.AddRange(definitions);
            await context.SaveChangesAsync();
            return;
        }

        var existingTitles = await context.JobRequisitions.Select(j => j.Title).ToListAsync();
        var missing = definitions.Where(d => !existingTitles.Contains(d.Title)).ToList();
        if (missing.Count > 0)
        {
            context.JobRequisitions.AddRange(missing);
            await context.SaveChangesAsync();
        }
    }

    private static List<JobRequisition> BuildCareerJobDefinitions(List<Department> depts)
    {
        var eng = depts.First(d => d.Name == "Engineering");
        var hr = depts.First(d => d.Name == "Human Resources");
        var sales = depts.First(d => d.Name == "Sales");
        var mktg = depts.First(d => d.Name == "Marketing");
        var fin = depts.First(d => d.Name == "Finance");
        var today = DateTime.UtcNow;

        return new List<JobRequisition>
        {
            new()
            {
                Title = "Senior React Developer",
                Description = "Lead frontend architecture for our EWXP platform. Build accessible, performant UIs with React 19 and TypeScript.",
                Requirements = "5+ years with React, TypeScript, Redux/RTK, REST APIs. Experience with design systems and CI/CD.",
                Status = "Open", DepartmentId = eng.Id, CreatedAt = today.AddDays(-1)
            },
            new()
            {
                Title = "Frontend Developer",
                Description = "Develop responsive web applications with modern frameworks for internal HR and candidate-facing products.",
                Requirements = "3+ years experience with React, TypeScript, CSS, and REST API integration.",
                Status = "Open", DepartmentId = eng.Id, CreatedAt = today.AddDays(-2)
            },
            new()
            {
                Title = "Senior Backend Engineer",
                Description = "Build and maintain scalable microservices architecture powering the EWXP HRMS platform.",
                Requirements = "7+ years experience with C#/.NET, SQL, REST APIs, and cloud platforms (Azure/AWS).",
                Status = "Open", DepartmentId = eng.Id, CreatedAt = today.AddDays(-3)
            },
            new()
            {
                Title = "Java Backend Developer",
                Description = "Design Spring Boot microservices, integrate with SQL databases, and support enterprise HR workflows.",
                Requirements = "4+ years Java, Spring Boot, Hibernate, Microservices, SQL, REST APIs.",
                Status = "Open", DepartmentId = eng.Id, CreatedAt = today.AddDays(-4)
            },
            new()
            {
                Title = "DevOps Engineer",
                Description = "Manage CI/CD pipelines, cloud infrastructure, and observability for production HRMS workloads.",
                Requirements = "5+ years experience with Docker, Kubernetes, Terraform, and Azure/AWS.",
                Status = "Open", DepartmentId = eng.Id, CreatedAt = today.AddDays(-5)
            },
            new()
            {
                Title = "QA Automation Engineer",
                Description = "Own test automation strategy across API and UI layers for critical HR modules.",
                Requirements = "3+ years with Selenium/Cypress, API testing, CI integration, and SQL.",
                Status = "Open", DepartmentId = eng.Id, CreatedAt = today.AddDays(-6)
            },
            new()
            {
                Title = "Data Science Analyst",
                Description = "Build predictive models for attrition, hiring forecast, and workforce analytics dashboards.",
                Requirements = "2+ years Python, SQL, pandas, scikit-learn. HR or people analytics experience is a plus.",
                Status = "Open", DepartmentId = eng.Id, CreatedAt = today.AddDays(-7)
            },
            new()
            {
                Title = "Mobile Developer (Flutter)",
                Description = "Create cross-platform mobile experiences for employee self-service and manager workflows.",
                Requirements = "3+ years Flutter/Dart, REST APIs, state management, App Store deployment.",
                Status = "Open", DepartmentId = eng.Id, CreatedAt = today.AddDays(-8)
            },
            new()
            {
                Title = "Software Engineering Intern",
                Description = "Summer internship working with senior engineers on real EWXP features — mentorship included.",
                Requirements = "Final-year BTech/MCA. Strong fundamentals in any programming language. Eager to learn React or .NET.",
                Status = "Open", DepartmentId = eng.Id, CreatedAt = today.AddDays(-2)
            },
            new()
            {
                Title = "Sales Operations Analyst",
                Description = "Analyze sales data, optimize revenue operations, and support forecasting for the sales leadership team.",
                Requirements = "2+ years experience in sales analytics, Excel, SQL, and CRM tools (Salesforce preferred).",
                Status = "Open", DepartmentId = sales.Id, CreatedAt = today.AddDays(-9)
            },
            new()
            {
                Title = "Enterprise Sales Executive",
                Description = "Manage mid-market and enterprise accounts, drive new logo acquisition and expansion revenue.",
                Requirements = "4+ years B2B SaaS sales, strong pipeline management, excellent communication skills.",
                Status = "Open", DepartmentId = sales.Id, CreatedAt = today.AddDays(-10)
            },
            new()
            {
                Title = "Digital Marketing Manager",
                Description = "Lead digital campaigns, employer branding, and growth initiatives for EWXP talent brand.",
                Requirements = "5+ years digital marketing, SEO/SEM, analytics, and campaign management.",
                Status = "Open", DepartmentId = mktg.Id, CreatedAt = today.AddDays(-11)
            },
            new()
            {
                Title = "Content Marketing Specialist",
                Description = "Create blogs, case studies, and social content that showcases our culture and product innovation.",
                Requirements = "2+ years content writing, SEO basics, social media, B2B tech preferred.",
                Status = "Open", DepartmentId = mktg.Id, CreatedAt = today.AddDays(-12)
            },
            new()
            {
                Title = "HR Business Partner",
                Description = "Partner with Engineering and Product leaders on workforce planning, performance, and employee experience.",
                Requirements = "5+ years HRBP experience, strong stakeholder management, HRMS systems knowledge.",
                Status = "Open", DepartmentId = hr.Id, CreatedAt = today.AddDays(-14)
            },
            new()
            {
                Title = "Talent Acquisition Specialist",
                Description = "Own full-cycle recruiting for tech and business roles using our AI-powered career portal pipeline.",
                Requirements = "3+ years corporate recruiting, ATS experience, strong candidate experience mindset.",
                Status = "Open", DepartmentId = hr.Id, CreatedAt = today.AddDays(-15)
            },
            new()
            {
                Title = "Financial Analyst",
                Description = "Support budgeting, payroll forecasting, and financial reporting for the People & Finance team.",
                Requirements = "2+ years financial analysis, Excel modeling, SQL. MBA or CA inter preferred.",
                Status = "Open", DepartmentId = fin.Id, CreatedAt = today.AddDays(-18)
            },
        };
    }

    private static async Task SeedCandidates(HRMSDbContext context)
    {
        if (await context.CandidateProfiles.AnyAsync()) return;
        var jobs = await context.JobRequisitions.ToListAsync();
        var backendJob = jobs.First(j => j.Title.Contains("Backend"));
        var frontendJob = jobs.First(j => j.Title.Contains("Frontend"));
        var salesJob = jobs.First(j => j.Title.Contains("Sales"));

        var today = DateTime.UtcNow;
        context.CandidateProfiles.AddRange(
            new CandidateProfile { FirstName = "Alex", LastName = "Turner", Email = "alex.turner@email.com", Phone = "+1-555-0201",
                Status = "Screening", Source = "LinkedIn", MatchScore = 92, JobRequisitionId = backendJob.Id, CreatedAt = today.AddDays(-10) },
            new CandidateProfile { FirstName = "Maria", LastName = "Garcia", Email = "maria.garcia@email.com", Phone = "+1-555-0202",
                Status = "Interviewed", Source = "Referral", MatchScore = 88, JobRequisitionId = backendJob.Id, CreatedAt = today.AddDays(-15) },
            new CandidateProfile { FirstName = "Kevin", LastName = "Nguyen", Email = "kevin.nguyen@email.com", Phone = "+1-555-0203",
                Status = "New", Source = "Indeed", MatchScore = 75, JobRequisitionId = frontendJob.Id, CreatedAt = today.AddDays(-3) },
            new CandidateProfile { FirstName = "Sophie", LastName = "Williams", Email = "sophie.w@email.com", Phone = "+1-555-0204",
                Status = "Offered", Source = "Company Website", MatchScore = 85, JobRequisitionId = frontendJob.Id, CreatedAt = today.AddDays(-20) },
            new CandidateProfile { FirstName = "Ryan", LastName = "Taylor", Email = "ryan.taylor@email.com", Phone = "+1-555-0205",
                Status = "Screening", Source = "LinkedIn", MatchScore = 70, JobRequisitionId = salesJob.Id, CreatedAt = today.AddDays(-7) }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedUsers(HRMSDbContext context)
    {
        if (await context.Users.CountAsync() > 1) return;

        var employees = await context.Employees.ToListAsync();
        var roles = await context.Roles.ToListAsync();
        var adminRole = roles.First(r => r.Name == "Administrator");
        var hrRole = roles.First(r => r.Name == "HRManager");
        var mgrRole = roles.First(r => r.Name == "Manager");
        var empRole = roles.First(r => r.Name == "Employee");
        var payrollRole = roles.First(r => r.Name == "PayrollStaff");

        var pwd = BCrypt.Net.BCrypt.HashPassword("Demo@123");

        var sarah = employees.First(e => e.Email == "sarah@hrms.com");
        var michael = employees.First(e => e.Email == "michael@hrms.com");
        var emily = employees.First(e => e.Email == "emily@hrms.com");
        var david = employees.First(e => e.Email == "david@hrms.com");
        var priya = employees.First(e => e.Email == "priya@hrms.com");
        var james = employees.First(e => e.Email == "james@hrms.com");
        var lisa = employees.First(e => e.Email == "lisa@hrms.com");
        var robert = employees.First(e => e.Email == "robert@hrms.com");
        var amanda = employees.First(e => e.Email == "amanda@hrms.com");
        var daniel = employees.First(e => e.Email == "daniel@hrms.com");

        // Create or update admin user linked to Sarah
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@hrms.com");
        if (adminUser == null)
        {
            adminUser = new User
            {
                Email = "admin@hrms.com",
                PasswordHash = pwd,
                FirstName = "Sarah",
                LastName = "Johnson",
                IsActive = true,
                EmployeeId = sarah.Id,
                UserRoles = new List<UserRole> { new() { Role = adminRole } }
            };
            context.Users.Add(adminUser);
        }
        else
        {
            adminUser.FirstName = "Sarah";
            adminUser.LastName = "Johnson";
            adminUser.EmployeeId = sarah.Id;
        }

        // Link sarah to admin user
        sarah.User = adminUser;

        var users = new List<User>
        {
            new() { Email = "michael@hrms.com", PasswordHash = pwd, FirstName = "Michael", LastName = "Chen", IsActive = true, EmployeeId = michael.Id,
                UserRoles = new List<UserRole> { new() { Role = mgrRole }, new() { Role = empRole } } },
            new() { Email = "emily@hrms.com", PasswordHash = pwd, FirstName = "Emily", LastName = "Rodriguez", IsActive = true, EmployeeId = emily.Id,
                UserRoles = new List<UserRole> { new() { Role = hrRole }, new() { Role = empRole } } },
            new() { Email = "david@hrms.com", PasswordHash = pwd, FirstName = "David", LastName = "Kim", IsActive = true, EmployeeId = david.Id,
                UserRoles = new List<UserRole> { new() { Role = empRole } } },
            new() { Email = "priya@hrms.com", PasswordHash = pwd, FirstName = "Priya", LastName = "Patel", IsActive = true, EmployeeId = priya.Id,
                UserRoles = new List<UserRole> { new() { Role = empRole } } },
            new() { Email = "james@hrms.com", PasswordHash = pwd, FirstName = "James", LastName = "Wilson", IsActive = true, EmployeeId = james.Id,
                UserRoles = new List<UserRole> { new() { Role = mgrRole }, new() { Role = empRole } } },
            new() { Email = "lisa@hrms.com", PasswordHash = pwd, FirstName = "Lisa", LastName = "Thompson", IsActive = true, EmployeeId = lisa.Id,
                UserRoles = new List<UserRole> { new() { Role = mgrRole }, new() { Role = empRole } } },
            new() { Email = "robert@hrms.com", PasswordHash = pwd, FirstName = "Robert", LastName = "Martinez", IsActive = true, EmployeeId = robert.Id,
                UserRoles = new List<UserRole> { new() { Role = payrollRole }, new() { Role = empRole } } },
            new() { Email = "amanda@hrms.com", PasswordHash = pwd, FirstName = "Amanda", LastName = "Lee", IsActive = true, EmployeeId = amanda.Id,
                UserRoles = new List<UserRole> { new() { Role = empRole } } },
            new() { Email = "daniel@hrms.com", PasswordHash = pwd, FirstName = "Daniel", LastName = "Brown", IsActive = true, EmployeeId = daniel.Id,
                UserRoles = new List<UserRole> { new() { Role = empRole } } },
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSocialPosts(HRMSDbContext context)
    {
        if (await context.SocialPosts.AnyAsync()) return;
        var emps = await context.Employees.Where(e => !e.IsDeleted).ToListAsync();
        if (emps.Count < 3) return;

        var posts = new List<SocialPost>
        {
            new()
            {
                Content = "Welcome to the new EWXP platform! 🎉 We've launched our AI-powered HR experience platform. Check out the new AI Copilot, Org Chart, and Analytics features. Excited to have everyone on board!",
                Type = "Announcement", Tags = "[\"Launch\",\"Platform\",\"Welcome\"]",
                AuthorId = emps[0].Id, CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Content = "Huge shoutout to the Engineering team for crushing the Q2 release! 🚀 Your hard work and late nights really paid off. The new features are getting great feedback from everyone.",
                Type = "Recognition", Tags = "[\"Engineering\",\"Q2\",\"Achievement\"]",
                AuthorId = emps[1].Id, CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Content = "Team, just a reminder that the quarterly review submissions are due by Friday. Please make sure all your goals are updated in the system. Let me know if you need any help!",
                Type = "Announcement", Tags = "[\"Reviews\",\"Deadline\",\"Reminder\"]",
                AuthorId = emps[2].Id, CreatedAt = DateTime.UtcNow.AddHours(-12)
            },
            new()
            {
                Content = "Had an amazing team lunch today! Great to connect with everyone outside of work. 🍕 Let's do this more often!",
                Type = "Post", Tags = "[\"TeamBuilding\",\"Lunch\",\"Culture\"]",
                AuthorId = emps[3].Id, CreatedAt = DateTime.UtcNow.AddHours(-6)
            },
            new()
            {
                Content = "Just completed the new AI Copilot feature and I'm blown away by what we've built! You can now ask questions in Hinglish like 'Meri team kaun kaun hai?' and get instant answers. Try it out! 🤖",
                Type = "Post", Tags = "[\"AI\",\"Copilot\",\"Feature\"]",
                AuthorId = emps[0].Id, CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
        };

        context.SocialPosts.AddRange(posts);
        await context.SaveChangesAsync();

        // Add some likes and comments
        var savedPosts = await context.SocialPosts.ToListAsync();
        var users = await context.Users.ToListAsync();

        foreach (var post in savedPosts)
        {
            foreach (var u in users.Take(3))
            {
                context.PostLikes.Add(new PostLike { PostId = post.Id, UserId = u.Id });
            }

            if (post.Type != "Announcement")
            {
                context.PostComments.Add(new PostComment
                {
                    PostId = post.Id, Content = "Great work! 🔥",
                    AuthorId = emps[new Random().Next(emps.Count)].Id,
                    CreatedAt = post.CreatedAt.AddMinutes(30)
                });
            }
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedPromotions(HRMSDbContext context)
    {
        if (await context.PromotionRequests.AnyAsync()) return;
        var emps = await context.Employees.ToListAsync();
        if (emps.Count < 4) return;

        var sarah = emps.First(e => e.Email == "sarah@hrms.com");
        var michael = emps.First(e => e.Email == "michael@hrms.com");
        var emily = emps.First(e => e.Email == "emily@hrms.com");
        var david = emps.First(e => e.Email == "david@hrms.com");
        var priya = emps.First(e => e.Email == "priya@hrms.com");
        var amanda = emps.First(e => e.Email == "amanda@hrms.com");

        context.PromotionRequests.AddRange(
            // Fully approved promotion (David Kim: Sr. Software Engineer → Staff Engineer)
            new PromotionRequest
            {
                EmployeeId = david.Id, RequestedById = michael.Id,
                CurrentPosition = "Sr. Software Engineer", CurrentSalary = 110000,
                ProposedPosition = "Staff Engineer", ProposedSalary = 135000,
                Justification = "David has consistently exceeded expectations. He led the microservices migration and mentored 3 junior engineers. His technical leadership has been instrumental in Q1 and Q2 deliveries.",
                Status = "Approved",
                ApprovedByManagerId = michael.Id, ManagerNotes = "Strongly recommend. David is ready for this role.",
                ApprovedByHrbpId = emily.Id, HrbpNotes = "Salary is within band for Staff Engineer role.",
                ApprovedByDeptHeadId = michael.Id, DeptHeadNotes = "Approved for Engineering.",
                ApprovedByCeoId = sarah.Id, CeoNotes = "Approved. Well deserved.",
                PayrollUpdated = true, CreatedAt = DateTime.UtcNow.AddDays(-14)
            },
            // Mid-approval promotion (Priya Patel: Software Engineer → Sr. Software Engineer)
            new PromotionRequest
            {
                EmployeeId = priya.Id, RequestedById = michael.Id,
                CurrentPosition = "Software Engineer", CurrentSalary = 85000,
                ProposedPosition = "Sr. Software Engineer", ProposedSalary = 105000,
                Justification = "Priya has grown significantly in the past year. She now independently handles complex features and contributes to architecture decisions.",
                Status = "PendingDeptHeadApproval",
                ApprovedByManagerId = michael.Id, ManagerNotes = "Priya has shown excellent growth. Ready for senior role.",
                ApprovedByHrbpId = emily.Id, HrbpNotes = "Reviewing comp band alignment.",
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            // New promotion request (Amanda Lee: Jr. Engineer → Software Engineer)
            new PromotionRequest
            {
                EmployeeId = amanda.Id, RequestedById = michael.Id,
                CurrentPosition = "Jr. Software Engineer", CurrentSalary = 65000,
                ProposedPosition = "Software Engineer", ProposedSalary = 80000,
                Justification = "Amanda has completed probation successfully and is performing at mid-level engineer capacity. She ships features independently and writes quality code.",
                Status = "PendingManagerApproval",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedTransfers(HRMSDbContext context)
    {
        if (await context.TransferRequests.AnyAsync()) return;
        var emps = await context.Employees.ToListAsync();
        var depts = await context.Departments.ToListAsync();
        if (emps.Count < 5 || depts.Count < 2) return;

        var sarah = emps.First(e => e.Email == "sarah@hrms.com");
        var michael = emps.First(e => e.Email == "michael@hrms.com");
        var emily = emps.First(e => e.Email == "emily@hrms.com");
        var james = emps.First(e => e.Email == "james@hrms.com");
        var daniel = emps.First(e => e.Email == "daniel@hrms.com");
        var lisa = emps.First(e => e.Email == "lisa@hrms.com");
        var robert = emps.First(e => e.Email == "robert@hrms.com");
        var eng = depts.First(d => d.Name == "Engineering");
        var mktg = depts.First(d => d.Name == "Marketing");
        var hr = depts.First(d => d.Name == "Human Resources");

        context.TransferRequests.AddRange(
            // Completed transfer (Daniel: Sales → Marketing)
            new TransferRequest
            {
                EmployeeId = daniel.Id, RequestedById = james.Id,
                CurrentPosition = "Sales Executive", ProposedPosition = "Marketing Specialist",
                Reason = "Daniel has shown strong interest in marketing campaigns. His sales experience will add valuable customer perspective to the marketing team.",
                Status = "Completed", EmployeeAccepted = true,
                CurrentDepartmentId = depts.First(d => d.Name == "Sales").Id,
                ProposedDepartmentId = mktg.Id,
                ApprovedByManagerId = james.Id, ManagerNotes = "Approved. Daniel has been cross-training with marketing for 2 months.",
                ApprovedByHrId = emily.Id, HrNotes = "Transfer approved. Benefits remain unchanged.",
                ApprovedByDeptId = lisa.Id, DeptNotes = "We welcome Daniel to the Marketing team!",
                ApprovedByItId = michael.Id, ItNotes = "IT access migrated. Laptop reimaged.",
                ApprovedByPayrollId = robert.Id, PayrollNotes = "Department code updated in payroll system.",
                EmployeeNotes = "I'm excited to join the Marketing team!", CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            // Mid-transfer (move employee to HR for a role)
            new TransferRequest
            {
                EmployeeId = emps.First(e => e.Email == "lisa@hrms.com").Id,
                RequestedById = sarah.Id,
                CurrentPosition = "Marketing Manager",
                ProposedPosition = "HR Operations Manager",
                Reason = "Strategic realignment. Lisa's process optimization experience in marketing will bring valuable operational excellence to HR.",
                Status = "PendingItApproval",
                CurrentDepartmentId = mktg.Id,
                ProposedDepartmentId = hr.Id,
                ApprovedByManagerId = sarah.Id, ManagerNotes = "Approved as part of restructuring.",
                ApprovedByHrId = emily.Id, HrNotes = "Role profile prepared. JD shared with Lisa.",
                ApprovedByDeptId = emps.First(e => e.Email == "sarah@hrms.com").Id, DeptNotes = "Organization realignment approved.",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            // Fresh transfer request
            new TransferRequest
            {
                EmployeeId = emps.First(e => e.Email == "priya@hrms.com").Id,
                RequestedById = michael.Id,
                CurrentPosition = "Software Engineer",
                ProposedPosition = "Technical Lead - Platform Team",
                Reason = "Priya's system design skills have grown significantly. She would be an excellent fit to lead our new Platform Engineering initiative.",
                Status = "PendingManagerApproval",
                CurrentDepartmentId = eng.Id,
                ProposedDepartmentId = eng.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedPolls(HRMSDbContext context)
    {
        if (await context.Polls.AnyAsync()) return;
        var emps = await context.Employees.Where(e => !e.IsDeleted).ToListAsync();
        var users = await context.Users.ToListAsync();
        if (emps.Count < 3) return;

        var poll = new Poll
        {
            Question = "What should be our next team-building activity?",
            MultiVote = false, CreatedById = emps[0].Id,
            Options = new List<PollOption>
            {
                new() { Text = "Go Kart Racing 🏎️" },
                new() { Text = "Escape Room 🧩" },
                new() { Text = "Paintball 🔫" },
                new() { Text = "Board Game Night 🎲" },
                new() { Text = "Karaoke 🎤" },
            }
        };
        context.Polls.Add(poll);
        await context.SaveChangesAsync();

        // Add votes
        var savedPoll = await context.Polls.Include(p => p.Options).FirstAsync(p => p.Question == poll.Question);
        var userList = users.Take(8).ToList();
        for (int i = 0; i < userList.Count; i++)
        {
            context.PollVotes.Add(new PollVote
            {
                PollOptionId = savedPoll.Options.ElementAt(i % savedPoll.Options.Count).Id,
                UserId = userList[i].Id
            });
        }
        await context.SaveChangesAsync();

        // Another poll
        context.Polls.Add(new Poll
        {
            Question = "Which programming language should our team standardize on?",
            MultiVote = true, CreatedById = emps[1].Id,
            Options = new List<PollOption>
            {
                new() { Text = "C#" }, new() { Text = "TypeScript" },
                new() { Text = "Python" }, new() { Text = "Go" },
            }
        });
        await context.SaveChangesAsync();
    }

    private static async Task SeedDiscussions(HRMSDbContext context)
    {
        if (await context.DiscussionThreads.AnyAsync()) return;
        var emps = await context.Employees.Where(e => !e.IsDeleted).ToListAsync();
        if (emps.Count < 3) return;

        var threads = new List<DiscussionThread>
        {
            new()
            {
                Title = "🚀 Tips for writing clean React components",
                Content = "I've been refactoring our frontend lately and wanted to share some patterns that really helped:\n\n1. Keep components small - if a component is >200 lines, split it\n2. Extract custom hooks for complex state logic\n3. Use TypeScript strictly - avoid `any`\n4. Memoize expensive computations with useMemo\n\nWhat patterns do you all follow?",
                Category = "Tech", Tags = "react,typescript,best-practices", IsPinned = true, ViewCount = 42,
                CreatedById = emps[0].Id, CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                Title = "📚 Book recommendations for engineering leaders",
                Content = "Some books that have shaped my engineering management philosophy:\n\n- The Manager's Path by Camille Fournier\n- An Elegant Puzzle by Will Larson\n- Team Topologies by Matthew Skelton\n\nWould love to hear your recommendations!",
                Category = "General", Tags = "books,leadership", ViewCount = 28,
                CreatedById = emps[1].Id, CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                Title = "Q3 Planning - What should the engineering team focus on?",
                Content = "We need to start thinking about Q3 priorities. Key areas we should consider:\n\n- Platform stability and reliability\n- Developer experience improvements\n- New feature development\n- Technical debt reduction\n\nPlease share your thoughts!",
                Category = "Ideas", Tags = "planning,q3,engineering", ViewCount = 35,
                CreatedById = emps[2].Id, CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
        };
        context.DiscussionThreads.AddRange(threads);
        await context.SaveChangesAsync();

        // Add some replies
        var savedThreads = await context.DiscussionThreads.ToListAsync();
        foreach (var t in savedThreads.Take(2))
        {
            context.DiscussionReplies.Add(new DiscussionReply
            {
                ThreadId = t.Id, Content = "Great insights! Thanks for sharing 🙌",
                CreatedById = emps[new Random().Next(emps.Count)].Id,
                CreatedAt = t.CreatedAt.AddHours(2)
            });
            context.DiscussionReplies.Add(new DiscussionReply
            {
                ThreadId = t.Id, Content = "I'd add that testing strategy is also crucial. We should document our approach.",
                CreatedById = emps[new Random().Next(emps.Count)].Id,
                CreatedAt = t.CreatedAt.AddHours(5)
            });
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedSkills(HRMSDbContext context)
    {
        if (await context.Skills.AnyAsync()) return;
        var skills = new List<Skill>
        {
            new() { Name = "C# / .NET", Category = "Technical", Description = "C# and .NET framework development" },
            new() { Name = "React", Category = "Technical", Description = "React frontend development" },
            new() { Name = "TypeScript", Category = "Technical", Description = "TypeScript programming" },
            new() { Name = "SQL", Category = "Technical", Description = "SQL and database querying" },
            new() { Name = "Azure", Category = "Technical", Description = "Microsoft Azure cloud platform" },
            new() { Name = "Docker", Category = "Technical", Description = "Containerization with Docker" },
            new() { Name = "Kubernetes", Category = "Technical", Description = "Container orchestration" },
            new() { Name = "Python", Category = "Technical", Description = "Python programming" },
            new() { Name = "Leadership", Category = "Soft", Description = "Team leadership and management" },
            new() { Name = "Communication", Category = "Soft", Description = "Verbal and written communication" },
            new() { Name = "Project Management", Category = "Soft", Description = "Project planning and execution" },
            new() { Name = "HR Compliance", Category = "Domain", Description = "Employment law and regulations" },
            new() { Name = "Recruitment", Category = "Domain", Description = "Talent acquisition and screening" },
            new() { Name = "Financial Analysis", Category = "Domain", Description = "Financial data analysis" },
        };
        context.Skills.AddRange(skills);
        await context.SaveChangesAsync();

        // Assign skills to employees
        var emps = await context.Employees.Where(e => !e.IsDeleted).ToListAsync();
        var savedSkills = await context.Skills.ToListAsync();

        foreach (var emp in emps)
        {
            var empSkills = savedSkills.OrderBy(_ => Random.Shared.Next()).Take(Random.Shared.Next(3, 7)).ToList();
            foreach (var s in empSkills)
            {
                context.EmployeeSkills.Add(new EmployeeSkill
                {
                    EmployeeId = emp.Id, SkillId = s.Id,
                    ProficiencyLevel = Random.Shared.Next(2, 6)
                });
            }
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedPositionSkillRequirements(HRMSDbContext context)
    {
        if (await context.PositionSkillRequirements.AnyAsync()) return;

        var skills = await context.Skills.ToListAsync();
        var csharp = skills.FirstOrDefault(s => s.Name == "C# / .NET");
        var react = skills.FirstOrDefault(s => s.Name == "React");
        var ts = skills.FirstOrDefault(s => s.Name == "TypeScript");
        var sql = skills.FirstOrDefault(s => s.Name == "SQL");
        var azure = skills.FirstOrDefault(s => s.Name == "Azure");
        var docker = skills.FirstOrDefault(s => s.Name == "Docker");
        var k8s = skills.FirstOrDefault(s => s.Name == "Kubernetes");
        var python = skills.FirstOrDefault(s => s.Name == "Python");
        var leadership = skills.FirstOrDefault(s => s.Name == "Leadership");
        var comm = skills.FirstOrDefault(s => s.Name == "Communication");
        var pm = skills.FirstOrDefault(s => s.Name == "Project Management");
        var hrComp = skills.FirstOrDefault(s => s.Name == "HR Compliance");
        var rec = skills.FirstOrDefault(s => s.Name == "Recruitment");
        var fin = skills.FirstOrDefault(s => s.Name == "Financial Analysis");

        var reqs = new List<PositionSkillRequirement>();

        void AddReq(string position, Skill? skill, int min)
        {
            if (skill != null)
                reqs.Add(new PositionSkillRequirement { Position = position, SkillId = skill.Id, MinimumProficiency = min });
        }

        // Software Engineer
        AddReq("Software Engineer", csharp, 3);
        AddReq("Software Engineer", react, 2);
        AddReq("Software Engineer", ts, 2);
        AddReq("Software Engineer", sql, 2);
        AddReq("Software Engineer", docker, 1);

        // Senior Software Engineer
        AddReq("Senior Software Engineer", csharp, 4);
        AddReq("Senior Software Engineer", react, 3);
        AddReq("Senior Software Engineer", ts, 3);
        AddReq("Senior Software Engineer", sql, 3);
        AddReq("Senior Software Engineer", azure, 2);
        AddReq("Senior Software Engineer", docker, 2);
        AddReq("Senior Software Engineer", leadership, 2);
        AddReq("Senior Software Engineer", comm, 3);

        // Tech Lead
        AddReq("Tech Lead", csharp, 5);
        AddReq("Tech Lead", react, 4);
        AddReq("Tech Lead", ts, 4);
        AddReq("Tech Lead", sql, 4);
        AddReq("Tech Lead", azure, 3);
        AddReq("Tech Lead", docker, 3);
        AddReq("Tech Lead", k8s, 2);
        AddReq("Tech Lead", leadership, 4);
        AddReq("Tech Lead", comm, 4);
        AddReq("Tech Lead", pm, 3);

        // DevOps Engineer
        AddReq("DevOps Engineer", docker, 4);
        AddReq("DevOps Engineer", k8s, 4);
        AddReq("DevOps Engineer", azure, 4);
        AddReq("DevOps Engineer", python, 3);
        AddReq("DevOps Engineer", sql, 2);
        AddReq("DevOps Engineer", csharp, 1);

        // HR Manager
        AddReq("HR Manager", hrComp, 4);
        AddReq("HR Manager", rec, 4);
        AddReq("HR Manager", comm, 4);
        AddReq("HR Manager", leadership, 3);
        AddReq("HR Manager", pm, 2);

        // Product Manager
        AddReq("Product Manager", pm, 4);
        AddReq("Product Manager", comm, 5);
        AddReq("Product Manager", leadership, 4);
        AddReq("Product Manager", sql, 1);
        AddReq("Product Manager", react, 1);

        // Frontend Developer
        AddReq("Frontend Developer", react, 4);
        AddReq("Frontend Developer", ts, 4);
        AddReq("Frontend Developer", csharp, 1);
        AddReq("Frontend Developer", sql, 1);
        AddReq("Frontend Developer", docker, 1);

        // Backend Developer
        AddReq("Backend Developer", csharp, 4);
        AddReq("Backend Developer", sql, 3);
        AddReq("Backend Developer", azure, 2);
        AddReq("Backend Developer", docker, 2);
        AddReq("Backend Developer", ts, 1);

        // Data Analyst
        AddReq("Data Analyst", python, 3);
        AddReq("Data Analyst", sql, 3);
        AddReq("Data Analyst", comm, 3);
        AddReq("Data Analyst", csharp, 1);

        context.PositionSkillRequirements.AddRange(reqs);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAssets(HRMSDbContext context)
    {
        if (await context.Assets.AnyAsync()) return;
        var emps = await context.Employees.Where(e => !e.IsDeleted).ToListAsync();
        if (emps.Count < 3) return;

        var david = emps.First(e => e.Email == "david@hrms.com");
        var priya = emps.First(e => e.Email == "priya@hrms.com");
        var michael = emps.First(e => e.Email == "michael@hrms.com");
        var amanda = emps.First(e => e.Email == "amanda@hrms.com");

        var assets = new List<Asset>
        {
            new() { Name = "MacBook Pro 16\" M3", AssetTag = "LAP-001", Category = "Laptop", Model = "MacBook Pro 2024",
                SerialNumber = "SN-MBP-001", PurchaseDate = new DateTime(2024, 1, 15), PurchasePrice = 3499, WarrantyExpiry = new DateTime(2027, 1, 15),
                Status = "Allocated" },
            new() { Name = "Dell UltraSharp 27\"", AssetTag = "MON-001", Category = "Monitor", Model = "U2723QE",
                SerialNumber = "SN-DEL-001", PurchaseDate = new DateTime(2024, 2, 1), PurchasePrice = 619, WarrantyExpiry = new DateTime(2027, 2, 1),
                Status = "Allocated" },
            new() { Name = "iPhone 15 Pro", AssetTag = "PHN-001", Category = "Phone", Model = "iPhone 15 Pro 256GB",
                SerialNumber = "SN-IPH-001", PurchaseDate = new DateTime(2024, 3, 10), PurchasePrice = 1099, WarrantyExpiry = new DateTime(2026, 3, 10),
                Status = "Allocated" },
            new() { Name = "Logitech MX Keys", AssetTag = "ACC-001", Category = "Accessory", Model = "MX Keys Advanced",
                SerialNumber = "SN-LOG-001", PurchaseDate = new DateTime(2024, 4, 5), PurchasePrice = 199, WarrantyExpiry = new DateTime(2026, 4, 5),
                Status = "Available" },
            new() { Name = "MacBook Air 15\" M3", AssetTag = "LAP-002", Category = "Laptop", Model = "MacBook Air 2024",
                SerialNumber = "SN-MBA-001", PurchaseDate = new DateTime(2024, 5, 1), PurchasePrice = 1299, WarrantyExpiry = new DateTime(2027, 5, 1),
                Status = "Available" },
            new() { Name = "Dell XPS 15", AssetTag = "LAP-003", Category = "Laptop", Model = "XPS 15 9530",
                SerialNumber = "SN-DXPS-001", PurchaseDate = new DateTime(2023, 11, 1), PurchasePrice = 2499, WarrantyExpiry = new DateTime(2026, 11, 1),
                Status = "Maintenance" },
        };
        context.Assets.AddRange(assets);
        await context.SaveChangesAsync();

        var saved = await context.Assets.ToListAsync();
        var allocations = new List<AssetAllocation>
        {
            new() { AssetId = saved.First(a => a.AssetTag == "LAP-001").Id, EmployeeId = david.Id, AllocatedAt = DateTime.UtcNow.AddMonths(-6) },
            new() { AssetId = saved.First(a => a.AssetTag == "MON-001").Id, EmployeeId = david.Id, AllocatedAt = DateTime.UtcNow.AddMonths(-5) },
            new() { AssetId = saved.First(a => a.AssetTag == "PHN-001").Id, EmployeeId = michael.Id, AllocatedAt = DateTime.UtcNow.AddMonths(-3) },
        };
        context.AssetAllocations.AddRange(allocations);
        await context.SaveChangesAsync();

        context.AssetMaintenances.Add(new AssetMaintenance
        {
            AssetId = saved.First(a => a.AssetTag == "LAP-003").Id,
            Description = "Battery replacement needed - swelling detected",
            Type = "Repair", Cost = 299, Status = "Pending", ScheduledDate = DateTime.UtcNow.AddDays(7)
        });
        await context.SaveChangesAsync();
    }

    private static async Task SeedTraining(HRMSDbContext context)
    {
        if (await context.Courses.AnyAsync()) return;
        var emps = await context.Employees.Where(e => !e.IsDeleted).ToListAsync();
        if (emps.Count < 3) return;

        var michael = emps.First(e => e.Email == "michael@hrms.com");
        var david = emps.First(e => e.Email == "david@hrms.com");
        var priya = emps.First(e => e.Email == "priya@hrms.com");
        var amanda = emps.First(e => e.Email == "amanda@hrms.com");
        var sarah = emps.First(e => e.Email == "sarah@hrms.com");

        var courses = new List<Course>
        {
            new() { Title = "Advanced .NET Architecture", Category = "Technical", Instructor = "Dr. James Wilson",
                DurationHours = 16, MaxCapacity = 15, Status = "Active", CreatedById = michael.Id,
                Description = "Deep dive into modern .NET architecture patterns including microservices, CQRS, Event Sourcing, and DDD." },
            new() { Title = "Leadership & Management Essentials", Category = "Leadership", Instructor = "Sarah Johnson",
                DurationHours = 8, MaxCapacity = 20, Status = "Active", CreatedById = sarah.Id,
                Description = "Core leadership skills for new and aspiring managers. Covers communication, delegation, conflict resolution, and team motivation." },
            new() { Title = "React Performance Optimization", Category = "Technical", Instructor = "Guest Speaker",
                DurationHours = 4, MaxCapacity = 25, Status = "Active", CreatedById = michael.Id,
                Description = "Advanced React patterns for optimal rendering performance. Memoization, code splitting, virtualization, and profiling." },
            new() { Title = "HR Compliance 2026", Category = "Compliance", Instructor = "Emily Rodriguez",
                DurationHours = 6, MaxCapacity = 30, Status = "Active", CreatedById = emps.First(e => e.Email == "emily@hrms.com").Id,
                Description = "Annual HR compliance training covering employment law updates, anti-harassment, data privacy, and workplace safety." },
        };
        context.Courses.AddRange(courses);
        await context.SaveChangesAsync();

        var savedCourses = await context.Courses.ToListAsync();
        context.TrainingEnrollments.AddRange(
            new TrainingEnrollment { CourseId = savedCourses[0].Id, EmployeeId = david.Id, Status = "Completed", Score = 92, CompletedAt = DateTime.UtcNow.AddDays(-14) },
            new TrainingEnrollment { CourseId = savedCourses[0].Id, EmployeeId = priya.Id, Status = "InProgress" },
            new TrainingEnrollment { CourseId = savedCourses[0].Id, EmployeeId = amanda.Id, Status = "Enrolled" },
            new TrainingEnrollment { CourseId = savedCourses[1].Id, EmployeeId = michael.Id, Status = "Enrolled" },
            new TrainingEnrollment { CourseId = savedCourses[2].Id, EmployeeId = priya.Id, Status = "Completed", Score = 88, CompletedAt = DateTime.UtcNow.AddDays(-7) },
            new TrainingEnrollment { CourseId = savedCourses[2].Id, EmployeeId = david.Id, Status = "Completed", Score = 95, CompletedAt = DateTime.UtcNow.AddDays(-5) }
        );
        await context.SaveChangesAsync();

        // Certifications
        if (!await context.Certifications.AnyAsync())
        {
            context.Certifications.AddRange(
                new Certification { Name = "AWS Solutions Architect", Issuer = "Amazon", ExpiryDays = 1095 },
                new Certification { Name = "Microsoft Certified: Azure Developer", Issuer = "Microsoft", ExpiryDays = 365 },
                new Certification { Name = "PMP - Project Management Professional", Issuer = "PMI", ExpiryDays = 1095 },
                new Certification { Name = "CISSP - Cybersecurity", Issuer = "ISC2", ExpiryDays = 1095 },
                new Certification { Name = "Professional in Human Resources (PHR)", Issuer = "HRCI", ExpiryDays = 1095 }
            );
            await context.SaveChangesAsync();

            var savedCerts = await context.Certifications.ToListAsync();
            context.EmployeeCertifications.AddRange(
                new EmployeeCertification { EmployeeId = david.Id, CertificationId = savedCerts[0].Id, ObtainedAt = DateTime.UtcNow.AddMonths(-6), ExpiryDate = DateTime.UtcNow.AddYears(3).AddMonths(-6) },
                new EmployeeCertification { EmployeeId = priya.Id, CertificationId = savedCerts[1].Id, ObtainedAt = DateTime.UtcNow.AddMonths(-2) },
                new EmployeeCertification { EmployeeId = michael.Id, CertificationId = savedCerts[3].Id, ObtainedAt = DateTime.UtcNow.AddYears(-1), ExpiryDate = DateTime.UtcNow.AddYears(2) }
            );
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedNotifications(HRMSDbContext context)
    {
        if (await context.Notifications.AnyAsync()) return;
        var users = await context.Users.ToListAsync();
        if (users.Count == 0) return;

        var notifs = new List<Notification>
        {
            new() { UserId = users[0].Id, Title = "Welcome to EWXP! 🎉", Message = "The new AI-powered HR platform is live. Explore AI Copilot, Org Chart, and more!", Type = "System", Link = "/" },
            new() { UserId = users[0].Id, Title = "Q2 Performance Review", Message = "Your Q2 performance review is ready for review. Please submit your self-assessment.", Type = "Review", Link = "/performance" },
            new() { UserId = users[0].Id, Title = "Leave Approved", Message = "Your annual leave request for next week has been approved.", Type = "Leave", Link = "/leave" },
        };
        if (users.Count > 1)
        {
            notifs.Add(new() { UserId = users[1].Id, Title = "New Team Member", Message = "Amanda Lee has joined your team. Please complete the onboarding checklist.", Type = "Team", Link = "/employees" });
            notifs.Add(new() { UserId = users[2].Id, Title = "Expense Report", Message = "A new expense report is pending your approval.", Type = "Expense", Link = "/expense" });
        }
        context.Notifications.AddRange(notifs);
        await context.SaveChangesAsync();
    }

    private static async Task SeedDocuments(HRMSDbContext context)
    {
        if (await context.Documents.AnyAsync()) return;
        var emps = await context.Employees.Where(e => !e.IsDeleted).ToListAsync();
        var adminUser = await context.Users.Include(u => u.Employee).FirstAsync(u => u.Email == "admin@hrms.com");
        if (emps.Count < 2) return;

        var david = emps.First(e => e.Email == "david@hrms.com");
        var priya = emps.First(e => e.Email == "priya@hrms.com");

        context.Documents.AddRange(
            new Document
            {
                Title = "Offer Letter - David Kim", Category = "Offer Letter",
                FileName = "offer-letter-david-kim.pdf", FileSize = 245760, FileType = "pdf",
                UploadedById = adminUser.EmployeeId!.Value, EmployeeId = david.Id,
                UploadedAt = david.DateOfJoining!.Value.AddDays(-14), Status = "Active"
            },
            new Document
            {
                Title = "Employment Contract", Category = "Contract",
                FileName = "employment-contract-david.pdf", FileSize = 389120, FileType = "pdf",
                UploadedById = adminUser.EmployeeId!.Value, EmployeeId = david.Id,
                UploadedAt = david.DateOfJoining!.Value, Status = "Active"
            },
            new Document
            {
                Title = "Offer Letter - Priya Patel", Category = "Offer Letter",
                FileName = "offer-letter-priya.pdf", FileSize = 241664, FileType = "pdf",
                UploadedById = adminUser.EmployeeId!.Value, EmployeeId = priya.Id,
                UploadedAt = priya.DateOfJoining!.Value.AddDays(-10), Status = "Active"
            },
            new Document
            {
                Title = "NDA Agreement", Category = "Contract",
                FileName = "nda-agreement.pdf", FileSize = 182272, FileType = "pdf",
                UploadedById = adminUser.EmployeeId!.Value,
                UploadedAt = DateTime.UtcNow.AddMonths(-1), Status = "Active"
            },
            new Document
            {
                Title = "Company Policy Handbook 2026", Category = "Policy",
                FileName = "employee-handbook-2026.pdf", FileSize = 1024000, FileType = "pdf",
                UploadedById = adminUser.EmployeeId!.Value,
                UploadedAt = DateTime.UtcNow.AddMonths(-2), Status = "Active",
                ExpiryDate = DateTime.UtcNow.AddMonths(10)
            }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedCompliance(HRMSDbContext context)
    {
        if (await context.ComplianceRecords.AnyAsync()) return;
        var emps = await context.Employees.Where(e => !e.IsDeleted).ToListAsync();
        if (emps.Count < 3) return;

        var emily = emps.First(e => e.Email == "emily@hrms.com");
        var robert = emps.First(e => e.Email == "robert@hrms.com");
        var michael = emps.First(e => e.Email == "michael@hrms.com");

        context.ComplianceRecords.AddRange(
            new ComplianceRecord
            {
                Title = "Annual Tax Filing - Q4 2026", Category = "Statutory",
                Regulation = "IRS Section 6011", Status = "Pending",
                DueDate = DateTime.UtcNow.AddMonths(2), AssignedToId = robert.Id,
                Notes = "Prepare and file quarterly tax returns for all employees."
            },
            new ComplianceRecord
            {
                Title = "Employee Data Privacy Audit", Category = "GDPR",
                Regulation = "GDPR Article 30", Status = "Pending",
                DueDate = DateTime.UtcNow.AddMonths(1), AssignedToId = emily.Id,
                Notes = "Review all employee data processing records for GDPR compliance."
            },
            new ComplianceRecord
            {
                Title = "Workplace Safety Inspection", Category = "Statutory",
                Regulation = "OSHA Standards", Status = "Completed",
                DueDate = DateTime.UtcNow.AddDays(-10), CompletedAt = DateTime.UtcNow.AddDays(-12),
                CompletedById = michael.Id, AssignedToId = michael.Id,
                Notes = "Annual workplace safety inspection completed. All clear."
            },
            new ComplianceRecord
            {
                Title = "Anti-Harassment Training Completion", Category = "Statutory",
                Regulation = "EEOC Guidelines", Status = "InProgress",
                DueDate = DateTime.UtcNow.AddMonths(3), AssignedToId = emily.Id,
                Notes = "Ensure 100% completion of annual anti-harassment training for all employees."
            },
            new ComplianceRecord
            {
                Title = "Data Retention Policy Review", Category = "GDPR",
                Regulation = "GDPR Article 5", Status = "Pending",
                DueDate = DateTime.UtcNow.AddDays(45), AssignedToId = emily.Id,
                Notes = "Review and update data retention schedules for HR records."
            }
        );
        await context.SaveChangesAsync();

        // Seed audit logs
        if (!await context.AuditLogs.AnyAsync())
        {
            var users = await context.Users.ToListAsync();
            context.AuditLogs.AddRange(
                new AuditLog { Action = "Created Employee", EntityType = "Employee", EntityId = 1, Details = "Created employee Sarah Johnson", UserId = users[0].Id, Timestamp = DateTime.UtcNow.AddDays(-90) },
                new AuditLog { Action = "Approved Leave", EntityType = "LeaveRequest", EntityId = 1, Details = "Approved annual leave for David Kim", UserId = users[1].Id, Timestamp = DateTime.UtcNow.AddDays(-60) },
                new AuditLog { Action = "Updated Payroll", EntityType = "Payroll", EntityId = 1, Details = "Updated salary component for Michael Chen", UserId = users[0].Id, Timestamp = DateTime.UtcNow.AddDays(-30) },
                new AuditLog { Action = "Promoted Employee", EntityType = "PromotionRequest", EntityId = 1, Details = "Promoted David Kim to Staff Engineer", UserId = users[0].Id, Timestamp = DateTime.UtcNow.AddDays(-14) },
                new AuditLog { Action = "Completed Training", EntityType = "TrainingEnrollment", EntityId = 1, Details = "David Kim completed Advanced .NET Architecture with 92%", UserId = users[1].Id, Timestamp = DateTime.UtcNow.AddDays(-7) },
                new AuditLog { Action = "System Configuration", EntityType = "SystemSetting", EntityId = null, Details = "Updated company name setting", UserId = users[0].Id, Timestamp = DateTime.UtcNow.AddDays(-1) }
            );
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedExpenseReports(HRMSDbContext context)
    {
        if (await context.ExpenseReports.AnyAsync()) return;
        var emps = await context.Employees.ToListAsync();
        var david = emps.First(e => e.Email == "david@hrms.com");
        var priya = emps.First(e => e.Email == "priya@hrms.com");
        var daniel = emps.First(e => e.Email == "daniel@hrms.com");
        var emily = emps.First(e => e.Email == "emily@hrms.com");
        var today = DateTime.UtcNow;

        context.ExpenseReports.AddRange(
            new ExpenseReport
            {
                Title = "Q2 Client Visit - NYC", Description = "Travel for enterprise client onboarding",
                TotalAmount = 1245.50m, Status = "Approved", EmployeeId = david.Id,
                ReviewedById = emily.Id, ReviewNotes = "Approved within policy limits.",
                ReviewedAt = today.AddDays(-5), CreatedAt = today.AddDays(-8),
                LineItems = new List<ExpenseLineItem>
                {
                    new() { Category = "Travel", Description = "Flight SFO → JFK", Amount = 520, ExpenseDate = today.AddDays(-10) },
                    new() { Category = "Travel", Description = "Hotel 2 nights", Amount = 480, ExpenseDate = today.AddDays(-9) },
                    new() { Category = "Food", Description = "Client dinner", Amount = 145.50m, ExpenseDate = today.AddDays(-9) },
                    new() { Category = "Travel", Description = "Uber / local transport", Amount = 100, ExpenseDate = today.AddDays(-8) },
                }
            },
            new ExpenseReport
            {
                Title = "Home Office Setup", Description = "Monitor stand and ergonomic keyboard",
                TotalAmount = 289.99m, Status = "Pending", EmployeeId = priya.Id,
                CreatedAt = today.AddDays(-2),
                LineItems = new List<ExpenseLineItem>
                {
                    new() { Category = "Equipment", Description = "Monitor arm", Amount = 189.99m, ExpenseDate = today.AddDays(-3) },
                    new() { Category = "Equipment", Description = "Ergonomic keyboard", Amount = 100, ExpenseDate = today.AddDays(-3) },
                }
            },
            new ExpenseReport
            {
                Title = "Team Lunch - Sprint Retro", Description = "Monthly team lunch",
                TotalAmount = 156.75m, Status = "Paid", EmployeeId = daniel.Id,
                ReviewedById = emily.Id, ReviewNotes = "Reimbursed via payroll.",
                ReviewedAt = today.AddDays(-20), CreatedAt = today.AddDays(-25),
                LineItems = new List<ExpenseLineItem>
                {
                    new() { Category = "Food", Description = "Restaurant bill", Amount = 156.75m, ExpenseDate = today.AddDays(-26) },
                }
            },
            new ExpenseReport
            {
                Title = "Conference Registration", Description = "React Summit 2026 ticket",
                TotalAmount = 499, Status = "Rejected", EmployeeId = david.Id,
                ReviewedById = emily.Id, ReviewNotes = "Budget exhausted for Q2 conferences.",
                ReviewedAt = today.AddDays(-3), CreatedAt = today.AddDays(-4),
                LineItems = new List<ExpenseLineItem>
                {
                    new() { Category = "Other", Description = "Conference ticket", Amount = 499, ExpenseDate = today.AddDays(-4) },
                }
            }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedNoCodePlatform(HRMSDbContext context)
    {
        if (!await context.FormDefinitions.AnyAsync())
        {
            var feedbackForm = new FormDefinition
            {
                Title = "Employee Pulse Survey",
                Description = "Quarterly engagement and satisfaction check-in",
                Schema = """[{"id":"satisfaction","label":"Overall Satisfaction","type":"select","required":true,"options":["Excellent","Good","Fair","Poor"]},{"id":"comments","label":"What can we improve?","type":"textarea","required":false,"placeholder":"Share your thoughts..."},{"id":"recommend","label":"Would you recommend us as a workplace?","type":"select","required":true,"options":["Yes","Maybe","No"]}]""",
                Submissions = new List<FormSubmission>
                {
                    new() { Data = """{"satisfaction":"Good","comments":"Love the new AI Copilot!","recommend":"Yes"}""", SubmittedBy = "priya@hrms.com", SubmittedAt = DateTime.UtcNow.AddDays(-3) },
                    new() { Data = """{"satisfaction":"Excellent","comments":"Great team culture and flexibility.","recommend":"Yes"}""", SubmittedBy = "david@hrms.com", SubmittedAt = DateTime.UtcNow.AddDays(-2) },
                    new() { Data = """{"satisfaction":"Fair","comments":"Need better meeting room availability.","recommend":"Maybe"}""", SubmittedBy = "amanda@hrms.com", SubmittedAt = DateTime.UtcNow.AddDays(-1) },
                }
            };
            var itRequestForm = new FormDefinition
            {
                Title = "IT Support Request",
                Description = "Request hardware, software, or access changes",
                Schema = """[{"id":"category","label":"Request Type","type":"select","required":true,"options":["Hardware","Software","Access","Other"]},{"id":"details","label":"Details","type":"textarea","required":true},{"id":"urgent","label":"Urgent?","type":"checkbox","required":false}]""",
                Submissions = new List<FormSubmission>
                {
                    new() { Data = """{"category":"Hardware","details":"Need second monitor for home office","urgent":false}""", SubmittedBy = "michael@hrms.com", SubmittedAt = DateTime.UtcNow.AddDays(-5) },
                }
            };
            context.FormDefinitions.AddRange(feedbackForm, itRequestForm);
            await context.SaveChangesAsync();
        }

        if (!await context.WorkflowDefinitions.AnyAsync())
        {
            var leaveWf = new WorkflowDefinition
            {
                Name = "Leave Approval Workflow",
                Description = "Standard leave request: Manager → HR review",
                Steps = """[{"name":"Manager Approval","assigneeRole":"Manager","action":"Approve"},{"name":"HR Review","assigneeRole":"HRManager","action":"Review"},{"name":"Payroll Notification","assigneeRole":"PayrollStaff","action":"Notify"}]""",
                Instances = new List<WorkflowInstance>
                {
                    new() { RecordId = 1, Status = "InProgress", CurrentStep = "HR Review", Data = """{"leaveRequestId":1,"days":3}""", CreatedAt = DateTime.UtcNow.AddDays(-2) },
                    new() { RecordId = 2, Status = "Approved", CurrentStep = "Payroll Notification", Data = """{"leaveRequestId":2,"days":5}""", CreatedAt = DateTime.UtcNow.AddDays(-10), CompletedAt = DateTime.UtcNow.AddDays(-8) },
                }
            };
            var expenseWf = new WorkflowDefinition
            {
                Name = "Expense Approval Workflow",
                Description = "Multi-step expense report approval",
                Steps = """[{"name":"Manager Review","assigneeRole":"Manager","action":"Approve"},{"name":"Finance Review","assigneeRole":"PayrollStaff","action":"Approve"}]""",
                Instances = new List<WorkflowInstance>
                {
                    new() { RecordId = 1, Status = "InProgress", CurrentStep = "Manager Review", Data = """{"expenseReportId":2,"amount":289.99}""", CreatedAt = DateTime.UtcNow.AddDays(-1) },
                }
            };
            context.WorkflowDefinitions.AddRange(leaveWf, expenseWf, new WorkflowDefinition
            {
                Name = "Promotion Approval",
                Description = "Manager → HRBP → Dept Head → CEO",
                Steps = """[{"name":"Manager","assigneeRole":"Manager","action":"Approve"},{"name":"HRBP","assigneeRole":"HRManager","action":"Review"},{"name":"Dept Head","assigneeRole":"Manager","action":"Approve"},{"name":"CEO","assigneeRole":"Administrator","action":"Approve"}]""",
            });
            await context.SaveChangesAsync();
        }

        if (!await context.ReportDefinitions.AnyAsync())
        {
            context.ReportDefinitions.AddRange(
                new ReportDefinition
                {
                    Title = "Active Employee Directory",
                    DataSource = "Employee",
                    Columns = """["EmployeeCode","FirstName","LastName","Email","Position","DepartmentName","Status"]"""
                },
                new ReportDefinition
                {
                    Title = "Leave Summary Report",
                    DataSource = "Leave",
                    Columns = """["EmployeeName","LeaveTypeName","StartDate","EndDate","Status","Reason"]"""
                },
                new ReportDefinition
                {
                    Title = "Monthly Attendance Overview",
                    DataSource = "Attendance",
                    Columns = """["EmployeeName","Date","CheckInTime","CheckOutTime","Status"]"""
                },
                new ReportDefinition
                {
                    Title = "Open Positions Pipeline",
                    DataSource = "Recruitment",
                    Columns = """["Title","DepartmentName","Status","CandidateCount"]"""
                }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.CustomFields.AnyAsync())
        {
            var tshirtField = new CustomField { Module = "Employee", FieldName = "T-Shirt Size", FieldType = "Select", Options = """["XS","S","M","L","XL","XXL"]""", SortOrder = 1 };
            var linkedinField = new CustomField { Module = "Employee", FieldName = "LinkedIn Profile", FieldType = "Text", SortOrder = 2 };
            var remoteField = new CustomField { Module = "Employee", FieldName = "Remote Work", FieldType = "Boolean", SortOrder = 3 };
            var leaveReasonField = new CustomField { Module = "Leave", FieldName = "Backup Contact", FieldType = "Text", SortOrder = 1 };
            context.CustomFields.AddRange(tshirtField, linkedinField, remoteField, leaveReasonField);
            await context.SaveChangesAsync();

            var david = await context.Employees.FirstAsync(e => e.Email == "david@hrms.com");
            var priya = await context.Employees.FirstAsync(e => e.Email == "priya@hrms.com");
            context.CustomFieldValues.AddRange(
                new CustomFieldValue { CustomFieldId = tshirtField.Id, RecordId = david.Id, Value = "L" },
                new CustomFieldValue { CustomFieldId = linkedinField.Id, RecordId = david.Id, Value = "linkedin.com/in/davidkim" },
                new CustomFieldValue { CustomFieldId = remoteField.Id, RecordId = david.Id, Value = "true" },
                new CustomFieldValue { CustomFieldId = tshirtField.Id, RecordId = priya.Id, Value = "M" },
                new CustomFieldValue { CustomFieldId = remoteField.Id, RecordId = priya.Id, Value = "true" }
            );
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedRecruitmentPipeline(HRMSDbContext context)
    {
        if (!await context.HiringRequests.AnyAsync())
        {
            var emps = await context.Employees.ToListAsync();
            var depts = await context.Departments.ToListAsync();
            var michael = emps.First(e => e.Email == "michael@hrms.com");
            var james = emps.First(e => e.Email == "james@hrms.com");
            var eng = depts.First(d => d.Name == "Engineering");
            var sales = depts.First(d => d.Name == "Sales");
            var devOpsJob = await context.JobRequisitions.FirstAsync(j => j.Title.Contains("DevOps"));

            context.HiringRequests.AddRange(
                new HiringRequest
                {
                    JobTitle = "Platform Engineer", Description = "Build internal developer platform",
                    Justification = "Reduce deployment friction across 5 teams", Headcount = 2,
                    BudgetRangeLow = 130000, BudgetRangeHigh = 160000, EmploymentType = "FullTime",
                    Status = "PendingDeptApproval", RequestedById = michael.Id, DepartmentId = eng.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new HiringRequest
                {
                    JobTitle = "Senior QA Engineer", Description = "Lead test automation initiative",
                    Justification = "Improve release quality and reduce regression bugs", Headcount = 1,
                    BudgetRangeLow = 95000, BudgetRangeHigh = 115000, EmploymentType = "FullTime",
                    Status = "PendingHrApproval", RequestedById = michael.Id, DepartmentId = eng.Id,
                    DeptApprovalNotes = "Approved — critical for Q3 release quality.",
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new HiringRequest
                {
                    JobTitle = "Enterprise Account Executive", Description = "Manage Fortune 500 accounts",
                    Justification = "Expand enterprise segment in North America", Headcount = 1,
                    BudgetRangeLow = 90000, BudgetRangeHigh = 120000, EmploymentType = "FullTime",
                    Status = "PendingBudgetApproval", RequestedById = james.Id, DepartmentId = sales.Id,
                    DeptApprovalNotes = "Sales headcount approved.", HrApprovalNotes = "JD finalized and posted internally.",
                    CreatedAt = DateTime.UtcNow.AddDays(-12)
                },
                new HiringRequest
                {
                    JobTitle = "DevOps Engineer", Description = "CI/CD and cloud infrastructure",
                    Justification = "Backfill for promoted engineer", Headcount = 1,
                    BudgetRangeLow = 120000, BudgetRangeHigh = 145000, EmploymentType = "FullTime",
                    Status = "Approved", RequestedById = michael.Id, DepartmentId = eng.Id,
                    JobRequisitionId = devOpsJob.Id,
                    DeptApprovalNotes = "Approved.", HrApprovalNotes = "Approved.", BudgetApprovalNotes = "Budget allocated.",
                    CreatedAt = DateTime.UtcNow.AddDays(-20), UpdatedAt = DateTime.UtcNow.AddDays(-5)
                }
            );
            await context.SaveChangesAsync();
        }

        var candidates = await context.CandidateProfiles.ToListAsync();
        if (candidates.Count == 0) return;
        var emilyEmp = await context.Employees.FirstAsync(e => e.Email == "emily@hrms.com");
        var sophie = candidates.FirstOrDefault(c => c.FirstName == "Sophie");
        var maria = candidates.FirstOrDefault(c => c.FirstName == "Maria");
        var alex = candidates.FirstOrDefault(c => c.FirstName == "Alex");

        if (!await context.InterviewSchedules.AnyAsync() && maria != null)
        {
            context.InterviewSchedules.AddRange(
                new InterviewSchedule { CandidateId = maria.Id, ScheduledDate = DateTime.UtcNow.AddDays(-5), InterviewerName = "Michael Chen", InterviewType = "Technical", Status = "Completed", Feedback = "Strong system design skills. Recommended for next round.", Rating = 4 },
                new InterviewSchedule { CandidateId = maria.Id, ScheduledDate = DateTime.UtcNow.AddDays(-2), InterviewerName = "David Kim", InterviewType = "Culture Fit", Status = "Completed", Feedback = "Great collaboration mindset.", Rating = 5 }
            );
        }

        if (!await context.Offers.AnyAsync() && sophie != null && maria != null)
        {
            context.Offers.AddRange(
                new Offer { CandidateId = sophie.Id, Salary = 1800000, Currency = "INR", Benefits = "Health insurance, PF, gratuity, 24 paid leaves", StartDate = DateTime.UtcNow.AddMonths(1), Status = "Approved", ApprovedById = emilyEmp.Id, CreatedAt = DateTime.UtcNow.AddDays(-5) },
                new Offer { CandidateId = maria.Id, Salary = 2400000, Currency = "INR", Benefits = "Health insurance, PF, ESOP, flexible hybrid", StartDate = DateTime.UtcNow.AddMonths(2), Status = "Draft", CreatedAt = DateTime.UtcNow.AddDays(-1) }
            );
        }

        if (!await context.BackgroundChecks.AnyAsync() && sophie != null)
        {
            context.BackgroundChecks.Add(new BackgroundCheck
            {
                CandidateId = sophie.Id, VendorName = "CheckR", Status = "Cleared",
                Notes = "All checks passed. Employment and education verified.",
                InitiatedAt = DateTime.UtcNow.AddDays(-8), CompletedAt = DateTime.UtcNow.AddDays(-3)
            });
        }

        if (!await context.OnboardingTasks.AnyAsync() && sophie != null)
        {
            context.OnboardingTasks.AddRange(
                new OnboardingTask { CandidateId = sophie.Id, Title = "Sign offer letter", Category = "Document", AssignedTo = "HR", IsCompleted = true, CompletedAt = DateTime.UtcNow.AddDays(-2) },
                new OnboardingTask { CandidateId = sophie.Id, Title = "Complete W-4 tax form", Category = "Document", AssignedTo = "Employee", IsCompleted = false },
                new OnboardingTask { CandidateId = sophie.Id, Title = "IT laptop provisioning", Category = "IT", AssignedTo = "IT", IsCompleted = false },
                new OnboardingTask { CandidateId = sophie.Id, Title = "Day 1 orientation session", Category = "Training", AssignedTo = "HR", IsCompleted = false },
                new OnboardingTask { CandidateId = sophie.Id, Title = "Set up email and Slack", Category = "IT", AssignedTo = "IT", IsCompleted = true, CompletedAt = DateTime.UtcNow.AddDays(-1) }
            );
        }

        if (alex != null && !await context.BackgroundChecks.AnyAsync(b => b.CandidateId == alex.Id))
        {
            context.BackgroundChecks.Add(new BackgroundCheck
            {
                CandidateId = alex.Id, VendorName = "Sterling", Status = "Pending",
                Notes = "Standard employment verification in progress.",
                InitiatedAt = DateTime.UtcNow.AddDays(-2)
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedTalentPools(HRMSDbContext context)
    {
        if (await context.TalentPools.AnyAsync()) return;
        var emps = await context.Employees.ToListAsync();
        var emily = emps.First(e => e.Email == "emily@hrms.com");
        var david = emps.First(e => e.Email == "david@hrms.com");
        var priya = emps.First(e => e.Email == "priya@hrms.com");
        var michael = emps.First(e => e.Email == "michael@hrms.com");
        var amanda = emps.First(e => e.Email == "amanda@hrms.com");

        context.TalentPools.AddRange(
            new TalentPool
            {
                Name = "Future Engineering Leaders",
                Description = "High-potential engineers identified for leadership development",
                CreatedById = emily.Id,
                Candidates = new List<TalentPoolCandidate>
                {
                    new() { EmployeeId = david.Id, Notes = "Ready for staff/principal track", Status = "Active" },
                    new() { EmployeeId = priya.Id, Notes = "Strong growth trajectory — watch for senior promotion", Status = "Active" },
                }
            },
            new TalentPool
            {
                Name = "Executive Succession",
                Description = "Cross-functional leaders for VP-level roles",
                CreatedById = emily.Id,
                Candidates = new List<TalentPoolCandidate>
                {
                    new() { EmployeeId = michael.Id, Notes = "Engineering leadership bench", Status = "Active" },
                    new() { EmployeeId = emily.Id, Notes = "HR operations excellence", Status = "Active" },
                }
            },
            new TalentPool
            {
                Name = "Graduate Hire Pipeline",
                Description = "Early-career talent for accelerated development",
                CreatedById = emily.Id,
                Candidates = new List<TalentPoolCandidate>
                {
                    new() { EmployeeId = amanda.Id, Notes = "Completed probation — assign mentor", Status = "Active" },
                }
            }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedReviewScores(HRMSDbContext context)
    {
        if (await context.ReviewScores.AnyAsync()) return;
        var completedReviews = await context.PerformanceReviews
            .Where(r => r.Status == "Completed" && r.OverallScore != null)
            .Include(r => r.Scores)
            .ToListAsync();
        if (completedReviews.Count == 0) return;

        foreach (var review in completedReviews)
        {
            var baseScore = review.OverallScore!.Value;
            context.ReviewScores.AddRange(
                new ReviewScore { ReviewId = review.Id, Criteria = "Technical Skills", Score = baseScore, Comments = "Consistently delivers high-quality work." },
                new ReviewScore { ReviewId = review.Id, Criteria = "Communication", Score = Math.Max(1, baseScore - 0.3m), Comments = "Clear and proactive in team updates." },
                new ReviewScore { ReviewId = review.Id, Criteria = "Teamwork", Score = Math.Min(5, baseScore + 0.2m), Comments = "Collaborates well across teams." },
                new ReviewScore { ReviewId = review.Id, Criteria = "Initiative", Score = baseScore - 0.1m, Comments = "Takes ownership of complex problems." }
            );
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedDataPrivacyLogs(HRMSDbContext context)
    {
        if (await context.DataPrivacyLogs.AnyAsync()) return;
        var emps = await context.Employees.ToListAsync();
        var users = await context.Users.ToListAsync();
        var david = emps.First(e => e.Email == "david@hrms.com");
        var priya = emps.First(e => e.Email == "priya@hrms.com");
        var emily = emps.First(e => e.Email == "emily@hrms.com");
        var adminUser = users.First(u => u.Email == "admin@hrms.com");

        context.DataPrivacyLogs.AddRange(
            new DataPrivacyLog { Action = "ConsentGranted", DataCategory = "PersonalInfo", Details = "Employee consented to data processing on onboarding", ConsentStatus = "Granted", EmployeeId = david.Id, UserId = adminUser.Id, IpAddress = "192.168.1.10", Timestamp = DateTime.UtcNow.AddDays(-90) },
            new DataPrivacyLog { Action = "DataExport", DataCategory = "PersonalInfo", Details = "Employee requested personal data export (GDPR Article 15)", ConsentStatus = "Granted", EmployeeId = priya.Id, IpAddress = "10.0.0.45", Timestamp = DateTime.UtcNow.AddDays(-14) },
            new DataPrivacyLog { Action = "DataAccess", DataCategory = "Performance", Details = "HR accessed performance review data for calibration", ConsentStatus = "Granted", EmployeeId = david.Id, UserId = users.First(u => u.Email == "emily@hrms.com").Id, IpAddress = "10.0.0.12", Timestamp = DateTime.UtcNow.AddDays(-7) },
            new DataPrivacyLog { Action = "ConsentWithdrawn", DataCategory = "Marketing", Details = "Employee opted out of non-essential communications", ConsentStatus = "Withdrawn", EmployeeId = emily.Id, IpAddress = "10.0.0.8", Timestamp = DateTime.UtcNow.AddDays(-3) },
            new DataPrivacyLog { Action = "DataCorrection", DataCategory = "PersonalInfo", Details = "Updated emergency contact phone number per employee request", ConsentStatus = "Granted", EmployeeId = priya.Id, UserId = adminUser.Id, IpAddress = "192.168.1.10", Timestamp = DateTime.UtcNow.AddDays(-1) },
            new DataPrivacyLog { Action = "DataRetention", DataCategory = "Payroll", Details = "Automated retention review — records within policy limits", ConsentStatus = "Granted", EmployeeId = null, UserId = users.First(u => u.Email == "robert@hrms.com").Id, IpAddress = "10.0.0.20", Timestamp = DateTime.UtcNow.AddHours(-6) }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedWorkplacePlatform(HRMSDbContext context)
    {
        await SeedCollaborationChannels(context);
        await SeedMeetings(context);
        await SeedWorkTasks(context);
        await SeedAnnouncements(context);
        await SeedEnhancedNotifications(context);
    }

    private static async Task SeedCollaborationChannels(HRMSDbContext context)
    {
        if (await context.CollaborationChannels.AnyAsync()) return;
        var depts = await context.Departments.ToListAsync();
        var eng = depts.FirstOrDefault(d => d.Name.Contains("Engineering"));
        var hr = depts.FirstOrDefault(d => d.Name.Contains("HR") || d.Name.Contains("Human"));
        var finance = depts.FirstOrDefault(d => d.Name.Contains("Finance"));
        var sales = depts.FirstOrDefault(d => d.Name.Contains("Sales"));
        var marketing = depts.FirstOrDefault(d => d.Name.Contains("Marketing"));

        var channels = new List<CollaborationChannel>
        {
            new() { Name = "General", Description = "Company-wide discussions", ChannelType = "General" },
            new() { Name = "HR", Description = "HR team channel", ChannelType = "Department", DepartmentId = hr?.Id },
            new() { Name = "Engineering", Description = "Engineering team", ChannelType = "Department", DepartmentId = eng?.Id },
            new() { Name = "Finance", Description = "Finance team", ChannelType = "Department", DepartmentId = finance?.Id },
            new() { Name = "Sales", Description = "Sales team", ChannelType = "Department", DepartmentId = sales?.Id },
            new() { Name = "Marketing", Description = "Marketing team", ChannelType = "Department", DepartmentId = marketing?.Id },
        };
        context.CollaborationChannels.AddRange(channels);
        await context.SaveChangesAsync();

        var emps = await context.Employees.Where(e => !e.IsDeleted).ToListAsync();
        var david = emps.FirstOrDefault(e => e.Email == "david@hrms.com");
        var michael = emps.FirstOrDefault(e => e.Email == "michael@hrms.com");
        var emily = emps.FirstOrDefault(e => e.Email == "emily@hrms.com");
        if (david == null || michael == null) return;

        var engChannel = channels.First(c => c.Name == "Engineering");
        context.ChannelMessages.AddRange(
            new ChannelMessage { ChannelId = engChannel.Id, AuthorId = david.Id, Content = "Sprint 24 planning starts Monday. Please update your Jira boards.", MessageType = "Message", CreatedAt = DateTime.UtcNow.AddHours(-5) },
            new ChannelMessage { ChannelId = engChannel.Id, AuthorId = michael.Id, Content = "Release v2.4 deployed to staging. QA can begin smoke tests.", MessageType = "Announcement", IsPinned = true, CreatedAt = DateTime.UtcNow.AddHours(-2) }
        );
        if (emily != null)
        {
            var hrChannel = channels.First(c => c.Name == "HR");
            context.ChannelMessages.Add(new ChannelMessage { ChannelId = hrChannel.Id, AuthorId = emily.Id, Content = "Q2 performance review cycle opens next week. Managers — please schedule 1:1s.", MessageType = "Announcement", CreatedAt = DateTime.UtcNow.AddHours(-8) });
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedMeetings(HRMSDbContext context)
    {
        if (await context.Meetings.AnyAsync()) return;
        var emps = await context.Employees.Where(e => !e.IsDeleted).ToListAsync();
        var depts = await context.Departments.ToListAsync();
        var michael = emps.FirstOrDefault(e => e.Email == "michael@hrms.com");
        var david = emps.FirstOrDefault(e => e.Email == "david@hrms.com");
        var priya = emps.FirstOrDefault(e => e.Email == "priya@hrms.com");
        var emily = emps.FirstOrDefault(e => e.Email == "emily@hrms.com");
        if (michael == null || david == null) return;

        var eng = depts.FirstOrDefault(d => d.Name.Contains("Engineering"));
        var hrDept = depts.FirstOrDefault(d => d.Name.Contains("HR") || d.Name.Contains("Human"));
        var today = DateTime.UtcNow.Date;

        var meetings = new List<Meeting>
        {
            new()
            {
                Title = "HR Weekly Standup", MeetingType = "HR", Priority = "Normal",
                OrganizerId = emily?.Id ?? michael.Id, DepartmentId = hrDept?.Id,
                StartTime = today.AddHours(9), EndTime = today.AddHours(9).AddMinutes(45),
                Location = "Conference Room A", Status = "Scheduled",
                Agenda = "Leave approvals, recruitment updates, policy reminders"
            },
            new()
            {
                Title = "Engineering Sprint Planning", MeetingType = "Sprint", Priority = "High",
                OrganizerId = michael.Id, DepartmentId = eng?.Id,
                StartTime = today.AddHours(11), EndTime = today.AddHours(12),
                OnlineLink = "https://meet.example.com/sprint-24", Status = "Scheduled",
                Agenda = "Sprint goals, story estimation, capacity planning"
            },
            new()
            {
                Title = "Performance Review — Priya", MeetingType = "1:1", Priority = "Normal",
                OrganizerId = michael.Id,
                StartTime = today.AddHours(14), EndTime = today.AddHours(14).AddMinutes(30),
                Location = "Manager Office", Status = "Scheduled"
            },
            new()
            {
                Title = "Client Demo — Acme Corp", MeetingType = "Client", Priority = "High",
                OrganizerId = david.Id,
                StartTime = today.AddDays(1).AddHours(10), EndTime = today.AddDays(1).AddHours(11),
                OnlineLink = "https://meet.example.com/acme-demo", Status = "Scheduled"
            },
            new()
            {
                Title = "Company Town Hall", MeetingType = "Town Hall", Priority = "Normal",
                OrganizerId = emily?.Id ?? michael.Id,
                StartTime = today.AddDays(3).AddHours(16), EndTime = today.AddDays(3).AddHours(17),
                OnlineLink = "https://meet.example.com/townhall", Status = "Scheduled",
                Agenda = "Q2 results, product roadmap, Q&A"
            }
        };
        context.Meetings.AddRange(meetings);
        await context.SaveChangesAsync();

        var sprint = meetings[1];
        var hrWeekly = meetings[0];
        context.MeetingParticipants.AddRange(
            new MeetingParticipant { MeetingId = hrWeekly.Id, EmployeeId = michael.Id, ResponseStatus = "Accepted" },
            new MeetingParticipant { MeetingId = hrWeekly.Id, EmployeeId = emily?.Id ?? michael.Id, ResponseStatus = "Accepted" },
            new MeetingParticipant { MeetingId = sprint.Id, EmployeeId = david.Id, ResponseStatus = "Accepted" },
            new MeetingParticipant { MeetingId = sprint.Id, EmployeeId = michael.Id, ResponseStatus = "Accepted" },
            new MeetingParticipant { MeetingId = sprint.Id, EmployeeId = priya?.Id ?? david.Id, ResponseStatus = "Pending" },
            new MeetingParticipant { MeetingId = meetings[2].Id, EmployeeId = priya?.Id ?? david.Id, ResponseStatus = "Pending" }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedWorkTasks(HRMSDbContext context)
    {
        if (await context.WorkTasks.AnyAsync()) return;
        var emps = await context.Employees.Where(e => !e.IsDeleted).ToListAsync();
        var david = emps.FirstOrDefault(e => e.Email == "david@hrms.com");
        var priya = emps.FirstOrDefault(e => e.Email == "priya@hrms.com");
        var michael = emps.FirstOrDefault(e => e.Email == "michael@hrms.com");
        if (david == null || priya == null || michael == null) return;

        context.WorkTasks.AddRange(
            new WorkTask { Title = "Update API documentation for v2.4", AssignedToId = david.Id, AssignedById = michael.Id, Priority = "High", DueDate = DateTime.UtcNow.AddDays(3), Status = "InProgress" },
            new WorkTask { Title = "Complete self-assessment for Q2 review", AssignedToId = priya.Id, AssignedById = michael.Id, Priority = "Normal", DueDate = DateTime.UtcNow.AddDays(5), Status = "Pending" },
            new WorkTask { Title = "Review expense policy updates", AssignedToId = michael.Id, AssignedById = david.Id, Priority = "Normal", DueDate = DateTime.UtcNow.AddDays(7), Status = "Pending" },
            new WorkTask { Title = "Prepare sprint retrospective notes", AssignedToId = david.Id, AssignedById = michael.Id, Priority = "Normal", DueDate = DateTime.UtcNow.AddDays(2), Status = "Completed", CompletedAt = DateTime.UtcNow.AddHours(-4) }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedAnnouncements(HRMSDbContext context)
    {
        if (await context.Announcements.AnyAsync()) return;
        var emps = await context.Employees.Where(e => !e.IsDeleted).ToListAsync();
        var emily = emps.FirstOrDefault(e => e.Email == "emily@hrms.com");
        var depts = await context.Departments.ToListAsync();
        var eng = depts.FirstOrDefault(d => d.Name.Contains("Engineering"));
        if (emily == null) return;

        context.Announcements.AddRange(
            new Announcement
            {
                Title = "EWXP Digital Workplace Launch",
                Content = "Welcome to the unified Enterprise Workforce Experience Platform. One login for HR, meetings, collaboration, and AI assistance.",
                Scope = "Company", Priority = "High", CreatedById = emily.Id,
                AcknowledgementRequired = true, CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Announcement
            {
                Title = "Q2 Performance Review Cycle",
                Content = "Performance review cycle opens June 25. All employees must complete self-assessment by July 5.",
                Scope = "Company", Priority = "Normal", CreatedById = emily.Id,
                AcknowledgementRequired = false, CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Announcement
            {
                Title = "Engineering Release Freeze",
                Content = "Code freeze for v2.4 release starts Friday 6 PM. No merges to main without approval.",
                Scope = "Department", Priority = "Urgent", CreatedById = emily.Id,
                DepartmentId = eng?.Id, AcknowledgementRequired = true, CreatedAt = DateTime.UtcNow.AddHours(-6)
            }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedEnhancedNotifications(HRMSDbContext context)
    {
        var users = await context.Users.ToListAsync();
        if (users.Count == 0) return;
        if (await context.Notifications.AnyAsync(n => n.Source == "Meeting")) return;

        var admin = users.First(u => u.Email == "admin@hrms.com");
        var michael = users.FirstOrDefault(u => u.Email == "michael@hrms.com");
        var priya = users.FirstOrDefault(u => u.Email == "priya@hrms.com");

        context.Notifications.AddRange(
            new Notification { UserId = admin.Id, Title = "Town Hall Reminder", Message = "Company Town Hall in 3 days at 4 PM IST", Type = "Meeting", Category = "Reminder", Source = "Meeting", Priority = "Normal", Link = "/meetings" },
            new Notification { UserId = admin.Id, Title = "Task Due Tomorrow", Message = "Update API documentation for v2.4", Type = "Task", Category = "Task", Source = "Meeting", Priority = "High", Link = "/tasks" },
            new Notification { UserId = admin.Id, Title = "Leave Approval Required", Message = "Priya Sharma requested 2 days annual leave", Type = "Leave", Category = "Approval", Source = "Leave", Priority = "Normal", Link = "/approvals" },
            new Notification { UserId = admin.Id, Title = "Payroll Processing", Message = "June payroll run scheduled for tomorrow", Type = "Payroll", Category = "Information", Source = "Payroll", Priority = "Normal", Link = "/payroll" }
        );
        if (michael != null)
        {
            context.Notifications.Add(new Notification { UserId = michael.Id, Title = "Sprint Meeting in 1 hour", Message = "Engineering Sprint Planning at 11:00 AM", Type = "Meeting", Category = "Reminder", Source = "Meeting", Priority = "High", Link = "/meetings", IsRead = false });
            context.Notifications.Add(new Notification { UserId = michael.Id, Title = "Expense Pending Approval", Message = "New expense report from team member", Type = "Expense", Category = "Approval", Source = "Payroll", Priority = "Normal", Link = "/approvals" });
        }
        if (priya != null)
        {
            context.Notifications.Add(new Notification { UserId = priya.Id, Title = "Meeting Invitation", Message = "Performance Review — 1:1 with Michael at 2 PM", Type = "Meeting", Category = "Reminder", Source = "Meeting", Priority = "Normal", Link = "/meetings" });
        }
        await context.SaveChangesAsync();
    }
}
