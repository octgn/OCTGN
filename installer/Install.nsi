!include "MUI.nsh"
!include "nsDialogs.nsh"
!include "WinVer.nsh"
!include "LogicLib.nsh"
!include "DotNetVer.nsh"
!include "GetDotNet.nsh"
!include "GetVC.nsh"
!include "WarningXpPage.nsdinc"

Name "OCTGN 3.1.129.298"
OutFile "OCTGN-Setup-3.1.129.298.exe"
ShowInstDetails show
LoadLanguageFile "${NSISDIR}\Contrib\Language files\English.nlf"

; Version Information
VIProductVersion "3.1.129.298"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "OCTGN"
VIAddVersionKey /LANG=${LANG_ENGLISH} "Comments" "A tabletop engine"
VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "OCTGN"
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalTrademarks" ""
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" ""
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "OCTGN release 3"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "3.1.129.298"

; Make plugin directory same as script
!addplugindir .

; Installation directory
InstallDir $DOCUMENTS\OCTGN\OCTGN

; Create registry key for installation directory
; CHANGE THIS BACK AFTER THIS RELEASE
;InstallDirRegKey HKCU "Software\OCTGN" "Install_Dir"

; Request application privileges for Windows Vista
RequestExecutionLevel user

;Pages
;Page custom fnc_WarningXpPage_Show
Page components
;Page directory
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles

; DotNet Checkup and Install
Section ""
  ;${IfNot} ${AtLeastWinVista}
    ${If} ${DOTNETVER_4_0} HasDotNetFullProfile 1
      DetailPrint "Microsoft .NET Framework 4.0 available."
    ${Else}
      DetailPrint "Microsoft .NET Framework 4.0 missing."
      !insertmacro GetDotNet
    ${EndIf}
  ;${Else}
    ; Magic numbers from http://msdn.microsoft.com/en-us/library/ee942965.aspx
   ; ClearErrors
    ;ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" "Release"

    ;IfErrors NotDetected

    ;${If} $0 >= 378389

        ;DetailPrint "Microsoft .NET Framework 4.5 is installed ($0)"
    ;${Else}
    ;NotDetected:
        ;DetailPrint "Installing Microsoft .NET Framework 4.5"
        ;SetDetailsPrint listonly
        ;ExecWait '"$INSTDIR\Tools\dotNetFx45_Full_setup.exe" /passive /norestart' $0
        ;${If} $0 == 3010 
        ;${OrIf} $0 == 1641
            ;DetailPrint "Microsoft .NET Framework 4.5 installer requested reboot"
            ;SetRebootFlag true
        ;${EndIf}
        ;SetDetailsPrint lastused
        ;DetailPrint "Microsoft .NET Framework 4.5 installer returned $0"
    ;${EndIf}
  ;${EndIf}
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
  File /r ..\octgnFX\OCTGN\bin\Release\*.*
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

Section "Launch OCTGN"
Exec "$INSTDIR\OCTGN.exe"
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
