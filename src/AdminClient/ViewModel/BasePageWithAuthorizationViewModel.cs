using MSiccDev.ServerlessBlog.AdminClient.Services;
namespace MSiccDev.ServerlessBlog.AdminClient.ViewModel
{
    public class BasePageWithAuthorizationViewModel : BasePageViewModel
    {
        internal readonly IAuthorizationService _authorizationService;

        public BasePageWithAuthorizationViewModel(IAuthorizationService authorizationService) =>
            _authorizationService = authorizationService;
    }
}
