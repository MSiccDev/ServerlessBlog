using CommunityToolkit.Maui.Behaviors;
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

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			if (this.BindingContext is LoginPageViewModel loginPageViewModel)
			{
				this.Behaviors.Add(new EventToCommandBehavior
				{
					EventName = nameof(this.Appearing),
					Command = loginPageViewModel.ViewAppearingCommand
				});
			}
		}
	}
}
