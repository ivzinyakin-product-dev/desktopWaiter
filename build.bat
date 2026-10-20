@echo off
cd /d "%~dp0"
echo Compiling DesktopPet.exe...
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /optimize /out:DesktopPet.exe /lib:C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF /r:PresentationCore.dll /r:PresentationFramework.dll /r:WindowsBase.dll /r:System.Xaml.dll /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll Program.cs
if %ERRORLEVEL% EQU 0 (
    echo [OK] Build successful! DesktopPet.exe is ready.
) else (
    echo [ERROR] Build failed.
)
if "%1" neq "--no-pause" (
    pause
)
