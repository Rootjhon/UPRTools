@ECHO off

SET CUR_PATH="%~dp0"

set /p prjPath="Unity project path:"

call :relink_dir %CUR_PATH%..\UPRTools %prjPath%\UPRTools

:: -------------------------------------- Def Function -------------------------------------------

:: %1=srcPath; %2=dstPath
:relink_dir
if exist %2 (
    rd /s/q %2
)
mklink /d %2 %1
goto EOF

:: -------------------------------------- End Function -------------------------------------------

:EOF