using System;
using System.Collections.Generic;
using Il2CppSteamworks;
using MelonLoader;

namespace PotatoCoop.Network
{
    public class LobbyInfo
    {
        public CSteamID LobbyId;
        public string HostName;
        public int MemberCount;
        public int MaxMembers;
    }

    public class LobbyManager
    {
        public static LobbyManager Instance { get; private set; }

        public const string LOBBY_GAME_KEY = "potato_please_coop";
        public const string LOBBY_GAME_VAL = "1.0";

        public event Action<LobbyInfo> OnLobbyCreatedSuccess;
        public event Action<LobbyInfo> OnLobbyJoinedSuccess;
        public event Action<List<LobbyInfo>> OnLobbyListUpdated;
        public event Action<string> OnLobbyError;

        private Callback<LobbyCreated_t> _lobbyCreatedCallback;
        private Callback<LobbyEnter_t> _lobbyEnterCallback;
        private Callback<LobbyMatchList_t> _lobbyMatchListCallback;
        private Callback<GameLobbyJoinRequested_t> _gameLobbyJoinRequestedCallback;

        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new LobbyManager();
                Instance.RegisterCallbacks();
            }
        }

        private void RegisterCallbacks()
        {
            try
            {
                _lobbyCreatedCallback = Callback<LobbyCreated_t>.Create((Action<LobbyCreated_t>)OnLobbyCreated);
                _lobbyEnterCallback = Callback<LobbyEnter_t>.Create((Action<LobbyEnter_t>)OnLobbyEntered);
                _lobbyMatchListCallback = Callback<LobbyMatchList_t>.Create((Action<LobbyMatchList_t>)OnLobbyMatchList);
                _gameLobbyJoinRequestedCallback = Callback<GameLobbyJoinRequested_t>.Create((Action<GameLobbyJoinRequested_t>)OnGameLobbyJoinRequested);
                MelonLogger.Msg("[LobbyManager] Steam lobby callbacks registered.");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LobbyManager] Failed to register callbacks: {ex.Message}");
            }
        }

        public void CreateLobby(bool friendsOnly = true)
        {
            ELobbyType lobbyType = friendsOnly ? ELobbyType.k_ELobbyTypeFriendsOnly : ELobbyType.k_ELobbyTypePublic;
            MelonLogger.Msg($"[LobbyManager] Creating Steam Lobby ({lobbyType})...");
            SteamMatchmaking.CreateLobby(lobbyType, 4);
        }

        private void OnLobbyCreated(LobbyCreated_t result)
        {
            if (result.m_eResult == EResult.k_EResultOK)
            {
                CSteamID lobbyId = new CSteamID(result.m_ulSteamIDLobby);
                SteamNetManager.Instance.CurrentLobbyId = lobbyId;
                SteamNetManager.Instance.IsHost = true;
                SteamNetManager.Instance.IsConnected = true;
                SteamNetManager.Instance.LocalAssignedSlot = 0;

                SteamMatchmaking.SetLobbyData(lobbyId, LOBBY_GAME_KEY, LOBBY_GAME_VAL);
                SteamMatchmaking.SetLobbyData(lobbyId, "HostName", SteamNetManager.Instance.LocalPlayerName);
                SteamMatchmaking.SetLobbyData(lobbyId, "HostSteamId", SteamNetManager.Instance.LocalSteamId.m_SteamID.ToString());

                MelonLogger.Msg($"[LobbyManager] Lobby created successfully! LobbyID: {lobbyId.m_SteamID}");

                var info = new LobbyInfo
                {
                    LobbyId = lobbyId,
                    HostName = SteamNetManager.Instance.LocalPlayerName,
                    MemberCount = 1,
                    MaxMembers = 4
                };
                OnLobbyCreatedSuccess?.Invoke(info);
            }
            else
            {
                MelonLogger.Error($"[LobbyManager] Failed to create lobby: {result.m_eResult}");
                OnLobbyError?.Invoke($"Не удалось создать лобби: {result.m_eResult}");
            }
        }

        public void JoinLobby(CSteamID lobbyId)
        {
            MelonLogger.Msg($"[LobbyManager] Joining lobby {lobbyId.m_SteamID}...");
            SteamMatchmaking.JoinLobby(lobbyId);
        }

        private void OnLobbyEntered(LobbyEnter_t result)
        {
            if (result.m_EChatRoomEnterResponse == (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
            {
                CSteamID lobbyId = new CSteamID(result.m_ulSteamIDLobby);
                SteamNetManager.Instance.CurrentLobbyId = lobbyId;

                string hostIdStr = SteamMatchmaking.GetLobbyData(lobbyId, "HostSteamId");
                string hostName = SteamMatchmaking.GetLobbyData(lobbyId, "HostName");

                if (ulong.TryParse(hostIdStr, out ulong hostId))
                {
                    SteamNetManager.Instance.HostSteamId = new CSteamID(hostId);
                    if (hostId == SteamNetManager.Instance.LocalSteamId.m_SteamID)
                    {
                        SteamNetManager.Instance.IsHost = true;
                    }
                    else
                    {
                        SteamNetManager.Instance.IsHost = false;
                        SteamNetManager.Instance.IsConnected = true;
                        SteamNetManager.Instance.SendHandshake(SteamNetManager.Instance.HostSteamId);
                    }
                }

                MelonLogger.Msg($"[LobbyManager] Entered lobby {lobbyId.m_SteamID}! Host: {hostName}");

                var info = new LobbyInfo
                {
                    LobbyId = lobbyId,
                    HostName = hostName,
                    MemberCount = SteamMatchmaking.GetNumLobbyMembers(lobbyId),
                    MaxMembers = 4
                };
                OnLobbyJoinedSuccess?.Invoke(info);
            }
            else
            {
                MelonLogger.Error($"[LobbyManager] Failed to enter lobby: error {result.m_EChatRoomEnterResponse}");
                OnLobbyError?.Invoke($"Ошибка входа в лобби: {result.m_EChatRoomEnterResponse}");
            }
        }

        public void RefreshLobbyList()
        {
            MelonLogger.Msg("[LobbyManager] Refreshing Steam lobby list...");
            SteamMatchmaking.AddRequestLobbyListStringFilter(LOBBY_GAME_KEY, LOBBY_GAME_VAL, ELobbyComparison.k_ELobbyComparisonEqual);
            SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
            SteamMatchmaking.RequestLobbyList();
        }

        private void OnLobbyMatchList(LobbyMatchList_t result)
        {
            var list = new List<LobbyInfo>();
            uint count = result.m_nLobbiesMatching;
            MelonLogger.Msg($"[LobbyManager] Lobbies found: {count}");

            for (int i = 0; i < count; i++)
            {
                CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
                string hostName = SteamMatchmaking.GetLobbyData(lobbyId, "HostName");
                int members = SteamMatchmaking.GetNumLobbyMembers(lobbyId);

                list.Add(new LobbyInfo
                {
                    LobbyId = lobbyId,
                    HostName = string.IsNullOrEmpty(hostName) ? "Бригада" : hostName,
                    MemberCount = members,
                    MaxMembers = 4
                });
            }

            OnLobbyListUpdated?.Invoke(list);
        }

        private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t result)
        {
            MelonLogger.Msg($"[LobbyManager] Steam Friend invite received! Joining lobby {result.m_steamIDLobby.m_SteamID}...");
            JoinLobby(result.m_steamIDLobby);
        }

        public void OpenInviteDialog()
        {
            if (SteamNetManager.Instance.CurrentLobbyId != CSteamID.Nil)
            {
                SteamFriends.ActivateGameOverlayInviteDialog(SteamNetManager.Instance.CurrentLobbyId);
            }
            else
            {
                MelonLogger.Warning("[LobbyManager] Cannot invite: Not currently in a lobby.");
            }
        }

        public void LeaveCurrentLobby()
        {
            if (SteamNetManager.Instance.CurrentLobbyId != CSteamID.Nil)
            {
                SteamMatchmaking.LeaveLobby(SteamNetManager.Instance.CurrentLobbyId);
                SteamNetManager.Instance.CurrentLobbyId = CSteamID.Nil;
                SteamNetManager.Instance.IsConnected = false;
                SteamNetManager.Instance.IsHost = false;
                SteamNetManager.Instance.DisconnectAll();
                MelonLogger.Msg("[LobbyManager] Left current Steam lobby.");
            }
        }
    }
}
