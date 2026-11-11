using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows.Threading;

namespace StickyNotes;
internal sealed class LiveVerification {
 readonly NotesApp app;readonly string report;readonly List<object> checks=new();readonly DispatcherTimer timer=new(){Interval=TimeSpan.FromMilliseconds(600)};int phase;int failed;
 LiveVerification(NotesApp app,string report){this.app=app;this.report=report;timer.Tick+=Tick;timer.Start();}
 public static void Start(NotesApp app,string report)=>_=new LiveVerification(app,report);
 void Check(string name,bool ok){checks.Add(new{name,passed=ok});if(!ok)failed++;}
 void Tick(object? sender,EventArgs e){try{switch(phase++){
  case 0:
   app.ShowNotes();Check("Native tray created",app.TrayVisible);Check("Exactly three fixture notes",app.NoteWindows.Count==3);break;
  case 1:
   var windows=app.NoteWindows.Values.ToList();Check("All notes visible",windows.All(w=>w.IsVisible));Check("Native windows absent from taskbar",windows.All(w=>!w.ShowInTaskbar&&(GetWindowLongPtr(new WindowInteropHelper(w).Handle,-20).ToInt64()&0x40000)==0));
   Check("Positions restored without collapsing into cascade",windows.Select(w=>(w.Left,w.Top)).Distinct().Count()==3);
   foreach(var w in windows)Check("Note inside monitor work area: "+w.Note.Title,NoteWindow.WorkArea(w).Contains(new Rect(w.Left,w.Top,w.Width,w.Height)));
   Check("Exactly six bottom actions stay aligned",windows.All(w=>w.BottomButtonsAligned()));Check("Four top actions show minimize with no legacy menu or passive labels",windows.All(w=>w.CompactChromeVisible()));var narrow=windows[0];double previousWidth=narrow.Width;narrow.Width=300;Check("Bottom actions fit and align at minimum note width",narrow.BottomButtonsAligned());Check("Four top actions remain visible at minimum note width",narrow.CompactChromeVisible());narrow.Width=previousWidth;
   Check("All four edges and four corners receive pointer hits with resize cursors",windows.All(w=>w.ResizeEdgesAvailable()));VerifyEdgeResizing(windows[0]);
   var moved=windows[0];var area=NoteWindow.WorkArea(moved);double x=area.Left+93,y=area.Top+117;moved.Left=x;moved.Top=y;moved.Save();moved.FitToScreen();Check("Free placement is not snapped or reset",Math.Abs(moved.Left-x)<1&&Math.Abs(moved.Top-y)<1);moved.Save();var restored=new Store(app.DataRoot).Book.Notes.Single(n=>n.Id==moved.Note.Id);Check("Moved position survives reload",Math.Abs(restored.Left-x)<1&&Math.Abs(restored.Top-y)<1);
   app.HideNotes();break;
  case 2:Check("Hide animation finishes and keeps process alive",app.NoteWindows.Values.All(w=>!w.IsVisible));app.HandleTrayClick();break;
  case 3:Check("Shared left-click handler restores saved notes",app.NoteWindows.Values.All(w=>w.IsVisible));app.Arrange();break;
  case 4:
   Check("Grid arrangement keeps notes on screen",app.NoteWindows.Values.All(w=>NoteWindow.WorkArea(w).Contains(new Rect(w.Left,w.Top,w.Width,w.Height))));
   app.Widget.Present();break;
  case 5:
   Check("Suite widget header aligns settings, pin and close",SuiteChrome.Aligned(app.Widget));
   Check("Suite widget stays off taskbar",!app.Widget.ShowInTaskbar);
   app.Widget.OpenSettings();break;
  case 6:
   Check("Settings panel opens above widget with matching width and 12 DIP gap",app.Widget.SettingsAligned);
   Check("Settings panel stays off taskbar",app.Widget.SettingsPanel?.ShowInTaskbar==false);
   app.Store.Book.Theme="Escuro";app.ApplyTheme();Check("Theme switch retains settings panel and header alignment",app.Widget.SettingsVisible&&app.Widget.SettingsAligned);
   Check("Outside focus policy protects pinned widgets, menus and own windows",SuiteActivation.VerifyFocusPolicy());
   Check("Native tray supports mouse and keyboard activation",NativeTray.CallbackAction(0x400,true)==1&&NativeTray.CallbackAction(0x401,true)==1&&NativeTray.CallbackAction(0x7b,true)==2);
   CaptureSuitePreview();
   app.Widget.CloseSettings();break;
  case 7:
   Check("Closing settings restores compact widget without closing notes",!app.Widget.SettingsVisible&&app.Widget.IsVisible&&app.NoteWindows.Values.All(w=>w.IsVisible));app.Widget.Conceal();break;
  case 8:
   Check("Recollecting widget preserves visible desktop notes",!app.Widget.IsVisible&&app.NoteWindows.Values.All(w=>w.IsVisible));app.HandleTrayClick();break;
  case 9:
   Check("Tray handler opens widget and saved notes together",app.Widget.IsVisible&&app.NoteWindows.Values.All(w=>w.IsVisible));
   var tiles=app.Widget.NoteTiles;
   Check("Widget has one compact single-line button per note with exact saved color and readable title",tiles.Length==3&&tiles.All(tile=>{var n=app.Store.Book.Notes.Single(n=>n.Id==(string)tile.Tag);return tile.Content is TextBlock text&&text.Text==n.Title&&text.TextWrapping==TextWrapping.NoWrap&&text.TextTrimming==TextTrimming.CharacterEllipsis&&tile.Height==46&&(SystemParameters.HighContrast||tile.Background is SolidColorBrush brush&&brush.Color==(Color)ColorConverter.ConvertFromString(n.Color)&&tile.Foreground is SolidColorBrush ink&&NoteColors.Contrast(brush.Color,ink.Color)>=4.5);}));
   app.HideNotes();break;
  case 10:
   Check("Notes concealed before tile activation",app.NoteWindows.Values.All(w=>!w.IsVisible));
   app.Widget.NoteTiles[0].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));break;
  case 11:
   var selected=app.NoteWindows[(string)app.Widget.NoteTiles[0].Tag];
   Check("Clicking colored tile opens only that note and highlights its border",selected.IsVisible&&selected.HighlightVisible&&app.NoteWindows.Values.Where(w=>w!=selected).All(w=>!w.IsVisible&&!w.HighlightVisible));
   app.Widget.NoteTiles[0].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));break;
  case 12:Check("Repeated tile click restarts border highlight",app.NoteWindows.Values.Count(w=>w.HighlightVisible)==1);break;
  case 13:case 14:case 15:break;
  case 16:
   Check("Highlight clears automatically while selected note stays visible",app.NoteWindows.Values.All(w=>!w.HighlightVisible)&&app.NoteWindows.Values.Count(w=>w.IsVisible)==1);
   Check("Flush succeeds",app.FlushAll());timer.Stop();Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(report))!);CapturePreview();File.WriteAllText(report,JsonSerializer.Serialize(new{at=DateTimeOffset.Now,failed,checks,notes=app.NoteWindows.Values.Select(w=>new{w.Note.Title,w.Left,w.Top,w.Width,w.Height,w.ShowInTaskbar}),limitations="Tray restoration invoked through the same handler as the native left click; no physical tray click was automated. Physical monitor hot-unplug, ARM64 hardware and OS reboot were not tested. Preview PNGs render the real WPF window tree with fixture notes, not a screenshot of the desktop."},new JsonSerializerOptions{WriteIndented=true}));app.ExitApp();break;
 }}catch(Exception ex){timer.Stop();File.WriteAllText(report,ex.ToString());app.ExitApp();}}
 void CapturePreview(){var examples=app.NoteWindows.Values.ToArray();for(int i=0;i<examples.Length;i++){examples[i].Note.ColorMode=i==0?"Vivid":"Neon";examples[i].Note.Color=i==0?"#8338EC":i==1?"#39FF14":"#FF00D4";}foreach(string theme in new[]{"Claro","Escuro"}){app.Store.Book.Theme=theme;app.ApplyTheme();app.FlushAll();var visual=new System.Windows.Media.DrawingVisual();using(var dc=visual.RenderOpen()){dc.DrawRectangle(Visuals.Brush(theme=="Claro"?"#E9EDF3":"#191C23"),null,new Rect(0,0,1100,410));int index=0;foreach(var w in app.NoteWindows.Values){w.UpdateLayout();dc.DrawRectangle(new System.Windows.Media.VisualBrush(w){Stretch=System.Windows.Media.Stretch.Fill},null,new Rect(18+index*356,20,w.Width,w.Height));index++;}}var bitmap=new System.Windows.Media.Imaging.RenderTargetBitmap(1650,615,144,144,System.Windows.Media.PixelFormats.Pbgra32);bitmap.Render(visual);var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));using var file=File.Create(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(report))!,theme=="Claro"?"preview-claro.png":"preview-escuro.png"));encoder.Save(file);}}
 void CaptureSuitePreview(){var windows=app.NoteWindows.Values.Cast<Window>().Concat(new Window[]{app.Widget.SettingsPanel!,app.Widget}).ToArray();foreach(var theme in new[]{"Claro","Escuro"}){app.Store.Book.Theme=theme;app.ApplyTheme();foreach(var w in windows)w.UpdateLayout();var work=NoteWindow.WorkArea(app.Widget);var visual=new System.Windows.Media.DrawingVisual();using(var dc=visual.RenderOpen()){dc.DrawRectangle(Visuals.Brush(theme=="Claro"?"#E9EDF3":"#191C23"),null,new Rect(0,0,1920,1080));double scale=Math.Min(1880/work.Width,1040/work.Height);dc.PushTransform(new System.Windows.Media.TranslateTransform(20,20));dc.PushTransform(new System.Windows.Media.ScaleTransform(scale,scale));foreach(var w in windows)dc.DrawRectangle(new System.Windows.Media.VisualBrush(w){Stretch=System.Windows.Media.Stretch.Fill},null,new Rect(w.Left-work.Left,w.Top-work.Top,w.Width,w.Height));dc.Pop();dc.Pop();}var bitmap=new System.Windows.Media.Imaging.RenderTargetBitmap(1920,1080,96,96,System.Windows.Media.PixelFormats.Pbgra32);bitmap.Render(visual);var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));using var file=File.Create(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(report))!,"suite-"+theme.ToLowerInvariant()+".png"));encoder.Save(file);}}
 void VerifyEdgeResizing(NoteWindow w){var work=NoteWindow.WorkArea(w);w.Left=work.Left+80;w.Top=work.Top+80;w.Width=344;w.Height=380;double x=w.Left,y=w.Top;w.ResizeFromEdge(NoteResizeEdge.Right,36,0);Check("Right edge increases width and keeps left edge fixed",Math.Abs(w.Width-380)<1&&Math.Abs(w.Left-x)<1);double right=w.Left+w.Width;w.ResizeFromEdge(NoteResizeEdge.Left,-24,0);Check("Left edge increases width and keeps right edge fixed",Math.Abs(w.Width-404)<1&&Math.Abs(w.Left+w.Width-right)<1);w.ResizeFromEdge(NoteResizeEdge.Bottom,0,28);Check("Bottom edge increases height and keeps top edge fixed",Math.Abs(w.Height-408)<1&&Math.Abs(w.Top-y)<1);double bottom=w.Top+w.Height;w.ResizeFromEdge(NoteResizeEdge.Top,0,-20);Check("Top edge increases height and keeps bottom edge fixed",Math.Abs(w.Height-428)<1&&Math.Abs(w.Top+w.Height-bottom)<1);w.ResizeFromEdge(NoteResizeEdge.Left|NoteResizeEdge.Top,-10,-10);Check("Corner resizes both axes without moving opposite edges",Math.Abs(w.Width-414)<1&&Math.Abs(w.Height-438)<1&&Math.Abs(w.Left+w.Width-right)<1&&Math.Abs(w.Top+w.Height-bottom)<1);w.ResizeFromEdge(NoteResizeEdge.Left|NoteResizeEdge.Top,100000,100000);Check("Resize respects minimum dimensions",Math.Abs(w.Width-w.MinWidth)<1&&Math.Abs(w.Height-w.MinHeight)<1);w.Save();var restored=new Store(app.DataRoot).Book.Notes.Single(n=>n.Id==w.Note.Id);Check("Edge resize dimensions and position survive reload",Math.Abs(restored.Width-w.Width)<1&&Math.Abs(restored.Height-w.Height)<1&&Math.Abs(restored.Left-w.Left)<1&&Math.Abs(restored.Top-w.Top)<1);}
 [DllImport("user32.dll",EntryPoint="GetWindowLongPtrW")]static extern IntPtr GetWindowLongPtr(IntPtr hwnd,int index);
}
