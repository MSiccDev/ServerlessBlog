using Foundation;
using MSiccDev.ServerlessBlog.AdminClient;

// ReSharper disable CheckNamespace
namespace AdminClient
{
	// ReSharper restore CheckNamespace

	[Register("AppDelegate")]
	public class AppDelegate : MauiUIApplicationDelegate
	{
		protected override MauiApp CreateMauiApp()
		{
			return MauiProgram.CreateMauiApp();
		}
	}
}
