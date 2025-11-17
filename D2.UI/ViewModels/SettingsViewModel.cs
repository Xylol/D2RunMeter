using CommunityToolkit.Mvvm.Input;
using D2.UI.Services;

namespace D2.UI.ViewModels
{
    public partial class SettingsViewModel(
        SettingsService settingsService,
        MainWindowViewModel mainWindowViewModel)
        : ViewModelBase
    {
        public bool IsAlwaysOnTop
        {
            get => mainWindowViewModel.IsAlwaysOnTop;
            set
            {
                if (mainWindowViewModel.IsAlwaysOnTop != value)
                {
                    mainWindowViewModel.IsAlwaysOnTop = value;
                    OnPropertyChanged(nameof(IsAlwaysOnTop));
                }
            }
        }

        [RelayCommand]
        private void NavigateToPathSelection()
        {
            mainWindowViewModel.NavigateTo(new PathSelectionViewModel(settingsService, mainWindowViewModel));
        }

        [RelayCommand]
        private void Back()
        {
            mainWindowViewModel.NavigateTo(new MainMenuViewModel(settingsService, mainWindowViewModel));
        }
    }
}
