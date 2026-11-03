using System;
using UnityEngine;
using UnityEngine.UI;

namespace PotatoCoop.Puppet
{
    public class RemotePuppet
    {
        public ulong SteamId { get; set; }
        public string PlayerName { get; set; } = "Worker";
        public int SkinIndex { get; set; } = 0;

        public GameObject RootGameObject { get; private set; }

        private Transform _headTransform;
        private Transform _rightHandAnchor;
        private Transform _leftArmTransform;
        private Transform _rightArmTransform;
        private Transform _leftLegTransform;
        private Transform _rightLegTransform;

        private GameObject _currentToolProp;
        private int _currentToolId = -1;

        // Target state for interpolation
        private Vector3 _targetPosition;
        private float _targetYaw;
        private float _targetPitch;
        private float _speed;
        private bool _isCrouching;

        // Procedural animation
        private float _walkCycle = 0f;
        private Text _nameplateText;

        public void Initialize(ulong steamId, string playerName, int skinIndex)
        {
            SteamId = steamId;
            PlayerName = playerName;
            SkinIndex = skinIndex;

            // Build character model
            RootGameObject = PuppetModelBuilder.BuildPuppet(
                skinIndex,
                out _headTransform,
                out _rightHandAnchor,
                out _leftArmTransform,
                out _rightArmTransform,
                out _leftLegTransform,
                out _rightLegTransform
            );
            RootGameObject.name = $"Puppet_{steamId}";
            GameObject.DontDestroyOnLoad(RootGameObject);

            // Attach world-space nameplate above head
            CreateNameplate();

            // Default tool: Hand / Knife
            SetTool(0);
        }

        private void CreateNameplate()
        {
            if (RootGameObject == null) return;

            GameObject canvasObj = new GameObject("NameplateCanvas");
            canvasObj.transform.SetParent(RootGameObject.transform, false);
            canvasObj.transform.localPosition = new Vector3(0, 2.35f, 0);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100;

            GameObject textObj = CreateTextObj(canvasObj.transform);
            _nameplateText = textObj.GetComponent<Text>();
            if (_nameplateText != null)
            {
                string role = PuppetModelBuilder.Presets[Mathf.Clamp(SkinIndex, 0, PuppetModelBuilder.Presets.Length - 1)].Title;
                _nameplateText.text = $"[{role}]\n<b>{PlayerName}</b>";
            }
        }

        private GameObject CreateTextObj(Transform parent)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parent, false);

            var txt = textObj.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = 28;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.rectTransform.sizeDelta = new Vector2(300, 60);

            var outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.9f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            return textObj;
        }

        public void UpdateTransform(Vector3 pos, float yaw, float pitch, float speed, bool isCrouching, int toolId, bool isInteracting)
        {
            _targetPosition = pos;
            _targetYaw = yaw;
            _targetPitch = pitch;
            _speed = speed;
            _isCrouching = isCrouching;

            if (toolId != _currentToolId)
            {
                SetTool(toolId);
            }
        }

        private void SetTool(int toolId)
        {
            _currentToolId = toolId;
            if (_currentToolProp != null)
            {
                GameObject.Destroy(_currentToolProp);
            }

            if (_rightHandAnchor != null)
            {
                _currentToolProp = PuppetModelBuilder.BuildToolProp(toolId, _rightHandAnchor);
            }
        }

        public void Update(float dt)
        {
            if (RootGameObject == null) return;

            // Smooth position & body rotation interpolation
            RootGameObject.transform.position = Vector3.Lerp(RootGameObject.transform.position, _targetPosition, dt * 15f);
            RootGameObject.transform.rotation = Quaternion.Slerp(RootGameObject.transform.rotation, Quaternion.Euler(0, _targetYaw, 0), dt * 15f);

            // Head pitch (looking up/down)
            if (_headTransform != null)
            {
                _headTransform.localRotation = Quaternion.Euler(_targetPitch, 0, 0);
            }

            // Billboard nameplate towards local main camera
            if (Camera.main != null && _nameplateText != null)
            {
                _nameplateText.transform.parent.rotation = Quaternion.LookRotation(_nameplateText.transform.parent.position - Camera.main.transform.position);
            }

            // Procedural walking animation
            AnimateLocomotion(dt);
        }

        private void AnimateLocomotion(float dt)
        {
            if (_speed > 0.1f)
            {
                _walkCycle += dt * _speed * 4.5f;

                float legSwing = Mathf.Sin(_walkCycle) * 28f;
                float armSwing = Mathf.Cos(_walkCycle) * 22f;

                if (_leftLegTransform != null)
                    _leftLegTransform.localRotation = Quaternion.Euler(legSwing, 0, 0);
                if (_rightLegTransform != null)
                    _rightLegTransform.localRotation = Quaternion.Euler(-legSwing, 0, 0);

                if (_leftArmTransform != null)
                    _leftArmTransform.localRotation = Quaternion.Euler(-armSwing, 0, 0);
                if (_rightArmTransform != null)
                    _rightArmTransform.localRotation = Quaternion.Euler(armSwing * 0.4f, 0, 0);
            }
            else
            {
                if (_leftLegTransform != null)
                    _leftLegTransform.localRotation = Quaternion.Slerp(_leftLegTransform.localRotation, Quaternion.identity, dt * 8f);
                if (_rightLegTransform != null)
                    _rightLegTransform.localRotation = Quaternion.Slerp(_rightLegTransform.localRotation, Quaternion.identity, dt * 8f);
                if (_leftArmTransform != null)
                    _leftArmTransform.localRotation = Quaternion.Slerp(_leftArmTransform.localRotation, Quaternion.identity, dt * 8f);
                if (_rightArmTransform != null)
                    _rightArmTransform.localRotation = Quaternion.Slerp(_rightArmTransform.localRotation, Quaternion.identity, dt * 8f);
            }
        }

        public void Destroy()
        {
            if (RootGameObject != null)
            {
                GameObject.Destroy(RootGameObject);
                RootGameObject = null;
            }
        }
    }
}
