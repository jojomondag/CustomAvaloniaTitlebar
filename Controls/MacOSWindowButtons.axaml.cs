using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Input;
using Avalonia.Threading;
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
            // Native buttons handle interaction now
            // _maximizeButton.AddHandler(InputElement.PointerPressedEvent, OnMaximizePointerPressed, RoutingStrategies.Tunnel);
            // _maximizeButton.AddHandler(InputElement.PointerReleasedEvent, OnMaximizePointerReleased, RoutingStrategies.Tunnel);
            // _maximizeButton.PointerEntered += OnMaximizePointerEntered;
            // _maximizeButton.PointerExited += OnMaximizePointerExited;
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

        // Use standard maximize behavior on all platforms
        Debug.WriteLine("Using standard maximize behavior");
        _parentWindow.WindowState = _parentWindow.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
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
    /// <summary>
    /// Gets the screen coordinates of the maximize button if visible
    /// </summary>
    public Rect? GetMaximizeButtonBounds()
    {
        if (_maximizeButton == null || !_maximizeButton.IsVisible)
            return null;

        var topLeft = _maximizeButton.PointToScreen(new Point(0, 0));
        var bottomRight = _maximizeButton.PointToScreen(new Point(_maximizeButton.Bounds.Width, _maximizeButton.Bounds.Height));
        
        // Convert pixel coordinates to logical coordinates if needed, but PointToScreen returns pixels which is what we want for Windows API
        // However, we need to return a Rect in screen coordinates
        
        return new Rect(
            new Point(topLeft.X, topLeft.Y),
            new Point(bottomRight.X, bottomRight.Y));
    }

    /// <summary>
    /// Manually sets the hover state of the maximize button.
    /// This is used when the window is handling WM_NCHITTEST and returning HTMAXBUTTON,
    /// which bypasses standard Avalonia pointer events.
    /// </summary>
    public void SetMaximizeHover(bool hover)
    {
        if (_maximizeIcon == null) return;

        if (hover)
        {
            bool isMaximized = _parentWindow?.WindowState == WindowState.Maximized;
            _maximizeIcon.Path = isMaximized
                ? "avares://Avalonia.FlexibleWindowControls/Icons/MacOS/maximize-contract-hover.svg"
                : "avares://Avalonia.FlexibleWindowControls/Icons/MacOS/maximize-expand-hover.svg";
        }
        else
        {
            // Reset to normal state
            _maximizeIcon.Path = "avares://Avalonia.FlexibleWindowControls/Icons/MacOS/maximize-normal.svg";
        }
    }



    // AlignToNativeButtons removed as we are using native buttons directly.
}
