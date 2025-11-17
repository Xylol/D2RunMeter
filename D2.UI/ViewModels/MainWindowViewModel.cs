using System;
using CommunityToolkit.Mvvm.ComponentModel;
using D2.UI.Services;

namespace D2.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly SettingsService settingsService;

    [ObservableProperty]
    private ViewModelBase? currentViewModel;

    [ObservableProperty]
    private bool isAlwaysOnTop;

    public MainWindowViewModel()
    {
        settingsService = new SettingsService();
        isAlwaysOnTop = settingsService.GetIsAlwaysOnTop();
        InitializeNavigation();
    }

    partial void OnIsAlwaysOnTopChanged(bool value)
    {
        settingsService.SaveIsAlwaysOnTop(value);
    }

    private void InitializeNavigation()
    {
        var saveGamePath = settingsService.GetSaveGamePath();

        if (string.IsNullOrWhiteSpace(saveGamePath))
        {
            NavigateTo(new PathSelectionViewModel(settingsService, this));
        }
        else
        {
            NavigateTo(new MainMenuViewModel(settingsService, this));
        }
    }

    public void NavigateTo(ViewModelBase viewModel)
    {
        if (CurrentViewModel is IDisposable disposable)
        {
            disposable.Dispose();
        }
        CurrentViewModel = viewModel;
    }

    public void ExitApplication()
    {
        Environment.Exit(0);
    }
}
