using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SvgControl = Avalonia.Svg.Skia.Svg;

namespace Avalonia.FlexibleWindowControls.Controls;

public enum PlatformStyle
{
    Auto,
    Windows,
    MacOS,
    Linux
}

public partial class CustomTitleBar : UserControl
{
    private Window? _parentWindow;
    private DispatcherTimer? _weekTimer;
    private TextBlock? _titleText;
    private TextBlock? _titleTime;
    private MacOSWindowButtons? _macOSControls;
    private WindowsWindowButtons? _windowsControls;
    private bool _isMacOS;
    private PlatformStyle _currentPlatformStyle = PlatformStyle.Auto;

    public static readonly StyledProperty<string> TitleTextProperty =
        AvaloniaProperty.Register<CustomTitleBar, string>(nameof(TitleText), "Application");

    public static readonly StyledProperty<string> TimeTextProperty =
        AvaloniaProperty.Register<CustomTitleBar, string>(nameof(TimeText), "");

    public static readonly StyledProperty<PlatformStyle> PlatformStyleProperty =
        AvaloniaProperty.Register<CustomTitleBar, PlatformStyle>(nameof(PlatformStyle), PlatformStyle.Auto);

    public string TitleText
    {
        get => GetValue(TitleTextProperty);
        set => SetValue(TitleTextProperty, value);
    }

    public string TimeText
    {
        get => GetValue(TimeTextProperty);
        set => SetValue(TimeTextProperty, value);
    }

    public PlatformStyle PlatformStyle
    {
        get => GetValue(PlatformStyleProperty);
        set
        {
            SetValue(PlatformStyleProperty, value);
            _currentPlatformStyle = value;
            ApplyPlatformStyle();
        }
    }

    public CustomTitleBar()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _parentWindow = this.FindAncestorOfType<Window>();
        if (_parentWindow == null)
            return;

        InitializeTitleBar();
    }

    /// <summary>
    /// Initializes the custom title bar with dragging capability and week display
    /// </summary>
    private void InitializeTitleBar()
    {
        if (_parentWindow == null)
            return;

        // Get references to titlebar elements
        _titleText = this.FindControl<TextBlock>("TitleTextBlock");
        _titleTime = this.FindControl<TextBlock>("TitleTime");
        
        // Get platform-specific controls
        _macOSControls = this.FindControl<MacOSWindowButtons>("MacOSControls");
            _windowsControls = this.FindControl<WindowsWindowButtons>("WindowsControls");
        
        // Determine platform style
        if (_currentPlatformStyle == PlatformStyle.Auto)
        {
            _isMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        }
        else
        {
            _isMacOS = _currentPlatformStyle == PlatformStyle.MacOS;
        }
        
        // Apply the initial platform style
        ApplyPlatformStyle();
        
        // Enable window dragging and double-click maximize/restore from the titlebar
        var titleBarGrid = this.FindControl<Grid>("TitleBarGrid");
        if (titleBarGrid != null && _parentWindow != null)
        {
            // Handle double-click to maximize/restore
            titleBarGrid.DoubleTapped += (sender, e) =>
            {
                // Don't handle if clicking on a button
                if (e.Source is Button)
                    return;
                
                ToggleMaximize();
                e.Handled = true;
            };
            
            // Handle single-click drag
            titleBarGrid.PointerPressed += (sender, e) =>
            {
                // Don't drag if clicking on a button
                if (e.Source is Button)
                    return;
                    
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                {
                    _parentWindow.BeginMoveDrag(e);
                }
            };
        }
        
        // Initialize and start the week display
        UpdateWeek();
        StartWeekTimer();
    }

    /// <summary>
    /// Toggles between maximized and normal window state
    /// </summary>
    private void ToggleMaximize()
    {
        if (_parentWindow == null)
            return;

        _parentWindow.WindowState = _parentWindow.WindowState == WindowState.Maximized 
            ? WindowState.Normal 
            : WindowState.Maximized;
    }

    /// <summary>
    /// Starts a timer to update the week display every second
    /// </summary>
    private void StartWeekTimer()
    {
        _weekTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _weekTimer.Tick += (sender, e) => UpdateWeek();
        _weekTimer.Start();
    }

    /// <summary>
    /// Updates the week number display in the titlebar
    /// </summary>
    private void UpdateWeek()
    {
        if (_titleTime != null)
        {
            var calendar = CultureInfo.CurrentCulture.Calendar;
            var weekNumber = calendar.GetWeekOfYear(
                DateTime.Now,
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday
            );
            TimeText = $"Week {weekNumber}";
        }
    }


    /// <summary>
    /// Applies the current platform style to show/hide appropriate controls
    /// </summary>
    private void ApplyPlatformStyle()
    {
        if (_macOSControls == null || _windowsControls == null)
            return;

        if (_isMacOS)
        {
            _macOSControls.IsVisible = true;
            _windowsControls.IsVisible = false;
        }
        else
        {
            _macOSControls.IsVisible = false;
            _windowsControls.IsVisible = true;
        }
    }

    /// <summary>
    /// Switches the titlebar to a specific platform style
    /// </summary>
    /// <param name="style">The platform style to switch to</param>
    public void SwitchPlatformStyle(PlatformStyle style)
    {
        _currentPlatformStyle = style;
        
        // Determine if macOS style
        if (style == PlatformStyle.Auto)
        {
            _isMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        }
        else
        {
            // macOS uses macOS style, Windows and Linux both use Windows/Linux style
            _isMacOS = style == PlatformStyle.MacOS;
        }
        
        ApplyPlatformStyle();
    }
}

