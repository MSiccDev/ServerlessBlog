using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MSiccDev.ServerlessBlog.DtoModel;
namespace MSiccDev.ServerlessBlog.AdminClient.ViewModel
{
	public class AuthorViewModel : ObservableObject
	{
		private MediumViewWodel? _authorImage;
		private Uri? _authorImageUrl;

		private readonly WeakEventManager _weakEventManager;

		public event EventHandler<AuthorViewModel> AuthorImageChangeRequested
		{
			add => _weakEventManager.AddEventHandler(value);
			remove => _weakEventManager.RemoveEventHandler(value);
		}

		public event EventHandler<AuthorViewModel> AuthorUpdateRequested
		{
			add => _weakEventManager.AddEventHandler(value);
			remove => _weakEventManager.RemoveEventHandler(value);
		}

		public event EventHandler<AuthorViewModel> AuthorDeleteRequested
		{
			add => _weakEventManager.AddEventHandler(value);
			remove => _weakEventManager.RemoveEventHandler(value);
		}
		
		public AuthorViewModel(Author author)
		{
			this.Author = author ?? throw new ArgumentNullException(nameof(author));
			this.AuthorImageUrl = this.Author.UserImage?.MediumUrl;

			_weakEventManager = new WeakEventManager();
			this.AuthorImageEditCommand = new RelayCommand(RequestAuthorImageUpdate);
			this.AuthorUpdateClickedCommand = new RelayCommand(RequestAuthorUpdate);
			this.AuthorDeleteClickedCommand = new RelayCommand(RequestAuthorDelete);
		}



		private void RequestAuthorImageUpdate() =>
			_weakEventManager.HandleEvent(this, this, nameof(this.AuthorImageChangeRequested));

		private void RequestAuthorDelete() =>
			_weakEventManager.HandleEvent(this, this, nameof(this.AuthorDeleteRequested));

		private void RequestAuthorUpdate() =>
			_weakEventManager.HandleEvent(this, this, nameof(this.AuthorUpdateRequested));

		public Author Author { get; }

		
		public string DisplayName
		{
			get => this.Author.DisplayName;
			set
			{
				this.Author.DisplayName = value;
				OnPropertyChanged();
			}
		}

		public string UserName
		{
			get => this.Author.UserName;
			set
			{
				this.Author.DisplayName = value;
				OnPropertyChanged();
			}
		}

		public Uri? AuthorImageUrl
		{
			get => _authorImageUrl;
			set
			{
				_authorImageUrl = value;
				OnPropertyChanged();
			}
		}

		public RelayCommand AuthorImageEditCommand { get; }
		public RelayCommand AuthorUpdateClickedCommand { get; }
		public RelayCommand AuthorDeleteClickedCommand { get; }
	}
}
