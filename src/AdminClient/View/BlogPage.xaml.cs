using CommunityToolkit.Maui.Behaviors;
using MSiccDev.ServerlessBlog.AdminClient.ViewModel;
namespace MSiccDev.ServerlessBlog.AdminClient.View;

public partial class BlogPage : ContentPage
{
	public BlogPage(BlogPageViewModel viewModel)
	{
		InitializeComponent();

		this.BindingContext = viewModel;
	}

	protected override void OnBindingContextChanged()
	{
		base.OnBindingContextChanged();

		if (this.BindingContext is BlogPageViewModel blogPageViewModel)
		{
			this.Behaviors.Add(new EventToCommandBehavior
			{
				EventName = nameof(this.Appearing),
				Command = blogPageViewModel.ViewAppearingCommand
			});
		}
	}
}
