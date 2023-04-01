using CommunityToolkit.Mvvm.ComponentModel;
using MSiccDev.ServerlessBlog.DtoModel;
namespace MSiccDev.ServerlessBlog.AdminClient.ViewModel
{
	public class AuthorViewModel : ObservableObject
	{
		private MediumViewWodel? _authorImage;
		private Uri? _authorImageUrl;

		private Author _author;

		public AuthorViewModel(Author author)
		{
			_author = author ?? throw new ArgumentNullException(nameof(author));
			this.AuthorImageUrl = _author.UserImage?.MediumUrl;
		}

		public string DisplayName
		{
			get => _author.DisplayName;
			set
			{
				_author.DisplayName = value;
				OnPropertyChanged();
			}
		}

		public string UserName
		{
			get => _author.UserName;
			set
			{
				_author.DisplayName = value;
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
	}
}
