using CommunityToolkit.Mvvm.ComponentModel;
using MSiccDev.ServerlessBlog.DtoModel;
namespace MSiccDev.ServerlessBlog.AdminClient.ViewModel
{
    public class MediumTypeViewModel : ObservableObject
    {
        public MediumType MediumType { get; }

        public MediumTypeViewModel(MediumType mediumType) =>
            this.MediumType = mediumType;

        public string Name
        {
            get => this.MediumType.Name;
            set
            {
                this.MediumType.Name = value;
                OnPropertyChanged();
            }
        }

        public string MimeType
        {
            get => this.MediumType.MimeType;
            set
            {
                this.MediumType.MimeType = value;
                OnPropertyChanged();
            }
        }

        public string Encoding
        {
            get => this.MediumType.Encoding;
            set
            {
                this.MediumType.Encoding = value;
                OnPropertyChanged();
            }
        }
    }
}
