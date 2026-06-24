using System.Security.Claims;
using HRMS.API.Models.Entities;

namespace HRMS.API.Authorization;

public enum HrAccessResult
{
    Ok,
    Unauthorized,
    Forbidden
}

public static class HrAccess
{
    private static readonly HashSet<string> ViewAllRoles = new(StringComparer.Ordinal)
    {
        "Administrator",
        "HRManager",
        "Manager"
    };

    public static bool CanViewAllHrData(ClaimsPrincipal user) =>
        user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Any(c => ViewAllRoles.Contains(c.Value));

    public static (int? EmployeeId, HrAccessResult Result) ResolveEmployeeScope(
        ClaimsPrincipal user,
        User? dbUser,
        int? requestedEmployeeId)
    {
        if (CanViewAllHrData(user))
            return (requestedEmployeeId, HrAccessResult.Ok);

        if (dbUser?.EmployeeId == null)
            return (null, HrAccessResult.Unauthorized);

        if (requestedEmployeeId.HasValue && requestedEmployeeId.Value != dbUser.EmployeeId.Value)
            return (null, HrAccessResult.Forbidden);

        return (dbUser.EmployeeId.Value, HrAccessResult.Ok);
    }
}
