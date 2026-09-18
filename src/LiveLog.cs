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
 public bool SoundsEnabled {get;set;} public bool PopupsEnabled {get;set;} public bool Enabled {get;set;} public string Path {get;set;} public int Volume {get;set;} public string Corner {get;set;}
 public List<MobMatch> Matches {get;set;}
 public LiveOptions(){SoundsEnabled=true;PopupsEnabled=true;Celebrations=true;Enabled=true;Path="";Volume=45;Corner="Top center";Matches=new List<MobMatch>();}
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
public class LiveNotice {public DateTime[] KillTimes;public string Name,Detail,Mob;public int? Current,Target;public bool Complete;}
public class KillProgress {
 Dictionary<string,List<DateTime>> killTimes=new Dictionary<string,List<DateTime>>();Dictionary<string,int> counts=new Dictionary<string,int>();HashSet<string> completed=new HashSet<string>();
 string partyExperienceStamp="";int partyExperienceLines;static string Stamp(string line){int end=line.IndexOf("] ",StringComparison.Ordinal);return line.StartsWith("[",StringComparison.Ordinal)&&end>0?line.Substring(0,end+1):"";}
 public string PetName="";public void Reset(){partyExperienceStamp="";partyExperienceLines=0;killTimes.Clear();counts.Clear();completed.Clear();PetName="";}
 public static string Message(string line){if(line.StartsWith("[",StringComparison.Ordinal)){int i=line.IndexOf("] ",StringComparison.Ordinal);if(i>=0)return line.Substring(i+2);}return line;}
 public static string Kill(string line){string s=Message(line);return s.StartsWith("You have slain ",StringComparison.Ordinal)&&s.EndsWith("!",StringComparison.Ordinal)?s.Substring(15,s.Length-16):"";}
 static string MobKey(string value){return Regex.Replace((value??"").Trim().ToLowerInvariant(),@"^(a|an|the)\s+","");}
 // Explicit observed variants map to creature families; ambiguous bandit races remain excluded.
 static bool Family(string mob,string requirement){string singular=MobKey(mob);if(singular=="bandit"||singular=="brigand")return false;
 if(new[]{"gundl","margyl darklin","peg leg","crytil dunfire","blyle bundin","glynda smeltpot","glynn smeltpot","barma dunfire"}.Contains(singular))singular="dwarves";
 if(new[]{"bink","gollee","leatherfoot medic","hamer","himmel","mardoon","rauner","jossle"}.Contains(singular))singular="halfling";
 if(new[]{"kobold runt","kobold scout","kobold shaman","kobold missionary","burly kobold","greater kobold","greater kobold shaman","kobold hunter","kobold king","kobold noble","kobold priest","kobold champion","kobold predator"}.Contains(singular))singular="kobold";
  if(requirement.StartsWith("Clockwork:",StringComparison.OrdinalIgnoreCase)){
  if(singular=="rebel clockwork"||singular=="rogue clockwork"||singular=="rogue cleaner"||singular=="runaway clockwork"||singular=="giant clockwork spider")return true;
  if(singular.StartsWith("clockwork "))singular=singular.Substring(10);
  else if(singular.StartsWith("clock work "))singular=singular.Substring(11);
  else return false;
  requirement=requirement.Substring("Clockwork:".Length).Trim();
 }
 if(new[]{"kerran `amir","kerran 'amir","kerran mujahed","kerran pasdar","kerran tiger spahi"}.Contains(singular))singular="kerran";
 if(singular=="mountain brownie"||singular=="brownie scout")singular="brownie";
 if(singular=="bixie drone")singular="bixie";
 if(singular=="gorge minotaur"||singular=="chasm minotaur"||singular=="minotaur slaver"||singular=="minotaur guard"||singular=="minotaur lord"||singular=="minotaur hero")singular="minotaur";
 if(singular=="rock dervish")singular="dervish";
 string plural=singular=="dervish"?"dervishes":singular.EndsWith("y")?singular.Substring(0,singular.Length-1)+"ies":singular+"s";
 if(singular=="tentacle tormentor")plural="tentacle terrors"; if(singular=="fae drake")plural="fay drakes";
 return Regex.Split(requirement.Split('\t')[0].ToLowerInvariant(),@",|\band\b|\.").Any(t=>t.Trim()==plural||t.Trim()==singular);
 }
 public List<LiveNotice> Process(string line,List<Achievement> all,List<MobMatch> matches){var result=new List<LiveNotice>();string msg=Message(line);if(partyExperienceLines>0&&--partyExperienceLines==0)partyExperienceStamp="";if(msg=="You gain party experience!"){partyExperienceStamp=Stamp(line);partyExperienceLines=12;return result;}var petReply=Regex.Match(msg,@"^([A-Za-z]+) told you, 'Attacking .+ Master\.'$");if(petReply.Success){PetName=petReply.Groups[1].Value;return result;}const string prefix="You have completed achievement: ";if(msg.StartsWith(prefix,StringComparison.Ordinal)){string name=msg.Substring(prefix.Length).Trim();if(name.Length>0&&completed.Add(Data.Normalize(name)))result.Add(new LiveNotice{Name=name,Detail="Confirmed by the game log · Refresh your achievement export to update the table",Complete=true});return result;}
 string mob=Kill(line);if(mob==""&&!String.IsNullOrEmpty(PetName)){var petKill=Regex.Match(msg,@"^(.+) has been slain by "+Regex.Escape(PetName)+@"!$");if(petKill.Success)mob=petKill.Groups[1].Value;}if(mob==""&&partyExperienceStamp!=""&&Stamp(line)==partyExperienceStamp){var groupKill=Regex.Match(msg,@"^(.+) has been slain by [A-Za-z]+!$");if(groupKill.Success)mob=groupKill.Groups[1].Value;}if(mob=="")return result;partyExperienceStamp="";partyExperienceLines=0;
 foreach(var a in all.Where(x=>!x.Complete&&x.Category.StartsWith("Slayer:")&&!completed.Contains(Data.Normalize(x.Name)))){
 var req=a.Requirements.Where(r=>!r.Optional&&!r.Complete&&r.Current.HasValue&&r.Target.HasValue).ToList();if(req.Count!=1)continue;
 bool custom=matches.Any(m=>MobKey(m.Mob)==MobKey(mob)&&String.Equals(m.Achievement,a.Name,StringComparison.OrdinalIgnoreCase));if(!custom&&!Family(mob,req[0].Text))continue;
 int n;counts.TryGetValue(a.Key,out n);counts[a.Key]=++n;List<DateTime> times;if(!killTimes.TryGetValue(a.Key,out times)){times=new List<DateTime>();killTimes[a.Key]=times;}var sample=KillPace.Record(times,KillPace.Time(line));
 result.Add(new LiveNotice{KillTimes=sample,Name=a.Name,Mob=mob,Current=Math.Min(req[0].Target.Value,req[0].Current.Value+n),Target=req[0].Target.Value,Detail=mob+"  ·  +"+n+" estimated this session\nLast export: "+req[0].Current+" / "+req[0].Target+"  ·  credit not confirmed"});
 }return result;}
}
public class LiveMonitor:IDisposable {public event Action<List<LiveNotice>> Notified;public event Action ResetProgress;public event Action SoundChanged;
 LiveOptions options;Func<List<Achievement>> achievements;Func<string> baseline;string lastBaseline;LogTail tail;KillProgress progress=new KillProgress();DispatcherTimer timer;ToastStack toast;Queue<LiveNotice> queue=new Queue<LiveNotice>();DateTime next;int generation;int epoch;Task<LogBatch> pending;class LogBatch{public int Epoch,Generation;public List<LiveNotice> Notices=new List<LiveNotice>();}
 public Func<List<MobMatch>> AdditionalMatches;
 public string Status {get{return !options.Enabled?"Live updates off":tail==null?"Choose a log file":tail.Status+(options.PopupsEnabled?"":" · popups off");}}
 public LiveMonitor(LiveOptions o,Func<List<Achievement>> a,Func<string> b){options=o;achievements=a;baseline=b;lastBaseline=b();Restart();timer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(500)};timer.Tick+=(s,e)=>Tick();timer.Start();}
 string monitoringSettings="";string MonitoringKey(){return options.Enabled+"|"+options.Path+"|"+Data.Json().Serialize(options.Matches);}
 public void ApplySettings(){if(monitoringSettings!=MonitoringKey())Restart();else if(!options.PopupsEnabled&&toast!=null)toast.Hide();}
 public void Restart(){monitoringSettings=MonitoringKey();epoch++;pending=null;tail=null;queue.Clear();progress=new KillProgress();if(ResetProgress!=null)ResetProgress();if(toast!=null)toast.Hide();if(options.Enabled&&!String.IsNullOrWhiteSpace(options.Path)){tail=new LogTail(options.Path);generation=tail.Generation;}}
 void Tick(){try{
  if(!options.Enabled||tail==null)return;
  if(pending!=null&&!pending.IsCompleted)return;
  bool changed=lastBaseline!=baseline();if(changed){lastBaseline=baseline();progress=new KillProgress{PetName=progress.PetName};epoch++;queue.Clear();if(ResetProgress!=null)ResetProgress();if(toast!=null)toast.Hide();}
  if(pending!=null){var completedTask=pending;pending=null;var result=completedTask.GetAwaiter().GetResult();if(result.Epoch==epoch){if(generation!=result.Generation){generation=result.Generation;queue.Clear();if(ResetProgress!=null)ResetProgress();}foreach(var notice in result.Notices){var retained=queue.Where(n=>n.Name!=notice.Name).ToList();queue.Clear();foreach(var n in retained)queue.Enqueue(n);if(queue.Count<50)queue.Enqueue(notice);}}}
  var currentTail=tail;var currentProgress=progress;var currentAchievements=achievements();var matches=(options.Matches??new List<MobMatch>()).ToList();if(AdditionalMatches!=null)matches.AddRange(AdditionalMatches());int currentEpoch=epoch;
  pending=Task.Run(()=>{var b=new LogBatch{Epoch=currentEpoch};int before=currentTail.Generation;var lines=currentTail.Read();if(before!=currentTail.Generation)currentProgress.Reset();foreach(var line in lines)b.Notices.AddRange(currentProgress.Process(line,currentAchievements,matches));b.Generation=currentTail.Generation;return b;});
  if(queue.Count>0&&DateTime.UtcNow>=next){var batch=new List<LiveNotice>();while(queue.Count>0)batch.Add(queue.Dequeue());Show(batch);next=DateTime.UtcNow;}
 }catch(Exception e){if(tail!=null)tail.Status="Monitor retrying: "+e.Message;pending=null;}}
 void Show(List<LiveNotice> notices){if(Notified!=null)Notified(notices);if(!options.PopupsEnabled){foreach(var n in notices)ProgressToast.PlayNotice(options,n);return;}if(toast==null)toast=new ToastStack();toast.SoundChanged=()=>{if(SoundChanged!=null)SoundChanged();};toast.Display(notices,options);}
 public void Test(LiveOptions preview){if(toast==null)toast=new ToastStack();toast.Display(new List<LiveNotice>{new LiveNotice{Name="You Keep Dragon Me Into This",Mob="a fae drake",Current=92,Target=100,Detail="a fae drake · +1 estimated this session\nExample only — no progress has been changed"}},preview);}
 public void TestCompletion(LiveOptions preview){if(toast==null)toast=new ToastStack();toast.Display(new List<LiveNotice>{new LiveNotice{Name="Example achievement",Complete=true}},preview);}
 public static void CheckPopupIndependence(){var o=new LiveOptions{Enabled=true,PopupsEnabled=false};using(var m=new LiveMonitor(o,()=>new List<Achievement>(),()=>"test")){int count=0;m.Notified+=n=>count+=n.Count;m.Show(new List<LiveNotice>{new LiveNotice{Name="Test"}});if(count!=1||m.toast!=null||!o.Enabled)throw new Exception("Silent live updates failed");var original=m.progress;m.ApplySettings();if(original!=m.progress)throw new Exception("Popup-only change reset estimates");}if(!Data.Json().Deserialize<LiveOptions>("{\"Enabled\":true}").PopupsEnabled)throw new Exception("Legacy popup default lost");}
 public void Dispose(){epoch++;timer.Stop();queue.Clear();if(toast!=null)toast.Close();}

}
}
