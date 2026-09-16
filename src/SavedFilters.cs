using System;
using System.Linq;
using System.Windows.Controls;
namespace LegendsCompanion {
public class SavedFilters {
 public string Focus {get;set;} public string State {get;set;} public string Progress {get;set;}
 public string Availability {get;set;} public string Risk {get;set;} public string Research {get;set;}
 public string Search {get;set;} public string Class {get;set;} public string Race {get;set;}
 // Null means All, while an empty array means explicitly None.
 public string[] Categories {get;set;} public string[] Regions {get;set;} public string[] Zones {get;set;}
 public bool ZoneOptimized {get;set;} public bool Detours {get;set;} public bool TableView {get;set;}
}
public partial class MainWindow {
 void RememberFilters(){
  settings.SavedFilters=new SavedFilters{Focus=Pick(group),State=Pick(state),Progress=Pick(near),Availability=Pick(era),Risk=Pick(risk),Research=Pick(metaMatch),Search=search.Text,Class=classFilter.Text,Race=raceFilter.Text,
   Categories=category.IsAll?null:category.Chosen.ToArray(),Regions=region.IsAll?null:region.Chosen.ToArray(),Zones=zone.IsAll?null:zone.Chosen.ToArray(),ZoneOptimized=matchingOptions.IsChecked==true,Detours=detours.IsChecked==true,TableView=tableMode};
 }
 static void RestoreChoice(ComboBox control,string choice){if(choice!=null&&control.Items.Contains(choice))control.SelectedItem=choice;}
 static void RestoreMultiple(MultiFilter control,string[] choices){if(choices==null)control.SelectedIndex=0;else control.SetChoices(choices);}
 void RestoreFilters(){
  var saved=settings.SavedFilters;if(saved==null)return;bool wasReady=ready;ready=false;
  try{
   RestoreChoice(group,saved.Focus);RestoreChoice(state,saved.State);RestoreChoice(near,saved.Progress);RestoreChoice(era,saved.Availability);RestoreChoice(risk,saved.Risk);RestoreChoice(metaMatch,saved.Research);
   RestoreMultiple(category,saved.Categories);RestoreMultiple(region,saved.Regions);RefreshZones();RestoreMultiple(zone,saved.Zones);
   search.Text=saved.Search??"";classFilter.Text=saved.Class??"";raceFilter.Text=saved.Race??"";
   matchingOptions.IsChecked=saved.ZoneOptimized;detours.IsChecked=saved.Detours;tableMode=saved.TableView;
  }finally{ready=wasReady;}
 }
}
}
