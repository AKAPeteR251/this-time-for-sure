$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
public static class R3Export {
 public static byte[] Pixels(Bitmap b){var d=b.LockBits(new Rectangle(0,0,b.Width,b.Height),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);var a=new byte[d.Stride*b.Height];Marshal.Copy(d.Scan0,a,0,a.Length);b.UnlockBits(d);return a;}
 public static void Set(Bitmap b,byte[] a){var d=b.LockBits(new Rectangle(0,0,b.Width,b.Height),ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);Marshal.Copy(a,0,d.Scan0,a.Length);b.UnlockBits(d);}
 public static Graphics G(Bitmap b){var g=Graphics.FromImage(b);g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.SmoothingMode=SmoothingMode.AntiAlias;g.PixelOffsetMode=PixelOffsetMode.HighQuality;return g;}
 public static Rectangle Bounds(Bitmap b,int threshold){var a=Pixels(b);int x0=b.Width,y0=b.Height,x1=-1,y1=-1;for(int y=0;y<b.Height;y++)for(int x=0;x<b.Width;x++)if(a[(y*b.Width+x)*4+3]>threshold){x0=Math.Min(x0,x);x1=Math.Max(x1,x);y0=Math.Min(y0,y);y1=Math.Max(y1,y);}return x1<0?new Rectangle(0,0,b.Width,b.Height):Rectangle.FromLTRB(x0,y0,x1+1,y1+1);}
 public static Bitmap Load(string path){using(var img=Image.FromFile(path)){var b=new Bitmap(img.Width,img.Height,PixelFormat.Format32bppArgb);using(var g=G(b))g.DrawImageUnscaled(img,0,0);return b;}}
 public static void Clean(Bitmap b,bool white,bool solid){var a=Pixels(b);for(int i=0;i<a.Length;i+=4){if(a[i+3]<24){a[i]=a[i+1]=a[i+2]=a[i+3]=0;}else {if(white)a[i]=a[i+1]=a[i+2]=255;if(solid && a[i+3]>220)a[i+3]=255;}}Set(b,a);}
 public static void Export(string source,string target,string id,int w,int h){using(var src=Load(source))using(var dst=new Bitmap(w,h,PixelFormat.Format32bppArgb)){
 bool bg=id=="BG01", neb=id=="BG02"||id=="BG03", type=id.StartsWith("UI");
 if(!bg&&!neb&&(type||id=="CH01"||id=="FX04"))Clean(src,id=="UI05"||id=="UI06"||id=="UI07",type);
 using(var g=G(dst)){if(bg||neb)g.DrawImage(src,new Rectangle(0,0,w,h));else {var box=Bounds(src,type?160:32);float maxW=w*.90f,maxH=h*.86f;float scale=Math.Min(maxW/box.Width,maxH/box.Height);float height=box.Height*scale;
 if(id=="UI01"||id=="UI02"||id=="UI03"||id=="UI04"){height=256;scale=height/box.Height;}
 float width=box.Width*scale;float y=(h-height)/2;if(id=="UI01"||id=="UI02"||id=="UI03"||id=="UI04")y=104;
 if(id=="UI07")g.DrawImage(src,new RectangleF(46,32,w-92,h-64),box,GraphicsUnit.Pixel);
 else g.DrawImage(src,new RectangleF((w-width)/2,y,width,height),box,GraphicsUnit.Pixel);}}
 var a=Pixels(dst);
 if(neb){int blend=128;for(int y=0;y<h;y++){double fade=Math.Min(1,Math.Min(y,h-1-y)/48.0);fade=fade*fade*(3-2*fade);for(int x=0;x<w;x++){int p=(y*w+x)*4;a[p+3]=(byte)Math.Round(a[p+3]*fade*.35);}
 for(int x=0;x<blend;x++){int p=(y*w+x)*4,q=(y*w+w-1-x)*4;double t=(double)x/(blend-1);double v=(1-t*t*(3-2*t))*.5;for(int k=0;k<4;k++){byte left=a[p+k],right=a[q+k];a[p+k]=(byte)Math.Round(left*(1-v)+right*v);a[q+k]=(byte)Math.Round(right*(1-v)+left*v);}}}}
 if(!bg&&!neb){for(int y=0;y<h;y++)for(int x=0;x<w;x++){int p=(y*w+x)*4;if(x<2||x>=w-2||y<2||y>=h-2||a[p+3]<8)a[p]=a[p+1]=a[p+2]=a[p+3]=0;}}
 if(bg)for(int p=3;p<a.Length;p+=4)a[p]=255;
 Set(dst,a);dst.Save(target,ImageFormat.Png);
 }}
 public static void Layer(Graphics g,string path,Rectangle dest,float opacity){using(var b=Load(path))using(var attr=new ImageAttributes()){float scale=Math.Min(dest.Width/(float)b.Width,dest.Height/(float)b.Height);int width=(int)(b.Width*scale),height=(int)(b.Height*scale);dest=new Rectangle(dest.X+(dest.Width-width)/2,dest.Y+(dest.Height-height)/2,width,height);var m=new ColorMatrix();m.Matrix33=opacity;attr.SetColorMatrix(m);attr.SetWrapMode(WrapMode.TileFlipXY);g.DrawImage(b,dest,0,0,b.Width,b.Height,GraphicsUnit.Pixel,attr);}}
 public static void Preview(string dir,string character){using(var b=new Bitmap(1920,1080,PixelFormat.Format32bppArgb))using(var g=G(b)){
 Layer(g,System.IO.Path.Combine(dir,"reveal_starfield_base_v003.png"),new Rectangle(0,0,1920,1080),1);
 Layer(g,System.IO.Path.Combine(dir,"reveal_nebula_far_v003.png"),new Rectangle(0,70,1920,960),.45f);
 Layer(g,System.IO.Path.Combine(dir,"reveal_nebula_near_v003.png"),new Rectangle(0,40,1920,960),.25f);
 Layer(g,System.IO.Path.Combine(dir,"SSR001_snowflake_backdrop_v003.png"),new Rectangle(540,60,980,980),.37f);
 Layer(g,character,new Rectangle(710,20,660,990),1);
 Layer(g,System.IO.Path.Combine(dir,"rarity_SSR_v003.png"),new Rectangle(144,600,350,175),1);
 Layer(g,System.IO.Path.Combine(dir,"rarity_accent_v003.png"),new Rectangle(130,574,380,48),.85f);
 using(var f=new Font("STZhongsong",68,FontStyle.Regular,GraphicsUnit.Pixel))using(var brush=new SolidBrush(Color.FromArgb(240,244,252)))g.DrawString("冰法师",f,brush,new PointF(150,462));
 using(var mask=Load(System.IO.Path.Combine(dir,"new_border_mask_v003.png"))){var a=Pixels(mask);Color[] palette={Color.Cyan,Color.MediumPurple,Color.LightPink,Color.FromArgb(255,223,144)};for(int y=0;y<mask.Height;y++)for(int x=0;x<mask.Width;x++){float t=x/(float)(mask.Width-1)*3;int k=Math.Min(2,(int)t);float u=t-k;int p=(y*mask.Width+x)*4;a[p]=(byte)(palette[k].B*(1-u)+palette[k+1].B*u);a[p+1]=(byte)(palette[k].G*(1-u)+palette[k+1].G*u);a[p+2]=(byte)(palette[k].R*(1-u)+palette[k+1].R*u);}Set(mask,a);g.DrawImage(mask,new Rectangle(148,358,164,55));}
 Layer(g,System.IO.Path.Combine(dir,"new_text_v003.png"),new Rectangle(174,366,112,42),1);
 int[,] stars={{84,150,24},{443,212,18},{270,813,22},{1370,250,16},{1480,980,16},{1750,760,20}};for(int i=0;i<stars.GetLength(0);i++){int n=stars[i,2];Layer(g,System.IO.Path.Combine(dir,"star_cross_v003.png"),new Rectangle(stars[i,0],stars[i,1],n,n),.65f);}
 b.Save(System.IO.Path.Combine(dir,"reveal_layout_preview_v003.png"),ImageFormat.Png);
 }}
 public static void Proof(string dir){string[] names={"rarity_R_v003.png","rarity_SR_v003.png","rarity_SSR_v003.png","rarity_UR_v003.png","new_text_v003.png","new_border_mask_v003.png","star_soft_v003.png","star_cross_v003.png","meteor_trail_v003.png","reveal_ice_shard_v003.png"};using(var b=new Bitmap(1000,640))using(var g=G(b)){g.Clear(Color.FromArgb(7,13,27));using(var font=new Font("Arial",12)){for(int i=0;i<names.Length;i++){int x=(i%5)*200,y=(i/5)*220;g.DrawString(names[i].Replace("_v003.png",""),font,Brushes.White,x+8,y+12);Layer(g,System.IO.Path.Combine(dir,names[i]),new Rectangle(x+10,y+48,180,i<4?90:100),1);}}
 for(int i=0;i<4;i++)Layer(g,System.IO.Path.Combine(dir,names[i]),new Rectangle(i*230+20,480,120,60),1);
 Layer(g,System.IO.Path.Combine(dir,"new_text_v003.png"),new Rectangle(30,570,80,30),1);Layer(g,System.IO.Path.Combine(dir,"new_text_v003.png"),new Rectangle(200,560,140,53),1);b.Save(System.IO.Path.Combine(dir,"readability_proof_v003.png"),ImageFormat.Png);}
 using(var b=new Bitmap(1600,800))using(var g=G(b)){g.Clear(Color.FromArgb(6,10,23));for(int i=0;i<2;i++)for(int j=0;j<2;j++)Layer(g,System.IO.Path.Combine(dir,i==0?"reveal_nebula_far_v003.png":"reveal_nebula_near_v003.png"),new Rectangle(j*800,i*400,800,400),1);b.Save(System.IO.Path.Combine(dir,"nebula_repeat_proof_v003.png"),ImageFormat.Png);}}
}
'@ -ReferencedAssemblies @('System.Drawing.Common','System.Drawing.Primitives','System.Private.Windows.GdiPlus','System.Private.Windows.Core','System.Runtime.InteropServices','System.Runtime')
$taskSpecs=@{BG01=@(3840,2160);BG02=@(2048,1024);BG03=@(2048,1024);CH01=@(2048,2048);FX01=@(256,256);FX02=@(256,256);FX03=@(1024,256);FX04=@(256,512);UI01=@(1024,512);UI02=@(1024,512);UI03=@(1024,512);UI04=@(1024,512);UI05=@(1024,128);UI06=@(512,192);UI07=@(768,256)}
$taskJobs=Get-Content (Join-Path $PSScriptRoot 'generation_jobs.json') -Raw | ConvertFrom-Json
foreach($taskJob in $taskJobs){$taskSize=$taskSpecs[$taskJob.id];[R3Export]::Export((Join-Path "$PSScriptRoot/raw_generated" $taskJob.file),(Join-Path $PSScriptRoot $taskJob.file),$taskJob.id,$taskSize[0],$taskSize[1])}
$taskCharacter=(Resolve-Path (Join-Path $PSScriptRoot '../../../UIAssets/Originals/ice_mage_cutout_v001.png')).Path
[R3Export]::Preview($PSScriptRoot,$taskCharacter)
[R3Export]::Proof($PSScriptRoot)
