using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace StickyNotes;
internal static class NoteEditorAppearance {
 // A note is already a colored surface. Fluent input hover/focus fills must not
 // paint another rectangle over it. Keep native editing, selection and scrolling.
 internal static ControlTemplate Template(string target)=>(ControlTemplate)XamlReader.Parse($$"""
 <ControlTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" TargetType="{{target}}">
  <Grid Background="Transparent">
   <ScrollViewer x:Name="PART_ContentHost" Margin="{TemplateBinding Padding}"/>
   <Border x:Name="Focus" BorderBrush="{TemplateBinding Foreground}" BorderThickness="0,0,0,1" Opacity="0" IsHitTestVisible="False"/>
  </Grid>
  <ControlTemplate.Triggers><Trigger Property="IsKeyboardFocusWithin" Value="True"><Setter TargetName="Focus" Property="Opacity" Value="0.4"/></Trigger></ControlTemplate.Triggers>
 </ControlTemplate>
 """);
 internal static void ClearBackgrounds(DependencyObject element){
  if(element is FlowDocument document&&Opaque(document.Background))document.Background=Brushes.Transparent;
  if(element is TextElement text&&Opaque(text.Background))text.Background=Brushes.Transparent;
  foreach(var child in LogicalTreeHelper.GetChildren(element).OfType<DependencyObject>().ToArray())ClearBackgrounds(child);
 }
 static bool Opaque(Brush? brush)=>brush!=null&&!(brush is SolidColorBrush solid&&solid.Color.A==0);
}
