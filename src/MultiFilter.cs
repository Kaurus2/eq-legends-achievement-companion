using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
namespace LegendsCompanion {
public class MultiFilter:UserControl {
 public List<string> Items=new List<string>(); public HashSet<string> Chosen=new HashSet<string>(); bool all=true; string label; StackPanel list=new StackPanel(); ToggleButton button=new ToggleButton(); public event EventHandler Changed;
 public MultiFilter(string title){label=title;MinWidth=150;Margin=new Thickness(0,2,12,5);var host=new Grid();Content=host;button.Padding=new Thickness(7);host.Children.Add(button);var pop=new Popup{PlacementTarget=button,Placement=PlacementMode.Bottom,StaysOpen=false,AllowsTransparency=true};host.Children.Add(pop);var content=new StackPanel();var actions=new StackPanel{Orientation=Orientation.Horizontal};foreach(string name in new[]{"Select all","Clear"}){var b=new Button{Content=name,Margin=new Thickness(3),Padding=new Thickness(8)};b.Click+=(s,e)=>{all=name=="Select all";Chosen.Clear();Render();Fire();};actions.Children.Add(b);}content.Children.Add(actions);content.Children.Add(new ScrollViewer{Content=list,MaxHeight=320,VerticalScrollBarVisibility=ScrollBarVisibility.Auto});pop.Child=new Border{Background=Palette.Surface,BorderBrush=Palette.Border,BorderThickness=new Thickness(1),Child=content,MinWidth=240};button.Click+=(s,e)=>pop.IsOpen=button.IsChecked==true;pop.Closed+=(s,e)=>button.IsChecked=false;Render();}
 public void SetChoices(params string[] values){all=false;Chosen=new HashSet<string>(values);Render();Fire();} public bool Matches(string value){return all||Chosen.Contains(value);} public bool IsAll{get{return all;}}
 public string SelectedItem{get{return all?label:Chosen.FirstOrDefault()??"";}set{all=value==label;Chosen.Clear();if(!all&&value!=null)Chosen.Add(value);Render();Fire();}}
 public int SelectedIndex{get{return all?0:-1;}set{SelectedItem=value==0?label:Items[value];}}
 public void Replace(IEnumerable<string> values){Items=values.Distinct().ToList();Render();}
 void Fire(){if(Changed!=null)Changed(this,EventArgs.Empty);}
 void Render(){list.Children.Clear();button.Content=all?label+" ▾":Chosen.Count==0?"None ▾":String.Join(", ",Chosen)+" ▾";button.MaxWidth=260;foreach(var value in Items.Where(x=>x!=label)){var box=new CheckBox{Content=value,IsChecked=all||Chosen.Contains(value),Margin=new Thickness(7)};box.Click+=(s,e)=>{if(all){all=false;Chosen=new HashSet<string>(Items.Where(x=>x!=label));}if(box.IsChecked==true)Chosen.Add(value);else Chosen.Remove(value);Render();Fire();};list.Children.Add(box);}}
}
}
