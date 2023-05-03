using System.Collections.ObjectModel;
using AsyncAwaitBestPractices;
using Microsoft.Extensions.Logging;
using MSiccDev.ServerlessBlog.AdminClient.Services;
using MSiccDev.ServerlessBlog.DtoModel;
namespace MSiccDev.ServerlessBlog.AdminClient.ViewModel
{
    public class AuthorPageViewModel : BasePageWithAuthorizationViewModel
    {
        private readonly ILogger<AuthorPageViewModel> _logger;
        private readonly ICacheService _cacheService;
        private readonly IDialogService _dialogService;
        private readonly IActionSheetService _actionSheetService;


        public AuthorPageViewModel(IAuthorizationService authorizationService,
            ILogger<AuthorPageViewModel> logger,
            ICacheService cacheService,
            IDialogService dialogService,
            IActionSheetService actionSheetService) :
            base(authorizationService)
        {
            _logger = logger;
            _cacheService = cacheService;
            _dialogService = dialogService;
            _actionSheetService = actionSheetService;

            _cacheService.CacheHasChanged += OnCacheHasChanged;
            
            Connectivity.ConnectivityChanged += (sender, args) =>
            {
                NotifyCommandCanExecuteChanges();
            };

            this.Title = "Authors";
        }

        private void OnCacheHasChanged(object? sender, EventArgs e) =>
            LoadAuthorsAsync().SafeFireAndForget();
        

        private void NotifyCommandCanExecuteChanges()
        {
            throw new NotImplementedException();
        }

        protected override async Task ExecuteViewAppearingAsync()
        {
            await base.ExecuteViewAppearingAsync();

            await LoadAuthorsAsync();
        }

        private async Task LoadAuthorsAsync()
        {
            List<Author>? cachedAuthors = await _cacheService.GetAuthorsAsync(this.CurrentBlogId);

            if (!cachedAuthors?.Any() ?? true)
                return;

            this.Authors.Clear();
            
            foreach (Author author in cachedAuthors!)
            {
                var authorVm = new AuthorViewModel(author);
                authorVm.AuthorImageChangeRequested += OnAuthorImageChangeRequested;
                authorVm.AuthorUpdateRequested += OnAuthorUpdateRequested;
                authorVm.AuthorDeleteRequested += OnAuthorDeleteRequested;
                this.Authors.Add(authorVm);
            }
        }



        private void OnAuthorImageChangeRequested(object? sender, AuthorViewModel vm) =>
            ChangeAuthorImageAndVerifyAsync(vm).SafeFireAndForget();


        private void OnAuthorUpdateRequested(object? sender, AuthorViewModel vm) =>
            VerifyUpdateAsync(vm).SafeFireAndForget();

        private void OnAuthorDeleteRequested(object? sender, AuthorViewModel vm) =>
            VerifyDeleteAsync(vm).SafeFireAndForget();

        private async Task ChangeAuthorImageAndVerifyAsync(AuthorViewModel vm)
        {
            try
            {
                FileResult? imageToUpload = null;

                string? selectedOption = await _actionSheetService.
                    ShowActionSheetAsync("Select a photo:", "Cancel", vm.AuthorImageUrl == null ? null : "Delete", "Choose from photo library", "Choose from file system");

                switch (selectedOption)
                {
                    case "Delete":
                        //TODO: Delete File from blob storage
                        vm.AuthorImageUrl = null;
                        break;
                    case "Choose from photo library":
                        imageToUpload = await MediaPicker.Default.PickPhotoAsync();
                        break;
                    case "Choose from file system":
                        //general problem on the API, need to investigate
                        //https://github.com/dotnet/maui/issues/9394
                        var filePickerOptions = new PickOptions
                        {
                            PickerTitle = "Please select an image file",
                            FileTypes = FilePickerFileType.Images
                        };
                        imageToUpload = await FilePicker.PickAsync(filePickerOptions);
                        break;
                }

                if (imageToUpload == null)
                    return;

                //upload medium
                //create new Medium
                // await _cacheService.UpdateAsync(vm.Author);

                await _dialogService.ShowMessageAsync("Coming soon", "This function will be implemented soon", "OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update image for author {Author} ({ResourceId})", vm.Author.DisplayName, vm.Author.ResourceId);

                await _dialogService.ShowMessageAsync("Update failed", $"Error while updating image for Author {vm.Author.DisplayName} ({vm.Author.ResourceId}):\n\n{ex.Message}", "Close");
            }
        }

        private async Task VerifyUpdateAsync(AuthorViewModel vm)
        {
            try
            {
                await _cacheService.UpdateAsync(vm.Author);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update author {Author} ({ResourceId})", vm.Author.DisplayName, vm.Author.ResourceId);

                await _dialogService.ShowMessageAsync("Update failed", $"Error while updating Author {vm.Author.DisplayName} ({vm.Author.ResourceId}):\n\n{ex.Message}", "Close");
            }
        }

        private async Task VerifyDeleteAsync(AuthorViewModel vm)
        {
            bool isDeleteConfirmed = await _dialogService.ShowMessageAsync("Delete author?", "Deleting an author cannot be undone. Continue?", "Delete", "Cancel");

            if (!isDeleteConfirmed)
                return;

            try
            {
                await _cacheService.DeleteAsync<Author>(this.CurrentBlogId, vm.Author.ResourceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete author {Author} ({ResourceId})", vm.Author.DisplayName, vm.Author.ResourceId);

                await _dialogService.ShowMessageAsync("Update failed", $"Error while deleting Author {vm.Author.DisplayName} ({vm.Author.ResourceId}):\n\n{ex.Message}", "Close");
            }
        }
        
        

        public ObservableCollection<AuthorViewModel> Authors { get; set; } = new ObservableCollection<AuthorViewModel>();
    }
}
