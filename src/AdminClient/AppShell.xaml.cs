using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.DependencyInjection;
using MSiccDev.ServerlessBlog.AdminClient.Common;
using MSiccDev.ServerlessBlog.AdminClient.Services;
using MSiccDev.ServerlessBlog.AdminClient.View;
using MSiccDev.ServerlessBlog.AdminClient.ViewModel;
namespace MSiccDev.ServerlessBlog.AdminClient
{
	public partial class AppShell : Shell
	{
		public AppShell(AppShellViewModel appShellViewModel)
		{
			InitializeComponent();

			this.BindingContext = appShellViewModel;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			if (Preferences.Default.Get<bool>(Constants.HasObtainedValidAccessTokenStorageName, false))
			{
				Ioc.Default.GetRequiredService<INavigationService>().
					NavigateToRouteAsync(nameof(BlogPage), false, ShellNavigationSearchDirection.Down).SafeFireAndForget();
			}
		}
	}
}
