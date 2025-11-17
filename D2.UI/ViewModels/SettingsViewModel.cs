using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D2.UI.Services;

namespace D2.UI.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsService settingsService;
        private readonly MainWindowViewModel mainWindowViewModel;

        public SettingsViewModel(
            SettingsService settingsService,
            MainWindowViewModel mainWindowViewModel)
        {
            this.settingsService = settingsService;
            this.mainWindowViewModel = mainWindowViewModel;
        }

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
