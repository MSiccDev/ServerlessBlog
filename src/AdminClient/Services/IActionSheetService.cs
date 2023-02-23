namespace MSiccDev.ServerlessBlog.AdminClient.Services
{
    public interface IActionSheetService
    {
        Task<string?> ShowActionSheetAsync(string title, params string[] buttons);

        Task<string?> ShowActionSheetAsync(string title, string? cancel, params string[] buttons);

        Task<string?> ShowActionSheetAsync(string title, string? cancel, string? destruction, params string[] buttons);
    }
}
