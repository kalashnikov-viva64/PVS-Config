rem Usage: GenerateSln.bat <Platform>
rem <Platform> - x86, x64
@echo off
@setlocal
pushd "%~dp0"
cd /d "%~dp0"

set Platform=%1%
set build_state=Full
set build_mode=Release
set build_version=4.0
set build_number=00000
set with_symbols=
set build_batch=generated_build_%build_version%-%build_number%-%build_mode%.bat

cd /d "s:\src\env\MasterBuild\"
call set-daletplus-dev-path.bat
  
rem Platform selection
if %Platform% EQU x86 goto lblx86
if %Platform% EQU x64 goto lblx64
goto lblError
:lblx86
  call tclsh.exe generate_compilation_batch.tcl "s:\src" %build_state% %build_mode% Build ProjectList.txt "%build_batch%" DaletVersion.rc.tmpl "%build_version%" "%build_number%" "%with_symbols%"
  if %ERRORLEVEL% NEQ 0 goto lblError
  goto lblEndIf

:lblx64
  call tclsh.exe generate_compilation_batch_x64.tcl "s:\src" %build_state% %build_mode% Build ProjectList-x64.txt "%build_batch%" DaletVersion.rc.tmpl "%build_version%" "%build_number%" "%with_symbols%"
  if %ERRORLEVEL% NEQ 0 goto lblError
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
