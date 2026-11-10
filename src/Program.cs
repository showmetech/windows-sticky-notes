using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using Forms=System.Windows.Forms;

namespace StickyNotes;
public sealed class NotesApp:Application {
 public Store Store=null!;
 public bool Exiting {get;private set;}
 public readonly Dictionary<string,NoteWindow> NoteWindows=new();
 NativeTray? tray;WidgetWindow? library;EventWaitHandle? activation,quit;RegisteredWaitHandle? waiter,quitWaiter;Mutex? mutex;
 public string DataRoot="";
 internal bool TrayVisible=>tray?.Visible==true;
 internal void HandleTrayClick(){bool visible=library?.IsVisible==true;ShowNotes();EnsureWidget();if(visible)library!.Conceal();else library!.Present();}
 internal WidgetWindow Widget=>EnsureWidget();
 WidgetWindow EnsureWidget(){if(library==null)library=new WidgetWindow(this);return library;}
 public static string DefaultRoot=>Distribution.IsPackaged?Path.Combine(global::Windows.Storage.ApplicationData.Current.LocalFolder.Path,"notes"):Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Bruno","WindowsStickyNotes");
 [STAThread] public static int Main(string[] args){var app=new NotesApp{ShutdownMode=ShutdownMode.OnExplicitShutdown};
  if(args.Contains("--self-test"))return SelfTests.Run(args);
  var arg=Array.IndexOf(args,"--data-dir");app.DataRoot=arg>=0&&arg+1<args.Length?Path.GetFullPath(args[arg+1]):DefaultRoot;
  string key=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(app.DataRoot.ToUpperInvariant())))[..16];
  app.mutex=new Mutex(true,@"Local\BrunoStickyNotes-"+key,out bool first);
  if(!first){using var signal=new EventWaitHandle(false,EventResetMode.AutoReset,@"Local\BrunoStickyNotes-"+(args.Contains("--quit")?"Quit-":"Show-")+key);signal.Set();return 0;}
  if(args.Contains("--quit"))return 0;
  app.activation=new EventWaitHandle(false,EventResetMode.AutoReset,@"Local\BrunoStickyNotes-Show-"+key);
  app.waiter=ThreadPool.RegisterWaitForSingleObject(app.activation,(_,_)=>app.Dispatcher.BeginInvoke(()=>{app.ShowNotes();app.Widget.Present();}),null,Timeout.Infinite,false);
  app.quit=new EventWaitHandle(false,EventResetMode.AutoReset,@"Local\BrunoStickyNotes-Quit-"+key);app.quitWaiter=ThreadPool.RegisterWaitForSingleObject(app.quit,(_,_)=>app.Dispatcher.BeginInvoke(app.ExitApp),null,Timeout.Infinite,false);
  app.Startup+=(_,_)=>{try{app.Start(args);}catch(Exception e){Visuals.Error("O aplicativo não pôde iniciar. Seus arquivos foram preservados.\n\n"+e.Message);app.Shutdown(1);}};
  app.DispatcherUnhandledException+=(_,e)=>{try{File.AppendAllText(Path.Combine(app.DataRoot,"errors.log"),DateTimeOffset.Now+" "+e.Exception+Environment.NewLine);}catch{}Visuals.Error("Não foi possível concluir a operação.\n"+e.Exception.Message);e.Handled=true;};
  app.SessionEnding+=(_,e)=>{if(!app.FlushAll())e.Cancel=true;};app.Exit+=(_,_)=>{app.tray?.Dispose();app.waiter?.Unregister(null);app.quitWaiter?.Unregister(null);app.activation?.Dispose();app.quit?.Dispose();app.mutex?.Dispose();SystemEvents.DisplaySettingsChanged-=app.DisplaysChanged;SystemEvents.UserPreferenceChanged-=app.PreferencesChanged;};
  return app.Run();
 }
 void Start(string[] args){Visuals.InstallResources();Store=new Store(DataRoot);var widget=EnsureWidget();var handle=new System.Windows.Interop.WindowInteropHelper(widget).EnsureHandle();Guid? trayIdentity=args.Contains("--data-dir")?new Guid(SHA256.HashData(Encoding.UTF8.GetBytes(DataRoot.ToUpperInvariant()))[..16]):null;tray=new NativeTray(System.Windows.Interop.HwndSource.FromHwnd(handle),Path.Combine(AppContext.BaseDirectory,"Assets","Notes.ico"),HandleTrayClick,widget.OpenTrayMenu,trayIdentity);
  SystemEvents.DisplaySettingsChanged+=DisplaysChanged;SystemEvents.UserPreferenceChanged+=PreferencesChanged;
  if(args.Contains("--demo")&&Store.Book.Notes.Count==0)CreateDemo();
  if(Store.Book.Notes.Count==0){var n=Store.Create();n.Title="Sua mesa, suas ideias";var w=Get(n);w.Editor.Document.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run("Escreva aqui. Cole uma imagem com Ctrl+V ou arraste um arquivo para dentro da nota.\n\nArraste pelo nome da área para mover. O canto inferior direito ajusta o tamanho.\n\nTudo fica salvo neste computador.")));w.MarkDirty();w.Save();}
  if(Store.RecoveryMessage!=null)Visuals.Error(Store.RecoveryMessage);
  if(!Distribution.IsStartupActivation(args)){ShowNotes();EnsureWidget().Present();}
  if(args.Contains("--settings"))ShowLibrary(true);
  int verify=Array.IndexOf(args,"--verify-live");if(verify>=0&&verify+1<args.Length){if(string.Equals(DataRoot,DefaultRoot,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Execute a verificação em uma pasta isolada.");LiveVerification.Start(this,args[verify+1]);}
 }
 NoteWindow Get(Note n){if(NoteWindows.TryGetValue(n.Id,out var w))return w;w=new NoteWindow(this,n);NoteWindows[n.Id]=w;return w;}
 public NoteWindow NewNote(){var n=Store.Create();var visible=NoteWindows.Values.Where(w=>w.IsVisible).ToList();if(visible.Count>0){var last=visible.Last();n.Left=last.Left+28;n.Top=last.Top+28;}var w=Get(n);w.Present();w.TitleBox.Focus();w.TitleBox.SelectAll();library?.Refresh();return w;}
 public void ShowNotes(){var notes=Store.Book.Notes.Where(n=>!n.Deleted&&n.Area==Store.Book.ActiveArea).ToList();foreach(var n in notes)Get(n).Present();if(notes.Count==0)ShowLibrary();}
 public void ShowNote(Note n){if(Store.Book.ActiveArea!=n.Area)SwitchArea(n.Area);Get(n).Present();}
 public void HideNotes(){foreach(var w in NoteWindows.Values)if(w.IsVisible)w.Conceal();}
 public void SwitchArea(string area){if(!FlushAll())return;HideNotes();Store.Book.ActiveArea=area;Store.Save();ShowNotes();}
 public void ShowLibrary(bool settings=false){var widget=EnsureWidget();widget.Present();if(settings)widget.OpenSettings();else widget.Refresh();}
 public void Arrange(){var windows=Store.Book.Notes.Where(n=>!n.Deleted&&n.Area==Store.Book.ActiveArea).Select(Get).ToList();if(windows.Count==0)return;foreach(var w in windows)w.Present();var r=NoteWindow.WorkArea(windows[0]);int cols=Math.Max(1,(int)((r.Width-24)/344));int rows=Math.Max(1,(int)((r.Height-24)/340));int capacity=cols*rows;for(int i=0;i<windows.Count;i++){var w=windows[i];int slot=i%capacity,page=i/capacity;w.Width=Math.Min(344,r.Width-16);w.Height=Math.Min(360,(r.Height-24)/rows-8);w.Left=r.Left+12+(slot%cols)*((r.Width-24)/cols)+Math.Min(page*12,36);w.Top=r.Top+12+(slot/cols)*((r.Height-24)/rows)+Math.Min(page*12,36);w.Note.Left=w.Left;w.Note.Top=w.Top;w.Note.Width=w.Width;w.Note.Height=w.Height;w.FitToScreen();w.Save();}Store.Save();}
 public bool FlushAll(){bool ok=true;foreach(var w in NoteWindows.Values)ok=w.Save()&&ok;return ok;}
 public void ExitApp(){if(!FlushAll()){Visuals.Error("Há alterações que não puderam ser salvas. Libere espaço ou verifique a pasta de dados e tente Ctrl+S antes de sair.");return;}Store.Save();Exiting=true;foreach(var w in NoteWindows.Values)w.Close();library?.Close();Shutdown();}
 public void ApplyTheme(){foreach(var w in NoteWindows.Values)w.ApplyColor();library?.Refresh();}
 internal void RefreshWidget()=>library?.RefreshNotes();
 void DisplaysChanged(object? s,EventArgs e)=>Dispatcher.BeginInvoke(()=>{foreach(var w in NoteWindows.Values)w.FitToScreen();library?.Refit();});
 void PreferencesChanged(object s,UserPreferenceChangedEventArgs e)=>Dispatcher.BeginInvoke(ApplyTheme);
 public System.Threading.Tasks.Task<bool> StartupEnabledAsync()=>Distribution.StartupEnabled();
 public System.Threading.Tasks.Task SetStartup(bool enabled){if(!string.Equals(DataRoot,DefaultRoot,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("A configuração de inicialização não pode ser alterada em uma sessão de teste.");return Distribution.SetStartup(enabled);}
 public void ExportBackup(){if(!FlushAll()){Visuals.Error("Salve todas as alterações antes de criar o backup.");return;}var d=new SaveFileDialog{FileName="StickyNotes-"+DateTime.Now.ToString("yyyy-MM-dd-HHmm")+".zip",Filter="Backup ZIP|*.zip"};if(d.ShowDialog(library)==true){Store.Save();var full=Path.GetFullPath(d.FileName);if(full.StartsWith(Path.GetFullPath(DataRoot)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)){Visuals.Error("Escolha uma pasta fora dos dados do aplicativo.");return;}using(var archive=new ZipArchive(File.Open(full,FileMode.Create),ZipArchiveMode.Create)){foreach(var path in Directory.EnumerateFiles(DataRoot,"*",SearchOption.AllDirectories))archive.CreateEntryFromFile(path,Path.GetRelativePath(DataRoot,path),CompressionLevel.Optimal);}Visuals.Error("Backup criado com suas notas, cores, posições e imagens.");}}
 void CreateDemo(){string[] titles={"Ideias que merecem ficar","Hoje, com calma","Referências visuais"};string[] texts={"Um lugar leve para tirar ideias da cabeça.\n\n• Rascunhar a próxima pauta\n• Guardar uma boa referência\n• Voltar àquela ideia de ontem","Menos abas abertas. Mais clareza.\n\nEscolha o essencial para hoje e deixe o restante em outra área.","Imagens também fazem parte da ideia.\n\nCole capturas de tela, arraste fotos e mantenha tudo junto do contexto."};for(int i=0;i<3;i++){var n=Store.Create();n.Title=titles[i];n.Left=520+i*360;n.Top=160+(i%2)*45;var w=Get(n);w.Editor.Document.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(texts[i])));if(i==2){var pixels=new byte[320*150*4];for(int y=0;y<150;y++)for(int x=0;x<320;x++){int p=(y*320+x)*4;pixels[p]=(byte)(150+x/4);pixels[p+1]=(byte)(120+y/2);pixels[p+2]=(byte)(80+x/3);pixels[p+3]=255;}w.InsertImage(System.Windows.Media.Imaging.BitmapSource.Create(320,150,96,96,System.Windows.Media.PixelFormats.Bgra32,null,pixels,1280));}w.MarkDirty();w.Save();}}
}
