using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace PotatoCoopInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@"
  ==============================================================
    ПОТАТО ПЛИЗ // POTATO PLEASE — STEAM CO-OP MOD INSTALLER
    Сетевой кооператив до 4 игроков через Steam Spacewar (AppID 480)
  ==============================================================");
            Console.ResetColor();

            string currentDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');
            string gamePath = FindGameDirectory(currentDir);

            if (string.IsNullOrEmpty(gamePath))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n[?] Не удалось автоматически найти папку с игрой Potato Please.");
                Console.Write("Пожалуйста, введите полный путь к папке с игрой (где лежит PotatoPlease.exe):\n> ");
                Console.ResetColor();

                string input = Console.ReadLine()?.Trim().Trim('"', '\'');
                if (!string.IsNullOrEmpty(input) && Directory.Exists(input) && File.Exists(Path.Combine(input, "PotatoPlease.exe")))
                {
                    gamePath = input;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[!] Ошибка: Папка не существует или в ней отсутствует PotatoPlease.exe.");
                    Console.ResetColor();
                    WaitAndExit();
                    return;
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n[+] Найдена игра: {gamePath}");
            Console.ResetColor();

            Console.WriteLine("\nВыберите действие:");
            Console.WriteLine("  1. Установить / Обновить Кооперативный мод");
            Console.WriteLine("  2. Удалить Кооперативный мод");
            Console.WriteLine("  3. Запустить игру");
            Console.WriteLine("  0. Выход");
            Console.Write("\nВведите номер действия (по умолчанию 1): ");

            string choice = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(choice)) choice = "1";

            switch (choice)
            {
                case "1":
                    InstallMod(currentDir, gamePath);
                    break;
                case "2":
                    UninstallMod(gamePath);
                    break;
                case "3":
                    LaunchGame(gamePath);
                    break;
                default:
                    Console.WriteLine("Выход.");
                    break;
            }

            WaitAndExit();
        }

        static string FindGameDirectory(string sourceDir)
        {
            // 1. Check current directory
            if (File.Exists(Path.Combine(sourceDir, "PotatoPlease.exe")))
                return sourceDir;

            // 2. Check parent directory
            string parent = Directory.GetParent(sourceDir)?.FullName;
            if (!string.IsNullOrEmpty(parent) && File.Exists(Path.Combine(parent, "PotatoPlease.exe")))
                return parent;

            // 3. Search common Steam library locations
            string[] commonPaths = new string[]
            {
                @"C:\Program Files (x86)\Steam\steamapps\common\Potato Please",
                @"C:\Program Files\Steam\steamapps\common\Potato Please",
                @"D:\Steam\steamapps\common\Potato Please",
                @"D:\SteamLibrary\steamapps\common\Potato Please",
                @"E:\SteamLibrary\steamapps\common\Potato Please",
                @"F:\SteamLibrary\steamapps\common\Potato Please",
                @"G:\SteamLibrary\steamapps\common\Potato Please",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "Potato Please")
            };

            foreach (var p in commonPaths)
            {
                if (Directory.Exists(p) && File.Exists(Path.Combine(p, "PotatoPlease.exe")))
                    return p;
            }

            return null;
        }

        static void InstallMod(string sourceDir, string gamePath)
        {
            Console.WriteLine("\n[*] Установка файлов кооп-мода...");

            try
            {
                // Source paths: check if payload is in sourceDir or parent
                string basePayloadDir = sourceDir;
                if (!File.Exists(Path.Combine(basePayloadDir, "steam_appid.txt")))
                {
                    string parent = Directory.GetParent(sourceDir)?.FullName;
                    if (!string.IsNullOrEmpty(parent) && File.Exists(Path.Combine(parent, "steam_appid.txt")))
                        basePayloadDir = parent;
                }

                // 1. steam_appid.txt
                string appidDst = Path.Combine(gamePath, "steam_appid.txt");
                File.WriteAllText(appidDst, "480");
                Console.WriteLine("  ✓ steam_appid.txt (AppID 480 Spacewar)");

                // 2. steam_api64.dll (in BOTH root and PotatoPlease_Data\Plugins\x86_64\)
                string steamApiSrc = Path.Combine(basePayloadDir, "steam_api64.dll");
                if (!File.Exists(steamApiSrc))
                    steamApiSrc = Path.Combine(basePayloadDir, "PotatoPlease_Data", "Plugins", "x86_64", "steam_api64.dll");

                if (File.Exists(steamApiSrc))
                {
                    // Copy to game root
                    File.Copy(steamApiSrc, Path.Combine(gamePath, "steam_api64.dll"), true);
                    Console.WriteLine("  ✓ steam_api64.dll (в корне игры)");

                    // Copy to PotatoPlease_Data/Plugins/x86_64/
                    string pluginsDir = Path.Combine(gamePath, "PotatoPlease_Data", "Plugins", "x86_64");
                    if (!Directory.Exists(pluginsDir)) Directory.CreateDirectory(pluginsDir);
                    File.Copy(steamApiSrc, Path.Combine(pluginsDir, "steam_api64.dll"), true);
                    Console.WriteLine("  ✓ PotatoPlease_Data/Plugins/x86_64/steam_api64.dll (замена кряка/эмулятора)");
                }

                // 3. version.dll (MelonLoader proxy)
                string versionSrc = Path.Combine(basePayloadDir, "version.dll");
                if (File.Exists(versionSrc))
                {
                    File.Copy(versionSrc, Path.Combine(gamePath, "version.dll"), true);
                    Console.WriteLine("  ✓ version.dll (MelonLoader bootstrap)");
                }

                // 4. MelonLoader folder
                string melonSrc = Path.Combine(basePayloadDir, "MelonLoader");
                string melonDst = Path.Combine(gamePath, "MelonLoader");
                if (Directory.Exists(melonSrc) && melonSrc != melonDst)
                {
                    CopyDirectory(melonSrc, melonDst, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Latest.log", "Logs" });
                    Console.WriteLine("  ✓ MelonLoader runtime core");
                }

                // 5. Mods/PotatoCoop.dll
                string modsDst = Path.Combine(gamePath, "Mods");
                if (!Directory.Exists(modsDst)) Directory.CreateDirectory(modsDst);

                string modDllSrc = Path.Combine(basePayloadDir, "Mods", "PotatoCoop.dll");
                if (!File.Exists(modDllSrc))
                    modDllSrc = Path.Combine(basePayloadDir, "PotatoCoop.dll");

                if (File.Exists(modDllSrc))
                {
                    File.Copy(modDllSrc, Path.Combine(modsDst, "PotatoCoop.dll"), true);
                    Console.WriteLine("  ✓ Mods/PotatoCoop.dll");
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(@"
  ==============================================================
    ✓ МОД УСПЕШНО УСТАНОВЛЕН!
  ==============================================================
  1. Запустите Steam (клиент должен быть открыт в фоне).
  2. Запустите PotatoPlease.exe.
  3. В главном меню появится кнопка 'Кооператив (Steam)'.
  4. Нажмите F1 или Ctrl+M в любой момент для открытия терминала.
  5. Создайте лобби и пригласите друзей через Steam или скопируйте ID!
  ==============================================================");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n[?] Для игры по сети Steam требует наличие лицензии Spacewar (AppID 480).");
                Console.WriteLine("    Если вы или ваш друг ни разу не играли в пиратский кооператив Steam,");
                Console.WriteLine("    нужно один раз зарегистрировать Spacewar в аккаунте Steam.");
                Console.Write("    Открыть регистрацию Spacewar в клиенте Steam сейчас? (Y/n): ");
                Console.ResetColor();

                string spacewarReg = Console.ReadLine()?.Trim().ToLower();
                if (string.IsNullOrEmpty(spacewarReg) || spacewarReg == "y" || spacewarReg == "yes" || spacewarReg == "д" || spacewarReg == "да")
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo("steam://run/480") { UseShellExecute = true });
                        Console.WriteLine("    [+] Команда отправлена в Steam. Если появится окно 'Установка Spacewar',");
                        Console.WriteLine("        нажмите 'Отмена' — лицензия уже навсегда добавлена на ваш аккаунт!");
                    }
                    catch { }
                }

                Console.Write("\nЗапустить игру сейчас? (Y/n): ");
                string start = Console.ReadLine()?.Trim().ToLower();
                if (string.IsNullOrEmpty(start) || start == "y" || start == "yes" || start == "д" || start == "да")
                {
                    LaunchGame(gamePath);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[!] Ошибка при установке: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void UninstallMod(string gamePath)
        {
            Console.WriteLine("\n[*] Удаление мода...");
            try
            {
                string modDll = Path.Combine(gamePath, "Mods", "PotatoCoop.dll");
                if (File.Exists(modDll)) File.Delete(modDll);

                string verDll = Path.Combine(gamePath, "version.dll");
                if (File.Exists(verDll)) File.Delete(verDll);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  ✓ Мод успешно удалён. Игра возвращена к исходному состоянию.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[!] Ошибка при удалении: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void LaunchGame(string gamePath)
        {
            try
            {
                string exePath = Path.Combine(gamePath, "PotatoPlease.exe");
                if (File.Exists(exePath))
                {
                    Console.WriteLine("\n[>] Запуск PotatoPlease.exe...");
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = exePath,
                        WorkingDirectory = gamePath,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Не удалось запустить игру: {ex.Message}");
            }
        }

        static void CopyDirectory(string sourceDir, string destinationDir, HashSet<string> excludeNames)
        {
            Directory.CreateDirectory(destinationDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string name = Path.GetFileName(file);
                if (excludeNames != null && excludeNames.Contains(name)) continue;
                string destFile = Path.Combine(destinationDir, name);
                File.Copy(file, destFile, true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string name = Path.GetFileName(subDir);
                if (excludeNames != null && excludeNames.Contains(name)) continue;
                string destSubDir = Path.Combine(destinationDir, name);
                CopyDirectory(subDir, destSubDir, excludeNames);
            }
        }

        static void WaitAndExit()
        {
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}
