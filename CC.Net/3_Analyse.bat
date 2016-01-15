rem Usage: 3_Analyse.bat <PVS_Platform> <PVS_IncrediBuild>
rem <PVS_Platform> - x86, x64
rem <PVS_IncrediBuild> - UseIB, NotUseIB
@echo on
@setlocal
pushd %~dp0
cd /d %~dp0
set PVS_Platform=%1
set PVS_IncrediBuild=%2
echo %TIME%: Starting 3_Analyse.bat

if %PVS_Platform% EQU x86 if %PVS_IncrediBuild% EQU UseIB goto lblx86_UseIB
if %PVS_Platform% EQU x86 if %PVS_IncrediBuild% EQU NotUseIB goto lblx86_NotUseIB
if %PVS_Platform% EQU x64 if %PVS_IncrediBuild% EQU UseIB goto lblx64_UseIB
if %PVS_Platform% EQU x64 if %PVS_IncrediBuild% EQU NotUseIB goto lblx64_NotUseIB
goto lblError
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

:lblAllOk
popd
@endlocal
exit /b 0

:lblError
popd
@endlocal
exit /b 2