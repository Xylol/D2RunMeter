using System.Windows.Input;
using D2.UI.Commands;
using D2.UI.Services;

namespace D2.UI.ViewModels;

public class AboutViewModel : ViewModelBase
{
    private readonly SettingsService settingsService;
    private readonly MainWindowViewModel mainWindowViewModel;

    public string Version { get; } = "D2RunMeter v1.0.0";
    public string GitHubUrl { get; } = "github.com/Xylol/D2RunMeter";

    public ICommand BackCommand { get; }

    public AboutViewModel(SettingsService settingsService, MainWindowViewModel mainWindowViewModel)
    {
        this.settingsService = settingsService;
        this.mainWindowViewModel = mainWindowViewModel;
        BackCommand = new RelayCommand(Back);
    }

    private void Back()
    {
        mainWindowViewModel.NavigateTo(new MainMenuViewModel(settingsService, mainWindowViewModel));
    }
}