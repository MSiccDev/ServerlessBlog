using MSiccDev.ServerlessBlog.AdminClient.Common;
using MSiccDev.ServerlessBlog.AdminClient.Services;
using MSiccDev.ServerlessBlog.AdminClient.ViewModel;
using MSiccDev.ServerlessBlog.ClientSdk;
namespace MSiccDev.ServerlessBlog.AdminClient
{
    public partial class App
    {
        public App(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            InitializeComponent();
            
            //Preferences.Default.Set(Constants.HasObtainedValidAccessTokenStorageName, false);
            
            //more inits
            serviceProvider.GetRequiredService<IBlogClient>().Init(SecureStorage.Default.GetAsync(Constants.AzureFunctionBaseUrlStorageName).GetAwaiter().GetResult());
            serviceProvider.GetRequiredService<ICacheService>().Init($"app_cache_{AppInfo.Current.PackageName}");

            this.MainPage = new AppShell(serviceProvider.GetRequiredService<AppShellViewModel>());
		}

    }
}
