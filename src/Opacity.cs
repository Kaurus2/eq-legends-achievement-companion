using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
namespace LegendsCompanion {
public partial class MainWindow {
 bool attentionActive,trackerFocused;
 double RestingOpacity(){return settings.TransparencyEnabled?Math.Max(.15,Math.Min(1,settings.TrackerOpacity)):1;}
 [DllImport("user32.dll",EntryPoint="GetWindowLongW")]static extern int AlphaGetStyle(IntPtr h,int index);
 [DllImport("user32.dll",EntryPoint="SetWindowLongW",SetLastError=true)]static extern int AlphaSetStyle(IntPtr h,int index,int value);
 [DllImport("user32.dll",SetLastError=true)]static extern bool SetLayeredWindowAttributes(IntPtr h,uint color,byte alpha,uint flags);
 [DllImport("user32.dll",SetLastError=true)]static extern bool GetLayeredWindowAttributes(IntPtr h,out uint color,out byte alpha,out uint flags);
 bool nativeAlphaHook;DispatcherTimer alphaTimer;double appliedAlpha=1;
 void ApplyNativeAlpha(double alpha){var handle=new WindowInteropHelper(this).Handle;if(handle==IntPtr.Zero)return;if(!nativeAlphaHook){var source=HwndSource.FromHwnd(handle);if(source!=null){source.AddHook((IntPtr hwnd,int message,IntPtr wp,IntPtr lp,ref bool handled)=>{if((message==0x7c||message==0x7d)&&wp.ToInt64()==-20){if(message==0x7c){int next=Marshal.ReadInt32(lp,4);Marshal.WriteInt32(lp,4,next|0x80000);}handled=true;}return IntPtr.Zero;});nativeAlphaHook=true;}}int style=AlphaGetStyle(handle,-20);if((style&0x80000)==0)AlphaSetStyle(handle,-20,style|0x80000);if(!SetLayeredWindowAttributes(handle,0,(byte)Math.Round(alpha*255),2)){if(status!=null)status.Text="Windows could not apply transparency ("+Marshal.GetLastWin32Error()+").";return;}appliedAlpha=alpha;}
 void UpdateOpacity(bool smooth){double target=trackerFocused||attentionActive?1:RestingOpacity();if(alphaTimer!=null)alphaTimer.Stop();BeginAnimation(Window.OpacityProperty,null);Opacity=1; // Keep the WPF content opaque; Windows blends the whole window with the game.
  if(!smooth||!SystemParameters.ClientAreaAnimation){ApplyNativeAlpha(target);return;}double start=appliedAlpha;var began=DateTime.UtcNow;alphaTimer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(40)};alphaTimer.Tick+=(s,e)=>{double fraction=Math.Min(1,(DateTime.UtcNow-began).TotalMilliseconds/320);ApplyNativeAlpha(start+(target-start)*fraction);if(fraction>=1)alphaTimer.Stop();};alphaTimer.Start();
 }
 public void CheckNativeTransparency(){bool originalFocus=trackerFocused;trackerFocused=false;bool enabled=settings.TransparencyEnabled;double value=settings.TrackerOpacity;bool attention=attentionActive;settings.TransparencyEnabled=true;settings.TrackerOpacity=.65;attentionActive=false;UpdateOpacity(false);uint color,flags;byte alpha;var h=new WindowInteropHelper(this).Handle;if(!GetLayeredWindowAttributes(h,out color,out alpha,out flags)||alpha!=166||(flags&2)==0||Opacity!=1)throw new Exception("Native 65% transparency failed: alpha="+alpha+" flags="+flags+" style="+AlphaGetStyle(h,-20)+" opacity="+Opacity+" error="+Marshal.GetLastWin32Error());attentionActive=true;UpdateOpacity(false);if(!GetLayeredWindowAttributes(h,out color,out alpha,out flags)||alpha!=255)throw new Exception("Attention opacity failed");attentionActive=false;UpdateOpacity(false);if(!GetLayeredWindowAttributes(h,out color,out alpha,out flags)||alpha!=166)throw new Exception("Transparency restore failed");settings.TrackerOpacity=0;UpdateOpacity(false);if(!GetLayeredWindowAttributes(h,out color,out alpha,out flags)||alpha!=38)throw new Exception("Minimum opacity clamp failed");attentionActive=true;UpdateOpacity(false);if(!GetLayeredWindowAttributes(h,out color,out alpha,out flags)||alpha!=255)throw new Exception("Zero opacity attention failed");attentionActive=false;UpdateOpacity(false);if(!GetLayeredWindowAttributes(h,out color,out alpha,out flags)||alpha!=38)throw new Exception("Minimum opacity restore failed");settings.TransparencyEnabled=false;UpdateOpacity(false);if(!GetLayeredWindowAttributes(h,out color,out alpha,out flags)||alpha!=255)throw new Exception("Disable transparency failed");trackerFocused=true;settings.TransparencyEnabled=true;settings.TrackerOpacity=.15;UpdateOpacity(false);if(!GetLayeredWindowAttributes(h,out color,out alpha,out flags)||alpha!=255)throw new Exception("Focused window must be opaque");trackerFocused=false;UpdateOpacity(false);if(!GetLayeredWindowAttributes(h,out color,out alpha,out flags)||alpha!=38)throw new Exception("Unfocused window must use saved opacity");trackerFocused=originalFocus;settings.TransparencyEnabled=enabled;settings.TrackerOpacity=value;attentionActive=attention;UpdateOpacity(false);}
 void SaveOpacity(){try{Data.Save(settingsPath,settings);}catch(Exception e){if(status!=null)status.Text="Could not remember transparency: "+e.Message;}}
 void BuildOpacityControl(){if(settings.TrackerOpacity<.15){settings.TrackerOpacity=.15;SaveOpacity();}
  var panel=new StackPanel{Margin=new Thickness(0,10,0,8)};configContent.Children.Insert(0,panel);
  var enabled=new CheckBox{Content="Enable window transparency",IsChecked=settings.TransparencyEnabled};panel.Children.Add(enabled);
  var value=new TextBlock{Margin=new Thickness(0,6,0,4)};panel.Children.Add(value);
  var slider=new Slider{Orientation=Orientation.Horizontal,Minimum=15,Maximum=100,Value=Math.Round(Math.Max(.15,Math.Min(1,settings.TrackerOpacity))*100),TickFrequency=5,IsSnapToTickEnabled=true,ToolTip="15% is the minimum; 100% is fully opaque"};panel.Children.Add(slider);
  enabled.Click+=(s,e)=>{settings.TransparencyEnabled=enabled.IsChecked==true;UpdateOpacity(true);SaveOpacity();};
  slider.ValueChanged+=(s,e)=>{settings.TrackerOpacity=slider.Value/100;value.Text=slider.Value.ToString("0")+"% opacity · 15% minimum / 100% fully visible";UpdateOpacity(false);};slider.LostMouseCapture+=(s,e)=>SaveOpacity();slider.KeyUp+=(s,e)=>SaveOpacity();value.Text=slider.Value.ToString("0")+"% opacity · 15% minimum / 100% fully visible";


  panel.Children.Add(new TextBlock{Text="Applies when you switch away. The app stays fully visible while you use it.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,5,0,0)});
  Activated+=(s,e)=>{trackerFocused=true;UpdateOpacity(false);};Deactivated+=(s,e)=>{trackerFocused=false;UpdateOpacity(true);};Loaded+=(s,e)=>{trackerFocused=IsActive;UpdateOpacity(false);};Closing+=(s,e)=>{if(alphaTimer!=null)alphaTimer.Stop();};
 }
 void PulseTile(Border tile){if(!SystemParameters.ClientAreaAnimation)return;var brush=new SolidColorBrush(Colors.Goldenrod);tile.BorderBrush=brush;brush.BeginAnimation(SolidColorBrush.ColorProperty,new ColorAnimation(Colors.Goldenrod,Colors.LightYellow,TimeSpan.FromMilliseconds(650)){AutoReverse=true,RepeatBehavior=new RepeatBehavior(2)});}
}
}







