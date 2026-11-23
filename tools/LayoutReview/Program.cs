using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StickyNotes;
// Tests the real WPF controls with fresh local data, no tray and no account.
internal static class LayoutReview {
 [STAThread] public static int Main(string[] args){
  if(args.Length!=2||args[1]!="--self-test")return 2;
  string report=Path.GetFullPath(args[0]);
  if(File.Exists(report))return 2;
  var app=new NotesApp{ShutdownMode=ShutdownMode.OnExplicitShutdown};
  app.DataRoot=Path.Combine(Path.GetTempPath(),"SMT-Layout-Review-"+Guid.NewGuid().ToString("N"));
  app.Store=new Store(app.DataRoot);
  var checks=new List<object>();int failed=0;
  void Check(string name,bool passed,object details){checks.Add(new{name,passed,details});if(!passed)failed++;}
  app.Startup+=(_,_)=>{
   try{
    Visuals.InstallResources();app.Sync=new SyncController(app);
    var widget=app.Widget;widget.ShowActivated=false;widget.Show();
    Check("Cold widget fits native work area",LiveVerification.NativeWindowFits(widget),new{dpi=DisplayDpi.Scale(widget).DpiScaleX});
    widget.OpenSettings();Check("Cold settings fit native work area",widget.SettingsPanel is {} cold&&LiveVerification.NativeWindowFits(cold),new{dpi=DisplayDpi.Scale(widget).DpiScaleX});widget.CloseSettings(false);
    foreach(string theme in new[]{"Claro","Escuro"}){
     app.Store.Book.Theme=theme;widget.Refresh();
     foreach(var size in new[]{new Size(360,400),new Size(360,320),new Size(300,320)}){
      var panel=new SettingsWindow(app,widget){ShowActivated=false,Width=size.Width,Height=size.Height};panel.Show();
      for(int section=0;section<panel.SectionCount;section++){
       panel.SelectSection(section);panel.UpdateLayout();
       var labels=Descendants<TextBlock>(panel).Where(t=>t.IsVisible&&!string.IsNullOrEmpty(t.Text)).ToArray();
       double RenderedFont(TextBlock text){var p=text.TranslatePoint(new Point(),panel);var q=text.TranslatePoint(new Point(0,text.FontSize),panel);return Math.Abs(q.Y-p.Y);}
       Check($"Settings {theme} {size.Width}x{size.Height} section {section}",panel.SectionsFit(),new{minRenderedFont=labels.Min(RenderedFont),panel.SectionCount});
      }
      panel.Dismiss(false);
     }
    }
    var note=app.Store.Create();note.Title=new string('W',180);
    var owner=new NoteWindow(app,note);new System.Windows.Interop.WindowInteropHelper(owner).EnsureHandle();
    foreach(double width in new[]{416d,360d,300d}){
     var dialog=new DeleteConfirmation(owner,"Escuro"){Width=width,ShowActivated=false};dialog.Show();dialog.UpdateLayout();
     var buttons=Descendants<Button>(dialog).Where(b=>b.IsVisible).ToArray();
     bool Fits(Button b){var p=b.TranslatePoint(new Point(),dialog);return p.X>=0&&p.Y>=0&&p.X+b.ActualWidth<=dialog.ActualWidth+1&&p.Y+b.ActualHeight<=dialog.ActualHeight+1;}
     Check($"Delete dialog at {width} DIP keeps cancel default and actions accessible",buttons.Length==3&&buttons.All(Fits)&&buttons.Single(b=>b.IsDefault).IsCancel,new{width,buttonCount=buttons.Length});
     dialog.Close();
    }
   }catch(Exception e){Check("Layout review completed",false,new{error=e.GetType().Name,message=e.Message});}
   finally{
    app.Sync?.Dispose();Directory.CreateDirectory(Path.GetDirectoryName(report)!);
    File.WriteAllText(report,JsonSerializer.Serialize(new{reviewAssemblyVersion=Distribution.Version,failed,passed=checks.Count-failed,checks,scope="Linked-source WPF layout harness, not the distributed executable. No physical input, tray icon, real account or global display change. 300 DIP is a stress case and may reduce text below 10 DIP."},new JsonSerializerOptions{WriteIndented=true}));
    app.Shutdown(failed==0?0:1);
   }
  };
  return app.Run();
 }
 static IEnumerable<T> Descendants<T>(DependencyObject parent)where T:DependencyObject{
  for(int i=0;i<VisualTreeHelper.GetChildrenCount(parent);i++){var child=VisualTreeHelper.GetChild(parent,i);if(child is T value)yield return value;foreach(var nested in Descendants<T>(child))yield return nested;}
 }
}
