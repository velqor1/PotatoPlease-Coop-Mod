using System;
using System.Collections.Generic;
using UnityEngine;
using PotatoCoop.Network;
using MelonLoader;

namespace PotatoCoop.Puppet
{
    public class PuppetManager
    {
        public static PuppetManager Instance { get; private set; }

        private readonly Dictionary<ulong, RemotePuppet> _puppets = new Dictionary<ulong, RemotePuppet>();

        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new PuppetManager();
                Instance.RegisterEvents();
            }
        }

        private void RegisterEvents()
        {
            SteamNetManager.Instance.OnPlayerTransformReceived += HandlePlayerTransform;
            SteamNetManager.Instance.OnPeerJoined += HandlePeerJoined;
            SteamNetManager.Instance.OnPeerLeft += HandlePeerLeft;
        }

        private void HandlePeerJoined(ulong steamId, string name, int skinIndex)
        {
            GetOrCreatePuppet(steamId, name, skinIndex);
        }

        private void HandlePlayerTransform(ulong steamId, Vector3 pos, float yaw, float pitch, float speed, bool isCrouching, int toolId, bool isInteracting)
        {
            RemotePuppet puppet = GetOrCreatePuppet(steamId);
            if (puppet != null)
            {
                puppet.UpdateTransform(pos, yaw, pitch, speed, isCrouching, toolId, isInteracting);
            }
        }

        private RemotePuppet GetOrCreatePuppet(ulong steamId, string name = null, int skinIndex = 1)
        {
            if (!_puppets.TryGetValue(steamId, out RemotePuppet puppet) || puppet == null)
            {
                puppet = new RemotePuppet();

                string pName = name ?? (SteamNetManager.Instance.PeerNames.TryGetValue(steamId, out string n) ? n : "Worker");
                int sIndex = skinIndex != 1 ? skinIndex : (SteamNetManager.Instance.PeerSkins.TryGetValue(steamId, out int s) ? s : 1);

                puppet.Initialize(steamId, pName, sIndex);
                _puppets[steamId] = puppet;

                MelonLogger.Msg($"[PuppetManager] Spawned puppet for {pName} (SteamID: {steamId})");
            }
            return puppet;
        }

        private void HandlePeerLeft(ulong steamId)
        {
            if (_puppets.TryGetValue(steamId, out RemotePuppet puppet))
            {
                puppet?.Destroy();
                _puppets.Remove(steamId);
                MelonLogger.Msg($"[PuppetManager] Removed puppet for SteamID: {steamId}");
            }
        }

        public void Update()
        {
            float dt = Time.deltaTime;

            // Check if local player is actively peeling a potato on the cutting board
            bool isLocalPeeling = false;
            try
            {
                var session = Il2CppPotatoPeeler.PeelSession.Instance;
                if (session != null && session.IsActive)
                {
                    isLocalPeeling = true;
                }
            }
            catch { }

            foreach (var puppet in _puppets.Values)
            {
                if (puppet == null || puppet.RootGameObject == null) continue;

                // When local player is in peeling mode, hide puppets so they never obstruct the cutting board
                if (puppet.RootGameObject.activeSelf == isLocalPeeling)
                {
                    puppet.RootGameObject.SetActive(!isLocalPeeling);
                }

                puppet.Update(dt);
            }
        }

        public void ClearAll()
        {
            foreach (var puppet in _puppets.Values)
            {
                puppet?.Destroy();
            }
            _puppets.Clear();
        }
    }
}
