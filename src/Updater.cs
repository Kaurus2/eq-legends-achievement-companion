using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
namespace LegendsCompanion {
public class ReleaseAsset {public string name {get;set;} public string browser_download_url {get;set;} public string digest {get;set;} public long size {get;set;}}
public class ReleaseInfo {public string tag_name {get;set;} public bool draft {get;set;} public bool prerelease {get;set;} public List<ReleaseAsset> assets {get;set;}}
public static class CompanionUpdater {
 public const string Version="2026.09.14.1";
 const string Repo="https://api.github.com/repos/Kaurus2/eq-legends-achievement-companion/releases/latest";
 public const string AssetName="EQ-Legends-Achievement-Companion.exe";
 static WebClient Client(){ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;var c=new WebClient();c.Headers["User-Agent"]="EQ-Legends-Achievement-Companion/"+Version;return c;}
 public static ReleaseInfo Latest(){using(var c=Client())return Data.Json().Deserialize<ReleaseInfo>(c.DownloadString(Repo));}
 public static bool Newer(string tag){System.Version a,b;return System.Version.TryParse((tag??"").TrimStart('v'),out a)&&System.Version.TryParse(Version,out b)&&a>b;}
 public static string Digest(string path){using(var s=File.OpenRead(path))using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(s)).Replace("-","").ToLowerInvariant();}
 public static ReleaseAsset Asset(ReleaseInfo release){if(release==null||release.draft||release.prerelease||release.assets==null)return null;return release.assets.FirstOrDefault(a=>a.name==AssetName);}
 public static void ValidateAsset(ReleaseAsset asset){
  if(asset==null||asset.size<1024||asset.size>50*1024*1024)throw new Exception("This release has no supported updater download. Download its ZIP from GitHub.");
  Uri uri;if(!Uri.TryCreate(asset.browser_download_url,UriKind.Absolute,out uri)||uri.Scheme!="https"||uri.Host!="github.com"||!uri.AbsolutePath.StartsWith("/Kaurus2/eq-legends-achievement-companion/releases/download/",StringComparison.Ordinal))throw new Exception("Unexpected update download address.");
  if(asset.digest==null||!System.Text.RegularExpressions.Regex.IsMatch(asset.digest,@"^sha256:[a-fA-F0-9]{64}$"))throw new Exception("The release has no SHA-256 verification information yet. Try again later.");
 }
 public static string Download(ReleaseAsset asset){ValidateAsset(asset);string dir=Path.Combine(Path.GetTempPath(),"EQCompanionUpdate-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);string path=Path.Combine(dir,"download.exe");using(var c=Client())c.DownloadFile(asset.browser_download_url,path);if(new FileInfo(path).Length!=asset.size||Digest(path)!=asset.digest.Substring(7).ToLowerInvariant())throw new Exception("Update verification failed. Your current installation has not changed.");return path;}
 public static void ReplaceVerified(string target,string downloaded,string expected){
  if(Digest(downloaded)!=expected)throw new Exception("Downloaded update failed verification.");
  string stage=target+".update",backup=target+".previous";File.Copy(downloaded,stage,true);
  if(Digest(stage)!=expected){File.Delete(stage);throw new Exception("Staged update failed verification.");}
  if(File.Exists(backup))File.Delete(backup);File.Replace(stage,target,backup);
 }
 public static int Apply(string[] args){
  string target=args.Length==5?args[2]:"";bool parentExited=false;try{
   if(args.Length!=5||!Path.IsPathRooted(target)||!target.EndsWith(".exe",StringComparison.OrdinalIgnoreCase))throw new Exception("Invalid update arguments.");
   int id=Int32.Parse(args[1]);try{using(var parent=Process.GetProcessById(id)){if(!parent.WaitForExit(90000))throw new Exception("The Companion did not close. Update cancelled.");}}catch(ArgumentException){}
   parentExited=true;ReplaceVerified(target,args[3],args[4]);
   try{Process.Start(new ProcessStartInfo(target){WorkingDirectory=Path.GetDirectoryName(target),UseShellExecute=true});}
   catch{File.Copy(target+".previous",target,true);throw;}
   return 0;
  }catch(Exception e){File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"update-error.txt"),e.ToString());MessageBox.Show("Could not finish the update: "+e.Message+"\nYour data is unchanged. A previous executable is kept beside the app after replacement.","Companion update");if(parentExited&&File.Exists(target)){try{Process.Start(new ProcessStartInfo(target){UseShellExecute=true,WorkingDirectory=Path.GetDirectoryName(target)});}catch{}}return 1;}
 }
}
public partial class MainWindow {
 bool checkingUpdate,installingUpdate;ReleaseInfo availableRelease;Button updateButton;
 void BuildUpdater(){
  updateButton=new Button{Content="Update available",Visibility=Visibility.Collapsed,Padding=new Thickness(7,4,7,4),Margin=new Thickness(0,0,4,4)};trackerToolbar.Children.Add(updateButton);updateButton.Click+=async(s,e)=>await InstallUpdate();
  configContent.Children.Add(Button("Check for updates",()=>{var check=CheckUpdates(true);}));
 }
 async Task CheckUpdates(bool manual){if(checkingUpdate||installingUpdate)return;checkingUpdate=true;if(manual)status.Text="Checking GitHub for updates…";try{
  var release=await Task.Run(()=>CompanionUpdater.Latest());if(CompanionUpdater.Newer(release.tag_name)&&!release.draft&&!release.prerelease){availableRelease=release;updateButton.Content="Update & restart — "+release.tag_name;updateButton.Visibility=Visibility.Visible;status.Text="Update available. Click the update button when you are ready.";}
  else if(manual)status.Text="You have the latest version ("+CompanionUpdater.Version+").";
 }catch(Exception e){if(manual)status.Text="Could not check for updates: "+e.Message;}finally{checkingUpdate=false;}}
 async Task InstallUpdate(){if(installingUpdate||availableRelease==null)return;
  if(MessageBox.Show(this,"Download "+availableRelease.tag_name+" and restart the Companion? Your data stays in place.","Update & restart",MessageBoxButton.YesNo)!=MessageBoxResult.Yes)return;
  installingUpdate=true;updateButton.IsEnabled=false;try{
   status.Text="Downloading and verifying update…";var asset=CompanionUpdater.Asset(availableRelease);string download=await Task.Run(()=>CompanionUpdater.Download(asset));
   if(!ResolveEdits())return;
   string helper=Path.Combine(Path.GetDirectoryName(download),"updater.exe");string target=Process.GetCurrentProcess().MainModule.FileName;File.Copy(target,helper,true);
   string arguments="--apply-update "+Process.GetCurrentProcess().Id+" \""+target+"\" \""+download+"\" "+asset.digest.Substring(7).ToLowerInvariant();
   Process.Start(new ProcessStartInfo(helper,arguments){UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=Path.GetDirectoryName(helper)});Close();
  }catch(Exception e){status.Text="Update failed: "+e.Message;}finally{installingUpdate=false;updateButton.IsEnabled=true;}
 }
}
}
