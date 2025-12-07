@echo off
REM Database Restore Script for Windows
REM Usage: restore.bat <backup-file.tar.gz>

if "%1"=="" (
    echo Error: Please provide backup file path
    echo Usage: restore.bat ^<backup-file.tar.gz^>
    exit /b 1
)

set BACKUP_FILE=%1

if not exist "%BACKUP_FILE%" (
    echo Error: Backup file not found: %BACKUP_FILE%
    exit /b 1
)

echo ========================================
echo SCADA System Restore Script
echo ========================================
echo.
echo WARNING: This will replace all existing data!
set /p confirm="Are you sure you want to continue? (yes/no): "

if not "%confirm%"=="yes" (
    echo Restore cancelled.
    exit /b 0
)

set RESTORE_DIR=restore_temp
mkdir "%RESTORE_DIR%" 2>nul

echo.
echo [1/4] Extracting backup...
tar -xzf "%BACKUP_FILE%" -C "%RESTORE_DIR%"
echo Extraction completed

echo.
echo [2/4] Restoring PostgreSQL database...
docker exec -i scada-postgres psql -U scada -d postgres -c "DROP DATABASE IF EXISTS scada;"
docker exec -i scada-postgres psql -U scada -d postgres -c "CREATE DATABASE scada;"
type "%RESTORE_DIR%\postgres_backup.sql" | docker exec -i scada-postgres psql -U scada -d scada
echo PostgreSQL restore completed

echo.
echo [3/4] Restoring InfluxDB database...
docker cp "%RESTORE_DIR%\influxdb-backup" scada-influxdb:/tmp/
docker exec scada-influxdb influx restore /tmp/influxdb-backup -t scada-token-change-in-production
echo InfluxDB restore completed

echo.
echo [4/4] Restoring configuration files...
copy "%RESTORE_DIR%\.env" .env >nul 2>&1
xcopy /E /I /Y "%RESTORE_DIR%\protocols" protocols >nul 2>&1
echo Configuration restore completed

echo.
echo Cleaning up...
rmdir /S /Q "%RESTORE_DIR%"
echo Cleanup completed

echo.
echo ========================================
echo Restore completed successfully!
echo Please restart SCADA system:
echo   docker-compose restart
echo ========================================
pause
