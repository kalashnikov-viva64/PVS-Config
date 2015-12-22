@echo on
@setlocal
set LastError=0

cd /d S:\src
pushd S:\src
svn cleanup S:\src
rem svn revert -R .
rem TortoiseProc.exe /command:cleanup /revert /delunversioned /delignored /refreshshell /path:"S:\src" /noui /closeonend:1
svn update
if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
popd

cd /d S:\external
pushd S:\external
svn cleanup S:\external
rem svn revert -R .
rem TortoiseProc.exe /command:cleanup /revert /delunversioned /delignored /refreshshell /path:"S:\external" /noui /closeonend:1
svn update
if %ERRORLEVEL% NEQ 0 set LastError=%ERRORLEVEL%
popd

rem todo: Удаление не версифициоранных файлов. Генерация солюшена. 

if %LastError% NEQ 0 goto lblError

:lblAllOk
@endlocal
exit /b 0

:lblError
@endlocal
exit /b 1

