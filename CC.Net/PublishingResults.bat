rem Usage: PublicationResults.bat <PVS_Platform> <PVS_Folder>
rem <PVS_Platform> - x86, x64
rem <PVS_PlogFile> - Full/New
rem <PVS_Folder> - folder
@echo on
@setlocal
pushd %~dp0
cd /d %~dp0
set PVS_Platform=%1
set PVS_PlogFile=%2
set PVS_Folder=%3
set LastError=0
set PlogFile=generated-x86-projects.plog
if %PVS_PlogFile% EQU Full set PlogFile=generated-x86-projects_WithSuppressedMessages.plog
echo %TIME%: Starting PublicationResults.bat

if %PVS_Platform% EQU x86 goto lblx86
if %PVS_Platform% EQU x64 goto lblx64
goto lblError
:lblx86
  rem PlogConverter
  cd /d %PVS_Folder%
  call C:\PVS-Config\PVS-Studio\PlogConverter.exe ^
    -p %PlogFile% -a GA:1,2 --header="[auto] PVS-Studio Analysis x86 Results" ^
	--server=212.143.237.10 --port=26 --fromAddress=buildmaster@dalet.com --autoEmail --sendEmail
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
  
  rem Update suppress bases
  @echo off
  robocopy "S:\src" "S:\src_suppress_x86_trunk" *.suppress /s /IS
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
  goto lblEndIf

:lblx64
  rem PlogConverter
  cd /d %PVS_Folder%
  call C:\PVS-Config\PVS-Studio\PlogConverter.exe ^
    -p %PlogFile% -a 64:1 --header="[auto] PVS-Studio Analysis x64 Results" ^
	--server=212.143.237.10 --port=26 --fromAddress=buildmaster@dalet.com --autoEmail --sendEmail
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
  
  rem Update suppress bases
  @echo off
  robocopy "S:\src" "S:\src_suppress_x64_trunk" *.suppress /s /IS
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
  goto lblEndIf
:lblEndIf
  if %LastError% NEQ 0 goto lblError

:lblAllOk
popd
@endlocal
exit /b 0

:lblError
popd
@endlocal
exit /b 2