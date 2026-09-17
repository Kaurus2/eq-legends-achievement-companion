using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
namespace LegendsCompanion {
public partial class MainWindow {
 Window setupWindow;
 void OfferSetup(){if(!settings.SetupComplete&&!File.Exists(settings.SourcePath)&&!Environment.GetCommandLineArgs().Any(a=>a=="--test"||a=="--preview"))OpenSetup();}
 void OpenSetup(){
  if(setupWindow!=null){setupWindow.Activate();return;}
  var w=new Window{Title="Welcome — set up your Companion",Owner=this,Width=590,Height=680,MinWidth=360,MinHeight=420,WindowStartupLocation=WindowStartupLocation.CenterOwner,Background=Palette.Background,Foreground=Palette.Text,FontFamily=FontFamily,FontSize=14};setupWindow=w;
  w.Resources.MergedDictionaries.Add(Resources);
  var panel=new StackPanel{Margin=new Thickness(18)};w.Content=new ScrollViewer{Content=panel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
  panel.Children.Add(new TextBlock{Text="Welcome to your Companion",FontSize=22,Foreground=Palette.Gold,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,12)});
  panel.Children.Add(Text("1. Choose your EverQuest Legends game folder",16));var folder=Input();folder.Text=settings.GameFolder??"";panel.Children.Add(folder);
  panel.Children.Add(Button("Browse folder…",()=>{using(var d=new System.Windows.Forms.FolderBrowserDialog()){d.SelectedPath=folder.Text;d.Description="Choose the EverQuest Legends game folder";if(d.ShowDialog()==System.Windows.Forms.DialogResult.OK)folder.Text=d.SelectedPath;}}));
  panel.Children.Add(Text("2. Turn on logging in game",16));panel.Children.Add(new TextBox{Text="/log on",IsReadOnly=true,Background=Palette.Surface,Foreground=Palette.Gold,Padding=new Thickness(7)});panel.Children.Add(Text("Enter this in game chat and press Enter. Look for the logging-enabled message.",12));
  panel.Children.Add(Text("3. Export your achievements",16));panel.Children.Add(Text("Open Inventory → Achiev. → Output To File. Include all categories and states if export options appear."));
  using(var stream=typeof(MainWindow).Assembly.GetManifestResourceStream("ExportInstructions")){if(stream!=null){var bitmap=new BitmapImage();bitmap.BeginInit();bitmap.CacheOption=BitmapCacheOption.OnLoad;bitmap.StreamSource=stream;bitmap.EndInit();panel.Children.Add(new Expander{Header="Show me where",Content=new Image{Source=bitmap,Stretch=Stretch.Uniform,MaxHeight=330}});}}
  panel.Children.Add(Text("4. Select your character",16));var picker=new ComboBox{Margin=new Thickness(0,4,0,8),MinHeight=32};panel.Children.Add(picker);
  TextBlock export=Text("✕ Achievement export needed"),log=Text("✕ Combat log needed"),activity=Text("Files are checked automatically.",12);panel.Children.Add(export);panel.Children.Add(log);panel.Children.Add(activity);
  var start=Button("Start tracking",()=>{});start.IsEnabled=false;panel.Children.Add(start);panel.Children.Add(Button("Set up later",()=>w.Close()));
  bool closed=false,busy=false,rebinding=false;string observedPath="";long observedLength=-1;bool liveSeen=false;
  Action update=()=>{var p=picker.SelectedItem as CharacterProfile;bool hasExport=p!=null&&File.Exists(p.ExportPath),hasLog=p!=null&&File.Exists(p.LogPath);export.Text=hasExport?"✓ Achievement export found":"✕ Achievement export needed";log.Text=hasLog?"✓ Combat log found":"✕ Combat log needed";export.Foreground=hasExport?Brushes.LimeGreen:Brushes.IndianRed;log.Foreground=hasLog?Brushes.LimeGreen:Brushes.IndianRed;start.IsEnabled=hasExport&&hasLog;
   if(hasLog){try{long length=new FileInfo(p.LogPath).Length;if(observedPath!=p.LogPath){observedPath=p.LogPath;observedLength=length;liveSeen=false;}else{if(length>observedLength)liveSeen=true;observedLength=length;}activity.Text=liveSeen?"✓ New log entries received":"Log found. Waiting for new game activity to confirm logging.";}catch(IOException){activity.Text="Waiting for the log to become readable.";}}else{observedPath="";observedLength=-1;liveSeen=false;activity.Text="Files are checked automatically. Keep this window open while you set up in game.";}};
  picker.SelectionChanged+=(s,e)=>{if(!rebinding)update();};
  var scan=new DispatcherTimer{Interval=TimeSpan.FromSeconds(2)};
  Action refresh=async ()=>{if(busy||closed)return;string path=folder.Text.Trim();if(!Directory.Exists(path)){picker.ItemsSource=null;update();return;}busy=true;try{var found=await Task.Run(()=>DiscoverProfiles(path));w.Dispatcher.Invoke(new Action(()=>{if(closed||path!=folder.Text.Trim())return;string key=(picker.SelectedItem as CharacterProfile)==null?"":((CharacterProfile)picker.SelectedItem).Key;rebinding=true;picker.ItemsSource=found;picker.SelectedItem=found.FirstOrDefault(p=>p.Key==key)??(found.Count==1?found[0]:null);rebinding=false;update();}));}catch(Exception error){w.Dispatcher.Invoke(new Action(()=>{if(!closed){picker.ItemsSource=null;update();activity.Text="Could not read folder: "+error.Message;}}));}finally{busy=false;}};
  scan.Tick+=(s,e)=>refresh();folder.TextChanged+=(s,e)=>{picker.ItemsSource=null;update();refresh();};
  start.Click+=(s,e)=>{var p=picker.SelectedItem as CharacterProfile;if(p==null)return;try{Data.Parse(File.ReadAllText(p.ExportPath));if(!File.Exists(p.LogPath))throw new Exception("Combat log is no longer available.");if(!ResolveEdits())return;settings.GameFolder=Path.GetFullPath(folder.Text.Trim());MergeProfiles(new System.Collections.Generic.List<CharacterProfile>{p});var chosen=settings.Profiles.First(x=>x.Key==p.Key);chosen.ExportPath=p.ExportPath;chosen.LogPath=p.LogPath;if(chosen.Live==null)chosen.Live=new LiveOptions();chosen.Live.Enabled=true;gameFolder.Text=settings.GameFolder;SwitchProfile(chosen,true);settings.SetupComplete=true;Data.Save(settingsPath,settings);w.Close();}catch(Exception error){activity.Text="Setup could not finish: "+error.Message;}};
  w.Closed+=(s,e)=>{closed=true;scan.Stop();setupWindow=null;};w.Show();scan.Start();refresh();
 }
}
}
