using System;
using System.IO;
using System.Text.Json;

namespace D2.UI.Services
{
    public class BaseSettings
    {
        public string? SelectedCharacter { get; set; }
        public string? SaveGamePath { get; set; }
        public bool IsAlwaysOnTop { get; set; }
        public double WindowWidth { get; set; } = 320;
        public double WindowHeight { get; set; } = 600;
    }

    public class SettingsService
    {
        private readonly string settingsFilePath;

        public SettingsService()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            this.settingsFilePath = Path.Combine(baseDirectory, "BaseSettings.json");
        }

        public string? GetSelectedCharacter()
        {
            var loadedSettings = LoadBaseSettings();
            return loadedSettings.SelectedCharacter;
        }

        public string? GetSaveGamePath()
        {
            var loadedSettings = LoadBaseSettings();
            return loadedSettings.SaveGamePath;
        }

        public bool GetIsAlwaysOnTop()
        {
            var loadedSettings = LoadBaseSettings();
            return loadedSettings.IsAlwaysOnTop;
        }

        private BaseSettings LoadBaseSettings()
        {
            if (!File.Exists(this.settingsFilePath))
            {
                return new BaseSettings()
                {
                    SelectedCharacter = null,
                    SaveGamePath = null,
                    IsAlwaysOnTop = false,
                    WindowWidth = 320,
                    WindowHeight = 600
                };
            }

            var baseSettingsText = File.ReadAllText(this.settingsFilePath);
            var deserializedBaseSettings = JsonSerializer.Deserialize<BaseSettings>(baseSettingsText);
            return new BaseSettings()
            {
                SelectedCharacter = deserializedBaseSettings?.SelectedCharacter,
                SaveGamePath = deserializedBaseSettings?.SaveGamePath,
                IsAlwaysOnTop = deserializedBaseSettings?.IsAlwaysOnTop ?? false,
                WindowWidth = deserializedBaseSettings?.WindowWidth ?? 320,
                WindowHeight = deserializedBaseSettings?.WindowHeight ?? 600
            };
        }

        public void SaveSelectedCharacter(string lastLoadedCharacter)
        {
            var currentSettings = LoadBaseSettings();
            currentSettings.SelectedCharacter = lastLoadedCharacter;
            var serializedSettings = JsonSerializer.Serialize(currentSettings);
            File.WriteAllText(this.settingsFilePath, serializedSettings);
        }

        public void SaveSaveGamePath(string saveGamePath)
        {
            var currentSettings = LoadBaseSettings();
            currentSettings.SaveGamePath = saveGamePath;
            var serializedSettings = JsonSerializer.Serialize(currentSettings);
            File.WriteAllText(this.settingsFilePath, serializedSettings);
        }

        public void SaveIsAlwaysOnTop(bool isAlwaysOnTop)
        {
            var currentSettings = LoadBaseSettings();
            currentSettings.IsAlwaysOnTop = isAlwaysOnTop;
            var serializedSettings = JsonSerializer.Serialize(currentSettings);
            File.WriteAllText(this.settingsFilePath, serializedSettings);
        }

        public (double Width, double Height) GetWindowDimensions()
        {
            var loadedSettings = LoadBaseSettings();
            return (loadedSettings.WindowWidth, loadedSettings.WindowHeight);
        }

        public void SaveWindowDimensions(double width, double height)
        {
            var currentSettings = LoadBaseSettings();
            currentSettings.WindowWidth = width;
            currentSettings.WindowHeight = height;
            var serializedSettings = JsonSerializer.Serialize(currentSettings);
            File.WriteAllText(this.settingsFilePath, serializedSettings);
        }
    }
}
