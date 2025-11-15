using Avalonia.Controls;
using Avalonia.FlexibleWindowControls.Controls;

namespace DemoApp;

public partial class MainWindow : Window
{
    private Border? _iconDisplayArea;
    
    public MainWindow()
    {
        InitializeComponent();
        Loaded += (s, e) =>
        {
            _iconDisplayArea = this.FindControl<Border>("IconDisplayArea");
            
            // Show initial icons based on current platform
            var actualStyle = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.OSX) 
                ? PlatformStyle.MacOS 
                : PlatformStyle.Windows;
            ShowLargeIcons(actualStyle);
        };
    }

    private void OnWindowsButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        TitleBar?.SwitchPlatformStyle(PlatformStyle.Windows);
        ShowLargeIcons(PlatformStyle.Windows);
    }

    private void OnMacOSButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        TitleBar?.SwitchPlatformStyle(PlatformStyle.MacOS);
        ShowLargeIcons(PlatformStyle.MacOS);
    }

    private void OnLinuxButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        TitleBar?.SwitchPlatformStyle(PlatformStyle.Linux);
        ShowLargeIcons(PlatformStyle.Linux);
    }

    private void OnAutoButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        TitleBar?.SwitchPlatformStyle(PlatformStyle.Auto);
        // For Auto, determine the actual platform
        var actualStyle = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.OSX) 
            ? PlatformStyle.MacOS 
            : PlatformStyle.Windows;
        ShowLargeIcons(actualStyle);
    }

    private void ShowLargeIcons(PlatformStyle style)
    {
        if (_iconDisplayArea == null) return;

        // Show the display area only for macOS style
        _iconDisplayArea.IsVisible = style == PlatformStyle.MacOS;
    }
}