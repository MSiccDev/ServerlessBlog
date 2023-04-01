using MSiccDev.ServerlessBlog.AdminClient.ViewModel;
namespace MSiccDev.ServerlessBlog.AdminClient.View;

public partial class AuthorPage
{
	public AuthorPage(AuthorPageViewModel authorPageVm)
	{
		InitializeComponent();

		this.BindingContext = authorPageVm;
	}
}
