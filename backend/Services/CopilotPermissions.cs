namespace HRMS.API.Services;

public static class CopilotPermissions
{
    private static readonly Dictionary<string, string[]> IntentRoles = new()
    {
        ["leave_balance"] = ["Administrator", "HRManager", "Manager", "Employee"],
        ["leave_apply"] = ["Administrator", "HRManager", "Manager", "Employee"],
        ["my_attendance"] = ["Administrator", "HRManager", "Manager", "Employee"],
        ["team_attendance"] = ["Administrator", "HRManager", "Manager"],
        ["team_members"] = ["Administrator", "HRManager", "Manager"],
        ["my_info"] = ["Administrator", "HRManager", "Manager", "Employee", "PayrollStaff"],
        ["department_info"] = ["Administrator", "HRManager", "Manager", "Employee"],
        ["employee_count"] = ["Administrator", "HRManager", "Manager", "PayrollStaff"],
        ["hiring_cost"] = ["Administrator", "HRManager"],
        ["performance"] = ["Administrator", "HRManager", "Manager", "Employee"],
        ["payroll"] = ["Administrator", "HRManager", "Manager", "Employee", "PayrollStaff"],
        ["policy"] = ["Administrator", "HRManager", "Manager", "Employee", "PayrollStaff"],
        ["team_compare"] = ["Administrator", "HRManager", "Manager"],
        ["best_performer"] = ["Administrator", "HRManager", "Manager"],
        ["dept_positions"] = ["Administrator", "HRManager"],
        ["leave_trends"] = ["Administrator", "HRManager", "Manager"],
        ["help"] = ["Administrator", "HRManager", "Manager", "Employee", "PayrollStaff"],
        ["general"] = ["Administrator", "HRManager", "Manager", "Employee", "PayrollStaff"],
    };

    private static readonly Dictionary<string, string[]> PathRoles = new()
    {
        ["/"] = ["Administrator", "HRManager", "Manager", "Employee", "PayrollStaff"],
        ["/leave"] = ["Administrator", "HRManager", "Manager", "Employee"],
        ["/attendance"] = ["Administrator", "HRManager", "Manager", "Employee"],
        ["/employees"] = ["Administrator", "HRManager", "Manager"],
        ["/departments"] = ["Administrator", "HRManager", "Manager"],
        ["/performance"] = ["Administrator", "HRManager", "Manager"],
        ["/payroll"] = ["Administrator", "PayrollStaff"],
        ["/recruitment"] = ["Administrator", "HRManager"],
        ["/reports"] = ["Administrator", "HRManager", "PayrollStaff"],
        ["/analytics"] = ["Administrator", "HRManager", "Manager"],
        ["/admin"] = ["Administrator"],
    };

    public static bool CanAccessIntent(IEnumerable<string> userRoles, string intent)
    {
        if (!IntentRoles.TryGetValue(intent, out var allowed)) return false;
        return userRoles.Any(r => allowed.Contains(r));
    }

    public static bool CanAccessPath(IEnumerable<string> userRoles, string path)
    {
        if (!PathRoles.TryGetValue(path, out var allowed)) return true;
        return userRoles.Any(r => allowed.Contains(r));
    }

    public static string GetPrimaryPersona(IEnumerable<string> userRoles)
    {
        var roles = userRoles.ToList();
        if (roles.Contains("Administrator")) return "admin";
        if (roles.Contains("HRManager")) return "hr";
        if (roles.Contains("PayrollStaff")) return "payroll";
        if (roles.Contains("Manager")) return "manager";
        return "employee";
    }

    public static bool IsHrOrAdmin(IEnumerable<string> userRoles) =>
        userRoles.Any(r => r is "Administrator" or "HRManager");

    public static bool IsManagerPlus(IEnumerable<string> userRoles) =>
        userRoles.Any(r => r is "Administrator" or "HRManager" or "Manager");

    public static string GetDeniedMessage(string intent, string persona)
    {
        return persona switch
        {
            "employee" =>
                "As an employee, I can only help with your personal HR data — leave, attendance, performance, and profile. " +
                "Team or company-wide insights are available to your manager or HR.",
            "payroll" =>
                "As payroll staff, I can help with payroll summaries, headcount, and policies — not recruitment or team management data.",
            "manager" =>
                "That request needs HR or admin access. I can help with your team, leave, attendance, and department info instead.",
            _ =>
                "You don't have access to that data with your current role."
        };
    }
}
