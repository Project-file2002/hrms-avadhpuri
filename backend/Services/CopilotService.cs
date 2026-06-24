using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.Copilot;
using HRMS.API.Models.Entities;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Services;

public class CopilotService : ICopilotService
{
    private readonly HRMSDbContext _context;
    private readonly IAIService _aiService;

    public CopilotService(HRMSDbContext context, IAIService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    public async Task<CopilotResponse> ProcessMessageAsync(string message, int userId)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Employee).ThenInclude(e => e!.Department)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.Employee == null)
            return new CopilotResponse { Reply = "I couldn't find your employee profile. Please contact HR." };

        var employee = user.Employee;
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var persona = CopilotPermissions.GetPrimaryPersona(roles);
        var msg = message.Trim().ToLowerInvariant();
        var intent = DetectIntent(msg);

        if (!CopilotPermissions.CanAccessIntent(roles, intent))
        {
            return ApplyMeta(new CopilotResponse
            {
                Reply = CopilotPermissions.GetDeniedMessage(intent, persona),
                Intent = intent,
                Restricted = true,
                Actions = new List<CopilotAction>
                {
                    new() { Label = "What can I ask?", Action = "help" }
                }
            }, roles, persona);
        }

        CopilotResponse response = intent switch
        {
            "leave_balance" => await HandleLeaveBalance(employee),
            "leave_apply" => await HandleLeaveApply(employee, msg),
            "my_attendance" => await HandleMyAttendance(employee),
            "team_attendance" => await HandleTeamAttendance(employee),
            "team_members" => await HandleTeamMembers(employee),
            "my_info" => HandleMyInfo(employee, user),
            "department_info" => await HandleDepartmentInfo(employee, msg, roles),
            "employee_count" => await HandleEmployeeCount(),
            "hiring_cost" => await HandleHiringCost(),
            "performance" => await HandlePerformance(employee, roles),
            "payroll" => await HandlePayroll(employee, roles),
            "policy" => await HandlePolicy(msg, roles),
            "team_compare" => await HandleTeamCompare(employee),
            "best_performer" => await HandleBestPerformer(employee),
            "dept_positions" => await HandleDeptPositions(employee, msg),
            "leave_trends" => await HandleLeaveTrends(employee, roles),
            "help" => HandleHelp(roles),
            _ => await HandleGeneral(msg, employee, user, roles)
        };

        return ApplyMeta(response, roles, persona);
    }

    public async Task<CopilotWelcomeResponse> GetWelcomeAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var roles = user?.UserRoles.Select(ur => ur.Role.Name).ToList() ?? ["Employee"];
        var persona = CopilotPermissions.GetPrimaryPersona(roles);
        var firstName = user?.FirstName ?? "there";

        return persona switch
        {
            "admin" => new CopilotWelcomeResponse
            {
                Persona = persona,
                Title = "AI HR Copilot — Admin",
                Subtitle = "Full workforce intelligence & system insights",
                Welcome = $"Hi {firstName}! I'm your **Admin HR Copilot**. I have access to company-wide HR, payroll, recruitment, and analytics data.\n\nAsk about headcount, leave trends, hiring costs, team performance, payroll, or policies.",
                SuggestedPrompts = ["Company headcount", "Leave trends this quarter", "Hiring cost summary", "Open positions in Engineering", "Pending leave overview", "Top performers by department"]
            },
            "hr" => new CopilotWelcomeResponse
            {
                Persona = persona,
                Title = "AI HR Copilot — HR",
                Subtitle = "Workforce ops, recruitment & employee insights",
                Welcome = $"Hi {firstName}! I'm your **HR Copilot**. I can help with employee data, leave trends, recruitment pipeline, department stats, and policies across the organization.",
                SuggestedPrompts = ["Company headcount", "Leave trends this quarter", "Open positions in Engineering", "Hiring cost summary", "Best performer in my department", "Leave policy"]
            },
            "manager" => new CopilotWelcomeResponse
            {
                Persona = persona,
                Title = "AI HR Copilot — Manager",
                Subtitle = "Your team, attendance & performance",
                Welcome = $"Hi {firstName}! I'm your **Manager Copilot**. I focus on your team — attendance, members, performance comparisons, and your own leave & profile.",
                SuggestedPrompts = ["Team attendance today", "Who is in my team?", "Compare my attendance with team", "My leave balance", "Best performer in my department", "My performance reviews"]
            },
            "payroll" => new CopilotWelcomeResponse
            {
                Persona = persona,
                Title = "AI HR Copilot — Payroll",
                Subtitle = "Payroll summaries & workforce counts",
                Welcome = $"Hi {firstName}! I'm your **Payroll Copilot**. I can help with payroll summaries, employee headcount, policies, and your profile — not recruitment or team management.",
                SuggestedPrompts = ["Payroll summary", "Company headcount", "My info", "Leave policy", "Attendance policy"]
            },
            _ => new CopilotWelcomeResponse
            {
                Persona = persona,
                Title = "AI HR Copilot — Employee",
                Subtitle = "Your leave, attendance & personal HR info",
                Welcome = $"Hi {firstName}! I'm your **Employee HR Assistant**. I only access **your personal data** — leave balance, attendance, performance, salary, and profile.\n\nFor team or company questions, please ask your manager or HR.",
                SuggestedPrompts = ["My leave balance", "My attendance this month", "My performance reviews", "My salary details", "Tell me about myself", "Leave policy"]
            }
        };
    }

    private static CopilotResponse ApplyMeta(CopilotResponse response, List<string> roles, string persona)
    {
        response.Persona = persona;
        response.Actions = response.Actions
            .Where(a => FilterAction(a, roles))
            .ToList();
        return response;
    }

    private static bool FilterAction(CopilotAction action, List<string> roles)
    {
        if (!action.Action.StartsWith("navigate:")) return true;
        var path = action.Action["navigate:".Length..];
        if (path.StartsWith("/employees/")) path = "/employees";
        return CopilotPermissions.CanAccessPath(roles, path);
    }

    private static string DetectIntent(string msg)
    {
        // Leave balance
        if (MatchesAny(msg, ["leave balance", "leave bachi", "kitni leave", "remaining leave",
            "leave remaining", "balance leave", "bachi hui leave", "my leaves", "my leave"]))
            return "leave_balance";

        // Leave apply
        if (MatchesAny(msg, ["apply leave", "leave apply", "leave chahiye", "chhutti chahiye",
            "leave lena", "take leave", "request leave", "kal chhutti", "leave kal",
            "leave tomorrow", "chhutti"]) && !msg.Contains("balance"))
            return "leave_apply";

        // My attendance
        if (MatchesAny(msg, ["my attendance", "meri attendance", "mera attendance",
            "my present", "my absent", "attendance dikhao", "main kitne din",
            "maine kitne din", "present count"]))
            return "my_attendance";

        // Team attendance
        if (MatchesAny(msg, ["team attendance", "team ka attendance", "meri team attendance",
            "my team attendance", "team present", "team absent", "reportees attendance",
            "department attendance", "team dikhao"]))
            return "team_attendance";

        // Team members
        if (MatchesAny(msg, ["team members", "team member", "team mein kaun", "meri team",
            "my team", "who reports", "my reportees", "team list", "meri team kaun"]))
            return "team_members";

        // My info
        if (MatchesAny(msg, ["my info", "my details", "my profile", "maine baare mein",
            "mere baare", "about me", "my data", "my information"]))
            return "my_info";

        // Department info
        if (MatchesAny(msg, ["department", "vibhag", "kitne log"]) ||
            Regex.IsMatch(msg, @"\b(engineering|marketing|sales|hr|finance|support|operations|it|admin)\b"))
            return "department_info";

        // Employee count / headcount
        if (MatchesAny(msg, ["headcount", "total employees", "kitne employees", "total log",
            "total people", "company size", "employee count"]))
            return "employee_count";

        // Hiring / recruitment cost
        if (MatchesAny(msg, ["hiring cost", "recruitment cost", "hiring kharcha",
            "recruitment kharcha", "hiring expense", "hiring budget", "recruitment expense"]))
            return "hiring_cost";

        // Performance
        if (MatchesAny(msg, ["my performance", "performance review", "my review",
            "mera performance", "meri performance", "review score", "appraisal",
            "my rating", "my score"]))
            return "performance";

        // Payroll / salary
        if (MatchesAny(msg, ["my salary", "meri salary", "payroll", "kitni salary",
            "salary details", "my pay", "compensation", "my ctc"]))
            return "payroll";

        // Policy
        if (MatchesAny(msg, ["policy", "rule", "rules", "company policy", "hr policy",
            "niyam", "guideline", "leave policy", "attendance policy"]))
            return "policy";

        // Team comparison
        if (MatchesAny(msg, ["compare my", "how am i doing", "team comparison", "vs team",
            "mera team se", "my vs team", "compare attendance"]))
            return "team_compare";

        // Best performer
        if (MatchesAny(msg, ["best performer", "top performer", "highest score", "sabse achha",
            "best performance", "who is the best", "top rating"]))
            return "best_performer";

        // Department positions / open jobs
        if (MatchesAny(msg, ["open positions in", "jobs in", "vacancies in", "hiring in",
            "positions in department", "recruitment in"]))
            return "dept_positions";

        // Leave trends
        if (MatchesAny(msg, ["leave trend", "leave pattern", "leave this month",
            "leave this quarter", "leave analysis", "leave statistics", "leave data"]))
            return "leave_trends";

        // Help
        if (MatchesAny(msg, ["help", "what can you do", "commands", "capabilities",
            "what you do", "guide", "tutorial"]))
            return "help";

        return "general";
    }

    private static bool MatchesAny(string msg, string[] keywords)
    {
        return keywords.Any(k => msg.Contains(k));
    }

    private async Task<CopilotResponse> HandleLeaveBalance(Employee employee)
    {
        var balances = await _context.LeaveBalances
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.EmployeeId == employee.Id && lb.Year == DateTime.UtcNow.Year)
            .ToListAsync();

        if (balances.Count == 0)
            return new CopilotResponse
            {
                Reply = "You don't have any leave balance records for this year.",
                Intent = "leave_balance"
            };

        var lines = balances.Select(b =>
            $"{b.LeaveType.Name}: {b.TotalDays - b.UsedDays:F1} days remaining (total {b.TotalDays}, used {b.UsedDays})");

        return new CopilotResponse
        {
            Reply = $"Here are your leave balances for {DateTime.UtcNow.Year}:\n\n• " + string.Join("\n• ", lines),
            Intent = "leave_balance",
            Actions = new List<CopilotAction>
            {
                new() { Label = "Apply for Leave", Action = "navigate:/leave" },
                new() { Label = "View Leave Policy", Action = "open:policy" }
            },
            Data = balances.Select(b => new
            {
                type = b.LeaveType.Name,
                total = b.TotalDays,
                used = b.UsedDays,
                remaining = b.TotalDays - b.UsedDays
            })
        };
    }

    private async Task<CopilotResponse> HandleLeaveApply(Employee employee, string msg)
    {
        var leaveTypes = await _context.LeaveTypes.ToListAsync();

        var lines = new List<string> { "I can help you apply for leave. Please go to the Leave page to submit a request." };
        var actions = new List<CopilotAction>
        {
            new() { Label = "Go to Leave Page", Action = "navigate:/leave" }
        };

        var linesForReply = string.Join("\n", lines);

        return new CopilotResponse
        {
            Reply = linesForReply,
            Intent = "leave_apply",
            Actions = actions
        };
    }

    private async Task<CopilotResponse> HandleMyAttendance(Employee employee)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var records = await _context.AttendanceRecords
            .Where(ar => ar.EmployeeId == employee.Id && ar.Date >= startOfMonth && ar.Date <= now)
            .OrderByDescending(ar => ar.Date)
            .ToListAsync();

        var present = records.Count(r => r.Status == "Present");
        var absent = records.Count(r => r.Status == "Absent");
        var late = records.Count(r => r.Status == "Late");
        var total = records.Count;

        return new CopilotResponse
        {
            Reply = $"Your attendance this month ({now:MMMM yyyy}):\n\n" +
                    $"• Present: {present} days\n• Absent: {absent} days\n• Late: {late} days\n• Total records: {total} days",
            Intent = "my_attendance",
            Actions = new List<CopilotAction>
            {
                new() { Label = "View Full Attendance", Action = "navigate:/attendance" }
            },
            Data = new { present, absent, late, total, month = now.ToString("MMMM yyyy") }
        };
    }

    private async Task<CopilotResponse> HandleTeamAttendance(Employee employee)
    {
        var team = await _context.Employees
            .Where(e => e.ManagerId == employee.Id && !e.IsDeleted)
            .ToListAsync();

        if (team.Count == 0)
            return new CopilotResponse
            {
                Reply = "You currently don't have any team members reporting to you.",
                Intent = "team_attendance"
            };

        var now = DateTime.UtcNow;
        var today = now.Date;

        var teamSummaries = new List<string>();
        foreach (var member in team)
        {
            var todayRecord = await _context.AttendanceRecords
                .FirstOrDefaultAsync(ar => ar.EmployeeId == member.Id && ar.Date == today);
            var status = todayRecord?.Status ?? "No record";
            var checkIn = todayRecord?.CheckInTime?.ToString("HH:mm") ?? "--";
            teamSummaries.Add($"{member.FirstName} {member.LastName}: {status} (in: {checkIn})");
        }

        return new CopilotResponse
        {
            Reply = $"Your team ({team.Count} members) — Today ({today:dd MMM yyyy}):\n\n• " +
                    string.Join("\n• ", teamSummaries),
            Intent = "team_attendance",
            Actions = new List<CopilotAction>
            {
                new() { Label = "View Team Members", Action = "navigate:/employees" }
            },
            Data = team.Select(t => new { name = $"{t.FirstName} {t.LastName}", id = t.Id })
        };
    }

    private async Task<CopilotResponse> HandleTeamMembers(Employee employee)
    {
        var team = await _context.Employees
            .Where(e => e.ManagerId == employee.Id && !e.IsDeleted)
            .Include(e => e.Department)
            .ToListAsync();

        if (team.Count == 0)
            return new CopilotResponse
            {
                Reply = "You don't have any direct reportees currently.",
                Intent = "team_members"
            };

        var lines = team.Select(m =>
            $"{m.FirstName} {m.LastName} — {m.Position ?? "N/A"} ({m.Department?.Name ?? "N/A"})");

        return new CopilotResponse
        {
            Reply = $"Your team has {team.Count} member(s):\n\n• " + string.Join("\n• ", lines),
            Intent = "team_members",
            Actions = new List<CopilotAction>
            {
                new() { Label = "View All Employees", Action = "navigate:/employees" }
            },
            Data = team.Select(t => new { name = $"{t.FirstName} {t.LastName}", position = t.Position, department = t.Department?.Name })
        };
    }

    private static CopilotResponse HandleMyInfo(Employee employee, User user)
    {
        return new CopilotResponse
        {
            Reply = $"Here's a quick summary about you:\n\n" +
                    $"• Name: {employee.FirstName} {employee.LastName}\n" +
                    $"• Email: {employee.Email}\n" +
                    $"• Position: {employee.Position ?? "N/A"}\n" +
                    $"• Department: {employee.Department?.Name ?? "N/A"}\n" +
                    $"• Status: {employee.Status}\n" +
                    $"• Roles: {string.Join(", ", user.UserRoles.Select(ur => ur.Role.Name))}",
            Intent = "my_info",
            Actions = new List<CopilotAction>
            {
                new() { Label = "View Full Profile", Action = $"navigate:/employees/{employee.Id}" }
            }
        };
    }

    private async Task<CopilotResponse> HandleDepartmentInfo(Employee employee, string msg, List<string> roles)
    {
        var deptNameMatch = Regex.Match(msg, @"\b(engineering|marketing|sales|hr|finance|support|operations|it|admin)\b", RegexOptions.IgnoreCase);
        string deptName;

        if (deptNameMatch.Success)
        {
            deptName = deptNameMatch.Value;
        }
        else if (employee.Department != null)
        {
            deptName = employee.Department.Name;
        }
        else
        {
            return new CopilotResponse
            {
                Reply = "Which department would you like to know about? Please specify a name.",
                Intent = "department_info"
            };
        }

        var dept = await _context.Departments
            .Include(d => d.Head)
            .Include(d => d.Employees.Where(e => !e.IsDeleted))
            .FirstOrDefaultAsync(d => d.Name.ToLower().Contains(deptName.ToLower()) && !d.IsDeleted);

        if (dept == null)
            return new CopilotResponse
            {
                Reply = $"I couldn't find a department named \"{deptName}\".",
                Intent = "department_info"
            };

        if (!CopilotPermissions.IsHrOrAdmin(roles) && !CopilotPermissions.IsManagerPlus(roles))
        {
            if (employee.DepartmentId != dept.Id)
            {
                return new CopilotResponse
                {
                    Reply = "I can only share details about your own department. Ask about your department or contact HR for org-wide info.",
                    Intent = "department_info",
                    Restricted = true
                };
            }
        }

        return new CopilotResponse
        {
            Reply = $"Department: {dept.Name}\n" +
                    $"• Head: {dept.Head?.FirstName} {dept.Head?.LastName ?? "Not assigned"}\n" +
                    $"• Total Employees: {dept.Employees.Count}\n" +
                    $"• Description: {dept.Description ?? "N/A"}",
            Intent = "department_info",
            Actions = new List<CopilotAction>
            {
                new() { Label = "View Departments", Action = "navigate:/departments" }
            },
            Data = new { name = dept.Name, head = dept.Head != null ? $"{dept.Head.FirstName} {dept.Head.LastName}" : null, count = dept.Employees.Count }
        };
    }

    private async Task<CopilotResponse> HandleEmployeeCount()
    {
        var total = await _context.Employees.CountAsync(e => !e.IsDeleted);
        var active = await _context.Employees.CountAsync(e => !e.IsDeleted && e.Status == "Active");
        var deptCount = await _context.Departments.CountAsync(d => !d.IsDeleted);

        return new CopilotResponse
        {
            Reply = $"Company Overview:\n\n" +
                    $"• Total Employees: {total}\n• Active Employees: {active}\n• Departments: {deptCount}",
            Intent = "employee_count",
            Actions = new List<CopilotAction>
            {
                new() { Label = "View All Employees", Action = "navigate:/employees" },
                new() { Label = "View Departments", Action = "navigate:/departments" }
            },
            Data = new { total, active, departments = deptCount }
        };
    }

    private async Task<CopilotResponse> HandleHiringCost()
    {
        var candidates = await _context.CandidateProfiles.CountAsync();
        var jobs = await _context.JobRequisitions.CountAsync(j => j.Status != "Closed");

        return new CopilotResponse
        {
            Reply = $"Recruitment Summary:\n\n" +
                    $"• Open Positions: {jobs}\n• Total Candidates in Pipeline: {candidates}\n\n" +
                    $"For detailed recruitment cost analysis, please visit the Reports page.",
            Intent = "hiring_cost",
            Actions = new List<CopilotAction>
            {
                new() { Label = "View Recruitment", Action = "navigate:/recruitment" },
                new() { Label = "View Reports", Action = "navigate:/reports" }
            },
            Data = new { openPositions = jobs, totalCandidates = candidates }
        };
    }

    private async Task<CopilotResponse> HandlePerformance(Employee employee, List<string> roles)
    {
        var reviews = await _context.PerformanceReviews
            .Include(pr => pr.Cycle)
            .Include(pr => pr.Reviewer)
            .Where(pr => pr.EmployeeId == employee.Id)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync();

        if (reviews.Count == 0)
            return new CopilotResponse
            {
                Reply = "You don't have any performance reviews yet.",
                Intent = "performance"
            };

        var lines = reviews.Select(r =>
            $"{r.Title} ({r.Cycle.Name}) — Score: {r.OverallScore?.ToString("F1") ?? "N/A"} — Status: {r.Status}");

        var reply = $"Your Performance Reviews:\n\n• " + string.Join("\n• ", lines);

        if (CopilotPermissions.IsManagerPlus(roles))
        {
            var team = await _context.Employees
                .Where(e => e.ManagerId == employee.Id && !e.IsDeleted)
                .Select(e => e.Id)
                .ToListAsync();

            if (team.Count > 0)
            {
                var teamReviews = await _context.PerformanceReviews
                    .Include(pr => pr.Employee)
                    .Where(pr => team.Contains(pr.EmployeeId) && pr.Status == "Completed" && pr.OverallScore != null)
                    .OrderByDescending(pr => pr.OverallScore)
                    .Take(5)
                    .ToListAsync();

                if (teamReviews.Count > 0)
                {
                    reply += "\n\nYour team's recent scores:\n• " +
                             string.Join("\n• ", teamReviews.Select(r =>
                                 $"{r.Employee.FirstName} {r.Employee.LastName}: {r.OverallScore:F1}"));
                }
            }
        }

        return new CopilotResponse
        {
            Reply = reply,
            Intent = "performance",
            Actions = CopilotPermissions.IsManagerPlus(roles)
                ? new List<CopilotAction>
                {
                    new() { Label = "View Performance Page", Action = "navigate:/performance" }
                }
                : new List<CopilotAction>()
        };
    }

    private async Task<CopilotResponse> HandlePayroll(Employee employee, List<string> roles)
    {
        if (roles.Contains("PayrollStaff") && !CopilotPermissions.IsHrOrAdmin(roles))
        {
            var activePayrolls = await _context.EmployeePayrolls.CountAsync(ep => ep.EffectiveTo == null);
            var structures = await _context.PayrollStructures.CountAsync();
            var totalComponents = await _context.SalaryComponents.CountAsync();

            return new CopilotResponse
            {
                Reply = $"Payroll Overview:\n\n" +
                        $"• Active employee payroll records: {activePayrolls}\n" +
                        $"• Payroll structures: {structures}\n" +
                        $"• Salary components configured: {totalComponents}\n\n" +
                        $"Visit the Payroll page for detailed processing.",
                Intent = "payroll",
                Actions = new List<CopilotAction>
                {
                    new() { Label = "View Payroll Page", Action = "navigate:/payroll" },
                    new() { Label = "View Reports", Action = "navigate:/reports" }
                },
                Data = new { activePayrolls, structures, totalComponents }
            };
        }

        var empPayroll = await _context.EmployeePayrolls
            .Include(ep => ep.PayrollStructure)
            .ThenInclude(ps => ps.Components)
            .FirstOrDefaultAsync(ep => ep.EmployeeId == employee.Id && ep.EffectiveTo == null);

        if (empPayroll == null)
            return new CopilotResponse
            {
                Reply = "No active payroll structure found for you.",
                Intent = "payroll"
            };

        var structure = empPayroll.PayrollStructure;
        var earnings = structure.Components.Where(c => c.Type == "Earning").Sum(c => c.Amount);
        var deductions = structure.Components.Where(c => c.Type == "Deduction").Sum(c => c.Amount);
        var net = earnings - deductions;

        var compLines = structure.Components.Select(c =>
            $"  {c.Name} ({c.Type}): {FormatInr(c.Amount)}");

        return new CopilotResponse
        {
            Reply = $"Your Payroll Structure: {structure.Name}\n\n" +
                    string.Join("\n", compLines) +
                    $"\n\nTotal Earnings: {FormatInr(earnings)}\nTotal Deductions: {FormatInr(deductions)}\nNet: {FormatInr(net)}",
            Intent = "payroll",
            Actions = CopilotPermissions.IsHrOrAdmin(roles) || roles.Contains("PayrollStaff")
                ? new List<CopilotAction> { new() { Label = "View Payroll Page", Action = "navigate:/payroll" } }
                : new List<CopilotAction>()
        };
    }

    private async Task<CopilotResponse> HandlePolicy(string msg, List<string> roles)
    {
        var policies = await _context.SystemSettings
            .Where(ss => ss.Key.StartsWith("policy."))
            .ToListAsync();

        if (policies.Count == 0)
        {
            var helpReply = "Policies haven't been configured yet. Please check with HR for policy details.";
            if (msg.Contains("leave"))
                helpReply = "Leave policy: Employees can apply for leave through the Leave page. Leaves are subject to manager approval. Please refer to HR for complete policy details.";
            else if (msg.Contains("attendance"))
                helpReply = "Attendance policy: Employees should mark their attendance daily. Late arrivals may be flagged. Please refer to HR for complete policy details.";

            return new CopilotResponse
            {
                Reply = helpReply,
                Intent = "policy"
            };
        }

        var matched = policies.Where(p =>
            p.Key.ToLower().Contains(msg) || p.Value.ToLower().Contains(msg)).ToList();

        if (matched.Count == 0)
            matched = policies;

        var lines = matched.Select(p => $"• {p.Key.Replace("policy.", "")}: {p.Value}");

        return new CopilotResponse
        {
            Reply = "Company Policies:\n\n" + string.Join("\n", lines),
            Intent = "policy",
            Actions = CopilotPermissions.IsHrOrAdmin(roles)
                ? new List<CopilotAction> { new() { Label = "Go to Settings", Action = "navigate:/admin" } }
                : new List<CopilotAction>()
        };
    }

    private static CopilotResponse HandleHelp(List<string> roles)
    {
        var persona = CopilotPermissions.GetPrimaryPersona(roles);
        var lines = new List<string>
        {
            "📋 **Leave** — \"My leave balance\", \"Apply for leave tomorrow\"",
            "📊 **Attendance** — \"My attendance\"",
            "ℹ️ **Info** — \"My info\", \"About me\"",
            "📈 **Performance** — \"My performance\", \"My reviews\"",
            "💰 **Payroll** — \"My salary\", \"Payroll details\"",
            "📋 **Policy** — \"Leave policy\", \"Attendance policy\"",
        };

        if (CopilotPermissions.IsManagerPlus(roles))
        {
            lines.Add("👥 **Team** — \"My team members\", \"Team attendance\"");
            lines.Add("🧠 **Compare** — \"Compare my attendance with team\", \"Top performer in my dept\"");
            lines.Add("📊 **Org** — \"Headcount\", \"Leave trends this quarter\"");
        }

        if (CopilotPermissions.IsHrOrAdmin(roles))
        {
            lines.Add("📋 **Recruitment** — \"Open positions in Engineering\", \"Hiring cost\"");
        }

        if (roles.Contains("PayrollStaff") && !CopilotPermissions.IsHrOrAdmin(roles))
        {
            lines.Add("💰 **Payroll** — \"Payroll summary\", \"Company headcount\"");
        }

        return new CopilotResponse
        {
            Reply = $"I'm your AI HR Copilot ({persona} mode)! Here's what I can help with:\n\n" +
                    string.Join("\n", lines),
            Intent = "help",
            Actions = new List<CopilotAction>
            {
                new() { Label = "View Dashboard", Action = "navigate:/" }
            }
        };
    }

    private async Task<CopilotResponse> HandleTeamCompare(Employee employee)
    {
        var team = await _context.Employees
            .Where(e => e.ManagerId == employee.Id && !e.IsDeleted)
            .ToListAsync();

        var ninetyDaysAgo = DateTime.UtcNow.AddDays(-90);
        var myAttendance = await _context.AttendanceRecords
            .Where(a => a.EmployeeId == employee.Id && a.Date >= ninetyDaysAgo)
            .ToListAsync();

        var myPresentPct = myAttendance.Count > 0
            ? (double)myAttendance.Count(a => a.Status == "Present") / myAttendance.Count * 100 : 0;

        if (team.Count == 0)
        {
            return new CopilotResponse
            {
                Reply = $"Your attendance rate is {myPresentPct:F0}% in the last 90 days. You don't have team members to compare with.",
                Intent = "team_compare"
            };
        }

        var teamPresentPcts = new List<double>();
        foreach (var member in team)
        {
            var memberAtt = await _context.AttendanceRecords
                .Where(a => a.EmployeeId == member.Id && a.Date >= ninetyDaysAgo)
                .ToListAsync();
            var pct = memberAtt.Count > 0
                ? (double)memberAtt.Count(a => a.Status == "Present") / memberAtt.Count * 100 : 0;
            teamPresentPcts.Add(pct);
        }

        var teamAvg = teamPresentPcts.Average();
        var comparison = myPresentPct >= teamAvg ? "ahead of" : "behind";

        return new CopilotResponse
        {
            Reply = $"📊 Attendance Comparison (last 90 days):\n\n" +
                    $"• You: {myPresentPct:F0}% present rate\n" +
                    $"• Team avg: {teamAvg:F0}%\n" +
                    $"• You're {comparison} your team average.\n\n" +
                    $"Your team has {team.Count} members.",
            Intent = "team_compare",
            Actions = new List<CopilotAction>
            {
                new() { Label = "View Attendance", Action = "navigate:/attendance" }
            }
        };
    }

    private async Task<CopilotResponse> HandleBestPerformer(Employee employee)
    {
        var deptId = employee.DepartmentId;
        if (deptId == null)
            return new CopilotResponse { Reply = "You're not assigned to a department.", Intent = "best_performer" };

        var completedReviews = await _context.PerformanceReviews
            .Include(r => r.Employee)
            .Where(r => r.Employee.DepartmentId == deptId && r.Status == "Completed" && r.OverallScore != null)
            .OrderByDescending(r => r.OverallScore)
            .ToListAsync();

        if (completedReviews.Count == 0)
            return new CopilotResponse
            {
                Reply = "No completed performance reviews found in your department.",
                Intent = "best_performer"
            };

        var best = completedReviews.First();
        var lines = completedReviews.Take(5).Select((r, i) =>
            $"{i + 1}. {r.Employee.FirstName} {r.Employee.LastName} — {r.OverallScore:F1} ({r.Title})");

        return new CopilotResponse
        {
            Reply = $"🏆 Top Performers in {employee.Department?.Name ?? "your department"}:\n\n" +
                    string.Join("\n", lines),
            Intent = "best_performer",
            Actions = new List<CopilotAction>
            {
                new() { Label = "View Performance Page", Action = "navigate:/performance" }
            }
        };
    }

    private async Task<CopilotResponse> HandleDeptPositions(Employee employee, string msg)
    {
        var deptName = "";
        foreach (var dept in await _context.Departments.Where(d => !d.IsDeleted).ToListAsync())
        {
            if (msg.Contains(dept.Name.ToLower()))
            {
                deptName = dept.Name;
                break;
            }
        }

        if (string.IsNullOrEmpty(deptName) && employee.Department != null)
            deptName = employee.Department.Name;

        if (string.IsNullOrEmpty(deptName))
            return new CopilotResponse
            {
                Reply = "Please specify which department you'd like to check open positions for.",
                Intent = "dept_positions"
            };

        var positions = await _context.JobRequisitions
            .Include(j => j.Department)
            .Where(j => j.Department != null && j.Department.Name == deptName && j.Status == "Open")
            .ToListAsync();

        if (positions.Count == 0)
            return new CopilotResponse
            {
                Reply = $"No open positions in {deptName} currently.",
                Intent = "dept_positions"
            };

        var lines = positions.Select(p =>
            $"{p.Title} — {p.Candidates.Count} candidate(s), open for {(DateTime.UtcNow - p.CreatedAt).Days} days");

        return new CopilotResponse
        {
            Reply = $"📋 Open Positions in {deptName}:\n\n• " + string.Join("\n• ", lines),
            Intent = "dept_positions",
            Actions = new List<CopilotAction>
            {
                new() { Label = "View Recruitment", Action = "navigate:/recruitment" }
            }
        };
    }

    private async Task<CopilotResponse> HandleLeaveTrends(Employee employee, List<string> roles)
    {
        var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);
        var query = _context.LeaveRequests
            .Include(l => l.LeaveType)
            .Include(l => l.Employee)
            .Where(l => l.StartDate >= threeMonthsAgo && l.Status == "Approved");

        if (!CopilotPermissions.IsHrOrAdmin(roles))
        {
            if (employee.DepartmentId.HasValue)
                query = query.Where(l => l.Employee!.DepartmentId == employee.DepartmentId);
            else
                query = query.Where(l => l.EmployeeId == employee.Id);
        }

        var leaves = await query
            .GroupBy(l => new { l.StartDate.Year, l.StartDate.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new
            {
                Month = $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM yyyy}",
                Count = g.Count(),
                TotalDays = g.Sum(l => (l.EndDate - l.StartDate).Days + 1)
            })
            .ToListAsync();

        if (leaves.Count == 0)
            return new CopilotResponse
            {
                Reply = "No leave trends available for the last 3 months.",
                Intent = "leave_trends"
            };

        var totalDays = leaves.Sum(l => l.TotalDays);
        var lines = leaves.Select(l => $"{l.Month}: {l.Count} requests ({l.TotalDays} days)");

        return new CopilotResponse
        {
            Reply = $"📈 Leave Trends (Last 3 Months):\n\n" +
                    string.Join("\n", lines) +
                    $"\n\nTotal: {leaves.Sum(l => l.Count)} requests, {totalDays} days approved",
            Intent = "leave_trends",
            Actions = new List<CopilotAction>
            {
                new() { Label = "View Leave Page", Action = "navigate:/leave" }
            }
        };
    }

    private async Task<CopilotResponse> HandleGeneral(string msg, Employee employee, User user, List<string> roles)
    {
        var hasAiKey = !string.IsNullOrEmpty(await GetAiApiKey());
        if (hasAiKey)
        {
            try
            {
                var aiResponse = await _aiService.GetResponseAsync(msg, employee, user, roles);
                return new CopilotResponse
                {
                    Reply = aiResponse,
                    Intent = "general",
                    Actions = new List<CopilotAction>
                    {
                        new() { Label = "View Dashboard", Action = "navigate:/" },
                        new() { Label = "Show Help", Action = "help" }
                    }
                };
            }
            catch
            {
                // Fallback to rule-based
            }
        }

        var persona = CopilotPermissions.GetPrimaryPersona(roles);
        var hints = persona switch
        {
            "employee" => "• Your leave balance or attendance\n• Your performance or salary\n• Company policies",
            "manager" => "• Your team attendance or members\n• Leave balance or headcount\n• Team performance comparison",
            "hr" => "• Headcount or leave trends\n• Recruitment or hiring cost\n• Department or policy info",
            "payroll" => "• Payroll summary or headcount\n• Policies or your profile",
            _ => "• Leave, attendance, or team data\n• Headcount, recruitment, or payroll\n• Policies and reports"
        };

        return new CopilotResponse
        {
            Reply = "I'm not sure how to answer that. Try asking me about:\n" + hints + "\n\nType \"help\" to see all things I can do!",
            Intent = "general",
            Actions = new List<CopilotAction>
            {
                new() { Label = "Show Help", Action = "help" }
            }
        };
    }

    private async Task<string?> GetAiApiKey()
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(ss => ss.Key == "AI:ApiKey");
        return setting?.Value;
    }

    private static string FormatInr(decimal amount) => $"₹{amount:N0}";
}
