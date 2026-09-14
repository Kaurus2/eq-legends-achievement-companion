using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shell;
namespace LegendsCompanion {
public partial class MainWindow {
 ToggleButton pinButton;
 void InstallWindowFrame(){
  WindowChrome.SetWindowChrome(this,new WindowChrome{CaptionHeight=32,ResizeBorderThickness=new Thickness(6),GlassFrameThickness=new Thickness(0),CornerRadius=new CornerRadius(0),UseAeroCaptionButtons=false});
  var content=(UIElement)Content;Content=null;var shell=new DockPanel{Background=Palette.Background};Content=shell;
  var bar=new DockPanel{Height=32,Background=Palette.Surface};DockPanel.SetDock(bar,Dock.Top);shell.Children.Add(bar);
  var buttons=new StackPanel{Orientation=Orientation.Horizontal};DockPanel.SetDock(buttons,Dock.Right);bar.Children.Add(buttons);
  themeButton=new Button{Content="💡",ToolTip=settings.DarkMode?"Switch to light mode":"Switch to dark mode",Width=30,Height=28,FontSize=18,Padding=new Thickness(0),Margin=new Thickness(0,2,2,2)};WindowChrome.SetIsHitTestVisibleInChrome(themeButton,true);themeButton.Click+=(s,e)=>ToggleTheme();buttons.Children.Add(themeButton);
  configButton=new Button{Content="⚙",ToolTip="Configuration",Width=32,Height=28,FontSize=17,Padding=new Thickness(0),Margin=new Thickness(0,2,2,2)};WindowChrome.SetIsHitTestVisibleInChrome(configButton,true);configButton.Click+=(s,e)=>ToggleConfiguration();buttons.Children.Add(configButton);
  var pinLabel=new StackPanel{Orientation=Orientation.Horizontal,VerticalAlignment=VerticalAlignment.Center};pinLabel.Children.Add(new System.Windows.Shapes.Path{Data=Geometry.Parse("M 3,1 L 11,1 L 10,3 L 10,7 L 13,10 L 8,10 L 7,16 L 6,10 L 1,10 L 4,7 L 4,3 Z"),Fill=Palette.Blue,Width=12,Height=16,Stretch=Stretch.Uniform,Margin=new Thickness(0,0,4,0)});pinLabel.Children.Add(new TextBlock{Text="Pin",VerticalAlignment=VerticalAlignment.Center});
  pinButton=new ToggleButton{Content=pinLabel,Width=58,Height=28,Margin=new Thickness(0,2,2,2),IsChecked=settings.AlwaysOnTop,ToolTip="Always on top"};WindowChrome.SetIsHitTestVisibleInChrome(pinButton,true);buttons.Children.Add(pinButton);
  pinButton.Click+=(s,e)=>{settings.AlwaysOnTop=pinButton.IsChecked==true;Topmost=settings.AlwaysOnTop;UpdatePin();try{Data.Save(settingsPath,settings);}catch(Exception error){status.Text="Always on top changed, but could not save: "+error.Message;}};
  Action<string,string,Action> add=(caption,tip,action)=>{var b=new Button{Content=caption,ToolTip=tip,Width=34,Height=28,Margin=new Thickness(0,2,2,2),Padding=new Thickness(0)};WindowChrome.SetIsHitTestVisibleInChrome(b,true);b.Click+=(s,e)=>action();buttons.Children.Add(b);};
  add("−","Minimize",()=>WindowState=WindowState.Minimized);add("□","Maximize / restore",()=>WindowState=WindowState==WindowState.Maximized?WindowState.Normal:WindowState.Maximized);add("×","Close",()=>Close());
  var oldParent=characterPicker.Parent as Panel;if(oldParent!=null)oldParent.Children.Remove(characterPicker);
    characterPicker.MinWidth=100;characterPicker.MaxWidth=180;characterPicker.Width=180;characterPicker.HorizontalAlignment=HorizontalAlignment.Left;
  characterPicker.Margin=new Thickness(4,2,8,2);WindowChrome.SetIsHitTestVisibleInChrome(characterPicker,true);
  var captionArea=new DockPanel{Background=Brushes.Transparent};
  var appName=new TextBlock{Text="EQ Legends Achievement Companion",Foreground=Palette.Gold,FontWeight=FontWeights.SemiBold,FontSize=12,Margin=new Thickness(10,0,8,0),VerticalAlignment=VerticalAlignment.Center,TextTrimming=TextTrimming.CharacterEllipsis,ToolTip="EQ Legends Achievement Companion — drag here to move"};
  captionArea.Children.Add(appName);bar.Children.Add(captionArea);
  bool? compactHeader=null;
  Action arrangeHeader=()=>{
   bool compact=Width<700;if(compactHeader==compact)return;compactHeader=compact;
   var parent=characterPicker.Parent as Panel;if(parent!=null)parent.Children.Remove(characterPicker);
   if(compact){trackerToolbar.Children.Insert(0,characterPicker);appName.Text="EQ Legends";}
   else{DockPanel.SetDock(characterPicker,Dock.Right);captionArea.Children.Insert(0,characterPicker);appName.Text="EQ Legends Achievement Companion";}
  };
  SizeChanged+=(s,e)=>arrangeHeader();arrangeHeader();
  var opacityRow=new StackPanel{Orientation=Orientation.Horizontal,Margin=new Thickness(0,0,8,3)};
  opacityRow.Children.Add(new TextBlock{Text="Opacity",VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(0,0,4,0)});
  frameOpacitySlider.Width=72;frameOpacitySlider.VerticalAlignment=VerticalAlignment.Center;opacityRow.Children.Add(frameOpacitySlider);opacityRow.Children.Add(opacityPercent);trackerToolbar.Children.Insert(0,opacityRow);
  shell.Children.Add(content);Topmost=settings.AlwaysOnTop;UpdatePin();
 }
 void UpdatePin(){pinButton.IsChecked=Topmost;pinButton.Background=Topmost?Palette.Highlight:Palette.Surface;pinButton.ToolTip=Topmost?"Always on top: on — click to unpin":"Always on top: off — click to pin";}
 public void CheckWindowPin(){bool before=Topmost;Topmost=true;UpdatePin();if(!Topmost||pinButton.IsChecked!=true)throw new Exception("Pin did not enable");Topmost=false;UpdatePin();if(Topmost||pinButton.IsChecked!=false)throw new Exception("Pin did not disable");if(WindowChrome.GetWindowChrome(this)==null)throw new Exception("Window frame missing");Topmost=before;UpdatePin();}
 void RestoreWindowPlacement(){
  if(!settings.WindowPlacementSaved)return;
  double w=settings.WindowWidth,h=settings.WindowHeight,x=settings.WindowLeft,y=settings.WindowTop;
  if(Double.IsNaN(w)||Double.IsInfinity(w)||Double.IsNaN(h)||Double.IsInfinity(h)||w<MinWidth||h<MinHeight||Double.IsNaN(x)||Double.IsInfinity(x)||Double.IsNaN(y)||Double.IsInfinity(y))return;
  var desktop=new Rect(SystemParameters.VirtualScreenLeft,SystemParameters.VirtualScreenTop,SystemParameters.VirtualScreenWidth,SystemParameters.VirtualScreenHeight);
  if(!desktop.IntersectsWith(new Rect(x,y,w,32))){var area=SystemParameters.WorkArea;x=area.Left;y=area.Top;w=Math.Min(w,area.Width);h=Math.Min(h,area.Height);}
  Width=Math.Max(MinWidth,w);Height=Math.Max(MinHeight,h);Left=x;Top=y;WindowStartupLocation=WindowStartupLocation.Manual;
  if(settings.WindowMaximized)WindowState=WindowState.Maximized;
 }
 void SaveWindowPlacement(){
  Rect bounds=WindowState==WindowState.Normal?new Rect(Left,Top,Width,Height):RestoreBounds;
  if(bounds.IsEmpty||bounds.Width<MinWidth||bounds.Height<MinHeight)return;
  // The research panel temporarily expands a compact tracker; reopen its normal tracker size.
  settings.WindowLeft=compactWidth>0?compactLeft:bounds.Left;settings.WindowTop=bounds.Top;
  settings.WindowWidth=compactWidth>0?compactWidth:bounds.Width;settings.WindowHeight=bounds.Height;
  if(WindowState!=WindowState.Minimized)settings.WindowMaximized=WindowState==WindowState.Maximized;
  settings.WindowPlacementSaved=true;
  try{Data.Save(settingsPath,settings);}catch(Exception error){status.Text="Could not remember window placement: "+error.Message;}
 }
 public void CheckWindowPlacement(){
  WindowState=WindowState.Normal;Left=80;Top=90;Width=340;Height=720;UpdateLayout();SaveWindowPlacement();
  var saved=Data.Load<Settings>(settingsPath);
  if(!saved.WindowPlacementSaved||saved.WindowWidth!=340||saved.WindowHeight!=720||saved.WindowLeft!=80||Math.Abs(saved.WindowTop-90)>1)throw new Exception("Window placement save failed");
  Width=900;Height=800;Left=140;RestoreWindowPlacement();
  if(Width!=340||Height!=720||Left!=80||Math.Abs(Top-90)>1)throw new Exception("Window placement restore failed");
 } Grid progressOverview;ProgressBar baneBar,overallBar;TextBlock baneLabel,overallLabel,showingLabel;bool? overviewWide;
 void BuildProgressOverview(StackPanel parent){
  var box=new StackPanel{Margin=new Thickness(0,0,0,6)};
  progressOverview=new Grid();
  baneBar=new ProgressBar{Minimum=0,Maximum=100,Foreground=Palette.Gold,Background=Palette.Border};
  overallBar=new ProgressBar{Minimum=0,Maximum=100,Foreground=Palette.Blue,Background=Palette.Border};
  baneLabel=new TextBlock{TextWrapping=TextWrapping.Wrap,Margin=new Thickness(6,0,6,4)};
  overallLabel=new TextBlock{TextWrapping=TextWrapping.Wrap,Margin=new Thickness(6,0,6,4)};
  progressOverview.Children.Add(baneBar);progressOverview.Children.Add(baneLabel);progressOverview.Children.Add(overallBar);progressOverview.Children.Add(overallLabel);
  box.Children.Add(progressOverview);showingLabel=new TextBlock{Margin=new Thickness(6,4,0,0)};box.Children.Add(showingLabel);
  box.Children.Add(new Expander{Header="Details",Content=summary,Margin=new Thickness(4,2,0,0)});
  parent.Children.Add(box);progressOverview.SizeChanged+=(s,e)=>LayoutProgressOverview();LayoutProgressOverview();
 }
 void LayoutProgressOverview(){
  bool wide=progressOverview.ActualWidth>=700;if(overviewWide==wide)return;overviewWide=wide;
  progressOverview.ColumnDefinitions.Clear();progressOverview.RowDefinitions.Clear();
  if(wide){
   foreach(double width in new[]{24.0,1.0,24.0,1.0})progressOverview.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(width,width==1?GridUnitType.Star:GridUnitType.Pixel)});
   progressOverview.RowDefinitions.Add(new RowDefinition{Height=new GridLength(62)});
  }else{
   progressOverview.ColumnDefinitions.Add(new ColumnDefinition());
   for(int n=0;n<4;n++)progressOverview.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});
  }
  UIElement[] elements={baneBar,baneLabel,overallBar,overallLabel};
  for(int n=0;n<4;n++){Grid.SetColumn(elements[n],wide?n:0);Grid.SetRow(elements[n],wide?0:(n==0?1:n==1?0:n==2?3:2));}
  foreach(var bar in new[]{baneBar,overallBar}){bar.Orientation=wide?Orientation.Vertical:Orientation.Horizontal;bar.Height=wide?56:6;bar.Width=wide?12:Double.NaN;bar.Margin=new Thickness(6,2,6,6);}
 }
 void UpdateProgressOverview(bool hasBane,int baneDone,int baneTotal,int done,int total,int kills){
  if(baneBar==null)return;
  baneBar.Value=baneTotal==0?0:100.0*baneDone/baneTotal;overallBar.Value=total==0?0:100.0*done/total;
  baneLabel.Text=hasBane?"Banestrike  "+baneDone+" / "+baneTotal+"\n"+(baneTotal-baneDone)+" remaining · "+kills.ToString("N0")+" kills left":"Banestrike: not in this export";
  overallLabel.Text="Overall  "+done+" / "+total+"\n"+overallBar.Value.ToString("0.0")+"% complete";
  showingLabel.Text="Showing "+filtered.Count+" achievements";
 }}
}
