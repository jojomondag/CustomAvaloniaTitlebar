using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System;
using System.Diagnostics;
using SvgControl = Avalonia.Svg.Skia.Svg;

namespace Avalonia.FlexibleWindowControls.Controls;

public partial class WindowsWindowButtons : UserControl
{
    private Window? _parentWindow;
    private SvgControl? _closeIcon;
    private SvgControl? _minimizeIcon;
    private SvgControl? _maximizeIcon;
    private StackPanel? _buttonsStack;
    private Button? _closeButton;
    private Button? _minimizeButton;
    private Button? _maximizeButton;

    public static readonly StyledProperty<double> IconSizeProperty =
        AvaloniaProperty.Register<WindowsWindowButtons, double>(nameof(IconSize), 32.0);

    public static readonly StyledProperty<bool> ShowIconsOnHoverProperty =
        AvaloniaProperty.Register<WindowsWindowButtons, bool>(nameof(ShowIconsOnHover), false);

    public double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public bool ShowIconsOnHover
    {
        get => GetValue(ShowIconsOnHoverProperty);
        set => SetValue(ShowIconsOnHoverProperty, value);
    }

    public WindowsWindowButtons()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _parentWindow = this.FindAncestorOfType<Window>();
        
        // Get references to controls
        _buttonsStack = this.FindControl<StackPanel>("WindowsButtonsStack");
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
                Debug.WriteLine("Windows maximize button clicked!");
                ToggleMaximize();
            };
        }

        // Wire up hover events for each button
        if (_minimizeButton != null)
        {
            _minimizeButton.PointerEntered += OnMinimizePointerEntered;
            _minimizeButton.PointerExited += OnMinimizePointerExited;
        }
        
        if (_maximizeButton != null)
        {
            _maximizeButton.PointerEntered += OnMaximizePointerEntered;
            _maximizeButton.PointerExited += OnMaximizePointerExited;
        }
        
        if (_closeButton != null)
        {
            _closeButton.PointerEntered += OnClosePointerEntered;
            _closeButton.PointerExited += OnClosePointerExited;
        }
        
        // If ShowIconsOnHover is false, immediately show the hover icons
        if (!ShowIconsOnHover)
        {
            ShowAllHoverIcons();
        }
        
        // Subscribe to window state changes for tooltip and icon updates
        if (_parentWindow != null)
        {
            _parentWindow.PropertyChanged += OnWindowStateChanged;
            UpdateMaximizeState();
            UpdateMaximizeIcon();
        }
    }

    private void OnMinimizePointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        // No action needed - background and foreground changes are handled by XAML styles
    }

    private void OnMinimizePointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        // No action needed - background and foreground changes are handled by XAML styles
    }

    private void OnMaximizePointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        // No action needed - background and foreground changes are handled by XAML styles
    }

    private void OnMaximizePointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        // No action needed - background and foreground changes are handled by XAML styles
    }

    private void OnClosePointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        // No action needed - background and foreground changes are handled by XAML styles
    }

    private void OnClosePointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        // No action needed - background and foreground changes are handled by XAML styles
    }

    private void ShowAllHoverIcons()
    {
        // Icons are always visible with normal appearance
        // Visual changes are handled by XAML styles on hover
        UpdateMaximizeState();
    }

    private void ToggleMaximize()
    {
        if (_parentWindow == null)
            return;

        _parentWindow.WindowState = _parentWindow.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void OnWindowStateChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Window.WindowStateProperty)
        {
            UpdateMaximizeState();
            UpdateMaximizeIcon();
        }
    }

    private void UpdateMaximizeState()
    {
        if (_parentWindow == null || _maximizeButton == null)
            return;

        bool isMaximized = _parentWindow.WindowState == WindowState.Maximized;

        // Update tooltip
        ToolTip.SetTip(_maximizeButton, isMaximized ? "Restore" : "Maximize");
    }

    private void UpdateMaximizeIcon()
    {
        if (_parentWindow == null || _maximizeIcon == null)
            return;

        bool isMaximized = _parentWindow.WindowState == WindowState.Maximized;
        _maximizeIcon.Path = isMaximized
            ? "avares://Avalonia.FlexibleWindowControls/Icons/WinOS/restore-normal.svg"
            : "avares://Avalonia.FlexibleWindowControls/Icons/WinOS/maximize-normal.svg";
    }

    /// <summary>
    /// Sets the hover behavior for showing icons
    /// </summary>
    /// <param name="enable">True to show icons on hover, false to always show icons</param>
    public void SetHoverBehavior(bool enable)
    {
        ShowIconsOnHover = enable;
        ShowAllHoverIcons();
    }
}
