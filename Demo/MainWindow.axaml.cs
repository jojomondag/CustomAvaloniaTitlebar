using Avalonia.Controls;
using Avalonia.FlexibleWindowControls.Controls;

namespace DemoApp;

public partial class MainWindow : Window
{
    private Border? _iconDisplayArea;
    private Border? _windowsIconDisplayArea;
    
    public MainWindow()
    {
        InitializeComponent();

        Opened += (s, e) =>
        {
            _iconDisplayArea = this.FindControl<Border>("IconDisplayArea");
            _windowsIconDisplayArea = this.FindControl<Border>("WindowsIconDisplayArea");

            // Show initial icons based on current platform
            var actualStyle = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.OSX)
                ? PlatformStyle.MacOS
                : PlatformStyle.Windows;
            ShowLargeIcons(actualStyle);
        };

        // When restored from minimized, ensure it activates properly
        PropertyChanged += (s, e) =>
        {
            if (e.Property == WindowStateProperty)
            {
                // Only activate if coming from minimized state and window is not active
                if (WindowState == WindowState.Normal && !IsActive)
                {
                    try
                    {
                        Activate();
                    }
                    catch { }
                }
            }
        };
    }

    private void OnActivateButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            Activate();
        }
        catch { }
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
           if (_iconDisplayArea == null || _windowsIconDisplayArea == null) return;

           // Show the appropriate display area based on style
        _iconDisplayArea.IsVisible = style == PlatformStyle.MacOS;
           _windowsIconDisplayArea.IsVisible = style == PlatformStyle.Windows || style == PlatformStyle.Linux;
    }
}