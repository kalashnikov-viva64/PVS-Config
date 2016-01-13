rem Usage: ClearWorkspace.bat <PVS_Platform>
rem <PVS_Platform> - Dalet_x86_trunk, Dalet_x64_trunk
@echo on
@setlocal

set PVS_Platform=%1
set LastError=0

if %PVS_Platform% EQU Dalet_x86_trunk goto lblDalet_x86_trunk
if %PVS_Platform% EQU Dalet_x64_trunk goto lblDalet_x64_trunk
goto lblError
:lblDalet_x86_trunk
  rem Del Parts
  cd /d "C:\PVS Dalet logs x86 trunk\temp"
  del generated-x86-projects_?.plog
  del generated-x86-projects_?_WithSuppressedMessages.plog

  rem delete temp folder
  cd /d "C:\PVS Dalet logs x86 trunk\"
  del /Q temp
  goto lblEndIf
  
:lblDalet_x64_trunk
  goto lblEndIf
:lblEndIf

if %LastError% NEQ 0 goto lblError

@endlocal
exit /b 0

:lblError
@endlocal
exit /b 1

