using System;
using System.Windows.Media;

namespace StickyNotes;
internal static class NoteColors {
 public static readonly string[] Vivid={"#FFBE0B","#FB5607","#FF006E","#8338EC","#2463EB","#00B8D9","#00B86B","#EF4444"};
 public static readonly string[] Neon={"#CCFF00","#39FF14","#00FFCC","#00E5FF","#FF00D4","#FF5F1F","#FFF700","#BF5FFF"};
 public static string[] Palette(string mode)=>mode switch{"Vivid"=>Vivid,"Neon"=>Neon,_=>Store.Palette};
 public static Color Surface(string hex,string mode,bool dark){var c=(Color)ColorConverter.ConvertFromString(hex);if(mode is "Vivid" or "Neon")return c;return dark?Color.FromRgb((byte)(24+c.R*.13),(byte)(26+c.G*.13),(byte)(29+c.B*.13)):Color.FromRgb((byte)(c.R*.38+255*.62),(byte)(c.G*.38+255*.62),(byte)(c.B*.38+255*.62));}
 static double Luminance(Color c){double Linear(byte value){double s=value/255d;return s<=.04045?s/12.92:Math.Pow((s+.055)/1.055,2.4);}return .2126*Linear(c.R)+.7152*Linear(c.G)+.0722*Linear(c.B);}
 public static double Contrast(Color a,Color b){double x=Luminance(a),y=Luminance(b);return (Math.Max(x,y)+.05)/(Math.Min(x,y)+.05);}
 public static Color Text(Color background)=>Contrast(background,Colors.Black)>=Contrast(background,Colors.White)?Colors.Black:Colors.White;
}
