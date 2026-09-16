using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace LegendsCompanion {
public class GuideEntry {
 public string Key {get;set;} public string Text {get;set;} public string Sources {get;set;} public string Gaps {get;set;}
 public List<Farm> Destinations {get;set;}
 public GuideEntry(){Text=Sources=Gaps="";Destinations=new List<Farm>();}
}
public static class Guides {
 public static List<Achievement> TargetGroups=new List<Achievement>(); public static List<GuideEntry> Catalog=new List<GuideEntry>();
 public static void Load(string home){var targetPath=Path.Combine(home,"data","target-groups.json");TargetGroups=File.Exists(targetPath)?Data.Load<List<Achievement>>(targetPath):new List<Achievement>();string p=Path.Combine(home,"data","guides.json");Catalog=File.Exists(p)?Data.Load<List<GuideEntry>>(p):new List<GuideEntry>();}
 public static string Zone(string value){string s=Regex.Replace(value??"",@"^(The |the )","").Trim();switch(s){
 case "Western Plains of Karana":case "Western Karana":return "West Karana";
 case "Northern Plains of Karana":case "Northern Karana":return "North Karana";
 case "Southern Plains of Karana":case "Southern Karana":return "South Karana";
 case "Eastern Plains of Karana":case "Eastern Karana":return "East Karana";
 case "Solusek's Eye":case "Sol A":return "Solusek's Eye";
 case "Castle of Mistmoore":case "Mistmoore Castle":return "Mistmoore Castle";
 case "Liberated Citadel of Runnyeye":return "Runnyeye";
 case "Ruins of Old Paineel":case "Ruins of Old Paineel or The Hole":case "Hole":return "The Hole";
 case "Northern Desert of Ro":return "North Ro";case "Southern Desert of Ro":return "South Ro";
 case "Temple of Cazic-Thule":return "Cazic-Thule";case "Feerrott":return "The Feerrott";
 default:return s;}}
 public static string Region(string zone){return zone.EndsWith("Karana")?"Karanas":zone.StartsWith("Plane of")?"Planes":"";}
 public static bool NoFixedLocation(Achievement a){return a.Category=="General: Level"||a.Category=="General: Skills"||a.Category.StartsWith("Tradeskill:")||a.Category=="Slayer: General"||a.Category=="EverQuest: General"||a.Requirements.Any(x=>x.Text.Contains("Future Placeholder")||x.Text.Contains("autocomplete when you unlock Human or Wood Elf"));}
 public static List<Farm> Destinations(Achievement a){var e=Catalog.FirstOrDefault(x=>x.Key==a.Key);if(e!=null&&e.Destinations.Count>0)return e.Destinations;
  string zone="";if(a.Category=="EverQuest: Exploration"&&a.Requirements.Count==1&&a.Requirements[0].Text.StartsWith("Visit "))zone=a.Requirements[0].Text.Substring(6);
  if((a.Category=="EverQuest: Hunter"||a.Category=="EverQuest: Raids")&&!a.Requirements.Any(x=>Regex.IsMatch(x.Text,@"^(\(Optional\) )?(Hunter|Conqueror) of ")))zone=Regex.Replace(a.Name,@"^(Hunter|Conqueror) of ","");
  if(zone=="")return new List<Farm>();zone=Zone(zone);return new List<Farm>{new Farm{Zone=zone,Region=Region(zone),Mobs=a.Category.Contains("Exploration")?"Visit destination":"Named checklist in TXT",Evidence="Destination specified by achievement export; exact access / spawns not verified",Faction=a.Category.Contains("Exploration")?"No kill required; entry faction may matter":"Unknown"}};
 }
 public static List<Farm> ComponentDestinations(Row parent,List<Row> rows){
  var result=new List<Farm>();var a=parent.A;if(a.Category!="Untapped Potential: Races"&&a.Category!="EverQuest: Exploration"&&a.Category!="EverQuest: Hunter"&&a.Category!="EverQuest: Raids")return result;
  foreach(var q in a.Requirements.Where(x=>!x.Optional&&!x.Complete)){
   string name=q.Reference;if(String.IsNullOrEmpty(name))name=Data.Normalize(Regex.Replace(q.Text,@"^Get maximum faction with ","").TrimEnd('.'));
   if(name=="newsebilisianexpedition")name="newsebilisexpedition";
   if(name=="freeportmilitia")name="thefreeportmilitia";
   if(name=="qeynosguards")name="guardsofqeynos";
   if(name==""||q.Text.StartsWith("This achievement"))continue;
   var child=rows.FirstOrDefault(r=>r.A.Key!=a.Key&&Data.Normalize(r.A.Name)==name);
   var farms=child!=null?child.Preferred:new List<Farm>();
   if(farms.Count==0&&q.Text=="Hunter of The Plane of Hate")farms.Add(new Farm{Zone="Plane of Hate",Region="Planes",Mobs="Complete Hunter of The Plane of Hate",Evidence="Child achievement named in export; individual target checklist is absent from this export",Faction="Encounter-specific faction effects need verification"});
   if(farms.Count==0&&q.Text.StartsWith("Get maximum faction with ")){var guide=Catalog.FirstOrDefault(g=>g.Key=="everquestprogression|"+name);if(guide!=null)farms=guide.Destinations.Take(1).ToList();}
   foreach(var f in farms){if(result.Any(x=>x.Zone==f.Zone&&x.Mobs==f.Mobs))continue;var copy=Data.Json().Deserialize<Farm>(Data.Json().Serialize(f));copy.Kind="Required stop";copy.Selected=false;copy.Evidence="Component: "+q.Text+". "+copy.Evidence;result.Add(copy);}
  }return result;
 }
 public static List<Farm> SlayerAlternatives(Achievement a,List<Achievement> all,ResearchFile file){
  var result=new List<Farm>();if((a.Category!="Slayer: Conquest"&&a.Category!="Slayer: Special")||a.Requirements.Count!=1)return result;
  Func<string,List<string>> tokens=t=>Regex.Split(t.Split('\t')[0].ToLowerInvariant(),@",|\band\b|\bor\b").Select(x=>Data.Normalize(x)).Where(x=>x!="").ToList();var wanted=tokens(a.Requirements[0].Text);
  foreach(var skill in all.Concat(TargetGroups).GroupBy(x=>x.Key).Select(g=>g.First()).Where(x=>x.Category=="Slayer: Skill"&&x.Requirements.Count==1)){var types=tokens(skill.Requirements[0].Text);if(types.Count==0||!types.All(t=>wanted.Contains(t)))continue;var source=file.Items.FirstOrDefault(r=>r.Key==skill.Key);if(source==null)continue;foreach(var f in source.Farms){if(result.Any(p=>p.Zone==f.Zone&&p.Mobs==f.Mobs))continue;var copy=Data.Json().Deserialize<Farm>(Data.Json().Serialize(f));copy.Selected=false;copy.Kind="Alternative";copy.Evidence="Target-group match to "+skill.Name+" in the TXT; confirm this objective's credit. "+copy.Evidence;result.Add(copy);}}
  return result;
 }
 public static string Gaps(Row r){if(r.R.Cleaned)return r.R.Verification??"";var e=Catalog.FirstOrDefault(x=>x.Key==r.A.Key);if(e!=null&&!String.IsNullOrWhiteSpace(e.Gaps))return e.Gaps;
  if(r.A.Requirements.Any(x=>x.Text.Contains("Future Placeholder")))return "Quest requirements are a future placeholder in TXT";
  if(r.A.Requirements.Any(x=>x.Text.Contains("autocomplete when you unlock Human or Wood Elf")))return "Complete either Human or Wood Elf unlock; no separate location required";
   if(r.A.Category.StartsWith("Tradeskill:"))return "Current recipe path, suppliers and skill-cap availability";
  if(r.A.Category=="General: Level"||r.A.Category=="General: Skills")return "No fixed mob location required";
  if(r.A.Category=="Slayer: General"||r.A.Category=="EverQuest: General")return "See component achievements";
  if(r.A.Category=="EverQuest: Exploration")return r.A.Name.EndsWith("Traveler")?"Current access / entry route":"See individual traveler achievements";
  if(r.A.Category=="EverQuest: Hunter"||r.A.Category=="EverQuest: Raids")return "Exact named spawns, placeholders, encounter and access details";
  return r.Locations.Count==0?"Completion method / locations needed":"Current spawn, target credit and faction verification";
 }
 public static string Text(Row r,List<Achievement> all){var a=r.A;var e=Catalog.FirstOrDefault(x=>x.Key==a.Key);string t=e==null?"":e.Text;
  if(t==""){
    if(a.Requirements.Any(x=>x.Text.Contains("autocomplete when you unlock Human or Wood Elf")))t="Unlock Human OR Wood Elf to complete Half Elf automatically, as stated in your achievement export. Follow either race unlock's faction methods; Half Elf has no separate farming destination. The export also lists character-creation and token alternatives.";
    else if(a.Category=="General: Level")t="Reach the level shown in the TXT. This tracks character level; no particular mob, portal or faction is required.";
   else if(a.Category=="General: Skills")t="Train or practice each listed skill until its required cap is reached. The TXT checklist identifies which skills remain. Requirements can differ by class; use your own export.";
   else if(a.Category.StartsWith("Tradeskill:"))t="Raise the named tradeskill to the listed threshold. Plan supplies and combines for your current skill, and check recipe availability in the current Legends era before buying a large batch. A milestone appearing in the TXT does not prove its cap is currently attainable.";
   else if(a.Category=="EverQuest: Exploration")t="Visit the listed destination, then re-export to check credit. For an Explorer parent, finish the required Traveler achievements individually. Housing and special-instance entries need their specific interior; a similarly named outdoor zone is not a substitute.";
   else if(a.Category=="EverQuest: Hunter")t="Defeat each named target in the TXT checklist. A generic mob of the same race is not a substitute. Zone names below come from the achievement, not a verified spawn map. Regional Hunter achievements require their child achievements.";
   else if(a.Category=="EverQuest: Raids")t="Arrange the named encounters shown in the TXT with your guild. Confirm current instance access and lockouts before scheduling. The listed destination identifies the encounter zone; boss tactics and exact spawn conditions still need review.";
   else if(a.Category=="Untapped Potential: Races")t=a.Name.EndsWith("Kerran")?"Complete Aid the Kerrans of Kerra Isle, the task named in your export. This is a task unlock, not the standard maximum-faction checklist. The TXT also lists character-creation and token alternatives. Task steps and access still need verification.":"Complete each faction achievement listed in your TXT. The Legends community guide describes +2000 personal standing for credit; a friendly con alone is insufficient. These faction achievements do not all need to be earned simultaneously. Check losses before repeating a quest so you can plan the order. Find each faction in Progression for its researched methods. Creation and token alternatives are listed in your export.\n\nReference: https://eqlwiki.com/Alanna%27s_Race_Unlock_Guide (reviewed 2026-09-11)";
   else if(a.Category=="Untapped Potential: Deity")t=a.Requirements.Any(x=>x.Text.Contains("Future Placeholder"))?"The TXT explicitly marks this deity's quest requirements as a future placeholder. It lists confirmation/token alternatives; no invented quest route is supplied.":"Complete the deity task named in the TXT and check its listed alternatives.";
   else if(a.Category.StartsWith("Slayer:")&&a.Category!="Slayer: General")t="Use the exact target groups in the TXT. For a multi-line checklist, each remaining group needs its own credit; a mixed bandit camp cannot cover every race. Listed camps are candidates unless observed in Legends. Check one kill's counter and faction messages before committing to a long farm.";
   else t="Complete the requirements in the TXT. Follow the component achievements below when this is a parent checklist; optional entries remain optional.";
  }
  var related=new List<string>();foreach(var q in a.Requirements){string n=q.Reference;if(String.IsNullOrEmpty(n))n=Data.Normalize(Regex.Replace(q.Text,@"^\(Optional\)\s*", "").Split('\t')[0]);var child=all.FirstOrDefault(x=>x.Key!=a.Key&&Data.Normalize(x.Name)==n);if(child!=null)related.Add((q.Optional?"Optional: ":"")+child.Name+" — "+(child.Complete?"complete":"incomplete"));}
  return t+(related.Count>0?"\n\nComponent achievements\n"+String.Join("\n",related):"")+"\n\nStill to verify: "+Gaps(r)+(e!=null&&e.Sources!=""?"\n\nGuide sources\n"+e.Sources:"");
 }
}
}

