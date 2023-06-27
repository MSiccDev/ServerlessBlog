using CommunityToolkit.Mvvm.ComponentModel;
using MSiccDev.ServerlessBlog.DtoModel;
namespace MSiccDev.ServerlessBlog.AdminClient.ViewModel
{
    public class MediumViewWodel : ObservableObject
    {
        private MediumTypeViewModel? _mediumTypeViewModel;
        public Medium Medium { get; }

        public MediumViewWodel(Medium medium) =>
            this.Medium = medium;

        public Uri? MediumUrl
        {
            get => this.Medium.MediumUrl;
            set
            {
                this.Medium.MediumUrl = value;
                OnPropertyChanged();
            }
        }

        public MediumTypeViewModel? MediumTypeViewModel
        {
            get => _mediumTypeViewModel ?? new MediumTypeViewModel(this.Medium.MediumType);
            set
            {
                if (SetProperty(ref _mediumTypeViewModel, value))
                    this.Medium.MediumType = _mediumTypeViewModel?.MediumType;
            }
        }

        public string AltText
        {
            get => this.Medium.AlternativeText;
            set
            {
                this.Medium.AlternativeText = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => this.Medium.Description;
            set
            {
                this.Medium.Description = value;
                OnPropertyChanged();
            }
        }
    }
}
