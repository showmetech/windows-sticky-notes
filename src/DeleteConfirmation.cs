using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StickyNotes;
internal sealed class DeleteConfirmation:Window {
 public DeleteConfirmation(NoteWindow owner,string theme){Title="Excluir nota";Owner=owner;Topmost=owner.Topmost;WindowStartupLocation=WindowStartupLocation.CenterOwner;Visuals.Base(this,416,278);Width=System.Math.Min(416,NoteWindow.WorkArea(owner).Width-16);var surface=Visuals.Surface(this,Visuals.Dark(theme));var content=new StackPanel();surface.Child=content;
  var head=new DockPanel();var close=Visuals.Icon("\uE8BB","Cancelar exclusão",()=>DialogResult=false);DockPanel.SetDock(close,Dock.Right);head.Children.Add(close);var title=Visuals.Label("Excluir esta nota?",21);title.FontWeight=FontWeights.SemiBold;head.Children.Add(title);content.Children.Add(head);
  content.Children.Add(new TextBlock{Text=owner.Note.Title,TextTrimming=TextTrimming.CharacterEllipsis,Margin=new Thickness(0,12,0,6),FontWeight=FontWeights.SemiBold});content.Children.Add(Visuals.Label("Ela será movida para a lixeira e poderá ser restaurada em Notas e áreas.",14));
  var buttons=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(0,14,0,0)};var cancel=Visuals.TextButton("Cancelar",()=>DialogResult=false);cancel.IsCancel=true;cancel.IsDefault=true;buttons.Children.Add(cancel);var confirm=Visuals.TextButton("Excluir nota",()=>DialogResult=true);confirm.Background=Visuals.Brush("#C42B1C");confirm.Foreground=Brushes.White;buttons.Children.Add(confirm);content.Children.Add(buttons);Loaded+=(_,_)=>{var area=NoteWindow.WorkArea(this);Left=System.Math.Clamp(Left,area.Left,System.Math.Max(area.Left,area.Right-Width));Top=System.Math.Clamp(Top,area.Top,System.Math.Max(area.Top,area.Bottom-Height));cancel.Focus();};
 }
}
