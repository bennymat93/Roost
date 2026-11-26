; Roost installer (NSIS / MUI2). Per-user install, no admin required.
; Build (Linux or Windows):  makensis -DAPPVER=1.3.0 installer/Roost.nsi
; Output: installer/Output/Roost-Setup.exe

Unicode true
!include "MUI2.nsh"
!include "FileFunc.nsh"
!include "nsDialogs.nsh"
!include "LogicLib.nsh"

!ifndef APPVER
  !define APPVER "1.3.1"
!endif
!define APPNAME "Roost"
!define PUBLISHER "Roost"
!define UNINSTKEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"

Name "${APPNAME} ${APPVER}"
OutFile "Output\Roost-Setup.exe"
RequestExecutionLevel user
InstallDir "$LOCALAPPDATA\Programs\${APPNAME}"
InstallDirRegKey HKCU "Software\${APPNAME}" "InstallDir"
SetCompressor /SOLID lzma
ShowInstDetails show
ShowUninstDetails show
VIProductVersion "${APPVER}.0"
VIAddVersionKey "ProductName" "${APPNAME}"
VIAddVersionKey "FileVersion" "${APPVER}.0"
VIAddVersionKey "ProductVersion" "${APPVER}"
VIAddVersionKey "CompanyName" "${PUBLISHER}"
VIAddVersionKey "FileDescription" "${APPNAME} installer"
VIAddVersionKey "LegalCopyright" "MIT License"

; ---- branding ----
!define MUI_ICON "..\src\Roost\Assets\roost.ico"
!define MUI_UNICON "..\src\Roost\Assets\roost.ico"
!define MUI_WELCOMEFINISHPAGE_BITMAP "welcome.bmp"
!define MUI_UNWELCOMEFINISHPAGE_BITMAP "welcome.bmp"
!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "header.bmp"
!define MUI_HEADERIMAGE_RIGHT
!define MUI_ABORTWARNING
!define MUI_FINISHPAGE_RUN "$INSTDIR\Roost.exe"
!define MUI_FINISHPAGE_RUN_TEXT "Launch Roost now"

; ---- default-engine chooser (custom page) ----
Var Dialog
Var RB_Auto
Var RB_WSA
Var RB_BS
Var RB_LD
Var RB_MuMu
Var RB_Nox
Var EngineId

Function EnginePageCreate
  !insertmacro MUI_HEADER_TEXT "Default Android engine" "Pick the engine Roost should use by default. You can change this anytime inside the app."
  nsDialogs::Create 1018
  Pop $Dialog
  ${If} $Dialog == error
    Abort
  ${EndIf}
  ${NSD_CreateLabel} 0 0 100% 20u "Roost runs Android apps through an engine (WSA or an emulator). Choose your preferred one; 'Auto-detect' simply uses whichever is running."
  Pop $0
  ${NSD_CreateRadioButton} 0 26u 100% 11u "Auto-detect (recommended)"
  Pop $RB_Auto
  ${NSD_CreateRadioButton} 0 40u 100% 11u "Windows Subsystem for Android (WSA)"
  Pop $RB_WSA
  ${NSD_CreateRadioButton} 0 54u 100% 11u "BlueStacks"
  Pop $RB_BS
  ${NSD_CreateRadioButton} 0 68u 100% 11u "LDPlayer"
  Pop $RB_LD
  ${NSD_CreateRadioButton} 0 82u 100% 11u "MuMu Player"
  Pop $RB_MuMu
  ${NSD_CreateRadioButton} 0 96u 100% 11u "Nox Player"
  Pop $RB_Nox
  ${NSD_Check} $RB_Auto
  nsDialogs::Show
FunctionEnd

Function EnginePageLeave
  StrCpy $EngineId ""
  ${NSD_GetState} $RB_WSA $0
  ${If} $0 == ${BST_CHECKED}
    StrCpy $EngineId "127.0.0.1:58526"
  ${EndIf}
  ${NSD_GetState} $RB_BS $0
  ${If} $0 == ${BST_CHECKED}
    StrCpy $EngineId "127.0.0.1:5555"
  ${EndIf}
  ${NSD_GetState} $RB_LD $0
  ${If} $0 == ${BST_CHECKED}
    StrCpy $EngineId "127.0.0.1:5555"
  ${EndIf}
  ${NSD_GetState} $RB_MuMu $0
  ${If} $0 == ${BST_CHECKED}
    StrCpy $EngineId "127.0.0.1:7555"
  ${EndIf}
  ${NSD_GetState} $RB_Nox $0
  ${If} $0 == ${BST_CHECKED}
    StrCpy $EngineId "127.0.0.1:62001"
  ${EndIf}
