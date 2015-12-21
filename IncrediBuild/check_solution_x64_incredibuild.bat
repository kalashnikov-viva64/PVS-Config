call S:\src\env\MasterBuild\set-daletplus-dev-path-x64.bat
call S:\src\env\MasterBuild\SetMsdevEnvx64.bat
devenv.exe /useenv "S:\src\env\MasterBuild\generated-x64-projects.sln" /command "PVSStudio.CheckSolution x64|Release|C:\PVS Dalet logs\temp\generated-x64-projects.plog|||SuppressAll"