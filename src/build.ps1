param([string]$OutputPath)
$ErrorActionPreference = 'Stop'
$eqlAppRoot = Split-Path -Parent $PSScriptRoot
if (-not $OutputPath) { $OutputPath = Join-Path $eqlAppRoot 'EQ Legends Achievement Companion.exe' }
$eqlFramework = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319'
$eqlArgs = @('/nologo','/target:winexe','/optimize+',('/out:' +  $OutputPath),('/reference:' + $eqlFramework + '\WPF\PresentationFramework.dll'),('/reference:' + $eqlFramework + '\WPF\PresentationCore.dll'),('/reference:' + $eqlFramework + '\WPF\WindowsBase.dll'),('/reference:' + $eqlFramework + '\System.Xaml.dll'),('/reference:' + $eqlFramework + '\System.Web.Extensions.dll'),(Join-Path $PSScriptRoot 'Data.cs'),(Join-Path $PSScriptRoot 'App.cs'),(Join-Path $PSScriptRoot 'Tests.cs'))
$eqlArgs += '/win32icon:' + (Join-Path $eqlAppRoot 'assets\achievement-crest.ico')
$eqlArgs += (Join-Path $PSScriptRoot 'Guides.cs')
$eqlArgs += '/resource:' + (Join-Path $eqlAppRoot 'assets\achievement-crest.ico') + ',AchievementIcon'
$eqlArgs += (Join-Path $PSScriptRoot 'MultiFilter.cs')
$eqlArgs += (Join-Path $PSScriptRoot 'TravelGuide.cs')
$eqlArgs += (Join-Path $PSScriptRoot 'LiveLog.cs')
$eqlArgs += (Join-Path $PSScriptRoot 'Tracker.cs')
$eqlArgs += (Join-Path $PSScriptRoot 'Opacity.cs')
$eqlArgs += (Join-Path $PSScriptRoot 'CompactToast.cs')
$eqlArgs += (Join-Path $PSScriptRoot 'WindowFrame.cs')
$eqlArgs += (Join-Path $PSScriptRoot "Profiles.cs")
$eqlArgs += (Join-Path $PSScriptRoot "Theme.cs")
$eqlArgs += (Join-Path $PSScriptRoot "Updater.cs")
$eqlArgs += (Join-Path $PSScriptRoot "SavedFilters.cs")
$eqlArgs += "/reference:System.Windows.Forms.dll"
$eqlArgs += "/resource:" + (Join-Path $eqlAppRoot "assets\clockwork-gear-hd.png") + ",ClockworkGear"
& (Join-Path $eqlFramework 'csc.exe') $eqlArgs
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed.' }







