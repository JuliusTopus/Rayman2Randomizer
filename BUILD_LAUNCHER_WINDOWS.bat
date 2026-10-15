@echo off
setlocal
cd /d "%~dp0"
set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" (
 echo ERROR: .NET Framework 4.x C# compiler not found.
 pause
 exit /b 1
)
if not exist "Rayman2Randomizer.dll" (
 echo ERROR: Rayman2Randomizer.dll was not found in this folder.
 echo Build the native C++ DLL in Release Win32 and copy it here.
 pause
 exit /b 1
)
if not exist "build" mkdir "build"
"%CSC%" /nologo /target:winexe /optimize+ /platform:anycpu /out:"build\Rayman2Randomizer_Beta0.1.5_Public_Rebuilt.exe" /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll /resource:"Rayman2Randomizer.dll",R2R_EMBEDDED_MOD_DLL "src\Rayman2Randomizer_Launcher.cs"
if errorlevel 1 (
 echo ERROR: Launcher build failed.
) else (
 echo OK: Launcher created in build\ (not yet gameplay-validated).
)
pause
