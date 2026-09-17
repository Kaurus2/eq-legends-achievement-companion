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
 public event Action Expired;public Action SoundChanged;public int Slot;LiveOptions currentOptions;Button speaker;bool leaving; public void MoveTo(int slot){Slot=slot;double target=Math.Max(SystemParameters.WorkArea.Top,SystemParameters.PrimaryScreenHeight*.05)+slot*102;BeginAnimation(TopProperty,new DoubleAnimation(Top,target,TimeSpan.FromMilliseconds(260)){EasingFunction=new CubicEase{EasingMode=EasingMode.EaseOut}});} public void RefreshSpeaker(){if(speaker!=null&&currentOptions!=null){speaker.Content=currentOptions.SoundsEnabled?"🔊":"🔇";speaker.ToolTip=currentOptions.SoundsEnabled?"Mute sounds":"Enable sounds";}} void Depart(){if(leaving)return;leaving=true;hide.Stop();var fade=new DoubleAnimation(Opacity,0,TimeSpan.FromMilliseconds(240));fade.Completed+=(s,e)=>{Hide();if(Expired!=null)Expired();};BeginAnimation(OpacityProperty,fade);} TextBlock heading,name,mob,count;ProgressBar progress;DispatcherTimer hide;Canvas sparks;static int playing;
 Border completionBorder;static Dictionary<string,byte[]> sounds=new Dictionary<string,byte[]>();
 public ProgressToast(){Width=540;Height=94;WindowStyle=WindowStyle.None;ResizeMode=ResizeMode.NoResize;Background=new SolidColorBrush(Color.FromRgb(9,30,65));Topmost=true;ShowInTaskbar=false;ShowActivated=false;Focusable=false;AllowsTransparency=true;
 var root=new Grid();Content=root;completionBorder=new Border{BorderBrush=Brushes.Gold,BorderThickness=new Thickness(2),IsHitTestVisible=false,Opacity=0};root.Children.Add(completionBorder);var panel=new StackPanel{Margin=new Thickness(12,5,12,5)};root.Children.Add(panel);
 heading=new TextBlock{FontSize=10,Foreground=Brushes.Gold,Text="ACHIEVEMENT PROGRESSED"};var titleRow=new DockPanel();speaker=new Button{FontSize=12,Padding=new Thickness(2,0,2,0),Background=Brushes.Transparent,Foreground=Brushes.Gold,BorderThickness=new Thickness(0),Focusable=false};DockPanel.SetDock(speaker,Dock.Right);titleRow.Children.Add(speaker);titleRow.Children.Add(heading);panel.Children.Add(titleRow);speaker.Click+=(s,e)=>{currentOptions.SoundsEnabled=!currentOptions.SoundsEnabled;RefreshSpeaker();if(SoundChanged!=null)SoundChanged();};var row=new DockPanel();count=new TextBlock{FontSize=16,Foreground=Brushes.Gold,Margin=new Thickness(10,0,0,0)};DockPanel.SetDock(count,Dock.Right);row.Children.Add(count);name=new TextBlock{FontSize=15,FontWeight=FontWeights.SemiBold,Foreground=Brushes.White,TextTrimming=TextTrimming.CharacterEllipsis};row.Children.Add(name);panel.Children.Add(row);mob=new TextBlock{FontSize=12,Foreground=Brushes.LightSteelBlue,TextTrimming=TextTrimming.CharacterEllipsis};panel.Children.Add(mob);progress=new ProgressBar{Minimum=0,Maximum=100,Height=4,Margin=new Thickness(0,4,0,3),Foreground=Brushes.Gold,Background=Brushes.SlateGray,BorderThickness=new Thickness(0)};panel.Children.Add(progress);panel.Children.Add(new TextBlock{Text="Refresh your achievement export periodically for best results.",FontSize=10,Foreground=Brushes.LightSteelBlue});sparks=new Canvas{IsHitTestVisible=false,ClipToBounds=true};root.Children.Add(sparks);
 SourceInitialized+=(s,e)=>{var h=new WindowInteropHelper(this).Handle;SetWindowLong(h,-20,GetWindowLong(h,-20)|0x08000000|0x80);};hide=new DispatcherTimer{Interval=TimeSpan.FromSeconds(8)};hide.Tick+=(s,e)=>{Depart();};Closed+=(s,e)=>{hide.Stop();BeginAnimation(TopProperty,null);BeginAnimation(OpacityProperty,null);};
 }
 public void Display(List<LiveNotice> notices,LiveOptions options){if(notices.Count==0)return;hide.Stop();leaving=false;currentOptions=options;RefreshSpeaker();var n=notices[0];hide.Interval=TimeSpan.FromSeconds(n.Complete?13:8);completionBorder.BeginAnimation(UIElement.OpacityProperty,null);completionBorder.Opacity=n.Complete?1:0;if(n.Complete&&options.Celebrations&&SystemParameters.ClientAreaAnimation)completionBorder.BeginAnimation(UIElement.OpacityProperty,new DoubleAnimation(.35,1,TimeSpan.FromMilliseconds(300)){AutoReverse=true,RepeatBehavior=RepeatBehavior.Forever});heading.Text=n.Complete?"ACHIEVEMENT COMPLETED":"ACHIEVEMENT PROGRESSED";name.Text=n.Name;mob.Text=n.Mob??(n.Complete?"Completed!":"Progress updated");count.Text=n.Complete?"Complete":n.Current.HasValue&&n.Target.HasValue?n.Current+" / "+n.Target:"";progress.Value=n.Complete?100:n.Target.GetValueOrDefault()>0?Math.Min(100,100.0*n.Current.GetValueOrDefault()/n.Target.Value):0;
 var area=SystemParameters.WorkArea;Width=Math.Min(560,area.Width-24);Height=94;Left=area.Left+(area.Width-Width)/2;double target=Math.Max(area.Top,SystemParameters.PrimaryScreenHeight*.05)+Slot*102;BeginAnimation(TopProperty,null);Top=target-Height;BeginAnimation(OpacityProperty,null);Opacity=0;Show();var entrance=new DoubleAnimation(Top,target,TimeSpan.FromMilliseconds(300)){EasingFunction=new CubicEase{EasingMode=EasingMode.EaseOut}};BeginAnimation(TopProperty,entrance);var appear=new DoubleAnimation(0,1,TimeSpan.FromMilliseconds(300));appear.Completed+=(s,e)=>{if(!leaving)hide.Start();};BeginAnimation(OpacityProperty,appear);sparks.Children.Clear();if(n.Complete&&options.Celebrations&&SystemParameters.ClientAreaAnimation)Fireworks();PlayNotice(options,n);}
 public static void PlayNotice(LiveOptions options,LiveNotice notice){if(options.SoundsEnabled)Play(options.Volume,notice.Complete&&options.Celebrations);} void Fireworks(){for(int k=0;k<16;k++){double angle=k*Math.PI/8;var dot=new Ellipse{Width=3,Height=3,Fill=k%2==0?Brushes.Gold:Brushes.DarkOrange};Canvas.SetLeft(dot,Width-45);Canvas.SetTop(dot,40);var t=new TranslateTransform();dot.RenderTransform=t;sparks.Children.Add(dot);t.BeginAnimation(TranslateTransform.XProperty,new DoubleAnimation(0,Math.Cos(angle)*35,TimeSpan.FromMilliseconds(850)));t.BeginAnimation(TranslateTransform.YProperty,new DoubleAnimation(0,Math.Sin(angle)*32,TimeSpan.FromMilliseconds(850)));dot.BeginAnimation(UIElement.OpacityProperty,new DoubleAnimation(1,0,TimeSpan.FromMilliseconds(850)));}}
 static void Play(int volume,bool fanfare){if(volume<=0)return;if(Interlocked.CompareExchange(ref playing,1,0)!=0)return;ThreadPool.QueueUserWorkItem(_=>{try{string key=volume+"|"+fanfare;byte[] bytes;if(!sounds.TryGetValue(key,out bytes)){bytes=MakeSound(volume,fanfare);sounds[key]=bytes;}using(var stream=new MemoryStream(bytes))using(var player=new System.Media.SoundPlayer(stream)){player.Load();player.PlaySync();}}catch{}finally{Interlocked.Exchange(ref playing,0);}});}
 static byte[] MakeSound(int volume,bool fanfare){
  int rate=22050;double duration=fanfare?5.4:.3;int n=(int)(rate*duration);
  // Original celebratory phrase: brisk brass-like lead, bass, and a sustained major chord.
  double[] notes={392,523.25,659.25,783.99,587.33,698.46,880,1046.5,987.77,783.99,659.25,1046.5,783.99,880,987.77,1046.5,1318.51,1174.66,1046.5,783.99,1046.5};
  double[] lengths={.12,.12,.18,.30,.12,.12,.18,.30,.12,.12,.24,.48,.12,.12,.12,.24,.24,.24,.24,.24,1.42};
  using(var stream=new MemoryStream()){var w=new BinaryWriter(stream);w.Write(Encoding.ASCII.GetBytes("RIFF"));w.Write(36+n*2);w.Write(Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);w.Write(Encoding.ASCII.GetBytes("data"));w.Write(n*2);
   int note=0;double onset=0;for(int i=0;i<n;i++){
    double t=(double)i/rate,sample;
    if(!fanfare)sample=(Math.Sin(t*2*Math.PI*1046.5)+.22*Math.Sin(t*4*Math.PI*1046.5))*Math.Min(1,t/.008)*Math.Exp(-t*12)*.30;
    else{
     while(note<notes.Length-1&&t>=onset+lengths[note]){onset+=lengths[note];note++;}
     double local=t-onset,len=lengths[note],freq=notes[note];
     double env=Math.Min(1,local/.012)*Math.Min(1,Math.Max(0,(len-local)/.07));
     double phase=2*Math.PI*freq*local;
     double lead=(Math.Sin(phase)+.30*Math.Sin(phase*2)+.12*Math.Sin(phase*3))*.26*env;
     double chord=(Math.Sin(t*2*Math.PI*261.63)+Math.Sin(t*2*Math.PI*329.63)+Math.Sin(t*2*Math.PI*392))*.065;
     double beat=t%.3;double bass=Math.Sin(t*2*Math.PI*(t<1.5?130.81:196))*.13*Math.Exp(-beat*9);
     double tail=Math.Min(1,t/.018)*Math.Min(1,Math.Max(0,(duration-t)/.30));
     double pulse=t%.24;double drum=Math.Sin(2*Math.PI*(90*pulse-70*pulse*pulse))*.13*Math.Exp(-pulse*32);sample=(lead+chord+bass+drum)*tail;
    }
    w.Write((short)(Math.Max(-.95,Math.Min(.95,sample))*32767*Math.Max(0,Math.Min(100,volume))/100));
   }return stream.ToArray();
  }
 }
}
public class ToastStack:IDisposable {
 readonly List<ProgressToast> visible=new List<ProgressToast>();readonly Queue<LiveNotice> waiting=new Queue<LiveNotice>();readonly DispatcherTimer arrivals=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(100)};DateTime nextArrival;bool disposed;LiveOptions options;public Action SoundChanged;
 public ToastStack(){arrivals.Tick+=(s,e)=>Fill();} public int VisibleCount{get{return visible.Count;}}public int WaitingCount{get{return waiting.Count;}}
 public void Display(List<LiveNotice> notices,LiveOptions settings){options=settings;foreach(var n in notices){if(waiting.Count<50)waiting.Enqueue(n);}Fill();}
 void Fill(){if(disposed)return;if(visible.Count<3&&waiting.Count>0&&DateTime.UtcNow>=nextArrival){nextArrival=DateTime.UtcNow.AddSeconds(1);var toast=new ProgressToast{Slot=visible.Count};visible.Add(toast);toast.SoundChanged=()=>{foreach(var t in visible)t.RefreshSpeaker();if(SoundChanged!=null)SoundChanged();};toast.Expired+=()=>{visible.Remove(toast);toast.Close();for(int i=0;i<visible.Count;i++)visible[i].MoveTo(i);Fill();};toast.Display(new List<LiveNotice>{waiting.Dequeue()},options);}if(waiting.Count>0&&visible.Count<3)arrivals.Start();else arrivals.Stop();}
 public void Hide(){arrivals.Stop();nextArrival=DateTime.MinValue;waiting.Clear();foreach(var t in visible.ToArray())t.Close();visible.Clear();}
 public void Close(){Dispose();}public void Dispose(){disposed=true;Hide();}
}}
