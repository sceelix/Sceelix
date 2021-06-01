@ECHO OFF
SETLOCAL

:: Build content with MonoGame Content Builder tool.
"..\Libs\DigitalRune Mono\Windows\MGCB.exe" /incremental /@:Windows.mgcb || GOTO error

:: ZIP content.
"..\Libs\DigitalRune Mono\Tools\Pack.exe" --output "Content.zip" --recursive --directory bin\Windows\Content *.* || GOTO error

cd ..

ECHO.
ECHO SUCCESS - Content build successful.
Rem PAUSE
EXIT

:error
ECHO.
ECHO ERROR - Content build failed.
Rem PAUSE
EXIT /B 1
