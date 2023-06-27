using CommunityToolkit.Maui.Behaviors;
using MSiccDev.ServerlessBlog.AdminClient.ViewModel;
namespace MSiccDev.ServerlessBlog.AdminClient.View
{
	public class BasePage : ContentPage
	{
		public BasePage()
		{
		
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			if (this.BindingContext is BasePageViewModel baseViewModel)
			{
				this.Behaviors.Add(new EventToCommandBehavior
				{
					EventName = nameof(BasePage.Appearing),
					Command = baseViewModel.ViewAppearingCommand
				});

				this.Behaviors.Add(new EventToCommandBehavior
				{
					EventName = nameof(BasePage.Disappearing),
					Command = baseViewModel.ViewDisappearingCommand
				});

				SetBinding(TitleProperty, new Binding(nameof(BasePageViewModel.Title)));
				return;
			}

			throw new NotSupportedException($"The ViewModel bound to this view must derive from {nameof(BasePageViewModel)}");
		}

	}
}

