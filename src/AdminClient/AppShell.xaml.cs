using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.DependencyInjection;
using MSiccDev.ServerlessBlog.AdminClient.Services;

namespace MSiccDev.ServerlessBlog.AdminClient
{
	public partial class AppShell : Shell
	{
		public AppShell()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			if (Preferences.Default.Get<bool>(Common.Constants.HasObtainedValidAccessTokenStorageName, false))
			{
				Ioc.Default.GetRequiredService<INavigationService>().
					NavigateToRouteAsync(nameof(MainPage), false, Common.ShellNavigationSearchDirection.Down).SafeFireAndForget();
			}
		}
	}
}