FunctionEnd

; ---- pages ----
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "..\LICENSE"
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
Page custom EnginePageCreate EnginePageLeave
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "Hebrew"

Function .onInit
  !insertmacro MUI_LANGDLL_DISPLAY
FunctionEnd

Function un.onInit
  !insertmacro MUI_UNGETLANGUAGE
FunctionEnd

; ---- sections ----
Section "${APPNAME} (required)" SecMain
  SectionIn RO
  SetOutPath "$INSTDIR"
  File "..\dist\Roost.exe"

  CreateDirectory "$SMPROGRAMS\${APPNAME}"
  CreateShortcut "$SMPROGRAMS\${APPNAME}\${APPNAME}.lnk" "$INSTDIR\Roost.exe"
  CreateShortcut "$SMPROGRAMS\${APPNAME}\Uninstall ${APPNAME}.lnk" "$INSTDIR\uninstall.exe"

  WriteRegStr HKCU "Software\${APPNAME}" "InstallDir" "$INSTDIR"
  WriteUninstaller "$INSTDIR\uninstall.exe"

  WriteRegStr   HKCU "${UNINSTKEY}" "DisplayName"     "${APPNAME}"
  WriteRegStr   HKCU "${UNINSTKEY}" "DisplayVersion"  "${APPVER}"
  WriteRegStr   HKCU "${UNINSTKEY}" "Publisher"       "${PUBLISHER}"
  WriteRegStr   HKCU "${UNINSTKEY}" "DisplayIcon"     "$INSTDIR\Roost.exe"
  WriteRegStr   HKCU "${UNINSTKEY}" "InstallLocation" "$INSTDIR"
  WriteRegStr   HKCU "${UNINSTKEY}" "UninstallString" "$INSTDIR\uninstall.exe"
  WriteRegStr   HKCU "${UNINSTKEY}" "QuietUninstallString" "$INSTDIR\uninstall.exe /S"
  WriteRegDWORD HKCU "${UNINSTKEY}" "NoModify" 1
  WriteRegDWORD HKCU "${UNINSTKEY}" "NoRepair" 1
  ${GetSize} "$INSTDIR" "/S=0K" $0 $1 $2
  IntFmt $0 "0x%08X" $0
  WriteRegDWORD HKCU "${UNINSTKEY}" "EstimatedSize" "$0"

  ; Pre-seed the chosen default engine (only when the app has no settings yet,
  ; so an existing user's configuration is never overwritten).
  ${If} $EngineId != ""
    ${IfNot} ${FileExists} "$APPDATA\${APPNAME}\settings.json"
      CreateDirectory "$APPDATA\${APPNAME}"
      FileOpen $9 "$APPDATA\${APPNAME}\settings.json" w
      FileWrite $9 '{$\r$\n  "DefaultEngineId": "$EngineId"$\r$\n}$\r$\n'
      FileClose $9
    ${EndIf}
  ${EndIf}
SectionEnd

Section "Desktop shortcut" SecDesktop
  CreateShortcut "$DESKTOP\${APPNAME}.lnk" "$INSTDIR\Roost.exe"
SectionEnd

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SecMain} "The Roost application and its Start Menu shortcut."
  !insertmacro MUI_DESCRIPTION_TEXT ${SecDesktop} "Add a shortcut on the Desktop."
!insertmacro MUI_FUNCTION_DESCRIPTION_END

Section "Uninstall"
  Delete "$INSTDIR\Roost.exe"
  Delete "$INSTDIR\uninstall.exe"
  RMDir "$INSTDIR"
  Delete "$SMPROGRAMS\${APPNAME}\${APPNAME}.lnk"
  Delete "$SMPROGRAMS\${APPNAME}\Uninstall ${APPNAME}.lnk"
  RMDir "$SMPROGRAMS\${APPNAME}"
  Delete "$DESKTOP\${APPNAME}.lnk"
  DeleteRegKey HKCU "Software\${APPNAME}"
  DeleteRegKey HKCU "${UNINSTKEY}"
SectionEnd
