namespace MSiccDev.ServerlessBlog.AdminClient.Services
{
    public class ActionSheetService : IActionSheetService
    {
        //lazily copied over from my private SliMvvm toolkit

        public async Task<string?> ShowActionSheetAsync(string title, string[] buttonTexts) =>
            await ShowActionSheetAsync(title, null, null, buttonTexts);

        public async Task<string?> ShowActionSheetAsync(string title, string? cancelButtonText, string[] buttonTexts) =>
            await ShowActionSheetAsync(title, cancelButtonText, null, buttonTexts);

        public async Task<string?> ShowActionSheetAsync(string title, string? cancelButtonText, string? destructiveButtonText, string[] buttonTexts)
        {
            if (Application.Current?.MainPage != null)
                return await Application.Current.MainPage.DisplayActionSheet(title, cancelButtonText, destructiveButtonText, buttonTexts);

            return null;
        }
    }
}
