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

        private Transform _visualBodyTransform;
        private Transform _headTransform;
        private Transform _rightHandAnchor;
        private Transform _leftArmTransform;
        private Transform _rightArmTransform;
        private Transform _leftLegTransform;
        private Transform _rightLegTransform;

        private GameObject _currentToolProp;
        private GameObject _leftHandPeelPotato;
        private int _currentToolId = -1;

        // Target state for interpolation
        private Vector3 _targetPosition;
        private float _targetYaw;
        private float _targetPitch;
        private float _speed;
        private bool _isCrouching;
        private bool _isInteracting;

        // Procedural animation state
        private float _walkCycle = 0f;
        private float _smoothSpeed = 0f;
        private Text _nameplateText;
        private bool _isVisualsEnabled = true;

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

            _visualBodyTransform = RootGameObject.transform.Find("VisualBody");

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
            canvasObj.transform.localPosition = new Vector3(0, 2.30f, 0);

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
            _isInteracting = isInteracting;

            // When peeling, remote puppet holds knife
            int effectiveTool = isInteracting ? 4 : toolId;

            if (effectiveTool != _currentToolId)
            {
                SetTool(effectiveTool);
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

            // When local player is actively peeling a potato, hide remote puppets from obscuring the cutting board
            bool localPeeling = false;
            try
            {
                var session = Il2CppPotatoPeeler.PeelSession.Instance;
                if (session != null && session.IsActive)
                {
                    localPeeling = true;
                }
            }
            catch { }

            SetVisualsVisible(!localPeeling);

            // Smooth speed interpolation for fluid walk transitions
            _smoothSpeed = Mathf.Lerp(_smoothSpeed, _speed, dt * 10f);

            // Smooth position & body rotation interpolation
            RootGameObject.transform.position = Vector3.Lerp(RootGameObject.transform.position, _targetPosition, dt * 15f);
            RootGameObject.transform.rotation = Quaternion.Slerp(RootGameObject.transform.rotation, Quaternion.Euler(0, _targetYaw, 0), dt * 15f);

            // Head pitch (looking up/down)
            if (_headTransform != null && !_isInteracting)
            {
                _headTransform.localRotation = Quaternion.Slerp(_headTransform.localRotation, Quaternion.Euler(_targetPitch, 0, 0), dt * 12f);
            }

            // Billboard nameplate towards local main camera
            if (Camera.main != null && _nameplateText != null)
            {
                _nameplateText.transform.parent.rotation = Quaternion.LookRotation(_nameplateText.transform.parent.position - Camera.main.transform.position);
            }

            // Procedural walking & peeling animation
            AnimateLocomotion(dt);
        }

        private void SetVisualsVisible(bool visible)
        {
            if (_isVisualsEnabled == visible) return;
            _isVisualsEnabled = visible;

            if (RootGameObject == null) return;

            Transform body = RootGameObject.transform.Find("VisualBody");
            if (body != null)
            {
                body.gameObject.SetActive(visible);
            }

            Transform np = RootGameObject.transform.Find("NameplateCanvas");
            if (np != null)
            {
                np.gameObject.SetActive(visible);
            }
        }

        private void AnimateLocomotion(float dt)
        {
            // =========================================================
            // 1. REALISTIC PEELING ANIMATION AT WORKSTATION
            // =========================================================
            if (_isInteracting)
            {
                // Ensure potato prop is held in left hand
                if (_leftHandPeelPotato == null && _leftArmTransform != null)
                {
                    _leftHandPeelPotato = PuppetModelBuilder.BuildToolProp(10, _leftArmTransform);
                    _leftHandPeelPotato.transform.localPosition = new Vector3(0, -0.48f, 0.08f);
                    _leftHandPeelPotato.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                }

                // Slicing stroke cycle (~2.8 strokes/sec)
                float strokeSpeed = 2.8f;
                float strokePhase = (Time.time * strokeSpeed) % 1.0f;

                float cutPitch;
                float cutYaw;
                if (strokePhase < 0.52f)
                {
                    // Downward peeling cut stroke
                    float t = strokePhase / 0.52f;
                    float smoothT = Mathf.SmoothStep(0f, 1f, t);
                    cutPitch = Mathf.Lerp(-46f, -68f, smoothT);
                    cutYaw = Mathf.Lerp(-14f, -22f, smoothT);
                }
                else
                {
                    // Clean return reset glide
                    float t = (strokePhase - 0.52f) / 0.48f;
                    float smoothT = Mathf.SmoothStep(0f, 1f, t);
                    cutPitch = Mathf.Lerp(-68f, -46f, smoothT);
                    cutYaw = Mathf.Lerp(-22f, -14f, smoothT);
                }

                // Torso leaning attentively over the cutting board
                if (_visualBodyTransform != null)
                {
                    _visualBodyTransform.localRotation = Quaternion.Slerp(_visualBodyTransform.localRotation, Quaternion.Euler(14f, 0f, 0f), dt * 10f);
                    _visualBodyTransform.localPosition = Vector3.Lerp(_visualBodyTransform.localPosition, new Vector3(0, -0.04f, 0.06f), dt * 10f);
                }

                // Head tilted down, focused on the knife & potato
                if (_headTransform != null)
                {
                    _headTransform.localRotation = Quaternion.Slerp(_headTransform.localRotation, Quaternion.Euler(26f, 0f, 0f), dt * 10f);
                }

                // Left arm: holding potato steadily in front of chest with subtle rotation
                if (_leftArmTransform != null)
                {
                    float potatoRoll = Mathf.Sin(Time.time * 1.8f) * 6f;
                    _leftArmTransform.localRotation = Quaternion.Slerp(_leftArmTransform.localRotation, Quaternion.Euler(-52f, 24f + potatoRoll, -12f), dt * 10f);
                }

                // Right arm: slicing knife down potato
                if (_rightArmTransform != null)
                {
                    _rightArmTransform.localRotation = Quaternion.Euler(cutPitch, cutYaw, 4f);
                }

                // Grounded legs
                if (_leftLegTransform != null)
                    _leftLegTransform.localRotation = Quaternion.Slerp(_leftLegTransform.localRotation, Quaternion.identity, dt * 10f);
                if (_rightLegTransform != null)
                    _rightLegTransform.localRotation = Quaternion.Slerp(_rightLegTransform.localRotation, Quaternion.identity, dt * 10f);

                return;
            }

            // Cleanup peeling potato prop if exiting peeling state
            if (_leftHandPeelPotato != null)
            {
                GameObject.Destroy(_leftHandPeelPotato);
                _leftHandPeelPotato = null;
            }

            // =========================================================
            // 2. SMOOTH REALISTIC WALKING ANIMATION
            // =========================================================
            if (_smoothSpeed > 0.12f)
            {
                float cycleSpeed = Mathf.Clamp(_smoothSpeed * 3.6f, 2.2f, 6.2f);
                _walkCycle += dt * cycleSpeed;

                // 1) Vertical Hip Bounce (double frequency: hips rise and drop on each step)
                float bounce = Mathf.Abs(Mathf.Sin(_walkCycle)) * 0.040f;

                // 2) Torso Sway & Forward Movement Lean
                float forwardLean = Mathf.Clamp(_smoothSpeed * 2.2f, 2f, 7.5f);
                float hipRoll = Mathf.Sin(_walkCycle) * 3.2f;    // Leaning gently into supporting hip
                float spineYaw = -Mathf.Sin(_walkCycle) * 4.2f; // Shoulders counter-rotate against pelvis

                if (_visualBodyTransform != null)
                {
                    _visualBodyTransform.localPosition = Vector3.Lerp(_visualBodyTransform.localPosition, new Vector3(0, bounce, 0), dt * 12f);
                    _visualBodyTransform.localRotation = Quaternion.Slerp(_visualBodyTransform.localRotation, Quaternion.Euler(forwardLean, spineYaw, hipRoll), dt * 12f);
                }

                // 3) Leg swinging & stepping
                float legAngle = Mathf.Sin(_walkCycle) * 30f;
                if (_leftLegTransform != null)
                    _leftLegTransform.localRotation = Quaternion.Euler(legAngle, 0f, -hipRoll * 0.4f);
                if (_rightLegTransform != null)
                    _rightLegTransform.localRotation = Quaternion.Euler(-legAngle, 0f, -hipRoll * 0.4f);

                // 4) Arm swinging arcs
                if (_currentToolId == 10) // Carrying potato
                {
                    float armBob = Mathf.Sin(_walkCycle * 2f) * 3f;
                    if (_leftArmTransform != null)
                        _leftArmTransform.localRotation = Quaternion.Euler(-44f + armBob, 16f, 0);
                    if (_rightArmTransform != null)
                        _rightArmTransform.localRotation = Quaternion.Euler(-44f + armBob, -16f, 0);
                }
                else if (_currentToolId == 11) // Carrying sack
                {
                    float sackSway = Mathf.Sin(_walkCycle) * 3.5f;
                    if (_leftArmTransform != null)
                        _leftArmTransform.localRotation = Quaternion.Euler(-40f, 24f + sackSway, 0);
                    if (_rightArmTransform != null)
                        _rightArmTransform.localRotation = Quaternion.Euler(-40f, -24f + sackSway, 0);
                }
                else
                {
                    // Natural arm swing with inward chest arc & relaxed elbow bend
                    float armSwing = Mathf.Cos(_walkCycle) * 22f;
                    if (_leftArmTransform != null)
                        _leftArmTransform.localRotation = Quaternion.Euler(-armSwing, -5f, -8f);
                    if (_rightArmTransform != null)
                        _rightArmTransform.localRotation = Quaternion.Euler(armSwing * 0.6f, 5f, 8f);
                }
            }
            // =========================================================
            // 3. IDLE NATURAL BREATHING ANIMATION
            // =========================================================
            else
            {
                float breath = Mathf.Sin(Time.time * 2.2f) * 0.012f;
                float breathTilt = Mathf.Sin(Time.time * 2.2f) * 1.0f;

                if (_visualBodyTransform != null)
                {
                    _visualBodyTransform.localPosition = Vector3.Lerp(_visualBodyTransform.localPosition, new Vector3(0, breath, 0), dt * 6f);
                    _visualBodyTransform.localRotation = Quaternion.Slerp(_visualBodyTransform.localRotation, Quaternion.Euler(breathTilt, 0, 0), dt * 6f);
                }

                if (_leftLegTransform != null)
                    _leftLegTransform.localRotation = Quaternion.Slerp(_leftLegTransform.localRotation, Quaternion.identity, dt * 8f);
                if (_rightLegTransform != null)
                    _rightLegTransform.localRotation = Quaternion.Slerp(_rightLegTransform.localRotation, Quaternion.identity, dt * 8f);

                if (_currentToolId == 10) // Potato
                {
                    if (_leftArmTransform != null)
                        _leftArmTransform.localRotation = Quaternion.Slerp(_leftArmTransform.localRotation, Quaternion.Euler(-44f, 16f, 0), dt * 8f);
                    if (_rightArmTransform != null)
                        _rightArmTransform.localRotation = Quaternion.Slerp(_rightArmTransform.localRotation, Quaternion.Euler(-44f, -16f, 0), dt * 8f);
                }
                else if (_currentToolId == 11) // Sack
                {
                    if (_leftArmTransform != null)
                        _leftArmTransform.localRotation = Quaternion.Slerp(_leftArmTransform.localRotation, Quaternion.Euler(-40f, 24f, 0), dt * 8f);
                    if (_rightArmTransform != null)
                        _rightArmTransform.localRotation = Quaternion.Slerp(_rightArmTransform.localRotation, Quaternion.Euler(-40f, -24f, 0), dt * 8f);
                }
                else
                {
                    // Natural relaxed arms at sides
                    if (_leftArmTransform != null)
                        _leftArmTransform.localRotation = Quaternion.Slerp(_leftArmTransform.localRotation, Quaternion.Euler(0, 0, -4f), dt * 8f);
                    if (_rightArmTransform != null)
                        _rightArmTransform.localRotation = Quaternion.Slerp(_rightArmTransform.localRotation, Quaternion.Euler(0, 0, 4f), dt * 8f);
                }
            }
        }

        public void Destroy()
        {
            if (_leftHandPeelPotato != null)
            {
                GameObject.Destroy(_leftHandPeelPotato);
                _leftHandPeelPotato = null;
            }

            if (RootGameObject != null)
            {
                GameObject.Destroy(RootGameObject);
                RootGameObject = null;
            }
        }
    }
}
