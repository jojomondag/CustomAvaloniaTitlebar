using Avalonia;
using Avalonia.Controls;
using Avalonia.FlexibleWindowControls.Controls;
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

    private void SetupMacOSNativeButtons()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return;

        // We are using standard native buttons now.
        // Avalonia's ExtendClientAreaToDecorationsHint="True" usually keeps them visible but in the titlebar area.
        // We just need to ensure we don't hide them.
        
        // No P/Invoke needed to hide them anymore.
    }

    protected override void OnOpened(System.EventArgs e)
    {
        base.OnOpened(e);
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            SetupMacOSNativeButtons();
        }

        // Hook into WndProc for Windows Snap Layout support
        if (OperatingSystem.IsWindows())
        {
            var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(this);
            var platformImpl = topLevel?.PlatformImpl;
            
            var handle = this.TryGetPlatformHandle();
            if (handle != null)
            {
                _hwnd = handle.Handle;
                _newWndProcDelegate = new WndProcDelegate(CustomWndProc);
                _oldWndProc = SetWindowLongPtr(_hwnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newWndProcDelegate));
            }
        }
    }
    
    // Windows P/Invoke for Snap Layouts

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

internal static class MacOSNativeInterop
{
    private const string ObjCLibrary = "/usr/lib/libobjc.dylib";
    private const string AppKitLibrary = "/System/Library/Frameworks/AppKit.framework/AppKit";

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    public static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    public static extern IntPtr IntPtr_objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    public static extern void void_objc_msgSend_Double(IntPtr receiver, IntPtr selector, double arg1);

    // For structs larger than 16 bytes (like CGRect on some archs) or specific ABIs, we might need st_ret.
    // On ARM64 (Apple Silicon), small structs like CGRect are returned in registers, so standard P/Invoke works.
    // On x64, it might be different.
    // However, for this demo, we assume standard behavior or ARM64.
    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    public static extern CGRect CGRect_objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "sel_registerName")]
    public static extern IntPtr GetSelector(string name);

    public enum NSWindowButton
    {
        CloseButton = 0,
        MiniaturizeButton = 1,
        ZoomButton = 2
    }

    public static IntPtr GetStandardWindowButton(IntPtr nsWindow, NSWindowButton button)
    {
        var sel = GetSelector("standardWindowButton:");
        return IntPtr_objc_msgSend_IntPtr(nsWindow, sel, (IntPtr)button);
    }

    public static void SetAlphaValue(IntPtr nsView, double alpha)
    {
        var sel = GetSelector("setAlphaValue:");
        void_objc_msgSend_Double(nsView, sel, alpha);
    }

    public static IntPtr GetSuperview(IntPtr nsView)
    {
        var sel = GetSelector("superview");
        return IntPtr_objc_msgSend(nsView, sel);
    }

    public static CGRect GetFrame(IntPtr nsView)
    {
        var sel = GetSelector("frame");
        return CGRect_objc_msgSend(nsView, sel);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CGRect
    {
        public double X, Y, Width, Height;
    }
}