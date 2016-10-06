rem Usage: Analyse.bat <PVS_Platform> <PVS_IncrediBuild> <PVS_Folder> <PVS_CmdVer>
rem <PVS_Platform> - x86, x64
rem <PVS_IncrediBuild> - UseIB, NotUseIB
rem <PVS_Folder> - folder
rem <PVS_CmdVer> - "", CmdVer
@echo on
@setlocal
pushd %~dp0
cd /d %~dp0
set PVS_Platform=%1
set PVS_IncrediBuild=%2
set PVS_Folder=%3
set PVS_CmdVer=%4
echo %TIME%: Starting Analyse.bat

if %PVS_Platform% EQU Amberfin_x64 goto lblAmberfin_x64
if %PVS_Platform% EQU x86 if %PVS_IncrediBuild% EQU UseIB goto lblx86_UseIB
if %PVS_Platform% EQU x86 if %PVS_IncrediBuild% EQU NotUseIB goto lblx86_NotUseIB
if %PVS_Platform% EQU x64 if %PVS_IncrediBuild% EQU UseIB goto lblx64_UseIB
if %PVS_Platform% EQU x64 if %PVS_IncrediBuild% EQU NotUseIB if "%PVS_CmdVer%" NEQ "CmdVer" goto lblx64_NotUseIB
if %PVS_Platform% EQU x64 if %PVS_IncrediBuild% EQU NotUseIB if "%PVS_CmdVer%" EQU "CmdVer" goto lblx64_NotUseIB_CmdVer
goto lblError

:lblAmberfin_x64
  call c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Amberfin_x64 333 1
  goto lblEndIf2

:lblx86_UseIB
  call "C:\Program Files (x86)\Xoreax\IncrediBuild\ibconsole.exe" ^
	/command="c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Dalet_x86_trunk 333 1 && c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Dalet_x86_trunk 333 2 && c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Dalet_x86_trunk 333 3" ^
	/profile=c:\PVS-Config\IncrediBuild\check_solution_x86_incredibuild.xml
  goto lblEndIf
  
:lblx86_NotUseIB
  call c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Dalet_x86_trunk 333 1
  call c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Dalet_x86_trunk 333 2
  call c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Dalet_x86_trunk 333 3
  goto lblEndIf
  
:lblx64_UseIB
  rem call "C:\Program Files (x86)\Xoreax\IncrediBuild\ibconsole.exe" ^
  rem /command="c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Dalet_x64_trunk 333 1 && c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Dalet_x64_trunk 333 2 && c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Dalet_x64_trunk 333 3" ^
  rem /profile=c:\PVS-Config\IncrediBuild\check_solution_x64_incredibuild.xml
  call "C:\Program Files (x86)\Xoreax\IncrediBuild\ibconsole.exe" ^
  /command="c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Dalet_x64_trunk CmdVer" ^
  /profile=c:\PVS-Config\IncrediBuild\check_solution_x64_incredibuild.xml
  goto lblEndIf

:lblx64_NotUseIB
  call c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Dalet_x64_trunk 333 1
  call c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Dalet_x64_trunk 333 2
  call c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Dalet_x64_trunk 333 3
  goto lblEndIf
  
:lblx64_NotUseIB_CmdVer
  call c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Dalet_x64_trunk CmdVer
  if %ERRORLEVEL% NEQ 0 goto lblError
  goto lblEndIf
:lblEndIf

rem PlogCombiner
if "%PVS_CmdVer%" EQU "CmdVer" goto lblEndIf2
cd /d %PVS_Folder%
  if %PVS_Platform% EQU x86 goto lblx86
if %PVS_Platform% EQU x64 goto lblx64
  goto lblEndIf2
:lblx86
  call c:\PVS-Config\PVS-Studio\PlogCombiner.exe %PVS_Folder%\generated-x86-projects{0}.plog 3
  if %ERRORLEVEL% NEQ 0 goto lblError
  call c:\PVS-Config\PVS-Studio\PlogCombiner.exe %PVS_Folder%\generated-x86-projects{0}_WithSuppressedMessages.plog 3
  if %ERRORLEVEL% NEQ 0 goto lblError
  goto lblEndIf2
:lblx64
  call c:\PVS-Config\PVS-Studio\PlogCombiner.exe %PVS_Folder%\generated-x64-projects{0}.plog 3
  if %ERRORLEVEL% NEQ 0 goto lblError
  call c:\PVS-Config\PVS-Studio\PlogCombiner.exe %PVS_Folder%\generated-x64-projects{0}_WithSuppressedMessages.plog 3
  if %ERRORLEVEL% NEQ 0 goto lblError
	goto lblEndIf2
:lblEndIf2

:lblAllOk
popd
@endlocal
exit /b 0

:lblError
popd
@endlocal
exit /b 2