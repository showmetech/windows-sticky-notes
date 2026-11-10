using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Documents;

namespace StickyNotes;
public sealed class Note {
 public string Id {get;set;}=Guid.NewGuid().ToString("N");
 public string Title {get;set;}="Nova nota";
 public string Preview {get;set;}="";
 public string Area {get;set;}="Minha mesa";
 public string Color {get;set;}="#F5D97E";
 public string ColorMode {get;set;}="Soft";
 public double Left {get;set;}=double.NaN;
 public double Top {get;set;}=double.NaN;
 public double Width {get;set;}=344;
 public double Height {get;set;}=380;
 public bool Pinned {get;set;}
 public bool Deleted {get;set;}
 public DateTimeOffset Updated {get;set;}=DateTimeOffset.Now;
}
public sealed class Notebook {
 public int Schema {get;set;}=1;
 public string Theme {get;set;}="Sistema";
 public bool WidgetPinned {get;set;}
 public string ActiveArea {get;set;}="Minha mesa";
 public int ColorSequence {get;set;}
 public List<string> Areas {get;set;}=new(){"Minha mesa"};
 public List<Note> Notes {get;set;}=new();
}
public sealed class Store {
 public readonly string Root;
 public Notebook Book {get;private set;}
 public string? RecoveryMessage {get;private set;}
 static readonly JsonSerializerOptions Json=new(){WriteIndented=true,NumberHandling=System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals};
 public static readonly string[] Palette={"#F5D97E","#B9B0ED","#A5D8BF","#A6CDEE","#EDAFC5","#F0BE95","#A8D8D5","#D1C3B4"};
 public Store(string root){Root=root;Directory.CreateDirectory(Root);Directory.CreateDirectory(Path.Combine(Root,"documents"));Book=Load();}
 Notebook Load(){var path=Path.Combine(Root,"notebook.json");if(!File.Exists(path))return new();try{return Read(path);}catch(Exception e) when(e is JsonException or IOException or InvalidDataException){
   var original=path+".unreadable-"+DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");File.Copy(path,original);
   if(File.Exists(path+".bak")){var b=Read(path+".bak");RecoveryMessage="O índice foi recuperado do backup. O arquivo original foi preservado.";return b;}
   throw new InvalidDataException("Não foi possível abrir suas notas. O arquivo original foi preservado; nenhum conteúdo foi substituído.",e);
 }}
 static Notebook Read(string path){var b=JsonSerializer.Deserialize<Notebook>(File.ReadAllText(path),Json)??throw new InvalidDataException("Índice vazio.");if(b.Schema!=1)throw new NotSupportedException("As notas foram criadas por uma versão mais recente. Atualize o aplicativo para abri-las.");if(b.Areas==null||b.Notes==null||b.Areas.Count==0||b.Notes.Any(n=>!Guid.TryParseExact(n.Id,"N",out _)))throw new InvalidDataException("Formato de índice inválido.");if(!b.Areas.Contains(b.ActiveArea))b.ActiveArea=b.Areas[0];return b;}
 public void Save()=>Atomic(Path.Combine(Root,"notebook.json"),System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Book,Json)));
 public bool MoveToTrash(Note note,bool confirmed){if(!confirmed||note.Deleted)return false;bool previous=note.Deleted;var updated=note.Updated;note.Deleted=true;note.Updated=DateTimeOffset.Now;try{Save();return true;}catch{note.Deleted=previous;note.Updated=updated;throw;}}
 public static void Atomic(string path,byte[] bytes){var temp=path+".tmp";using(var f=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None)){f.Write(bytes);f.Flush(true);}if(File.Exists(path))File.Replace(temp,path,path+".bak",true);else File.Move(temp,path);}
 public Note Create(){string color;var used=Book.Notes.Where(n=>!n.Deleted).Select(n=>n.Color).ToHashSet(StringComparer.OrdinalIgnoreCase);do{color=NextColor(Book.ColorSequence++);}while(used.Contains(color));var n=new Note{Area=Book.ActiveArea,Color=color};Book.Notes.Add(n);Save();return n;}
 public static string NextColor(int index){if(index<Palette.Length)return Palette[index];double h=(index*137.507764)%360,s=.50,l=.77;double c=(1-Math.Abs(2*l-1))*s,x=c*(1-Math.Abs(h/60%2-1)),m=l-c/2;var rgb=h switch{<60=>(c,x,0d),<120=>(x,c,0d),<180=>(0d,c,x),<240=>(0d,x,c),<300=>(x,0d,c),_=>(c,0d,x)};return $"#{(int)Math.Round((rgb.Item1+m)*255):X2}{(int)Math.Round((rgb.Item2+m)*255):X2}{(int)Math.Round((rgb.Item3+m)*255):X2}";}
 public string DocumentPath(Note n)=>Path.Combine(Root,"documents",n.Id+".note");
 public void SaveDocument(Note n,FlowDocument doc){using var ms=new MemoryStream();new TextRange(doc.ContentStart,doc.ContentEnd).Save(ms,DataFormats.XamlPackage);Atomic(DocumentPath(n),ms.ToArray());n.Preview=new TextRange(doc.ContentStart,doc.ContentEnd).Text.Trim();n.Updated=DateTimeOffset.Now;Save();}
 public void LoadDocument(Note n,FlowDocument doc){var path=DocumentPath(n);if(!File.Exists(path))return;try{using var stream=File.OpenRead(path);new TextRange(doc.ContentStart,doc.ContentEnd).Load(stream,DataFormats.XamlPackage);}catch(Exception e) when(e is IOException or System.Windows.Markup.XamlParseException or ArgumentException){if(!File.Exists(path+".bak"))throw;using var backup=File.OpenRead(path+".bak");new TextRange(doc.ContentStart,doc.ContentEnd).Load(backup,DataFormats.XamlPackage);File.Copy(path,path+".unreadable-"+DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));RecoveryMessage="Uma nota foi recuperada do backup. O original foi preservado.";}}
}
