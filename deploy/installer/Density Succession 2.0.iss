; LANDIS-II Extension infomation
#define CoreRelease "LANDIS-II-V7"
#define ExtensionName "Density-Succession"
#define AppVersion "2.0"
#define AppPublisher "LANDIS-II Foundation"
#define AppURL "http://www.landis-ii.org/"

; Build directory
#define BuildDir "..\release"
#define DependDir "..\..\..\Library-Succession\Deploy\release"

; LANDIS-II installation directories
#define ExtDir "C:\Program Files\LANDIS-II-v7\extensions"
#define AppDir "C:\Program Files\LANDIS-II-v7"
#define LandisPlugInDir "C:\Program Files\LANDIS-II-v7\plug-ins-installer-files"
#define ExtensionsCmd AppDir + "\commands\landis-ii-extensions.cmd"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{C7B48755-64C6-42F9-8544-802FA84D1959}
AppName={#CoreRelease} {#ExtensionName}
AppVersion={#AppVersion}
; Name in "Programs and Features"
AppVerName={#CoreRelease} {#ExtensionName} v{#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={pf}\{#ExtensionName}
DisableDirPage=yes
DefaultGroupName={#ExtensionName}
DisableProgramGroupPage=yes
LicenseFile=LANDIS-II_Binary_license.rtf
OutputDir={#SourcePath}
OutputBaseFilename={#CoreRelease} {#ExtensionName} {#AppVersion}-setup
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"


[Files]
; This .dll IS the extension (ie, the extension's assembly)
; NB: Do not put an additional version number in the file name of this .dll
; (The name of this .dll is defined in the extension's \src\*.csproj file)
Source: {#BuildDir}\Landis.Extension.Succession.Density-v2.dll; DestDir: {#ExtDir}; Flags: replacesameversion

; Requisite auxiliary libraries
; NB. These libraries are used by other extensions and thus are never uninstalled.
Source: {#BuildDir}\Landis.Library.AgeOnlyCohorts-v3.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion
Source: {#BuildDir}\Landis.Library.Cohorts-v2.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion
Source: {#BuildDir}\Landis.Library.BiomassCohorts-v3.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion
Source: {#BuildDir}\Landis.Library.Metadata-v2.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion
Source: {#BuildDir}\Landis.Library.Parameters-v2.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion
Source: {#BuildDir}\Landis.Library.Biomass-v2.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion
Source: {#BuildDir}\Landis.Library.Climate-v4.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion
Source: {#BuildDir}\Landis.Library.Succession-v9.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion
Source: {#BuildDir}\Landis.Library.DensityCohorts-v1.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion
Source: {#DependDir}\Accord.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion
Source: {#DependDir}\Accord.Statistics.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion
Source: {#DependDir}\Accord.Math.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion
Source: {#DependDir}\MathNet.Numerics.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion

; Complete example for testing the extension
Source: ..\examples\density\*.txt; DestDir: {#AppDir}\examples\{#ExtensionName}; Flags: replacesameversion
Source: ..\examples\density\*.img; DestDir: {#AppDir}\examples\{#ExtensionName}; Flags: replacesameversion
Source: ..\examples\density\*.bat; DestDir: {#AppDir}\examples\{#ExtensionName}; Flags: replacesameversion


; LANDIS-II identifies the extension with the info in this .txt file
; NB. New releases must modify the name of this file and the info in it
#define InfoTxt "Density-Succession.txt"
Source: {#InfoTxt}; DestDir: {#LandisPlugInDir}

#define InfoTxt2 "Density-Output.txt"
Source: {#InfoTxt2}; DestDir: {#LandisPlugInDir}
; NOTE: Don't use "Flags: ignoreversion" on any shared system files


[Run]
Filename: {#ExtensionsCmd}; Parameters: "remove ""Density-Succession"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#ExtensionsCmd}; Parameters: "add ""{#InfoTxt}"" "; WorkingDir: {#LandisPlugInDir} 
Filename: {#ExtensionsCmd}; Parameters: "remove ""Density-Output"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#ExtensionsCmd}; Parameters: "add ""{#InfoTxt2}"" "; WorkingDir: {#LandisPlugInDir} 


[UninstallRun]
; Remove "Age-Only Succession" from "extensions.xml" file.
Filename: {#ExtensionsCmd}; Parameters: "remove ""Density-Succession"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#ExtensionsCmd}; Parameters: "remove ""Density-Output"" "; WorkingDir: {#LandisPlugInDir}


