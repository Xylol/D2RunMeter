using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D2.Model;
using D2.UI.Services;

namespace D2.UI.ViewModels
{
    public partial class CharacterInformationViewModel : ViewModelBase, IDisposable
    {
        private readonly SettingsService settingsService;
        private readonly MainWindowViewModel mainWindowViewModel;
        private readonly CharacterDataLoader characterDataLoader;
        private Character characterData;
        private readonly List<long> experienceHistory = new();
        private readonly List<long> goldHistory = new();
        private const int MaxHistorySize = 3600; // Keep max 1 hour of history
        private long previousExperience;
        private long previousGold;
        private const int TimeBetweenCharacterRefreshInSeconds = 1;
        private readonly DateTime sessionStartedAt = DateTime.Now;
        private const double OneHourInSeconds = 3600.0;
        private int runCounter;
        private readonly List<long> expOfRuns = new();
        private DateTime previousChangedAt;
        private CancellationTokenSource? cancellationTokenSource;

        [ObservableProperty]
        private string characterName = string.Empty;

        [ObservableProperty]
        private int characterLevel;

        [ObservableProperty]
        private string currentDateTime = string.Empty;

        [ObservableProperty]
        private string levelUpEta = string.Empty;

        [ObservableProperty]
        private string expPerHour = string.Empty;

        [ObservableProperty]
        private string goldPerHour = string.Empty;

        [ObservableProperty]
        private string levelUpProgress = string.Empty;

        [ObservableProperty]
        private string sessionTimer = string.Empty;

        [ObservableProperty]
        private int runs;

        [ObservableProperty]
        private string levelUpRunsEta = string.Empty;

        public CharacterInformationViewModel(SettingsService settingsService, MainWindowViewModel mainWindowViewModel)
        {
            this.settingsService = settingsService;
            this.mainWindowViewModel = mainWindowViewModel;

            var path = settingsService.GetSaveGamePath();
            var chosenCharacter = settingsService.GetSelectedCharacter();

            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(chosenCharacter))
            {
                throw new InvalidOperationException("Path or character not set");
            }

            var characterFullPath = Path.Combine(path, $"{chosenCharacter}.d2s");
            this.characterDataLoader = new CharacterDataLoader(characterFullPath);
            this.characterData = this.characterDataLoader.GetCurrentCharacterData();

            this.characterName = this.characterData.Name;
            this.characterLevel = this.characterData.Level;

            this.previousExperience = this.characterData.Experience;
            this.previousGold = GetGold();
            this.previousChangedAt = this.characterData.LastChangedAt;

            StartMonitoring();
        }

        private void StartMonitoring()
        {
            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(async () => await MonitorCharacterAsync(cancellationTokenSource.Token));
        }

        private async Task MonitorCharacterAsync(CancellationToken cancellationToken)
        {
            int updateCounter = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    this.characterData = this.characterDataLoader.GetCurrentCharacterData();

                    var currentExperience = this.characterData.Experience;

                    var currentLastChangedAt = this.characterData.LastChangedAt;
                    if (currentLastChangedAt > this.previousChangedAt)
                    {
                        this.runCounter++;
                        this.expOfRuns.Add(currentExperience - this.previousExperience);
                        this.previousChangedAt = currentLastChangedAt;
                    }

                    var currentGold = GetGold();

                    UpdateHistory(currentExperience, currentGold);

                    var currentGoldPerHour = GetCurrentGoldPerHour();
                    var experienceThresholdForLevelUp = this.characterData.NextLevelAtExperience;
                    var currentExperiencePerHour = GetCurrentExperiencePerHour();
                    var experienceDelta = experienceThresholdForLevelUp - currentExperience;

                    double hoursForLevelUp = 999999999;
                    double runsForLevelUp = 999999999;
                    if (currentExperiencePerHour > 0)
                    {
                        hoursForLevelUp = experienceDelta / currentExperiencePerHour;
                        if (this.expOfRuns.Count > 0)
                        {
                            runsForLevelUp = experienceDelta / this.expOfRuns.Average();
                        }
                    }

                    if (updateCounter % 5 == 0)
                    {
                        // Marshal UI updates to the UI thread
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            CurrentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            LevelUpEta = ReadabilityHelper.ConvertToHoursAndMinutesText(hoursForLevelUp);
                            ExpPerHour = ReadabilityHelper.ConvertToSi(currentExperiencePerHour);
                            GoldPerHour = ReadabilityHelper.ConvertToSi(currentGoldPerHour);
                            LevelUpProgress = $"{CalculateLvlUpProgressPercentage():0.00}%";
                            SessionTimer = $"{(DateTime.Now - this.sessionStartedAt).TotalMinutes:0}m";
                            Runs = this.runCounter;
                            LevelUpRunsEta = $"{runsForLevelUp:0}";
                        });
                    }

                    updateCounter++;

                    await Task.Delay(TimeBetweenCharacterRefreshInSeconds * 1000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // Ignore errors during monitoring
                }
            }
        }

        private double GetCurrentExperiencePerHour()
        {
            if (experienceHistory.Count == 0) return 0;
            var result = this.experienceHistory.Average() * (OneHourInSeconds / TimeBetweenCharacterRefreshInSeconds);
            return result;
        }

        private double GetCurrentGoldPerHour()
        {
            if (goldHistory.Count == 0) return 0;
            var result = this.goldHistory.Average() * (OneHourInSeconds / TimeBetweenCharacterRefreshInSeconds);
            return result;
        }

        private void UpdateHistory(long currentExperience, long currentGold)
        {
            this.experienceHistory.Add(currentExperience - this.previousExperience);
            this.previousExperience = currentExperience;

            this.goldHistory.Add(currentGold - this.previousGold);
            this.previousGold = currentGold;

            // Limit history size to prevent unbounded memory growth
            if (this.experienceHistory.Count > MaxHistorySize)
            {
                this.experienceHistory.RemoveAt(0);
            }
            if (this.goldHistory.Count > MaxHistorySize)
            {
                this.goldHistory.RemoveAt(0);
            }
        }

        private long GetGold() => this.characterData.GoldInventory + this.characterData.GoldStash;

        private double CalculateLvlUpProgressPercentage()
        {
            var currentExp = this.characterData.Experience;
            var thresholdCurrentLevel = this.characterData.ExperienceRequiredForCurrentLevel;
            var thresholdNextLevel = this.characterData.NextLevelAtExperience;
            var differenceBetweenTresholds = thresholdNextLevel - thresholdCurrentLevel;
            if (differenceBetweenTresholds == 0) return 0;
            double experienceCollectedOnThisLevel = currentExp - thresholdCurrentLevel;
            return (experienceCollectedOnThisLevel / differenceBetweenTresholds) * 100;
        }

        [RelayCommand]
        private void Back()
        {
            mainWindowViewModel.NavigateTo(new MainMenuViewModel(settingsService, mainWindowViewModel));
        }

        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }
    }
}
