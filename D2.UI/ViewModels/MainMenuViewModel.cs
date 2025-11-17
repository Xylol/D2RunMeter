using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D2.UI.Services;

namespace D2.UI.ViewModels
{
    public partial class MainMenuViewModel : ViewModelBase
    {
        private readonly SettingsService settingsService;
        private readonly MainWindowViewModel mainWindowViewModel;

        [ObservableProperty]
        private int selectedIndex = 0;

        public MainMenuViewModel(
            SettingsService settingsService,
            MainWindowViewModel mainWindowViewModel)
        {
            this.settingsService = settingsService;
            this.mainWindowViewModel = mainWindowViewModel;
        }

        [RelayCommand]
        public void ExecuteSelected()
        {
            switch (SelectedIndex)
            {
                case 0:
                    Resume();
                    break;
                case 1:
                    LoadCharacter();
                    break;
                case 2:
                    Settings();
                    break;
                case 3:
                    About();
                    break;
                case 4:
                    Exit();
                    break;
            }
        }

        public void MoveSelectionUp()
        {
            if (SelectedIndex > 0)
            {
                SelectedIndex--;
            }
        }

        public void MoveSelectionDown()
        {
            if (SelectedIndex < 4)
            {
                SelectedIndex++;
            }
        }

        public void SelectByNumber(int number)
        {
            if (number >= 1 && number <= 5)
            {
                SelectedIndex = number - 1;
                ExecuteSelected();
            }
        }

        [RelayCommand]
        private void Resume()
        {
            if (string.IsNullOrWhiteSpace(settingsService.GetSelectedCharacter()))
            {
                LoadCharacter();
                return;
            }
            mainWindowViewModel.NavigateTo(new CharacterInformationViewModel(settingsService, mainWindowViewModel));
        }

        [RelayCommand]
        private void LoadCharacter()
        {
            mainWindowViewModel.NavigateTo(new LoadCharacterViewModel(settingsService, mainWindowViewModel));
        }

        [RelayCommand]
        private void Settings()
        {
            mainWindowViewModel.NavigateTo(new SettingsViewModel(settingsService, mainWindowViewModel));
        }

        [RelayCommand]
        private void About()
        {
            mainWindowViewModel.NavigateTo(new AboutViewModel(settingsService, mainWindowViewModel));
        }

        [RelayCommand]
        private void Exit()
        {
            mainWindowViewModel.ExitApplication();
        }
    }
}
