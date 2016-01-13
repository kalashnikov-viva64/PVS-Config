rem Usage: 1_SvnUpdate.bat
@echo off
@setlocal
pushd %~dp0
cd /d %~dp0
set PVS_Root = %~dp0\..\
set LastError=0

rem Очистка репозиториев, удаление не версифициоранных файлов, обновление.
pushd S:\src
cd /d S:\src
svn cleanup
rem svn revert -R .
rem TortoiseProc.exe /command:cleanup /revert /delunversioned /delignored /refreshshell /path:"S:\src" /noui /closeonend:1
svn update
if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
popd
pushd S:\external
cd /d S:\external
svn cleanup
rem svn revert -R .
rem TortoiseProc.exe /command:cleanup /revert /delunversioned /delignored /refreshshell /path:"S:\external" /noui /closeonend:1
svn update
if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
popd
if %LastError% NEQ 0 goto lblError

rem Генерация солюшена
call PVS_Root\PVS-Studio\GenerateSln.bat

if %LastError% NEQ 0 goto lblError

:lblAllOk
popd
@endlocal
exit /b 0

:lblError
popd
@endlocal
exit /b 1

