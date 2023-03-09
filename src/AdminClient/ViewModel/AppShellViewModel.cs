using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using MSiccDev.ServerlessBlog.AdminClient.Services;
using MSiccDev.ServerlessBlog.AdminClient.View;
namespace MSiccDev.ServerlessBlog.AdminClient.ViewModel
{
    public class AppShellViewModel : ObservableObject
    {
        private readonly ILogger<AppShellViewModel> _logger;
        private readonly ICacheService _cacheService;
        private readonly INavigationService _navigationService;

        public AppShellViewModel(ILogger<AppShellViewModel> logger, ICacheService cacheService, INavigationService navigationService)
        {
            _logger = logger;

            _cacheService = cacheService;
            _navigationService = navigationService;

            _cacheService.AuthorizationExpired += (sender, args) =>
                RefreshAuthorizationAsync().SafeFireAndForget();

        }

        private async Task RefreshAuthorizationAsync() =>
            await _navigationService.NavigateToRouteAsync($"{nameof(LoginPage)}?returnRoute={Shell.Current.CurrentPage.GetType().Name}");


        public string VersionString => $"Version {AppInfo.Current.VersionString}";

        public string CopyRightString => $"© {DateTimeOffset.Now.Date.Year:0000} MSiccDev Software Development";
        public string ApplicationTitle => "#CASBAN6";
    }
}
