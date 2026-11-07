using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StickyNotes;
internal static class SelfTests {
 public static int Run(string[] args){string root=Path.Combine(Path.GetTempPath(),"WindowsStickyNotes-tests-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);var results=new List<object>();int failures=0;
  void Check(string name,Action action){try{action();results.Add(new{name,passed=true});}catch(Exception e){failures++;results.Add(new{name,passed=false,error=e.ToString()});}}
  void Assert(bool ok,string why){if(!ok)throw new Exception(why);}
  var app=(NotesApp)Application.Current;app.Store=new Store(root);app.DataRoot=root;Note? note=null;
  Check("Automatic colors: first 32 unique",()=>Assert(Enumerable.Range(0,32).Select(Store.NextColor).Distinct().Count()==32,"Repeated automatic color"));
  Check("New color also avoids manually chosen colors",()=>{var colors=new Store(Path.Combine(root,"colors"));var a=colors.Create();a.Color=Store.Palette[1];colors.Save();var b=colors.Create();Assert(a.Color!=b.Color,"Manual color was repeated");});
  Check("Create, title, area and position persist",()=>{note=app.Store.Create();note.Title="Teste com acentuação — ideia";note.Left=-120;note.Top=72;app.Store.Book.Areas.Add("Projetos");note.Area="Projetos";app.Store.Save();var s=new Store(root);var n=s.Book.Notes.Single();Assert(n.Title==note.Title&&n.Left==-120&&n.Area=="Projetos","Metadata did not round-trip");});
  NoteWindow? window=null;
  Check("Native rich text and embedded image survive reopen",()=>{window=new NoteWindow(app,note!);window.Editor.Document.Blocks.Add(new Paragraph(new Bold(new Run("Texto salvo com imagem"))));byte[] pixels=Enumerable.Repeat((byte)255,80*40*4).ToArray();window.InsertImage(BitmapSource.Create(80,40,96,96,PixelFormats.Bgra32,null,pixels,320));Assert(window.Save(),"Save failed");var doc=new FlowDocument();app.Store.LoadDocument(note!,doc);Assert(new TextRange(doc.ContentStart,doc.ContentEnd).Text.Contains("Texto salvo com imagem"),"Text missing");var block=doc.Blocks.OfType<BlockUIContainer>().Single();Assert(block.Child is System.Windows.Controls.Image im&&im.Source is BitmapSource bm&&bm.PixelWidth==80&&bm.PixelHeight==40,"Embedded image missing: "+(block.Child is System.Windows.Controls.Image img?img.Source?.ToString():block.Child?.GetType().Name));});
  Check("Color, pin, resized window and no taskbar entry",()=>{window!.TitleBox.Text="Título editado";note!.Color="#A6CDEE";note.Pinned=true;window.ApplyColor();window.Width=410;window.Height=420;window.MarkDirty();Assert(window.Save(),"Save failed");Assert(!window.ShowInTaskbar,"Unexpected taskbar entry");Assert(new Store(root).Book.Notes.Single().Color=="#A6CDEE","Color not persisted");});
  Check("Imported PNG remains after source file is removed",()=>{string path=Path.Combine(root,"source.png");var enc=new PngBitmapEncoder();enc.Frames.Add(BitmapFrame.Create(BitmapSource.Create(40,20,96,96,PixelFormats.Bgra32,null,Enumerable.Repeat((byte)220,40*20*4).ToArray(),160)));using(var file=File.Create(path))enc.Save(file);window!.AddImageFile(path);Assert(window.Save(),"Save failed");File.Delete(path);var doc=new FlowDocument();app.Store.LoadDocument(note!,doc);Assert(doc.Blocks.OfType<BlockUIContainer>().Count()==2,"Imported image depended on source file");});
  Check("Archive and restore retain document",()=>{note!.Deleted=true;app.Store.Save();Assert(new Store(root).Book.Notes.Single().Deleted,"Archive missing");note.Deleted=false;app.Store.Save();Assert(File.Exists(app.Store.DocumentPath(note)),"Document lost");});
  Check("Document backup recovers corrupt current file",()=>{window!.MarkDirty();Assert(window.Save(),"Save failed");File.WriteAllText(app.Store.DocumentPath(note!),"broken package");var d=new FlowDocument();app.Store.LoadDocument(note!,d);Assert(new TextRange(d.ContentStart,d.ContentEnd).Text.Contains("Texto salvo"),"Backup not recovered");});
  Check("Atomic metadata backup recovers invalid JSON",()=>{app.Store.Save();File.WriteAllText(Path.Combine(root,"notebook.json"),"{broken");var recovered=new Store(root);Assert(recovered.Book.Notes.Count==1&&recovered.RecoveryMessage!=null,"Index backup not recovered");});
  Check("Invalid index without backup fails without overwrite",()=>{string dir=Path.Combine(root,"invalid");Directory.CreateDirectory(dir);string path=Path.Combine(dir,"notebook.json");File.WriteAllText(path,"broken");try{_ =new Store(dir);throw new Exception("Corruption silently ignored");}catch(InvalidDataException){Assert(File.ReadAllText(path)=="broken","Original overwritten");}});
  Check("Future schema is preserved and rejected even with old backup",()=>{string dir=Path.Combine(root,"future");Directory.CreateDirectory(dir);File.WriteAllText(Path.Combine(dir,"notebook.json"),"{\"Schema\":42}");File.WriteAllText(Path.Combine(dir,"notebook.json.bak"),"{\"Schema\":1}");try{_=new Store(dir);throw new Exception("Future schema accepted");}catch(NotSupportedException){Assert(File.ReadAllText(Path.Combine(dir,"notebook.json"))=="{\"Schema\":42}","Future data overwritten");}});
  Check("Write failure stays dirty and prevents closing",()=>{var blocked=new Store(Path.Combine(root,"blocked"));var n=blocked.Create();var previous=app.Store;app.Store=blocked;var w=new NoteWindow(app,n);app.Store=previous;string dest=previous.DocumentPath(n);Directory.CreateDirectory(dest);w.MarkDirty();Assert(!w.Save(),"Write error ignored");w.Conceal();Assert(Directory.Exists(dest),"Test setup unexpectedly mutated");});
  int p=Array.IndexOf(args,"--report");string report=p>=0&&p+1<args.Length?args[p+1]:Path.Combine(root,"report.json");Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(report))!);File.WriteAllText(report,JsonSerializer.Serialize(new{at=DateTimeOffset.Now,version=Distribution.Version,processArchitecture=System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),osArchitecture=System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString(),packaged=Distribution.IsPackaged,passed=results.Count-failures,failed=failures,root,results},new JsonSerializerOptions{WriteIndented=true}));return failures==0?0:1;
 }
}
