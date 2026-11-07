using System;
using System.Windows.Media;

namespace StickyNotes;
internal static class NoteColors {
 public static readonly string[] Normal={"#E76F51","#F4A261","#E9C46A","#A9C46C","#76B58A","#5BAFA8","#6499C9","#867BC5","#B17AB3","#D982A6"};
 public static readonly string[] Pastel={"#EDAFC5","#F0BE95","#F5D97E","#D8E5AD","#A5D8BF","#A8D8D5","#A6CDEE","#B9B0ED","#F2BDD9","#D1C3B4"};
 public static readonly string[] Vivid={"#EF4444","#FB5607","#FFBE0B","#A3C900","#00B86B","#00B8D9","#2463EB","#8338EC","#C026D3","#FF006E"};
 public static readonly string[] Neon={"#FF3131","#FF5F1F","#FFF700","#CCFF00","#39FF14","#00FFCC","#00E5FF","#7A00FF","#BF5FFF","#FF00D4"};
 public static readonly string[] Earth={"#B65C46","#C98A57","#BB9B49","#92985D","#628568","#58817E","#647C91","#81708D","#A87886","#8F8175"};
 public static readonly (string Mode,string Name,string[] Colors)[] Rows={("Normal","Normal",Normal),("Pastel","Pastel",Pastel),("Vivid","Vivas",Vivid),("Neon","Neon",Neon),("Earth","Terrosas",Earth)};
 public static string[] Palette(string mode)=>mode switch{"Normal"=>Normal,"Pastel"=>Pastel,"Vivid"=>Vivid,"Neon"=>Neon,"Earth"=>Earth,_=>Store.Palette};
 public static Color Surface(string hex,string mode,bool dark){var c=(Color)ColorConverter.ConvertFromString(hex);if(mode is "Normal" or "Pastel" or "Vivid" or "Neon" or "Earth")return c;return dark?Color.FromRgb((byte)(24+c.R*.13),(byte)(26+c.G*.13),(byte)(29+c.B*.13)):Color.FromRgb((byte)(c.R*.38+255*.62),(byte)(c.G*.38+255*.62),(byte)(c.B*.38+255*.62));}
 static double Luminance(Color c){double Linear(byte value){double s=value/255d;return s<=.04045?s/12.92:Math.Pow((s+.055)/1.055,2.4);}return .2126*Linear(c.R)+.7152*Linear(c.G)+.0722*Linear(c.B);}
 public static double Contrast(Color a,Color b){double x=Luminance(a),y=Luminance(b);return (Math.Max(x,y)+.05)/(Math.Min(x,y)+.05);}
 public static Color Text(Color background)=>Contrast(background,Colors.Black)>=Contrast(background,Colors.White)?Colors.Black:Colors.White;
}
