call S:\src\env\MasterBuild\set-daletplus-dev-path.bat
call S:\src\env\MasterBuild\SetMsdevEnv.bat
devenv.exe /useenv "S:\src\env\MasterBuild\generated-all-projects-with-dependencies.sln" /command "PVSStudio.CheckSolution Win32|Release|C:\PVS Dalet logs\temp\generated-x86-projects.plog|||SuppressAll"