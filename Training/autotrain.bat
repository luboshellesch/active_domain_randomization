@echo off
REM === CONFIGURATION ===
set "CONDA_ENV=C:\Users\Lubos\miniconda3\envs\mlagents"
set "PYTHON_EXE=%USERPROFILE%\anaconda3\envs\%CONDA_ENV%\python.exe"
set "MLAGENTS_LEARN=mlagents-learn"
set "CONFIG_PATH=C:\diplomovka\active_domain_randomization\Training\trainingConfig.yaml"
REM === Leave ENV_PATH empty if running via Unity Editor ===
set "ENV_PATH="  
set "BEHAVIOR_NAME=agent"
set "RESULTS_DIR=C:\diplomovka\active_domain_randomization\Training\results"

REM === AUTOINCREMENT RUN ID ===
set "BASE_RUN_NAME=%BEHAVIOR_NAME%"
set /A COUNTER=1

:CHECK_FOLDER
set "PADDED=00%COUNTER%"
set "RUN_NAME=%BASE_RUN_NAME%%PADDED:~-3%"

if exist "%RESULTS_DIR%\%RUN_NAME%" (
    set /A COUNTER+=1
    goto CHECK_FOLDER
)

set "RUN_ID=%RESULTS_DIR%\%RUN_NAME%"

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


timeout /t 2 /nobreak >nul

REM === START TENSORBOARD IN NEW CMD ===
start "TensorBoard Window" cmd /k "conda activate %CONDA_ENV% && tensorboard --logdir=%RESULTS_DIR%"

REM === OPEN BROWSER ===
start "" http://localhost:6006
