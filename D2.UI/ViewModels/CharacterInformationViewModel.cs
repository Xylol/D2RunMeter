using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D2.Model;

namespace D2.UI.ViewModels;

public partial class CharacterInformationViewModel : ViewModelBase, IDisposable
{
    private readonly MainWindowViewModel mainWindowViewModel;
    private readonly CharacterDataLoader characterDataLoader;
    private Character characterData;
    private readonly DateTime sessionStartedAt = DateTime.Now;
    private readonly long sessionStartExp;
    private readonly long sessionStartGold;
    private long previousExperience;
    private const int FallbackPollingIntervalInSeconds = 10;
    private int runCounter;
    private readonly List<long> expOfRuns = [];
    private const int MaxRunHistorySize = 1000;
    private DateTime lastKnownFileTimestamp;
    private CancellationTokenSource? cancellationTokenSource;
    private FileSystemWatcher? fileWatcher;
    private readonly object updateLock = new();
    private System.Threading.Timer? debounceTimer;
    private const int DebounceDelayMilliseconds = 500;

    [ObservableProperty]
    private string characterName;

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

    [ObservableProperty]
    private string runsPerHour = string.Empty;

    public CharacterInformationViewModel(MainWindowViewModel mainWindowViewModel, CharacterDataLoader characterDataLoader)
    {
        this.mainWindowViewModel = mainWindowViewModel;
        this.characterDataLoader = characterDataLoader;
        this.characterData = this.characterDataLoader.GetCurrentCharacterData();

        this.characterName = this.characterData.Name;
        this.characterLevel = this.characterData.Level;

        this.sessionStartExp = this.characterData.Experience;
        this.sessionStartGold = GetGold();
        this.previousExperience = this.sessionStartExp;
        this.lastKnownFileTimestamp = this.characterDataLoader.GetLastWriteTime();

        StartMonitoring();
    }

    private void StartMonitoring()
    {
        cancellationTokenSource = new CancellationTokenSource();

        try
        {
            SetupFileSystemWatcher();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WARNING: FileSystemWatcher setup failed: {ex.Message}. Using polling fallback only.");
        }

        Task.Run(async () => await MonitorCharacterAsync(cancellationTokenSource.Token));
    }

    private void SetupFileSystemWatcher()
    {
        var filePath = this.characterDataLoader.CharacterFilePath;
        var directory = Path.GetDirectoryName(filePath);
        var fileName = Path.GetFileName(filePath);

        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentException("Invalid file path for FileSystemWatcher");
        }

        fileWatcher = new FileSystemWatcher(directory)
        {
            Filter = fileName,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };

        fileWatcher.Changed += OnFileChanged;
        fileWatcher.Error += OnFileWatcherError;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce file changes to prevent counting the same save multiple times
        debounceTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        debounceTimer = new System.Threading.Timer(
            _ => CheckForUpdates(),
            null,
            DebounceDelayMilliseconds,
            Timeout.Infinite);
    }

    private void OnFileWatcherError(object sender, ErrorEventArgs e)
    {
        Console.WriteLine($"WARNING: FileSystemWatcher error: {e.GetException().Message}. Relying on polling fallback.");
    }

    private void CheckForUpdates()
    {
        bool runDetected = false;

        lock (updateLock)
        {
            try
            {
                var currentFileTimestamp = this.characterDataLoader.GetLastWriteTime();

                // Only reload and count as new run if file timestamp actually changed (debouncing)
                if (currentFileTimestamp != this.lastKnownFileTimestamp)
                {
                    this.characterData = this.characterDataLoader.GetCurrentCharacterData();
                    var currentExperience = this.characterData.Experience;

                    // Count as new run whenever file write time changed
                    this.runCounter++;
                    this.expOfRuns.Add(currentExperience - this.previousExperience);
                    this.previousExperience = currentExperience;

                    // Update last known timestamp to prevent counting same write twice
                    this.lastKnownFileTimestamp = currentFileTimestamp;

                    // Prevent unbounded list growth
                    if (this.expOfRuns.Count > MaxRunHistorySize)
                    {
                        this.expOfRuns.RemoveAt(0);
                    }

                    runDetected = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Error checking for updates: {ex.Message}");
            }
        }

        // Trigger immediate UI update if run was detected
        if (runDetected)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        try
        {
            var currentExperience = this.characterData.Experience;
            var currentGold = GetGold();
            var now = DateTime.Now;

            var currentGoldPerHour = SessionStatsCalculator.CalculateGoldPerHour(
                currentGold, this.sessionStartGold, this.sessionStartedAt, now);
            var currentExperiencePerHour = SessionStatsCalculator.CalculateExperiencePerHour(
                currentExperience, this.sessionStartExp, this.sessionStartedAt, now);

            var hoursForLevelUp = SessionStatsCalculator.CalculateHoursForLevelUp(
                currentExperience, this.characterData.NextLevelAtExperience, currentExperiencePerHour);
            var runsForLevelUp = SessionStatsCalculator.CalculateRunsForLevelUp(
                currentExperience, this.sessionStartExp, this.characterData.NextLevelAtExperience, this.runCounter);
            var progressForLevelUp = SessionStatsCalculator.CalculateLevelUpProgressPercentage(
                currentExperience, this.characterData.ExperienceRequiredForCurrentLevel, this.characterData.NextLevelAtExperience);
            var sessionMinutes = SessionStatsCalculator.CalculateSessionMinutes(this.sessionStartedAt, now);
            var currentRunsPerHour = SessionStatsCalculator.CalculateRunsPerHour(this.runCounter, this.sessionStartedAt, now);

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                CurrentDateTime = now.ToString("yyyy-MM-dd HH:mm:ss");
                LevelUpEta = hoursForLevelUp > 999999 ? "N/A" : ReadabilityHelper.ConvertToHoursAndMinutesText(hoursForLevelUp);
                ExpPerHour = ReadabilityHelper.ConvertToSi(currentExperiencePerHour);
                GoldPerHour = ReadabilityHelper.ConvertToSi(currentGoldPerHour);
                LevelUpProgress = $"{progressForLevelUp:0.00}%";
                SessionTimer = $"{sessionMinutes:0}m";
                Runs = this.runCounter;
                LevelUpRunsEta = runsForLevelUp > 999999 ? "N/A" : $"{runsForLevelUp:0}";
                RunsPerHour = $"{currentRunsPerHour:0}";
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Error updating UI: {ex.Message}");
        }
    }

    private async Task MonitorCharacterAsync(CancellationToken cancellationToken)
    {
        var uiUpdateCounter = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Check for updates (called every 10s as fallback)
                CheckForUpdates();

                // Update UI every 2 iterations (20 seconds) for regular clock/timer updates
                if (uiUpdateCounter % 2 == 0)
                {
                    UpdateUI();
                }

                uiUpdateCounter++;

                await Task.Delay(FallbackPollingIntervalInSeconds * 1000, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Error during character monitoring: {ex.Message}");
            }
        }
    }

    private long GetGold() => this.characterData.GoldInventory + this.characterData.GoldStash;

    [RelayCommand]
    private void Back()
    {
        mainWindowViewModel.NavigateToMainMenu();
    }

    public void Dispose()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();

        debounceTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        debounceTimer?.Dispose();

        if (fileWatcher != null)
        {
            fileWatcher.EnableRaisingEvents = false;
            fileWatcher.Changed -= OnFileChanged;
            fileWatcher.Error -= OnFileWatcherError;
            fileWatcher.Dispose();
        }
    }
}