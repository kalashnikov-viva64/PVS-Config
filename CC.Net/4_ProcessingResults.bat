rem Usage: 4_ProcessingResults.bat <PVS_Platform>
rem <PVS_Platform> - x86, x64
@echo on
@setlocal
pushd %~dp0
cd /d %~dp0
set PVS_Platform=%1
set LastError=0

if %PVS_Platform% EQU x86 goto lblx86
if %PVS_Platform% EQU x64 goto lblx64
goto lblError
:lblx86
  rem PlogCombiner
  cd /d "C:\PVS Dalet logs x86 trunk\temp"
  call c:\PVS-Config\PVS-Studio\PlogCombiner.exe "C:\PVS Dalet logs x86 trunk\temp\generated-x86-projects{0}.plog" 3
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
  call c:\PVS-Config\PVS-Studio\PlogCombiner.exe "C:\PVS Dalet logs x86 trunk\temp\generated-x86-projects{0}_WithSuppressedMessages.plog" 3
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%

  rem PlogConverter
  cd /d "C:\PVS Dalet logs x86 trunk\temp"
  call C:\PVS-Config\Utils\PlogConverter\Pvs.PlogConverter\bin\Debug\PlogConverter.exe ^
    -p generated-x86-projects.plog -a GA:1,2 --header="[auto] PVS-Studio Analysis x86 Results" ^
	--server=212.143.237.10 --port=26 --fromAddress=buildmaster@dalet.com --autoEmail --sendEmail
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
  
  rem Update suppress bases
  robocopy "S:\src" "S:\src_suppress_x86_trunk" *.suppress /s /IS
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
  goto lblEndIf

:lblx64
  rem PlogCombiner
  cd /d "C:\PVS Dalet logs x64 trunk\temp"
  call c:\PVS-Config\PVS-Studio\PlogCombiner.exe "C:\PVS Dalet logs x64 trunk\temp\generated-x64-projects{0}.plog" 3
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
  call c:\PVS-Config\PVS-Studio\PlogCombiner.exe "C:\PVS Dalet logs x64 trunk\temp\generated-x64-projects{0}_WithSuppressedMessages.plog" 3
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
  
  rem PlogConverter
  cd /d "C:\PVS Dalet logs x64 trunk\temp"
  call C:\PVS-Config\Utils\PlogConverter\Pvs.PlogConverter\bin\Debug\PlogConverter.exe ^
    -p generated-x64-projects.plog -a 64:1 --header="[auto] PVS-Studio Analysis x64 Results" ^
	--server=212.143.237.10 --port=26 --fromAddress=buildmaster@dalet.com --autoEmail --sendEmail
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
  
  rem Update suppress bases
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