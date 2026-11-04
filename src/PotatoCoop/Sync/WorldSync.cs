using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using PotatoCoop.Network;
using Il2CppPotatoPeeler;
using MelonLoader;

namespace PotatoCoop.Sync
{
    public class WorldSync
    {
        public static WorldSync Instance { get; private set; }

        public static bool IsSyncingEconomy = false;
        public static bool IsSyncingUpgrade = false;
        public static bool IsSyncingDoor = false;
        public static bool IsSyncingSack = false;
        public static bool IsSyncingQuest = false;

        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new WorldSync();
                Instance.RegisterEvents();
            }
        }

        private void RegisterEvents()
        {
            SteamNetManager.Instance.OnMachineSyncReceived += HandleMachineSync;
            SteamNetManager.Instance.OnDoorSyncReceived += HandleDoorSync;
            SteamNetManager.Instance.OnEconomySyncReceived += HandleEconomySync;
            SteamNetManager.Instance.OnUpgradeSyncReceived += HandleUpgradeSync;
            SteamNetManager.Instance.OnSackRippedReceived += HandleSackRipped;
            SteamNetManager.Instance.OnPotatoCleanReceived += HandlePotatoClean;
            SteamNetManager.Instance.OnQuestBumpReceived += HandleQuestBump;
            SteamNetManager.Instance.OnQuestFlagReceived += HandleQuestFlag;
            SteamNetManager.Instance.OnQuestPageReceived += HandleQuestPage;
        }

        public void SendDoorState(int doorId, bool isOpen, bool isSliding = false)
        {
            if (IsSyncingDoor) return;
            byte[] pkt = PacketSerializer.SerializeDoorSync(doorId, isOpen, isSliding);
            SteamNetManager.Instance.BroadcastPacket(pkt, true);
        }

        public void SendEconomyUpdate(long moneyTotal)
        {
            if (IsSyncingEconomy) return;
            byte[] pkt = PacketSerializer.SerializeEconomySync(moneyTotal);
            SteamNetManager.Instance.BroadcastPacket(pkt, true);
        }

        public void SendUpgradeUpdate(int upgradeId, int level)
        {
            if (IsSyncingUpgrade) return;
            byte[] pkt = PacketSerializer.SerializeUpgradeSync(upgradeId, level);
            SteamNetManager.Instance.BroadcastPacket(pkt, true);
        }

        public void SendSackRipped(int sackIndex)
        {
            byte[] pkt = PacketSerializer.SerializeSackRipped(sackIndex);
            SteamNetManager.Instance.BroadcastPacket(pkt, true);
        }

        public void SendPotatoClean(int workstationIndex)
        {
            byte[] pkt = PacketSerializer.SerializePotatoClean(workstationIndex);
            SteamNetManager.Instance.BroadcastPacket(pkt, true);
        }

        public void SendQuestBump(int kind, long amount)
        {
            if (IsSyncingQuest) return;
            byte[] pkt = PacketSerializer.SerializeQuestBump(kind, amount);
            SteamNetManager.Instance.BroadcastPacket(pkt, true);
        }

        public void SendQuestFlag(string flagId)
        {
            if (IsSyncingQuest) return;
            byte[] pkt = PacketSerializer.SerializeQuestFlag(flagId);
            SteamNetManager.Instance.BroadcastPacket(pkt, true);
        }

        public void SendQuestPage(int page)
        {
            if (IsSyncingQuest) return;
            byte[] pkt = PacketSerializer.SerializeQuestPage(page);
            SteamNetManager.Instance.BroadcastPacket(pkt, true);
        }

        private void HandleMachineSync(int machineId, float crankAngle, bool isRunning, int peeledCount)
        {
            try
            {
                var machine = GameObject.FindObjectOfType<PotatoPeelMachine>();
                if (machine != null)
                {
                    // Machine sync
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WorldSync] Error syncing machine: {ex.Message}");
            }
        }

        private void HandleDoorSync(int doorId, bool isOpen, bool isSliding)
        {
            try
            {
                IsSyncingDoor = true;
                if (isSliding)
                {
                    var slidingDoors = GameObject.FindObjectsOfType<SlidingDoorInteractable>();
                    if (slidingDoors != null && doorId >= 0 && doorId < slidingDoors.Length)
                    {
                        if (isOpen) slidingDoors[doorId].Open();
                        else slidingDoors[doorId].Close();
                    }
                }
                else
                {
                    var hingedDoors = GameObject.FindObjectsOfType<HingedDoor>();
                    if (hingedDoors != null && doorId >= 0 && doorId < hingedDoors.Length)
                    {
                        hingedDoors[doorId].SetOpen(isOpen, false);
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WorldSync] Error syncing door: {ex.Message}");
            }
            finally
            {
                IsSyncingDoor = false;
            }
        }

        private void HandleEconomySync(long moneyTotal)
        {
            try
            {
                var eco = EconomyManager.Instance ?? GameObject.FindObjectOfType<EconomyManager>();
                if (eco != null)
                {
                    IsSyncingEconomy = true;
                    eco.SetBalance(moneyTotal);
                    MelonLogger.Msg($"[WorldSync] Shared balance updated to: {moneyTotal} ₽");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WorldSync] Error syncing economy: {ex.Message}");
            }
            finally
            {
                IsSyncingEconomy = false;
            }
        }

        private void HandleUpgradeSync(int upgradeId, int level)
        {
            try
            {
                var upMgr = UpgradeManager.Instance ?? GameObject.FindObjectOfType<UpgradeManager>();
                if (upMgr != null)
                {
                    IsSyncingUpgrade = true;
                    upMgr.DebugSetLevel((UpgradeId)upgradeId, level);
                    upMgr.ApplyAll();
                    MelonLogger.Msg($"[WorldSync] Shared upgrade unlocked: {(UpgradeId)upgradeId} -> Lvl {level}!");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WorldSync] Error syncing upgrade: {ex.Message}");
            }
            finally
            {
                IsSyncingUpgrade = false;
            }
        }

        private void HandleSackRipped(int sackIndex)
        {
            try
            {
                IsSyncingSack = true;
                var sacks = GameObject.FindObjectsOfType<PatatoSackInteractable>();
                if (sacks != null && sackIndex >= 0 && sackIndex < sacks.Length)
                {
                    var sack = sacks[sackIndex];
                    if (sack != null && !sack.RipStarted)
                    {
                        sack.Discard();
                        MelonLogger.Msg($"[WorldSync] Synced ripped potato sack #{sackIndex}!");
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WorldSync] Error syncing sack: {ex.Message}");
            }
            finally
            {
                IsSyncingSack = false;
            }
        }

        private void HandlePotatoClean(int workstationIndex)
        {
            try
            {
                var stations = GameObject.FindObjectsOfType<Workstation>();
                if (stations != null && workstationIndex >= 0 && workstationIndex < stations.Length)
                {
                    var ws = stations[workstationIndex];
                    if (ws != null)
                    {
                        var potato = ws.GetComponentInChildren<PotatoInstance>();
                        if (potato != null)
                        {
                            ws.PlaceClean(potato, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WorldSync] Error syncing clean potato: {ex.Message}");
            }
        }

        private void HandleQuestBump(int kind, long amount)
        {
            try
            {
                IsSyncingQuest = true;
                var qm = QuestManager.Instance ?? GameObject.FindObjectOfType<QuestManager>();
                if (qm != null)
                {
                    qm.Bump((ObjectiveKind)kind, amount);
                    MelonLogger.Msg($"[WorldSync] Synced quest bump: (ObjectiveKind){kind} +{amount}");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WorldSync] Error syncing quest bump: {ex.Message}");
            }
            finally
            {
                IsSyncingQuest = false;
            }
        }

        private void HandleQuestFlag(string flagId)
        {
            try
            {
                IsSyncingQuest = true;
                var qm = QuestManager.Instance ?? GameObject.FindObjectOfType<QuestManager>();
                if (qm != null)
                {
                    qm.ReportFlag(flagId);
                    MelonLogger.Msg($"[WorldSync] Synced quest flag: {flagId}");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WorldSync] Error syncing quest flag: {ex.Message}");
            }
            finally
            {
                IsSyncingQuest = false;
            }
        }

        private void HandleQuestPage(int page)
        {
            try
            {
                IsSyncingQuest = true;
                var qm = QuestManager.Instance ?? GameObject.FindObjectOfType<QuestManager>();
                if (qm != null && qm.ActiveIndex != page)
                {
                    qm.ActivatePage(page);
                    MelonLogger.Msg($"[WorldSync] Synced quest page: {page}");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WorldSync] Error syncing quest page: {ex.Message}");
            }
            finally
            {
                IsSyncingQuest = false;
            }
        }
    }

    // --- HARMONY PATCHES FOR INSTANT GAMEPLAY SYNCHRONIZATION ---

    [HarmonyPatch(typeof(EconomyManager), "set_Balance")]
    public static class Patch_EconomyManager_SetBalance
    {
        public static void Postfix(EconomyManager __instance, long value)
        {
            if (WorldSync.IsSyncingEconomy) return;
            if (SteamNetManager.Instance != null && (SteamNetManager.Instance.IsConnected || SteamNetManager.Instance.IsHost))
            {
                WorldSync.Instance?.SendEconomyUpdate(value);
            }
        }
    }

    [HarmonyPatch(typeof(UpgradeManager), "TryPurchase")]
    public static class Patch_UpgradeManager_TryPurchase
    {
        public static void Postfix(UpgradeManager __instance, UpgradeId id, bool __result)
        {
            if (WorldSync.IsSyncingUpgrade) return;
            if (__result && SteamNetManager.Instance != null && (SteamNetManager.Instance.IsConnected || SteamNetManager.Instance.IsHost))
            {
                int newLevel = __instance.GetLevel(id);
                WorldSync.Instance?.SendUpgradeUpdate((int)id, newLevel);
            }
        }
    }

    [HarmonyPatch(typeof(HingedDoor), "SetOpen")]
    public static class Patch_HingedDoor_SetOpen
    {
        public static void Postfix(HingedDoor __instance, bool open)
        {
            if (WorldSync.IsSyncingDoor) return;
            if (SteamNetManager.Instance != null && (SteamNetManager.Instance.IsConnected || SteamNetManager.Instance.IsHost))
            {
                var doors = GameObject.FindObjectsOfType<HingedDoor>();
                if (doors != null)
                {
                    for (int i = 0; i < doors.Length; i++)
                    {
                        if (doors[i] == __instance)
                        {
                            WorldSync.Instance?.SendDoorState(i, open, false);
                            break;
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(SlidingDoorInteractable), "Open")]
    public static class Patch_SlidingDoor_Open
    {
        public static void Postfix(SlidingDoorInteractable __instance)
        {
            if (WorldSync.IsSyncingDoor) return;
            SyncSlidingDoor(__instance, true);
        }

        private static void SyncSlidingDoor(SlidingDoorInteractable door, bool open)
        {
            if (SteamNetManager.Instance != null && (SteamNetManager.Instance.IsConnected || SteamNetManager.Instance.IsHost))
            {
                var doors = GameObject.FindObjectsOfType<SlidingDoorInteractable>();
                if (doors != null)
                {
                    for (int i = 0; i < doors.Length; i++)
                    {
                        if (doors[i] == door)
                        {
                            WorldSync.Instance?.SendDoorState(i, open, true);
                            break;
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(SlidingDoorInteractable), "Close")]
    public static class Patch_SlidingDoor_Close
    {
        public static void Postfix(SlidingDoorInteractable __instance)
        {
            if (WorldSync.IsSyncingDoor) return;
            if (SteamNetManager.Instance != null && (SteamNetManager.Instance.IsConnected || SteamNetManager.Instance.IsHost))
            {
                var doors = GameObject.FindObjectsOfType<SlidingDoorInteractable>();
                if (doors != null)
                {
                    for (int i = 0; i < doors.Length; i++)
                    {
                        if (doors[i] == __instance)
                        {
                            WorldSync.Instance?.SendDoorState(i, false, true);
                            break;
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PatatoSackInteractable), "OnInteract")]
    public static class Patch_PatatoSack_OnInteract
    {
        public static void Postfix(PatatoSackInteractable __instance)
        {
            if (SteamNetManager.Instance != null && (SteamNetManager.Instance.IsConnected || SteamNetManager.Instance.IsHost))
            {
                var sacks = GameObject.FindObjectsOfType<PatatoSackInteractable>();
                if (sacks != null)
                {
                    for (int i = 0; i < sacks.Length; i++)
                    {
                        if (sacks[i] == __instance)
                        {
                            WorldSync.Instance?.SendSackRipped(i);
                            break;
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Workstation), "PlaceClean")]
    public static class Patch_Workstation_PlaceClean
    {
        public static void Postfix(Workstation __instance)
        {
            if (SteamNetManager.Instance != null && (SteamNetManager.Instance.IsConnected || SteamNetManager.Instance.IsHost))
            {
                var stations = GameObject.FindObjectsOfType<Workstation>();
                if (stations != null)
                {
                    for (int i = 0; i < stations.Length; i++)
                    {
                        if (stations[i] == __instance)
                        {
                            WorldSync.Instance?.SendPotatoClean(i);
                            break;
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PatatoSackInteractable), "Discard")]
    public static class Patch_PatatoSack_Discard
    {
        public static void Postfix(PatatoSackInteractable __instance)
        {
            if (WorldSync.IsSyncingSack) return;
            if (SteamNetManager.Instance != null && (SteamNetManager.Instance.IsConnected || SteamNetManager.Instance.IsHost))
            {
                var sacks = GameObject.FindObjectsOfType<PatatoSackInteractable>();
                if (sacks != null)
                {
                    for (int i = 0; i < sacks.Length; i++)
                    {
                        if (sacks[i] == __instance)
                        {
                            WorldSync.Instance?.SendSackRipped(i);
                            break;
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(QuestManager), "Bump")]
    public static class Patch_QuestManager_Bump
    {
        public static void Postfix(ObjectiveKind kind, long amount)
        {
            if (WorldSync.IsSyncingQuest) return;
            if (SteamNetManager.Instance != null && (SteamNetManager.Instance.IsConnected || SteamNetManager.Instance.IsHost))
            {
                WorldSync.Instance?.SendQuestBump((int)kind, amount);
            }
        }
    }

    [HarmonyPatch(typeof(QuestManager), "ReportFlag")]
    public static class Patch_QuestManager_ReportFlag
    {
        public static void Postfix(string flagId)
        {
            if (WorldSync.IsSyncingQuest) return;
            if (SteamNetManager.Instance != null && (SteamNetManager.Instance.IsConnected || SteamNetManager.Instance.IsHost))
            {
                WorldSync.Instance?.SendQuestFlag(flagId);
            }
        }
    }

    [HarmonyPatch(typeof(QuestManager), "ActivatePage")]
    public static class Patch_QuestManager_ActivatePage
    {
        public static void Postfix(int index)
        {
            if (WorldSync.IsSyncingQuest) return;
            if (SteamNetManager.Instance != null && SteamNetManager.Instance.IsHost)
            {
                WorldSync.Instance?.SendQuestPage(index);
            }
        }
    }
}
