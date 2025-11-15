using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using SvgControl = Avalonia.Svg.Skia.Svg;

namespace Avalonia.FlexibleWindowControls.Controls;

public partial class MacOSWindowButtons : UserControl
{
    private Window? _parentWindow;
    private SvgControl? _closeIcon;
    private SvgControl? _minimizeIcon;
    private SvgControl? _maximizeIcon;
    private StackPanel? _buttonsStack;
    private Button? _closeButton;
    private Button? _minimizeButton;
    private Button? _maximizeButton;

    public static readonly StyledProperty<double> ButtonSizeProperty =
        AvaloniaProperty.Register<MacOSWindowButtons, double>(nameof(ButtonSize), 12.0);

    public static readonly StyledProperty<double> ButtonSpacingProperty =
        AvaloniaProperty.Register<MacOSWindowButtons, double>(nameof(ButtonSpacing), 8.0);

    public static readonly StyledProperty<bool> ShowIconsOnHoverProperty =
        AvaloniaProperty.Register<MacOSWindowButtons, bool>(nameof(ShowIconsOnHover), true);

    public double ButtonSize
    {
        get => GetValue(ButtonSizeProperty);
        set => SetValue(ButtonSizeProperty, value);
    }

    public double ButtonSpacing
    {
        get => GetValue(ButtonSpacingProperty);
        set => SetValue(ButtonSpacingProperty, value);
    }

    public bool ShowIconsOnHover
    {
        get => GetValue(ShowIconsOnHoverProperty);
        set => SetValue(ShowIconsOnHoverProperty, value);
    }

    public MacOSWindowButtons()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _parentWindow = this.FindAncestorOfType<Window>();
        
        // Get references to controls
        _buttonsStack = this.FindControl<StackPanel>("MacOSButtonsStack");
        _closeButton = this.FindControl<Button>("CloseButton");
        _minimizeButton = this.FindControl<Button>("MinimizeButton");
        _maximizeButton = this.FindControl<Button>("MaximizeButton");
        _closeIcon = this.FindControl<SvgControl>("CloseIcon");
        _minimizeIcon = this.FindControl<SvgControl>("MinimizeIcon");
        _maximizeIcon = this.FindControl<SvgControl>("MaximizeIcon");

