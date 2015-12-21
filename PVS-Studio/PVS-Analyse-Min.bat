rem Разделение на 10 частей, чтоб прошел за 36 минут
call c:\configuring\PVS-Studio\SlnSplitter.py s:\src\env\MasterBuild\generated-all-projects-with-dependencies.sln 10

set PlogFile=C:\PVS Dalet logs x86 trunk\temp\generated-x86-projects_1.plog
set Project=generated-all-projects-with-dependencies_1.sln
set PluginExec="PVSStudio.CheckSolution Win32|Release|%PlogFile%|||SuppressAll"
set PVS_MSVC=devenv.exe
rem set PVS_MSVC=C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\devenv.com

cd /d S:\src\env\MasterBuild
call set-daletplus-dev-path.bat
call SetMsdevEnv.bat

rem start devenv.exe /useenv %Project% /command %PluginExec%
call "%PVS_MSVC%" %Project% /command %PluginExec%
