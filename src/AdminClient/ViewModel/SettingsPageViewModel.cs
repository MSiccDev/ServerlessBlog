using Microsoft.Extensions.Logging;
using MSiccDev.ServerlessBlog.AdminClient.Common;
using MSiccDev.ServerlessBlog.AdminClient.Services;
namespace MSiccDev.ServerlessBlog.AdminClient.ViewModel
{
    public class SettingsPagePageViewModel : BasePageViewModel
    {
        private readonly ILogger<SettingsPagePageViewModel> _logger;
        private readonly ICacheService _cacheService;

        public SettingsPagePageViewModel(ILogger<SettingsPagePageViewModel> logger, ICacheService cacheService)
        {
            _logger = logger;
            _cacheService = cacheService;

            Connectivity.ConnectivityChanged += (sender, args) =>
            {
                NotifyCommandCanExecuteChanges();
            };

            this.Title = "Settings";
        }

        private void NotifyCommandCanExecuteChanges()
        {
            throw new NotImplementedException();
        }

        public bool DebugLocally
        {
            get => Preferences.Get(Constants.DebugLocallyStorageName, false);
            set
            {
                Preferences.Set(Constants.DebugLocallyStorageName, value);
                OnPropertyChanged();
            }
        }
    }
}
