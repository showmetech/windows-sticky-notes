using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace StickyNotes;

// Pure layout calculation: no windows or saved notes change until every monitor fits.
internal static class NoteArrangement {
 internal const double Gap=2;
 internal static IReadOnlyList<Rect>? Plan(Rect work,IReadOnlyList<Size> sizes){
  if(sizes.Count==0)return Array.Empty<Rect>();
  if(work.IsEmpty||!double.IsFinite(work.Width)||!double.IsFinite(work.Height)||work.Width<=0||work.Height<=0)return null;
  double insetX=work.Width>=324?12:0,insetY=work.Height>=284?12:0;
  double width=work.Width-2*insetX,height=work.Height-2*insetY;
  double minWidth=Math.Min(300,width),minHeight=Math.Min(260,height);
  int rows=Math.Max(1,(int)Math.Floor((height+Gap)/(minHeight+Gap)));
  int columns=(int)Math.Ceiling(sizes.Count/(double)rows);
  if(columns>Math.Max(1,(int)Math.Floor((width+Gap)/(minWidth+Gap))))return null;
  double desiredWidth=sizes.Max(s=>double.IsFinite(s.Width)?s.Width:344);
  double columnWidth=Math.Min(Math.Clamp(desiredWidth,minWidth,Math.Max(minWidth,420)),(width-Gap*(columns-1))/columns);
  var heights=sizes.Select(s=>Math.Clamp(double.IsFinite(s.Height)?s.Height:380,minHeight,height)).ToArray();
  int Required(double scale){int count=1;double used=0;foreach(double h in heights){double next=Math.Max(minHeight,h*scale);if(used>0&&used+Gap+next>height+.0001){count++;used=0;}used+=(used>0?Gap:0)+next;}return count;}
  double scale=1;
  if(Required(scale)>columns){double low=0,high=1;for(int i=0;i<40;i++){double mid=(low+high)/2;if(Required(mid)<=columns)low=mid;else high=mid;}scale=low;}
  var result=new List<Rect>();int column=0;double y=0;
  foreach(double original in heights){double h=Math.Max(minHeight,original*scale);if(y>0&&y+Gap+h>height+.0001){column++;y=0;}if(y>0)y+=Gap;result.Add(new Rect(work.Right-insetX-columnWidth-column*(columnWidth+Gap),work.Bottom-insetY-y-h,columnWidth,h));y+=h;}
  return result;
 }
}
