@echo off
REM ================================================
REM Frontend Dependencies Installation Script
REM ================================================
REM Purpose: Install required NPM packages for Energy Management UI
REM Date: 2025-01-08
REM ================================================

echo ============================================
echo Installing Frontend Dependencies
echo ============================================
echo.

cd frontend\scada-dashboard

echo [1/3] In installing core React libraries...
call npm install react react-dom

echo.
echo [2/3] Installing TypeScript types...
call npm install --save-dev @types/react @types/react-dom

echo.
echo [3/3] Installing Nivo chart libraries...
call npm install @nivo/core @nivo/sankey @nivo/pie @nivo/line

echo.
echo ============================================
echo Installation Complete!
echo ============================================
echo.
echo Installed packages:
echo - react, react-dom
echo - @types/react, @types/react-dom
echo - @nivo/core, @nivo/sankey, @nivo/pie, @nivo/line
echo.
echo You can now build and run the frontend.
echo.

pause
