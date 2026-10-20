@echo off
cd /d "%~dp0"
echo ==========================================================
echo    RUNNING ALL TESTS: LOW-RES ASSETS AND PHYSICS FSM
echo ==========================================================

echo [1/2] Compiling and running Asset Validator...
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /out:validate_lowres_assets.exe /r:System.Drawing.dll validate_lowres_assets.cs
if %ERRORLEVEL% EQU 0 (
    validate_lowres_assets.exe
    del validate_lowres_assets.exe >nul 2>&1
) else (
    echo [ERROR] Failed to compile validate_lowres_assets.cs
)

echo.
echo [2/2] Compiling and running Physics / FSM Simulator...
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /out:test_physics_fsm.exe test_physics_fsm.cs
if %ERRORLEVEL% EQU 0 (
    test_physics_fsm.exe
    del test_physics_fsm.exe >nul 2>&1
) else (
    echo [ERROR] Failed to compile test_physics_fsm.cs
)

echo.
echo [COMPLETE] All tests finished.
if "%1" neq "--no-pause" (
    pause
)
