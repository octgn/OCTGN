!include "LogicLib.nsh"
!include "GetVC.nsh"

Name "OCTGN 3.1.15.98"
OutFile "OCTGN-Test-Setup-3.1.15.98.exe"
ShowInstDetails show
LoadLanguageFile "${NSISDIR}\Contrib\Language files\English.nlf"

; Version Information
VIProductVersion "3.1.15.98"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "OCTGN - Test"
VIAddVersionKey /LANG=${LANG_ENGLISH} "Comments" "A tabletop engine"
VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "OCTGN"
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalTrademarks" ""
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" ""
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "OCTGN release 3"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "3.1.15.98"

; Make plugin directory same as script
!addplugindir .

; Installation directory
InstallDir $DOCUMENTS\OCTGN\OCTGN-Test

; Create registry key for installation directory
; CHANGE THIS BACK AFTER THIS RELEASE
;InstallDirRegKey HKCU "Software\OCTGN" "Install_Dir"

; Request application privileges for Windows Vista
;RequestExecutionLevel admin

;Pages
Page components
;Page directory
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles

Function CheckAndDownloadDotNet45
	# Let's see if the user has the .NET Framework 4.5 installed on their system or not
	# Remember: you need Vista SP2 or 7 SP1.  It is built in to Windows 8, and not needed
	# In case you're wondering, running this code on Windows 8 will correctly return is_equal
	# or is_greater (maybe Microsoft releases .NET 4.5 SP1 for example)
 
	# Set up our Variables
	Var /GLOBAL dotNET45IsThere
	Var /GLOBAL dotNET_CMD_LINE
	Var /GLOBAL EXIT_CODE
 
        # We are reading a version release DWORD that Microsoft says is the documented
        # way to determine if .NET Framework 4.5 is installed
	ReadRegDWORD $dotNET45IsThere HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" "Release"
	IntCmp $dotNET45IsThere 378389 is_equal is_less is_greater
 
	is_equal:
		Goto done_compare_not_needed
	is_greater:
		# Useful if, for example, Microsoft releases .NET 4.5 SP1
		# We want to be able to simply skip install since it's not
		# needed on this system
		Goto done_compare_not_needed
	is_less:
		Goto done_compare_needed
 
	done_compare_needed:
		#.NET Framework 4.5 install is *NEEDED*
 
		# Microsoft Download Center EXE:
		# Web Bootstrapper: http://go.microsoft.com/fwlink/?LinkId=225704
		# Full Download: http://go.microsoft.com/fwlink/?LinkId=225702

		StrCpy $dotNET_CMD_LINE "/q /norestart"
		Goto LookForLocalFile
 
		LookForLocalFile:
			# Let's see if the user stored the Full Installer
			IfFileExists "$EXEPATH\dotNET45Full.exe" do_local_install do_network_install
 
			do_local_install:
				# .NET Framework found on the local disk.  Use this copy
 
				ExecWait '"$EXEPATH\components\dotNET45Full.exe" $dotNET_CMD_LINE' $EXIT_CODE
				Goto is_reboot_requested
 
			# Now, let's Download the .NET
			do_network_install:
 
				Var /GLOBAL dotNetDidDownload
				NSISdl::download "http://go.microsoft.com/fwlink/?LinkId=225702" "$TEMP\dotNET45Full.exe" $dotNetDidDownload
 
				StrCmp $dotNetDidDownload success fail
				success:
					ExecWait '"$TEMP\dotNET45Full.exe" $dotNET_CMD_LINE' $EXIT_CODE
					Goto is_reboot_requested
 
				fail:
					MessageBox MB_OK|MB_ICONEXCLAMATION "Unable to download .NET Framework.  Program will be installed, but will not function without the Framework!"
					Goto done_dotNET_function
 
				# $EXIT_CODE contains the return codes.  1641 and 3010 means a Reboot has been requested
				is_reboot_requested:
					${If} $EXIT_CODE = 1641
					${OrIf} $EXIT_CODE = 3010
						SetRebootFlag true
					${EndIf}
 
	done_compare_not_needed:
		# Done dotNET Install
		Goto done_dotNET_function
 
	#exit the function
	done_dotNET_function:
 
    FunctionEnd

; DotNet Checkup and Install
Section ""
  Call CheckAndDownloadDotNet45
  !insertmacro GetVC++
SectionEnd

	
 
; Start default section
Section "Main"
  SectionIn RO
  ; set the installation directory as the destination for the following actions
  SetOutPath $INSTDIR
 
  ; Write the installation path into the registry
  WriteRegStr HKCU "SOFTWARE\OCTGN-Test" "Install_Dir" "$INSTDIR"

  ; create the uninstaller
  WriteUninstaller "$INSTDIR\uninstall.exe"
 
  ; set folder and files to be included in setup
  File /r ..\octgnFX\OCTGN\bin\Release_Test\*.*
SectionEnd

Section "Start Menu Shortcuts"
  ; Entry for Start Menu shortcuts. Optional
  CreateDirectory "$SMPROGRAMS\OCTGN-Test"  
  CreateShortCut "$SMPROGRAMS\OCTGN-Test\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  CreateShortCut "$SMPROGRAMS\OCTGN-Test\OCTGN-Test.lnk" "$INSTDIR\OCTGN.exe" "" "$INSTDIR\OCTGN.exe" 0
SectionEnd

Section ""
  ; Run hash program
  ExecWait '"$INSTDIR\HashGenCLI.exe"' $0
SectionEnd

Section "Launch OCTGN-Test"
Exec "$INSTDIR\OCTGN.exe"
SectionEnd
 
Section "Uninstall" 
  Delete $INSTDIR\uninstall.exe

  ; Remove registry keys
  DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\OCTGN-Test"
  DeleteRegKey HKCU SOFTWARE\OCTGN-Test

  ; Remove shortcuts, if any
  Delete "$SMPROGRAMS\OCTGN-Test\*.*"

  ; Remove directories used
  RMDir "$SMPROGRAMS\OCTGN-Test"
  RMDir /r /REBOOTOK $INSTDIR
SectionEnd
