rem Usage: 2_PrepareWorkspace.bat <PVS_Platform> <PVS_IncrediBuild>
rem <PVS_Platform> - Dalet_x86_trunk, Dalet_x64_trunk
rem <PVS_IncrediBuild> - UseIB, NotUseIB
@echo on
@setlocal

set PVS_Platform=%1
set PVS_IncrediBuild=%2
set LastError=0

if %PVS_Platform% EQU Dalet_x86_trunk goto lblDalet_x86_trunk
if %PVS_Platform% EQU Dalet_x64_trunk goto lblDalet_x64_trunk
goto lblError
:lblDalet_x86_trunk
  cd /d c:\
  mkdir "C:\PVS Dalet logs x86 trunk"
  mkdir "C:\PVS Dalet logs x86 trunk\temp"
  robocopy "s:\src_suppress_x86_trunk" "s:\src" *.suppress /s /IS
  SlnSplitter.py s:\src\env\MasterBuild\generated-all-projects-with-dependencies.sln 3
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
  if %PVS_Platform% EQU Dalet_x86_trunk goto lblDalet_x86_trunk
  goto lblEndIf
:lblDalet_x64_trunk
  cd /d c:\
  mkdir "C:\PVS Dalet logs x64 trunk"
  mkdir "C:\PVS Dalet logs x64 trunk\temp"
  robocopy "s:\src_suppress_x64_trunk" "s:\src" *.suppress /s /IS
  SlnSplitter.py s:\src\env\MasterBuild\generated-x64-projects.sln 3
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
  goto lblEndIf
:lblEndIf

if %LastError% NEQ 0 goto lblError

:lblAllOk
@endlocal
exit /b 0

:lblError
@endlocal
exit /b 1

