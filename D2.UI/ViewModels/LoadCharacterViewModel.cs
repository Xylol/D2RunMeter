using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D2.UI.Services;

namespace D2.UI.ViewModels;

public partial class LoadCharacterViewModel : ViewModelBase
{
    private readonly SettingsService settingsService;
    private readonly MainWindowViewModel mainWindowViewModel;

    [ObservableProperty]
    private string? characterName;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string? currentCharacter;

    [ObservableProperty]
    private List<string> availableCharacters = [];

    public List<string> FilteredCharacters
    {
        get
        {
            if (string.IsNullOrWhiteSpace(CharacterName))
            {
                return AvailableCharacters;
            }

            return AvailableCharacters
                .Where(c => c.Contains(CharacterName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    public LoadCharacterViewModel(SettingsService settingsService, MainWindowViewModel mainWindowViewModel)
    {
        this.settingsService = settingsService;
        this.mainWindowViewModel = mainWindowViewModel;
        this.currentCharacter = settingsService.GetSelectedCharacter();

        var saveGamePath = settingsService.GetSaveGamePath();
        this.availableCharacters = SaveGameHelperService.GetListOfAvailableCharacters(saveGamePath);
    }

    partial void OnCharacterNameChanged(string? value)
    {
        OnPropertyChanged(nameof(FilteredCharacters));
        ErrorMessage = null;
    }

    [RelayCommand]
    private void LoadCharacter()
    {
        if (string.IsNullOrWhiteSpace(CharacterName) || CharacterName.Length > 15)
        {
            ErrorMessage = "Error: Name cannot be empty or longer than 15 characters.";
            return;
        }

        if (!AvailableCharacters.Any(available => available.Equals(CharacterName, StringComparison.OrdinalIgnoreCase)))
        {
            if (AvailableCharacters.Count > 0)
            {
                var random = new Random();
                var randomCharacter = AvailableCharacters[random.Next(AvailableCharacters.Count)];
                ErrorMessage = $"Error: No Character with this name, you could try this one: {randomCharacter}";
            }
            else
            {
                ErrorMessage = "Error: No Character with this name.";
            }
            return;
        }

        settingsService.SaveSelectedCharacter(CharacterName);
        ErrorMessage = null;
        mainWindowViewModel.NavigateTo(new CharacterInformationViewModel(settingsService, mainWindowViewModel));
    }

    [RelayCommand]
    private void SelectCharacter(string characterName)
    {
        CharacterName = characterName;
        settingsService.SaveSelectedCharacter(characterName);
        ErrorMessage = null;
        mainWindowViewModel.NavigateTo(new CharacterInformationViewModel(settingsService, mainWindowViewModel));
    }

    [RelayCommand]
    private void Back()
    {
        mainWindowViewModel.NavigateTo(new MainMenuViewModel(settingsService, mainWindowViewModel));
    }
}