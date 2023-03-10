using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using MSiccDev.Libs.Maui.SystemIcons;
using MSiccDev.ServerlessBlog.AdminClient.Services;
using MSiccDev.ServerlessBlog.AdminClient.View;
using MSiccDev.ServerlessBlog.AdminClient.ViewModel;
using MSiccDev.ServerlessBlog.ClientSdk;
namespace MSiccDev.ServerlessBlog.AdminClient
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>().UseMauiCommunityToolkit().
                    ConfigureFonts(fonts =>
                    {
                        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    }).ConfigureImageSources(services =>
                    {
                        services.AddService<ISfSymbolsImageSource, SfSymbolsImageSourceService>();
                    });
#if DEBUG
            builder.Logging.AddDebug();
#endif
            
            RegisterServices(builder.Services);
            RegisterViewModels(builder.Services);
            RegisterViews(builder.Services);

            return builder.Build();
        }

        private static void RegisterViews(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<LoginPage>();
            serviceCollection.AddSingleton<BlogPage>();
        }

        private static void RegisterViewModels(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<AppShellViewModel>();
            serviceCollection.AddSingleton<LoginPageViewModel>();
            serviceCollection.AddSingleton<BlogPageViewModel>();
        }

        private static void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHttpClient();
            serviceCollection.AddSingleton<INavigationService, NavigationService>();
            serviceCollection.AddSingleton<IActionSheetService, ActionSheetService>();
            serviceCollection.AddSingleton<IBlogClient, BlogClient>();
            serviceCollection.AddSingleton<ICacheService, CacheService>();
        }
    }
}
