using System;
using System.Collections.Generic;
using System.IO;
using Il2CppSteamworks;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using MelonLoader;

namespace PotatoCoop.Network
{
    public class SteamNetManager
    {
        public static SteamNetManager Instance { get; private set; }

        public bool IsHost { get; set; } = false;
        public bool IsConnected { get; set; } = false;
        public CSteamID HostSteamId { get; set; } = CSteamID.Nil;
        public CSteamID CurrentLobbyId { get; set; } = CSteamID.Nil;
        public CSteamID LocalSteamId { get; private set; }

        public string LocalPlayerName { get; private set; } = "Worker";
        public int LocalSkinIndex { get; set; } = 0;
        public int LocalAssignedSlot { get; set; } = 0;

        public readonly List<CSteamID> ConnectedPeers = new List<CSteamID>();
        public readonly Dictionary<ulong, string> PeerNames = new Dictionary<ulong, string>();
        public readonly Dictionary<ulong, int> PeerSkins = new Dictionary<ulong, int>();

        public event Action<ulong, Vector3, float, float, float, bool, int, bool> OnPlayerTransformReceived;
        public event Action<int, long[], bool[]> OnQuestSyncReceived;
        public event Action<Vector3, float, float> OnVoxelCutReceived;
        public event Action<int, float, bool, int> OnMachineSyncReceived;
        public event Action<int, bool> OnDoorSyncReceived;
        public event Action<int> OnEconomySyncReceived;
        public event Action<ulong, string, int> OnPeerJoined;
        public event Action<ulong> OnPeerLeft;

        private Callback<P2PSessionRequest_t> _p2pSessionRequestCallback;
        private Callback<P2PSessionConnectFail_t> _p2pSessionConnectFailCallback;

