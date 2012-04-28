!include "LogicLib.nsh"
!include "DotNetVer.nsh"
!include "GetDotNet.nsh"
!include "GetVC.nsh"

Name "OCTGN 3.0.1.5"
OutFile "OCTGN Setup.exe"
ShowInstDetails show
LoadLanguageFile "${NSISDIR}\Contrib\Language files\English.nlf"

; Version Information
VIProductVersion "3.0.1.4"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "OCTGN"
VIAddVersionKey /LANG=${LANG_ENGLISH} "Comments" "A tabletop engine"
VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "Skylabs"
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalTrademarks" ""
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" ""
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "OCTGN release 2"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "3.0.1.5"

; Make plugin directory same as script
!addplugindir .

; Installation directory
InstallDir $PROGRAMFILES\OCTGN

; Create registry key for installation directory
InstallDirRegKey HKCU "Software\OCTGN" "Install_Dir"

; Request application privileges for Windows Vista
RequestExecutionLevel admin

;Pages
Page components
Page directory
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles

; DotNet Checkup and Install
Section ""
  ${If} ${DOTNETVER_4_0} HasDotNetClientProfile 1
	DetailPrint "Microsoft .NET Framework 4.0 (Client Profile) available."
  ${Else}
	DetailPrint "Microsoft .NET Framework 4.0 (Client Profile) missing."
	!insertmacro GetDotNet
  ${EndIf}
  !insertmacro GetVC++
SectionEnd
 
; Start default section
Section "Main"
  SectionIn RO
  ; set the installation directory as the destination for the following actions
  SetOutPath $INSTDIR
 
  ; Write the installation path into the registry
  WriteRegStr HKCU "SOFTWARE\OCTGN" "Install_Dir" "$INSTDIR"

  ; create the uninstaller
  WriteUninstaller "$INSTDIR\uninstall.exe"
 
  ; set folder and files to be included in setup
  File /r ..\octgnFX\OCTGN\bin\x86\Release\*.*
SectionEnd

Section "Start Menu Shortcuts"
  ; Entry for Start Menu shortcuts. Optional
  CreateDirectory "$SMPROGRAMS\OCTGN"  
  CreateShortCut "$SMPROGRAMS\OCTGN\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  CreateShortCut "$SMPROGRAMS\OCTGN\OCTGN.lnk" "$INSTDIR\OCTGN.exe" "" "$INSTDIR\OCTGN.exe" 0
SectionEnd

Section ""
  ; Run hash program
  ExecWait '"$INSTDIR\HashGenCLI.exe"' $0
SectionEnd
 
Section "Uninstall" 
  ; Has to be removed first for some reason
  Delete $INSTDIR\uninstall.exe

  ; Remove registry keys
  DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\OCTGN"
  DeleteRegKey HKCU SOFTWARE\OCTGN

  ; Remove shortcuts, if any
  Delete "$SMPROGRAMS\OCTGN\*.*"

  ; Remove directories used
  RMDir "$SMPROGRAMS\OCTGN"
  RMDir /r /REBOOTOK $INSTDIR
SectionEnd
