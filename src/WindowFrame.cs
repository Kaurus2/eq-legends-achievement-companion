using System;
using System.Linq;
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
  configButton=new Button{Content=ClockworkGear(),ToolTip="Configuration",Width=32,Height=28,FontSize=17,Padding=new Thickness(0),Margin=new Thickness(0,2,2,2)};WindowChrome.SetIsHitTestVisibleInChrome(configButton,true);configButton.Click+=(s,e)=>ToggleConfiguration();buttons.Children.Add(configButton);
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
  frameOpacitySlider.Width=72;frameOpacitySlider.VerticalAlignment=VerticalAlignment.Center;opacityRow.Children.Add(frameOpacitySlider);opacityRow.Children.Add(opacityPercent);WindowChrome.SetIsHitTestVisibleInChrome(opacityRow,true);DockPanel.SetDock(opacityRow,Dock.Right);captionArea.Children.Insert(Math.Min(1,captionArea.Children.Count),opacityRow);
  Action placeOpacity=()=>{var parent=opacityRow.Parent as Panel;if(parent!=null)parent.Children.Remove(opacityRow);if(Width<850)trackerToolbar.Children.Insert(0,opacityRow);else captionArea.Children.Insert(Math.Min(1,captionArea.Children.Count),opacityRow);};SizeChanged+=(s,e)=>placeOpacity();placeOpacity();
  shell.Children.Add(content);Topmost=settings.AlwaysOnTop;UpdatePin();
 }
 Image ClockworkGear(){
  using(var stream=System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("ClockworkGear")){
   var bitmap=new System.Windows.Media.Imaging.BitmapImage();bitmap.BeginInit();bitmap.DecodePixelWidth=96;bitmap.CacheOption=System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;bitmap.StreamSource=stream;bitmap.EndInit();bitmap.Freeze();
   return new Image{Source=bitmap,Width=24,Height=24,Stretch=Stretch.Uniform};
  }
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
 } Expander summaryExpander;bool summaryInHeader;Grid progressOverview;ProgressBar baneBar,overallBar;TextBlock baneLabel,overallLabel;
 FrameworkElement SummaryBar(string title,Brush accent,out ProgressBar bar,out TextBlock counts){
  var body=new Grid{Height=38};
  body.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(92)});body.ColumnDefinitions.Add(new ColumnDefinition());
  var name=new TextBlock{Text=title,Foreground=accent,FontWeight=FontWeights.SemiBold,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(8,0,0,0)};body.Children.Add(name);
  var area=new Grid();Grid.SetColumn(area,1);body.Children.Add(area);
  bar=new ProgressBar{Minimum=0,Maximum=100,Margin=new Thickness(3),VerticalAlignment=VerticalAlignment.Stretch,Foreground=accent,Background=Palette.Border,Opacity=.30};area.Children.Add(bar);
  counts=new TextBlock{Foreground=Palette.Text,VerticalAlignment=VerticalAlignment.Center,HorizontalAlignment=HorizontalAlignment.Stretch,TextAlignment=TextAlignment.Center,FontSize=12,Margin=new Thickness(5,0,5,0)};area.Children.Add(counts);
  return new Border{BorderBrush=Palette.Border,BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(5),Background=Palette.Surface,Child=body,Margin=new Thickness(0,0,0,5)};
 }
 Grid banestrikeSegments;FrameworkElement focusSummaryBar;ProgressBar[] milestoneBars=new ProgressBar[3];TextBlock[] milestoneCounts=new TextBlock[3];
 static readonly string[] milestoneNames={"Progressive","Highly Decorated","A Force of Nature"};
 FrameworkElement BuildBanestrikeSegments(){
  banestrikeSegments=new Grid{Margin=new Thickness(0,0,0,5),Visibility=Visibility.Collapsed};
  for(int i=0;i<3;i++){
   banestrikeSegments.ColumnDefinitions.Add(new ColumnDefinition());
   var content=new StackPanel{Margin=new Thickness(5)};
   content.Children.Add(new TextBlock{Text=milestoneNames[i],FontSize=11,Height=30,TextWrapping=TextWrapping.Wrap,TextAlignment=TextAlignment.Center,Foreground=Palette.Text});
   milestoneBars[i]=new ProgressBar{Minimum=0,Maximum=100,Height=6,Foreground=Palette.Blue,Background=Palette.Border};content.Children.Add(milestoneBars[i]);
   milestoneCounts[i]=new TextBlock{FontSize=11,TextAlignment=TextAlignment.Center,Foreground=Palette.Gold,Margin=new Thickness(0,4,0,0)};content.Children.Add(milestoneCounts[i]);
   var cell=new Border{BorderBrush=Palette.Border,BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(5),Background=Palette.Surface,Margin=new Thickness(i==0?0:3,0,0,0),Child=content};Grid.SetColumn(cell,i);banestrikeSegments.Children.Add(cell);
  }return banestrikeSegments;
 }
 void UpdateBanestrikeSegments(bool visible){
  focusSummaryBar.Visibility=visible?Visibility.Collapsed:Visibility.Visible;banestrikeSegments.Visibility=visible?Visibility.Visible:Visibility.Collapsed;
  for(int i=0;i<3;i++){
   var a=achievements.FirstOrDefault(x=>x.Category=="Slayer: General"&&Data.Normalize(x.Name)==Data.Normalize(milestoneNames[i]));
   var req=a==null?new System.Collections.Generic.List<Requirement>():a.Requirements.Where(q=>!q.Optional&&!String.IsNullOrEmpty(q.Reference)).ToList();int done=req.Count(q=>q.Complete);
   milestoneBars[i].Value=a!=null&&a.Complete?100:req.Count==0?0:100.0*done/req.Count;
   milestoneBars[i].Foreground=a!=null&&a.Complete?Palette.Gold:Palette.Blue;
   milestoneCounts[i].Text=a==null?"Not in export":a.Complete?"Complete":req.Count==0?"No checklist":(req.Count-done)+" left · +1 rank";
   milestoneBars[i].ToolTip=milestoneNames[i]+": "+done+" / "+req.Count+" required achievements complete";
  }
 }
 string summaryDetails="";TextBlock focusBarTitle;
 void BuildProgressOverview(StackPanel parent){
  progressOverview=new Grid{Margin=new Thickness(4,0,8,4)};var bars=new StackPanel();
  var focus=SummaryBar("Focus",Palette.Gold,out baneBar,out baneLabel);focusBarTitle=(TextBlock)((Grid)((Border)focus).Child).Children[0];focusBarTitle.TextTrimming=TextTrimming.CharacterEllipsis;focusBarTitle.ToolTip="Selected focus";
  focusSummaryBar=focus;bars.Children.Add(BuildBanestrikeSegments());bars.Children.Add(focus);bars.Children.Add(SummaryBar("Overall",Palette.Blue,out overallBar,out overallLabel));progressOverview.Children.Add(bars);
  tileCount.Visibility=Visibility.Collapsed;trackerHeader.Children.Add(progressOverview);Grid.SetRow(progressOverview,1);
  var heading=new StackPanel{Orientation=Orientation.Horizontal};heading.Children.Add(new TextBlock{Text="Achievement summary",VerticalAlignment=VerticalAlignment.Center});var info=Button("ⓘ",()=>ShowText("Export details",summaryDetails));info.ToolTip="Export details";info.Padding=new Thickness(5,0,5,0);heading.Children.Add(info);
  summaryExpander=new Expander{Header=heading,Content=summary,Margin=new Thickness(4),VerticalAlignment=VerticalAlignment.Top};var summaryRow=new Grid();summaryRow.ColumnDefinitions.Add(new ColumnDefinition());summaryRow.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});summaryRow.Children.Add(summaryExpander);configContent.Children.Remove(recentCheck);recentCheck.Content="Auto shuffle";recentCheck.VerticalAlignment=VerticalAlignment.Top;recentCheck.Margin=new Thickness(5,8,5,0);recentCheck.FontSize=12;Grid.SetColumn(recentCheck,1);summaryRow.Children.Add(recentCheck);headerSummary.Content=summaryRow;
  trackerHeader.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});trackerHeader.SizeChanged+=delegate{PlaceSummary();};Loaded+=delegate{PlaceSummary();};
 }
 void PlaceSummary(){
  bool wide=trackerHeader.ActualWidth>=850;summaryInHeader=wide;
  trackerHeader.ColumnDefinitions[0].Width=wide?new GridLength(350):new GridLength(1,GridUnitType.Star);trackerHeader.ColumnDefinitions[1].Width=wide?new GridLength(1,GridUnitType.Star):new GridLength(0);
  Grid.SetColumn(headerSummary,wide?1:0);Grid.SetRow(headerSummary,wide?0:2);Grid.SetRowSpan(headerSummary,wide?2:1);
 }
 void UpdateFocusLine(){
  if(tileCount==null||group==null||baneBar==null)return;var rows=joined.Where(GroupMatch).ToList();int done=rows.Count(r=>r.A.Complete),total=rows.Count;string focus=Pick(group);
  UpdateBanestrikeSegments(focus=="BANESTRIKE");
  focusBarTitle.Text=focus=="ALL"?"All focus":System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(focus.ToLowerInvariant());focusBarTitle.ToolTip=focus;
  baneBar.Value=total==0?0:100.0*done/total;baneLabel.Text=done+" complete · "+(total-done)+" remaining";
  int overall=achievements.Count(a=>a.Complete);overallBar.Value=achievements.Count==0?0:100.0*overall/achievements.Count;overallLabel.Text=overall+" complete · "+(achievements.Count-overall)+" remaining";
  summary.Text=focus+": "+(total-done)+" achievements remaining · "+rows.Where(r=>!r.A.Complete&&r.A.Remaining.HasValue).Sum(r=>r.A.Remaining.Value).ToString("N0")+" tracked actions left\nNearly finished: "+rows.Count(r=>!r.A.Complete&&r.A.Progress>=.9)+" at 90% or higher\nShowing "+filtered.Count+" achievements with your filters.";
  if(focus=="BANESTRIKE")summary.Text=Data.BanestrikeSummary(achievements)+"\nShowing "+filtered.Count+" achievements with your filters.";
  baneBar.ToolTip=baneBar.Value.ToString("0.0")+"% complete";overallBar.ToolTip=overallBar.Value.ToString("0.0")+"% complete";
 }
 void UpdateProgressOverview(bool hasBane,int baneDone,int baneTotal,int done,int total,int kills){UpdateFocusLine();}
}}