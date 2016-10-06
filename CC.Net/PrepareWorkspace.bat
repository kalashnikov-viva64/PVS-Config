rem Usage: PrepareWorkspace.bat <PVS_Platform> <PVS_IncrediBuild> <PVS_Team> <PVS_CmdVer>
rem <PVS_Platform> - x64, Amberfin_x86
rem <PVS_IncrediBuild> - UseIB, NotUseIB
rem <PVS_Team> - Dalet, Viva
rem <PVS_CmdVer> - "", CmdVer
@echo on
@setlocal
pushd %~dp0
cd /d %~dp0
set PVS_Platform=%1
set PVS_IncrediBuild=%2
set PVS_Team=%3
set PVS_CmdVer=%4
echo %TIME%: Starting PrepareWorkspace.bat

rem Install last build of PVS-Studio
if %PVS_Team% NEQ Dalet goto lblEndUpdater
  cd /d "C:\PVS Dalet logs x64 trunk\temp\"
  copy /Y "c:\Program Files (x86)\PVS-Studio\PVS-Studio-Updater.exe" PVS-Studio-Updater.exe
  call PVS-Studio-Updater.exe /VERYSILENT /SUPPRESSMSGBOXES
  del PVS-Studio-Updater.exe
:lblEndUpdater
rem goto lblWithoutUpdate
if %PVS_Team% EQU Viva call \\Viva64-build\Builder\PVS-Studio_setup.exe ^
  /VERYSILENT /SUPPRESSMSGBOXES /COMPONENTS=Core,Standalone,MSVS,MSVS\2010,MSVS\2012
if %ERRORLEVEL% NEQ 0 goto lblError
:lblWithoutUpdate

if %PVS_Platform% EQU Amberfin_x86 goto lblAmberfin_x86
if %PVS_Platform% EQU x64 goto lblx64
goto lblError
:lblAmberfin_x86
  rem Props
  cd /d %appdata%\..\Local\Microsoft\MSBuild\v4.0\
  copy /Y c:\PVS-Config\PVS-Studio\Amberfin-Microsoft.Cpp.x64.user.props Microsoft.Cpp.x64.user.props
  copy /Y c:\PVS-Config\PVS-Studio\Amberfin-Microsoft.Cpp.Win32.user.props Microsoft.Cpp.Win32.user.props
  goto lblAllOk

  rem Compile IDL, MC, TLH
  cd /d s:\src\env\MasterBuild\
  call set-daletplus-dev-path-x64.bat 
  call SetMsdevEnvx64.bat 
  call BuildNetProjectsx64.bat
  devenv AutoGenSources.sln /rebuild "Release|x64" /useenv
  
:lblx64
  rem Props
  cd /d %appdata%\..\Local\Microsoft\MSBuild\v4.0\
  copy /Y c:\PVS-Config\PVS-Studio\Dalet-Microsoft.Cpp.x64.user.props Microsoft.Cpp.x64.user.props
  copy /Y c:\PVS-Config\PVS-Studio\Dalet-Microsoft.Cpp.Win32.user.props Microsoft.Cpp.Win32.user.props

  rem Settings
  if %PVS_IncrediBuild% EQU NotUseIB copy ^
    /Y c:\PVS-Config\PVS-Studio\Settings-x64.xml %appdata%\PVS-Studio\Settings.xml
  if %PVS_IncrediBuild% EQU UseIB copy ^
    /Y c:\PVS-Config\IncrediBuild\Old-Dalet-Settings.xml %appdata%\PVS-Studio\Settings.xml

  mkdir "C:\PVS Dalet logs x64 trunk"
  mkdir "C:\PVS Dalet logs x64 trunk\temp"
  if "%PVS_CmdVer%" EQU "CmdVer" goto lblCmdVer
  mkdir "S:\src_suppress_x64_trunk"
  robocopy "s:\src_suppress_x64_trunk" "s:\src" *.suppress /s /IS
	goto lblEndIf3
:lblCmdVer
    mkdir "S:\src_suppress_x64_trunk_CmdVer"
	robocopy "s:\src_suppress_x64_trunk_CmdVer" "s:\src" *.suppress /s /IS
:lblEndIf3
  
  call c:\PVS-Config\PVS-Studio\GenerateSln.bat x64
  if %ERRORLEVEL% NEQ 0 goto lblError
  call c:\PVS-Config\PVS-Studio\SlnSplitter.py ^
    s:\src\env\MasterBuild\generated-x64-projects.sln 3
  if %ERRORLEVEL% NEQ 0 goto lblError
  
  rem Compile IDL, MC, TLH
  cd /d s:\src\env\MasterBuild\
  call set-daletplus-dev-path-x64.bat 
  call SetMsdevEnvx64.bat 
  call BuildNetProjectsx64.bat
  devenv AutoGenSources.sln /rebuild "Release|x64" /useenv
  
:lblAllOk
popd
@endlocal
exit /b 0

:lblError
popd
@endlocal
exit /b 2