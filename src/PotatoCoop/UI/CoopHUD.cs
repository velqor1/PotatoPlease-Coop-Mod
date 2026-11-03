using System;
using System.Collections.Generic;
using UnityEngine;
using PotatoCoop.Network;
using PotatoCoop.Puppet;

namespace PotatoCoop.UI
{
    public class CoopHUD
    {
        public static CoopHUD Instance { get; private set; }

        private Texture2D _hudBgTex;
        private Texture2D _dotTex;

        private readonly List<string> _notifications = new List<string>();
        private float _notifTimer = 0f;

        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new CoopHUD();
                Instance.Init();
            }
        }

        private void Init()
        {
            SteamNetManager.Instance.OnPeerJoined += (id, name, skin) =>
            {
                ShowNotification($"Игрок {name} присоединился к смене!");
            };
            SteamNetManager.Instance.OnPeerLeft += (id) =>
            {
                ShowNotification("Напарник покинул смену.");
            };
        }

        public void ShowNotification(string message)
        {
            _notifications.Add(message);
            _notifTimer = 4.5f;
        }

        public void Update()
        {
            if (_notifTimer > 0)
            {
                _notifTimer -= Time.deltaTime;
                if (_notifTimer <= 0 && _notifications.Count > 0)
                {
                    _notifications.RemoveAt(0);
                    if (_notifications.Count > 0) _notifTimer = 3f;
                }
            }
        }

        private void EnsureTextures()
        {
            if (_hudBgTex == null)
            {
                _hudBgTex = MakeTex(2, 2, new Color(0.12f, 0.13f, 0.15f, 0.85f));
                _dotTex = MakeTex(2, 2, Color.white);
            }
        }

        private void DrawTex(Rect r, Texture2D t)
        {
            GUI.DrawTexture(r, t, ScaleMode.StretchToFill, true);
        }

        public void DrawGUI()
        {
            EnsureTextures();

            // Draw compact team status in top-left corner
            if (SteamNetManager.Instance != null && (SteamNetManager.Instance.IsConnected || SteamNetManager.Instance.IsHost))
            {
                int peerCount = SteamNetManager.Instance.ConnectedPeers.Count;
                int totalWorkers = peerCount + 1;

                float boxW = 240f;
                float boxH = 40f + (totalWorkers * 24f);

                DrawTex(new Rect(15, 15, boxW, boxH), _hudBgTex);

                GUI.color = new Color(0.95f, 0.70f, 0.20f);
                GUI.Label(new Rect(25, 20, 200, 20), $"<b>БРИГАДА СМЕНЫ ({totalWorkers} чел.) [F1: Меню]</b>");
                GUI.color = Color.white;

                float rowY = 44f;

                // Local player
                var localPreset = PuppetModelBuilder.Presets[SteamNetManager.Instance.LocalSkinIndex];
                GUI.color = localPreset.ApronColor;
                DrawTex(new Rect(25, rowY + 4, 10, 10), _dotTex);
                GUI.color = Color.white;
                GUI.Label(new Rect(42, rowY, 190, 20), $"{SteamNetManager.Instance.LocalPlayerName} ({localPreset.Title})");
                rowY += 24f;

                // Remote players
                for (int i = 0; i < peerCount; i++)
                {
                    var pId = SteamNetManager.Instance.ConnectedPeers[i];
                    string pName = SteamNetManager.Instance.PeerNames.TryGetValue(pId.m_SteamID, out string n) ? n : "Рабочий";
                    int skin = SteamNetManager.Instance.PeerSkins.TryGetValue(pId.m_SteamID, out int s) ? s : 1;
                    var preset = PuppetModelBuilder.Presets[Mathf.Clamp(skin, 0, PuppetModelBuilder.Presets.Length - 1)];

                    GUI.color = preset.ApronColor;
                    DrawTex(new Rect(25, rowY + 4, 10, 10), _dotTex);
                    GUI.color = Color.white;
                    GUI.Label(new Rect(42, rowY, 190, 20), $"{pName} ({preset.Title})");
                    rowY += 24f;
                }
            }

            // Draw notification toast
            if (_notifications.Count > 0)
            {
                string msg = _notifications[0];
                float w = 400f;
                float h = 42f;
                float x = (Screen.width - w) * 0.5f;
                float y = 50f;

                DrawTex(new Rect(x, y, w, h), _hudBgTex);
                GUI.color = new Color(0.95f, 0.85f, 0.25f);
                GUI.Label(new Rect(x + 15, y + 10, w - 30, 24), $"📢 {msg}");
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
