# Enterprise SCADA System - Complete Windows Setup Guide
## For Complete Beginners - No Prior Knowledge Required!

**Version**: 2.0  
**Platform**: Windows 10/11  
**Time Required**: 60-90 minutes (first time)  
**Skill Level**: Beginner-Friendly (Explained step-by-step)

---

## 📖 What You'll Learn

By the end of this guide, you will have:
- ✅ Installed all necessary software on your Windows computer
- ✅ Set up a complete SCADA system running on your machine
- ✅ Understand what each tool does and why we need it
- ✅ Know how to start, stop, and manage the system

**Don't worry if you're new to programming or servers!** This guide explains everything in simple terms.

---

## 🎯 Before You Start

### What is a SCADA System?

**SCADA** stands for **Supervisory Control and Data Acquisition**. In simple terms:
- It **collects data** from industrial equipment (like temperature sensors, motors, solar panels)
- It **stores this data** so you can look at it later
- It **monitors everything** in real-time on a dashboard
- It **sends alerts** if something goes wrong
- It **predicts problems** before they happen (using AI)

**Think of it like**: A smart home system, but for factories, power plants, and industrial facilities.

### What Will We Install?

We'll install several programs. Here's **WHY** we need each one:

| Software | What It Does | Why We Need It |
|----------|--------------|----------------|
| **Docker** | Runs mini-computers (containers) inside your computer | So all services run isolated and can't break each other |
| **Git** | Keeps track of code changes and lets you download projects | To get the SCADA code and manage versions |
| **.NET** | Microsoft's programming platform | To run the backend services (written in C#) |
| **Node.js** | JavaScript runtime for web apps | To run the React frontend (web dashboard) |
| **Python** | Programming language for data science | To run the AI/ML service (predictions) |
| **VS Code** | Code editor (like Microsoft Word for code) | To view and edit code files easily |

---

## 📋 Table of Contents

