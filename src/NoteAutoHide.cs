using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace StickyNotes;
// Group focus keeps editing, popups and owned dialogs intact. No global input hook.
internal sealed class NoteAutoHide:IDisposable {
 readonly NotesApp app;readonly Func<bool> overTray;
 readonly DispatcherTimer timer=new(){Interval=TimeSpan.FromMilliseconds(100)};
 long holdUntil;bool outsideProcessed;
 internal NoteAutoHide(NotesApp app,Func<bool> overTray,bool poll){this.app=app;this.overTray=overTray;timer.Tick+=(_,_)=>{if(Environment.TickCount64>=holdUntil)ObserveForeground(GetForegroundWindow(),overTray());};if(poll)timer.Start();}
 internal void Hold(){holdUntil=Environment.TickCount64+350;outsideProcessed=false;}
 internal void ObserveForeground(IntPtr foreground,bool pointerOverTray=false){
  if(foreground==IntPtr.Zero||app.Exiting)return;
  GetWindowThreadProcessId(foreground,out var process);
  if(process==0)return;
  if(process==Environment.ProcessId||SuiteActivation.MenuOpen||pointerOverTray){outsideProcessed=false;return;}
  if(outsideProcessed)return;
  outsideProcessed=true;
  foreach(var note in app.NoteWindows.Values.Where(w=>w.IsPresented&&!w.Note.Pinned).ToArray())note.Conceal();
 }
 public void Dispose()=>timer.Stop();
 [DllImport("user32.dll")]static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(IntPtr window,out uint process);
}
