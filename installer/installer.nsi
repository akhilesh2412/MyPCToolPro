; MyPC Tool Pro — Professional Installer (NSIS)
Unicode True
SetCompressor /SOLID lzma
SetCompressorDictSize 64

!include "MUI2.nsh"

Name "MyPC Tool Pro"
OutFile "MyPCToolPro-Setup.exe"
InstallDir "$PROGRAMFILES64\MyPC Tool Pro"
InstallDirRegKey HKLM "Software\MyPC Tool Pro" "InstallDir"
RequestExecutionLevel admin

VIProductVersion "2.0.0.0"
VIAddVersionKey "ProductName" "MyPC Tool Pro"
VIAddVersionKey "FileDescription" "Professional PC Optimizer - Cleaner, Process Killer and RAM Booster"
VIAddVersionKey "FileVersion" "2.0.0.0"
VIAddVersionKey "ProductVersion" "2.0.0"

!define MUI_ICON "app.ico"
!define MUI_UNICON "app.ico"
!define MUI_ABORTWARNING
!define MUI_FINISHPAGE_RUN "$INSTDIR\MyPCToolPro.exe"
!define MUI_FINISHPAGE_RUN_TEXT "Launch MyPC Tool Pro now"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "English"

Section "MyPC Tool Pro (required)"
  SectionIn RO
  SetOutPath "$INSTDIR"
  File "MyPCToolPro.exe"

  ; Desktop + Start Menu shortcuts
  CreateShortCut "$DESKTOP\MyPC Tool Pro.lnk" "$INSTDIR\MyPCToolPro.exe" "" "$INSTDIR\MyPCToolPro.exe" 0
  CreateDirectory "$SMPROGRAMS\MyPC Tool Pro"
  CreateShortCut "$SMPROGRAMS\MyPC Tool Pro\MyPC Tool Pro.lnk" "$INSTDIR\MyPCToolPro.exe" "" "$INSTDIR\MyPCToolPro.exe" 0
  CreateShortCut "$SMPROGRAMS\MyPC Tool Pro\Uninstall.lnk" "$INSTDIR\Uninstall.exe"

  WriteRegStr HKLM "Software\MyPC Tool Pro" "InstallDir" "$INSTDIR"
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MyPCToolPro" "DisplayName" "MyPC Tool Pro"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MyPCToolPro" "DisplayVersion" "2.0.0"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MyPCToolPro" "Publisher" "MyPC Tool"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MyPCToolPro" "UninstallString" '"$INSTDIR\Uninstall.exe"'
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MyPCToolPro" "DisplayIcon" "$INSTDIR\MyPCToolPro.exe"
SectionEnd

Section "Uninstall"
  Delete "$INSTDIR\MyPCToolPro.exe"
  Delete "$INSTDIR\Uninstall.exe"
  Delete "$DESKTOP\MyPC Tool Pro.lnk"
  Delete "$SMPROGRAMS\MyPC Tool Pro\MyPC Tool Pro.lnk"
  Delete "$SMPROGRAMS\MyPC Tool Pro\Uninstall.lnk"
  RMDir "$SMPROGRAMS\MyPC Tool Pro"
  RMDir "$INSTDIR"
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MyPCToolPro"
  DeleteRegKey HKLM "Software\MyPC Tool Pro"
SectionEnd
