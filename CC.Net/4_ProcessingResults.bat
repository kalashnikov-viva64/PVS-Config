rem Usage: ProcessingResults.bat <PVS_Platform>
rem <PVS_Platform> - Dalet_x86_trunk, Dalet_x64_trunk
@echo on
@setlocal

set PVS_Platform=%1
set LastError=0

if %PVS_Platform% EQU Dalet_x86_trunk goto lblDalet_x86_trunk
if %PVS_Platform% EQU Dalet_x64_trunk goto lblDalet_x64_trunk
goto lblError
:lblDalet_x86_trunk
  
  rem PlogCombiner
  cd /d "C:\PVS Dalet logs x86 trunk\temp"
  call c:\PVS-Config\PVS-Studio\PlogCombiner.exe "C:\PVS Dalet logs x86 trunk\temp\generated-x86-projects{0}.plog" 3
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
  call c:\PVS-Config\PVS-Studio\PlogCombiner.exe "C:\PVS Dalet logs x86 trunk\temp\generated-x86-projects{0}_WithSuppressedMessages.plog" 3
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
  
  rem PlogConverter
  cd /d "C:\PVS Dalet logs x86 trunk\temp"
  call C:\PVS-Config\Utils\PlogConverter\Pvs.PlogConverter\bin\Debug\PlogConverter.exe ^
    -p generated-x86-projects.plog -a GA:1,2;64:1 --header="[auto] PVS-Studio Analysis x86 Results" ^
    --server=212.143.237.10 --port=26 --fromAddress=buildmaster@dalet.com
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
  
  rem Update suppress bases
  cd /d "S:\"
  robocopy ".\src" ".\src_suppress_x86_trunk" *.suppress /s /IS
  if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
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