        WireUpControls();
    }

    private void WireUpControls()
    {
        if (_parentWindow == null)
            return;

        // Wire up button clicks
        if (_closeButton != null)
        {
            _closeButton.Click += (sender, e) => _parentWindow?.Close();
        }
        if (_minimizeButton != null)
        {
            _minimizeButton.Click += (sender, e) => _parentWindow.WindowState = WindowState.Minimized;
        }
        if (_maximizeButton != null)
        {
            _maximizeButton.Click += (sender, e) =>
            {
                Debug.WriteLine("Maximize button clicked!");
                ToggleMaximize();
            };
        }
        else
        {
            Debug.WriteLine("WARNING: _maximizeButton is null! Button not wired up.");
        }

        // Wire up hover events - both modes use stack hover, but ShowIconsOnHover=false starts with icons visible
        if (_buttonsStack != null)
        {
            _buttonsStack.PointerEntered += OnPointerEntered;
            _buttonsStack.PointerExited += OnPointerExited;
            
            if (!ShowIconsOnHover)
            {
                // For always-on mode: show icons immediately
                OnPointerEntered(null, null!);
            }
        }
        
        // Subscribe to window state changes for tooltip and icon updates
        if (_parentWindow != null)
        {
            _parentWindow.PropertyChanged += OnWindowStateChanged;
            UpdateMaximizeState();
        }
    }

    private void OnPointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (_closeIcon != null)
            _closeIcon.Path = "avares://Avalonia.FlexibleWindowControls/Icons/MacOS/close-hover.svg";
        if (_minimizeIcon != null)
            _minimizeIcon.Path = "avares://Avalonia.FlexibleWindowControls/Icons/MacOS/minimize-hover.svg";
        if (_maximizeIcon != null)
        {
            bool isMaximized = _parentWindow?.WindowState == WindowState.Maximized;
            _maximizeIcon.Path = isMaximized
                ? "avares://Avalonia.FlexibleWindowControls/Icons/MacOS/maximize-contract-hover.svg"
                : "avares://Avalonia.FlexibleWindowControls/Icons/MacOS/maximize-expand-hover.svg";
        }
    }

    private void OnPointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (_closeIcon != null)
            _closeIcon.Path = "avares://Avalonia.FlexibleWindowControls/Icons/MacOS/close-normal.svg";
        if (_minimizeIcon != null)
            _minimizeIcon.Path = "avares://Avalonia.FlexibleWindowControls/Icons/MacOS/minimize-normal.svg";
        if (_maximizeIcon != null)
        {
            // Always use maximize-normal.svg (green circle) when not hovering, regardless of window state
            _maximizeIcon.Path = "avares://Avalonia.FlexibleWindowControls/Icons/MacOS/maximize-normal.svg";
        }
    }

    private void ToggleMaximize()
    {
        if (_parentWindow == null)
            return;

        Debug.WriteLine($"ToggleMaximize called. Platform: {RuntimeInformation.OSDescription}, IsOSX: {RuntimeInformation.IsOSPlatform(OSPlatform.OSX)}");

        // On macOS, use window tiling instead of standard maximize
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Debug.WriteLine("Calling TileWindowLeftOnMac()");
            TileWindowLeftOnMac();
        }
        else
        {
            // On other platforms, use standard maximize behavior
            Debug.WriteLine("Using standard maximize behavior (not macOS)");
            _parentWindow.WindowState = _parentWindow.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
    }

    /// <summary>
    /// Invokes the macOS Sequoia window tiling feature to move the frontmost window.
    /// </summary>
    private void TileWindowLeftOnMac()
    {
        Debug.WriteLine("TileWindowLeftOnMac() called");
        
        // 1. Check if we are actually running on macOS
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Not on a Mac, so just do nothing.
            Debug.WriteLine("Not running on macOS, returning early");
            return;
        }

        Debug.WriteLine("Running on macOS, proceeding with AppleScript");

        // 2. Define the AppleScript command
        // You can change "Left" to "Right", "Top Left Quarter", "Bottom Right Quarter", etc.
        string appleScript = @"
        tell application ""System Events""
            tell (first process whose frontmost is true)
                tell menu bar 1
                    tell menu bar item ""Window""
                        tell menu ""Window""
                            tell menu item ""Move & Resize""
                                tell menu ""Move & Resize""
                                    click menu item ""Left""
                                end tell
                            end tell
                        end tell
                    end tell
                end tell
            end tell
        end tell";

        Debug.WriteLine($"AppleScript prepared: {appleScript}");

        // 3. Set up the process to run "osascript"
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/usr/bin/osascript",
                RedirectStandardInput = true, // We will pipe the script in
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        // 4. Start the process, write the script to it, and wait for it to exit
        try
        {
            Debug.WriteLine("Starting osascript process...");
            process.Start();
            Debug.WriteLine("Process started, writing AppleScript to StandardInput...");
            process.StandardInput.Write(appleScript);
            process.StandardInput.Close(); // Signal that we're done writing
            Debug.WriteLine("Waiting for process to exit...");
            process.WaitForExit();
            Debug.WriteLine($"Process exited with code: {process.ExitCode}");
        }
        catch (Exception ex)
        {
            // Log or handle the error (e.g., osascript not found)
            Debug.WriteLine($"Error executing AppleScript: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        finally
        {
            process?.Dispose();
        }
    }

    private void OnWindowStateChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Window.WindowStateProperty)
        {
            UpdateMaximizeState();
        }
    }

    private void UpdateMaximizeState()
    {
        if (_parentWindow == null || _maximizeButton == null || _maximizeIcon == null)
            return;

        bool isMaximized = _parentWindow.WindowState == WindowState.Maximized;
        
        // Update tooltip
        ToolTip.SetTip(_maximizeButton, isMaximized ? "Restore" : "Maximize");
        
        // Update icon - always use maximize-normal.svg for non-hover state
        // Hover state changes between expand and contract based on window state
        if (ShowIconsOnHover)
        {
            // For hover mode, always show maximize-normal.svg (green circle) when not hovering
            _maximizeIcon.Path = "avares://Avalonia.FlexibleWindowControls/Icons/MacOS/maximize-normal.svg";
        }
        else
        {
            // For always-on mode, update the hover state icon
            _maximizeIcon.Path = isMaximized 
                ? "avares://Avalonia.FlexibleWindowControls/Icons/MacOS/maximize-contract-hover.svg"
                : "avares://Avalonia.FlexibleWindowControls/Icons/MacOS/maximize-expand-hover.svg";
        }
    }

    /// <summary>
    /// Sets the hover behavior for showing icons
    /// </summary>
    /// <param name="enable">True to show icons on hover, false to always show icons</param>
    public void SetHoverBehavior(bool enable)
    {
        ShowIconsOnHover = enable;
        
        if (_buttonsStack != null)
        {
            if (enable)
            {
                _buttonsStack.PointerEntered += OnPointerEntered;
                _buttonsStack.PointerExited += OnPointerExited;
            }
            else
            {
                _buttonsStack.PointerEntered -= OnPointerEntered;
                _buttonsStack.PointerExited -= OnPointerExited;
                // Show icons always
                OnPointerEntered(null, null!);
            }
        }
    }
}
