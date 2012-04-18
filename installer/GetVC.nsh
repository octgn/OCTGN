!include LogicLib.nsh
!include x64.nsh

!macro GetVC++
  !define VCplus_URL "http://download.microsoft.com/download/5/B/C/5BC5DBB3-652D-4DCE-B14A-475AB85EEF6E/vcredist_x86.exe"
  !define VCplus_URL64 "http://download.microsoft.com/download/3/2/2/3224B87F-CFA0-4E70-BDA3-3DE650EFEBA5/vcredist_x64.exe"
 
; CheckVCplusplus:
  Push 0
  Pop $1
  ;${If} ${RunningX64} 
  ; ReadRegDWORD $1 HKLM Software\Microsoft\VisualStudio\10.0\VC\VCRedist\x64 Installed
  ;${else}
   ReadRegDWORD $1 HKLM Software\Microsoft\VisualStudio\10.0\VC\VCRedist\x86 Installed
  ;${EndIf}
  IntCmp $1 1 VCplusplusInstalled DownloadVCplusplus DownloadVCplusplus
  
  VCplusplusInstalled:
  DetailPrint "VC++ Runtime already installed."
  goto NewVCplusplus
  
; DownloadDotNET:
  DownloadVCplusplus:
  DetailPrint "Beginning download of latest VC++ Redistributable."
  ;${If} ${RunningX64} 
  ; inetc::get /TIMEOUT=30000 ${VCplus_URL64} "$TEMP\vcredist_x64.exe" /END
  ;${else}
   inetc::get /TIMEOUT=30000 ${VCplus_URL} "$TEMP\vcredist_x86.exe" /END
  ;${EndIf}
  Pop $0
  DetailPrint "Result: $0"
  StrCmp $0 "OK" InstallVCplusplus
  StrCmp $0 "cancelled" GiveUpVCplusplus
  ;${If} ${RunningX64} 
  ; inetc::get /TIMEOUT=30000 /NOPROXY ${VCplus_URL64} "$TEMP\vcredist_x64.exe" /END
  ;${else}
   inetc::get /TIMEOUT=30000 /NOPROXY ${VCplus_URL} "$TEMP\vcredist_x86.exe" /END
  ;${EndIf}
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
  ;${If} ${RunningX64} 
  ; ExecWait '$TEMP\vcredist_x64.exe /passive /norestart'
  ;${else}
   ExecWait '$TEMP\vcredist_x86.exe /passive /norestart'
  ;${EndIf}
  DetailPrint "Completed .NET Framework install/update. Removing VC++ installer."
  ;${If} ${RunningX64} 
  ; Delete "$TEMP\vcredist_x64.exe"
  ;${else}
   Delete "$TEMP\vcredist_x86.exe"
  ;${EndIf}
  
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