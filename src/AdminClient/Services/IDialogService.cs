namespace MSiccDev.ServerlessBlog.AdminClient.Services
{
    public interface IDialogService
    {
        Task ShowMessageAsync(string title, string message, string buttonText);
        Task<bool> ShowMessageAsync(string title, string message, string positiveButtonText, string negativeButtonText);
    }
}
