using MSiccDev.ServerlessBlog.AdminClient.Common;
using MSiccDev.ServerlessBlog.AdminClient.ViewModel;
namespace MSiccDev.ServerlessBlog.AdminClient.View
{
	public partial class LoginPage
	{
		public LoginPage(LoginPagePageViewModel loginPagePageViewModel)
		{
			InitializeComponent();

			this.BindingContext = loginPagePageViewModel;


			Shell.SetPresentationMode(this,
			Preferences.Default.Get(Constants.HasObtainedValidAccessTokenStorageName, false)
				? PresentationMode.Animated
				: PresentationMode.ModalAnimated);
		}
	}
}
