using System;
using Avalonia.Controls;
using D2.UI.Services;

namespace D2.UI.Views;

public partial class MainWindow : Window
{
    private readonly SettingsService settingsService;

    public MainWindow()
    {
        InitializeComponent();
        settingsService = new SettingsService();
        LoadWindowDimensions();

        // Subscribe to window property changes to save dimensions
        this.PropertyChanged += OnPropertyChanged;
    }

    private void LoadWindowDimensions()
    {
        var (width, height) = settingsService.GetWindowDimensions();
        Width = width;
        Height = height;
    }

    private void OnPropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(Width) || e.Property.Name == nameof(Height))
        {
            // Save dimensions when they change
            if (Width > 0 && Height > 0)
            {
                settingsService.SaveWindowDimensions(Width, Height);
            }
        }
        else if (e.Property.Name == nameof(Topmost))
        {
            // Save always on top setting when it changes (via right-click menu or settings UI)
            settingsService.SaveIsAlwaysOnTop(Topmost);
        }
    }
}