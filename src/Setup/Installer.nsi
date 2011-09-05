;xUnit plugin for ReSharper 6.0 (by hazzik) installation script
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

	!define InstallationName "xUnit plugin for ReSharper 6.0"	
	!define InstallRegKeyName "ReSharper.XUnitTestRunner.60"
	!define InstallRegKey "Software\${InstallRegKeyName}"
	
	!define OutputFileName "ReSharper.XUnitTestRunner.6.0.exe"
	!define UninstallerName "Uninstall ${OutputFileName}"

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
	LangString NAME_SecPlugin ${LANG_ENGLISH} "Plugin installation"
	LangString NAME_SecPlugin ${LANG_RUSSIAN} "��������� �������"
	
	LangString NAME_SecNoTemplates ${LANG_ENGLISH} "Do not install templates"
	LangString NAME_SecNoTemplates ${LANG_RUSSIAN} "�� ������������� �������"
	
	LangString NAME_SecAeTemplates ${LANG_ENGLISH} "Install ae- templates"
	LangString NAME_SecAeTemplates ${LANG_RUSSIAN} "���������� ������� ae-"
	
	LangString NAME_SecXeTemplates ${LANG_ENGLISH} "Install xe- templates"
	LangString NAME_SecXeTemplates ${LANG_RUSSIAN} "���������� ������� xe-"
	
	;Section descriptions
	LangString DESC_SecPlugin ${LANG_ENGLISH} "Installation of xUnit plugin for ReSharper 6.0"
	LangString DESC_SecPlugin ${LANG_RUSSIAN} "��������� ������� xUnit ��� ReSharper 6.0"

;--------------------------------
;Installer Sections: Main section

Section "!$(NAME_SecPlugin)" SecPlugin

	;Mark section as readonly
	SectionIn RO

	;Installing plugin
	SetOutPath "$INSTDIR\vs10.0\plugins\XUnitTestRunner"
	File /r "data\XUnitTestRunner\*"
	
	;Installing external annotation file
	SetOutPath "$INSTDIR\vs10.0\ExternalAnnotations"
	File "data\ExternalAnnotations\xunit.xml"

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

;--------------------------------
;Installer Sections: macros

!macro InstallTemplates liveTemplates
	
	;Extract ae- live templates to installation directory
	SetOutPath "$INSTDIR\vs10.0"
	File "data\LiveTemplates\${liveTemplates}"
	
	;Extract live templates installer
	File "data\TemplatesInstaller.exe"
	
	;Executing live templates installer
	nsExec::ExecToLog '"$INSTDIR\vs10.0\TemplatesInstaller.exe" "$INSTDIR\vs10.0" "${liveTemplates}"'
	Pop $1 ;Output value
	
	StrCmp "$1" "1" 0 +2
	MessageBox MB_OK "Error while installing live templates"
	
	;Delete ae- live templates and installer from installation directory
	Delete "$INSTDIR\vs10.0\${liveTemplates}"
	Delete "$INSTDIR\vs10.0\TemplatesInstaller.exe"

!macroend

;--------------------------------
;Installer Sections: satellite sections

Section $(NAME_SecNoTemplates) SecNoTemplates
	
	;Empty section

SectionEnd

Section /o $(NAME_SecAeTemplates) SecAeTemplates
	
	!insertmacro InstallTemplates "xunit-ae.xml"

SectionEnd

Section /o $(NAME_SecXeTemplates) SecXeTemplates

	!insertmacro InstallTemplates "xunit-xe.xml"

SectionEnd

;--------------------------------
;Descriptions	

	;Assign language strings to sections
	!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
	!insertmacro MUI_DESCRIPTION_TEXT ${SecPlugin} $(DESC_SecPlugin)
	!insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
;Uninstaller Section

Section "Uninstall"

	;Delete plugin files
	RMDir /r "$INSTDIR\vs10.0\plugins\XUnitTestRunner"
	;Delete external annotation file
	Delete "$INSTDIR\vs10.0\ExternalAnnotations\xunit.xml"
	;Delete uninstaller
	Delete "$INSTDIR\${UninstallerName}"
	
	;Remove uninstall infromation to Add/Remove Programs
	DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${InstallRegKeyName}"

	DeleteRegKey /ifempty HKCU ${InstallRegKey}

SectionEnd

;--------------------------------
;Global event handlers

Function .onInit

	;Try to get ReSharper's installation directory from registry key
	ReadRegStr $R0 HKLM Software\JetBrains\ReSharper\v6.0\vs10.0 "InstallDir"  
	
	;Check whether specified registry key exists or not
	StrCmp $R0 "" 0 +3
	MessageBox MB_OK|MB_ICONSTOP "Unable to locate ReSharper 6.0$\r$\nInstallation process aborted" /SD IDOK
	Quit
	
	;Form path to ReSharper's app data directory for current user
	StrCpy $R1 "$APPDATA\JetBrains\ReSharper\v6.0"
	
	;Check whether ReSharper's app data directory exists
	${DirState} $R1 $R2
	StrCmp $R2 "-1" 0 +3
	MessageBox MB_OK|MB_ICONSTOP "Unable to locate ReSharper 6.0 directory under current user's ApplicationData$\r$\nInstallation process aborted" /SD IDOK
	Quit
	
	;Sets the default installation folder to ReSharper's app data directory for currect user
	StrCpy $INSTDIR $R1	
	
	;SecNoTemplates is selected by default
	StrCpy $R9 ${SecNoTemplates}
	
FunctionEnd

Function .onSelChange

	;Process radio button section selection change
	!insertmacro StartRadioButtons $R9
		!insertmacro RadioButton ${SecNoTemplates}
		!insertmacro RadioButton ${SecAeTemplates}
		!insertmacro RadioButton ${SecXeTemplates}
	!insertmacro EndRadioButtons	

FunctionEnd