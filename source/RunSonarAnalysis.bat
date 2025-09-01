@echo off
REM ==============================
REM SonarQube analysis for OstPlayer
REM ==============================

REM Nastav proměnné
SET SONAR_HOST=http://localhost:9000
SET SONAR_PROJECT_KEY=OstPlayer
SET SONAR_TOKEN=sqp_a748d59b5494d5a3fcedfc0141b53d8b8e31da24

REM Spuštění SonarScanner begin
echo [1/3] Initializing SonarQube Scanner...
SonarScanner.MSBuild.exe begin /k:"%SONAR_PROJECT_KEY%" /d:sonar.host.url="%SONAR_HOST%" /d:sonar.token="%SONAR_TOKEN%"

IF ERRORLEVEL 1 (
    echo ERROR: SonarScanner begin failed
    pause
    exit /b 1
)

REM Build projektu
echo [2/3] Building the project...
MsBuild.exe /t:Rebuild

IF ERRORLEVEL 1 (
    echo ERROR: Build failed
    pause
    exit /b 1
)

REM Ukončení SonarScanner a odeslání výsledků
echo [3/3] Finalizing SonarQube analysis...
SonarScanner.MSBuild.exe end /d:sonar.token="%SONAR_TOKEN%"

IF ERRORLEVEL 1 (
    echo ERROR: SonarScanner end failed
    pause
    exit /b 1
)

echo Analysis completed successfully!
pause
