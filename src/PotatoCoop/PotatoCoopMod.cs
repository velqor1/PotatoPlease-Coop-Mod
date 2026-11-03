using System;
using System.IO;
using System.Runtime.InteropServices;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;
using HarmonyLib;
using PotatoCoop.Network;
using PotatoCoop.Puppet;
using PotatoCoop.UI;
using PotatoCoop.Sync;
using Il2CppPotatoPeeler;

[assembly: MelonInfo(typeof(PotatoCoop.PotatoCoopMod), "Potato Please Steam Co-op", "1.0.0", "Co-op Mod Team")]
[assembly: MelonGame("GoldenHorn", "PotatoPlease")]

namespace PotatoCoop
{
    public class PotatoCoopMod : MelonMod
    {
        public static PotatoCoopMod Instance { get; private set; }

        private float _sendTimer = 0f;
        private const float SEND_INTERVAL = 0.05f; // 20 Hz update rate for smooth movement

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibraryW(string lpLibFileName);

        public override void OnInitializeMelon()
        {
            Instance = this;
            MelonLogger.Msg("==================================================");
            MelonLogger.Msg("  Potato Please Steam Spacewar Co-op Mod Loading  ");
            MelonLogger.Msg("==================================================");

            EnsureSteamOverlay();

            SteamNetManager.Initialize();
            LobbyManager.Initialize();
            PuppetManager.Initialize();
            QuestSync.Initialize();
            VoxelSync.Initialize();
            WorldSync.Initialize();
            LobbyUI.Initialize();
            CoopHUD.Initialize();

            MelonLogger.Msg("[PotatoCoop] All co-op subsystems initialized successfully!");
        }

        private static void EnsureSteamOverlay()
        {
            try
            {
                string[] searchPaths = new string[]
                {
                    @"C:\Program Files (x86)\Steam\GameOverlayRenderer64.dll",
                    @"C:\Program Files\Steam\GameOverlayRenderer64.dll"
                };

                foreach (var path in searchPaths)
                {
                    if (File.Exists(path))
                    {
                        IntPtr handle = LoadLibraryW(path);
                        MelonLogger.Msg($"[PotatoCoop] Steam GameOverlayRenderer64 loaded from: {path} (Handle: {handle})");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[PotatoCoop] Could not auto-load Steam Overlay: {ex.Message}");
            }
        }

        public override void OnUpdate()
        {
            SteamNetManager.Instance?.Update();
            PuppetManager.Instance?.Update();
            CoopHUD.Instance?.Update();

            // Hotkey F1 or Ctrl+M to toggle Co-op UI (using Unity InputSystem)
            try
            {
                var kb = Keyboard.current;
                if (kb != null)
                {
                    if (kb.f1Key.wasPressedThisFrame || (kb.leftCtrlKey.isPressed && kb.mKey.wasPressedThisFrame))
                    {
                        if (LobbyUI.Instance != null)
                        {
                            LobbyUI.Instance.IsVisible = !LobbyUI.Instance.IsVisible;
                        }
                    }
                }
            }
            catch
            {
                // InputSystem fallback
            }

            // Stream local player transform to peers
            if (SteamNetManager.Instance != null && (SteamNetManager.Instance.IsConnected || SteamNetManager.Instance.IsHost))
            {
                _sendTimer += Time.deltaTime;
                if (_sendTimer >= SEND_INTERVAL)
                {
                    _sendTimer = 0f;
                    StreamLocalPlayerTransform();
                }
            }
        }

        public override void OnGUI()
        {
            // IMGUI Hotkey F1 fallback
            if (Event.current != null && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F1)
            {
                if (LobbyUI.Instance != null)
                {
                    LobbyUI.Instance.IsVisible = !LobbyUI.Instance.IsVisible;
                }
            }

            LobbyUI.Instance?.DrawGUI();
            CoopHUD.Instance?.DrawGUI();
        }

        private void StreamLocalPlayerTransform()
        {
            try
            {
                var player = GameObject.FindObjectOfType<Player>();
                if (player == null) return;

                Vector3 pos = player.transform.position;
                float yaw = player.transform.eulerAngles.y;
                float pitch = player.Camera != null ? player.Camera.transform.eulerAngles.x : 0f;
                float speed = player.Move != null ? player.Move.CurrentSpeed : 0f;
                bool isCrouching = player.Move != null && player.Move.IsCrouching;

                int toolId = 0;
                if (player.ToolSwitcher != null)
                {
                    toolId = (int)player.ToolSwitcher.Current;
                }

                byte[] pkt = PacketSerializer.SerializePlayerTransform(pos, yaw, pitch, speed, isCrouching, toolId, false);
                SteamNetManager.Instance.BroadcastPacket(pkt, false);
            }
            catch
            {
                // Silently handle scene transition frames
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            MelonLogger.Msg($"[PotatoCoop] Scene loaded: {sceneName} (Index: {buildIndex})");

            if (sceneName == "MainMenu" || buildIndex == 0)
            {
                if (LobbyUI.Instance != null)
                {
                    LobbyUI.Instance.IsVisible = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(MainMenuController), "Awake")]
    public static class Patch_MainMenuController_Awake
    {
        public static void Postfix(MainMenuController __instance)
        {
            MelonLogger.Msg("[PotatoCoop] MainMenuController detected, adding Co-op button!");
            MenuButtonPatcher.PatchMainMenu(__instance);
            if (LobbyUI.Instance != null)
            {
                LobbyUI.Instance.IsVisible = true;
            }
        }
    }

    [HarmonyPatch(typeof(PauseUI), "Awake")]
    public static class Patch_PauseUI_Awake
    {
        public static void Postfix(PauseUI __instance)
        {
            MelonLogger.Msg("[PotatoCoop] PauseUI detected, adding Co-op button!");
            MenuButtonPatcher.PatchPauseMenu(__instance);
        }
    }

    [HarmonyPatch(typeof(QuestManager), "Awake")]
    public static class Patch_QuestManager_Awake
    {
        public static void Postfix(QuestManager __instance)
        {
            MelonLogger.Msg("[PotatoCoop] QuestManager initialized, binding synchronized co-op quest hooks!");
            QuestSync.Instance?.HookQuestManager(__instance);
        }
    }
}