        public bool IsSteamInitialized { get; private set; } = false;

        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new SteamNetManager();
                Instance.InitSteam();
            }
        }

        private void InitSteam()
        {
            try
            {
                Environment.SetEnvironmentVariable("SteamAppId", "480");
                Environment.SetEnvironmentVariable("SteamGameId", "480");

                bool initialized = SteamAPI.Init();
                if (initialized && SteamAPI.IsSteamRunning())
                {
                    IsSteamInitialized = true;
                    LocalSteamId = SteamUser.GetSteamID();
                    LocalPlayerName = SteamFriends.GetPersonaName();
                    MelonLogger.Msg($"[SteamNet] Logged into Steam as: {LocalPlayerName} (SteamID: {LocalSteamId.m_SteamID})");

                    _p2pSessionRequestCallback = Callback<P2PSessionRequest_t>.Create((Action<P2PSessionRequest_t>)OnP2PSessionRequest);
                    _p2pSessionConnectFailCallback = Callback<P2PSessionConnectFail_t>.Create((Action<P2PSessionConnectFail_t>)OnP2PSessionConnectFail);
                }
                else
                {
                    IsSteamInitialized = false;
                    MelonLogger.Warning("[SteamNet] Steam is not running or SteamAPI could not be initialized (AppID 480). Check if Steam client is open and Spacewar is registered via steam://run/480.");
                }
            }
            catch (Exception ex)
            {
                IsSteamInitialized = false;
                MelonLogger.Error($"[SteamNet] Error initializing Steam: {ex.Message}");
            }
        }

        private void OnP2PSessionRequest(P2PSessionRequest_t req)
        {
            MelonLogger.Msg($"[SteamNet] Incoming P2P session request from {req.m_steamIDRemote.m_SteamID}. Accepting...");
            SteamNetworking.AcceptP2PSessionWithUser(req.m_steamIDRemote);
        }

        private void OnP2PSessionConnectFail(P2PSessionConnectFail_t fail)
        {
            MelonLogger.Warning($"[SteamNet] P2P session connection failure with {fail.m_steamIDRemote.m_SteamID}: Error code {fail.m_eP2PSessionError}");
        }

        public void Update()
        {
            try
            {
                SteamAPI.RunCallbacks();
            }
            catch { }

            if (!IsConnected && ConnectedPeers.Count == 0 && HostSteamId == CSteamID.Nil)
                return;

            uint msgSize;
            while (SteamNetworking.IsP2PPacketAvailable(out msgSize, 0))
            {
                var buffer = new Il2CppStructArray<byte>(msgSize);
                uint bytesRead;
                CSteamID remoteSteamId;

                if (SteamNetworking.ReadP2PPacket(buffer, msgSize, out bytesRead, out remoteSteamId, 0))
                {
                    byte[] data = ((ReadOnlySpan<byte>)buffer).ToArray();
                    ProcessPacket(remoteSteamId, data);
                }
            }
        }

        public void SendPacketTo(CSteamID recipient, byte[] packet, bool reliable = true)
        {
            if (recipient == CSteamID.Nil || packet == null || packet.Length == 0) return;
            EP2PSend sendType = reliable ? EP2PSend.k_EP2PSendReliable : EP2PSend.k_EP2PSendUnreliableNoDelay;
            SteamNetworking.SendP2PPacket(recipient, (Il2CppStructArray<byte>)packet, (uint)packet.Length, sendType, 0);
        }

        public void BroadcastPacket(byte[] packet, bool reliable = true)
        {
            if (packet == null || packet.Length == 0) return;
            if (IsHost)
            {
                for (int i = 0; i < ConnectedPeers.Count; i++)
                {
                    SendPacketTo(ConnectedPeers[i], packet, reliable);
                }
            }
            else if (HostSteamId != CSteamID.Nil)
            {
                SendPacketTo(HostSteamId, packet, reliable);
            }
        }

        public void SendHandshake(CSteamID target)
        {
            byte[] hs = PacketSerializer.SerializeHandshake(LocalSteamId.m_SteamID, LocalPlayerName, LocalSkinIndex);
            SendPacketTo(target, hs, true);
        }

        private void ProcessPacket(CSteamID sender, byte[] data)
        {
            if (data == null || data.Length == 0) return;

            try
            {
                using var ms = new MemoryStream(data);
                using var reader = new BinaryReader(ms);
                PacketType pType = (PacketType)reader.ReadByte();

                switch (pType)
                {
                    case PacketType.Handshake:
                    {
                        ulong steamId = reader.ReadUInt64();
                        string name = reader.ReadString();
                        int skinIndex = reader.ReadInt32();

                        if (!ConnectedPeers.Contains(sender))
                            ConnectedPeers.Add(sender);

                        PeerNames[sender.m_SteamID] = name;
                        PeerSkins[sender.m_SteamID] = skinIndex;

                        MelonLogger.Msg($"[SteamNet] Peer joined: {name} (SteamID: {steamId}, Skin: {skinIndex})");
                        OnPeerJoined?.Invoke(sender.m_SteamID, name, skinIndex);

                        if (IsHost)
                        {
                            // Send Ack back to this peer
                            int assignedSlot = ConnectedPeers.Count;
                            byte[] ack = PacketSerializer.SerializeHandshakeAck(assignedSlot, 0, 0);
                            SendPacketTo(sender, ack, true);

                            // Broadcast this new peer to other peers
                            for (int i = 0; i < ConnectedPeers.Count; i++)
                            {
                                if (ConnectedPeers[i] != sender)
                                {
                                    byte[] joinPkt = PacketSerializer.SerializeHandshake(steamId, name, skinIndex);
                                    SendPacketTo(ConnectedPeers[i], joinPkt, true);
                                }
                            }
                        }
                        break;
                    }

                    case PacketType.HandshakeAck:
                    {
                        int slot = reader.ReadInt32();
                        int questIdx = reader.ReadInt32();
                        int money = reader.ReadInt32();
                        LocalAssignedSlot = slot;
                        IsConnected = true;
                        MelonLogger.Msg($"[SteamNet] Connected to Host! Assigned Slot: {slot}");
                        break;
                    }

                    case PacketType.PlayerTransform:
                    {
                        float px = reader.ReadSingle();
                        float py = reader.ReadSingle();
                        float pz = reader.ReadSingle();
                        float yaw = reader.ReadSingle();
                        float pitch = reader.ReadSingle();
                        float spd = reader.ReadSingle();
                        bool crouch = reader.ReadBoolean();
                        int tool = reader.ReadInt32();
                        bool interact = reader.ReadBoolean();

                        OnPlayerTransformReceived?.Invoke(sender.m_SteamID, new Vector3(px, py, pz), yaw, pitch, spd, crouch, tool, interact);

                        if (IsHost)
                        {
                            for (int i = 0; i < ConnectedPeers.Count; i++)
                            {
                                if (ConnectedPeers[i] != sender)
                                {
                                    SendPacketTo(ConnectedPeers[i], data, false);
                                }
                            }
                        }
                        break;
                    }

                    case PacketType.QuestSync:
                    {
                        int qIdx = reader.ReadInt32();
                        int pLen = reader.ReadInt32();
                        long[] prog = new long[pLen];
                        for (int i = 0; i < pLen; i++) prog[i] = reader.ReadInt64();
                        int dLen = reader.ReadInt32();
                        bool[] done = new bool[dLen];
                        for (int i = 0; i < dLen; i++) done[i] = reader.ReadBoolean();

                        OnQuestSyncReceived?.Invoke(qIdx, prog, done);

                        if (IsHost)
                        {
                            for (int i = 0; i < ConnectedPeers.Count; i++)
                            {
                                if (ConnectedPeers[i] != sender)
                                    SendPacketTo(ConnectedPeers[i], data, true);
                            }
                        }
                        break;
                    }

                    case PacketType.VoxelCut:
                    {
                        float cx = reader.ReadSingle();
                        float cy = reader.ReadSingle();
                        float cz = reader.ReadSingle();
                        float rad = reader.ReadSingle();
                        float dlt = reader.ReadSingle();

                        OnVoxelCutReceived?.Invoke(new Vector3(cx, cy, cz), rad, dlt);

                        if (IsHost)
                        {
                            for (int i = 0; i < ConnectedPeers.Count; i++)
                            {
                                if (ConnectedPeers[i] != sender)
                                    SendPacketTo(ConnectedPeers[i], data, true);
                            }
                        }
                        break;
                    }

                    case PacketType.MachineSync:
                    {
                        int mId = reader.ReadInt32();
                        float angle = reader.ReadSingle();
                        bool run = reader.ReadBoolean();
                        int cnt = reader.ReadInt32();

                        OnMachineSyncReceived?.Invoke(mId, angle, run, cnt);

                        if (IsHost)
                        {
                            for (int i = 0; i < ConnectedPeers.Count; i++)
                            {
                                if (ConnectedPeers[i] != sender)
                                    SendPacketTo(ConnectedPeers[i], data, true);
                            }
                        }
                        break;
                    }

                    case PacketType.DoorSync:
                    {
                        int dId = reader.ReadInt32();
                        bool op = reader.ReadBoolean();

                        OnDoorSyncReceived?.Invoke(dId, op);

                        if (IsHost)
                        {
                            for (int i = 0; i < ConnectedPeers.Count; i++)
                            {
                                if (ConnectedPeers[i] != sender)
                                    SendPacketTo(ConnectedPeers[i], data, true);
                            }
                        }
                        break;
                    }

                    case PacketType.EconomySync:
                    {
                        int total = reader.ReadInt32();
                        OnEconomySyncReceived?.Invoke(total);

                        if (IsHost)
                        {
                            for (int i = 0; i < ConnectedPeers.Count; i++)
                            {
                                if (ConnectedPeers[i] != sender)
                                    SendPacketTo(ConnectedPeers[i], data, true);
                            }
                        }
                        break;
                    }

                    case PacketType.PlayerLeave:
                    {
                        ConnectedPeers.Remove(sender);
                        PeerNames.Remove(sender.m_SteamID);
                        PeerSkins.Remove(sender.m_SteamID);
                        OnPeerLeft?.Invoke(sender.m_SteamID);
                        break;
                    }

                    case PacketType.StartShift:
                    {
                        int difficulty = reader.ReadInt32();
                        MelonLogger.Msg($"[SteamNet] Received StartShift from Host! Starting game session (Diff: {difficulty})...");
                        if (UI.LobbyUI.Instance != null)
                        {
                            UI.LobbyUI.Instance.IsVisible = false;
                        }

                        var mmc = GameObject.FindObjectOfType<Il2CppPotatoPeeler.MainMenuController>();
                        if (mmc != null)
                        {
                            mmc.BeginShift();
                        }
                        else
                        {
                            UnityEngine.SceneManagement.SceneManager.LoadScene("level0");
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[SteamNet] Error parsing packet from {sender.m_SteamID}: {ex.Message}");
            }
        }

        public void BroadcastStartShift()
        {
            byte[] pkt = PacketSerializer.SerializeStartShift(0);
            BroadcastPacket(pkt, true);
            MelonLogger.Msg("[SteamNet] Broadcasted StartShift packet to all connected peers!");
        }

        public void RemovePeer(ulong steamId)
        {
            CSteamID target = new CSteamID(steamId);
            ConnectedPeers.Remove(target);
            PeerNames.Remove(steamId);
            PeerSkins.Remove(steamId);
            OnPeerLeft?.Invoke(steamId);
            MelonLogger.Msg($"[SteamNet] Peer removed: {steamId}");
        }

        public void Disconnect()
        {
            if (IsConnected || IsHost)
            {
                byte[] leave = new byte[] { (byte)PacketType.PlayerLeave };
                BroadcastPacket(leave, true);
            }

            for (int i = 0; i < ConnectedPeers.Count; i++)
            {
                SteamNetworking.CloseP2PSessionWithUser(ConnectedPeers[i]);
            }

            if (HostSteamId != CSteamID.Nil)
            {
                SteamNetworking.CloseP2PSessionWithUser(HostSteamId);
            }

            ConnectedPeers.Clear();
            PeerNames.Clear();
            PeerSkins.Clear();
            IsHost = false;
            IsConnected = false;
            HostSteamId = CSteamID.Nil;
            CurrentLobbyId = CSteamID.Nil;
            MelonLogger.Msg("[SteamNet] Network session closed.");
        }

        public void DisconnectAll()
        {
            Disconnect();
        }
    }
}
