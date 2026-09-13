using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Windows.Threading;
namespace LegendsCompanion {
public class ProgressToast:Window {
 [DllImport("user32.dll",EntryPoint="GetWindowLong")]static extern int GetWindowLong(IntPtr h,int i);
 [DllImport("user32.dll",EntryPoint="SetWindowLong")]static extern int SetWindowLong(IntPtr h,int i,int v);
 TextBlock heading,name,mob,count;ProgressBar progress;DispatcherTimer hide;Canvas sparks;static int playing;
 static Dictionary<string,byte[]> sounds=new Dictionary<string,byte[]>();
 public ProgressToast(){Width=540;Height=94;WindowStyle=WindowStyle.None;ResizeMode=ResizeMode.NoResize;Background=new SolidColorBrush(Color.FromRgb(9,30,65));Topmost=true;ShowInTaskbar=false;ShowActivated=false;Focusable=false;IsHitTestVisible=false;
 var root=new Grid();Content=root;var panel=new StackPanel{Margin=new Thickness(12,5,12,5)};root.Children.Add(panel);
 heading=new TextBlock{FontSize=10,Foreground=Brushes.Gold,Text="ACHIEVEMENT PROGRESSED"};panel.Children.Add(heading);var row=new DockPanel();count=new TextBlock{FontSize=16,Foreground=Brushes.Gold,Margin=new Thickness(10,0,0,0)};DockPanel.SetDock(count,Dock.Right);row.Children.Add(count);name=new TextBlock{FontSize=15,FontWeight=FontWeights.SemiBold,Foreground=Brushes.White,TextTrimming=TextTrimming.CharacterEllipsis};row.Children.Add(name);panel.Children.Add(row);mob=new TextBlock{FontSize=12,Foreground=Brushes.LightSteelBlue,TextTrimming=TextTrimming.CharacterEllipsis};panel.Children.Add(mob);progress=new ProgressBar{Minimum=0,Maximum=100,Height=4,Margin=new Thickness(0,4,0,3),Foreground=Brushes.Gold,Background=Brushes.SlateGray,BorderThickness=new Thickness(0)};panel.Children.Add(progress);panel.Children.Add(new TextBlock{Text="Refresh your achievement export periodically for best results.",FontSize=10,Foreground=Brushes.LightSteelBlue});sparks=new Canvas{IsHitTestVisible=false,ClipToBounds=true};root.Children.Add(sparks);
 SourceInitialized+=(s,e)=>{var h=new WindowInteropHelper(this).Handle;SetWindowLong(h,-20,GetWindowLong(h,-20)|0x08000000|0x20|0x80);};hide=new DispatcherTimer{Interval=TimeSpan.FromSeconds(3)};hide.Tick+=(s,e)=>{hide.Stop();sparks.Children.Clear();Hide();};Closed+=(s,e)=>hide.Stop();
 }
 public void Display(List<LiveNotice> notices,LiveOptions options){if(notices.Count==0)return;hide.Stop();var n=notices[0];heading.Text=n.Complete?"ACHIEVEMENT COMPLETED":"ACHIEVEMENT PROGRESSED";name.Text=n.Name;mob.Text=n.Mob??(n.Complete?"Completed!":"Progress updated");count.Text=n.Complete?"Complete":n.Current.HasValue&&n.Target.HasValue?n.Current+" / "+n.Target:"";progress.Value=n.Complete?100:n.Target.GetValueOrDefault()>0?Math.Min(100,100.0*n.Current.GetValueOrDefault()/n.Target.Value):0;
 var area=SystemParameters.WorkArea;Width=Math.Min(560,area.Width-24);Height=94;string corner=options.Corner??"Top center";Left=corner=="Top center"?area.Left+(area.Width-Width)/2:corner.Contains("left")?area.Left+12:area.Right-Width-12;Top=corner.Contains("Bottom")?area.Bottom-Height-12:Math.Max(area.Top,SystemParameters.PrimaryScreenHeight*.05);Show();hide.Start();sparks.Children.Clear();if(n.Complete&&options.Celebrations&&SystemParameters.ClientAreaAnimation)Fireworks();Play(options.Volume,n.Complete&&options.Celebrations);}
 void Fireworks(){for(int k=0;k<16;k++){double angle=k*Math.PI/8;var dot=new Ellipse{Width=3,Height=3,Fill=k%2==0?Brushes.Gold:Brushes.DarkOrange};Canvas.SetLeft(dot,Width-45);Canvas.SetTop(dot,40);var t=new TranslateTransform();dot.RenderTransform=t;sparks.Children.Add(dot);t.BeginAnimation(TranslateTransform.XProperty,new DoubleAnimation(0,Math.Cos(angle)*35,TimeSpan.FromMilliseconds(850)));t.BeginAnimation(TranslateTransform.YProperty,new DoubleAnimation(0,Math.Sin(angle)*32,TimeSpan.FromMilliseconds(850)));dot.BeginAnimation(UIElement.OpacityProperty,new DoubleAnimation(1,0,TimeSpan.FromMilliseconds(850)));}}
 static void Play(int volume,bool fanfare){if(volume<=0)return;if(Interlocked.CompareExchange(ref playing,1,0)!=0)return;ThreadPool.QueueUserWorkItem(_=>{try{string key=volume+"|"+fanfare;byte[] bytes;if(!sounds.TryGetValue(key,out bytes)){bytes=MakeSound(volume,fanfare);sounds[key]=bytes;}using(var stream=new MemoryStream(bytes))using(var player=new System.Media.SoundPlayer(stream)){player.Load();player.PlaySync();}}catch{}finally{Interlocked.Exchange(ref playing,0);}});}
 static byte[] MakeSound(int volume,bool fanfare){int rate=22050;double duration=fanfare?1.35:.3;int n=(int)(rate*duration);using(var stream=new MemoryStream()){var w=new BinaryWriter(stream);w.Write(Encoding.ASCII.GetBytes("RIFF"));w.Write(36+n*2);w.Write(Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);w.Write(Encoding.ASCII.GetBytes("data"));w.Write(n*2);double[] melody={523.25,659.25,783.99,1046.5,783.99,1046.5};for(int i=0;i<n;i++){double t=(double)i/rate;double local=fanfare?t%.225:t;double freq=fanfare?melody[Math.Min(5,(int)(t/.225))]:1046.5;double envelope=Math.Min(1,local/.008)*Math.Exp(-local*12);double sample=Math.Sin(t*2*Math.PI*freq)+.22*Math.Sin(t*4*Math.PI*freq);w.Write((short)(sample*envelope*10000*Math.Max(0,Math.Min(100,volume))/100));}return stream.ToArray();}}
}
}
