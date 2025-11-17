using Avalonia.Controls;
using Avalonia.Input;
using D2.UI.ViewModels;

namespace D2.UI.Views;

public partial class MainMenuView : UserControl
{
    public MainMenuView()
    {
        InitializeComponent();
        Focusable = true;
        KeyDown += OnKeyDown;
        AttachedToVisualTree += (s, e) => Focus();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not MainMenuViewModel viewModel)
            return;

        switch (e.Key)
        {
            case Key.D1:
            case Key.NumPad1:
                viewModel.SelectByNumber(1);
                e.Handled = true;
                break;
            case Key.D2:
            case Key.NumPad2:
                viewModel.SelectByNumber(2);
                e.Handled = true;
                break;
            case Key.D3:
            case Key.NumPad3:
                viewModel.SelectByNumber(3);
                e.Handled = true;
                break;
            case Key.D4:
            case Key.NumPad4:
                viewModel.SelectByNumber(4);
                e.Handled = true;
                break;
            case Key.D5:
            case Key.NumPad5:
                viewModel.SelectByNumber(5);
                e.Handled = true;
                break;
            case Key.Up:
                viewModel.MoveSelectionUp();
                e.Handled = true;
                break;
            case Key.Down:
                viewModel.MoveSelectionDown();
                e.Handled = true;
                break;
            case Key.Enter:
                viewModel.ExecuteSelected();
                e.Handled = true;
                break;
        }
    }
}