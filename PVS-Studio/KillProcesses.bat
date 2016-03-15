@echo on

TASKLIST /FI "IMAGENAME eq devenv.exe" | FIND "devenv.exe" > NUL
if %ERRORLEVEL% NEQ 0 goto lblNotFoundDevEnv
  taskkill /f /im devenv.exe
:lblNotFoundDevEnv

TASKLIST /FI "IMAGENAME eq Standalone.exe" | FIND "Standalone.exe" > NUL
if %ERRORLEVEL% NEQ 0 goto lblNotFoundStandalone
  taskkill /f /im Standalone.exe
:lblNotFoundStandalone

exit /b 0