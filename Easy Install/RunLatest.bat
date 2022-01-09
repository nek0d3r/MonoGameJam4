@echo off

: Environment variables
set @netmin=3.1&                                        : Minimum .NET SDK version
set @netmax=3.2&                                        : Upper bound .NET version
set @gitfile=Git-2.34.1-64-bit&                         : Filename of git installer
set @project=MonoGameJam4\&                             : Project directory
set @gitrepo=https://github.com/nek0d3r/MonoGameJam4&   : Git repository URL

@REM .NET DEPENDENCY

echo Checking for .NET...
: If dotnet CLI command succeeds
dotnet --version >nul 2>&1 && (
    : Output version to temp file
    dotnet --version>temp
    : Read contents of temp file
    for /f %%i in (temp) do (
        : If the version listed is greater than the minimum
        if %%i geq %@netmin% if %%i lss %@netmax% (
            echo .NET SDK %%i found!
        ) else (
            : Install .NET SDK LTS using Microsoft powershell script
            echo .NET SDK version %%i is not supported. Installing %@netmin%...
            : Thank you SO MUCH to Daniel Schroeder for this one
            Powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "& './dotnet-install.ps1' -Channel %@netmin%"
            setlocal
            set PATH="%PATH%;%LOCALAPPDATA%\Microsoft\dotnet"

        )
    )
    : Remove temp file
    del temp
) || (
    : Install .NET SDK LTS using Microsoft powershell script
    echo .NET SDK not found. Installing %@netmin%...
    Powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "& './dotnet-install.ps1' -Channel %@netmin%"
    setlocal
    set PATH="%PATH%;%LOCALAPPDATA%\Microsoft\dotnet"
)

@REM GIT DEPENDENCY

echo,
echo Checking for Git...
: If git CLI command succeeds
git --version >nul 2>&1 && (
    echo Git found!
) || (
    : Install git for windows using provided installer
    : and script written by Pat Migliaccio
    echo Git not found. Installing Git...
    call install-git.bat
)

@REM PROJECT DEPENDENCY

echo,
echo Checking if repository is cloned...
: Project is cloned if directory exists
if exist %@project% (
    echo Project source code is already cloned!
) else (
    : Clone project using given URL
    echo Project not cloned. Cloning...
    git clone %@gitrepo%
)

: Pull latest changes
echo,
echo Pulling changes from latest commit...
git -C %@project% pull

: Build and run
echo,
echo Source code up to date! Compiling and running...
dotnet build %@project%
dotnet run --project %@project%

pause