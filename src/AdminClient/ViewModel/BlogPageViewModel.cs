using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MSiccDev.ServerlessBlog.AdminClient.Common;
using MSiccDev.ServerlessBlog.AdminClient.Services;
using MSiccDev.ServerlessBlog.ClientSdk;
using MSiccDev.ServerlessBlog.DtoModel;
namespace MSiccDev.ServerlessBlog.AdminClient.ViewModel
{
	// ReSharper disable ClassNeverInstantiated.Global
	public class BlogPagePageViewModel : BasePageWithAuthorizationViewModel
		// ReSharper restore ClassNeverInstantiated.Global
	{
		private readonly ILogger<BlogPagePageViewModel> _logger;
		private readonly ICacheService _cacheService;
		private readonly IBlogClient _blogClient;

		private BlogOverview? _savedSelectedBlog;


		private RelayCommand? _deleteBlogImageCommand;
		private AsyncRelayCommand? _uploadBlogImageCommand;

		private AsyncRelayCommand? _refreshCommand;
		private AsyncRelayCommand? _saveCommand;

		public BlogPagePageViewModel(IAuthorizationService authorizationService, ILogger<BlogPagePageViewModel> logger, ICacheService cacheService, IBlogClient blogClient) :
			base(authorizationService)
		{
			_logger = logger;
			_cacheService = cacheService;
			_blogClient = blogClient;
			
			Connectivity.ConnectivityChanged += (sender, args) =>
			{
				NotifyCommandCanExecuteChanges();
			};
		}

		protected override async Task ExecuteViewAppearingAsync()
		{
			string savedSelectedBlogIdString = Preferences.Default.Get(Constants.CurrentSelectedBlogIdStorageName, string.Empty);
			if (!string.IsNullOrWhiteSpace(savedSelectedBlogIdString))
			{
				Guid selectedBlogId = Guid.Parse(savedSelectedBlogIdString);

				if (selectedBlogId != Guid.Empty)
				{
					_savedSelectedBlog = await _cacheService.GetCurrentBlogAsync(selectedBlogId);

					_logger.LogDebug("Loaded saved selected Blog with Id {BlogId}", _savedSelectedBlog?.BlogId.ToString());

					RaisePropertyChanges();
					NotifyCommandCanExecuteChanges();
				}
			}
		}

		private Task UploadBlogImageAsync() =>
			//TODO:
			//UPLOAD image to Azure Blob Storage
			//save uri of it here.
			throw new NotImplementedException();

		private void DeleteBlogImage()
		{
			this.BlogLogoUrl = null;
			this.DeleteBlogImageCommand.NotifyCanExecuteChanged();
			this.UploadBlogImageCommand.NotifyCanExecuteChanged();
		}

		private async Task RefreshCurrentSelectedBlogAsync()
		{
			if (_savedSelectedBlog != null)
			{
				_savedSelectedBlog = await _cacheService.GetCurrentBlogAsync(_savedSelectedBlog.BlogId.GetValueOrDefault(), forceRefresh: true);
				
				RaisePropertyChanges();
			}
			else
			{
				await _authorizationService.RefreshAuthorizationAsync();
			}
		}

		private Task SaveCurrentSelectedBlogAsync() =>
			//TODO:
			//Save changes to DB via BlogClient
			throw new NotImplementedException();

		private void RaisePropertyChanges()
		{
			OnPropertyChanged(nameof(this.BlogName));
			OnPropertyChanged(nameof(this.BlogLogoUrl));
			OnPropertyChanged(nameof(this.Slogan));
			OnPropertyChanged(nameof(this.AuthorCount));
			OnPropertyChanged(nameof(this.MediaCount));
			OnPropertyChanged(nameof(this.TagCount));
			OnPropertyChanged(nameof(this.PostCount));
		}

		private void NotifyCommandCanExecuteChanges()
		{
			this.RefreshCommand.NotifyCanExecuteChanged();
			this.SaveCommand.NotifyCanExecuteChanged();
			this.UploadBlogImageCommand.NotifyCanExecuteChanged();
			this.DeleteBlogImageCommand.NotifyCanExecuteChanged();
		}

		private bool CanExecuteUploadBlogImage() => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

		private bool CanExecuteDeleteBlogImage() => this.BlogLogoUrl != null;

		private bool CanExecuteSave() =>
			Connectivity.Current.NetworkAccess == NetworkAccess.Internet &&
			_savedSelectedBlog != null &&
			this.BlogLogoUrl != null &&
			!string.IsNullOrWhiteSpace(this.BlogName) &&
			!string.IsNullOrWhiteSpace(this.Slogan);


		private bool CanExecuteRefresh() => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;


		public string? BlogName
		{
			get => _savedSelectedBlog?.Name;
			set
			{
				if (_savedSelectedBlog != null)
				{
					_savedSelectedBlog.Name = value;
					OnPropertyChanged();
				}
			}
		}

		public string? Slogan
		{
			get => _savedSelectedBlog?.Slogan;
			set
			{
				if (_savedSelectedBlog != null)
				{
					_savedSelectedBlog.Slogan = value;
					OnPropertyChanged();
				}
			}

		}

		public Uri? BlogLogoUrl
		{
			get => _savedSelectedBlog?.LogoUrl;
			set
			{
				if (_savedSelectedBlog != null)
				{
					_savedSelectedBlog.LogoUrl = value;
					OnPropertyChanged();
				}
			}
		}

		public int PostCount => _savedSelectedBlog?.PostCount ?? 0;

		public int TagCount => _savedSelectedBlog?.TagCount ?? 0;

		public int AuthorCount => _savedSelectedBlog?.AuthorsCount ?? 0;

		public int MediaCount => _savedSelectedBlog?.MediaCount ?? 0;

		public AsyncRelayCommand RefreshCommand => _refreshCommand ??= new AsyncRelayCommand(RefreshCurrentSelectedBlogAsync, CanExecuteRefresh);
		public AsyncRelayCommand SaveCommand => _saveCommand ??= new AsyncRelayCommand(SaveCurrentSelectedBlogAsync, CanExecuteSave);

		public AsyncRelayCommand UploadBlogImageCommand => _uploadBlogImageCommand ??= new AsyncRelayCommand(UploadBlogImageAsync, CanExecuteUploadBlogImage);

		public RelayCommand DeleteBlogImageCommand => _deleteBlogImageCommand ??= new RelayCommand(DeleteBlogImage, CanExecuteDeleteBlogImage);





	}
}

