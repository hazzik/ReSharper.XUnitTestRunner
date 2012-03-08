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

	!define InstallationName "xUnit plugin for ReSharper ${Version}"	
	!define InstallRegKeyName "ReSharper.XUnitTestRunner.${Version}"
	!define InstallRegKey "Software\${InstallRegKeyName}"
	
	!define UninstallerName "Uninstall.ReSharper.XUnitTestRunner.${Version}.exe"

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
	LangString NAME_SecPlugin ${LANG_RUSSIAN} "Установка плагина"
	
	LangString NAME_SecNoTemplates ${LANG_ENGLISH} "Do not install templates"
	LangString NAME_SecNoTemplates ${LANG_RUSSIAN} "Не устанавливать шаблоны"
	
	LangString NAME_SecAeTemplates ${LANG_ENGLISH} "Install ae- templates"
	LangString NAME_SecAeTemplates ${LANG_RUSSIAN} "Установить шаблоны ae-"
	
	LangString NAME_SecXeTemplates ${LANG_ENGLISH} "Install xe- templates"
	LangString NAME_SecXeTemplates ${LANG_RUSSIAN} "Установить шаблоны xe-"
	
	;Section descriptions
	LangString DESC_SecPlugin ${LANG_ENGLISH} "Installation of xUnit plugin for ReSharper ${Version}"
	LangString DESC_SecPlugin ${LANG_RUSSIAN} "Установка плагина xUnit для ReSharper ${Version}"

;--------------------------------
;Installer Sections: Main section

Section "!$(NAME_SecPlugin)" SecPlugin

	;Mark section as readonly
	SectionIn RO

	;Installing plugin
	SetOutPath "$INSTDIR\plugins\XUnitTestRunner"
	File /r "build\XUnitTestRunner\*"
	
	;Installing external annotation file
	SetOutPath "$INSTDIR\ExternalAnnotations"
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

;--------------------------------
;Installer Sections: macros

!macro InstallTemplates liveTemplates
	
	;Extract ae- live templates to installation directory
	SetOutPath "$INSTDIR"
	File "build\LiveTemplates\${liveTemplates}"
	
	;Extract live templates installer
	File "build\TemplatesInstaller.exe"
	
	;Executing live templates installer
	nsExec::ExecToLog '"$INSTDIR\TemplatesInstaller.exe" "$INSTDIR" "${liveTemplates}"'
	Pop $1 ;Output value
	
	StrCmp "$1" "1" 0 +2
	MessageBox MB_OK "Error while installing live templates"
	
	;Delete ae- live templates and installer from installation directory
	Delete "$INSTDIR\${liveTemplates}"
	Delete "$INSTDIR\TemplatesInstaller.exe"

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
	RMDir /r "$INSTDIR\plugins\XUnitTestRunner"
	;Delete external annotation file
	Delete "$INSTDIR\ExternalAnnotations\xunit.xml"
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
	StrCpy $R1 "$APPDATA\JetBrains\ReSharper\v${Version}"
	
	;Check whether ReSharper's app data directory exists
	${DirState} $R1 $R2
	StrCmp $R2 "-1" 0 +3
	MessageBox MB_OK|MB_ICONSTOP "Unable to locate ReSharper ${Version} directory under current user's ApplicationData$\r$\nInstallation process aborted" /SD IDOK
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