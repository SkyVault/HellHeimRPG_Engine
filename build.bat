@echo off

setlocal enabledelaydexpansion

for /f "usebackq tokens=*" %%i in (`vswhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do (
    "%%i" %*
    exit /b !errorlevel!
)