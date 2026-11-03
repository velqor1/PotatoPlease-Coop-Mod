@echo off
chcp 65001 >nul
echo ==============================================================
echo   Сборка Potato Please Steam Co-op Mod & Installer
echo ==============================================================

echo [*] Сборка мода PotatoCoop.dll...
dotnet build src\PotatoCoop\PotatoCoop.csproj -c Release
if %ERRORLEVEL% NEQ 0 (
    echo [!] Ошибка сборки мода.
    pause
    exit /b %ERRORLEVEL%
)

echo [*] Публикация PotatoCoopInstaller.exe...
dotnet publish src\PotatoCoopInstaller\PotatoCoopInstaller.csproj -c Release -o dist\Installer
if %ERRORLEVEL% NEQ 0 (
    echo [!] Ошибка сборки установщика.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo [✓] Сборка успешно завершена! Файлы находятся в Mods\ и dist\Installer\
pause
