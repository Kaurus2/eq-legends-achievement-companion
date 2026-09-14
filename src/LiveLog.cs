using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Win32;
namespace LegendsCompanion {
public class LiveOptions {public bool Celebrations {get;set;}
 public bool PopupsEnabled {get;set;} public bool Enabled {get;set;} public string Path {get;set;} public int Volume {get;set;} public string Corner {get;set;}
 public List<MobMatch> Matches {get;set;}
 public LiveOptions(){PopupsEnabled=true;Celebrations=true;Enabled=true;Path="";Volume=45;Corner="Top center";Matches=new List<MobMatch>();}
}
public class MobMatch {public string Mob {get;set;} public string Achievement {get;set;} public MobMatch(){Mob=Achievement="";}}
// Tail only new complete lines. A bounded read keeps a busy log from blocking the UI.
public class LogTail {
 string path; long offset; DateTime created; string pending=""; byte[] anchor=new byte[0]; bool attached;
 public int Generation; public string Status="Waiting for log";
 public LogTail(string p){path=p;try{Attach();}catch(IOException){Status="Log temporarily unavailable — retrying";}catch(UnauthorizedAccessException){Status="Cannot read log — check file access";}}
 void Attach(){if(!File.Exists(path))return;using(var f=Open()){offset=f.Length;created=File.GetCreationTimeUtc(path);attached=true;Generation++;pending="";Anchor(f);}Status="Watching new log entries";}
 FileStream Open(){return new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);}
 void Anchor(FileStream f){int n=(int)Math.Min(64,offset);anchor=new byte[n];f.Position=offset-n;f.Read(anchor,0,n);}
 public List<string> Read(){var lines=new List<string>();try{if(!File.Exists(path)){attached=false;Status="Log not found — waiting";return lines;}if(!attached){Attach();return lines;}using(var f=Open()){
 bool reset=f.Length<offset||created!=File.GetCreationTimeUtc(path);if(!reset&&anchor.Length>0){var b=new byte[anchor.Length];f.Position=offset-b.Length;reset=f.Read(b,0,b.Length)!=b.Length||!b.SequenceEqual(anchor);}if(reset){Generation++;offset=f.Length;pending="";created=File.GetCreationTimeUtc(path);Anchor(f);Status="Log replaced or cleared — watching new entries";return lines;}
 f.Position=offset;var buffer=new byte[(int)Math.Min(262144,f.Length-offset)];int count=f.Read(buffer,0,buffer.Length);offset+=count;string text=pending+Encoding.Default.GetString(buffer,0,count);int last=text.LastIndexOf('\n');if(last>=0){lines.AddRange(text.Substring(0,last).Split('\n').Select(x=>x.TrimEnd('\r')));pending=text.Substring(last+1);}else pending=text;if(pending.Length>65536)pending="";Anchor(f);Status="Watching new log entries";
 }}catch(IOException){Status="Log temporarily unavailable — retrying";}catch(UnauthorizedAccessException){Status="Cannot read log — check file access";}return lines;}
}
public class LiveNotice {public string Name,Detail,Mob;public int? Current,Target;public bool Complete;}
public class KillProgress {
 Dictionary<string,int> counts=new Dictionary<string,int>();HashSet<string> completed=new HashSet<string>();
 public void Reset(){counts.Clear();completed.Clear();}
 public static string Message(string line){if(line.StartsWith("[",StringComparison.Ordinal)){int i=line.IndexOf("] ",StringComparison.Ordinal);if(i>=0)return line.Substring(i+2);}return line;}
 public static string Kill(string line){string s=Message(line);return s.StartsWith("You have slain ",StringComparison.Ordinal)&&s.EndsWith("!",StringComparison.Ordinal)?s.Substring(15,s.Length-16):"";}
 static string MobKey(string value){return Regex.Replace((value??"").Trim().ToLowerInvariant(),@"^(a|an|the)\s+","");}
 // Explicit observed variants map to creature families; ambiguous bandit races remain excluded.
 static bool Family(string mob,string requirement){string singular=MobKey(mob);if(singular=="bandit"||singular=="brigand")return false;
 if(singular=="bixie drone")singular="bixie";
 if(singular=="minotaur slaver"||singular=="minotaur guard"||singular=="minotaur lord"||singular=="minotaur hero")singular="minotaur";
 if(singular=="rock dervish")singular="dervish";
 string plural=singular=="dervish"?"dervishes":singular.EndsWith("y")?singular.Substring(0,singular.Length-1)+"ies":singular+"s";
 if(singular=="tentacle tormentor")plural="tentacle terrors"; if(singular=="fae drake")plural="fay drakes";
 return Regex.Split(requirement.Split('\t')[0].ToLowerInvariant(),@",|\band\b|\.").Any(t=>t.Trim()==plural||t.Trim()==singular);
 }
 public List<LiveNotice> Process(string line,List<Achievement> all,List<MobMatch> matches){var result=new List<LiveNotice>();string msg=Message(line);const string prefix="You have completed achievement: ";if(msg.StartsWith(prefix,StringComparison.Ordinal)){string name=msg.Substring(prefix.Length).Trim();if(name.Length>0&&completed.Add(Data.Normalize(name)))result.Add(new LiveNotice{Name=name,Detail="Confirmed by the game log · Refresh your achievement export to update the table",Complete=true});return result;}
 string mob=Kill(line);if(mob=="")return result;
 foreach(var a in all.Where(x=>!x.Complete&&x.Category.StartsWith("Slayer:")&&!completed.Contains(Data.Normalize(x.Name)))){
 var req=a.Requirements.Where(r=>!r.Optional&&!r.Complete&&r.Current.HasValue&&r.Target.HasValue).ToList();if(req.Count!=1)continue;
 bool custom=matches.Any(m=>MobKey(m.Mob)==MobKey(mob)&&String.Equals(m.Achievement,a.Name,StringComparison.OrdinalIgnoreCase));if(!custom&&!Family(mob,req[0].Text))continue;
 int n;counts.TryGetValue(a.Key,out n);counts[a.Key]=++n;
 result.Add(new LiveNotice{Name=a.Name,Mob=mob,Current=Math.Min(req[0].Target.Value,req[0].Current.Value+n),Target=req[0].Target.Value,Detail=mob+"  ·  +"+n+" estimated this session\nLast export: "+req[0].Current+" / "+req[0].Target+"  ·  credit not confirmed"});
 }return result;}
}
public class LiveMonitor:IDisposable {public event Action<List<LiveNotice>> Notified;public event Action ResetProgress;
 LiveOptions options;Func<List<Achievement>> achievements;Func<string> baseline;string lastBaseline;LogTail tail;KillProgress progress=new KillProgress();DispatcherTimer timer;ProgressToast toast;Queue<LiveNotice> queue=new Queue<LiveNotice>();DateTime next;int generation;int epoch;Task<LogBatch> pending;class LogBatch{public int Epoch,Generation;public List<LiveNotice> Notices=new List<LiveNotice>();}
 public string Status {get{return !options.Enabled?"Live updates off":tail==null?"Choose a log file":tail.Status+(options.PopupsEnabled?"":" · popups off");}}
 public LiveMonitor(LiveOptions o,Func<List<Achievement>> a,Func<string> b){options=o;achievements=a;baseline=b;lastBaseline=b();Restart();timer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(500)};timer.Tick+=(s,e)=>Tick();timer.Start();}
 string monitoringSettings="";string MonitoringKey(){return options.Enabled+"|"+options.Path+"|"+Data.Json().Serialize(options.Matches);}
 public void ApplySettings(){if(monitoringSettings!=MonitoringKey())Restart();else if(!options.PopupsEnabled&&toast!=null)toast.Hide();}
 public void Restart(){monitoringSettings=MonitoringKey();epoch++;pending=null;tail=null;queue.Clear();progress=new KillProgress();if(ResetProgress!=null)ResetProgress();if(toast!=null)toast.Hide();if(options.Enabled&&!String.IsNullOrWhiteSpace(options.Path)){tail=new LogTail(options.Path);generation=tail.Generation;}}
 void Tick(){try{
  if(!options.Enabled||tail==null)return;
  if(pending!=null&&!pending.IsCompleted)return;
  bool changed=lastBaseline!=baseline();if(changed){lastBaseline=baseline();progress=new KillProgress();epoch++;queue.Clear();if(ResetProgress!=null)ResetProgress();if(toast!=null)toast.Hide();}
  if(pending!=null){var completedTask=pending;pending=null;var result=completedTask.GetAwaiter().GetResult();if(result.Epoch==epoch){if(generation!=result.Generation){generation=result.Generation;queue.Clear();if(ResetProgress!=null)ResetProgress();}foreach(var notice in result.Notices){var retained=queue.Where(n=>n.Name!=notice.Name).ToList();queue.Clear();foreach(var n in retained)queue.Enqueue(n);if(queue.Count<50)queue.Enqueue(notice);}}}
  var currentTail=tail;var currentProgress=progress;var currentAchievements=achievements();var matches=(options.Matches??new List<MobMatch>()).ToList();int currentEpoch=epoch;
  pending=Task.Run(()=>{var b=new LogBatch{Epoch=currentEpoch};int before=currentTail.Generation;var lines=currentTail.Read();if(before!=currentTail.Generation)currentProgress.Reset();foreach(var line in lines)b.Notices.AddRange(currentProgress.Process(line,currentAchievements,matches));b.Generation=currentTail.Generation;return b;});
  if(queue.Count>0&&DateTime.UtcNow>=next){var batch=new List<LiveNotice>{queue.Dequeue()};Show(batch);next=DateTime.UtcNow.AddSeconds(3.2);}
 }catch(Exception e){if(tail!=null)tail.Status="Monitor retrying: "+e.Message;pending=null;}}
 void Show(List<LiveNotice> notices){if(Notified!=null)Notified(notices);if(!options.PopupsEnabled)return;if(toast==null)toast=new ProgressToast();toast.Display(notices,options);}
 public void Test(LiveOptions preview){if(toast==null)toast=new ProgressToast();toast.Display(new List<LiveNotice>{new LiveNotice{Name="You Keep Dragon Me Into This",Mob="a fae drake",Current=92,Target=100,Detail="a fae drake · +1 estimated this session\nExample only — no progress has been changed"}},preview);}
 public void TestCompletion(LiveOptions preview){if(toast==null)toast=new ProgressToast();toast.Display(new List<LiveNotice>{new LiveNotice{Name="Example achievement",Complete=true}},preview);}
 public static void CheckPopupIndependence(){var o=new LiveOptions{Enabled=true,PopupsEnabled=false};using(var m=new LiveMonitor(o,()=>new List<Achievement>(),()=>"test")){int count=0;m.Notified+=n=>count+=n.Count;m.Show(new List<LiveNotice>{new LiveNotice{Name="Test"}});if(count!=1||m.toast!=null||!o.Enabled)throw new Exception("Silent live updates failed");var original=m.progress;m.ApplySettings();if(original!=m.progress)throw new Exception("Popup-only change reset estimates");}if(!Data.Json().Deserialize<LiveOptions>("{\"Enabled\":true}").PopupsEnabled)throw new Exception("Legacy popup default lost");}
 public void Dispose(){epoch++;timer.Stop();queue.Clear();if(toast!=null)toast.Close();}
 public void Configure(Window owner,Action save){var w=new Window{Owner=owner,Title="Live achievement notifications",Width=700,Height=710,MinWidth=540,MinHeight=500,WindowStartupLocation=WindowStartupLocation.CenterOwner};var p=new StackPanel{Margin=new Thickness(18)};w.Content=new ScrollViewer{Content=p,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};Action<string> label=t=>p.Children.Add(new TextBlock{Text=t,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,8,0,6)});
 var enabled=new CheckBox{Content="Enable live updates",IsChecked=options.Enabled};p.Children.Add(enabled);var popups=new CheckBox{Content="Show popups + sounds",IsChecked=options.PopupsEnabled};p.Children.Add(popups);label("Combat log file (read-only)");var path=new TextBox{Text=options.Path,Padding=new Thickness(6)};p.Children.Add(path);var browse=new Button{Content="Browse…",HorizontalAlignment=HorizontalAlignment.Left,Margin=new Thickness(0,5,0,0)};browse.Click+=(s,e)=>{var d=new OpenFileDialog{Filter="Game logs (*.txt)|*.txt",FileName=path.Text};if(d.ShowDialog(w)==true)path.Text=d.FileName;};p.Children.Add(browse);label(Status);
 label("Popup position — Top center starts 5% down the primary screen");var corner=new ComboBox{ItemsSource=new[]{"Top center","Top right","Top left","Bottom right","Bottom left"},SelectedItem=options.Corner};p.Children.Add(corner);var celebrate=new CheckBox{Content="Completion tune + small fireworks",IsChecked=options.Celebrations};p.Children.Add(celebrate);label("Bing volume (0 = mute)");var volume=new Slider{Minimum=0,Maximum=100,Value=options.Volume,TickFrequency=5,IsSnapToTickEnabled=true};p.Children.Add(volume);
 label("Counts are estimates from your own ‘You have slain’ lines. Other players’ kills and ambiguous mob races are not guessed. Confirmed personal completions also trigger a popup. Export achievements again to update the main table; session estimates reset when that export changes.");
 label("Optional exact mob matches — enter a mob name from the log and an incomplete Slayer achievement name. Generic bandits can have different races: only add a match if every kill with that name qualifies. These matches affect notifications only.");var items=new ObservableCollection<MobMatch>((options.Matches??new List<MobMatch>()).Select(m=>new MobMatch{Mob=m.Mob,Achievement=m.Achievement}));var grid=new DataGrid{ItemsSource=items,AutoGenerateColumns=false,Height=150,CanUserAddRows=true,CanUserDeleteRows=true};grid.Columns.Add(new DataGridTextColumn{Header="Exact mob name",Binding=new System.Windows.Data.Binding("Mob"),Width=new DataGridLength(1,DataGridLengthUnitType.Star)});grid.Columns.Add(new DataGridTextColumn{Header="Achievement name",Binding=new System.Windows.Data.Binding("Achievement"),Width=new DataGridLength(2,DataGridLengthUnitType.Star)});p.Children.Add(grid);
 label("The overlay stays above other windows and lets clicks pass through. Use windowed or borderless game mode. Monitoring begins at the end of the log, so old kills are not replayed. Restarting monitoring clears session estimates.");var buttons=new WrapPanel();p.Children.Add(buttons);var test=new Button{Content="Test popup + bing",Padding=new Thickness(12,6,12,6),Margin=new Thickness(0,8,8,0)};test.Click+=(s,e)=>Test(new LiveOptions{Corner=(string)corner.SelectedItem??"Top center",Volume=(int)volume.Value});buttons.Children.Add(test);var completionTest=new Button{Content="Test completion",Padding=new Thickness(12,6,12,6),Margin=new Thickness(0,8,8,0)};completionTest.Click+=(s,e)=>TestCompletion(new LiveOptions{Corner=(string)corner.SelectedItem??"Top center",Volume=(int)volume.Value,Celebrations=celebrate.IsChecked==true});buttons.Children.Add(completionTest);var apply=new Button{Content="Save settings",Padding=new Thickness(12,6,12,6),Margin=new Thickness(0,8,8,0)};apply.Click+=(s,e)=>{grid.CommitEdit(DataGridEditingUnit.Cell,true);grid.CommitEdit(DataGridEditingUnit.Row,true);var valid=items.Where(m=>!String.IsNullOrWhiteSpace(m.Mob)||!String.IsNullOrWhiteSpace(m.Achievement)).ToList();if(valid.Any(m=>String.IsNullOrWhiteSpace(m.Mob)||!achievements().Any(a=>a.Category.StartsWith("Slayer:")&&String.Equals(a.Name,m.Achievement.Trim(),StringComparison.OrdinalIgnoreCase)))){MessageBox.Show(w,"Each match needs a mob name and an exact Slayer achievement name from your loaded export.");return;}options.PopupsEnabled=popups.IsChecked==true;options.Celebrations=celebrate.IsChecked==true;options.Enabled=enabled.IsChecked==true;options.Path=path.Text.Trim();options.Volume=(int)volume.Value;options.Corner=(string)corner.SelectedItem??"Top center";options.Matches=valid.Select(m=>new MobMatch{Mob=m.Mob.Trim(),Achievement=m.Achievement.Trim()}).ToList();save();ApplySettings();w.Close();};buttons.Children.Add(apply);w.ShowDialog();}
}
}
