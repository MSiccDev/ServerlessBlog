using CommunityToolkit.Mvvm.DependencyInjection;
using MSiccDev.ServerlessBlog.AdminClient.View;

namespace MSiccDev.ServerlessBlog.AdminClient
{
    public partial class App
    {
        public App(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            InitializeComponent();


            this.MainPage = new AppShell();
		}
    }
}
