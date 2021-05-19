@echo off
SET mypath="%~dp0"
cmd.exe /c start "RunDS4Windows" %mypath%\DS4Windows.exe -m
exit
