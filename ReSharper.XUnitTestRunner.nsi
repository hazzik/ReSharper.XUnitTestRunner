;xUnit plugin for ReSharper 6 and above (by hazzik) installation script
;Written by Eskat0n

;--------------------------------
;Include Modern UI

	!include "MUI2.nsh"

;--------------------------------
;Additional includes

	!include "FileFunc.nsh"
	!include "Sections.nsh"

;--------------------------------
;Global definitions

	!define InstallationName "xUnit plugin for ReSharper"	
	!define InstallRegKeyName "ReSharper.XUnitTestRunner"
	!define InstallRegKey "Software\${InstallRegKeyName}"
	
	!define UninstallerName "Uninstall.ReSharper.XUnitTestRunner.exe"

;--------------------------------
;General

	;Name and file
	Name "${InstallationName}"
	OutFile "${OutputFileName}"

	;Gets installation folder from registry if available
	InstallDirRegKey HKCU ${InstallRegKey} ""

	;Requests application privileges for Windows Vista/7
	RequestExecutionLevel user
  
;--------------------------------
;Interface Settings

	!define MUI_ABORTWARNING

;--------------------------------
;Pages

	!insertmacro MUI_PAGE_LICENSE "License.txt"
	!insertmacro MUI_PAGE_COMPONENTS

	!insertmacro MUI_PAGE_INSTFILES

	!define MUI_FINISHPAGE_LINK "Project page"
	!define MUI_FINISHPAGE_LINK_LOCATION "http://github.com/hazzik/ReSharper.XUnitTestRunner"
	!insertmacro MUI_PAGE_FINISH

	!insertmacro MUI_UNPAGE_CONFIRM
	!insertmacro MUI_UNPAGE_INSTFILES
  
;--------------------------------
;Languages
 
	!insertmacro MUI_LANGUAGE "English"
	!insertmacro MUI_LANGUAGE "Russian"

;--------------------------------
;Translations

	;Section names
	LangString NAME_SecPlugin_71 ${LANG_ENGLISH} "R# v7.1 Plugin"
	LangString NAME_SecPlugin_71 ${LANG_RUSSIAN} "Установка плагина для ReSharper v7.1"
	LangString NAME_SecPlugin_80 ${LANG_ENGLISH} "R# v8.0 Plugin"
	LangString NAME_SecPlugin_80 ${LANG_RUSSIAN} "Установка плагина для ReSharper v8.0"
	
	;Section descriptions
	LangString DESC_SecPlugin_71 ${LANG_ENGLISH} "Installation of xUnit plugin for ReSharper v7.1"
	LangString DESC_SecPlugin_71 ${LANG_RUSSIAN} "Установка плагина xUnit для ReSharper v7.1"
	LangString DESC_SecPlugin_80 ${LANG_ENGLISH} "Installation of xUnit plugin for ReSharper v8.0"
	LangString DESC_SecPlugin_80 ${LANG_RUSSIAN} "Установка плагина xUnit для ReSharper v8.0"

;--------------------------------
;Installer Sections: Main section

!macro InstallPlugin Version
	;Installing plugin
	SetOutPath "$INSTDIR\v${Version}\plugins\XUnitTestRunner"
	File /r "build\XUnitTestRunner\xunit.???"
	File /r "build\XUnitTestRunner\xunit.extensions.???"
	File /r "build\XUnitTestRunner\xunit.runner.utility.???"
	File /r "build\XUnitTestRunner\ReSharper.XUnitTestRunner.${Version}.???"
	File /r "build\XUnitTestRunner\ReSharper.XUnitTestProvider.${Version}.???"
!macroend

Section "Common" SecUninstaller
	SectionIn RO

	;Installing external annotation file
	SetOutPath "$INSTDIR\vAny\ExternalAnnotations"
	File "build\ExternalAnnotations\xunit.xml"

	;Store installation folder
	WriteRegStr HKCU ${InstallRegKey} "" $INSTDIR

	;Create uninstaller
	StrCpy $R0 "$INSTDIR\${UninstallerName}"
	WriteUninstaller $R0
	
	;Add uninstall infromation to Add/Remove Programs
	WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${InstallRegKeyName}" \
					 "DisplayName" "${InstallationName}"
	WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${InstallRegKeyName}" \
					 "UninstallString" "$\"$R0$\""
	WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${InstallRegKeyName}" \
					 "QuietUninstallString" "$\"$R0$\" /S"
	WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${InstallRegKeyName}" \
					 "DisplayVersion" "1.0"
	WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${InstallRegKeyName}" \
					 "DisplayIcon" "$INSTDIR\${UninstallerName},0"				 
	WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${InstallRegKeyName}" \
					 "Publisher" "Community"
                     
	;Specify EstimatedSize for installation
	${GetSize} "$INSTDIR" "/S=0K" $0 $1 $2
	IntFmt $0 "0x%08X" $0
	WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${InstallRegKeyName}" \
					   "EstimatedSize" "$0"

SectionEnd

Section "!$(NAME_SecPlugin_71)" SecPlugin_71
    !insertmacro InstallPlugin "7.1"
SectionEnd

Section "!$(NAME_SecPlugin_80)" SecPlugin_80
    !insertmacro InstallPlugin "8.0"
SectionEnd

;--------------------------------
;Installer Sections: macros

;--------------------------------
;Installer Sections: satellite sections

;--------------------------------
;Descriptions	

	;Assign language strings to sections
	!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
	!insertmacro MUI_DESCRIPTION_TEXT ${SecPlugin_71} $(DESC_SecPlugin_71)
	!insertmacro MUI_DESCRIPTION_TEXT ${SecPlugin_80} $(DESC_SecPlugin_80)
	!insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
;Uninstaller Section

Section "Uninstall"

	;Delete plugin files
	RMDir /r "$INSTDIR\v7.1\plugins\XUnitTestRunner"
	;Delete plugin files
	RMDir /r "$INSTDIR\v8.0\plugins\XUnitTestRunner"
	;Delete external annotation file
	Delete "$INSTDIR\vAny\ExternalAnnotations\xunit.xml"
	;Delete uninstaller
	Delete "$INSTDIR\${UninstallerName}"
	
	;Remove uninstall infromation to Add/Remove Programs
	DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${InstallRegKeyName}"

	DeleteRegKey /ifempty HKCU ${InstallRegKey}
SectionEnd

;--------------------------------
;Global event handlers

Function .onInit
	;Form path to ReSharper's app data directory for current user
	StrCpy $R1 "$APPDATA\JetBrains\ReSharper"
	
	;Check whether ReSharper's app data directory exists
	${DirState} $R1 $R2
	StrCmp $R2 "-1" 0 +3
	MessageBox MB_OK|MB_ICONSTOP "Unable to locate ReSharper directory under current user's ApplicationData$\r$\nInstallation process aborted" /SD IDOK
	Quit
	
	;Sets the default installation folder to ReSharper's app data directory for currect user
	StrCpy $INSTDIR $R1	
FunctionEnd