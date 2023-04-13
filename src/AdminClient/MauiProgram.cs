using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using MSiccDev.Libs.Maui.SystemIcons;
using MSiccDev.ServerlessBlog.AdminClient.Common;
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
            RegisterViewsAndViewModels(builder.Services);

            return builder.Build();
        }

        private static void RegisterViewsAndViewModels(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<AppShellViewModel>();

            serviceCollection.AddSingletonWithShellRoute<LoginPage, LoginPagePageViewModel>(Constants.LoginPageRoute);
            serviceCollection.AddSingletonWithShellRoute<BlogPage, BlogPagePageViewModel>(Constants.BlogPageRoute);
            serviceCollection.AddSingletonWithShellRoute<AuthorPage, AuthorPageViewModel>(Constants.AuthorPageRoute);
            serviceCollection.AddSingletonWithShellRoute<SettingsPage, SettingsPagePageViewModel>(Constants.SettingsPageRoute);
        }

        private static void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHttpClient();
            serviceCollection.AddSingleton<INavigationService, NavigationService>();
            serviceCollection.AddSingleton<IAuthorizationService, AuthorizationService>();
            serviceCollection.AddSingleton<IActionSheetService, ActionSheetService>();
            serviceCollection.AddSingleton<IDialogService, DialogService>();
            serviceCollection.AddSingleton<IBlogClient, BlogClient>();
            serviceCollection.AddSingleton<ICacheService, CacheService>();
        }
    }
}
