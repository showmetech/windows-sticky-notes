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
 string? manuallyMovedId;Rect manuallyMovedBounds;
 LiveVerification(NotesApp app,string report){this.app=app;this.report=report;timer.Tick+=Tick;timer.Start();}
 public static void Start(NotesApp app,string report)=>_=new LiveVerification(app,report);
 void Check(string name,bool ok){checks.Add(new{name,passed=ok});if(!ok)failed++;}
 void Tick(object? sender,EventArgs e){try{switch(phase++){
  case 0:
   if(!app.Widget.IsVisible)app.Widget.Present();
   Check("Cold first widget presentation stays inside the native monitor work area",app.Widget.IsVisible&&NativeWindowFits(app.Widget));
   app.Widget.OpenSettings();Check("Cold first settings presentation stays inside the native monitor work area",app.Widget.SettingsPanel is {} coldSettings&&NativeWindowFits(coldSettings));app.Widget.CloseSettings(false);
   app.ShowNotes();Check("Native tray created",app.TrayVisible);Check("Exactly three fixture notes",app.NoteWindows.Count==3);Check("First note presentation automatically stacks bottom right without a cascade",BottomArranged());CaptureArrangementPreview("first-open-preview.png");break;
  case 1:
   var windows=app.NoteWindows.Values.ToList();Check("All notes visible",windows.All(w=>w.IsVisible));Check("Native windows absent from taskbar",windows.All(w=>!w.ShowInTaskbar&&(GetWindowLongPtr(new WindowInteropHelper(w).Handle,-20).ToInt64()&0x40000)==0));
   Check("Positions restored without collapsing into cascade",windows.Select(w=>(w.Left,w.Top)).Distinct().Count()==3);
   foreach(var w in windows)Check("Note inside monitor work area: "+w.Note.Title,NoteWindow.WorkArea(w).Contains(new Rect(w.Left,w.Top,w.Width,w.Height)));
   Check("Exactly six bottom actions stay aligned",windows.All(w=>w.BottomButtonsAligned()));Check("Four top actions show minimize with no legacy menu or passive labels",windows.All(w=>w.CompactChromeVisible()));var narrow=windows[0];double previousWidth=narrow.Width;narrow.Width=300;Check("Bottom actions fit and align at minimum note width",narrow.BottomButtonsAligned());Check("Four top actions remain visible at minimum note width",narrow.CompactChromeVisible());narrow.Width=previousWidth;
   Check("All four edges and four corners receive pointer hits with resize cursors",windows.All(w=>w.ResizeEdgesAvailable()));VerifyEdgeResizing(windows[0]);VerifyTitleCommit(windows[0]);
   var formatting=windows[0];formatting.Editor.SelectAll();formatting.Editor.Selection.ApplyPropertyValue(System.Windows.Documents.TextElement.FontWeightProperty,FontWeights.Bold);formatting.Editor.Selection.Select(formatting.Editor.Document.ContentStart,formatting.Editor.Document.ContentEnd);Check("Bold toolbar state follows selected text",formatting.BoldSelected);formatting.Editor.Selection.ApplyPropertyValue(System.Windows.Documents.TextElement.FontWeightProperty,FontWeights.Normal);formatting.Editor.SelectAll();Check("Bold toolbar clears for regular text",!formatting.BoldSelected);formatting.OpenPalette();Check("All fifty palette colors remain labeled and keyboard accessible",formatting.PaletteAccessible());formatting.ClosePalette();
   var moved=windows[0];var area=NoteWindow.WorkArea(moved);double x=area.Left+93,y=area.Top+117;moved.Left=x;moved.Top=y;moved.Save();moved.FitToScreen();Check("Free placement is not snapped or reset",Math.Abs(moved.Left-x)<1&&Math.Abs(moved.Top-y)<1);moved.Save();var restored=new Store(app.DataRoot).Book.Notes.Single(n=>n.Id==moved.Note.Id);Check("Moved position survives reload",Math.Abs(restored.Left-x)<1&&Math.Abs(restored.Top-y)<1);
   manuallyMovedId=moved.Note.Id;manuallyMovedBounds=new Rect(moved.Left,moved.Top,moved.Width,moved.Height);app.HideNotes();break;
  case 2:Check("Hide animation finishes and keeps process alive",app.NoteWindows.Values.All(w=>!w.IsVisible));
   var areaItems=app.Widget.AreaPicker.ItemsSource;app.Widget.Refresh();Check("Refreshing notes retains the unchanged area item source",ReferenceEquals(areaItems,app.Widget.AreaPicker.ItemsSource));
   var active=app.Store.Book.ActiveArea;app.Widget.AreaPicker.RaiseEvent(new SelectionChangedEventArgs(ComboBox.SelectionChangedEvent,new object[]{active},new object[]{active}));
   Check("Reselecting the current area cannot reopen concealed notes",app.NoteWindows.Values.All(w=>!w.IsPresented));
   if(app.NoteWindows.Values.Any(w=>w.IsPresented))app.HideNotes();app.HandleTrayClick();break;
  case 3:Check("Shared left-click handler restores saved notes",app.NoteWindows.Values.All(w=>w.IsVisible));var manual=app.NoteWindows[manuallyMovedId!];Check("Later tray restores preserve manual placement after the first automatic arrangement",manuallyMovedBounds==new Rect(manual.Left,manual.Top,manual.Width,manual.Height));var beforeContent=app.NoteWindows.Values.ToDictionary(w=>w.Note.Id,w=>new System.Windows.Documents.TextRange(w.Editor.Document.ContentStart,w.Editor.Document.ContentEnd).Text);
   app.Widget.AutoArrangeButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
   Check("Auto-arrange button invokes the real command and preserves note contents",app.NoteWindows.Values.All(w=>beforeContent[w.Note.Id]==new System.Windows.Documents.TextRange(w.Editor.Document.ContentStart,w.Editor.Document.ContentEnd).Text));break;
  case 4:
   Check("Auto-arrangement keeps notes on each monitor work area",app.NoteWindows.Values.All(w=>NoteWindow.WorkArea(w).Contains(new Rect(w.Left,w.Top,w.Width,w.Height))));
   var arranged=app.NoteWindows.Values.ToArray();
   Check("Auto-arrange button uses the same bottom-right upward layout",BottomArranged());
   Check("Auto-arrangement has no overlapping windows",arranged.SelectMany((a,i)=>arranged.Skip(i+1).Select(b=>Rect.Intersect(new Rect(a.Left,a.Top,a.Width,a.Height),new Rect(b.Left,b.Top,b.Width,b.Height)))).All(r=>r.IsEmpty||r.Width==0||r.Height==0));
   Check("Organized visible cards have a compact ten DIP gap",arranged.GroupBy(w=>w.Left).All(group=>{var column=group.OrderBy(w=>w.Top).ToArray();return column.Skip(1).Select((next,i)=>Math.Abs(next.Top-column[i].Top-column[i].Height+((Border)next.Content).Margin.Top+((Border)column[i].Content).Margin.Bottom-10)<.1).All(ok=>ok);}));
   Check("Auto-arrangement saves geometry for restart",arranged.All(w=>{var n=new Store(app.DataRoot).Book.Notes.Single(n=>n.Id==w.Note.Id);return Math.Abs(n.Left-w.Left)<.1&&Math.Abs(n.Top-w.Top)<.1&&Math.Abs(n.Width-w.Width)<.1&&Math.Abs(n.Height-w.Height)<.1;}));
   var firstArrangement=arranged.Select(w=>new Rect(w.Left,w.Top,w.Width,w.Height)).ToArray();app.Arrange();
   Check("Repeated auto-arrangement leaves the same windows aligned",arranged.Select((w,i)=>Math.Abs(w.Left-firstArrangement[i].Left)<.1&&Math.Abs(w.Top-firstArrangement[i].Top)<.1&&Math.Abs(w.Height-firstArrangement[i].Height)<.1).All(ok=>ok));
   CaptureArrangementPreview();app.Widget.Present();break;
  case 5:
   Check("Widget uses the public SMT product name",app.Widget.Title==Distribution.DisplayName+" — Widget");
   Check("Suite widget header aligns settings, pin and close",SuiteChrome.Aligned(app.Widget));
   Check("Suite widget stays off taskbar",!app.Widget.ShowInTaskbar);
   Check("Three icon actions fit in one aligned row",app.Widget.PanelActionsAligned());
   Check("Empty search shows its visual hint",app.Widget.SearchHintVisible);
   app.Widget.SearchInput.Text="Referências";
   Check("Search hides hint and filters colored note cards",!app.Widget.SearchHintVisible&&app.Widget.NoteTiles.Length==1);Check("Search provides an accessible clear action",app.Widget.ClearSearchButton.IsVisible);app.Widget.ClearSearchButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Check("Clear search restores every note and hides itself",app.Widget.SearchInput.Text.Length==0&&app.Widget.NoteTiles.Length==3&&!app.Widget.ClearSearchButton.IsVisible);
   VerifySettingsIdleReturn();app.Widget.OpenSettings();break;
  case 6:
   Check("Settings panel uses available monitor space with matching width",app.Widget.SettingsAligned);
   Check("Settings panel stays off taskbar",app.Widget.SettingsPanel?.ShowInTaskbar==false);
   Check("Three settings sections fit without scrolling",app.Widget.SettingsPanel!.SectionCount==3&&app.Widget.SettingsPanel.SectionsFit());
   var panel=app.Widget.SettingsPanel!;double savedHeight=panel.Height;panel.Height=460;Check("Settings sections remain fully reachable at natural font size",panel.SectionsFit()&&panel.NaturalTextSize());panel.Height=savedHeight;panel.UpdateLayout();
   panel.SelectSection(1);var areaInput=Walk<TextBox>(panel).Single(t=>System.Windows.Automation.AutomationProperties.GetName(t)=="Nome da nova área");var createArea=Walk<Button>(panel).Single(t=>System.Windows.Automation.AutomationProperties.GetName(t)=="Criar área");string previousArea=app.Store.Book.ActiveArea;areaInput.Text="QA new area";createArea.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Check("Creating an area persists it and keeps success feedback inside the panel",new Store(app.DataRoot).Book.Areas.Contains("QA new area")&&panel.SectionsFit());areaInput.Text="QA new area";createArea.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Check("Duplicate area validation stays inline without adding a second area",app.Store.Book.Areas.Count(a=>a=="QA new area")==1&&panel.SectionsFit());areaInput.Clear();createArea.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Check("Empty area validation keeps every control reachable",panel.SectionsFit());app.Widget.AreaPicker.SelectedItem="QA new area";Check("Choosing another area through the picker updates the active area",app.Store.Book.ActiveArea=="QA new area");app.Widget.AreaPicker.SelectedItem=previousArea;Check("Area picker returns to the previous area without losing notes",app.Store.Book.ActiveArea==previousArea&&app.NoteWindows.Count==3);app.Store.Book.Areas.Remove("QA new area");app.Store.Save();panel.SelectSection(0);
   var opacity=app.Widget.SettingsPanel!.OpacitySlider;
   Check("Opacity slider starts at 100 percent with keyboard range controls",opacity.Value==100&&opacity.Minimum==0&&opacity.Maximum==100&&opacity.SmallChange==1);
   opacity.Value=35;Check("Opacity slider updates all window backgrounds immediately",app.NoteWindows.Values.Cast<Window>().Concat(new Window[]{app.Widget,app.Widget.SettingsPanel}).All(w=>SurfaceAlpha(w,SystemParameters.HighContrast?(byte)255:(byte)89)));
   opacity.Value=0;Check("Zero background opacity keeps windows and child controls visible",app.NoteWindows.Values.Cast<Window>().Concat(new Window[]{app.Widget,app.Widget.SettingsPanel}).All(w=>w.IsVisible&&w.Opacity==1&&SurfaceAlpha(w,SystemParameters.HighContrast?(byte)255:(byte)0)));
   opacity.Value=100;
   app.Store.Book.Theme="Escuro";app.ApplyTheme();Check("Theme switch retains settings panel and header alignment",app.Widget.SettingsVisible&&app.Widget.SettingsAligned);
   Check("Outside focus policy protects pinned widgets, menus and own windows",SuiteActivation.VerifyFocusPolicy());
   Check("Native tray supports mouse and keyboard activation",NativeTray.CallbackAction(0x400,true)==1&&NativeTray.CallbackAction(0x401,true)==1&&NativeTray.CallbackAction(0x7b,true)==2);
   CaptureSuitePreview();CaptureSettingsPreview();app.Widget.SettingsPanel!.OpacitySlider.Value=35;
   app.Widget.CloseSettings();break;
  case 7:
   Check("Closing settings flushes the opacity preference to disk",new Store(app.DataRoot).Book.BackgroundOpacity==35);
   app.Widget.OpenSettings();Check("Reopening settings restores saved opacity",app.Widget.SettingsPanel!.OpacitySlider.Value==35);app.Widget.SettingsPanel.OpacitySlider.Value=100;app.Widget.CloseSettings(false);
   Check("Returning slider to 100 restores and saves the opaque default",new Store(app.DataRoot).Book.BackgroundOpacity==100&&SurfaceAlpha(app.Widget,255));
   Check("Closing settings restores compact widget without closing notes",!app.Widget.SettingsVisible&&app.Widget.IsVisible&&app.NoteWindows.Values.All(w=>w.IsVisible));app.Widget.Conceal();break;
  case 8:
   Check("Recollecting widget preserves visible desktop notes",!app.Widget.IsVisible&&app.NoteWindows.Values.All(w=>w.IsVisible));app.HandleTrayClick();break;
  case 9:
   Check("Tray click hides every visible note",!app.Widget.IsVisible&&app.NoteWindows.Values.All(w=>!w.IsVisible&&!w.IsPresented));
   var trayGeometry=app.NoteWindows.Values.ToDictionary(w=>w.Note.Id,w=>new Rect(w.Left,w.Top,w.Width,w.Height));var trayText=app.NoteWindows.Values.ToDictionary(w=>w.Note.Id,w=>new System.Windows.Documents.TextRange(w.Editor.Document.ContentStart,w.Editor.Document.ContentEnd).Text);
   app.HandleTrayClick();Check("Next tray click restores every saved note",app.NoteWindows.Values.All(w=>w.IsVisible&&w.IsPresented));
   Check("Tray click controls notes without opening the widget",!app.Widget.IsVisible);
   app.NoteWindows.Values.First().Conceal();app.HandleTrayClick();Check("Mixed visible and hidden notes are all hidden by one tray click",app.NoteWindows.Values.All(w=>!w.IsPresented));app.HandleTrayClick();
   for(int tap=0;tap<6;tap++)app.HandleTrayClick();Check("Rapid tray clicks alternate correctly during fade animations",app.NoteWindows.Values.All(w=>w.IsPresented));
   Check("Tray toggling preserves content and note geometry",app.NoteWindows.Values.All(w=>trayGeometry[w.Note.Id]==new Rect(w.Left,w.Top,w.Width,w.Height)&&trayText[w.Note.Id]==new System.Windows.Documents.TextRange(w.Editor.Document.ContentStart,w.Editor.Document.ContentEnd).Text));
   phase=90;break;
  case 90:
   Check("Rapid click result remains visible after animations complete",app.NoteWindows.Values.All(w=>w.IsVisible&&w.IsPresented));
   foreach(var noteWindow in app.NoteWindows.Values)noteWindow.WindowState=WindowState.Minimized;app.HandleTrayClick();Check("Tray click also restores windows minimized by Windows",app.NoteWindows.Values.All(w=>w.WindowState==WindowState.Normal&&w.IsPresented));
   VerifyNoteAutoHide();app.Widget.Present();
   var tiles=app.Widget.NoteTiles;
   Check("Widget has one compact single-line button per note with exact saved color and readable title",tiles.Length==3&&tiles.All(tile=>{var n=app.Store.Book.Notes.Single(n=>n.Id==(string)tile.Tag);return tile.Content is TextBlock text&&text.Text==n.Title&&text.TextWrapping==TextWrapping.NoWrap&&text.TextTrimming==TextTrimming.CharacterEllipsis&&tile.Height==46&&(SystemParameters.HighContrast||tile.Background is SolidColorBrush brush&&brush.Color==(Color)ColorConverter.ConvertFromString(n.Color)&&tile.Foreground is SolidColorBrush ink&&NoteColors.Contrast(brush.Color,ink.Color)>=4.5);}));
   app.HideNotes();phase=10;break;
  case 10:
   Check("Notes concealed before tile activation",app.NoteWindows.Values.All(w=>!w.IsVisible));
   app.Widget.NoteTiles[0].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));break;
  case 11:
   var selected=app.NoteWindows[(string)app.Widget.NoteTiles[0].Tag];
   Check("Clicking colored tile opens only that note and highlights its border",selected.IsVisible&&selected.HighlightVisible&&app.NoteWindows.Values.Where(w=>w!=selected).All(w=>!w.IsVisible&&!w.HighlightVisible));
   app.Widget.NoteTiles[0].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));break;
  case 12:Check("Repeated tile click restarts border highlight",app.NoteWindows.Values.Count(w=>w.HighlightVisible)==1);app.Widget.NoteTiles[1].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));break;
  case 13:Check("Selecting another tile transfers the highlight to that note only",app.NoteWindows.Values.Count(w=>w.HighlightVisible)==1&&app.NoteWindows[(string)app.Widget.NoteTiles[1].Tag].HighlightVisible);break;
  case 14:case 15:case 16:break;
  case 17:
   Check("Highlight clears automatically while selected notes stay visible",app.NoteWindows.Values.All(w=>!w.HighlightVisible)&&app.NoteWindows.Values.Count(w=>w.IsVisible)==2);
   Check("Flush succeeds",app.FlushAll());app.ShowNotes();
   foreach(var w in app.NoteWindows.Values){w.HideAnimationOverride=_=>{};try{w.Conceal();}finally{w.HideAnimationOverride=null;}}break;
  case 18:
   Check("Interrupted note fade still completes concealment without an animation callback",app.NoteWindows.Values.All(w=>!w.IsVisible&&!w.IsPresented));
   app.HandleTrayClick();Check("Tray restores notes after interrupted fade completion",app.NoteWindows.Values.All(w=>w.IsVisible&&w.IsPresented));
   foreach(var w in app.NoteWindows.Values){w.Conceal();w.Present();}break;
  case 19:
   Check("Stale fade completion never hides notes restored immediately",app.NoteWindows.Values.All(w=>w.IsVisible&&w.IsPresented));timer.Stop();Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(report))!);CapturePreview();File.WriteAllText(report,JsonSerializer.Serialize(new{at=DateTimeOffset.Now,failed,checks,notes=app.NoteWindows.Values.Select(w=>new{w.Note.Title,w.Left,w.Top,w.Width,w.Height,w.ShowInTaskbar}),limitations="Tray restoration invoked through the same handler as the native left click; no physical tray click was automated. Physical monitor hot-unplug, ARM64 hardware and OS reboot were not tested. Preview PNGs render the real WPF window tree with fixture notes, not a screenshot of the desktop."},new JsonSerializerOptions{WriteIndented=true}));app.ExitApp();break;
 }}catch(Exception ex){timer.Stop();File.WriteAllText(report,ex.ToString());app.ExitApp();}}
 void VerifyNoteAutoHide(){
  var windows=app.NoteWindows.Values.ToArray();var focus=app.AutoHide!;var first=windows[0];var original=first.Note.Pinned;
  var text=windows.ToDictionary(w=>w.Note.Id,w=>new System.Windows.Documents.TextRange(w.Editor.Document.ContentStart,w.Editor.Document.ContentEnd).Text);var geometry=windows.ToDictionary(w=>w.Note.Id,w=>new Rect(w.Left,w.Top,w.Width,w.Height));
  first.Note.Pinned=true;first.Topmost=true;var own=new WindowInteropHelper(first).Handle;var outside=GetShellWindow();
  focus.ObserveForeground(own);Check("Editing another Sticky Notes window keeps the group visible",windows.All(w=>w.IsPresented));
  focus.ObserveForeground(IntPtr.Zero);Check("Transient missing foreground does not hide notes",windows.All(w=>w.IsPresented));
  SuiteActivation.MenuOpen=true;focus.ObserveForeground(outside);SuiteActivation.MenuOpen=false;Check("Open tray menu protects unpinned notes",windows.All(w=>w.IsPresented));
  focus.ObserveForeground(outside,true);Check("Pointer over this app's tray icon protects tray toggle ordering",windows.All(w=>w.IsPresented));
  focus.ObserveForeground(outside);Check("Outside desktop focus hides every unpinned note and preserves pinned notes",first.IsPresented&&windows.Where(w=>w!=first).All(w=>!w.IsPresented));
  app.HandleTrayClick();app.HandleTrayClick();Check("Tray restores all notes after outside-focus dismissal with a pinned note",windows.All(w=>w.IsPresented));
  first.Note.Pinned=false;first.Topmost=false;focus.ObserveForeground(own);focus.ObserveForeground(outside);Check("Removing pin makes the note dismiss with the others",windows.All(w=>!w.IsPresented));
  Check("Outside-focus dismissal preserves documents and geometry",windows.All(w=>text[w.Note.Id]==new System.Windows.Documents.TextRange(w.Editor.Document.ContentStart,w.Editor.Document.ContentEnd).Text&&geometry[w.Note.Id]==new Rect(w.Left,w.Top,w.Width,w.Height)));
  first.Note.Pinned=original;first.Topmost=original;app.ShowNotes();
 }
 [DllImport("user32.dll")]static extern IntPtr GetShellWindow();
 bool BottomArranged(){foreach(var group in app.NoteWindows.Values.GroupBy(w=>NoteWindow.WorkArea(w))){var ordered=group.OrderByDescending(w=>w.Left).ThenByDescending(w=>w.Top).ToArray();var plan=NoteArrangement.Plan(group.Key,ordered.Select(w=>new Size(w.Width,w.Height)).ToArray());if(plan==null||ordered.Where((w,i)=>Math.Abs(w.Left-plan[i].Left)>.1||Math.Abs(w.Top-plan[i].Top)>.1).Any())return false;}return true;}
 void CaptureArrangementPreview(string name="auto-arrange-preview.png"){
  var work=NoteWindow.WorkArea(app.NoteWindows.Values.First());double scale=Math.Min(1600/work.Width,1200/work.Height);
  var visual=new DrawingVisual();using(var dc=visual.RenderOpen()){dc.DrawRectangle(Visuals.Brush("#E9EDF3"),null,new Rect(0,0,work.Width*scale,work.Height*scale));dc.PushTransform(new ScaleTransform(scale,scale));foreach(var w in app.NoteWindows.Values){w.UpdateLayout();dc.DrawRectangle(new VisualBrush(w){Stretch=Stretch.Fill},null,new Rect(w.Left-work.Left,w.Top-work.Top,w.Width,w.Height));}dc.Pop();}
  var bitmap=new System.Windows.Media.Imaging.RenderTargetBitmap((int)Math.Ceiling(work.Width*scale),(int)Math.Ceiling(work.Height*scale),96,96,PixelFormats.Pbgra32);bitmap.Render(visual);var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));using var file=File.Create(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(report))!,name));encoder.Save(file);
 }
 void VerifySettingsIdleReturn(){var widget=app.Widget;var original=widget.Clock;long tick=1000;try{widget.Clock=()=>tick;widget.Present();widget.OpenSettings();tick+=25000;widget.EvaluateIdle();Check("Settings protect widget through more than twenty seconds of inactivity",widget.IsVisible&&widget.SettingsVisible);widget.CloseSettings(false);bool expired=widget.EvaluateIdle();Check("Closing settings restarts the widget idle period instead of hiding immediately",!expired&&widget.IsVisible);tick+=19000;expired=widget.EvaluateIdle();Check("Returned widget keeps its normal idle grace period",!expired&&widget.IsVisible);}finally{widget.Clock=original;widget.Present();}}
 static IEnumerable<T> Walk<T>(DependencyObject root)where T:DependencyObject{for(int i=0;i<VisualTreeHelper.GetChildrenCount(root);i++){var child=VisualTreeHelper.GetChild(root,i);if(child is T value)yield return value;foreach(var next in Walk<T>(child))yield return next;}}
 void VerifyTitleCommit(NoteWindow w){
  var original=w.Note.Title;var text=new System.Windows.Documents.TextRange(w.Editor.Document.ContentStart,w.Editor.Document.ContentEnd).Text;
  w.Activate();w.TitleBox.Focus();w.TitleBox.Text="Título confirmado com Enter";
  var enter=new System.Windows.Input.KeyEventArgs(System.Windows.Input.Keyboard.PrimaryDevice,PresentationSource.FromVisual(w),0,System.Windows.Input.Key.Enter){RoutedEvent=System.Windows.Input.Keyboard.PreviewKeyDownEvent};w.TitleBox.RaiseEvent(enter);
  Check("Enter confirms and persists title and transfers focus to note body",enter.Handled&&w.Editor.IsKeyboardFocusWithin&&!w.TitleBox.IsKeyboardFocusWithin&&new Store(app.DataRoot).Book.Notes.Single(n=>n.Id==w.Note.Id).Title==w.TitleBox.Text);
  Check("Confirming title adds no newline to title or body",!w.TitleBox.Text.Contains('\n')&&new System.Windows.Documents.TextRange(w.Editor.Document.ContentStart,w.Editor.Document.ContentEnd).Text==text);
  var bodyEnter=new System.Windows.Input.KeyEventArgs(System.Windows.Input.Keyboard.PrimaryDevice,PresentationSource.FromVisual(w),0,System.Windows.Input.Key.Enter){RoutedEvent=System.Windows.Input.Keyboard.PreviewKeyDownEvent};w.Editor.RaiseEvent(bodyEnter);Check("Title confirmation does not intercept Enter in note body",!bodyEnter.Handled);
  w.TitleBox.Text=original;w.Save();
 }
 void CapturePreview(){var examples=app.NoteWindows.Values.ToArray();for(int i=0;i<examples.Length;i++){examples[i].Note.ColorMode=i==0?"Vivid":"Neon";examples[i].Note.Color=i==0?"#8338EC":i==1?"#39FF14":"#FF00D4";}foreach(string theme in new[]{"Claro","Escuro"}){app.Store.Book.Theme=theme;app.ApplyTheme();app.FlushAll();var visual=new System.Windows.Media.DrawingVisual();using(var dc=visual.RenderOpen()){dc.DrawRectangle(Visuals.Brush(theme=="Claro"?"#E9EDF3":"#191C23"),null,new Rect(0,0,1100,410));int index=0;foreach(var w in app.NoteWindows.Values){w.UpdateLayout();dc.DrawRectangle(new System.Windows.Media.VisualBrush(w){Stretch=System.Windows.Media.Stretch.Fill},null,new Rect(18+index*356,20,w.Width,w.Height));index++;}}var bitmap=new System.Windows.Media.Imaging.RenderTargetBitmap(1650,615,144,144,System.Windows.Media.PixelFormats.Pbgra32);bitmap.Render(visual);var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));using var file=File.Create(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(report))!,theme=="Claro"?"preview-claro.png":"preview-escuro.png"));encoder.Save(file);}}
 void CaptureSuitePreview(){var windows=app.NoteWindows.Values.Cast<Window>().Concat(new Window[]{app.Widget.SettingsPanel!,app.Widget}).ToArray();foreach(var theme in new[]{"Claro","Escuro"}){app.Store.Book.Theme=theme;app.ApplyTheme();foreach(var w in windows)w.UpdateLayout();Check("Widget labels remain readable in "+theme,app.Widget.PanelTextReadable());VerifyFluent(theme);var widgetImage=new System.Windows.Media.Imaging.RenderTargetBitmap((int)Math.Ceiling(app.Widget.Width*1.5),(int)Math.Ceiling(app.Widget.Height*1.5),144,144,PixelFormats.Pbgra32);widgetImage.Render(app.Widget);var widgetEncoder=new System.Windows.Media.Imaging.PngBitmapEncoder();widgetEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(widgetImage));using(var widgetFile=File.Create(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(report))!,"widget-"+theme.ToLowerInvariant()+".png")))widgetEncoder.Save(widgetFile);var work=NoteWindow.WorkArea(app.Widget);var visual=new System.Windows.Media.DrawingVisual();using(var dc=visual.RenderOpen()){dc.DrawRectangle(Visuals.Brush(theme=="Claro"?"#E9EDF3":"#191C23"),null,new Rect(0,0,1920,1080));double scale=Math.Min(1880/work.Width,1040/work.Height);dc.PushTransform(new System.Windows.Media.TranslateTransform(20,20));dc.PushTransform(new System.Windows.Media.ScaleTransform(scale,scale));foreach(var w in windows)dc.DrawRectangle(new System.Windows.Media.VisualBrush(w){Stretch=System.Windows.Media.Stretch.Fill},null,new Rect(w.Left-work.Left,w.Top-work.Top,w.Width,w.Height));dc.Pop();dc.Pop();}var bitmap=new System.Windows.Media.Imaging.RenderTargetBitmap(1920,1080,96,96,System.Windows.Media.PixelFormats.Pbgra32);bitmap.Render(visual);var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));using var file=File.Create(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(report))!,"suite-"+theme.ToLowerInvariant()+".png"));encoder.Save(file);}}
 void VerifyFluent(string theme){
  bool dark=Visuals.Dark(theme);
  Check("Fluent accent and selected text retain accessible contrast in "+theme,SystemParameters.HighContrast||(NoteColors.Contrast(((SolidColorBrush)FluentDesign.Accent(dark)).Color,((SolidColorBrush)FluentDesign.Layer(dark)).Color)>=4.5&&NoteColors.Contrast(((SolidColorBrush)FluentDesign.SelectedText(dark)).Color,((SolidColorBrush)FluentDesign.Selected(dark)).Color)>=4.5));
  Check("All note windows load the native Fluent control dictionary in "+theme,SystemParameters.HighContrast||app.NoteWindows.Values.All(w=>w.Resources.MergedDictionaries.Any(d=>d.Source?.OriginalString.Contains("Fluent."+(dark?"Dark":"Light"))==true)));
  var button=app.Widget.AutoArrangeButton;app.Widget.Activate();System.Windows.Input.Keyboard.Focus(button);button.UpdateLayout();
  Check("Keyboard focus has an inner visible ring in "+theme,button.IsKeyboardFocused&&button.Template.FindName("Focus",button) is Border ring&&ring.Visibility==Visibility.Visible&&ring.BorderThickness.Left==2);
  Check("Button text stays fully opaque while state overlays are independent in "+theme,button.Opacity==1&&button.Template.FindName("State",button) is Border state&&state.IsHitTestVisible==false);
  System.Windows.Input.Keyboard.ClearFocus();
 }
 void VerifyEdgeResizing(NoteWindow w){var work=NoteWindow.WorkArea(w);w.Left=work.Left+80;w.Top=work.Top+80;w.Width=344;w.Height=380;double x=w.Left,y=w.Top;w.ResizeFromEdge(NoteResizeEdge.Right,36,0);Check("Right edge increases width and keeps left edge fixed",Math.Abs(w.Width-380)<1&&Math.Abs(w.Left-x)<1);double right=w.Left+w.Width;w.ResizeFromEdge(NoteResizeEdge.Left,-24,0);Check("Left edge increases width and keeps right edge fixed",Math.Abs(w.Width-404)<1&&Math.Abs(w.Left+w.Width-right)<1);w.ResizeFromEdge(NoteResizeEdge.Bottom,0,28);Check("Bottom edge increases height and keeps top edge fixed",Math.Abs(w.Height-408)<1&&Math.Abs(w.Top-y)<1);double bottom=w.Top+w.Height;w.ResizeFromEdge(NoteResizeEdge.Top,0,-20);Check("Top edge increases height and keeps bottom edge fixed",Math.Abs(w.Height-428)<1&&Math.Abs(w.Top+w.Height-bottom)<1);w.ResizeFromEdge(NoteResizeEdge.Left|NoteResizeEdge.Top,-10,-10);Check("Corner resizes both axes without moving opposite edges",Math.Abs(w.Width-414)<1&&Math.Abs(w.Height-438)<1&&Math.Abs(w.Left+w.Width-right)<1&&Math.Abs(w.Top+w.Height-bottom)<1);w.ResizeFromEdge(NoteResizeEdge.Left|NoteResizeEdge.Top,100000,100000);Check("Resize respects minimum dimensions",Math.Abs(w.Width-w.MinWidth)<1&&Math.Abs(w.Height-w.MinHeight)<1);w.Save();var restored=new Store(app.DataRoot).Book.Notes.Single(n=>n.Id==w.Note.Id);Check("Edge resize dimensions and position survive reload",Math.Abs(restored.Width-w.Width)<1&&Math.Abs(restored.Height-w.Height)<1&&Math.Abs(restored.Left-w.Left)<1&&Math.Abs(restored.Top-w.Top)<1);}
 [DllImport("user32.dll",EntryPoint="GetWindowLongPtrW")]static extern IntPtr GetWindowLongPtr(IntPtr hwnd,int index);
 [StructLayout(LayoutKind.Sequential)]struct NativeBounds{public int Left,Top,Right,Bottom;}
 [StructLayout(LayoutKind.Sequential)]struct NativeMonitor{public int Size;public NativeBounds Monitor,Work;public uint Flags;}
 [DllImport("user32.dll")]static extern bool GetWindowRect(IntPtr window,out NativeBounds bounds);
 [DllImport("user32.dll")]static extern IntPtr MonitorFromWindow(IntPtr window,uint flags);
 [DllImport("user32.dll")]static extern bool GetMonitorInfo(IntPtr monitor,ref NativeMonitor info);
 internal static bool NativeWindowFits(Window window){var handle=new WindowInteropHelper(window).Handle;if(handle==IntPtr.Zero||!GetWindowRect(handle,out var bounds))return false;var monitor=new NativeMonitor{Size=Marshal.SizeOf<NativeMonitor>()};if(!GetMonitorInfo(MonitorFromWindow(handle,2),ref monitor))return false;return bounds.Left>=monitor.Work.Left-1&&bounds.Top>=monitor.Work.Top-1&&bounds.Right<=monitor.Work.Right+1&&bounds.Bottom<=monitor.Work.Bottom+1;}
 static bool SurfaceAlpha(Window window,byte alpha)=>window.Content is Border border&&border.Background is SolidColorBrush brush&&brush.Color.A==alpha;
 void CaptureSettingsPreview(){var window=app.Widget.SettingsPanel!;for(int i=0;i<window.SectionCount;i++){window.SelectSection(i);window.UpdateLayout();var bitmap=new System.Windows.Media.Imaging.RenderTargetBitmap((int)(window.Width*1.5),(int)(window.Height*1.5),144,144,PixelFormats.Pbgra32);bitmap.Render(window);var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));string name=i==0?"settings-opacity-100.png":"settings-section-"+i+".png";using var file=File.Create(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(report))!,name));encoder.Save(file);}window.SelectSection(0);}
}
