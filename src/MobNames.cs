using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace LegendsCompanion {
public partial class MainWindow {
 Dictionary<string,List<string>> mobNames;
 StackPanel mobNameRows;TextBox mobNameInput;FrameworkElement mobNameSection;
 List<MobMatch> SavedMobMatches(){return achievements.Where(a=>mobNames!=null&&mobNames.ContainsKey(a.Key)&&mobNames[a.Key]!=null).SelectMany(a=>mobNames[a.Key].Where(n=>!String.IsNullOrWhiteSpace(n)).Select(n=>new MobMatch{Mob=n,Achievement=a.Name})).ToList();}
 string MobNamesPath {get{return Path.Combine(store,"mob-names.json");}}
 void BuildMobNames(StackPanel parent){
  mobNames=File.Exists(MobNamesPath)?Data.Load<Dictionary<string,List<string>>>(MobNamesPath):new Dictionary<string,List<string>>();
  var section=new StackPanel{Margin=new Thickness(0,8,0,8)};mobNameSection=section;parent.Children.Add(section);
  section.Children.Add(Text("Mob names",15));mobNameRows=new StackPanel();section.Children.Add(new ScrollViewer{Content=mobNameRows,MaxHeight=300,VerticalScrollBarVisibility=ScrollBarVisibility.Auto});
  var entry=new Grid();entry.ColumnDefinitions.Add(new ColumnDefinition());entry.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
  mobNameInput=Input();mobNameInput.ToolTip="Enter one mob name per line. Ctrl+Enter adds all names.";mobNameInput.AcceptsReturn=true;mobNameInput.TextWrapping=TextWrapping.Wrap;mobNameInput.MinHeight=72;mobNameInput.MaxHeight=180;mobNameInput.VerticalScrollBarVisibility=ScrollBarVisibility.Auto;entry.Children.Add(mobNameInput);
  var add=Button("+",AddMobName);add.ToolTip="Add mob name";Grid.SetColumn(add,1);entry.Children.Add(add);section.Children.Add(entry);section.Children.Add(Text("One name per line. Click + or Ctrl+Enter to save and track future kills.",11));
  mobNameInput.KeyDown+=(s,e)=>{if(e.Key==Key.Enter&&(Keyboard.Modifiers&ModifierKeys.Control)!=0){AddMobName();e.Handled=true;}};RefreshMobNames();
 }
 void RefreshMobNames(){if(mobNameRows==null)return;mobNameRows.Children.Clear();mobNameInput.Text="";mobNameSection.IsEnabled=editing!=null;if(editing==null)return;
  List<string> names;if(!mobNames.TryGetValue(editing.A.Key,out names)||names==null)return;
  foreach(var name in names){string captured=name;var row=new Grid();row.ColumnDefinitions.Add(new ColumnDefinition());row.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
   row.Children.Add(new TextBlock{Text=name,TextWrapping=TextWrapping.Wrap,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(5)});
   var remove=Button("×",()=>ChangeMobName(captured,false));remove.ToolTip="Remove mob name";Grid.SetColumn(remove,1);row.Children.Add(remove);mobNameRows.Children.Add(row);
  }
 }
 void AddMobName(){string name=mobNameInput.Text.Trim();if(name.Length==0||editing==null)return;ChangeMobName(name,true);}
 void ChangeMobName(string name,bool add){
  if(editing==null)return;
  var next=Data.Json().Deserialize<Dictionary<string,List<string>>>(Data.Json().Serialize(mobNames));List<string> names;
  if(!next.TryGetValue(editing.A.Key,out names)||names==null){names=new List<string>();next[editing.A.Key]=names;}
  if(add){if(names.Any(n=>String.Equals(n,name,StringComparison.OrdinalIgnoreCase))){status.Text="That mob name is already listed.";return;}names.AddRange(name.Split(new[]{'\r','\n'},StringSplitOptions.RemoveEmptyEntries).Select(n=>n.Trim()).Where(n=>n.Length>0&&!names.Any(old=>String.Equals(old,n,StringComparison.OrdinalIgnoreCase))).Distinct(StringComparer.OrdinalIgnoreCase));}else names.RemoveAll(n=>String.Equals(n,name,StringComparison.OrdinalIgnoreCase));
  try{Data.Save(MobNamesPath,next);mobNames=next;RefreshMobNames();mobNameInput.Focus();status.Text=add?"Mob name added and saved.":"Mob name removed and saved.";}catch(Exception error){MessageBox.Show(this,error.Message,"Could not save mob names");}
 }
}
}
