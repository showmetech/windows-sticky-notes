using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StickyNotes;
// Explicit development command; never runs against the installed user's notebook.
internal static class GoogleSyncLiveVerification {
 internal const string FixtureId="e7ed6719a2a84a23a603f8c40a29b225";
 internal static async Task Run(NotesApp app,string phase,string report){try{
  if(string.Equals(app.DataRoot,NotesApp.DefaultRoot,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Use uma pasta isolada para testar a sincronização Google.");
  if(phase is not ("seed" or "pull" or "edit" or "check-edit" or "trash" or "check-trash"))throw new ArgumentException("Fase de teste inválida.");
  var sync=app.Sync??throw new InvalidOperationException("Sincronização indisponível.");
  async Task SyncNow(){for(int attempt=0;sync.Busy&&attempt<600;attempt++)await Task.Delay(200);if(sync.Busy)throw new TimeoutException("Sincronização ocupada.");await sync.Sync();}
  if(phase=="seed"){var n=app.Store.Book.Notes.FirstOrDefault(n=>n.Id==FixtureId)??new Note{Id=FixtureId};if(!app.Store.Book.Notes.Contains(n))app.Store.Book.Notes.Add(n);n.Title="Teste de sincronização Google";n.Area="Teste de sincronização";n.Color="#39FF14";n.ColorMode="Neon";n.Deleted=false;if(!app.Store.Book.Areas.Contains(n.Area))app.Store.Book.Areas.Add(n.Area);var doc=new FlowDocument(new Paragraph(new Bold(new Run("Conteúdo sintético do desktop — acentuação"))));var image=BitmapSource.Create(32,16,96,96,PixelFormats.Bgra32,null,Enumerable.Repeat((byte)200,32*16*4).ToArray(),128);doc.Blocks.Add(new BlockUIContainer(new System.Windows.Controls.Image{Source=image,Width=200}));app.Store.SaveDocument(n,doc);}
  if(sync.Email==null)await sync.Connect(app.Widget);
  if(sync.Email==null)throw new InvalidOperationException("Teste aguardando conexão Google: "+sync.Status+"");
  if(phase is "edit" or "trash"){await SyncNow();var n=app.Store.Book.Notes.Single(n=>n.Id==FixtureId);if(phase=="edit"){var doc=new FlowDocument();app.Store.LoadDocument(n,doc);doc.Blocks.Add(new Paragraph(new Italic(new Run("Edição sintética recebida do notebook"))));n.Title="Teste Google · editado no notebook";app.Store.SaveDocument(n,doc);}else{app.Store.MoveToTrash(n,true);}}
  await SyncNow();if(!sync.Status.StartsWith("Sincronizado"))throw new InvalidOperationException(sync.Status);
  var note=app.Store.Book.Notes.Single(n=>n.Id==FixtureId);var restored=new FlowDocument();app.Store.LoadDocument(note,restored);var data=SyncDocument.Capture(note,restored);bool imagePresent=data.Blocks.Any(b=>b.Kind=="image");bool edited=phase is "edit" or "check-edit";bool deleted=phase is "trash" or "check-trash";if(!imagePresent||data.Color!="#39FF14"||data.ColorMode!="Neon"||data.Deleted!=deleted||!new TextRange(restored.ContentStart,restored.ContentEnd).Text.Contains(edited?"Edição sintética recebida do notebook":"Conteúdo sintético do desktop"))throw new InvalidDataException("Conteúdo recebido não corresponde à nota de teste.");
  Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(report))!);File.WriteAllBytes(report,SyncJson.Bytes(new{phase,passed=true,at=DateTimeOffset.Now,version=Distribution.Version,fixtureId=FixtureId,fingerprint=SyncJson.Hash(SyncJson.Bytes(data)),embeddedImage=imagePresent,notesInProfile=app.Store.Book.Notes.Count,limitations="Dois perfis isolados neste Windows; não são dois computadores físicos."}));
 }catch(Exception e){Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(report))!);File.WriteAllBytes(report,SyncJson.Bytes(new{phase,passed=false,error=e.Message,at=DateTimeOffset.Now}));}finally{app.ExitApp();}}
}
