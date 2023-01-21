using System;
using MSiccDev.ServerlessBlog.AdminClient.Common;

namespace MSiccDev.ServerlessBlog.AdminClient.Services
{
	public interface INavigationService
	{
		Task NavigateToRouteAsync(string route, bool keepNavigationStack = false, ShellNavigationSearchDirection searchDirection = ShellNavigationSearchDirection.Down);
	}
}

