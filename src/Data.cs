using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace LegendsCompanion {
public class Requirement {
 public string Text {get;set;} public bool Complete {get;set;} public bool Optional {get;set;}
 public int? Current {get;set;} public int? Target {get;set;} public string Reference {get;set;}
}
public class Achievement {
 public string Key {get;set;} public string Name {get;set;} public string Category {get;set;} public bool Complete {get;set;}
 public List<Requirement> Requirements {get;set;} public Achievement(){Requirements=new List<Requirement>();}
 public int? Remaining {get {if(Complete)return 0;var r=Requirements.Where(x=>!x.Optional && !x.Complete).ToList(); if(r.Any(x=>!x.Target.HasValue))return null;return r.Count==0?(int?)null:r.Sum(x=>Math.Max(0,x.Target.Value-x.Current.Value));}}
 public double Progress {get {if(Complete)return 1;var r=Requirements.Where(x=>!x.Optional && !x.Text.StartsWith("This achievement",StringComparison.OrdinalIgnoreCase)).ToList();if(r.Count==0)return 0;return r.Average(x=>x.Complete?1:x.Target.HasValue && x.Target.Value>0?Math.Min(1,(double)x.Current.Value/x.Target.Value):0);}}
 public string Basis {get {return Complete?"Complete in TXT":Requirements.Any(x=>x.Target.HasValue)?"Counter estimate":"Checklist estimate; alternatives may apply";}}
}
public class Farm : System.ComponentModel.INotifyPropertyChanged { public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged; private bool selected; public bool Selected {get{return selected;}set{selected=value;if(PropertyChanged!=null)PropertyChanged(this,new System.ComponentModel.PropertyChangedEventArgs("Selected"));}}
 public string PortRoute {get;set;} public string Kind {get;set;} public string Zone {get;set;} public string Region {get;set;} public string Mobs {get;set;} public string Portal {get;set;}
 public string Via {get;set;} public string Evidence {get;set;} public string Faction {get;set;} public Farm(){PortRoute=Zone=Region=Mobs=Portal=Via=Evidence="";Faction="Unknown";Kind="Alternative";}
}
public class Research {
 public string Key {get;set;} public string Name {get;set;} public string Category {get;set;}
 public List<Farm> Farms {get;set;} public string Risk {get;set;} public string Tier {get;set;} public string Era {get;set;}
 public string Class {get;set;} public string Race {get;set;} public string Notes {get;set;} public string PersonalNotes {get;set;} public string Sources {get;set;}
 public string Bottleneck {get;set;} public int Difficulty {get;set;} public int Travel {get;set;}
 public Research(){Farms=new List<Farm>();Risk="Unknown";Tier="Automatic";Era="Unknown";Class=Race=Notes=PersonalNotes=Sources=Bottleneck="";Difficulty=3;Travel=3;}
}
public class ResearchFile {public int Version {get;set;} public List<Research> Items {get;set;} public List<string> AppliedUpdates {get;set;} public ResearchFile(){Version=1;Items=new List<Research>();AppliedUpdates=new List<string>();}}
public class Settings {public Dictionary<string,List<string>> ManualTileOrders {get;set;} public SavedFilters SavedFilters {get;set;} public bool DarkMode {get;set;} public string GameFolder {get;set;} public string ActiveProfile {get;set;} public List<CharacterProfile> Profiles {get;set;} public bool WindowPlacementSaved {get;set;} public double WindowLeft {get;set;} public double WindowTop {get;set;} public double WindowWidth {get;set;} public double WindowHeight {get;set;} public bool WindowMaximized {get;set;} public bool AlwaysOnTop {get;set;} public bool TransparencyEnabled {get;set;} public double TrackerOpacity {get;set;} public bool TileLayoutInitialized {get;set;} public bool RecentFirst {get;set;} public LiveOptions Live {get;set;} public string SourcePath {get;set;} public bool FiltersHidden {get;set;} public bool TableOnly {get;set;} public Settings(){SourcePath="";TrackerOpacity=.75;}}
public class Row {
 public Achievement A; public Research R; public bool Required {get;set;} public bool Optional {get;set;} public double Score {get;set;}
 public string Achievement {get{return A.Name;}} public string Category {get{return A.Category;}}
 public string State {get{return A.Complete?"Complete":"Incomplete";}} public string Progress {get{return A.Progress.ToString("P0");}}
 public string Remaining {get{return A.Remaining.HasValue?A.Remaining.Value.ToString("N0"):"Unspecified";}}
 public List<Farm> DisplayOverride; public List<Farm> Candidates {get{return (R.Farms.Count>0?R.Farms:Guides.Destinations(A)).OrderByDescending(f=>f.Selected).ThenByDescending(f=>(f.Evidence??"").StartsWith("User EQL observation")).ThenBy(f=>(f.Faction??"").Contains("loss")?1:0).ToList();}} public List<Farm> Preferred {get{var c=Candidates;var stops=c.Where(f=>f.Kind=="Required stop").ToList();if(stops.Count>0)return stops;return c.Any(f=>f.Selected)?c.Where(f=>f.Selected).Take(1).ToList():c.Take(1).ToList();}} public List<Farm> Locations {get{return (DisplayOverride??Preferred).SelectMany(f=>(f.Zone??"").Split(new[]{';',','},StringSplitOptions.RemoveEmptyEntries).Select(z=>new Farm{PortRoute=f.PortRoute,Kind=f.Kind,Zone=Guides.Zone(z.Trim()),Region=Guides.Region(Guides.Zone(z.Trim()))!=""?Guides.Region(Guides.Zone(z.Trim())):f.Region,Mobs=f.Mobs,Portal=f.Portal,Via=f.Via,Evidence=f.Evidence,Faction=f.Faction,Selected=f.Selected})).ToList();}}
 public string PortsAndRoute {get{return Locations.Count==0?(Guides.NoFixedLocation(A)?"Not applicable":Candidates.Count==0?"No researched destination yet":"No destination matches the current filters"):String.Join("\n",Locations.Select(f=>(Locations.Count>1?f.Zone+"\n":"")+TravelGuide.ForFarm(f)));}} public string ResearchGaps {get{return Guides.Gaps(this);}}
 public string Location {get{return Locations.Count==0?(Guides.NoFixedLocation(A)?"No fixed location":Candidates.Count>0?"No matching location":"Location needed"):String.Join("; ",Locations.Select(x=>Guides.Zone(x.Zone)).Distinct());}}
 public string Mobs {get {var named=Locations.Select(f=>f.Mobs).Where(s=>!String.IsNullOrWhiteSpace(s)).Distinct().ToList();if(named.Count>0)return String.Join("; ",named);return String.Join("; ",A.Requirements.Where(r=>String.IsNullOrEmpty(r.Reference)&&!r.Text.StartsWith("This achievement",StringComparison.OrdinalIgnoreCase)).Select(r=>r.Text.Split('\t')[0]));}}
 public string LocationEvidence {get{return Locations.Count==0?(Guides.NoFixedLocation(A)?"Not applicable":"Needs research"):R.Farms.Count==0?"Guide / TXT destination":R.Farms.All(f=>(f.Evidence??"").StartsWith("User EQL observation"))?"User observed":"Verify location / credit";}}
 public string CampFaction {get{return Locations.Count==0?(Guides.NoFixedLocation(A)?"Not applicable":"Unknown"):String.Join("; ",Locations.Select(f=>f.Zone+": "+(String.IsNullOrWhiteSpace(f.Faction)?"Unknown":f.Faction)));}}
 public string Risk {get{return R.Risk=="Unknown"&&(A.Category=="General: Level"||A.Category=="General: Skills"||A.Category.StartsWith("Tradeskill:"))?"Not applicable":R.Risk=="Unknown"&&Locations.Count>0&&Locations.All(f=>!String.IsNullOrWhiteSpace(f.Faction)&&f.Faction!="Unknown")?"See location notes":R.Risk;}} public string Era {get{return R.Era=="Unknown"&&A.Requirements.Any(q=>q.Text.Contains("Future Placeholder"))?"Quest placeholder (TXT)":R.Era;}}
 public string Tier {get {if(R.Tier!="Automatic")return R.Tier;if(!A.Category.StartsWith("Slayer:"))return "Checklist / progression";if(R.Era=="Unavailable"||R.Bottleneck!="")return "E - Defer / Bottleneck";if(R.Risk=="High")return "D - Faction Cleanup";if(A.Remaining.HasValue && A.Remaining<=10)return "A - Quick Win";if(A.Remaining.HasValue && A.Remaining<=50)return "B - Efficient";return "C - Grind / Stack";}}
 public string Why {get{return (Required?"Required Progressive; ":"")+Tier+"; "+(R.Era=="Unknown"?"verify era; ":"")+(Locations.Count==0?(Guides.NoFixedLocation(A)?"no fixed location":"research location"):Location);}}
}
public static class Data {
 public static bool HasResearch(Research r){return r.Farms.Count>0 || !String.IsNullOrWhiteSpace(r.Notes) || !String.IsNullOrWhiteSpace(r.Sources) || !String.IsNullOrWhiteSpace(r.Class) || !String.IsNullOrWhiteSpace(r.Race) || !String.IsNullOrWhiteSpace(r.Bottleneck) || r.Risk!="Unknown" || r.Era!="Unknown" || r.Tier!="Automatic" || r.Difficulty!=3 || r.Travel!=3;}
 public static string Normalize(string s){return Regex.Replace((s??"").ToLowerInvariant(),"[^a-z0-9]","");}
 public static string Key(string c,string n){return Normalize(c)+"|"+Normalize(n);}
 public static List<Achievement> Parse(string text){
  var list=new List<Achievement>();string category="";Achievement current=null;int lineNo=0;
  foreach(var raw in text.TrimStart('\uFEFF').Split('\n')){lineNo++;var line=raw.TrimEnd('\r');if(String.IsNullOrWhiteSpace(line))continue;
   var m=Regex.Match(line,@"^([CI])\t([^\t].*)$");
   if(m.Success){if(category=="")throw new Exception("Achievement without category at line "+lineNo);current=new Achievement{Name=m.Groups[2].Value.Trim(),Category=category,Complete=m.Groups[1].Value=="C"};current.Key=Key(category,current.Name);list.Add(current);continue;}
   m=Regex.Match(line,@"^([CI])\t\t(.+)$");
   if(m.Success){if(current==null)throw new Exception("Requirement without achievement at line "+lineNo);var r=new Requirement{Text=m.Groups[2].Value,Complete=m.Groups[1].Value=="C"};r.Optional=r.Text.IndexOf("(Optional)",StringComparison.OrdinalIgnoreCase)>=0;
    var counter=Regex.Match(r.Text,@"\t([\d,]+)/([\d,]+)$");if(counter.Success){r.Current=Int32.Parse(counter.Groups[1].Value.Replace(",",""));r.Target=Int32.Parse(counter.Groups[2].Value.Replace(",",""));if(r.Target<=0)throw new Exception("Invalid counter at line "+lineNo);}
    var reference=Regex.Match(r.Text,"Complete the achievement \"([^\"]+)\"",RegexOptions.IgnoreCase);r.Reference=reference.Success?Normalize(reference.Groups[1].Value):"";current.Requirements.Add(r);continue;}
   if(line.Contains("\t") || line.StartsWith("C\t") || line.StartsWith("I\t"))throw new Exception("Unrecognized export line "+lineNo);
   category=line.Trim();current=null;
  }
  if(list.Count==0)throw new Exception("No achievements found. The previous valid snapshot has been kept.");
  if(list.GroupBy(x=>x.Key).Any(g=>g.Count()>1))throw new Exception("Duplicate normalized achievement keys. Import stopped to protect research matches.");
  if(list.Any(x=>x.Requirements.Count==0))throw new Exception("An achievement has no requirements. The export may still be writing.");
  return list;
 }
 public static List<Row> Join(List<Achievement> all,ResearchFile research){
  var p=all.FirstOrDefault(x=>Normalize(x.Name)=="progressive" && x.Category.StartsWith("Slayer"));
  var required=new HashSet<string>(p==null?new string[0]:p.Requirements.Where(x=>!x.Optional).Select(x=>x.Reference));
  var optional=new HashSet<string>(p==null?new string[0]:p.Requirements.Where(x=>x.Optional).Select(x=>x.Reference));
  var rows=all.Select(a=>new Row{A=a,R=research.Items.FirstOrDefault(m=>m.Key==a.Key)??new Research{Key=a.Key,Name=a.Name,Category=a.Category},Required=required.Contains(Normalize(a.Name))&&a.Category.StartsWith("Slayer"),Optional=optional.Contains(Normalize(a.Name))&&a.Category.StartsWith("Slayer")}).ToList();
  foreach(var row in rows){if(row.R.Farms.Count==0){var options=Guides.SlayerAlternatives(row.A,all,research);if(options.Count>0){row.R=Json().Deserialize<Research>(Json().Serialize(row.R));row.R.Farms=options;}}}
  for(int pass=0;pass<3;pass++)foreach(var row in rows.Where(r=>r.Candidates.Count==0)){var options=Guides.ComponentDestinations(row,rows);if(options.Count>0){row.R=Json().Deserialize<Research>(Json().Serialize(row.R));row.R.Farms=options;}}
  foreach(var row in rows){var tier=row.Tier[0];var stack=rows.Count(o=>!o.A.Complete && o.A.Key!=row.A.Key && row.R.Farms.Any(f=>o.R.Farms.Any(z=>z.Zone==f.Zone)));row.Score=(row.Required && p!=null && !p.Complete?1000:0)+(tier=='A'?500:tier=='B'?400:tier=='C'?250:tier=='D'?100:0)+row.A.Progress*100+Math.Min(75,stack*15)-row.R.Difficulty*20-row.R.Travel*10-(row.R.Risk=="High"?150:row.R.Risk=="Medium"?60:row.R.Risk=="Unknown"?40:0)-(row.R.Era=="Unknown"?100:0)-(row.R.Bottleneck!=""?200:0);}
  return rows;
 }
 public static ResearchFile Seed(List<Achievement> all){var file=new ResearchFile();foreach(var a in all){var r=new Research{Key=a.Key,Name=a.Name,Category=a.Category};var n=Normalize(a.Name);
   if(a.Category=="Slayer: Skill" && a.Requirements.Any(x=>Regex.IsMatch(x.Text,@"^(Humans|Barbarians|Erudites|Wood Elves|High Elves|Dark Elves|Half Elves|Dwarves|Halflings|Gnomes|Trolls|Ogres|Iksars|Kerrans)(\t|$)"))){r.Risk="High";r.Notes="Playable-race kills may damage city and unlock factions. Verify individual NPC factions.";r.Sources="User-provided EQL faction caution";}
   string zone="",region="",mob="",via="";
   if(n=="bearwithme"){zone="North Karana";region="Karanas";mob="Grizzly bears";via="North Karana";}
   if(n=="yourenotscaringanyone"){zone="West Karana";region="Karanas";mob="Scarecrow fields";via="North Karana > West Karana";}
   if(n=="theelephantintheroom"){zone="South Karana";region="Karanas";mob="Elephants";via="North Karana > South Karana";}
   if(n=="lumberer"){zone="South Karana";region="Karanas";mob="Karana treants";via="North Karana > South Karana";r.Notes="Check faction consequences before farming treants.";}
   if(n=="eyeseewhatyoudidthere"){zone="Gorge of King Xorbb";region="Gorge of King Xorbb";mob="Evil Eyes";via="North Karana > East Karana > Gorge of King Xorbb";}
   if(n=="now49999"){zone="South Qeynos";region="Qeynos";mob="Half-Elf loop";via="North Karana > West Karana > Qeynos > North/South Qeynos";}
   if(zone!=""){r.Farms.Add(new Farm{Zone=zone,Region=region,Mobs=mob,Portal="Circle of North Karana",Via=via,Evidence="User planner screenshot; destination inferred from travel route, not independently verified"});r.Sources="User-provided planner screenshot";}
   if(n=="aclockworkgnome"){r.Farms.Add(new Farm{Zone="Sol A",Region="Lavastorm",Mobs="Gnome-race mobs",Evidence="User EQL observation"});r.Risk="High";r.Era="Observed EQL";r.Notes="Counted for A Clockwork Gnome but damaged Ak'Anon-related factions.";r.Sources="User EQL observation";}
   if(n=="terroribletentacles"){r.Farms.Add(new Farm{Zone="Plane of Fear",Region="Planes",Mobs="Tentacle Terrors",Evidence="User EQL observation"});r.Era="Observed EQL";r.Notes="Fear was useful. Najena did not have the large tentacle population expected from generic Classic data.";r.Sources="User EQL observation";}
   if(n=="stopdragonthisout"){r.Farms.Add(new Farm{Zone="Plane of Hate",Region="Planes",Mobs="Dragon Skeletons (investigation)",Evidence="Unverified target credit"});r.Bottleneck="Target credit and planar access";r.Notes="Test whether Dragon Skeleton kills advance this counter before farming.";r.Sources="User EQL investigation";}
   if(n=="youkeepdragonmeintothis"){r.Farms.Add(new Farm{Zone="Lesser Faydark",Region="Faydwer",Mobs="Fae Drakes",Evidence="User EQL observation"});r.Era="Observed EQL";r.Sources="User EQL observation";}
   if(a.Category.Contains("Classes"))r.Class=a.Name.Replace("Primary Class Unlock - ","");if(a.Category.Contains("Races"))r.Race=a.Name.Replace("Race Unlock - ","");
   if(r.Farms.Count>0||r.Notes!=""||r.Class!=""||r.Race!="")file.Items.Add(r);
  }return file;}
 public static JavaScriptSerializer Json(){return new JavaScriptSerializer{MaxJsonLength=16000000};}
 public static bool ApplyResearchUpdates(List<Achievement> all,ResearchFile file){
  const string id="2026-09-10-locations-and-camp-observations";if(file.AppliedUpdates==null)file.AppliedUpdates=new List<string>();bool corrected=false;const string correction="2026-09-15-elf-bandit-credit";
if(!file.AppliedUpdates.Contains(correction)){
 foreach(var r in file.Items.Where(r=>r.Key=="slayerskill|woodyoucouldyou"||r.Key=="slayerskill|highlyuncivilized")){
  r.Farms.RemoveAll(f=>(f.Mobs??"").IndexOf("bandit",StringComparison.OrdinalIgnoreCase)>=0);
  r.Notes+="\n2026-09-15: Previous bandit recommendation withdrawn after a user reported no counter increase in a fresh achievement export. Wood You Could You requires Wood Elves; Highly Uncivilized requires High Elves. A generic bandit name does not identify race. No replacement farming target is verified yet.";
 }file.AppliedUpdates.Add(correction);corrected=true;
}if(file.AppliedUpdates.Contains(id))return corrected;
  Action<string,Farm> add=(name,farm)=>{var a=all.FirstOrDefault(x=>Normalize(x.Name)==Normalize(name)&&x.Category.StartsWith("Slayer"));if(a==null)return;var r=file.Items.FirstOrDefault(x=>x.Key==a.Key);if(r==null){r=new Research{Key=a.Key,Name=a.Name,Category=a.Category};file.Items.Add(r);}if(!r.Farms.Any(f=>f.Zone==farm.Zone&&f.Mobs==farm.Mobs))r.Farms.Add(farm);};
  add("Get Stupid",new Farm{Zone="West Karana",Region="Karanas",Mobs="Ogres at Chief Goonda camp (map label)",Faction="No faction hit observed (user)",Evidence="User EQL observation, 2026-09-10: West Karana ogre camp labeled Chief Goonda on the map. No faction hit observed for the ogres fought; Chief Goonda himself is not confirmed."});
  foreach(string name in new[]{"Oh the Humanity!","Barbarous","Doesn't Play Well With Others","I'm a People Person!"})add(name,new Farm{Zone="West Karana",Region="Karanas",Mobs="Bandits — Human, High Elf and Barbarian variants (select the needed race)",Faction="No faction hit observed (user)",Evidence="User EQL observation, 2026-09-10: West Karana bandits include Humans, High Elves and Barbarians. Applies to the observed targets, not all bandits worldwide."});
  add("Barbarous",new Farm{Zone="West Karana",Region="Karanas",Mobs="Barbarian brigands",Faction="No faction hit observed (user)",Evidence="User EQL observation, 2026-09-10: Barbarian brigands alongside West Karana bandits."});
  add("Snake in the Grass",new Farm{Zone="East Karana",Region="Karanas",Mobs="Bullhorn Snake (wiki-listed candidate)",Evidence="https://eqlwiki.com/Eastern_Plains_of_Karana — reviewed 2026-09-10. EQL community listing; current spawn and Slayer credit still need in-game verification."});
  add("Amazing!",new Farm{Zone="Steamfont Mountains",Region="Faydwer",Mobs="Minotaurs near the caves (wiki-listed candidate)",Evidence="https://eqlwiki.com/Steamfont_Mountains — reviewed 2026-09-10. Community page contains legacy notes; verify current EQL presence and credit before farming."});
  add("Cubic",new Farm{Zone="Runnyeye",Region="Runnyeye",Mobs="A Gelatinous Cube (wiki-listed candidate)",Evidence="https://eqlwiki.com/Runnyeye — reviewed 2026-09-10. Community NPC listing; availability, spawn rate and Slayer credit unverified."});
  file.AppliedUpdates.Add(id);return true;
 }
 public static T Load<T>(string path){return Json().Deserialize<T>(File.ReadAllText(path));}
 public static void Save(string path,object value){Atomic(path,Json().Serialize(value));}
 public static void Atomic(string path,string content){Directory.CreateDirectory(Path.GetDirectoryName(path));string tmp=path+".tmp";File.WriteAllText(tmp,content,new UTF8Encoding(false));if(File.Exists(path)){var backup=Path.Combine(Path.GetDirectoryName(path),"backups");Directory.CreateDirectory(backup);File.Replace(tmp,path,Path.Combine(backup,Path.GetFileName(path)+"."+DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+".bak"));}else File.Move(tmp,path);}
}
}




