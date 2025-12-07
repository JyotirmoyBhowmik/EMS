@echo off
REM SCADA System Backup Script for Windows
REM This script backs up PostgreSQL, InfluxDB, and configuration files

echo ========================================
echo SCADA System Backup Script
echo ========================================
echo.

REM Set variables
set BACKUP_DATE=%date:~-4,4%%date:~-10,2%%date:~-7,2%_%time:~0,2%%time:~3,2%%time:~6,2%
set BACKUP_DATE=%BACKUP_DATE: =0%
set BACKUP_DIR=backups\%BACKUP_DATE%
set PROJECT_ROOT=%~dp0..

echo Creating backup directory: %BACKUP_DIR%
mkdir "%PROJECT_ROOT%\%BACKUP_DIR%" 2>nul

echo.
echo [1/5] Backing up PostgreSQL database...
docker exec scada-postgres pg_dump -U scada scada > "%PROJECT_ROOT%\%BACKUP_DIR%\postgres_backup.sql"
if %ERRORLEVEL% EQU 0 (
    echo PostgreSQL backup completed successfully
) else (
    echo ERROR: PostgreSQL backup failed
)

echo.
echo [2/5] Backing up InfluxDB database...
docker exec scada-influxdb influx backup /tmp/influxdb-backup -t scada-token-change-in-production
docker cp scada-influxdb:/tmp/influxdb-backup "%PROJECT_ROOT%\%BACKUP_DIR%\influxdb-backup"
if %ERRORLEVEL% EQU 0 (
    echo InfluxDB backup completed successfully
) else (
    echo ERROR: InfluxDB backup failed
)

echo.
echo [3/5] Backing up configuration files...
copy "%PROJECT_ROOT%\.env" "%PROJECT_ROOT%\%BACKUP_DIR%\.env" >nul 2>&1
xcopy /E /I /Y "%PROJECT_ROOT%\protocols" "%PROJECT_ROOT%\%BACKUP_DIR%\protocols" >nul 2>&1
xcopy /E /I /Y "%PROJECT_ROOT%\infrastructure" "%PROJECT_ROOT%\%BACKUP_DIR%\infrastructure" >nul 2>&1
echo Configuration backup completed

echo.
echo [4/5] Creating backup manifest...
(
    echo Backup Date: %date% %time%
    echo PostgreSQL: Included
    echo InfluxDB: Included
    echo Configuration: Included
    echo Docker Version: 
    docker --version
) > "%PROJECT_ROOT%\%BACKUP_DIR%\manifest.txt"
echo Manifest created

echo.
echo [5/5] Compressing backup...
tar -czf "%PROJECT_ROOT%\backups\scada_backup_%BACKUP_DATE%.tar.gz" -C "%PROJECT_ROOT%\%BACKUP_DIR%" .
if %ERRORLEVEL% EQU 0 (
    echo Compression completed
    rmdir /S /Q "%PROJECT_ROOT%\%BACKUP_DIR%"
    echo Temporary files cleaned up
) else (
    echo WARNING: Compression failed, backup files remain in: %BACKUP_DIR%
)

echo.
echo ========================================
echo Backup completed successfully!
echo Location: backups\scada_backup_%BACKUP_DATE%.tar.gz
echo ========================================
echo.

REM Cleanup old backups (keep last 7 days)
echo Cleaning up backups older than 7 days...
forfiles /P "%PROJECT_ROOT%\backups" /S /M *.tar.gz /D -7 /C "cmd /c del @path" 2>nul
echo.

echo Backup process finished.
pause
