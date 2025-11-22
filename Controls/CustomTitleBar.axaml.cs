using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Globalization;
using System.Runtime.InteropServices;


namespace DemoApp.Controls;



public partial class CustomTitleBar : UserControl
{
    private Window? _parentWindow;
    private TextBlock? _titleText;

    public static readonly StyledProperty<string> TitleTextProperty =
        AvaloniaProperty.Register<CustomTitleBar, string>(nameof(TitleText), "Application");

    public static readonly StyledProperty<string> TimeTextProperty =
        AvaloniaProperty.Register<CustomTitleBar, string>(nameof(TimeText), "");

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

    private void InitializeTitleBar()
    {
        if (_parentWindow == null)
            return;

        _titleText = this.FindControl<TextBlock>("TitleTextBlock");
        
        // Enable window dragging and double-click maximize/restore from the titlebar
        var titleBarGrid = this.FindControl<Grid>("TitleBarGrid");
        if (titleBarGrid != null && _parentWindow != null)
        {
            // Handle double-click to maximize/restore
            titleBarGrid.DoubleTapped += (sender, e) =>
            {
                ToggleMaximize();
                e.Handled = true;
            };
            
            // Handle single-click drag
            titleBarGrid.PointerPressed += (sender, e) =>
            {
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

    private void ToggleMaximize()
    {
        if (_parentWindow == null)
            return;

        _parentWindow.WindowState = _parentWindow.WindowState == WindowState.Maximized 
            ? WindowState.Normal 
            : WindowState.Maximized;
    }

    private void StartWeekTimer()
    {
        // Update every minute is enough for week number, but to be safe we can do it more often or just once if we don't expect it to change (app running for weeks).
        // Let's stick to the previous implementation of a timer.
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        timer.Tick += (sender, e) => UpdateWeek();
        timer.Start();
    }

    private void UpdateWeek()
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
