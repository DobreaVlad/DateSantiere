using Hangfire.Dashboard;

namespace DateSantiere.Web.Services;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        // Allow access only to authenticated users with SuperAdmin role
        var isAuthenticated = httpContext.User?.Identity?.IsAuthenticated ?? false;
        var isSuperAdmin = httpContext.User?.IsInRole("Admin") ?? false;

        return isAuthenticated && isSuperAdmin;
    }
}
