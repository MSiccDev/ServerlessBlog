using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using MSiccDev.ServerlessBlog.AdminClient.Services;
using MSiccDev.ServerlessBlog.ClientSdk;
using MSiccDev.ServerlessBlog.DtoModel;
namespace MSiccDev.ServerlessBlog.AdminClient.ViewModel
{
    public class AuthorPageViewModel : BasePageViewModel
    {
        private readonly ILogger<AuthorPageViewModel> _logger;
        private readonly ICacheService _cacheService;
        private readonly IBlogClient _blogClient;

        public AuthorPageViewModel(ILogger<AuthorPageViewModel> logger, ICacheService cacheService, IBlogClient blogClient)
        {
            _logger = logger;
            _cacheService = cacheService;
            _blogClient = blogClient;

            Connectivity.ConnectivityChanged += (sender, args) =>
            {
                NotifyCommandCanExecuteChanges();
            };

            this.Title = "Authors";

        }

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

            foreach (Author author in cachedAuthors!)
            {
                this.Authors.Add(new AuthorViewModel(author));
            }
        }



        public ObservableCollection<AuthorViewModel> Authors { get; set; } = new ObservableCollection<AuthorViewModel>();
    }
}
