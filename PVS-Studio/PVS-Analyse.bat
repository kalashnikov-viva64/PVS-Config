rem Usage: PVS-Analyse.bat <PVS_Platform> <PVS_DelayMin> <PVS_NumPart>
rem <PVS_Platform> - Dalet_x86_trunk, Dalet_x64_trunk, Amberfin_x64
rem <PVS_DelayMin> - build time in minutes
rem <PVS_NumPart> - number of parts
@echo on
@setlocal

rem Command line parameters
set PVS_Platform=%1
set PVS_DelayMin=%2
set PVS_NumPart=%3

rem Platform selection
if %PVS_Platform% EQU Dalet_x86_trunk goto lblDalet_x86_trunk
if %PVS_Platform% EQU Dalet_x64_trunk goto lblDalet_x64_trunk
if %PVS_Platform% EQU Amberfin_x64 goto lblAmberfin_x64
goto lblError
:lblDalet_x86_trunk
  set PVS_MSVC=devenv.exe
  set PVS_SolutionDir=S:\src\env\MasterBuild
  set PVS_Solution=generated-all-projects-with-dependencies_%PVS_NumPart%.sln
  set PVS_PlogDir=C:\PVS Dalet logs x86 trunk\temp
  set PVS_PlogFile=%PVS_PlogDir%\generated-x86-projects_%PVS_NumPart%.plog
  set PVS_PlogFileWithSuppress=%PVS_PlogDir%\generated-x86-projects_%PVS_NumPart%_WithSuppressedMessages.plog
  set PVS_PluginExec="PVSStudio.CheckSolution Win32|Release|%PVS_PlogFile%|||SuppressAll"
  cd /d S:\src\env\MasterBuild
  call set-daletplus-dev-path.bat
  call SetMsdevEnv.bat
  goto lblEndIf
:lblDalet_x64_trunk
  set PVS_MSVC=devenv.exe
  set PVS_SolutionDir=S:\src\env\MasterBuild
  set PVS_Solution=generated-x64-projects_%PVS_NumPart%.sln
  set PVS_PlogDir=C:\PVS Dalet logs x64 trunk\temp
  set PVS_PlogFile=%PVS_PlogDir%\generated-x64-projects_%PVS_NumPart%.plog
  set PVS_PlogFileWithSuppress=%PVS_PlogDir%\generated-x64-projects_%PVS_NumPart%_WithSuppressedMessages.plog
  set PVS_PluginExec="PVSStudio.CheckSolution x64|Release|%PVS_PlogFile%|||SuppressAll"
  cd /d S:\src\env\MasterBuild
  call set-daletplus-dev-path-x64.bat
  call SetMsdevEnvx64.bat
  goto lblEndIf
:lblAmberfin_x64
  cd /d f:\ANT-Build\ANT-Workspace\
  call set-amberfin-paths.bat x64 Release
  rem set PVS_MSVC=C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\devenv.com
  set PVS_MSVC=%Amberfin_Compiler%
  rem set PVS_SolutionDir=%Amberfin_Workspace%
  set PVS_SolutionDir=F:\ANT-Build\ANT-Workspace\libdsp\libdspdev\build\ms100_libdsp\
  set PVS_Solution=ms100_libdsp.sln
  set PVS_PlogDir=C:\PVS_Amberfin_x64\temp
  set PVS_PlogFile=%PVS_PlogDir%\PVS_Amberfin_x64.plog
  set PVS_PlogFileWithSuppress=%PVS_PlogDir%\PVS_Amberfin_x64_WithSuppressedMessages.plog
  set PVS_PluginExec="PVSStudio.CheckSolution x64|Release|%PVS_PlogFile%|||SuppressAll"
  goto lblEndIf
:lblEndIf

rem Make several analysis attempts
cd /d %PVS_SolutionDir%
@echo on

if not exist %PVS_Solution% goto lblError
set I=1
set Cnt=0
set /a CntMax="%PVS_DelayMin%*4"

:lblNextAttempt
	echo %TIME%: Starting PVS-Analyse attempt %I%
	
	rem Make sure that devenv.exe is unloaded
	TASKLIST /FI "IMAGENAME eq devenv.exe" | FIND "devenv.exe" > NUL
	if %ERRORLEVEL% NEQ 0 goto lblDevenvUnloaded
      start /wait taskkill /f /im devenv.exe
	  del /A:H *.suo
	  del *.sdf
:lblDevenvUnloaded
	
    rem Run the analysis
    rem start devenv.exe /useenv %PVS_Solution% /command %PVS_PluginExec%
	call "%PVS_MSVC%" %PVS_Solution% /command %PVS_PluginExec%

    rem Wait for the analysis completion
	@echo off
:lblWaitDevEnv
    TASKLIST /FI "IMAGENAME eq devenv.exe" | FIND "devenv.exe" > NUL
    if %ERRORLEVEL% NEQ 0 goto lblCheckPlog
	rem "timeout" does not work with IncrediBuild, and "sleep" is external tool for Windows
	rem timeout /T 15
	sleep 15
    set /a Cnt+=1
    if %Cnt% LEQ %CntMax% goto lblWaitDevEnv
	goto lblAnalyseFail
	
    rem Check the Plog
:lblCheckPlog
	@echo on
    if exist "%PVS_PlogFile%" goto lblAllOk

	rem Forced analysis completion and retry
:lblAnalyseFail
	@echo on
	echo Analyse fail
    start /wait taskkill /f /im devenv.exe
	del /A:H *.suo
	del *.sdf
	rem timeout /T 300
	sleep 300
	set /a I+=1
    if %I% LEQ 10 if %Cnt% LEQ %CntMax% goto lblNextAttempt

:lblError
@endlocal
exit /b 2

:lblAllOk
	if exist "%PVS_PlogFileWithSuppress%" goto lblWithSuppress
	copy "%PVS_PlogFile%" "%PVS_PlogFileWithSuppress%"
:lblWithSuppress
@endlocal
exit /b 0