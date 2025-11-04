@echo off
REM === MAKE PATHS RELATIVE TO THIS FILE ===
set "CONDA_ENV=C:\Users\Lubos\miniconda3\envs\mlagents"
cd /d "%~dp0"

REM === CONFIGURATION ===
set "PYTHON_EXE=%USERPROFILE%\anaconda3\envs\%CONDA_ENV%\python.exe"
set "MLAGENTS_LEARN=mlagents-learn"
set "CONFIG_PATH=trainingConfig.yaml"
REM === Leave ENV_PATH empty if running via Unity Editor ===
set "ENV_PATH="
set "BEHAVIOR_NAME=nico_new_agent_"
set "RESULTS_DIR=..\NICOADR\Assets\Runtime\ML\Models\experiments"

REM === AUTOINCREMENT RUN ID ===
set "BASE_RUN_NAME=%BEHAVIOR_NAME%"
set /A COUNTER=1

:CHECK_FOLDER
if exist "%RESULTS_DIR%\%BASE_RUN_NAME%%COUNTER%" (
    set /A COUNTER+=1
    goto CHECK_FOLDER
)

set "RUN_ID=%RESULTS_DIR%\%BASE_RUN_NAME%%COUNTER%"

REM === ECHO WHAT WILL RUN ===
echo --------------------------------------
echo Training Run ID: %RUN_ID%
echo Conda Environment: %CONDA_ENV%
echo Config Path: %CONFIG_PATH%
echo Environment Path: %ENV_PATH%
echo --------------------------------------

REM === START TRAINING IN NEW CMD ===
if "%ENV_PATH%"=="" (
    start "Training Window" cmd /k "conda activate %CONDA_ENV% && mlagents-learn %CONFIG_PATH% --run-id=%RUN_ID% --train"
) else (
    start "Training Window" cmd /k "conda activate %CONDA_ENV% && mlagents-learn %CONFIG_PATH% --run-id=%RUN_ID% --env=%ENV_PATH% --train"
)

REM === START TENSORBOARD IN NEW CMD ===
start "TensorBoard Window" cmd /k "conda activate %CONDA_ENV% && tensorboard --logdir=%RESULTS_DIR%"

REM === OPEN BROWSER ===
start "" http://localhost:6006
