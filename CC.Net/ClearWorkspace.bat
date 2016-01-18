rem Usage: ClearWorkspace.bat <PVS_Platform>
rem <PVS_Platform> - x86, x64
@echo on
@setlocal
pushd %~dp0
cd /d %~dp0
set PVS_Platform=%1
echo %TIME%: Starting 5_ClearWorkspace.bat

if %PVS_Platform% EQU x86 goto lblx86
if %PVS_Platform% EQU x64 goto lblx64
goto lblError
:lblx86
  del s:\src\env\MasterBuild\generated-all-projects-with-dependencies_?.sln
  rem del "C:\PVS Dalet logs x86 trunk\temp\generated-x86-projects_?.plog"
  rem del "C:\PVS Dalet logs x86 trunk\temp\generated-x86-projects_?_WithSuppressedMessages.plog"
  del /Q "C:\PVS Dalet logs x86 trunk\temp"
  goto lblEndIf
:lblx64
  del s:\src\env\MasterBuild\generated-x64-projects_?.sln
  rem del "C:\PVS Dalet logs x64 trunk\temp\generated-x64-projects_?.plog"
  rem del "C:\PVS Dalet logs x64 trunk\temp\generated-x64-projects_?_WithSuppressedMessages.plog"
  del /Q "C:\PVS Dalet logs x64 trunk\temp"
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