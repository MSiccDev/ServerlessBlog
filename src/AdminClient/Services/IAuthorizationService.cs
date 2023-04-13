namespace MSiccDev.ServerlessBlog.AdminClient.Services
{
    public interface IAuthorizationService
    {
        Task RefreshAuthorizationAsync();
    }
}