1. [Installing Required Software](#installing-required-software)
2. [Understanding Docker (Most Important)](#understanding-docker)
3. [Setting Up the Project](#setting-up-the-project)
4. [Starting Your SCADA System](#starting-your-scada-system)
5. [Using the System](#using-the-system)
6. [Troubleshooting Common Problems](#troubleshooting-common-problems)

---

## 🔧 Installing Required Software

### Understanding "Administrator" Mode

Some installations need **Administrator** access. This means "full permission to make changes to your computer."

**How to open PowerShell as Administrator:**
1. Click the Windows Start button (bottom-left)
2. Type `powershell`
3. Right-click on "Windows PowerShell"
4. Click "Run as administrator"
5. Click "Yes" when Windows asks for permission

A blue window will open - this is PowerShell! You'll type commands here.

---

### Step 1: Install Docker Desktop (MOST IMPORTANT!)

**What is Docker?**  
Imagine Docker as a way to run multiple "mini-computers" inside your one computer. Each "mini-computer" (called a container) runs one part of the SCADA system. They can all talk to each other but stay separate.

**Why do we need it?**  
Without Docker, you'd have to:
- Install 15+ different programs manually
- Configure each one individually
- Make sure they all work together
- Deal with conflicts and errors

With Docker: Everything just works! Click one button, and all 15+ services start together.

**Installation:**

1. Visit: https://www.docker.com/products/docker-desktop
2. Click the big blue "Download for Windows" button
3. Run the downloaded file (Docker Desktop Installer.exe)
4. Follow the installation wizard:
   - ✅ Check "Use WSL 2 instead of Hyper-V" (if asked)
   - ✅ Check "Add shortcut to desktop"
   - Click "Ok" and wait for installation (5-10 minutes)
5. **Restart your computer** (important!)
6. After restart, Docker Desktop will start automatically
   - You'll see a whale icon in the system tray (bottom-right)
   - Wait until the whale icon stops moving (Docker is starting)

**OR use** Windows Package Manager (easier):

Open PowerShell as Administrator and run:
```powershell
# This downloads and installs Docker automatically
winget install Docker.DockerDesktop

# Wait for installation to complete (5-10 minutes)
# Restart your computer after installation
```

**Verify Installation:**

After restart, open PowerShell (**normal mode**, not Administrator) and type:

```powershell
docker --version
```

**Expected output:** `Docker version 24.0.x` (or higher)

If you see this, Docker is installed! ✅

**Configure Docker** (Very Important):

Docker needs enough memory and CPU to run all services.

1. Right-click the Docker whale icon (system tray)
2. Click "Settings"
3. Click "Resources" on the left
4. Set these values:
   - **CPUs**: 4 (or more if you have them)
   - **Memory**: 8 GB minimum ⚠️ (16 GB recommended)
   - **Swap**: 2 GB
   - **Disk image size**: 60 GB minimum
5. Click "Apply & Restart"

**Why these settings?**
- Our SCADA system has 15+ services running at once
- Each service needs memory and CPU power
- Too little = services crash or run very slowly
- Too much = your computer might slow down for other tasks

**Finding the right balance:**
- If you have 16 GB RAM total → Give Docker 8 GB
- If you have 32 GB RAM total → Give Docker 16 GB
- If you have 8 GB RAM total → This might be tight, try 6 GB

---

### Step 2: Install Git (Version Control)

**What is Git?**  
Git is like "Google Docs version history" but for code. It lets programmers:
- Save different versions of code
- Go back to old versions if something breaks
- Share code with team members
- Download code from the internet (like GitHub)

**Why do we need it?**  
To download the SCADA system code from GitHub (or wherever it's stored).

**Installation:**

**Method 1** - Automatic (Recommended):
```powershell
# Open PowerShell as Administrator
winget install Git.Git

# Wait for installation (2-3 minutes)
```

**Method 2** - Manual:
1. Visit: https://git-scm.com/download/win
2. Click "Click here to download"
3. Run the installer
4. Keep clicking "Next" (default settings are fine)
5. Click "Install"

**Verify Installation:**

Close and reopen PowerShell, then:
```powershell
git --version
```

**Expected**: `git version 2.4x.x`

**First-Time Configuration:**

Tell Git who you are (only needed once):

```powershell
# Replace "Your Name" with your actual name
git config --global user.name "Your Name"

# Replace with your email
git config --global user.email "your.email@company.com"

# This helps Git work better on Windows
git config --global core.autocrlf true
```

**What does this do?**
- When you make changes to code, Git records WHO made the change
- This email/name will be attached to your changes

---

### Step 3: Install .NET 8.0 SDK

**What is .NET?**  
.NET is Microsoft's platform for building applications. Our backend services (the "brain" of the SCADA system) are written in C# (a programming language) and run on .NET.

**Why do we need it?**  
Without .NET, the backend code won't run. It's like trying to open a Word document without having Microsoft Word installed.

**Installation:**

 Method 1** - Automatic:
```powershell
# Open PowerShell as Administrator
winget install Microsoft.DotNet.SDK.8

# Wait for download and install (5 minutes)
```

**Method 2** - Manual:
1. Visit: https://dotnet.microsoft.com/download/dotnet/8.0
2. Click "Download .NET SDK x64" (NOT Runtime, we need full SDK)
3. Run the downloaded installer
4. Click through installation wizard

**Verify Installation:**

**IMPORTANT**: Close PowerShell completely and open a new one (this is required!)

```powershell
dotnet --version
```

**Expected**: `8.0.xxx`

**Test it works:**

Let's create a tiny test program:

```powershell
# Create a simple "Hello World" program
dotnet new console -n TestApp

# Go into the folder
cd TestApp

# Run it
dotnet run
```

You should see: `Hello, World!`

This means .NET is working! ✅

**Clean up the test:**

```powershell
# Go back to parent folder
cd ..

# Delete the test folder
Remove-Item -Recurse -Force TestApp
```

---

### Step 4: Install Node.js (JavaScript Runtime)

**What is Node.js?**  
Node.js lets you run JavaScript code on a server (not just in web browsers). Our frontend (the pretty dashboard you see in your browser) is built with React, which needs Node.js to build and run.

**Why do we need it?**  
To run the React dashboard and build the web interface.

**What's the difference between Node.js and JavaScript?**
- JavaScript = The programming language
- Node.js = The environment that runs JavaScript
- Think of it like: English (language) vs. a Person who speaks English (environment)

**Installation:**

**Method 1** - Automatic:
```powershell
# Install LTS (Long Term Support) version - most stable
winget install OpenJS.NodeJS.LTS

# Wait for installation (3-5 minutes)
```

**Method 2** - Manual:
1. Visit: https://nodejs.org/
2. Click the **LTS** button (Long Term Support - recommended!)
3. Run the installer
4. Keep clicking "Next"
5. ✅ Make sure "Automatically install the necessary tools" is checked

**Verify Installation:**

Close and reopen PowerShell:

```powershell
node --version
```

**Expected**: `v18.x.x` or `v20.x.x`

```powershell
npm --version
```

**Expected**: `9.x.x` or `10.x.x`

**What is npm?**  
NPM (Node Package Manager) comes with Node.js. It's like the App Store but for code libraries. Developers use it to download useful code packages.

---

### Step 5: Install Python 3.11 (For AI/ML)

**What is Python?**  
Python is a popular programming language, especially for AI and data science. Our ML Service (Machine Learning - the AI that predicts equipment failures) is written in Python.

**Why do we need it?**  
To run the AI features:
- Anomaly detection (finding unusual patterns)
- Forecasting (predicting future values)
- Predictive maintenance (predicting when equipment will fail)

**Installation:**

**Method 1** - Automatic:
```powershell
winget install Python.Python.3.11
```

**Method 2** - Manual:
1. Visit: https://www.python.org/downloads/
2. Click "Download Python 3.11.x" (latest 3.11 version)
3. Run the installer
4. ⚠️ **VERY IMPORTANT**: Check the box "Add Python to PATH"
5. Click "Install Now"

**What does "Add Python to PATH" mean?**
- PATH is a list of folders where Windows looks for programs
- If Python isn't in PATH, Windows can't find it
- Always check this box!

**Verify Installation:**

Close and reopen PowerShell:

```powershell
python --version
```

**Expected**: `Python 3.11.x`

```powershell
pip --version
```

**Expected**: `pip 23.x.x`

**What is pip?**  
Like npm for Node.js, pip is Python's package manager. It downloads Python libraries.

**Upgrade pip** (recommended):

```powershell
python -m pip install --upgrade pip
```

This makes sure you have the latest version of pip.

---

### Step 6: Install Visual Studio Code (Code Editor) - OPTIONAL but Recommended

**What is VS Code?**  
Visual Studio Code is a code editor - like Microsoft Word, but designed for code. It has:
- Syntax highlighting (colors different parts of code)
- IntelliSense (auto-complete for code)
- Debugging tools
- Extensions for different languages

**Why do we need it?**  
You don't NEED it to run the SCADA system, but it makes viewing and editing code much easier!

**Alternatives:**
- Notepad++ (simpler, free)
- Sublime Text (lightweight)
- Full Visual Studio (heavier, more features)

**Installation:**

```powershell
winget install Microsoft.VisualStudioCode
```

**OR** Download from: https://code.visualstudio.com/

**Install Recommended Extensions:**

After installing VS Code:
1. Open VS Code
2. Click the Extensions icon (left sidebar, looks like 4 squares)
3. Search and install these:
   - **C# Dev Kit** (for .NET code)
   - **ESLint** (for JavaScript linting)
   - **Prettier** (code formatter - makes code look neat)
   - **Docker** (manage Docker containers)
   - **GitLens** (see Git history in files)

---

### Step 7: Install Additional Tools (OPTIONAL)

These are completely optional but make life easier:

**Windows Terminal** - Better command line:
```powershell
winget install Microsoft.WindowsTerminal
```
- Prettier than PowerShell
- Supports tabs
- Copy-paste works better

**Postman** - Test APIs:
```powershell
winget install Postman.Postman
```
- Send HTTP requests to test the backend
- See API responses
- Very useful for debugging

**Azure Data Studio** - Database viewer:
```powershell
winget install Microsoft.AzureDataStudio
```
- View and edit PostgreSQL databases
- Run SQL queries
- See what data is stored

---

## 🐋 Understanding Docker (Most Important Section!)

Before we continue, let's make sure you understand Docker because it's the heart of everything.

### Docker Concepts Explained Simply

**1. What is a Container?**

Think of a container like a shipping container for software:
- A shipping container holds stuff (like furniture)
- A Docker container holds software (like a database or web server)
- Just like you can stack shipping containers on a ship, you can run multiple Docker containers on one computer

**2. What is an Image?**

An image is like a blueprint or recipe:
- Dockerfile = Recipe
- Image = Cake batter (prepared from recipe)
- Container = Baked cake (running instance)

**3. What is Docker Compose?**

Imagine you're cooking a full dinner:
- Appetizer needs oven at 350°F for 20 minutes
- Main course needs stovetop, medium heat
- Dessert needs to chill in fridge

Docker Compose is like a "dinner planner" - it starts all parts at the right time in the right order!

**For our SCADA system:**
- Docker Compose starts 15+ services together
- Makes sure databases start before backend services
- Configures networking between containers
- One command starts everything!

### Our Docker Setup

When you run `docker-compose up`, here's what happens:

```
1. Docker reads docker-compose.yml (our "dinner plan")
2. Creates a private network for all containers to talk
3. Starts databases first (PostgreSQL, InfluxDB, Redis, RabbitMQ)
4. Waits for databases to be ready
5. Starts backend services (ScadaCore, DataAcquisition, etc.)
6. Starts frontend (React dashboard)
7. Starts monitoring (Prometheus, Grafana)
8. Everything is now running and talking to each other!
```

**Visual representation:**

```
Your Computer
│
├─ Docker Engine
   │
   ├─ Container 1: PostgreSQL (database for metadata)
   ├─ Container 2: InfluxDB (database for time-series data)
   ├─ Container 3: RabbitMQ (message queue)
   ├─ Container 4: Redis (cache for fast access)
   ├─ Container 5: ScadaCore (tag management)
   ├─ Container 6: DataAcquisition (data collection)
   ├─ Container 7: AlarmManagement (sends alerts)
   ├─ Container 8: AuthService (login/security)
   ├─ Container 9: ReportingService (generates reports)
   ├─ Container 10: APIGateway (routes requests)
   ├─ Container 11: MLService (AI predictions)
   ├─ Container 12: GraphQLService (modern API)
   ├─ Container 13: WebSocketService (real-time data)
   ├─ Container 14: Frontend (web dashboard)
   ├─ Container 15: Prometheus (metrics)
   └─ Container 16: Grafana (monitoring dashboards)
```

All these containers can talk to each other through Docker's network!

---

## 📦 Setting Up the Project

Now that all tools are installed, let's set up the actual SCADA system!

```powershell
# Download Git for Windows
# Visit: https://git-scm.com/download/win

# Or use winget
winget install Git.Git

# Verify installation
git --version
# Expected: git version 2.4x.x or higher
```

**Configure Git**:
```powershell
# Set your name and email (replace with yours)
git config --global user.name "Your Name"
git config --global user.email "your.email@company.com"

# Set line endings for Windows
git config --global core.autocrlf true

# Verify configuration
git config --list
```

---

### Step 3: Install .NET 8.0 SDK (REQUIRED for Backend Development)

.NET SDK is required to build and run backend services.

```powershell
# Download .NET 8.0 SDK
# Visit: https://dotnet.microsoft.com/download/dotnet/8.0

# Or use winget
winget install Microsoft.DotNet.SDK.8

# Close and reopen PowerShell, then verify
dotnet --version
# Expected: 8.0.x

# List installed SDKs
dotnet --list-sdks
# Should show: 8.0.xxx [path]

# Verify you can create a project
dotnet new console -n TestApp
cd TestApp
dotnet run
# Should print "Hello, World!"
cd ..
rmdir -Recurse -Force TestApp
```

---

### Step 4: Install Node.js (REQUIRED for Frontend Development)

Node.js is needed for the React frontend.

```powershell
# Download Node.js LTS (v18.x or v20.x)
# Visit: https://nodejs.org/

# Or use winget
winget install OpenJS.NodeJS.LTS

# Close and reopen PowerShell, then verify
node --version
# Expected: v18.x.x or v20.x.x

npm --version
# Expected: 9.x.x or 10.x.x

# Test npm
npm --version
```

---

### Step 5: Install Python 3.11 (REQUIRED for ML Service)

Python is needed for the Machine Learning service.

```powershell
# Download Python 3.11
# Visit: https://www.python.org/downloads/

# Or use winget
winget install Python.Python.3.11

# IMPORTANT: During installation, check "Add Python to PATH"

# Close and reopen PowerShell, then verify
python --version
# Expected: Python 3.11.x

pip --version
# Expected: pip 23.x.x or higher

# Upgrade pip
python -m pip install --upgrade pip
```

---

### Step 6: Install Visual Studio Code (RECOMMENDED)

VS Code is the recommended IDE for development.

```powershell
# Download VS Code
# Visit: https://code.visualstudio.com/

# Or use winget
winget install Microsoft.VisualStudioCode

# After installation, install recommended extensions:
# - C# Dev Kit (Microsoft)
# - ESLint (Microsoft)
# - Prettier (Prettier)
# - Docker (Microsoft)
# - GitLens (GitKraken)
```

---

## 💻 Development Tools Setup

### Step 7: Install Additional Tools (OPTIONAL but Recommended)

```powershell
# Install Windows Terminal (better than Command Prompt)
winget install Microsoft.WindowsTerminal

# Install Postman (for API testing)
winget install Postman.Postman

# Install Azure Data Studio or pgAdmin (for database management)
winget install Microsoft.AzureDataStudio
```

---

## 📦 Project Setup

### Step 8: Clone the Repository

```powershell
# Navigate to your development folder
cd C:\Users\YourUsername\Projects

# Clone the SCADA project (replace with your actual repo URL)
# If you don't have a Git repo yet, the files are in C:\Users\TEST\EMS
cd C:\Users\TEST\EMS

# Or if you need to copy from another location:
# xcopy /E /I C:\Users\TEST\EMS C:\Users\YourUsername\Projects\SCADA

# Navigate to project root
cd C:\Users\TEST\EMS
```

---

### Step 9: Configure Environment Variables

```powershell
# Copy the example environment file
copy .env.example .env

# Edit .env file with your settings
notepad .env
```

**Edit `.env` with these settings** (minimum required):

```env
# Database Configuration
POSTGRES_CONNECTION=Host=postgres;Database=scada;Username=scada;Password=scada123
INFLUXDB_URL=http://influxdb:8086
INFLUXDB_TOKEN=scada-token-change-in-production
INFLUXDB_ORG=scada-org
INFLUXDB_BUCKET=scada-data

# Redis
REDIS_CONNECTION=redis:6379,password=redis123

# RabbitMQ
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672
RABBITMQ_USER=guest
RABBITMQ_PASS=guest

# Security (IMPORTANT: Change in production!)
JWT_SECRET=change-this-to-a-secure-256-bit-random-string-in-production
JWT_ISSUER=ScadaSystem
JWT_AUDIENCE=ScadaClient
JWT_EXPIRY_MINUTES=60

# Email Notifications (Optional - configure if you want email alerts)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=your-email@gmail.com
SMTP_PASS=your-app-specific-password
SMTP_FROM=SCADA Alerts <your-email@gmail.com>

# SMS Notifications (Optional - configure if you want SMS alerts)
TWILIO_ACCOUNT_SID=your-twilio-account-sid
TWILIO_AUTH_TOKEN=your-twilio-auth-token
TWILIO_FROM_NUMBER=+1234567890

# API Gateway
API_GATEWAY_URL=http://localhost:5000

# Frontend URLs
REACT_APP_API_URL=http://localhost:5000
REACT_APP_WS_URL=ws://localhost:5007/hubs/tags
REACT_APP_GRAPHQL_URL=http://localhost:5006/graphql
REACT_APP_ML_API_URL=http://localhost:8000

# ClickHouse (Analytics)
CLICKHOUSE_HOST=clickhouse
CLICKHOUSE_PORT=9000
CLICKHOUSE_DB=scada_analytics
CLICKHOUSE_USER=scada
CLICKHOUSE_PASSWORD=clickhouse123
```

Save and close the file.

---

## 🚀 Local Development

### Step 10: Start Infrastructure Services ONLY (First Time Setup)

We'll start services in stages to ensure everything initializes properly.

```powershell
# Make sure you're in the project root
cd C:\Users\TEST\EMS

# Start ONLY infrastructure services first (databases, message queue)
docker-compose up -d influxdb postgres rabbitmq redis

# Wait 30 seconds for databases to initialize
Start-Sleep -Seconds 30

# Check if services are running
docker ps

# You should see 4 containers running:
# - scada-influxdb
# - scada-postgres  
# - scada-rabbitmq
# - scada-redis

# Check logs to ensure no errors
docker-compose logs influxdb
docker-compose logs postgres

# Test PostgreSQL connection
docker exec -it scada-postgres psql -U scada -d scada -c "SELECT version();"
# Should show PostgreSQL version

# Test InfluxDB
curl http://localhost:8086/health
# Should return: {"status":"pass"}
```

---

### Step 11: Initialize Databases

```powershell
# Database migrations should run automatically, but let's verify

# Check PostgreSQL tables
docker exec -it scada-postgres psql -U scada -d scada -c "\dt"
# Should show: tags, users, alarm_rules, sessions, audit_logs, etc.

# Check that admin user exists
docker exec -it scada-postgres psql -U scada -d scada -c "SELECT email, role FROM users WHERE username='admin';"
# Should show: admin@scada.local | Administrator

# If tables are missing, run migration manually:
docker exec -i scada-postgres psql -U scada -d scada < database\migrations\001_initial_schema.sql
```

---

### Step 12: Start Backend Services

```powershell
# Now start backend microservices
docker-compose up -d scada-core data-acquisition alarm-management auth-service reporting-service api-gateway

# Wait for services to start
Start-Sleep -Seconds 20

# Check all backend services are running
docker ps

# Test each service health endpoint
curl http://localhost:5001/health  # ScadaCore
curl http://localhost:5002/health  # DataAcquisition
curl http://localhost:5003/health  # AlarmManagement
curl http://localhost:5004/health  # AuthService
curl http://localhost:5005/health  # ReportingService
curl http://localhost:5000/health  # ApiGateway

# All should return: {"status":"Healthy"}

# Check API Gateway Swagger
Start-Process "http://localhost:5001/swagger"
# Should open Swagger UI in browser
```

---

### Step 13: Start Advanced Services (ML, GraphQL, WebSocket)

```powershell
# Start ML Service
docker-compose up -d ml-service

# Wait for Python dependencies to install (first time is slow)
Start-Sleep -Seconds 30

# Check ML service
curl http://localhost:8000/health
# Should return: {"status":"healthy","service":"ml-service"}

# Test ML API docs
Start-Process "http://localhost:8000/docs"
# Should open FastAPI Swagger UI

# Start GraphQL and WebSocket services
docker-compose up -d graphql-service websocket-service

# Wait for services
Start-Sleep -Seconds 15

# Test GraphQL
Start-Process "http://localhost:5006/graphql"
# Should open GraphQL Playground

# Test WebSocket (will show in logs when frontend connects)
docker-compose logs websocket-service
```

---

### Step 14: Start Frontend

```powershell
# Start the React frontend
docker-compose up -d frontend

# Wait for frontend to build (first time takes 2-3 minutes)
Start-Sleep -Seconds 120

# Check frontend logs
docker-compose logs frontend

# Open the dashboard
Start-Process "http://localhost:3000"
# Should open SCADA dashboard in browser
```

**Login Credentials**:
- **Email**: admin@scada.local
- **Password**: Admin123!

---

### Step 15: Start Monitoring (Optional but Recommended)

```powershell
# Start Prometheus and Grafana
docker-compose up -d prometheus grafana

# Wait for services
Start-Sleep -Seconds 15

# Open Prometheus
Start-Process "http://localhost:9090"

# Open Grafana
Start-Process "http://localhost:3001"
# Login: admin / admin (change password on first login)
```

---

### Step 16: Start ClickHouse Analytics (Optional)

```powershell
# Start ClickHouse for analytics
docker-compose -f docker-compose.clickhouse.yml up -d

# Wait for initialization
Start-Sleep -Seconds 20

# Test ClickHouse
curl http://localhost:8123/ping
# Should return: Ok.

# Start Analytics Service
docker-compose up -d analytics-service
```

---

## ✅ Testing & Verification

### Step 17: Verify Complete System

```powershell
# Check ALL running containers
docker ps

# You should see approximately 15-20 containers:
# - influxdb, postgres, rabbitmq, redis (infrastructure)
# - scada-core, data-acquisition, alarm-management, auth-service, reporting-service, api-gateway (core backend)
# - ml-service, graphql-service, websocket-service (advanced services)
# - frontend (React app)
# - prometheus, grafana (monitoring)
# - clickhouse (optional analytics)
# - node-red (protocol translation)

# Check resource usage
docker stats --no-stream

# View all logs
docker-compose logs

# View logs for specific service
docker-compose logs -f scada-core
# Press Ctrl+C to stop following logs
```

---

### Step 18: Test System Functionality

**Test 1: Authentication**
```powershell
# Test login endpoint
Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/auth/login" `
  -ContentType "application/json" `
  -Body '{"username":"admin","password":"Admin123!"}'

# Should return JWT token
```

**Test 2: Tag Management**
```powershell
# Get all tags
Invoke-RestMethod -Uri "http://localhost:5000/api/tags"

# Should return list of sample tags (turbines, solar panels, etc.)
```

**Test 3: ML Anomaly Detection**
```powershell
# Test anomaly detection
Invoke-RestMethod -Method Post -Uri "http://localhost:8000/api/ml/detect-anomaly" `
  -ContentType "application/json" `
  -Body '{"tagName":"TEST","currentValue":100,"historicalValues":[50,55,52,53,51]}'

# Should return anomaly detection result
```

**Test 4: GraphQL**
```graphql
# Open http://localhost:5006/graphql
# Run this query:
query {
  tags {
    name
    description
  }
}
```

**Test 5: Frontend**
1. Open http://localhost:3000
2. Login with admin@scada.local / Admin123!
3. Check Dashboard displays KPIs
4. Navigate to Tags page
5. Navigate to Alarms page
6. Check real-time updates

---

## 🏭 Production Deployment

### Step 19: Prepare for Production

**Security Hardening**:
```powershell
# Generate strong JWT secret
$bytes = [byte[]]::new(32)
[System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
$jwtSecret = [Convert]::ToBase64String($bytes)
Write-Host "JWT_SECRET=$jwtSecret"

# Update .env with production values
notepad .env
```

**Production `.env` Updates**:
```env
# Change these for production:
JWT_SECRET=<use generated secret above>
POSTGRES_PASSWORD=<complex password>
REDIS_PASSWORD=<complex password>
RABBITMQ_PASS=<complex password>
INFLUXDB_TOKEN=<complex token>
CLICKHOUSE_PASSWORD=<complex password>

# Use production URLs
REACT_APP_API_URL=https://scada.yourdomain.com/api
REACT_APP_WS_URL=wss://scada.yourdomain.com/ws
```

---

### Step 20: Build Production Images

```powershell
# Build all Docker images for production
docker-compose build --no-cache

# Tag images with version
docker tag scada-core:latest scada-core:2.0.0
docker tag scada-frontend:latest scada-frontend:2.0.0
# ... repeat for all services

# Push to Docker registry (if using one)
docker login your-registry.com
docker push scada-core:2.0.0
# ... repeat for all services
```

---

### Step 21: Deploy to Kubernetes (Production)

```powershell
# Install kubectl (if not already installed)
winget install Kubernetes.kubectl

# Verify kubectl
kubectl version --client

# Connect to your Kubernetes cluster
# (This depends on your cloud provider: Azure AKS, AWS EKS, GCP GKE)

# For example, Azure AKS:
az aks get-credentials --resource-group myResourceGroup --name myAKSCluster

# Verify connection
kubectl get nodes

# Create namespace
kubectl create namespace scada-system

# Create secrets
kubectl create secret generic scada-secrets -n scada-system \
  --from-literal=jwt-secret=$jwtSecret \
  --from-literal=postgres-password=<password> \
  --from-literal=influxdb-token=<token>

# Deploy to Kubernetes
kubectl apply -f infrastructure\kubernetes\scada-deployment.yaml

# Check deployment status
kubectl get pods -n scada-system
kubectl get services -n scada-system

# Check logs
kubectl logs -f deployment/scada-core -n scada-system

# Access services (if using LoadBalancer)
kubectl get svc -n scada-system
# Note the EXTERNAL-IP addresses
```

---

### Step 22: Configure SSL/TLS (Production)

```powershell
# Install cert-manager for automatic SSL certificates
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml

# Wait for cert-manager to be ready
kubectl wait --for=condition=Ready pods --all -n cert-manager --timeout=300s

# Create ClusterIssuer for Let's Encrypt
@"
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: your-email@company.com
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
    - http01:
        ingress:
          class: nginx
"@ | kubectl apply -f -

# TLS will be automatically provisioned for your Ingress
```

---

## 🐛 Troubleshooting

### Common Issues and Solutions

**Issue 1: Docker Desktop won't start**
```powershell
# Solution: Enable Hyper-V and WSL2
# 1. Open PowerShell as Administrator
Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V -All
Enable-WindowsOptionalFeature -Online -FeatureName VirtualMachinePlatform

# 2. Install WSL2
wsl --install

# 3. Restart computer
# 4. Start Docker Desktop
```

**Issue 2: Port already in use**
```powershell
# Find what's using the port (e.g., port 5000)
netstat -ano | findstr :5000

# Kill the process (replace PID with actual process ID)
taskkill /PID <PID> /F

# Or change the port in docker-compose.yml
```

**Issue 3: Database connection failed**
```powershell
# Check if PostgreSQL is running
docker ps | findstr postgres

# Check logs
docker logs scada-postgres

# Restart PostgreSQL
docker-compose restart postgres

# Wait 10 seconds
Start-Sleep -Seconds 10

# Test connection again
```

**Issue 4: Frontend can't connect to backend**
```powershell
# Check API Gateway is running
curl http://localhost:5000/health

# Check CORS settings in .env
# Make sure CORS_ORIGINS includes http://localhost:3000

# Check browser console for errors (F12)

# Restart frontend
docker-compose restart frontend
```

**Issue 5: ML Service fails to start**
```powershell
# Check Python dependencies
docker-compose logs ml-service

# Rebuild ML service
docker-compose build --no-cache ml-service
docker-compose up -d ml-service

# Check if Python version is correct (should be 3.11)
docker exec scada-ml-service python --version
```

---

## 📊 Performance Optimization

### For Development

```powershell
# Reduce Docker Desktop resource usage
# Settings → Resources → Advanced
# Memory: 8 GB (instead of 16 GB)
# CPUs: 4 (instead of 8)

# Disable services you're not actively developing
docker-compose stop prometheus grafana clickhouse

# Use development builds (faster)
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up
```

### For Production

```powershell
# Enable Docker BuildKit for faster builds
$env:DOCKER_BUILDKIT=1

# Use multi-stage builds (already configured in Dockerfiles)

# Enable Kubernetes Horizontal Pod Autoscaling
kubectl autoscale deployment scada-core --cpu-percent=70 --min=3 --max=10 -n scada-system
```

---

## 🔄 Maintenance Commands

### Daily Operations

```powershell
# View all running services
docker-compose ps

# View resource usage
docker stats

# View logs (last 100 lines)
docker-compose logs --tail=100

# Follow logs in real-time
docker-compose logs -f

# Restart a specific service
docker-compose restart scada-core

# Stop all services
docker-compose down

# Start all services
docker-compose up -d
```

### Weekly Maintenance

```powershell
# Backup databases (run weekly)
.\scripts\backup.sh  # On Linux/Git Bash
# Or manually:
docker exec scada-postgres pg_dump -U scada scada > backup_$(Get-Date -Format "yyyyMMdd").sql

# Clean up unused images and containers
docker system prune -a --volumes

# Update Docker images
docker-compose pull
docker-compose up -d
```

### Updates and Upgrades

```powershell
# Pull latest code
git pull origin main

# Rebuild services
docker-compose build

# Restart with new images
docker-compose down
docker-compose up -d

# Run database migrations (if any)
kubectl exec -it deployment/scada-core -n scada-system -- dotnet ef database update
```

---

## 🎓 Next Steps

**After Successful Setup**:

1. ✅ **Completed Setup** - All services running
2. 📚 **Read Documentation**:
   - `README.md` - System overview
   - `QUICKSTART.md` - Quick reference
   - `ADVANCED_FEATURES.md` - AI/ML features
   - `docs/administrator-guide.md` - Operations
   - `docs/security-best-practices.md` - Security hardening

3. 🔧 **Configure Your Environment**:
   - Add your actual devices (PLCs, RTUs) in Node-RED
   - Configure SMTP for email alerts
   - Set up Twilio for SMS alerts
   - Create additional users

4. 📊 **Start Using**:
   - Create tags for your equipment
   - Set up alarm rules
   - Build custom dashboards
   - Generate reports

5. 🚀 **Scale to Production**:
   - Deploy to Kubernetes
   - Enable SSL/TLS
   - Configure monitoring
   - Set up automated backups

---

## 📞 Support

**Need Help?**
- Check Troubleshooting section above
- Review logs: `docker-compose logs`
- Check documentation in `docs/` folder
- Open an issue on GitHub

**Emergency Commands**:
```powershell
# Stop everything immediately
docker-compose down

# Nuclear option: Remove everything and start fresh
docker-compose down -v
docker system prune -a --volumes
# Then start from Step 10 again
```

---

## ✅ Verification Checklist

Use this checklist to verify your installation:

- [ ] Docker Desktop installed and running
- [ ] Git installed and configured
- [ ] .NET 8.0 SDK installed
- [ ] Node.js 18+ installed
- [ ] Python 3.11 installed
- [ ] VS Code installed (optional)
- [ ] Project cloned/copied to local directory
- [ ] `.env` file created and configured
- [ ] Infrastructure services running (postgres, influxdb, redis, rabbitmq)
- [ ] Backend services running (all 6 core + 3 advanced)
- [ ] Frontend accessible at http://localhost:3000
- [ ] Can login with admin credentials
- [ ] ML API accessible at http://localhost:8000/docs
- [ ] GraphQL accessible at http://localhost:5006/graphql
- [ ] Monitoring (Grafana) accessible at http://localhost:3001
- [ ] All health checks passing

**If all checkboxes are checked, congratulations! Your SCADA system is fully operational! 🎉**

---

**Version**: 2.0.0  
**Last Updated**: 2025-01-01  
**Author**: Jyotirmoy Bhowmik  
**Platform**: Windows 10/11 Professional/Enterprise
