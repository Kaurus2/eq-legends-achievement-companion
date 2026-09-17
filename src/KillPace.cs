using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
namespace LegendsCompanion {
public static class KillPace {
 public static DateTime Time(string line){DateTime time;int end=line.IndexOf(']');if(line.StartsWith("[")&&end>1&&DateTime.TryParseExact(line.Substring(1,end-1),"ddd MMM dd HH:mm:ss yyyy",CultureInfo.InvariantCulture,DateTimeStyles.AllowWhiteSpaces|DateTimeStyles.AssumeLocal,out time))return time.ToUniversalTime();return DateTime.UtcNow;}
 public static DateTime[] Record(List<DateTime> times,DateTime now){if(times.Count>0&&(now<times[times.Count-1]||(now-times[times.Count-1]).TotalSeconds>90))times.Clear();times.Add(now);times.RemoveAll(t=>(now-t).TotalMinutes>5);if(times.Count>500)times.RemoveRange(0,times.Count-500);return times.ToArray();}
 public static string Describe(LiveNotice notice,DateTime now){
  if(notice==null)return "Learning your pace — waiting for qualifying kills.";
  if(notice.Complete)return "Achievement complete in the game log.";
  if(!notice.Current.HasValue||!notice.Target.HasValue)return "No kill-count estimate for this achievement.";
  int left=Math.Max(0,notice.Target.Value-notice.Current.Value);if(left==0)return "Estimated target reached — refresh your achievement export.";
  var times=notice.KillTimes??new DateTime[0];if(times.Length>0&&(now-times.Last()).TotalSeconds>90)return left+" left · Paused — no recent qualifying kills.";
  times=times.Where(t=>(now-t).TotalMinutes<=5&&t<=now).ToArray();
  double seconds=times.Length<2?0:(now-times[0]).TotalSeconds;
  if(times.Length<5||seconds<30)return left+" left · Learning your pace…";
  double rate=(times.Length-1)*60.0/seconds;double minutes=left/rate;
  string eta=minutes<1?"under 1 minute":minutes<60?Math.Ceiling(minutes)+" minutes":Math.Floor(minutes/60)+"h "+Math.Ceiling(minutes%60)+"m";
  return left+" left · "+rate.ToString("0.0",CultureInfo.CurrentCulture)+" kills/min · about "+eta;
 }
}
public partial class MainWindow {
 TextBlock killPaceText;
 void BuildKillPace(StackPanel parent){killPaceText=Text("",13);killPaceText.Foreground=Palette.Gold;killPaceText.ToolTip="Estimate from the last five minutes of qualifying kills. Learns after five kills over at least 30 seconds; pauses after 90 seconds without a qualifying kill. A fresh export starts a new sample.";parent.Children.Add(killPaceText);}
 void UpdateKillPace(){if(killPaceText==null)return;killPaceText.Visibility=editing!=null&&editing.A.Category.StartsWith("Slayer:")?Visibility.Visible:Visibility.Collapsed;if(editing==null)return;LiveNotice notice;liveNotices.TryGetValue(editing.A.Key,out notice);killPaceText.Text=editing.A.Complete?"Achievement complete.":settings.Live==null||!settings.Live.Enabled?"Enable live updates to estimate your kill pace.":KillPace.Describe(notice,DateTime.UtcNow);}
}
}
