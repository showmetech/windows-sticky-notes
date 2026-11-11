using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace StickyNotes;
internal sealed class SyncController:IDisposable {
 readonly NotesApp app;readonly GoogleDriveSync drive;readonly DispatcherTimer timer=new(){Interval=TimeSpan.FromSeconds(30)};readonly CancellationTokenSource lifetime=new();CancellationTokenSource? operation;bool busy;SyncJournal? journal;
 internal event Action? Changed;
 internal string Status {get;private set;}="Conecte a mesma conta no desktop e no notebook.";
 internal string? Email=>drive.Session?.Email;
 internal bool Busy=>busy;
 internal bool Configured=>drive.Configured;
 internal SyncController(NotesApp app){this.app=app;drive=new(app.DataRoot);timer.Tick+=async(_,_)=>await Sync();timer.Start();if(drive.Session!=null)app.Dispatcher.BeginInvoke(async()=>await Sync());}
 void Set(string text){Status=text;Changed?.Invoke();}
 internal async Task Connect(Window owner){if(busy)return;busy=true;Changed?.Invoke();operation=CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);try{Set("Escolha sua conta no navegador…");var session=await drive.Authorize(operation.Token);if(MessageBox.Show(owner,"Sincronizar as notas deste computador com "+session.Email+"?\n\nTítulos, textos, imagens, cores e lixeira serão enviados à pasta privada do Sticky Notes no Google Drive e combinados com as notas dessa conta. Posições e tamanhos ficam separados em cada computador.\n\nEdições simultâneas serão preservadas como cópias em conflito.","Conectar conta Google",MessageBoxButton.OKCancel,MessageBoxImage.Question,MessageBoxResult.Cancel)!=MessageBoxResult.OK){Set("Conexão cancelada. Nenhuma nota foi enviada.");return;}drive.Accept(session);journal=null;Set("Conta conectada.");}catch(OperationCanceledException){Set("Conexão cancelada. As notas locais foram preservadas.");}catch(Exception e){Set(e.Message);}finally{busy=false;operation.Dispose();operation=null;Changed?.Invoke();}if(drive.Session!=null)await Sync();}
 internal void Cancel()=>operation?.Cancel();
 internal void Disconnect(){if(busy)return;drive.Disconnect();journal=null;Set("Conta desconectada neste computador. Notas locais e cópia no Google foram preservadas.");}
 List<SyncData> Capture(){if(!app.FlushAll())throw new IOException("Salve suas alterações antes de sincronizar.");var notes=new List<SyncData>();foreach(var n in app.Store.Book.Notes.Where(n=>!n.Welcome)){var doc=new FlowDocument();app.Store.LoadDocument(n,doc);var data=SyncDocument.Capture(n,doc);SyncJson.Validate(data);notes.Add(data);}return notes;}
 internal async Task Sync(){if(busy||drive.Session==null||lifetime.IsCancellationRequested)return;busy=true;operation=CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);var token=operation.Token;try{Set("Sincronizando…");journal??=new SyncJournal(Path.Combine(app.DataRoot,".sync",drive.AccountKey));journal.Capture(Capture());var files=await drive.List(token);foreach(var file in files.Where(p=>!journal.Revisions.ContainsKey(p.Key)))journal.Add(await drive.Download(file.Value,file.Key,token));
  // Capture again after network awaits: typing during a download becomes its own branch.
  journal.Capture(Capture());var resolved=journal.Resolve();foreach(var revision in journal.Revisions.ToArray().Where(p=>!files.ContainsKey(p.Key))){await drive.Upload(revision.Key,revision.Value,token);uploaded.Add(revision.Key);}
  // Uploads also yield to the editor. Resolve once more before touching any visible document.
  journal.Capture(Capture());resolved=journal.Resolve();foreach(var revision in journal.Revisions.ToArray().Where(p=>!files.ContainsKey(p.Key))) {if(!uploaded.Contains(revision.Key)){await drive.Upload(revision.Key,revision.Value,token);uploaded.Add(revision.Key);}}
  // No awaits between final capture, merge and local commit.
  token.ThrowIfCancellationRequested();journal.Capture(Capture());resolved=journal.Resolve();Apply(resolved);foreach(var item in resolved)journal.Baseline[item.Data.Id]=new(item.Head,SyncJson.Hash(SyncJson.Bytes(item.Data)));journal.Save();Set(journal.Revisions.Keys.Any(h=>!files.ContainsKey(h)&&!uploaded.Contains(h))?"Alterações salvas neste computador; envio pendente na próxima sincronização.":"Sincronizado · "+DateTime.Now.ToString("HH:mm")+" · "+Email);
 }catch(OperationCanceledException){Set("Sincronização interrompida. Alterações locais preservadas.");}catch(Exception e){Set("Sincronização pendente · "+e.Message);}finally{uploaded.Clear();busy=false;operation.Dispose();operation=null;Changed?.Invoke();}}
 readonly HashSet<string> uploaded=new();
 void Apply(List<(string Head,SyncData Data)> resolved){var changed=resolved.Where(item=>{var n=app.Store.Book.Notes.FirstOrDefault(n=>n.Id==item.Data.Id);if(n==null)return true;var doc=new FlowDocument();app.Store.LoadDocument(n,doc);return SyncJson.Hash(SyncJson.Bytes(SyncDocument.Capture(n,doc)))!=SyncJson.Hash(SyncJson.Bytes(item.Data));}).ToList();if(changed.Count==0)return;
  // Decode all cloud input before starting the local transaction.
  var documents=changed.ToDictionary(p=>p.Data.Id,p=>SyncDocument.Restore(p.Data));app.Store.SyncTransaction(()=>{foreach(var item in changed){var d=item.Data;var n=app.Store.Book.Notes.FirstOrDefault(n=>n.Id==d.Id);if(n==null){n=new Note{Id=d.Id};app.Store.Book.Notes.Add(n);}n.Title=d.Title;n.Area=d.Area;n.Color=d.Color;n.ColorMode=d.ColorMode;n.Pinned=d.Pinned;n.Deleted=d.Deleted;n.Welcome=false;if(!app.Store.Book.Areas.Contains(n.Area))app.Store.Book.Areas.Add(n.Area);app.Store.SaveDocument(n,documents[d.Id]);}foreach(var welcome in app.Store.Book.Notes.Where(n=>n.Welcome).ToList())app.Store.Book.Notes.Remove(welcome);app.Store.Save();});
  foreach(var item in changed)if(app.NoteWindows.TryGetValue(item.Data.Id,out var window)){window.ReloadSynced();if(item.Data.Deleted||item.Data.Area!=app.Store.Book.ActiveArea)window.Conceal();}foreach(var window in app.NoteWindows.Values.Where(w=>w.Note.Welcome))window.Conceal();app.RefreshWidget();
 }
 public void Dispose(){lifetime.Cancel();operation?.Cancel();timer.Stop();drive.Dispose();lifetime.Dispose();}
}
