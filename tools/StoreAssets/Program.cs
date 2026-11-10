using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
class Program {
 static byte[] Render(BitmapImage source,int size){var visual=new DrawingVisual();RenderOptions.SetBitmapScalingMode(visual,BitmapScalingMode.HighQuality);using(var drawing=visual.RenderOpen())drawing.DrawImage(source,new Rect(0,0,size,size));var bitmap=new RenderTargetBitmap(size,size,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using var output=new MemoryStream();png.Save(output);return output.ToArray();}
 [STAThread] static void Main(string[] args){if(args.Length<2)throw new ArgumentException("Source PNG and output directory are required.");Directory.CreateDirectory(args[1]);var source=new BitmapImage(new Uri(Path.GetFullPath(args[0])));
  if(args.Length>2&&args[2]=="--app-icons"){
   int[] sizes={16,20,24,32,40,48,64,128,256};var images=sizes.Select(size=>Render(source,size)).ToArray();using var output=new BinaryWriter(File.Create(Path.Combine(args[1],"Notes.ico")));output.Write((ushort)0);output.Write((ushort)1);output.Write((ushort)sizes.Length);int offset=6+16*sizes.Length;for(int i=0;i<sizes.Length;i++){output.Write((byte)(sizes[i]==256?0:sizes[i]));output.Write((byte)(sizes[i]==256?0:sizes[i]));output.Write((byte)0);output.Write((byte)0);output.Write((ushort)1);output.Write((ushort)32);output.Write(images[i].Length);output.Write(offset);offset+=images[i].Length;}foreach(var image in images)output.Write(image);
   return;
  }
  foreach(var size in new[]{44,50,150,310})File.WriteAllBytes(Path.Combine(args[1],"Logo"+size+".png"),Render(source,size));
 }
}
