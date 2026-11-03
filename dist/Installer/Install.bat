@echo off
chcp 65001 >nul
title Potato Please — Steam Co-op Mod Installer

echo ==============================================================
echo   POTATO PLEASE // STEAM CO-OP MOD INSTALLER (v1.0.0)
echo ==============================================================
echo.

if exist "PotatoCoopInstaller.exe" (
    PotatoCoopInstaller.exe
) else (
    echo [!] Копирование файлов мода в папку игры...
    if exist "..\PotatoPlease.exe" (
        copy /Y "version.dll" "..\version.dll" >nul 2>&1
        copy /Y "steam_appid.txt" "..\steam_appid.txt" >nul 2>&1
        copy /Y "steam_api64.dll" "..\steam_api64.dll" >nul 2>&1
        if not exist "..\Mods" mkdir "..\Mods"
        copy /Y "Mods\PotatoCoop.dll" "..\Mods\PotatoCoop.dll" >nul 2>&1
        echo [✓] Файлы мода успешно скопированы!
    ) else (
        echo [!] Поместите установщик рядом с игрой или запустите PotatoCoopInstaller.exe.
    )
    pause
)
