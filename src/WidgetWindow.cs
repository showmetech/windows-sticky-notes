using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
namespace StickyNotes;
public sealed class WidgetWindow:Window {
 readonly NotesApp app;readonly Border surface;readonly StackPanel body=new();readonly TextBox search;readonly ComboBox areas=new();readonly Button pin,config;readonly TextBlock count=new();
 WindowRevealAnimation? animation;SettingsWindow? settings;bool refreshing,trash;readonly DispatcherTimer idle=new(){Interval=TimeSpan.FromSeconds(1)};long lastInput=Environment.TickCount64;
 internal bool SettingsVisible=>settings?.IsVisible==true;
 internal bool SettingsAligned=>settings!=null&&SuitePlacement.IsAbove(this,settings)&&SuiteChrome.Aligned(settings,true);
 internal SettingsWindow? SettingsPanel=>settings;
 internal void RefreshNotes()=>Render();
 internal void Refit()=>Fit();
 public WidgetWindow(NotesApp app){this.app=app;Title="Sticky Notes — Widget";Visuals.Base(this,360,600);ShowInTaskbar=Environment.GetCommandLineArgs().Contains("--inspect");surface=SuitePanels.Surface(this);var root=new DockPanel();surface.Child=root;
  var title=Visuals.Label("Sticky Notes",20);title.FontWeight=FontWeights.SemiBold;config=Visuals.Icon("\uE713","Configurações",()=>{if(SettingsVisible)CloseSettings();else OpenSettings();});pin=Visuals.Icon("\uE718","Manter widget aberto",()=>{app.Store.Book.WidgetPinned=!app.Store.Book.WidgetPinned;app.Store.Save();Refresh();});var close=Visuals.Icon("\uE8BB","Recolher widget para a bandeja",Conceal);
  var header=SuiteChrome.Header(title,Visuals.Icon("\uE710","Nova nota",()=>app.NewNote()),config,pin,close);DockPanel.SetDock(header,Dock.Top);root.Children.Add(header);
  var footer=new StackPanel{Margin=new Thickness(20,8,20,12)};count.FontSize=10;count.Opacity=.65;footer.Children.Add(count);DockPanel.SetDock(footer,Dock.Bottom);root.Children.Add(footer);
  var content=new StackPanel{Margin=new Thickness(20,0,20,0)};areas.MinHeight=34;System.Windows.Automation.AutomationProperties.SetName(areas,"Área ativa");areas.SelectionChanged+=(_,_)=>{if(!refreshing&&areas.SelectedItem is string a){app.SwitchArea(a);Refresh();}};content.Children.Add(areas);
  content.Children.Add(Visuals.Label("BUSCAR EM TODAS AS ÁREAS",10));search=Visuals.Input("Buscar notas");search.ToolTip="Buscar títulos e textos em todas as áreas";search.Padding=new Thickness(10,8,10,8);search.Margin=new Thickness(0,4,0,6);search.TextChanged+=(_,_)=>Render();content.Children.Add(search);
  var navigation=new System.Windows.Controls.Primitives.UniformGrid{Columns=2,Margin=new Thickness(0,0,0,8)};navigation.Children.Add(Visuals.TextButton("Notas",()=>{trash=false;Render();}));navigation.Children.Add(Visuals.TextButton("Lixeira",()=>{trash=true;Render();}));content.Children.Add(navigation);
  var actions=new WrapPanel();actions.Children.Add(Visuals.TextButton("Mostrar notas",app.ShowNotes));actions.Children.Add(Visuals.TextButton("Ocultar notas",app.HideNotes));actions.Children.Add(Visuals.TextButton("Organizar em grade",app.Arrange));content.Children.Add(actions);content.Children.Add(body);
  root.Children.Add(SuiteScroll.Wrap(new ScrollViewer{Content=content}));SourceInitialized+=(_,_)=>Fit();DpiChanged+=(_,_)=>Dispatcher.BeginInvoke(Fit);
  PreviewKeyDown+=(_,e)=>{if(e.Key==Key.N&&Keyboard.Modifiers==ModifierKeys.Control){app.NewNote();e.Handled=true;}else if(e.Key==Key.Escape){Conceal();e.Handled=true;}else if(e.Key==Key.OemComma&&Keyboard.Modifiers==ModifierKeys.Control){OpenSettings();e.Handled=true;}};
  Closing+=(_,e)=>{if(!app.Exiting){e.Cancel=true;Conceal();}};Closed+=(_,_)=>{animation?.Dispose();idle.Stop();};PreviewMouseMove+=(_,_)=>lastInput=Environment.TickCount64;PreviewMouseDown+=(_,_)=>lastInput=Environment.TickCount64;PreviewKeyDown+=(_,_)=>lastInput=Environment.TickCount64;
  idle.Tick+=(_,_)=>{if(IsVisible&&!app.Store.Book.WidgetPinned&&!SettingsVisible&&!SuiteActivation.MenuOpen&&!IsMouseCaptureWithin&&Environment.TickCount64-lastInput>20000&&!Environment.GetCommandLineArgs().Contains("--verify-live"))Conceal();};idle.Start();SuiteActivation.Attach(this,Conceal,()=>app.Store.Book.WidgetPinned||SettingsVisible);Refresh();
 }
 void Fit(){SuitePanels.Place(this);if(SettingsVisible){SuitePlacement.PrepareWidget(this);SuitePlacement.Above(this,settings!);}}
 public void Present(){animation?.Dispose();Refresh();Fit();Show();WindowState=WindowState.Normal;Activate();lastInput=Environment.TickCount64;SuiteActivation.Claim();if(SystemParameters.ClientAreaAnimation&&!SystemParameters.HighContrast)animation=new WindowRevealAnimation(this,true,()=>{});}
 public void Conceal(){CloseSettings(false);animation?.Dispose();if(IsVisible&&SystemParameters.ClientAreaAnimation&&!SystemParameters.HighContrast)animation=new WindowRevealAnimation(this,false,()=>Hide());else Hide();}
 public void OpenSettings(){if(SettingsVisible){settings!.Activate();return;}if(!IsVisible)Present();settings=new SettingsWindow(app,this);settings.Closed+=(_,_)=>{settings=null;Fit();Refresh();};SuitePlacement.PrepareWidget(this);SuitePlacement.Above(this,settings);settings.Present();Refresh();}
 internal void CloseSettings(bool animate=true){settings?.Dismiss(animate);}
 public void Refresh(bool unused=false){SuitePanels.Theme(this,surface,app.Store.Book.Theme);Topmost=app.Store.Book.WidgetPinned;pin.Background=Topmost?Visuals.Brush("#407F7FFF"):Brushes.Transparent;config.Background=SettingsVisible?Visuals.Brush("#407F7FFF"):Brushes.Transparent;search.Foreground=Foreground;search.Background=Visuals.Brush(Visuals.Dark(app.Store.Book.Theme)?"#383B43":"#E8EAF0");refreshing=true;areas.ItemsSource=app.Store.Book.Areas.ToArray();areas.SelectedItem=app.Store.Book.ActiveArea;refreshing=false;if(settings!=null){settings.Topmost=Topmost;settings.ApplyTheme();}Render();}
 void Render(){if(refreshing)return;body.Children.Clear();var q=search.Text.Trim();var notes=app.Store.Book.Notes.Where(n=>n.Deleted==trash&&(q.Length>0?(n.Title+" "+n.Preview).Contains(q,StringComparison.CurrentCultureIgnoreCase):trash||n.Area==app.Store.Book.ActiveArea)).OrderByDescending(n=>n.Updated).ToList();count.Text=(trash?"Lixeira":"Notas")+" · "+notes.Count;
  if(notes.Count==0)body.Children.Add(Visuals.Label(trash?"Nenhuma nota na lixeira.":"Sua mesa está livre. Use + para criar uma nota.",14));
  foreach(var n in notes){
   string title=string.IsNullOrWhiteSpace(n.Title)?"Sem título":n.Title.Replace('\r',' ').Replace('\n',' ');
   var tile=Visuals.TextButton(title,()=>OpenFromList(n));
   var color=(Color)ColorConverter.ConvertFromString(n.Color);
   tile.Content=new TextBlock{Text=title,FontSize=15,FontWeight=FontWeights.SemiBold,TextWrapping=TextWrapping.NoWrap,TextTrimming=TextTrimming.CharacterEllipsis};
   tile.HorizontalContentAlignment=HorizontalAlignment.Left;tile.Height=46;tile.Margin=new Thickness(0,5,0,5);tile.Padding=new Thickness(14,0,14,0);
   tile.Background=SystemParameters.HighContrast?SystemColors.ControlBrush:new SolidColorBrush(color);
   tile.Foreground=SystemParameters.HighContrast?SystemColors.ControlTextBrush:new SolidColorBrush(NoteColors.Text(color));
   tile.BorderBrush=tile.Foreground;tile.BorderThickness=new Thickness(SystemParameters.HighContrast?1:0);
   tile.ToolTip=trash?"Restaurar nota: "+title:title;
   // A stretching presenter lets long titles ellipsize inside the colored rectangle.
   tile.Template=(ControlTemplate)System.Windows.Markup.XamlReader.Parse("<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' TargetType='Button'><Border x:Name='Tile' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}' BorderThickness='{TemplateBinding BorderThickness}' CornerRadius='6'><ContentPresenter Margin='{TemplateBinding Padding}' VerticalAlignment='Center' HorizontalAlignment='Stretch'/></Border><ControlTemplate.Triggers><Trigger Property='IsMouseOver' Value='True'><Setter TargetName='Tile' Property='Opacity' Value='0.88'/></Trigger><Trigger Property='IsPressed' Value='True'><Setter TargetName='Tile' Property='Opacity' Value='0.72'/></Trigger><Trigger Property='IsKeyboardFocused' Value='True'><Setter TargetName='Tile' Property='BorderThickness' Value='2'/></Trigger></ControlTemplate.Triggers></ControlTemplate>");
   System.Windows.Automation.AutomationProperties.SetName(tile,(trash?"Restaurar nota: ":"Abrir nota: ")+title);tile.Tag=n.Id;body.Children.Add(tile);
  }
 }
 internal void OpenFromList(Note note){if(note.Deleted){note.Deleted=false;app.Store.Save();}app.ShowNote(note);Render();}
 internal Button[] NoteTiles=>body.Children.OfType<Button>().ToArray();
 internal void OpenTrayMenu(){var menu=new ContextMenu();void Add(string label,Action action){var item=new MenuItem{Header=label};item.Click+=(_,_)=>action();menu.Items.Add(item);}Add("Abrir widget",Present);Add("Mostrar notas",app.ShowNotes);Add("Ocultar notas",app.HideNotes);menu.Items.Add(new Separator());Add("Nova nota",()=>app.NewNote());Add("Organizar em grade",app.Arrange);Add("Configurações",()=>{Present();OpenSettings();});menu.Items.Add(new Separator());Add("Sair",app.ExitApp);menu.Opened+=(_,_)=>SuiteActivation.MenuOpen=true;menu.Closed+=(_,_)=>SuiteActivation.MenuOpen=false;menu.Placement=System.Windows.Controls.Primitives.PlacementMode.MousePoint;menu.IsOpen=true;}
}
internal static class SuitePanels {
 public static Border Surface(Window window){var border=new Border{CornerRadius=new CornerRadius(8),BorderThickness=new Thickness(1)};window.Content=border;return border;}
 public static void Theme(Window window,Border border,string theme){bool dark=Visuals.Dark(theme);var uri=new Uri("pack://application:,,,/PresentationFramework.Fluent;component/Themes/Fluent."+(dark?"Dark":"Light")+".xaml");if(SystemParameters.HighContrast)window.Resources.MergedDictionaries.Clear();else if(window.Resources.MergedDictionaries.Count==0||window.Resources.MergedDictionaries[0].Source!=uri){window.Resources.MergedDictionaries.Clear();window.Resources.MergedDictionaries.Add(new ResourceDictionary{Source=uri});}window.Foreground=SystemParameters.HighContrast?SystemColors.WindowTextBrush:Visuals.Brush(dark?"#F4F5F7":"#20242C");bool transparent=(int)(Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize","EnableTransparency",1)??1)!=0;byte alpha=!transparent?(byte)255:window is SettingsWindow?(byte)210:(byte)180;border.Background=SystemParameters.HighContrast?SystemColors.WindowBrush:new SolidColorBrush(dark?Color.FromArgb(alpha,32,32,32):Color.FromArgb(alpha,243,243,243));border.BorderBrush=SystemParameters.HighContrast?SystemColors.WindowTextBrush:Visuals.Brush("#20808080");UpdateButtons(border,window.Foreground,dark);}
 static void UpdateButtons(DependencyObject root,Brush foreground,bool dark){if(root is TextBlock label)label.Foreground=foreground;if(root is Expander expander)expander.Foreground=foreground;if(root is CheckBox check)check.Foreground=foreground;if(root is Button button){button.Foreground=foreground;if(button.Content is string)button.Background=SystemParameters.HighContrast?SystemColors.ControlBrush:Visuals.Brush(dark?"#3B3E47":"#E4E6ED");}if(root is TextBox input){input.Foreground=foreground;input.Background=SystemParameters.HighContrast?SystemColors.WindowBrush:Visuals.Brush(dark?"#383B43":"#E8EAF0");}for(int i=0;i<VisualTreeHelper.GetChildrenCount(root);i++)UpdateButtons(VisualTreeHelper.GetChild(root,i),foreground,dark);}
 public static void Place(Window window){var work=NoteWindow.WorkArea(window);window.Width=Math.Max(1,Math.Min(360,work.Width-24));window.Height=Math.Max(1,Math.Min(600,work.Height-24));window.Left=work.Right-window.Width-12;window.Top=work.Bottom-window.Height-12;}
}
