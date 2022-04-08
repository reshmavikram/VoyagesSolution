@echo off
::Determine which version of Windows is installed
::Set Variable Version to unknown

SET Version=Unknown

VER | FINDSTR /IL "5.0" > NUL
IF %ERRORLEVEL% EQU 0 SET Version="Windows 2000"

VER | FINDSTR /IL "5.1." > NUL
IF %ERRORLEVEL% EQU 0 SET Version="Windows XP"

VER | FINDSTR /IL "5.2." > NUL
IF %ERRORLEVEL% EQU 0 SET Version="Windows 2003"

VER | FINDSTR /IL "6.0." > NUL
IF %ERRORLEVEL% EQU 0 SET Version="Windows Vista"

VER | FINDSTR /IL "6.1." > NUL
IF %ERRORLEVEL% EQU 0 SET Version="Windows 7"

VER | FINDSTR /IL "6.2." > NUL
IF %ERRORLEVEL% EQU 0 SET Version="Windows 8"

ECHO The version of Windows found is %VERSION%

IF %VERSION% == "Windows XP" GOTO XP
IF %VERSION% == "Windows 2000" GOTO XP
IF %VERSION% == "Windows 2003" GOTO XP
IF %VERSION% == "Windows Vista" GOTO WIN7
IF %VERSION% == "Windows 7" GOTO WIN7
IF %VERSION% == "Windows 8" GOTO WIN7

::If no versions are found go to UNKNOWN
GOTO UNKNOWN

:WIN7
ECHO WIN7 script to execute
cd /D C:\users
ECHO Clean Temp Folder
for /D %%a in (*.*) do DEL /F /S /Q "%%a\AppData\Local\Temp\*.*"
for /D %%a in (*.*) do FOR /D %%b IN ("%%a\AppData\Local\Temp\*.*") DO RMDIR /S /Q "%%b"
ECHO Clean IE Cache
for /D %%a in (*.*) do DEL /F /S /Q "%%a\AppData\Local\Microsoft\Windows\Temporary Internet Files\*.*"
for /D %%a in (*.*) do FOR /D %%b IN ("%%a\AppData\Local\Microsoft\Windows\Temporary Internet Files\*.*") DO RMDIR /S /Q "%%b"
ECHO Clean JAVA Cache
for /D %%a in (*.*) do DEL /F /S /Q "%%a\AppData\LocalLow\Sun\Java\Deployment\cache\*.*"
for /D %%a in (*.*) do FOR /D %%b IN ("%%a\AppData\LocalLow\Sun\Java\Deployment\cache\*.*") DO RMDIR /S /Q "%%b"

GOTO FINISH

:XP
ECHO XP script to execute
cd /D "C:\Documents and Settings"
ECHO Clean Temp Folder
for /D %%a in (*.*) do DEL /F /S /Q "%%a\Local Settings\Temp\*.*"
for /D %%a in (*.*) do FOR /D %%b IN ("%%a\Local Settings\Temp\*.*") DO RMDIR /S /Q "%%b"
ECHO Clean IE Cache
for /D %%a in (*.*) do DEL /F /S /Q "%%a\Local Settings\Temporary Internet Files\*.*"
for /D %%a in (*.*) do FOR /D %%b IN ("%%a\Local Settings\Temporary Internet Files\*.*") DO RMDIR /S /Q "%%b"
ECHO Clean JAVA Cache
for /D %%a in (*.*) do DEL /F /S /Q "%%a\Application Data\Sun\Java\Deployment\cache\*.*"
for /D %%a in (*.*) do FOR /D %%b IN ("%%a\Application Data\Sun\Java\Deployment\cache\*.*") DO RMDIR /S /Q "%%b"

GOTO FINISH

:FINISH
ECHO Temporary files successfully deleted
GOTO END

:UNKNOWN
ECHO Operating System Unknown

:END