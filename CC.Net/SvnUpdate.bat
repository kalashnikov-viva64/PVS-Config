rem Usage: SvnUpdate.bat
@echo on
@setlocal
pushd %~dp0
cd /d %~dp0
echo %TIME%: Starting 1_SvnUpdate.bat

rem Cleaning repositories, deletion unversioned files, updating.
pushd S:\src
cd /d S:\src
svn cleanup
svn revert -R .
TortoiseProc.exe /command:cleanup /revert /delunversioned ^
  /delignored /refreshshell /path:"S:\src" /noui /closeonend:1
svn update
if %ERRORLEVEL% NEQ 0 (popd && goto lblError)
popd
pushd S:\external
cd /d S:\external
svn cleanup
svn revert -R .
TortoiseProc.exe /command:cleanup /revert /delunversioned ^
  /delignored /refreshshell /path:"S:\external" /noui /closeonend:1
svn update
if %ERRORLEVEL% NEQ 0 (popd && goto lblError)
popd

:lblAllOk
popd
@endlocal
exit /b 0

:lblError
popd
@endlocal
exit /b 2