namespace MSiccDev.ServerlessBlog.AdminClient.Services
{
    public class DialogService : IDialogService
    {
        public async Task ShowMessageAsync(string title, string message, string buttonText) =>
            await Application.Current.MainPage.DisplayAlert(title, message, buttonText);

        public async Task<bool> ShowMessageAsync(string title, string message, string positiveButtonText, string negativeButtonText) =>
            await Application.Current.MainPage.DisplayAlert(title, message, positiveButtonText, negativeButtonText);
    }
}
