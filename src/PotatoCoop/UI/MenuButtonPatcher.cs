using System;
using UnityEngine;
using UnityEngine.UI;
using Il2CppTMPro;
using Il2CppPotatoPeeler;
using MelonLoader;

namespace PotatoCoop.UI
{
    public static class MenuButtonPatcher
    {
        public static void PatchMainMenu(MainMenuController mmc)
        {
            try
            {
                if (mmc == null) return;

                GameObject template = mmc.loadButton ?? mmc.continueButton;
                if (template == null)
                {
                    var allButtons = mmc.GetComponentsInChildren<Button>(true);
                    if (allButtons != null && allButtons.Length > 0)
                    {
                        template = allButtons[0].gameObject;
                    }
                }

                if (template != null && template.transform.parent != null)
                {
                    if (template.transform.parent.Find("Button_CoopMultiplayer") != null)
                        return;

                    GameObject coopBtnObj = GameObject.Instantiate(template, template.transform.parent);
                    coopBtnObj.name = "Button_CoopMultiplayer";
                    coopBtnObj.transform.SetSiblingIndex(template.transform.GetSiblingIndex() + 1);
                    coopBtnObj.SetActive(true);

                    TMP_Text tmp = coopBtnObj.GetComponentInChildren<TMP_Text>(true);
                    if (tmp != null)
                    {
                        tmp.text = "Кооператив (Steam)";
                    }
                    else
                    {
                        Text uiText = coopBtnObj.GetComponentInChildren<Text>(true);
                        if (uiText != null)
                            uiText.text = "Кооператив (Steam)";
                    }

                    Button btn = coopBtnObj.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.onClick = new Button.ButtonClickedEvent();
                        btn.onClick.AddListener((Action)(() =>
                        {
                            if (LobbyUI.Instance != null)
                            {
                                LobbyUI.Instance.IsVisible = true;
                            }
                        }));
                    }

                    MelonLogger.Msg("[MenuButtonPatcher] Successfully inserted 'Кооператив (Steam)' button into MainMenu!");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[MenuButtonPatcher] Could not auto-clone MainMenu button: {ex.Message}");
            }
        }

        public static void PatchPauseMenu(PauseUI pauseUI)
        {
            try
            {
                if (pauseUI == null) return;

                GameObject template = null;
                if (pauseUI.resumeButton != null)
                {
                    template = pauseUI.resumeButton.gameObject;
                }
                else
                {
                    var allButtons = pauseUI.GetComponentsInChildren<Button>(true);
                    if (allButtons != null && allButtons.Length > 0)
                        template = allButtons[0].gameObject;
                }

                if (template != null && template.transform.parent != null)
                {
                    if (template.transform.parent.Find("Button_CoopPause") != null)
                        return;

                    GameObject coopBtnObj = GameObject.Instantiate(template, template.transform.parent);
                    coopBtnObj.name = "Button_CoopPause";
                    coopBtnObj.transform.SetSiblingIndex(template.transform.GetSiblingIndex() + 1);
                    coopBtnObj.SetActive(true);

                    TMP_Text tmp = coopBtnObj.GetComponentInChildren<TMP_Text>(true);
                    if (tmp != null)
                    {
                        tmp.text = "Сетевая смена (Steam)";
                    }
                    else
                    {
                        Text uiText = coopBtnObj.GetComponentInChildren<Text>(true);
                        if (uiText != null)
                            uiText.text = "Сетевая смена (Steam)";
                    }

                    Button btn = coopBtnObj.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.onClick = new Button.ButtonClickedEvent();
                        btn.onClick.AddListener((Action)(() =>
                        {
                            if (LobbyUI.Instance != null)
                            {
                                LobbyUI.Instance.IsVisible = true;
                            }
                        }));
                    }

                    MelonLogger.Msg("[MenuButtonPatcher] Successfully inserted 'Сетевая смена (Steam)' button into PauseMenu!");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[MenuButtonPatcher] Could not auto-clone PauseMenu button: {ex.Message}");
            }
        }
    }
}
