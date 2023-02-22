namespace MSiccDev.ServerlessBlog.AdminClient.Services
{
    public interface ICacheService
    {
        void Init(string id, string? path = null);
    }
}
