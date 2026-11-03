using System;
using UnityEngine;
using PotatoCoop.Network;
using Il2CppPotatoPeeler;
using MelonLoader;

namespace PotatoCoop.Sync
{
    public class WorldSync
    {
        public static WorldSync Instance { get; private set; }

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
        }

        public void SendMachineUpdate(int machineId, float crankAngle, bool isRunning, int peeledCount)
        {
            byte[] pkt = PacketSerializer.SerializeMachineSync(machineId, crankAngle, isRunning, peeledCount);
            SteamNetManager.Instance.BroadcastPacket(pkt, false);
        }

        public void SendDoorState(int doorId, bool isOpen)
        {
            byte[] pkt = PacketSerializer.SerializeDoorSync(doorId, isOpen);
            SteamNetManager.Instance.BroadcastPacket(pkt, true);
        }

        public void SendEconomyUpdate(int moneyTotal)
        {
            byte[] pkt = PacketSerializer.SerializeEconomySync(moneyTotal);
            SteamNetManager.Instance.BroadcastPacket(pkt, true);
        }

        private void HandleMachineSync(int machineId, float crankAngle, bool isRunning, int peeledCount)
        {
            try
            {
                PotatoPeelMachine machine = GameObject.FindObjectOfType<PotatoPeelMachine>();
                if (machine != null)
                {
                    // Sync visual state if needed
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WorldSync] Error syncing machine: {ex.Message}");
            }
        }

        private void HandleDoorSync(int doorId, bool isOpen)
        {
            try
            {
                var doors = GameObject.FindObjectsOfType<HingedDoor>();
                if (doors != null && doorId >= 0 && doorId < doors.Length)
                {
                    // Sync door if needed
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WorldSync] Error syncing door: {ex.Message}");
            }
        }

        private void HandleEconomySync(int moneyTotal)
        {
            try
            {
                EconomyManager eco = GameObject.FindObjectOfType<EconomyManager>();
                if (eco != null)
                {
                    MelonLogger.Msg($"[WorldSync] Synced shared balance: {moneyTotal} ₽");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WorldSync] Error syncing economy: {ex.Message}");
            }
        }
    }
}
