cd /d "%~dp0"

xcopy /y /exclude:setup.copy.exclude.txt .\bin\Debug\* .\..\dist\

pause