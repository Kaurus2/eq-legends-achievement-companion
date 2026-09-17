using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
namespace LegendsCompanion {
public class CharacterProfile {
 public string Key {get;set;} public string Character {get;set;} public string Server {get;set;}
 public string ExportPath {get;set;} public string LogPath {get;set;} public DateTime LastActivity {get;set;}
 public LiveOptions Live {get;set;} public Dictionary<string,string> Notes {get;set;}
 public CharacterProfile(){ExportPath=LogPath="";Notes=new Dictionary<string,string>();}
 public override string ToString(){return Character+" · "+Server+(String.IsNullOrEmpty(ExportPath)?" — export needed":String.IsNullOrEmpty(LogPath)?" — log needed":"");}
}
public partial class MainWindow {
 TextBlock connectedStatus;ComboBox characterPicker;TextBox gameFolder;TextBlock profileStatus;bool choosingProfile;
 CharacterProfile ActiveProfile {get{return settings.Profiles.FirstOrDefault(p=>p.Key==settings.ActiveProfile);}}
 static CharacterProfile IdentifyProfile(string file,bool log){
  string stem=Path.GetFileNameWithoutExtension(file);
  if(log){if(!stem.StartsWith("eqlog_",StringComparison.OrdinalIgnoreCase))return null;stem=stem.Substring(6);}
  else{if(!stem.EndsWith("-Achievements",StringComparison.OrdinalIgnoreCase))return null;stem=stem.Substring(0,stem.Length-13);}
  int split=stem.IndexOf('_');if(split<1||split==stem.Length-1)return null;
  return new CharacterProfile{Key=stem.ToLowerInvariant(),Character=stem.Substring(0,split),Server=stem.Substring(split+1)};
 }
 public static List<CharacterProfile> DiscoverProfiles(string folder){
  var found=new Dictionary<string,CharacterProfile>(StringComparer.OrdinalIgnoreCase);
  var files=new List<string>();files.AddRange(Directory.GetFiles(folder,"*-Achievements.txt"));
  string logs=Path.Combine(folder,"Logs");if(Directory.Exists(logs))files.AddRange(Directory.GetFiles(logs,"eqlog_*.txt"));
  string nested=Path.Combine(logs,"eqlog");if(Directory.Exists(nested))files.AddRange(Directory.GetFiles(nested,"eqlog_*.txt"));
  foreach(string file in files){bool log=Path.GetFileName(file).StartsWith("eqlog_",StringComparison.OrdinalIgnoreCase);var candidate=IdentifyProfile(file,log);if(candidate==null)continue;
   CharacterProfile p;if(!found.TryGetValue(candidate.Key,out p)){p=candidate;found.Add(p.Key,p);}
   if(log){if(String.IsNullOrEmpty(p.LogPath)||File.GetLastWriteTimeUtc(file)>File.GetLastWriteTimeUtc(p.LogPath))p.LogPath=file;p.LastActivity=File.GetLastWriteTimeUtc(p.LogPath);}
   else p.ExportPath=file;
  }
  return found.Values.OrderByDescending(p=>p.LastActivity).ThenBy(p=>p.Character).ToList();
 }
 void PrepareProfiles(){
  if(settings.Profiles==null)settings.Profiles=new List<CharacterProfile>();
  if(String.IsNullOrEmpty(settings.GameFolder)&&File.Exists(settings.SourcePath))settings.GameFolder=Path.GetDirectoryName(settings.SourcePath);
  if(Directory.Exists(settings.GameFolder))MergeProfiles(DiscoverProfiles(settings.GameFolder));
  if(String.IsNullOrEmpty(settings.ActiveProfile)){
   var old=settings.Profiles.FirstOrDefault(p=>String.Equals(p.ExportPath,settings.SourcePath,StringComparison.OrdinalIgnoreCase));
   if(old!=null){settings.ActiveProfile=old.Key;old.Live=Data.Json().Deserialize<LiveOptions>(Data.Json().Serialize(settings.Live));if(research!=null)foreach(var r in research.Items)if(!String.IsNullOrEmpty(r.PersonalNotes))old.Notes[r.Key]=r.PersonalNotes;}
   else if(settings.Profiles.Count==1){settings.ActiveProfile=settings.Profiles[0].Key;settings.SourcePath=settings.Profiles[0].ExportPath;settings.Live.Path=settings.Profiles[0].LogPath;}
  }
 }
 void MergeProfiles(List<CharacterProfile> discovered){
  foreach(var p in discovered){var old=settings.Profiles.FirstOrDefault(x=>x.Key==p.Key);if(old==null)settings.Profiles.Add(p);else{if(!File.Exists(old.ExportPath))old.ExportPath=p.ExportPath;if(!File.Exists(old.LogPath))old.LogPath=p.LogPath;old.LastActivity=p.LastActivity;}}
 }
 void BuildProfileControls(Panel toolbar){
  characterPicker=new ComboBox{MinWidth=130,MaxWidth=260,Margin=new Thickness(0,0,4,4),Padding=new Thickness(6,4,6,4),ToolTip="Character · server — most recent combat log activity first"};var characterRow=new DockPanel{Margin=new Thickness(0,0,0,4)};characterPicker.MaxWidth=Double.PositiveInfinity;characterPicker.HorizontalAlignment=HorizontalAlignment.Stretch;characterRow.Children.Add(characterPicker);DockPanel.SetDock(characterRow,Dock.Top);
  characterPicker.SelectionChanged+=(s,e)=>{if(!choosingProfile&&characterPicker.SelectedItem is CharacterProfile)SwitchProfile((CharacterProfile)characterPicker.SelectedItem);};RefreshProfilePicker();
  var panel=new StackPanel();panel.Children.Add(Text("Game installation folder",14));gameFolder=Input();gameFolder.Text=settings.GameFolder??"";panel.Children.Add(gameFolder);
  var actions=new WrapPanel();actions.Children.Add(Button("Browse folder…",()=>{using(var d=new System.Windows.Forms.FolderBrowserDialog()){d.Description="Choose your EverQuest Legends installation folder";d.SelectedPath=gameFolder.Text;if(d.ShowDialog()==System.Windows.Forms.DialogResult.OK){gameFolder.Text=d.SelectedPath;ScanProfiles();}}}));actions.Children.Add(Button("Find characters",ScanProfiles));
  var setupHelp=new Border{Visibility=Visibility.Collapsed,Background=Palette.Surface,BorderBrush=Palette.Border,BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(5),Padding=new Thickness(10),Margin=new Thickness(0,4,0,8)};
  setupHelp.Child=new TextBox{Text="CREATE YOUR GAME FILES\n\n1. Turn on combat logging\nWhile logged into your character, type /log on in game chat and press Enter. Look for the logging-enabled message.\n\n2. Export your achievements\nOpen Inventory → Achiev. → Output To File. Include all categories and completed/incomplete achievements if export options appear.\n\n3. Connect the Companion\nChoose your game folder above, click Find characters, and select your character. Check that both files show connected, then enable live updates.\n\nClick Output To File again periodically to refresh confirmed progress. The combat log supplies live kill estimates; /log on does not export achievements.",IsReadOnly=true,TextWrapping=TextWrapping.Wrap,Background=Palette.Surface,Foreground=Palette.Text,BorderThickness=new Thickness(0)};
  var setupInfo=Button("ⓘ",()=>{setupHelp.Visibility=setupHelp.Visibility==Visibility.Visible?Visibility.Collapsed:Visibility.Visible;});setupInfo.ToolTip="How to enable logging and export achievements";actions.Children.Add(setupInfo);panel.Children.Add(actions);panel.Children.Add(setupHelp);
  profileStatus=Text("Choose the game folder once. Exports and combat logs are paired by character and server.",12);panel.Children.Add(profileStatus);connectedStatus=Text("",12);panel.Children.Add(connectedStatus);
  var advancedPanel=new StackPanel();
  int exportIndex=configContent.Children.IndexOf(source);
  for(int i=0;i<3;i++){var element=configContent.Children[exportIndex-1];configContent.Children.RemoveAt(exportIndex-1);advancedPanel.Children.Add(element);}
  int logIndex=configContent.Children.IndexOf(combatPath);
  for(int i=0;i<2;i++){var element=configContent.Children[logIndex-1];configContent.Children.RemoveAt(logIndex-1);advancedPanel.Children.Add(element);}
  var logActions=(Panel)configContent.Children[logIndex-1];var browseLog=logActions.Children[0];logActions.Children.RemoveAt(0);advancedPanel.Children.Add(browseLog);
  configContent.Children.Add(new Expander{Header="Advanced file settings",Content=advancedPanel,Margin=new Thickness(0,8,0,0)});
  configContent.Children.Insert(0,panel);source.TextChanged+=(s,e)=>UpdateConnections();combatPath.TextChanged+=(s,e)=>UpdateConnections();UpdateConnections();
 }
 void UpdateConnections(){if(connectedStatus==null)return;bool export=File.Exists(source.Text),log=File.Exists(combatPath.Text);connectedStatus.Inlines.Clear();connectedStatus.Inlines.Add(new System.Windows.Documents.Run(export?"✓":"✕"){Foreground=export?System.Windows.Media.Brushes.LimeGreen:System.Windows.Media.Brushes.IndianRed});connectedStatus.Inlines.Add(export?" Export connected · ":" Export needed · ");connectedStatus.Inlines.Add(new System.Windows.Documents.Run(log?"✓":"✕"){Foreground=log?System.Windows.Media.Brushes.LimeGreen:System.Windows.Media.Brushes.IndianRed});connectedStatus.Inlines.Add(log?" Combat log connected":" Combat log needed");connectedStatus.ToolTip=(export?source.Text:"Export achievements in game, then Find characters.")+"\n"+(log?combatPath.Text:"Use /log on in game, then Find characters.");}
 void RefreshProfilePicker(){choosingProfile=true;var choices=settings.Profiles.OrderByDescending(p=>p.LastActivity).ThenBy(p=>p.Character).Cast<object>().ToList();if(ActiveProfile==null)choices.Insert(0,"Choose character — set game folder");characterPicker.ItemsSource=choices;characterPicker.SelectedItem=(object)ActiveProfile??choices[0];choosingProfile=false;}
 void RememberProfile(){var p=ActiveProfile;if(p==null)return;p.ExportPath=settings.SourcePath;p.LogPath=settings.Live.Path;p.Live=Data.Json().Deserialize<LiveOptions>(Data.Json().Serialize(settings.Live));}
 void ScanProfiles(){try{if(!ResolveEdits())return;var discovered=DiscoverProfiles(gameFolder.Text.Trim());RememberProfile();settings.GameFolder=Path.GetFullPath(gameFolder.Text.Trim());MergeProfiles(discovered);Data.Save(settingsPath,settings);RefreshProfilePicker();profileStatus.Text=discovered.Count+" characters found. Missing exports: export achievements in game. Missing logs: use /log on.";if(ActiveProfile!=null)SwitchProfile(ActiveProfile,true);else if(discovered.Count==1)SwitchProfile(settings.Profiles.First(p=>p.Key==discovered[0].Key));}catch(Exception e){profileStatus.Text="Could not scan folder: "+e.Message;}}
 void SwitchProfile(CharacterProfile p,bool reload=false){
  if(p==ActiveProfile&&!reload)return;if(!ResolveEdits()){RefreshProfilePicker();return;}if(p!=ActiveProfile)RememberProfile();
  if(live!=null){live.Dispose();live=null;}recentStamps.Clear();stableTileOrder.Clear();ClearLiveTiles();Edit(null);binding=true;grid.UnselectAllCells();grid.SelectedItem=null;binding=false;ShrinkDetails();
  settings.ActiveProfile=p.Key;settings.SourcePath=p.ExportPath;settings.Live=p.Live==null?new LiveOptions():Data.Json().Deserialize<LiveOptions>(Data.Json().Serialize(p.Live));settings.Live.Path=p.LogPath;
  source.Text=p.ExportPath;combatPath.Text=p.LogPath;liveEnabled.IsChecked=settings.Live.Enabled;popupsEnabled.IsChecked=settings.Live.PopupsEnabled;soundEnabled.IsChecked=settings.Live.SoundsEnabled;SyncNotificationControls();
  achievements.Clear();joined.Clear();filtered.Clear();loadedHash=candidateHash="";loadedExportStamp=0;lastSuccess="Never";RefreshFilters();Apply();
  Data.Save(settingsPath,settings);if(File.Exists(p.ExportPath))LoadSource(true);else status.Text="Achievement export needed for "+p.Character+". Export achievements in game, then Find characters.";
  live=new LiveMonitor(settings.Live,()=>achievements,()=>settings.SourcePath+loadedHash+loadedExportStamp);live.Notified+=LiveProgress;live.SoundChanged+=SavePopupSound;live.ResetProgress+=ClearLiveTiles;RefreshProfilePicker();
 }
 bool SelectManualProfile(string path){
  var identified=IdentifyProfile(path,false);if(identified==null)return false;
  if(ActiveProfile!=null&&identified.Key==ActiveProfile.Key)return false;
  var p=settings.Profiles.FirstOrDefault(x=>x.Key==identified.Key);
  if(p==null){p=identified;settings.Profiles.Add(p);}p.ExportPath=Path.GetFullPath(path);
  string log=Path.Combine(Path.GetDirectoryName(path),"Logs","eqlog_"+identified.Character+"_"+identified.Server+".txt");if(File.Exists(log))p.LogPath=log;
  SwitchProfile(p);return true;
 }
 public void CheckProfiles(){
  string folder=Path.Combine(Home,"profile-fixtures");Directory.CreateDirectory(folder);Directory.CreateDirectory(Path.Combine(folder,"Logs"));
  string fixture="Slayer: Skill\nI\tTiny Foe\nI\t\tRats\t3/10\n";
  File.WriteAllText(Path.Combine(folder,"Alpha_one-Achievements.txt"),fixture);
  File.WriteAllText(Path.Combine(folder,"Alpha_two-Achievements.txt"),fixture.Replace("3/10","7/10"));
  File.WriteAllText(Path.Combine(folder,"Logs","eqlog_Alpha_one.txt"),"");
  File.WriteAllText(Path.Combine(folder,"Logs","eqlog_Beta_one.txt"),"");
  var found=DiscoverProfiles(folder);if(found.Count!=3||found.First(p=>p.Key=="alpha_one").LogPath==""||found.First(p=>p.Key=="beta_one").ExportPath!="")throw new Exception("Profile discovery mismatch");
  MergeProfiles(found);foreach(var p in settings.Profiles)p.Live=new LiveOptions{Enabled=false};
  SwitchProfile(settings.Profiles.First(p=>p.Key=="alpha_one"));if(achievements.Count!=1||achievements[0].Remaining!=7)throw new Exception("First character failed");
  string key=achievements[0].Key;SaveProfileNotes(key,"Only Alpha one");
  SwitchProfile(settings.Profiles.First(p=>p.Key=="alpha_two"));if(achievements[0].Remaining!=3||ProfileNotes(new Research{Key=key})!="")throw new Exception("Character data crossed profiles");
  SwitchProfile(settings.Profiles.First(p=>p.Key=="beta_one"));if(achievements.Count!=0)throw new Exception("Missing export showed another character");
  SwitchProfile(settings.Profiles.First(p=>p.Key=="alpha_one"));if(ProfileNotes(new Research{Key=key})!="Only Alpha one"||achievements[0].Remaining!=7)throw new Exception("Profile did not restore");
 }
 string ExportSnapshotPath(){return ActiveProfile==null?Path.Combine(store,"last-valid-export.txt"):Path.Combine(store,"profiles",ActiveProfile.Key,"last-valid-export.txt");}
 string ProfileNotes(Research r){if(ActiveProfile==null)return r.PersonalNotes??"";string value;return ActiveProfile.Notes.TryGetValue(r.Key,out value)?value:"";}
 void SaveProfileNotes(string key,string value){if(ActiveProfile!=null){ActiveProfile.Notes[key]=value;Data.Save(settingsPath,settings);}}
}
}
