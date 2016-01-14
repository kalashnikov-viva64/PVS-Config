rem Usage: 2_PrepareWorkspace.bat <PVS_Platform> <PVS_IncrediBuild>
rem <PVS_Platform> - x86, x64
rem <PVS_IncrediBuild> - UseIB, NotUseIB
@echo on
@setlocal
pushd %~dp0
cd /d %~dp0
set PVS_Platform=%1
set PVS_IncrediBuild=%2

if %PVS_Platform% EQU x86 goto lblx86
if %PVS_Platform% EQU x64 goto lblx64
goto lblError
:lblx86
  mkdir "C:\PVS Dalet logs x86 trunk"
  mkdir "C:\PVS Dalet logs x86 trunk\temp"
  mkdir "S:\src_suppress_x86_trunk"
  robocopy "s:\src_suppress_x86_trunk" "s:\src" *.suppress /s /IS
  call c:\PVS-Config\PVS-Studio\GenerateSln.bat x86
  if %ERRORLEVEL% NEQ 0 goto lblError
  call c:\PVS-Config\PVS-Studio\SlnSplitter.py ^
    s:\src\env\MasterBuild\generated-all-projects-with-dependencies.sln 3
  if %ERRORLEVEL% NEQ 0 goto lblError
  if %PVS_IncrediBuild% EQU NotUseIB copy ^
    /Y c:\PVS-Config\PVS-Studio\Settings-x86.xml %appdata%\PVS-Studio\Settings.xml
  if %PVS_IncrediBuild% EQU UseIB copy ^
    /Y c:\PVS-Config\IncrediBuild\Old-Dalet-Settings-x86.xml %appdata%\PVS-Studio\Settings.xml
  goto lblEndIf
:lblx64
  mkdir "C:\PVS Dalet logs x64 trunk"
  mkdir "C:\PVS Dalet logs x64 trunk\temp"
  mkdir "S:\src_suppress_x64_trunk"
  robocopy "s:\src_suppress_x64_trunk" "s:\src" *.suppress /s /IS
  call c:\PVS-Config\PVS-Studio\GenerateSln.bat x64
  if %ERRORLEVEL% NEQ 0 goto lblError
  call c:\PVS-Config\PVS-Studio\SlnSplitter.py ^
    s:\src\env\MasterBuild\generated-x64-projects.sln 3
  if %ERRORLEVEL% NEQ 0 goto lblError
  if %PVS_IncrediBuild% EQU NotUseIB copy ^
    /Y c:\PVS-Config\PVS-Studio\Settings-x64.xml %appdata%\PVS-Studio\Settings.xml
  if %PVS_IncrediBuild% EQU UseIB copy ^
    /Y c:\PVS-Config\IncrediBuild\Old-Dalet-Settings.xml %appdata%\PVS-Studio\Settings.xml
  goto lblEndIf
:lblEndIf

:lblAllOk
popd
@endlocal
exit /b 0

:lblError
popd
@endlocal
exit /b 2