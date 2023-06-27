using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using MSiccDev.ServerlessBlog.AdminClient.Services;
using MSiccDev.ServerlessBlog.ClientSdk;
namespace MSiccDev.ServerlessBlog.AdminClient.ViewModel
{
    public class AppShellViewModel : ObservableObject
    {
        private readonly ILogger<AppShellViewModel> _logger;
        private readonly ICacheService _cacheService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IDialogService _dialogService;

        public AppShellViewModel(ILogger<AppShellViewModel> logger, ICacheService cacheService, IAuthorizationService authorizationService, IDialogService dialogService)
        {
            _logger = logger;

            _cacheService = cacheService;
            _authorizationService = authorizationService;
            _dialogService = dialogService;

            _cacheService.AuthorizationExpired += (sender, args) =>
            {
                _authorizationService.RefreshAuthorizationAsync().SafeFireAndForget();
            };

            _cacheService.ApiErrorOccured += (sender, args) =>
            {
                HandleApiErrorsAsync(args).SafeFireAndForget();
            };
        }

        private async Task HandleApiErrorsAsync(RequestError? args)
        {
#if MACCATALYST || IOS
            if (string.IsNullOrWhiteSpace(args?.Message))
                return;

            if (args.Message.Contains("Code=-1004"))
            {
                await _dialogService.ShowMessageAsync("Connection Error", "Service is currently not available. Please try again later.", "OK");
            }
            else
            {
                await _dialogService.ShowMessageAsync("Error", args.Message, "OK");
            }
#endif
        }




        public string VersionString => $"Version {AppInfo.Current.VersionString}";

        public string CopyRightString => $"© {DateTimeOffset.Now.Date.Year:0000} MSiccDev Software Development";
        public string ApplicationTitle => "#CASBAN6";
    }
}
