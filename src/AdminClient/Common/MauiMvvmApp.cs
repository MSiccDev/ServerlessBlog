using CommunityToolkit.Mvvm.DependencyInjection;
namespace MSiccDev.ServerlessBlog.AdminClient.Common
{
	public class MauiMvvmApp : Application
	{
		public MauiMvvmApp(IServiceProvider services)
		{
			Ioc.Default.ConfigureServices(services);
		}
	}
}
