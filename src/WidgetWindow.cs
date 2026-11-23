using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
namespace StickyNotes;
public sealed class WidgetWindow:Window {
 readonly NotesApp app;readonly Border surface;readonly StackPanel body=new();readonly TextBox search;readonly ComboBox areas=new();readonly Button pin,config,notesTab,trashTab,showNotes,hideNotes;readonly Border areaField,searchField;readonly TextBlock searchHint,listCaption,areaGlyph,searchGlyph;readonly TextBlock count=new();
 WindowRevealAnimation? animation;SettingsWindow? settings;bool refreshing,trash;readonly DispatcherTimer idle=new(){Interval=TimeSpan.FromSeconds(1)};long lastInput=Environment.TickCount64;
 internal bool SettingsVisible=>settings?.IsVisible==true;
 internal bool SettingsAligned=>settings!=null&&SuitePlacement.IsAbove(this,settings)&&SuiteChrome.Aligned(settings,true);
 internal SettingsWindow? SettingsPanel=>settings;
 internal Button AutoArrangeButton {get;}
 internal TextBox SearchInput=>search;
 internal bool SearchHintVisible=>searchHint.Visibility==Visibility.Visible;
 internal void RefreshNotes()=>Render();
 internal void Refit()=>Fit();
 public WidgetWindow(NotesApp app){this.app=app;Title=Distribution.DisplayName+" — Widget";Visuals.Base(this,360,600);ShowInTaskbar=Environment.GetCommandLineArgs().Contains("--inspect");surface=SuitePanels.Surface(this);var root=new DockPanel();surface.Child=root;
  var title=Visuals.Label(Distribution.DisplayName,18);title.FontWeight=FontWeights.SemiBold;config=Visuals.Icon("\uE713","Configurações",()=>{if(SettingsVisible)CloseSettings();else OpenSettings();});pin=Visuals.Icon("\uE718","Manter widget aberto",()=>{app.Store.Book.WidgetPinned=!app.Store.Book.WidgetPinned;app.Store.Save();Refresh();});var close=Visuals.Icon("\uE921","Recolher widget para a bandeja",Conceal);
  var header=SuiteChrome.Header(title,Visuals.Icon("\uE710","Nova nota",()=>app.NewNote()),config,pin,close);DockPanel.SetDock(header,Dock.Top);root.Children.Add(header);
  var footer=new Border{Height=8};DockPanel.SetDock(footer,Dock.Bottom);root.Children.Add(footer);
  var content=new StackPanel{Margin=new Thickness(20,0,20,0)};
  areas.MinHeight=34;areas.BorderThickness=new Thickness(0);areas.Padding=new Thickness(6,3,6,3);areas.FontSize=13;System.Windows.Automation.AutomationProperties.SetName(areas,"Área ativa");areas.SelectionChanged+=(_,_)=>{if(!refreshing&&areas.SelectedItem is string a){app.SwitchArea(a);Refresh();}};
  var areaRow=new Grid();areaRow.ColumnDefinitions.Add(new(){Width=GridLength.Auto});areaRow.ColumnDefinitions.Add(new());areaGlyph=Glyph("\uE8B7",16);areaGlyph.Margin=new Thickness(12,0,2,0);areaRow.Children.Add(areaGlyph);Grid.SetColumn(areas,1);areaRow.Children.Add(areas);areaField=new Border{CornerRadius=new CornerRadius(4),Height=38,Margin=new Thickness(0,0,0,6),Child=areaRow};content.Children.Add(areaField);
  search=Visuals.Input("Buscar notas");search.ToolTip="Buscar títulos e textos em todas as áreas";search.Padding=new Thickness(0);search.Margin=new Thickness(36,0,10,0);search.FontSize=13;
  var searchRow=new Grid();searchGlyph=Glyph("\uE721",15);searchGlyph.HorizontalAlignment=HorizontalAlignment.Left;searchGlyph.Margin=new Thickness(12,0,0,0);searchRow.Children.Add(searchGlyph);searchHint=new TextBlock{Text="Buscar em todas as áreas",FontSize=12,Opacity=.7,Margin=new Thickness(36,0,8,0),VerticalAlignment=VerticalAlignment.Center,IsHitTestVisible=false};searchRow.Children.Add(searchHint);searchRow.Children.Add(search);searchField=new Border{CornerRadius=new CornerRadius(4),Height=38,Margin=new Thickness(0,0,0,8),Child=searchRow};content.Children.Add(searchField);search.TextChanged+=(_,_)=>{searchHint.Visibility=search.Text.Length==0?Visibility.Visible:Visibility.Collapsed;Render();};
  var navigation=new System.Windows.Controls.Primitives.UniformGrid{Columns=2,Height=36,Margin=new Thickness(-3,0,-3,8)};
  notesTab=PanelButton("\uE8A5","Notas","Notas",()=>{trash=false;Render();});trashTab=PanelButton("\uE74D","Lixeira","Lixeira",()=>{trash=true;Render();});navigation.Children.Add(notesTab);navigation.Children.Add(trashTab);content.Children.Add(navigation);
  var actions=new System.Windows.Controls.Primitives.UniformGrid{Columns=3,Height=52,Margin=new Thickness(-3,0,-3,8)};
  showNotes=PanelButton("\uE890","Mostrar","Mostrar notas",app.ShowNotes,true);hideNotes=PanelButton("\uE921","Ocultar","Ocultar notas",app.HideNotes,true);AutoArrangeButton=PanelButton("\uE8A9","Auto-organizar","Auto-organizar",app.Arrange,true);AutoArrangeButton.ToolTip="Alinhar notas em colunas à direita • Ctrl+Shift+O";System.Windows.Automation.AutomationProperties.SetAutomationId(AutoArrangeButton,"Notes.AutoArrange");actions.Children.Add(showNotes);actions.Children.Add(hideNotes);actions.Children.Add(AutoArrangeButton);content.Children.Add(actions);
  var heading=new DockPanel{Height=20,Margin=new Thickness(0,0,0,3)};count.FontSize=11;count.Opacity=.7;count.VerticalAlignment=VerticalAlignment.Center;DockPanel.SetDock(count,Dock.Right);heading.Children.Add(count);listCaption=new TextBlock{Text="Suas notas",FontSize=12,FontWeight=FontWeights.SemiBold,VerticalAlignment=VerticalAlignment.Center};heading.Children.Add(listCaption);content.Children.Add(heading);content.Children.Add(body);
  root.Children.Add(SuiteScroll.Wrap(new ScrollViewer{Content=content}));SourceInitialized+=(_,_)=>Fit();DpiChanged+=(_,_)=>Dispatcher.BeginInvoke(Fit);
  PreviewKeyDown+=(_,e)=>{if(e.Key==Key.O&&Keyboard.Modifiers==(ModifierKeys.Control|ModifierKeys.Shift)){app.Arrange();e.Handled=true;}else if(e.Key==Key.N&&Keyboard.Modifiers==ModifierKeys.Control){app.NewNote();e.Handled=true;}else if(e.Key==Key.Escape){Conceal();e.Handled=true;}else if(e.Key==Key.OemComma&&Keyboard.Modifiers==ModifierKeys.Control){OpenSettings();e.Handled=true;}};
  Closing+=(_,e)=>{if(!app.Exiting){e.Cancel=true;Conceal();}};Closed+=(_,_)=>{animation?.Dispose();idle.Stop();};PreviewMouseMove+=(_,_)=>lastInput=Environment.TickCount64;PreviewMouseDown+=(_,_)=>lastInput=Environment.TickCount64;PreviewKeyDown+=(_,_)=>lastInput=Environment.TickCount64;
  idle.Tick+=(_,_)=>{if(IsVisible&&!app.Store.Book.WidgetPinned&&!SettingsVisible&&!SuiteActivation.MenuOpen&&!IsMouseCaptureWithin&&Environment.TickCount64-lastInput>20000&&!Environment.GetCommandLineArgs().Contains("--verify-live"))Conceal();};idle.Start();SuiteActivation.Attach(this,Conceal,()=>app.Store.Book.WidgetPinned||SettingsVisible);Refresh();
 }
 static TextBlock Glyph(string text,double size)=>new(){Text=text,FontFamily=new FontFamily("Segoe Fluent Icons"),FontSize=size,VerticalAlignment=VerticalAlignment.Center,HorizontalAlignment=HorizontalAlignment.Center};
 static Button PanelButton(string glyph,string label,string name,Action action,bool vertical=false){
  var button=Visuals.TextButton(label,action);button.Margin=new Thickness(3,0,3,0);button.MinHeight=vertical?52:36;button.VerticalAlignment=VerticalAlignment.Stretch;button.Padding=new Thickness(4,4,4,4);button.ToolTip=name;
  System.Windows.Automation.AutomationProperties.SetName(button,name);
  var panel=new StackPanel{Orientation=vertical?Orientation.Vertical:Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Center};var icon=Glyph(glyph,vertical?17:14);icon.Margin=vertical?new Thickness(0,0,0,3):new Thickness(0,0,7,0);panel.Children.Add(icon);panel.Children.Add(new TextBlock{Text=label,FontSize=vertical?11:12,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center});button.Content=panel;return button;
 }
 void StyleActions(){
  bool dark=Visuals.Dark(app.Store.Book.Theme);
  foreach(var button in new[]{notesTab,trashTab,showNotes,hideNotes,AutoArrangeButton}){bool active=button==notesTab&&!trash||button==trashTab&&trash;FluentDesign.Select(button,active,dark);if(button.Content is Panel panel)foreach(var text in panel.Children.OfType<TextBlock>())text.Foreground=button.Foreground;}

 }
 internal bool PanelTextReadable(){
  if(SystemParameters.HighContrast)return true;
  bool Readable(TextBlock text,Border backdrop){if(text.Foreground is not SolidColorBrush foreground||backdrop.Background is not SolidColorBrush background)return false;var fg=foreground.Color;var bg=background.Color;double opacity=text.Opacity*fg.A/255d;var effective=Color.FromRgb((byte)Math.Round(fg.R*opacity+bg.R*(1-opacity)),(byte)Math.Round(fg.G*opacity+bg.G*(1-opacity)),(byte)Math.Round(fg.B*opacity+bg.B*(1-opacity)));return NoteColors.Contrast(bg,effective)>=4.5;}
  return Readable(searchHint,searchField)&&Readable(listCaption,surface)&&Readable(count,surface);
 }
 internal bool PanelActionsAligned(){UpdateLayout();var buttons=new[]{showNotes,hideNotes,AutoArrangeButton};var positions=buttons.Select(b=>b.TranslatePoint(new Point(),this)).ToArray();return buttons.All(b=>b.ActualWidth>=80&&b.ActualHeight>=48&&b.Content is Panel p&&p.Children.OfType<TextBlock>().Count()==2)&&positions.All(p=>Math.Abs(p.Y-positions[0].Y)<1)&&buttons.All(b=>Math.Abs(b.ActualWidth-buttons[0].ActualWidth)<1);}
 void Fit(){SuitePanels.Place(this);if(SettingsVisible){SuitePlacement.PrepareWidget(this);SuitePlacement.Above(this,settings!);}}
 public void Present(){animation?.Dispose();Refresh();Fit();Show();WindowState=WindowState.Normal;Activate();lastInput=Environment.TickCount64;SuiteActivation.Claim();if(SystemParameters.ClientAreaAnimation&&!SystemParameters.HighContrast)animation=new WindowRevealAnimation(this,true,()=>{});}
 public void Conceal(){CloseSettings(false);animation?.Dispose();if(IsVisible&&SystemParameters.ClientAreaAnimation&&!SystemParameters.HighContrast)animation=new WindowRevealAnimation(this,false,()=>Hide());else Hide();}
 public void OpenSettings(){if(SettingsVisible){settings!.Activate();return;}if(!IsVisible)Present();settings=new SettingsWindow(app,this);settings.Closed+=(_,_)=>{settings=null;Fit();Refresh();};SuitePlacement.PrepareWidget(this);SuitePlacement.Above(this,settings);settings.Present();Refresh();}
 internal void CloseSettings(bool animate=true){settings?.Dismiss(animate);}
 public void Refresh(bool unused=false){SuitePanels.Theme(this,surface,app.Store.Book.Theme);Topmost=app.Store.Book.WidgetPinned;pin.Background=Topmost?FluentDesign.Selected(Visuals.Dark(app.Store.Book.Theme)):Brushes.Transparent;config.Background=SettingsVisible?FluentDesign.Selected(Visuals.Dark(app.Store.Book.Theme)):Brushes.Transparent;foreach(var label in new[]{areaGlyph,searchGlyph,searchHint,listCaption,count})label.Foreground=Foreground;areas.Foreground=Foreground;search.Foreground=Foreground;search.Background=Brushes.Transparent;areas.Background=Brushes.Transparent;foreach(var field in new[]{areaField,searchField}){field.Background=SystemParameters.HighContrast?SystemColors.ControlBrush:FluentDesign.Control(Visuals.Dark(app.Store.Book.Theme));field.BorderBrush=SystemParameters.HighContrast?SystemColors.ControlTextBrush:FluentDesign.Stroke(Visuals.Dark(app.Store.Book.Theme));field.BorderThickness=new Thickness(1);}refreshing=true;areas.ItemsSource=app.Store.Book.Areas.ToArray();areas.SelectedItem=app.Store.Book.ActiveArea;refreshing=false;if(settings!=null){settings.Topmost=Topmost;settings.ApplyTheme();}Render();}
 void Render(){if(refreshing)return;body.Children.Clear();var q=search.Text.Trim();var notes=app.Store.Book.Notes.Where(n=>n.Deleted==trash&&(q.Length>0?(n.Title+" "+n.Preview).Contains(q,StringComparison.CurrentCultureIgnoreCase):trash||n.Area==app.Store.Book.ActiveArea)).OrderByDescending(n=>n.Updated).ToList();count.Text=notes.Count.ToString();listCaption.Text=trash?"Na lixeira":"Suas notas";StyleActions();
  if(notes.Count==0)body.Children.Add(Visuals.Label(trash?"Nenhuma nota na lixeira.":"Sua mesa está livre. Use + para criar uma nota.",14));
  foreach(var n in notes){
   string title=string.IsNullOrWhiteSpace(n.Title)?"Sem título":n.Title.Replace('\r',' ').Replace('\n',' ');
   var tile=Visuals.TextButton(title,()=>OpenFromList(n));
   var color=(Color)ColorConverter.ConvertFromString(n.Color);
   tile.Content=new TextBlock{Text=title,FontSize=14,FontWeight=FontWeights.SemiBold,TextWrapping=TextWrapping.NoWrap,TextTrimming=TextTrimming.CharacterEllipsis};
   tile.HorizontalContentAlignment=HorizontalAlignment.Left;tile.Height=46;tile.Margin=new Thickness(0,3,0,3);tile.Padding=new Thickness(14,0,14,0);
   tile.Background=SystemParameters.HighContrast?SystemColors.ControlBrush:new SolidColorBrush(color);
   tile.Foreground=SystemParameters.HighContrast?SystemColors.ControlTextBrush:new SolidColorBrush(NoteColors.Text(color));
   tile.BorderBrush=tile.Foreground;tile.BorderThickness=new Thickness(SystemParameters.HighContrast?1:0);
   tile.ToolTip=trash?"Restaurar nota: "+title:title;
   // A stretching presenter lets long titles ellipsize inside the colored rectangle.
   tile.HorizontalContentAlignment=HorizontalAlignment.Stretch;tile.Template=FluentDesign.ButtonTemplate();
   System.Windows.Automation.AutomationProperties.SetName(tile,(trash?"Restaurar nota: ":"Abrir nota: ")+title);tile.Tag=n.Id;body.Children.Add(tile);
  }
 }
 internal void OpenFromList(Note note){if(note.Deleted){note.Deleted=false;app.Store.Save();}app.ShowNote(note);Render();}
 internal Button[] NoteTiles=>body.Children.OfType<Button>().ToArray();
 internal void OpenTrayMenu(){var menu=new ContextMenu{Foreground=Foreground};FluentDesign.ThemeControls(this,Visuals.Dark(app.Store.Book.Theme));foreach(var dictionary in Resources.MergedDictionaries)menu.Resources.MergedDictionaries.Add(dictionary);void Add(string label,Action action){var item=new MenuItem{Header=label};item.Click+=(_,_)=>action();menu.Items.Add(item);}Add("Abrir widget",Present);Add("Mostrar notas",app.ShowNotes);Add("Ocultar notas",app.HideNotes);menu.Items.Add(new Separator());Add("Nova nota",()=>app.NewNote());Add("Auto-organizar notas",app.Arrange);Add("Configurações",()=>{Present();OpenSettings();});menu.Items.Add(new Separator());Add("Sair",app.ExitApp);menu.Opened+=(_,_)=>SuiteActivation.MenuOpen=true;menu.Closed+=(_,_)=>SuiteActivation.MenuOpen=false;menu.Placement=System.Windows.Controls.Primitives.PlacementMode.MousePoint;menu.IsOpen=true;}
}
internal static class SuitePanels {
 public static Border Surface(Window window){var border=new Border{CornerRadius=new CornerRadius(8),BorderThickness=new Thickness(1)};window.Content=border;return border;}
 public static void Theme(Window window,Border border,string theme){bool dark=Visuals.Dark(theme);FluentDesign.ThemeControls(window,dark);window.Foreground=FluentDesign.Text(dark);byte alpha=Visuals.BackgroundAlpha;border.Background=SystemParameters.HighContrast?SystemColors.WindowBrush:new SolidColorBrush(dark?Color.FromArgb(alpha,32,32,32):Color.FromArgb(alpha,243,243,243));border.BorderBrush=FluentDesign.Stroke(dark);UpdateButtons(border,window.Foreground,dark);}

 static void UpdateButtons(DependencyObject root,Brush foreground,bool dark){if(root is TextBlock label)label.Foreground=foreground;if(root is Expander expander)expander.Foreground=foreground;if(root is CheckBox check)check.Foreground=foreground;if(root is Button button){button.Foreground=foreground;if(button.Content is string)button.Background=SystemParameters.HighContrast?SystemColors.ControlBrush:FluentDesign.Control(dark);}if(root is TextBox input){input.Foreground=foreground;input.Background=SystemParameters.HighContrast?SystemColors.WindowBrush:FluentDesign.Control(dark);}for(int i=0;i<VisualTreeHelper.GetChildrenCount(root);i++)UpdateButtons(VisualTreeHelper.GetChild(root,i),foreground,dark);}
 public static void Place(Window window){var work=NoteWindow.WorkArea(window);window.Width=Math.Max(1,Math.Min(360,work.Width-24));window.Height=Math.Max(1,Math.Min(600,work.Height-24));window.Left=work.Right-window.Width-12;window.Top=work.Bottom-window.Height-12;}
}
