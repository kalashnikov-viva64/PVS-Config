rem Usage: PublishingResults.bat <PVS_Platform> <PVS_Folder> <PVS_Team>
rem <PVS_Platform> - x64, Amberfin_x86
rem <PVS_Folder> - folder
rem <PVS_Team> - Dalet, Viva
@echo on
@setlocal
pushd %~dp0
cd /d %~dp0
set PVS_Platform=%1
set PVS_Folder=%2
set PVS_Team=%3
set LastError=0
echo %TIME%: Starting PublishingResults.bat

rem Day of week
for /F "tokens=1-4 delims=/ " %%i in ('date /t') do (
set WeekDay=%%i
set D=%%j
set M=%%k
set Y=%%l
) 

if %PVS_Platform% EQU Amberfin_x86 goto lblAmberfin_x86
if %PVS_Team% EQU Dalet if %PVS_Platform% EQU x64 goto lblDaletx64
if %PVS_Team% EQU Viva if %PVS_Platform% EQU x64 goto lblVivax64
goto lblError
:lblAmberfin_x86
  set PlogFile=PVS_Amberfin_x86_WithSuppressedMessages.plog
  cd /d %PVS_Folder%
  call C:\PVS-Config\PVS-Studio\PlogConverter.exe ^
    -p %PlogFile% -a GA:1,2;64:1 --emailList="Emails_Amberfin.lst" --header="[auto] PVS-Studio Amberfin x86 Results" ^
	--server=212.143.237.10 --port=26 --fromAddress=buildmaster@dalet.com --autoEmail
	rem --sendEmail
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%  
  goto lblEndIf
  
:lblDaletx64
  set PlogFile=generated-x64-projects.plog
  if %WeekDay% EQU Sun set PlogFile=generated-x64-projects_WithSuppressedMessages.plog
  
  rem PlogConverter
  cd /d %PVS_Folder%
  call C:\PVS-Config\PVS-Studio\PlogConverter.exe ^
    -p %PlogFile% -a GA:1,2;64:1 --emailList="Emails.lst" --header="[auto] PVS-Studio Analysis x64 Results" ^
	--server=212.143.237.10 --port=26 --fromAddress=buildmaster@dalet.com --autoEmail --sendEmail
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
  
  rem Update suppress bases
  robocopy "S:\src" "S:\src_suppress_x64_trunk" *.suppress /s /IS
  goto lblEndIf
  
:lblVivax64
  set PlogFile=generated-x64-projects.plog

  rem PlogConverter
  cd /d %PVS_Folder%
  call "c:\Program Files (x86)\PVS-Studio\PlogConverter.exe" -a GA:1,2;64:1 %PlogFile%
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
  call c:\sendemail\sendemail.exe ^
	-f builder@viva64.com ^
	-t kalashnikov@viva64.com ^
	-s smtp-9.1gb.ru ^
	-o message-content-type=html message-file=generated-x64-projects.plog.html ^
	-u [auto] PVS-Studio Analysis x64 Results ^
	-o username=u410986 ^
	-o password=213327374ui 
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
  
  rem Update suppress bases
  robocopy "S:\src" "S:\src_suppress_x64_trunk" *.suppress /s /IS
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
