using Avalonia;
using Avalonia.Controls;
using DemoApp.Controls;
using System;
using System.Runtime.InteropServices;

namespace DemoApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

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

    // Native setup removed as we use standard macOS buttons now.;

    }

