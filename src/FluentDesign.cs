using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace StickyNotes;
// Semantic roles shared by the solid Fluent 2 surfaces. Note colors remain user data.
internal static class FluentDesign {
 internal const double ControlRadius=4,CardRadius=8,FlyoutRadius=12;
 internal static Brush Text(bool dark)=>SystemParameters.HighContrast?SystemColors.WindowTextBrush:Visuals.Brush(dark?"#FFFFFF":"#1A1A1A");
 internal static Brush Secondary(bool dark)=>SystemParameters.HighContrast?SystemColors.WindowTextBrush:Visuals.Brush(dark?"#D6D6D6":"#5C5C5C");
 internal static Brush Layer(bool dark)=>SystemParameters.HighContrast?SystemColors.ControlBrush:Visuals.Brush(dark?"#2B2B2B":"#FFFFFF");
 internal static Brush Control(bool dark)=>SystemParameters.HighContrast?SystemColors.ControlBrush:Visuals.Brush(dark?"#333333":"#FFFFFF");
 internal static Brush Stroke(bool dark)=>SystemParameters.HighContrast?SystemColors.WindowTextBrush:Visuals.Brush(dark?"#28FFFFFF":"#24000000");
 internal static Brush Accent(bool dark){if(SystemParameters.HighContrast)return SystemColors.HighlightBrush;var color=dark?SystemColors.AccentColorLight2:SystemColors.AccentColorDark1;var background=((SolidColorBrush)Layer(dark)).Color;for(int i=0;i<12&&NoteColors.Contrast(color,background)<4.5;i++)color=Mix(color,dark?Colors.White:Colors.Black,.12);return new SolidColorBrush(color);}
 internal static Brush Selected(bool dark){if(SystemParameters.HighContrast)return SystemColors.HighlightBrush;return new SolidColorBrush(Mix(((SolidColorBrush)Layer(dark)).Color,((SolidColorBrush)Accent(dark)).Color,.12));}
 internal static Brush SelectedText(bool dark)=>SystemParameters.HighContrast?SystemColors.HighlightTextBrush:Text(dark);
 static Color Mix(Color from,Color to,double amount)=>Color.FromRgb((byte)Math.Round(from.R+(to.R-from.R)*amount),(byte)Math.Round(from.G+(to.G-from.G)*amount),(byte)Math.Round(from.B+(to.B-from.B)*amount));
 internal static void Select(Button button,bool selected,bool dark){button.Background=selected?Selected(dark):Control(dark);button.Foreground=selected?SelectedText(dark):Text(dark);button.BorderBrush=selected?Accent(dark):Stroke(dark);button.BorderThickness=new Thickness(selected?2:1);}
 internal static void ThemeControls(Window window,bool dark){
  if(SystemParameters.HighContrast){window.Resources.MergedDictionaries.Clear();return;}
  var uri=new Uri("pack://application:,,,/PresentationFramework.Fluent;component/Themes/Fluent."+(dark?"Dark":"Light")+".xaml");
  if(window.Resources.MergedDictionaries.Count==0||window.Resources.MergedDictionaries[0].Source!=uri){window.Resources.MergedDictionaries.Clear();window.Resources.MergedDictionaries.Add(new ResourceDictionary{Source=uri});}
 }
 // States overlay the fill rather than fading the text. The inner ring stays within layout.
 internal static ControlTemplate ButtonTemplate()=> (ControlTemplate)XamlReader.Parse("""
 <ControlTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" TargetType="Button">
  <Grid>
   <Border Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}" BorderThickness="{TemplateBinding BorderThickness}" CornerRadius="4"/>
   <Border x:Name="State" Background="{TemplateBinding Foreground}" Opacity="0" CornerRadius="4" IsHitTestVisible="False"/>
   <ContentPresenter Margin="{TemplateBinding Padding}" HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}" VerticalAlignment="Center"/>
   <Border x:Name="Focus" BorderBrush="{TemplateBinding Foreground}" BorderThickness="2" CornerRadius="3" Margin="2" Visibility="Collapsed" IsHitTestVisible="False"/>
  </Grid>
  <ControlTemplate.Triggers>
   <Trigger Property="IsMouseOver" Value="True"><Setter TargetName="State" Property="Opacity" Value="0.06"/></Trigger>
   <Trigger Property="IsPressed" Value="True"><Setter TargetName="State" Property="Opacity" Value="0.12"/></Trigger>
   <Trigger Property="IsKeyboardFocused" Value="True"><Setter TargetName="Focus" Property="Visibility" Value="Visible"/></Trigger>
   <Trigger Property="IsEnabled" Value="False"><Setter Property="Opacity" Value="0.4"/></Trigger>
  </ControlTemplate.Triggers>
 </ControlTemplate>
 """);
}
