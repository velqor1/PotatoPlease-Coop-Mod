using System;
using UnityEngine;
using PotatoCoop.Network;
using Il2CppPotatoPeeler;
using MelonLoader;

namespace PotatoCoop.Sync
{
    public class QuestSync
    {
        public static QuestSync Instance { get; private set; }
        private bool _isApplyingRemoteSync = false;

        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new QuestSync();
                Instance.RegisterEvents();
            }
        }

        private void RegisterEvents()
        {
            SteamNetManager.Instance.OnQuestSyncReceived += HandleQuestSyncReceived;
        }

        public void HookQuestManager(QuestManager qm)
        {
            if (qm == null) return;

            try
            {
                qm.add_OnProgressChanged((Action)(() =>
                {
                    if (_isApplyingRemoteSync) return;
                    if (SteamNetManager.Instance.IsHost)
                    {
                        BroadcastCurrentQuestState(qm);
                    }
                }));

                qm.add_OnActiveQuestChanged((Action)(() =>
                {
                    if (_isApplyingRemoteSync) return;
                    if (SteamNetManager.Instance.IsHost)
                    {
                        BroadcastCurrentQuestState(qm);
                    }
                }));

                qm.add_OnQuestCompleted((Action<QuestDefinition>)((def) =>
                {
                    if (_isApplyingRemoteSync) return;
                    if (SteamNetManager.Instance.IsHost)
                    {
                        BroadcastCurrentQuestState(qm);
                    }
                }));

                MelonLogger.Msg("[QuestSync] Hooked QuestManager events for multiplayer synchronization.");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[QuestSync] Could not hook QuestManager events directly: {ex.Message}");
            }
        }

        public void BroadcastCurrentQuestState(QuestManager qm)
        {
            if (qm == null) return;

            try
            {
                int activeIdx = qm.ActiveIndex;
                byte[] pkt = PacketSerializer.SerializeQuestSync(activeIdx, null, null);
                SteamNetManager.Instance.BroadcastPacket(pkt, true);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[QuestSync] Error broadcasting quest state: {ex.Message}");
            }
        }

        private void HandleQuestSyncReceived(int activeIndex, long[] progress, bool[] done)
        {
            if (SteamNetManager.Instance.IsHost) return;

            try
            {
                _isApplyingRemoteSync = true;
                QuestManager qm = GameObject.FindObjectOfType<QuestManager>();
                if (qm != null)
                {
                    if (qm.ActiveIndex != activeIndex)
                    {
                        MelonLogger.Msg($"[QuestSync] Synced quest active index to: {activeIndex}");
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[QuestSync] Error handling quest sync: {ex.Message}");
            }
            finally
            {
                _isApplyingRemoteSync = false;
            }
        }
    }
}
