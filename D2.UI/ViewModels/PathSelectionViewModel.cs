using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D2.UI.Services;

namespace D2.UI.ViewModels;

public partial class PathSelectionViewModel(
    SettingsService settingsService,
    MainWindowViewModel mainWindowViewModel)
    : ViewModelBase
{
    [ObservableProperty]
    private string? pathInput;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string? currentPath = settingsService.GetSaveGamePath();

    [RelayCommand]
    private void SavePath()
    {
        if (string.IsNullOrWhiteSpace(PathInput))
        {
            ErrorMessage = "Error: Path cannot be empty.";
            return;
        }

        var path = PathInput.Trim();

        if (!path.ToLower().EndsWith("save"))
        {
            ErrorMessage = $"Error: Path must end with 'save'. Your path ends with: '{path.Substring(Math.Max(0, path.Length - 10))}'";
            return;
        }

        if (!Directory.Exists(path))
        {
            ErrorMessage = $"Error: Directory does not exist: {path}";
            return;
        }

        try
        {
            settingsService.SaveSaveGamePath(path);
            ErrorMessage = null;
            mainWindowViewModel.NavigateTo(new MainMenuViewModel(settingsService, mainWindowViewModel));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save path: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Back()
    {
        mainWindowViewModel.NavigateTo(new MainMenuViewModel(settingsService, mainWindowViewModel));
    }
}