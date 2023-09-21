using Hangfire.Dashboard;

namespace Tgs.GameRecord.SBO;

public class IgnoreHangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}
