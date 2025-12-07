@echo off
REM SCADA System Startup Script for Windows
REM This script starts all SCADA services in the correct order

echo Starting Enterprise SCADA System...

REM Check prerequisites
where docker >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo Docker is required but not installed. Aborting.
    exit /b 1
)

where docker-compose >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo Docker Compose is required but not installed. Aborting.
    exit /b 1
)

REM Check if .env file exists
if not exist .env (
    echo .env file not found. Creating from template...
    copy .env.example .env
    echo Created .env file. Please update with your configuration.
    echo Edit .env and run this script again.
    exit /b 1
)

REM Start infrastructure services first
echo Starting infrastructure services...
docker-compose up -d influxdb postgres rabbitmq redis node-red

REM Wait for databases
echo Waiting for databases to initialize...
timeout /t 15 /nobreak

REM Start backend services
echo Starting backend services...
docker-compose up -d scada-core data-acquisition alarm-management auth-service reporting-service api-gateway

REM Wait for backend
echo Waiting for backend services...
timeout /t 10 /nobreak

REM Start frontend
echo Starting frontend...
docker-compose up -d frontend

REM Start monitoring
echo Starting monitoring services...
docker-compose up -d prometheus grafana

echo.
echo ========================================
echo SCADA System Started Successfully!
echo ========================================
echo.
echo Access Points:
echo   Frontend Dashboard:    http://localhost:3000
echo   API Gateway:           http://localhost:5000
echo   API Documentation:     http://localhost:5001/swagger
echo   RabbitMQ Management:   http://localhost:15672 (guest/guest)
echo   Node-RED:              http://localhost:1880
echo   Grafana:               http://localhost:3001 (admin/admin)
echo   Prometheus:            http://localhost:9090
echo.
echo Default Login: admin@scada.local / Admin123!
echo.
echo To view logs: docker-compose logs -f
echo To stop: scripts\stop.bat
echo.
pause
