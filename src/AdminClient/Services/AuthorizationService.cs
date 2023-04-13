using MSiccDev.ServerlessBlog.AdminClient.View;
namespace MSiccDev.ServerlessBlog.AdminClient.Services
{


    public class AuthorizationService : IAuthorizationService
    {
        private readonly INavigationService _navigationService;

        public AuthorizationService(INavigationService navigationService) =>
            _navigationService = navigationService;

        public async Task RefreshAuthorizationAsync()
        {
            //avoiding double navigation in Shell
            if (Shell.Current.CurrentPage is not LoginPage _)
                await _navigationService.NavigateToRouteAsync($"{nameof(LoginPage)}?returnRoute={Shell.Current.CurrentPage.GetType().Name}");
        }
    }
}
