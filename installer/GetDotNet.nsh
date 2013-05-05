!include LogicLib.nsh

!macro GetDotNet
;  full 4.0 framework url
  !define DOTNET_URL "http://download.microsoft.com/download/B/A/4/BA4A7E71-2906-4B2D-A0E1-80CF16844F5F/dotNetFx45_Full_setup.exe"
;  client framework url  
;  !define DOTNET_URL "http://download.microsoft.com/download/5/6/2/562A10F9-C9F4-4313-A044-9C94E0A8FAC8/dotNetFx40_Client_x86_x64.exe"
 
; DownloadDotNET:
  DetailPrint "Beginning download of latest .NET Framework version."
  inetc::get /TIMEOUT=30000 ${DOTNET_URL} "$TEMP\dotNetFx45_Full_setup.exe" /END
  Pop $0
  DetailPrint "Result: $0"
  StrCmp $0 "OK" InstallDotNet
  StrCmp $0 "cancelled" GiveUpDotNET
  inetc::get /TIMEOUT=30000 /NOPROXY ${DOTNET_URL} "$TEMP\dotNetFx45_Full_setup.exe" /END
  Pop $0
  DetailPrint "Result: $0"
  StrCmp $0 "OK" InstallDotNet
 
  MessageBox MB_ICONSTOP "Download failed: $0"
  Abort
  InstallDotNet:
  DetailPrint "Completed download."
  Pop $0
  ${If} $0 == "cancel"
    MessageBox MB_YESNO|MB_ICONEXCLAMATION \
    "Download cancelled.  Continue Installation?" \
    IDYES NewDotNET IDNO GiveUpDotNET
  ${EndIf}
;  TryFailedDownload:
  DetailPrint "Pausing installation while downloaded .NET Framework installer runs."
  DetailPrint "Installation could take several minutes to complete."
  ExecWait '$TEMP\dotNetFx45_Full_setup.exe /q /norestart /c:"install /q"'
  DetailPrint "Completed .NET Framework install/update. Removing .NET Framework installer."
  Delete "$TEMP\dotNetFx45_Full_setup.exe"
  DetailPrint ".NET Framework installer removed."
  goto NewDotNet
 
GiveUpDotNET:
  Abort "Installation cancelled by user."
 
NewDotNET:
  Pop $7
  Pop $6
  Pop $5
  Pop $4
  Pop $3
  Pop $2
  Pop $1
  Pop $0
!macroend