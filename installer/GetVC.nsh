!include LogicLib.nsh

!macro GetVC++
  !define VCplus_URL "http://download.microsoft.com/download/5/B/C/5BC5DBB3-652D-4DCE-B14A-475AB85EEF6E/vcredist_x86.exe"
 
; DownloadDotNET:
  DetailPrint "Beginning download of latest VC++ Redistributable."
  inetc::get /TIMEOUT=30000 ${VCplus_URL} "$TEMP\vcredist_x86.exe" /END
  Pop $0
  DetailPrint "Result: $0"
  StrCmp $0 "OK" InstallVCplusplus
  StrCmp $0 "cancelled" GiveUpVCplusplus
  inetc::get /TIMEOUT=30000 /NOPROXY ${VCplus_URL} "$TEMP\vcredist_x86.exe" /END
  Pop $0
  DetailPrint "Result: $0"
  StrCmp $0 "OK" InstallVCplusplus
 
  MessageBox MB_ICONSTOP "Download failed: $0"
  Abort
  InstallVCplusplus:
  DetailPrint "Completed download."
  Pop $0
  ${If} $0 == "cancel"
    MessageBox MB_YESNO|MB_ICONEXCLAMATION \
    "Download cancelled.  Continue Installation?" \
    IDYES NewVCplusplus IDNO GiveUpVCplusplus
  ${EndIf}
;  TryFailedDownload:
  DetailPrint "Pausing installation while downloaded VC++ installer runs."
  DetailPrint "Installation could take several minutes to complete."
  ExecWait '$TEMP\vcredist_x86.exe /passive /norestart'
  DetailPrint "Completed .NET Framework install/update. Removing VC++ installer."
  Delete "$TEMP\vcredist_x86.exe"
  DetailPrint "VC++ installer removed."
  goto NewVCplusplus
 
GiveUpVCplusplus:
  Abort "Installation cancelled by user."
 
NewVCplusplus:
  Pop $7
  Pop $6
  Pop $5
  Pop $4
  Pop $3
  Pop $2
  Pop $1
  Pop $0
!macroend