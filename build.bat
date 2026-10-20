@echo off
setlocal EnableExtensions
cd /d "%~dp0"

set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"

if not exist "%CSC%" (
    echo.
    echo [ERROR] Cannot find the .NET Framework C# compiler ^(csc.exe^).
    echo Searched:
    echo   %WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
    echo   %WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
    echo.
    echo On Windows, csc.exe ships with the .NET Framework 4.x.
    echo Install/enable .NET Framework 4.8 from Windows Update, then run build.bat again.
    echo You can also download a prebuilt release from GitHub Releases.
    echo.
    exit /b 1
)

echo Using compiler: %CSC%
echo Building WinAdapterSwitcher.exe ...

"%CSC%" /nologo /target:winexe /out:WinAdapterSwitcher.exe /win32manifest:app.manifest /win32icon:app.ico ^
    /reference:System.dll ^
    /reference:System.Drawing.dll ^
    /reference:System.Windows.Forms.dll ^
    /reference:System.Core.dll ^
    /reference:System.Management.dll ^
    /reference:System.Xml.dll ^
    /langversion:5 /codepage:65001 /optimize+ /debug- ^
    *.cs

if errorlevel 1 (
    echo.
    echo [ERROR] Build failed.
    exit /b 1
)

echo.
echo Build OK: %CD%\WinAdapterSwitcher.exe
for %%F in (WinAdapterSwitcher.exe) do echo Size: %%~zF bytes
exit /b 0
