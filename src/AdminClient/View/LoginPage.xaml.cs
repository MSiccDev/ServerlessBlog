using MSiccDev.ServerlessBlog.AdminClient.ViewModel;
namespace MSiccDev.ServerlessBlog.AdminClient.View
{
	public partial class LoginPage
	{
		public LoginPage(LoginPageViewModel loginPageViewModel)
		{
			InitializeComponent();

			this.BindingContext = loginPageViewModel;
		}
	}
}
