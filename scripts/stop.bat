@echo off
REM SCADA System Shutdown Script for Windows

echo Stopping Enterprise SCADA System...

docker-compose down

echo.
echo All services stopped
echo.
echo To start again: scripts\start.bat
echo To remove all data: docker-compose down -v
echo.
pause
