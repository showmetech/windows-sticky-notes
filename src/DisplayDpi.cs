using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace StickyNotes;
internal static class DisplayDpi {
 // A WPF visual has no PresentationSource before its first Show. Native HWND
 // DPI is already available at SourceInitialized, so never treat pixels as DIP.
 static uint Dpi(Window window){var handle=new WindowInteropHelper(window).Handle;uint value=handle==IntPtr.Zero?0:GetDpiForWindow(handle);if(value==0)value=GetDpiForSystem();return value==0?96:value;}
 internal static DpiScale Scale(Window window){double scale=Dpi(window)/96d;return new DpiScale(scale,scale);}
 internal static Matrix FromDevice(Window window)=>FromDpi(Dpi(window));
 internal static Matrix FromDpi(uint dpi){double scale=96d/(dpi==0?96:dpi);return new Matrix(scale,0,0,scale,0,0);}
 [DllImport("user32.dll")]static extern uint GetDpiForWindow(IntPtr window);
 [DllImport("user32.dll")]static extern uint GetDpiForSystem();
}
