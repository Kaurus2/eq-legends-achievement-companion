using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
namespace LegendsCompanion {
public class CleanEntry {
 public string Key,Name,Category,Zone,Mobs,RequiredFaction,Route,FactionHit,Tier,Verification,Availability;
}
public static class CleanResearch {
 public const string Id="2026-09-16-cleaned-research";
 public static List<CleanEntry> Entries(){using(var stream=typeof(CleanResearch).Assembly.GetManifestResourceStream("CleanResearch")){if(stream==null)throw new Exception("Clean research resource missing");using(var reader=new StreamReader(stream))return Data.Json().Deserialize<List<CleanEntry>>(reader.ReadToEnd());}}
 public static bool Apply(ResearchFile file){
  if(file.AppliedUpdates==null)file.AppliedUpdates=new List<string>();
  if(file.AppliedUpdates.Contains(Id))return false;
  var entries=Entries();if(entries.Count!=488||entries.Select(x=>x.Key).Distinct().Count()!=488)throw new Exception("Invalid clean research catalog");
  foreach(var e in entries){var r=file.Items.FirstOrDefault(x=>x.Key==e.Key);if(r==null){r=new Research{Key=e.Key,Name=e.Name,Category=e.Category};file.Items.Add(r);}
   r.ResearchHistory=Data.Json().Serialize(new {r.Farms,r.Notes,r.Sources,r.ResearchHistory});
   r.Cleaned=true;r.RequiredFaction=e.RequiredFaction;r.FactionHit=e.FactionHit;r.Verification=e.Verification;r.Tier=e.Tier;r.Era=e.Availability;r.Notes="";
   r.Farms=(e.Zone??"").Split(new[]{';'},StringSplitOptions.RemoveEmptyEntries).Select(z=>new Farm{Zone=z.Trim(),Region=Guides.Region(Guides.Zone(z.Split(new[]{" — "},StringSplitOptions.None)[0].Trim())),Mobs=e.Mobs,PortRoute=e.Route,Faction=e.FactionHit,Kind="Required stop"}).ToList();
  }
  file.AppliedUpdates.Add(Id);return true;
 }
 public static string Guide(Row r){var lines=new List<string>{r.Location,r.Mobs};if(!String.IsNullOrWhiteSpace(r.R.RequiredFaction))lines.Add("Required faction: "+r.R.RequiredFaction);if(!String.IsNullOrWhiteSpace(r.R.FactionHit))lines.Add("Faction hit: "+r.R.FactionHit);if(!String.IsNullOrWhiteSpace(r.Era))lines.Add("Availability: "+r.Era);if(!String.IsNullOrWhiteSpace(r.PortsAndRoute))lines.Add(r.PortsAndRoute);if(!String.IsNullOrWhiteSpace(r.ResearchGaps))lines.Add("Needs verification: "+r.ResearchGaps);return String.Join("\n\n",lines);}
}
}
