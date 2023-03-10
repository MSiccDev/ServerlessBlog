using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace MSiccDev.ServerlessBlog.AdminClient.ViewModel
{
	public abstract class BaseViewModel : ObservableObject
	{
		private string? _title = string.Empty;

		private AsyncRelayCommand? _viewAppearingCommand;
		private AsyncRelayCommand? _viewDisappearingCommand;



		protected virtual bool CanExecuteViewAppearing() =>
			true;

		protected virtual bool CanExecuteViewDisappearing() =>
			true;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		protected virtual async Task ExecuteViewAppearingAsync()
		{
		}

		protected virtual async Task ExecuteViewDisappearingAsync()
		{

		}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

		public string? Title { get => _title; set => SetProperty(ref _title, value); }
		public AsyncRelayCommand ViewAppearingCommand => _viewAppearingCommand ??= new AsyncRelayCommand(ExecuteViewAppearingAsync, CanExecuteViewAppearing);
		public AsyncRelayCommand ViewDisappearingCommand => _viewDisappearingCommand ??= new AsyncRelayCommand(ExecuteViewDisappearingAsync, CanExecuteViewDisappearing);

	}
}

