using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D2.Model;
using D2.UI.Services;

namespace D2.UI.ViewModels;

public partial class MainMenuViewModel(
    SettingsService settingsService,
    MainWindowViewModel mainWindowViewModel)
    : ViewModelBase
{
    [ObservableProperty]
    private int selectedIndex = 0;

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
        var path = settingsService.GetSaveGamePath();
        var chosenCharacter = settingsService.GetSelectedCharacter();

        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(chosenCharacter))
        {
            LoadCharacter();
            return;
        }

        var characterFullPath = Path.Combine(path, $"{chosenCharacter}.d2s");
        var characterDataLoader = new CharacterDataLoader(characterFullPath, new ContentLoader());
        mainWindowViewModel.NavigateTo(new CharacterInformationViewModel(mainWindowViewModel, characterDataLoader));
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