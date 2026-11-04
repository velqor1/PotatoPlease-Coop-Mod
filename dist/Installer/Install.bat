@echo off
chcp 65001 >nul
title Potato Please — Steam Co-op Mod Installer

echo ==============================================================
echo   POTATO PLEASE // STEAM CO-OP MOD INSTALLER (v1.0.1)
echo   Сетевой кооператив до 4 игроков через Steam Spacewar (480)
echo ==============================================================
echo.

if exist "PotatoCoopInstaller.exe" (
    PotatoCoopInstaller.exe
) else (
    echo [*] Копирование файлов мода в папку игры...
    if exist "..\PotatoPlease.exe" (
        copy /Y "version.dll" "..\version.dll" >nul 2>&1
        copy /Y "steam_appid.txt" "..\steam_appid.txt" >nul 2>&1
        copy /Y "steam_api64.dll" "..\steam_api64.dll" >nul 2>&1
        
        if not exist "..\PotatoPlease_Data\Plugins\x86_64" mkdir "..\PotatoPlease_Data\Plugins\x86_64"
        copy /Y "steam_api64.dll" "..\PotatoPlease_Data\Plugins\x86_64\steam_api64.dll" >nul 2>&1
        
        if exist "PotatoPlease_Data\Plugins\x86_64\steam_api64.dll" (
            copy /Y "PotatoPlease_Data\Plugins\x86_64\steam_api64.dll" "..\PotatoPlease_Data\Plugins\x86_64\steam_api64.dll" >nul 2>&1
        )
        
        if exist "MelonLoader" (
            xcopy /E /I /Y "MelonLoader" "..\MelonLoader" >nul 2>&1
        )
        
        if not exist "..\Mods" mkdir "..\Mods"
        if exist "Mods\PotatoCoop.dll" (
            copy /Y "Mods\PotatoCoop.dll" "..\Mods\PotatoCoop.dll" >nul 2>&1
        )
        
        echo [✓] Файлы мода успешно скопированы!
        echo.
        echo [?] Если в Steam нет статуса Spacewar:
        echo     Нажмите Win + R, введите: steam://run/480 и нажмите Enter.
    ) else (
        echo [!] Поместите папку мода в папку с игрой или запустите PotatoCoopInstaller.exe.
    )
    pause
)
