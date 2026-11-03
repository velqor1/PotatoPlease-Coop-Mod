using System;
using System.Collections.Generic;
using UnityEngine;
using PotatoCoop.Network;
using PotatoCoop.Puppet;
using Il2CppSteamworks;
using Il2CppPotatoPeeler;

namespace PotatoCoop.UI
{
    public class LobbyUI
    {
        public static LobbyUI Instance { get; private set; }
        public bool IsVisible { get; set; } = false;

        private List<LobbyInfo> _availableLobbies = new List<LobbyInfo>();
        private string _statusMessage = "Готов к подключению через Steam Spacewar (AppID 480)";

        private Texture2D _bgTex;
        private Texture2D _cardTex;
        private Texture2D _headerTex;
        private Texture2D _borderTex;

        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new LobbyUI();
                Instance.RegisterEvents();
            }
        }

        private void RegisterEvents()
        {
            LobbyManager.Instance.OnLobbyCreatedSuccess += (info) =>
            {
                _statusMessage = $"Смена создана! Вы Хост. ID Лобби: {info.LobbyId.m_SteamID}";
            };
            LobbyManager.Instance.OnLobbyJoinedSuccess += (info) =>
            {
                _statusMessage = $"Подключено к смене игрока {info.HostName}!";
            };
            LobbyManager.Instance.OnLobbyListUpdated += (list) =>
            {
                _availableLobbies = list;
                _statusMessage = $"Найдено открытых смен: {list.Count}";
            };
            LobbyManager.Instance.OnLobbyError += (err) =>
            {
                _statusMessage = $"Ошибка: {err}";
            };
        }

        private void EnsureTextures()
        {
            if (_bgTex == null)
            {
                _bgTex = MakeTex(2, 2, new Color(0.10f, 0.11f, 0.13f, 0.97f));
                _cardTex = MakeTex(2, 2, new Color(0.16f, 0.17f, 0.20f, 0.95f));
                _headerTex = MakeTex(2, 2, new Color(0.14f, 0.15f, 0.18f, 1f));
                _borderTex = MakeTex(2, 2, new Color(0.85f, 0.65f, 0.15f, 1f));
            }
        }

        private void DrawTex(Rect r, Texture2D t)
        {
            GUI.DrawTexture(r, t, ScaleMode.StretchToFill, true);
        }

        private bool DrawBtn(Rect r, string text)
        {
            return GUI.Button(r, new GUIContent(text), GUI.skin.button);
        }

        public void DrawGUI()
        {
            if (!IsVisible) return;

            EnsureTextures();

            float width = 900f;
            float height = 580f;
            float left = (Screen.width - width) * 0.5f;
            float top = (Screen.height - height) * 0.5f;

            // Outer gold accent frame
            DrawTex(new Rect(left - 2, top - 2, width + 4, height + 4), _borderTex);
            // Main Dark Background
            DrawTex(new Rect(left, top, width, height), _bgTex);

            // Header Banner
            DrawTex(new Rect(left, top, width, 50), _headerTex);
            GUI.color = new Color(0.95f, 0.70f, 0.20f);
            GUI.Label(new Rect(left + 20, top + 14, 600, 30), "<b>СЕТЕВОЙ ТЕРМИНАЛ // СОВМЕСТНАЯ СМЕНА (STEAM SPACEWAR 480)</b>");
            GUI.color = Color.white;

            if (DrawBtn(new Rect(left + width - 110, top + 10, 95, 30), "✕ ЗАКРЫТЬ"))
            {
                IsVisible = false;
            }

            // Status strip
            GUI.color = new Color(0.35f, 0.85f, 0.35f);
            GUI.Label(new Rect(left + 20, top + 55, width - 40, 25), $"Статус: {_statusMessage}");
            GUI.color = Color.white;

            // Content Area: Split Left (Controls) and Right (Roster)
            float colLeft = left + 20;
            float colTop = top + 85;

            // --- LEFT COLUMN: ACTIONS ---
            float leftW = 300f;
            DrawTex(new Rect(colLeft, colTop, leftW, 470), _cardTex);

            GUI.Label(new Rect(colLeft + 15, colTop + 15, leftW - 30, 25), "<b>УПРАВЛЕНИЕ СЕАНСОМ</b>");

            float btnY = colTop + 45;

            if (!SteamNetManager.Instance.IsConnected && !SteamNetManager.Instance.IsHost)
            {
                if (DrawBtn(new Rect(colLeft + 15, btnY, leftW - 30, 42), "★ СОЗДАТЬ СМЕНУ (ХОСТ)"))
                {
                    LobbyManager.Instance.CreateLobby(true);
                }
                btnY += 50;

                if (DrawBtn(new Rect(colLeft + 15, btnY, leftW - 30, 36), "ПОИСК СМЕН ДРУЗЕЙ"))
                {
                    LobbyManager.Instance.RefreshLobbyList();
                }
                btnY += 45;

                if (DrawBtn(new Rect(colLeft + 15, btnY, leftW - 30, 36), "📥 ВСТАВИТЬ ID И ВОЙТИ"))
                {
                    string clip = GUIUtility.systemCopyBuffer;
                    if (!string.IsNullOrEmpty(clip) && ulong.TryParse(clip.Trim(), out ulong parsedLobbyId))
                    {
                        _statusMessage = $"Подключение к {parsedLobbyId}...";
                        LobbyManager.Instance.JoinLobby(new CSteamID(parsedLobbyId));
                    }
                    else
                    {
                        _statusMessage = "В буфере нет корректного Steam ID лобби!";
                    }
                }
                btnY += 45;
            }
            else
            {
                if (DrawBtn(new Rect(colLeft + 15, btnY, leftW - 30, 42), "✉ ПРИГЛАСИТЬ В STEAM"))
                {
                    LobbyManager.Instance.OpenInviteDialog();
                }
                btnY += 48;

                if (DrawBtn(new Rect(colLeft + 15, btnY, leftW - 30, 34), "📋 СКОПИРОВАТЬ ID ЛОББИ"))
                {
                    if (SteamNetManager.Instance.CurrentLobbyId != CSteamID.Nil)
                    {
                        GUIUtility.systemCopyBuffer = SteamNetManager.Instance.CurrentLobbyId.m_SteamID.ToString();
                        _statusMessage = "ID лобби скопирован в буфер обмена!";
                    }
                }
                btnY += 40;

                if (SteamNetManager.Instance.IsHost)
                {
                    GUI.color = new Color(0.95f, 0.70f, 0.20f);
                    if (DrawBtn(new Rect(colLeft + 15, btnY, leftW - 30, 42), "▶ НАЧАТЬ СМЕНУ ВСЕМ"))
                    {
                        IsVisible = false;
                        var mmc = GameObject.FindObjectOfType<MainMenuController>();
                        if (mmc != null)
                        {
                            mmc.BeginShift();
                        }
                    }
                    GUI.color = Color.white;
                    btnY += 48;
                }

                if (DrawBtn(new Rect(colLeft + 15, btnY, leftW - 30, 34), "ПОКИНУТЬ ЛОББИ"))
                {
                    LobbyManager.Instance.LeaveCurrentLobby();
                    _statusMessage = "Вы покинули лобби.";
                }
                btnY += 42;
            }

            // Skin Selector
            btnY = colTop + 245;
            GUI.Label(new Rect(colLeft + 15, btnY, leftW - 30, 25), "<b>СПЕЦИАЛИЗАЦИЯ / СТИЛЬ:</b>");
            btnY += 28;

            if (DrawBtn(new Rect(colLeft + 15, btnY, 40, 32), "◀"))
            {
                SteamNetManager.Instance.LocalSkinIndex = (SteamNetManager.Instance.LocalSkinIndex - 1 + PuppetModelBuilder.Presets.Length) % PuppetModelBuilder.Presets.Length;
            }

            var currentPreset = PuppetModelBuilder.Presets[SteamNetManager.Instance.LocalSkinIndex];
            GUI.color = currentPreset.ApronColor;
            GUI.Box(new Rect(colLeft + 60, btnY, leftW - 120, 32), currentPreset.Title);
            GUI.color = Color.white;

            if (DrawBtn(new Rect(colLeft + leftW - 55, btnY, 40, 32), "▶"))
            {
                SteamNetManager.Instance.LocalSkinIndex = (SteamNetManager.Instance.LocalSkinIndex + 1) % PuppetModelBuilder.Presets.Length;
            }

            // Available friends lobbies list
            if (_availableLobbies.Count > 0 && !SteamNetManager.Instance.IsConnected)
            {
                btnY += 45;
                GUI.Label(new Rect(colLeft + 15, btnY, leftW - 30, 20), "<b>НАЙДЕННЫЕ СМЕНЫ:</b>");
                btnY += 24;

                foreach (var lobby in _availableLobbies)
                {
                    if (DrawBtn(new Rect(colLeft + 15, btnY, leftW - 30, 30), $"{lobby.HostName} ({lobby.MemberCount}/4) -> ВОЙТИ"))
                    {
                        LobbyManager.Instance.JoinLobby(lobby.LobbyId);
                    }
                    btnY += 34;
                }
            }

            // --- RIGHT COLUMN: 4 PLAYER SLOTS ---
            float rightX = colLeft + leftW + 20;
            float rightW = width - leftW - 60;

            GUI.Label(new Rect(rightX, colTop - 5, rightW, 25), "<b>РАБОЧАЯ БРИГАДА (ДО 4 ИГРОКОВ):</b>");

            for (int slot = 0; slot < 4; slot++)
            {
                DrawSlot(slot, rightX, colTop + 25 + (slot * 95), rightW, 85);
            }
        }

        private void DrawSlot(int slotIndex, float x, float y, float w, float h)
        {
            DrawTex(new Rect(x, y, w, h), _cardTex);

            bool isOccupied = false;
            string pName = "";
            string role = "";
            Color roleCol = Color.gray;

            if (slotIndex == 0)
            {
                isOccupied = true;
                pName = SteamNetManager.Instance.IsHost ? SteamNetManager.Instance.LocalPlayerName : (SteamNetManager.Instance.PeerNames.Count > 0 ? "Хост" : "Ожидание хоста...");
                int skin = SteamNetManager.Instance.IsHost ? SteamNetManager.Instance.LocalSkinIndex : 0;
                var preset = PuppetModelBuilder.Presets[skin];
                role = preset.Title;
                roleCol = preset.ApronColor;
            }
            else
            {
                int peerIdx = slotIndex - 1;
                if (!SteamNetManager.Instance.IsHost && slotIndex == SteamNetManager.Instance.LocalAssignedSlot)
                {
                    isOccupied = true;
                    pName = SteamNetManager.Instance.LocalPlayerName + " (Вы)";
                    var preset = PuppetModelBuilder.Presets[SteamNetManager.Instance.LocalSkinIndex];
                    role = preset.Title;
                    roleCol = preset.ApronColor;
                }
                else if (peerIdx < SteamNetManager.Instance.ConnectedPeers.Count)
                {
                    isOccupied = true;
                    var peerId = SteamNetManager.Instance.ConnectedPeers[peerIdx];
                    pName = SteamNetManager.Instance.PeerNames.TryGetValue(peerId.m_SteamID, out string n) ? n : "Напарник";
                    int skin = SteamNetManager.Instance.PeerSkins.TryGetValue(peerId.m_SteamID, out int s) ? s : slotIndex;
                    var preset = PuppetModelBuilder.Presets[Mathf.Clamp(skin, 0, PuppetModelBuilder.Presets.Length - 1)];
                    role = preset.Title;
                    roleCol = preset.ApronColor;
                }
            }

            GUI.color = isOccupied ? roleCol : new Color(0.25f, 0.25f, 0.25f);
            DrawTex(new Rect(x + 15, y + 15, 55, 55), _borderTex);
            GUI.color = Color.white;

            if (isOccupied)
            {
                GUI.Label(new Rect(x + 85, y + 15, 300, 25), $"<b>{pName}</b>");
                GUI.color = new Color(0.75f, 0.75f, 0.75f);
                GUI.Label(new Rect(x + 85, y + 42, 300, 25), $"Роль: {role}  |  Связь: Отличная");
                GUI.color = new Color(0.3f, 0.9f, 0.3f);
                GUI.Label(new Rect(x + w - 110, y + 30, 95, 25), "✓ ГОТОВ");
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = new Color(0.6f, 0.6f, 0.6f);
                GUI.Label(new Rect(x + 85, y + 18, 300, 25), $"<i>Слот #{slotIndex + 1}: Свободен</i>");
                GUI.Label(new Rect(x + 85, y + 42, 300, 25), "Ожидание приглашения друга...");
                GUI.color = Color.white;
            }
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
