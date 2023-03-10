using Microsoft.Extensions.Logging;
using MSiccDev.ServerlessBlog.AdminClient.Common;
namespace MSiccDev.ServerlessBlog.AdminClient.Services
{
	public class NavigationService : INavigationService
	{
		private readonly ILogger<INavigationService> _logger;

		public NavigationService(ILogger<NavigationService> logger) =>
			_logger = logger;

		public async Task NavigateToRouteAsync(string route, bool keepNavigationStack = false, ShellNavigationSearchDirection searchDirection = ShellNavigationSearchDirection.Down)
		{
			string navigationPrefix = keepNavigationStack ? string.Empty : "//";

			if (searchDirection == ShellNavigationSearchDirection.Down)
				navigationPrefix += "/";

			route = $"{navigationPrefix}{route}";

			_logger.LogDebug("Navigation requested for route {Route}", route);

			await Shell.Current.GoToAsync(route);
		}

		public async Task GoBackAsync(string? route = null)
		{
			string navigationRoute = "..";

			if (!string.IsNullOrWhiteSpace(route))
				navigationRoute += $"/{route}";

			await Shell.Current.GoToAsync(navigationRoute);
		}
		
		
	}
}

