rem Usage: Analyse.bat <PVS_Platform> <PVS_IncrediBuild> <PVS_Folder>
rem <PVS_Platform> - x86, x64
rem <PVS_IncrediBuild> - UseIB, NotUseIB
rem <PVS_Folder> - folder
@echo on
@setlocal
pushd %~dp0
cd /d %~dp0
set PVS_Platform=%1
set PVS_IncrediBuild=%2
set PVS_Folder=%3
echo %TIME%: Starting Analyse.bat

if %PVS_Platform% EQU Amberfin_x64 goto lblAmberfin_x64
if %PVS_Platform% EQU x86 if %PVS_IncrediBuild% EQU UseIB goto lblx86_UseIB
if %PVS_Platform% EQU x86 if %PVS_IncrediBuild% EQU NotUseIB goto lblx86_NotUseIB
if %PVS_Platform% EQU x64 if %PVS_IncrediBuild% EQU UseIB goto lblx64_UseIB
if %PVS_Platform% EQU x64 if %PVS_IncrediBuild% EQU NotUseIB goto lblx64_NotUseIB
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
  call "C:\Program Files (x86)\Xoreax\IncrediBuild\ibconsole.exe" ^
    /command="c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Dalet_x64_trunk 333 1 && c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Dalet_x64_trunk 333 2 && c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Dalet_x64_trunk 333 3" ^
	/profile=c:\PVS-Config\IncrediBuild\check_solution_x64_incredibuild.xml
  goto lblEndIf

:lblx64_NotUseIB
  call c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Dalet_x64_trunk 333 1
  call c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Dalet_x64_trunk 333 2
  call c:\PVS-Config\PVS-Studio\PVS-Analyse.bat Dalet_x64_trunk 333 3
  goto lblEndIf
:lblEndIf

rem PlogCombiner
cd /d %PVS_Folder%
if %PVS_Platform% EQU x64 goto lblx64
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
:lblEndIf2

:lblAllOk
popd
@endlocal
exit /b 0

:lblError
popd
@endlocal
exit /b 2