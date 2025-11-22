using Avalonia.Controls;
using Avalonia.FlexibleWindowControls.Controls;
using System;
using System.Runtime.InteropServices;
using Avalonia;

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

    protected override void OnOpened(System.EventArgs e)
    {
        base.OnOpened(e);

        // Hook into WndProc for Windows Snap Layout support
        if (OperatingSystem.IsWindows())
        {
            var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(this);
            var platformImpl = topLevel?.PlatformImpl;
            
            // We need to access the underlying window handle and WndProc
            // Avalonia doesn't expose WndProc directly in a cross-platform way, 
            // but we can use reflection or specific platform interfaces if available.
            // However, for a cleaner approach with standard Avalonia, we might need to use P/Invoke 
            // to subclass the window or use a library that exposes this.
            // 
            // Fortunately, Avalonia.Win32 exposes a way to set a WndProc callback if we cast to the right type,
            // but that requires adding a dependency on Avalonia.Win32 which might not be desirable if we want to keep it clean.
            //
            // A common workaround in Avalonia for this specific "Snap Layout" feature without adding heavy dependencies
            // is to use the standard Win32 SetWindowLongPtr to hook the WndProc.
            
            var handle = this.TryGetPlatformHandle();
            if (handle != null)
            {
                _hwnd = handle.Handle;
                _newWndProcDelegate = new WndProcDelegate(CustomWndProc);
                _oldWndProc = SetWindowLongPtr(_hwnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newWndProcDelegate));
            }
        }
    }

    private IntPtr _hwnd;
    private IntPtr _oldWndProc;
    private WndProcDelegate? _newWndProcDelegate;

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (IntPtr.Size == 8)
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        else
            return SetWindowLong32(hWnd, nIndex, dwNewLong);
    }

    private const int GWLP_WNDPROC = -4;
    private const uint WM_NCHITTEST = 0x0084;
    private const uint WM_NCLBUTTONDOWN = 0x00A1;
    private const uint WM_NCLBUTTONUP = 0x00A2;
    private const uint WM_NCMOUSELEAVE = 0x02A2;
    private const int HTMAXBUTTON = 9;
    private const int HTCLIENT = 1;

    private bool _wasOverMaximize = false;

    private IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_NCHITTEST)
        {
            // Get mouse coordinates from lParam
            int x = (short)(lParam.ToInt64() & 0xFFFF);
            int y = (short)((lParam.ToInt64() >> 16) & 0xFFFF);

            // Check if mouse is over our custom maximize button
            if (TitleBar != null)
            {
                var buttonBounds = TitleBar.GetMaximizeButtonBounds();
                if (buttonBounds.HasValue)
                {
                    if (buttonBounds.Value.Contains(new Point(x, y)))
                    {
                        if (!_wasOverMaximize)
                        {
                            TitleBar.SetMaximizeHover(true);
                            _wasOverMaximize = true;
                        }
                        return (IntPtr)HTMAXBUTTON;
                    }
                }
            }
            
            // If we were over maximize but now we are not (and we didn't return above), reset hover
            if (_wasOverMaximize)
            {
                TitleBar?.SetMaximizeHover(false);
                _wasOverMaximize = false;
            }
        }
        else if (msg == WM_NCMOUSELEAVE)
        {
            if (_wasOverMaximize)
            {
                TitleBar?.SetMaximizeHover(false);
                _wasOverMaximize = false;
            }
        }
        else if (msg == WM_NCLBUTTONDOWN)
        {
            // If we clicked the max button, we want to handle the up event to toggle state
            if (wParam.ToInt64() == HTMAXBUTTON)
            {
                // Return 0 to prevent default processing but ensure we get the UP message
                // Actually, for standard behavior we might want to let it pass, but since the user reported issues,
                // we will consume it to ensure we control the flow.
                return IntPtr.Zero;
            }
        }
        else if (msg == WM_NCLBUTTONUP)
        {
            // Handle the click on the maximize button
            if (wParam.ToInt64() == HTMAXBUTTON)
            {
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                }
                else
                {
                    WindowState = WindowState.Maximized;
                }
                return IntPtr.Zero;
            }
        }

        return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
    }
}