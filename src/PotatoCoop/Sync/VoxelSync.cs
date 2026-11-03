using System;
using UnityEngine;
using PotatoCoop.Network;
using Il2CppPotatoPeeler;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;

namespace PotatoCoop.Sync
{
    public class VoxelSync
    {
        public static VoxelSync Instance { get; private set; }
        private bool _isApplyingRemoteCut = false;

        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new VoxelSync();
                Instance.RegisterEvents();
            }
        }

        private void RegisterEvents()
        {
            SteamNetManager.Instance.OnVoxelCutReceived += HandleVoxelCutReceived;
        }

        public void OnLocalVoxelCut(Vector3 localPoint, float radius, float cutDelta)
        {
            if (_isApplyingRemoteCut) return;

            byte[] pkt = PacketSerializer.SerializeVoxelCut(localPoint, radius, cutDelta);
            SteamNetManager.Instance.BroadcastPacket(pkt, false);
        }

        private void HandleVoxelCutReceived(Vector3 localPoint, float radius, float cutDelta)
        {
            _isApplyingRemoteCut = true;
            try
            {
                VoxelPotato potato = GameObject.FindObjectOfType<VoxelPotato>();
                if (potato != null)
                {
                    BrushOp op = BrushOp.Capsule(new Il2CppStructArray<Vector3>(new Vector3[] { localPoint }), radius);
                    potato.ApplyBrush(ref op, true);
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[VoxelSync] Error applying remote voxel cut: {ex.Message}");
            }
            finally
            {
                _isApplyingRemoteCut = false;
            }
        }
    }
}
