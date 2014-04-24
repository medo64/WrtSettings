#define AppName        GetStringFileInfo('..\Binaries\WrtSettings.exe', 'ProductName')
#define AppVersion     GetStringFileInfo('..\Binaries\WrtSettings.exe', 'ProductVersion')
#define AppFileVersion GetStringFileInfo('..\Binaries\WrtSettings.exe', 'FileVersion')
#define AppCompany     GetStringFileInfo('..\Binaries\WrtSettings.exe', 'CompanyName')
#define AppCopyright   GetStringFileInfo('..\Binaries\WrtSettings.exe', 'LegalCopyright')
#define AppBase        LowerCase(StringChange(AppName, ' ', ''))
#define AppSetupFile   AppBase + StringChange(AppVersion, '.', '')
#define AppVersionEx   StringChange(AppVersion, '0.00', '(latest)')

[Setup]
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppCompany}
AppPublisherURL=http://jmedved.com/{#AppBase}/
AppCopyright={#AppCopyright}
VersionInfoProductVersion={#AppVersion}
VersionInfoProductTextVersion={#AppVersion}
VersionInfoVersion={#AppFileVersion}
DefaultDirName={pf}\{#AppCompany}\{#AppName}
OutputBaseFilename={#AppSetupFile}
OutputDir=..\Releases
SourceDir=..\Binaries
AppId=JosipMedved_WrtSettings
CloseApplications="yes"
RestartApplications="no"
UninstallDisplayIcon={app}\WrtSettings.exe
AlwaysShowComponentsList=no
ArchitecturesInstallIn64BitMode=x64
DisableProgramGroupPage=yes
MergeDuplicateFiles=yes
MinVersion=0,6.01.7200
PrivilegesRequired=admin
ShowLanguageDialog=no
SolidCompression=yes
ChangesAssociations=yes
DisableWelcomePage=yes
LicenseFile=..\Setup\License.txt


[Messages]
SetupAppTitle=Setup {#AppName} {#AppVersionEx}
SetupWindowTitle=Setup {#AppName} {#AppVersionEx}
BeveledLabel=jmedved.com


[Files]
Source: "WrtSettings.exe";  DestDir: "{app}";  Flags: ignoreversion;


[Icons]
Name: "{userstartmenu}\WRT Settings";  Filename: "{app}\WrtSettings.exe"


[Registry]
Root: HKCU;  Subkey: "Software\Josip Medved";              ValueType: none;  Flags: uninsdeletekeyifempty;
Root: HKCU;  Subkey: "Software\Josip Medved\WrtSettings";  ValueType: none;  Flags: deletekey uninsdeletekey;


[Run]
Filename: "{app}\WrtSettings.exe";  Flags: postinstall nowait skipifsilent runasoriginaluser;  Description: "Launch application now";


[Code]
procedure InitializeWizard;
begin
  WizardForm.LicenseAcceptedRadio.Checked := True;
end;
