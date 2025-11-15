## Why 
This project enables the creation of custom title bars in Avalonia, designed to retain the native look and feel of the operating system's window controls.

While preserving this native appearance, developers can extend the title bar to add any Avalonia control, such as displaying the date, week number, or embedding custom buttons.

To accomplish this, I developed reusable components for the window icons, adapting the macOS icons from this [GitHub library](https://github.com/lwouis/macos-traffic-light-buttons-as-SVG). 
These components were built to mimic the standard behavior of their native counterparts.

The key benefit is the ability to build title bars that look native but are fully customizable. This opens up possibilities for adding unique functionality or improving accessibility—for example, by implementing larger controls for visually impaired users.

## Issues
As Avalonia UI is not native creating a custom titlebar with icons won't lever the same functionality. For example the maximize press and hold maximize for different window sizes, positions in Mac OS and is a native feature and this won't work for a custom made taskbar.