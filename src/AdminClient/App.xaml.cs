using MSiccDev.ServerlessBlog.AdminClient.Services;
namespace MSiccDev.ServerlessBlog.AdminClient
{
    public partial class App
    {
        public App(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            InitializeComponent();

            //more inits
            serviceProvider.GetRequiredService<ICacheService>().Init($"app_cache_{AppInfo.Current.PackageName}");
            
            this.MainPage = new AppShell();
		}


    }
}
