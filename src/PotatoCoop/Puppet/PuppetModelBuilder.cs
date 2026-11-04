using System;
using UnityEngine;

namespace PotatoCoop.Puppet
{
    public struct CharacterVisualConfig
    {
        public string Title;
        public Color CoatColor;
        public Color ApronColor;
        public Color HatColor;
        public Color HatAccentColor;
        public Color GloveColor;
        public Color BootColor;
        public HatType Hat;
        public FacialType Facial;
    }

    public enum HatType
    {
        ChefToque,
        SovietUshanka,
        SafetyHelmet,
        WorkerCap
    }

    public enum FacialType
    {
        CurledMustache,
        BushyMustache,
        WeldingGoggles,
        CleanShaven
    }

    public static class PuppetModelBuilder
    {
        public static readonly CharacterVisualConfig[] Presets = new CharacterVisualConfig[]
        {
            // Player 1: Master Chef (Dark Navy Coat, Pristine White Apron, Tall Chef Toque with Gold Star, Cossack Mustache)
            new CharacterVisualConfig
            {
                Title = "ШЕФ-ПОВАР",
                CoatColor = new Color(0.15f, 0.20f, 0.28f),
                ApronColor = new Color(0.92f, 0.92f, 0.90f),
                HatColor = new Color(0.95f, 0.95f, 0.95f),
                HatAccentColor = new Color(0.85f, 0.65f, 0.15f), // Gold Cockade
                GloveColor = new Color(0.24f, 0.20f, 0.18f), // Dark leather
                BootColor = new Color(0.12f, 0.12f, 0.12f),
                Hat = HatType.ChefToque,
                Facial = FacialType.CurledMustache
            },
            // Player 2: Peeler Specialist (Soviet Olive Vatnik, Burlap Brown Apron, Ushanka with Red Star)
            new CharacterVisualConfig
            {
                Title = "МАСТЕР ОЧИСТКИ",
                CoatColor = new Color(0.30f, 0.35f, 0.25f),
                ApronColor = new Color(0.48f, 0.32f, 0.20f),
                HatColor = new Color(0.28f, 0.24f, 0.20f),
                HatAccentColor = new Color(0.85f, 0.15f, 0.15f), // Red Star
                GloveColor = new Color(0.85f, 0.45f, 0.15f), // Heavy industrial rubber orange
                BootColor = new Color(0.16f, 0.12f, 0.10f),
                Hat = HatType.SovietUshanka,
                Facial = FacialType.BushyMustache
            },
            // Player 3: Press Mechanic (Charcoal Work Jacket, Rubber Black Apron, Hardhat & Welding Goggles)
            new CharacterVisualConfig
            {
                Title = "МЕХАНИК ПРЕССА",
                CoatColor = new Color(0.25f, 0.26f, 0.28f),
                ApronColor = new Color(0.16f, 0.17f, 0.18f),
                HatColor = new Color(0.92f, 0.75f, 0.10f), // Yellow hardhat
                HatAccentColor = new Color(0.15f, 0.15f, 0.15f),
                GloveColor = new Color(0.22f, 0.22f, 0.22f), // Black work gloves
                BootColor = new Color(0.10f, 0.10f, 0.10f),
                Hat = HatType.SafetyHelmet,
                Facial = FacialType.WeldingGoggles
            },
            // Player 4: Kitchen Apprentice (Slate Blue Jacket, Crimson Apron, Peaked Worker Cap)
            new CharacterVisualConfig
            {
                Title = "ПОДМАСТЕРЬЕ",
                CoatColor = new Color(0.35f, 0.42f, 0.50f),
                ApronColor = new Color(0.55f, 0.18f, 0.22f),
                HatColor = new Color(0.30f, 0.28f, 0.26f),
                HatAccentColor = new Color(0.70f, 0.20f, 0.20f),
                GloveColor = new Color(0.35f, 0.40f, 0.45f), // Slate grip
                BootColor = new Color(0.18f, 0.14f, 0.12f),
                Hat = HatType.WorkerCap,
                Facial = FacialType.CleanShaven
            }
        };

        public static GameObject BuildPuppet(int skinIndex, out Transform headTransform, out Transform rightHandAnchor, out Transform leftArmTransform, out Transform rightArmTransform, out Transform leftLegTransform, out Transform rightLegTransform)
        {
            skinIndex = Mathf.Clamp(skinIndex, 0, Presets.Length - 1);
            CharacterVisualConfig config = Presets[skinIndex];

            GameObject root = new GameObject($"Puppet_{config.Title}");

            // Standard / URP Lit shader
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");

            // Matte non-glaring materials with _Smoothness = 0.04f (prevents white ball specular glare!)
            Material coatMat = CreateMaterial(shader, config.CoatColor, 0.04f);
            Material apronMat = CreateMaterial(shader, config.ApronColor, 0.04f);
            Material hatMat = CreateMaterial(shader, config.HatColor, 0.04f);
            Material hatAccentMat = CreateMaterial(shader, config.HatAccentColor, 0.08f);
            Material skinMat = CreateMaterial(shader, new Color(0.88f, 0.72f, 0.58f), 0.05f); // Natural matte skin
            Material eyeWhiteMat = CreateMaterial(shader, new Color(0.92f, 0.92f, 0.92f), 0.10f);
            Material eyePupilMat = CreateMaterial(shader, new Color(0.12f, 0.12f, 0.14f), 0.10f);
            Material eyebrowMat = CreateMaterial(shader, new Color(0.20f, 0.15f, 0.10f), 0.02f);
            Material gloveMat = CreateMaterial(shader, config.GloveColor, 0.05f);
            Material bootMat = CreateMaterial(shader, config.BootColor, 0.06f);
            Material beltMat = CreateMaterial(shader, new Color(0.18f, 0.12f, 0.08f), 0.06f); // Dark leather
            Material buckleMat = CreateMaterial(shader, new Color(0.85f, 0.75f, 0.25f), 0.60f, 0.85f); // Brass metallic
            Material buttonMat = CreateMaterial(shader, new Color(0.85f, 0.70f, 0.20f), 0.60f, 0.85f); // Brass buttons
            Material hairMat = CreateMaterial(shader, new Color(0.22f, 0.15f, 0.10f), 0.02f);

            // Container for puppet body visuals so they can be toggled without disabling RootGameObject
            GameObject bodyContainer = new GameObject("VisualBody");
            bodyContainer.transform.SetParent(root.transform, false);

            // --- TORSO (Padded Soviet Vatnik Coat) ---
            GameObject torso = CreatePrimitive(PrimitiveType.Capsule, "Torso", bodyContainer.transform);
            torso.transform.localPosition = new Vector3(0, 1.05f, 0);
            torso.transform.localScale = new Vector3(0.48f, 0.44f, 0.36f);
            SetMaterial(torso, coatMat);

            // Coat Collar
            GameObject collar = CreatePrimitive(PrimitiveType.Cylinder, "CoatCollar", bodyContainer.transform);
            collar.transform.localPosition = new Vector3(0, 1.35f, 0);
            collar.transform.localScale = new Vector3(0.34f, 0.04f, 0.32f);
            SetMaterial(collar, coatMat);

            // Double row of brass buttons on jacket chest
            for (int r = 0; r < 3; r++)
            {
                float y = 1.22f - (r * 0.12f);
                GameObject btnL = CreatePrimitive(PrimitiveType.Cylinder, $"BtnL_{r}", bodyContainer.transform);
                btnL.transform.localPosition = new Vector3(-0.08f, y, 0.18f);
                btnL.transform.localRotation = Quaternion.Euler(90f, 0, 0);
                btnL.transform.localScale = new Vector3(0.035f, 0.015f, 0.035f);
                SetMaterial(btnL, buttonMat);

                GameObject btnR = CreatePrimitive(PrimitiveType.Cylinder, $"BtnR_{r}", bodyContainer.transform);
                btnR.transform.localPosition = new Vector3(0.08f, y, 0.18f);
                btnR.transform.localRotation = Quaternion.Euler(90f, 0, 0);
                btnR.transform.localScale = new Vector3(0.035f, 0.015f, 0.035f);
                SetMaterial(btnR, buttonMat);
            }

            // Apron front panel
            GameObject apron = CreatePrimitive(PrimitiveType.Cube, "Apron", bodyContainer.transform);
            apron.transform.localPosition = new Vector3(0, 0.98f, 0.19f);
            apron.transform.localScale = new Vector3(0.42f, 0.60f, 0.03f);
            SetMaterial(apron, apronMat);

            // Apron kangaroo front pocket
            GameObject pocket = CreatePrimitive(PrimitiveType.Cube, "ApronPocket", bodyContainer.transform);
            pocket.transform.localPosition = new Vector3(0, 0.88f, 0.21f);
            pocket.transform.localScale = new Vector3(0.28f, 0.16f, 0.02f);
            SetMaterial(pocket, apronMat);

            // Heavy leather tool belt
            GameObject belt = CreatePrimitive(PrimitiveType.Cylinder, "Belt", bodyContainer.transform);
            belt.transform.localPosition = new Vector3(0, 0.84f, 0);
            belt.transform.localScale = new Vector3(0.50f, 0.045f, 0.40f);
            SetMaterial(belt, beltMat);

            // Brass belt buckle
            GameObject buckle = CreatePrimitive(PrimitiveType.Cube, "BeltBuckle", bodyContainer.transform);
            buckle.transform.localPosition = new Vector3(0, 0.84f, 0.22f);
            buckle.transform.localScale = new Vector3(0.10f, 0.08f, 0.03f);
            SetMaterial(buckle, buckleMat);

            // Side leather tool pouch
            GameObject pouch = CreatePrimitive(PrimitiveType.Cube, "ToolPouch", bodyContainer.transform);
            pouch.transform.localPosition = new Vector3(0.24f, 0.82f, 0.05f);
            pouch.transform.localScale = new Vector3(0.08f, 0.12f, 0.14f);
            SetMaterial(pouch, beltMat);

            // --- HEAD HIERARCHY ---
            GameObject headObj = new GameObject("HeadAnchor");
            headObj.transform.SetParent(bodyContainer.transform, false);
            headObj.transform.localPosition = new Vector3(0, 1.52f, 0);
            headTransform = headObj.transform;

            // Sculpted Head Base
            GameObject headMesh = CreatePrimitive(PrimitiveType.Sphere, "Head", headTransform);
            headMesh.transform.localPosition = Vector3.zero;
            headMesh.transform.localScale = new Vector3(0.36f, 0.38f, 0.34f);
            SetMaterial(headMesh, skinMat);

            // Left & Right Ears
            GameObject lEar = CreatePrimitive(PrimitiveType.Sphere, "LeftEar", headTransform);
            lEar.transform.localPosition = new Vector3(-0.19f, 0.01f, 0);
            lEar.transform.localScale = new Vector3(0.05f, 0.09f, 0.06f);
            SetMaterial(lEar, skinMat);

            GameObject rEar = CreatePrimitive(PrimitiveType.Sphere, "RightEar", headTransform);
            rEar.transform.localPosition = new Vector3(0.19f, 0.01f, 0);
            rEar.transform.localScale = new Vector3(0.05f, 0.09f, 0.06f);
            SetMaterial(rEar, skinMat);

            // Nose
            GameObject nose = CreatePrimitive(PrimitiveType.Cube, "Nose", headTransform);
            nose.transform.localPosition = new Vector3(0, 0.02f, 0.18f);
            nose.transform.localRotation = Quaternion.Euler(-15f, 0, 0);
            nose.transform.localScale = new Vector3(0.05f, 0.08f, 0.06f);
            SetMaterial(nose, skinMat);

            // Eyes
            GameObject leftEye = CreatePrimitive(PrimitiveType.Sphere, "LeftEye", headTransform);
            leftEye.transform.localPosition = new Vector3(-0.085f, 0.05f, 0.155f);
            leftEye.transform.localScale = new Vector3(0.065f, 0.065f, 0.03f);
            SetMaterial(leftEye, eyeWhiteMat);

            GameObject leftPupil = CreatePrimitive(PrimitiveType.Sphere, "LeftPupil", headTransform);
            leftPupil.transform.localPosition = new Vector3(-0.085f, 0.05f, 0.17f);
            leftPupil.transform.localScale = new Vector3(0.035f, 0.035f, 0.02f);
            SetMaterial(leftPupil, eyePupilMat);

            GameObject rightEye = CreatePrimitive(PrimitiveType.Sphere, "RightEye", headTransform);
            rightEye.transform.localPosition = new Vector3(0.085f, 0.05f, 0.155f);
            rightEye.transform.localScale = new Vector3(0.065f, 0.065f, 0.03f);
            SetMaterial(rightEye, eyeWhiteMat);

            GameObject rightPupil = CreatePrimitive(PrimitiveType.Sphere, "RightPupil", headTransform);
            rightPupil.transform.localPosition = new Vector3(0.085f, 0.05f, 0.17f);
            rightPupil.transform.localScale = new Vector3(0.035f, 0.035f, 0.02f);
            SetMaterial(rightPupil, eyePupilMat);

            // Thick expressive Soviet worker eyebrows
            GameObject leftBrow = CreatePrimitive(PrimitiveType.Cube, "LeftBrow", headTransform);
            leftBrow.transform.localPosition = new Vector3(-0.085f, 0.10f, 0.165f);
            leftBrow.transform.localRotation = Quaternion.Euler(0, 0, -8f);
            leftBrow.transform.localScale = new Vector3(0.08f, 0.025f, 0.03f);
            SetMaterial(leftBrow, eyebrowMat);

            GameObject rightBrow = CreatePrimitive(PrimitiveType.Cube, "RightBrow", headTransform);
            rightBrow.transform.localPosition = new Vector3(0.085f, 0.10f, 0.165f);
            rightBrow.transform.localRotation = Quaternion.Euler(0, 0, 8f);
            rightBrow.transform.localScale = new Vector3(0.08f, 0.025f, 0.03f);
            SetMaterial(rightBrow, eyebrowMat);

            // --- HAT BUILDER ---
            BuildHat(config.Hat, headTransform, hatMat, hatAccentMat);

            // --- FACIAL FEATURES ---
            BuildFacial(config.Facial, headTransform, hairMat, buckleMat);

            // --- ARMS & REALISTIC HANDS (NO GIANT WHITE BALLS!) ---
            // Left Arm
            GameObject leftArm = new GameObject("LeftArm");
            leftArm.transform.SetParent(bodyContainer.transform, false);
            leftArm.transform.localPosition = new Vector3(-0.32f, 1.30f, 0);
            leftArmTransform = leftArm.transform;

            GameObject lUpperArm = CreatePrimitive(PrimitiveType.Cylinder, "L_UpperArm", leftArm.transform);
            lUpperArm.transform.localPosition = new Vector3(0, -0.16f, 0);
            lUpperArm.transform.localScale = new Vector3(0.12f, 0.16f, 0.12f);
            SetMaterial(lUpperArm, coatMat);

            GameObject lForearm = CreatePrimitive(PrimitiveType.Cylinder, "L_Forearm", leftArm.transform);
            lForearm.transform.localPosition = new Vector3(0, -0.32f, 0);
            lForearm.transform.localScale = new Vector3(0.11f, 0.16f, 0.11f);
            SetMaterial(lForearm, coatMat);

            // Left Glove / Hand (Palm, Thumb, Fingers)
            BuildHand(leftArm.transform, gloveMat, true);

            // Right Arm
            GameObject rightArm = new GameObject("RightArm");
            rightArm.transform.SetParent(bodyContainer.transform, false);
            rightArm.transform.localPosition = new Vector3(0.32f, 1.30f, 0);
            rightArmTransform = rightArm.transform;

            GameObject rUpperArm = CreatePrimitive(PrimitiveType.Cylinder, "R_UpperArm", rightArm.transform);
            rUpperArm.transform.localPosition = new Vector3(0, -0.16f, 0);
            rUpperArm.transform.localScale = new Vector3(0.12f, 0.16f, 0.12f);
            SetMaterial(rUpperArm, coatMat);

            GameObject rForearm = CreatePrimitive(PrimitiveType.Cylinder, "R_Forearm", rightArm.transform);
            rForearm.transform.localPosition = new Vector3(0, -0.32f, 0);
            rForearm.transform.localScale = new Vector3(0.11f, 0.16f, 0.11f);
            SetMaterial(rForearm, coatMat);

            // Right Glove / Hand (Palm, Thumb, Fingers)
            BuildHand(rightArm.transform, gloveMat, false);

            // Tool mount socket on right hand
            GameObject toolSocket = new GameObject("RightHandAnchor");
            toolSocket.transform.SetParent(rightArm.transform, false);
            toolSocket.transform.localPosition = new Vector3(0, -0.52f, 0.12f);
            rightHandAnchor = toolSocket.transform;

            // --- LEGS & SOVIET KIRZA BOOTS ---
            // Left Leg
            GameObject leftLeg = new GameObject("LeftLeg");
            leftLeg.transform.SetParent(bodyContainer.transform, false);
            leftLeg.transform.localPosition = new Vector3(-0.15f, 0.76f, 0);
            leftLegTransform = leftLeg.transform;

            GameObject lPants = CreatePrimitive(PrimitiveType.Cylinder, "L_Pants", leftLeg.transform);
            lPants.transform.localPosition = new Vector3(0, -0.22f, 0);
            lPants.transform.localScale = new Vector3(0.14f, 0.24f, 0.14f);
            SetMaterial(lPants, coatMat);

            // Left Kirza Boot (Shaft, Foot, Heel, Sole)
            BuildBoot(leftLeg.transform, bootMat);

            // Right Leg
            GameObject rightLeg = new GameObject("RightLeg");
            rightLeg.transform.SetParent(bodyContainer.transform, false);
            rightLeg.transform.localPosition = new Vector3(0.15f, 0.76f, 0);
            rightLegTransform = rightLeg.transform;

            GameObject rPants = CreatePrimitive(PrimitiveType.Cylinder, "R_Pants", rightLeg.transform);
            rPants.transform.localPosition = new Vector3(0, -0.22f, 0);
            rPants.transform.localScale = new Vector3(0.14f, 0.24f, 0.14f);
            SetMaterial(rPants, coatMat);

            // Right Kirza Boot (Shaft, Foot, Heel, Sole)
            BuildBoot(rightLeg.transform, bootMat);

            return root;
        }

        private static void BuildHand(Transform armParent, Material gloveMat, bool isLeft)
        {
            // Wrist Cuff
            GameObject cuff = CreatePrimitive(PrimitiveType.Cylinder, "WristCuff", armParent);
            cuff.transform.localPosition = new Vector3(0, -0.42f, 0);
            cuff.transform.localScale = new Vector3(0.12f, 0.03f, 0.12f);
            SetMaterial(cuff, gloveMat);

            // Palm (flat rounded box)
            GameObject palm = CreatePrimitive(PrimitiveType.Cube, "Palm", armParent);
            palm.transform.localPosition = new Vector3(0, -0.48f, 0.02f);
            palm.transform.localScale = new Vector3(0.09f, 0.10f, 0.05f);
            SetMaterial(palm, gloveMat);

            // Thumb (angled outward)
            float thumbSide = isLeft ? 0.055f : -0.055f;
            GameObject thumb = CreatePrimitive(PrimitiveType.Capsule, "Thumb", armParent);
            thumb.transform.localPosition = new Vector3(thumbSide, -0.47f, 0.04f);
            thumb.transform.localRotation = Quaternion.Euler(20f, isLeft ? 30f : -30f, 0);
            thumb.transform.localScale = new Vector3(0.035f, 0.045f, 0.035f);
            SetMaterial(thumb, gloveMat);

            // Fingers (curled forward naturally)
            GameObject fingers = CreatePrimitive(PrimitiveType.Capsule, "Fingers", armParent);
            fingers.transform.localPosition = new Vector3(0, -0.54f, 0.03f);
            fingers.transform.localRotation = Quaternion.Euler(35f, 0, 0);
            fingers.transform.localScale = new Vector3(0.08f, 0.05f, 0.045f);
            SetMaterial(fingers, gloveMat);
        }

        private static void BuildBoot(Transform legParent, Material bootMat)
        {
            // Kirza Boot Shaft
            GameObject shaft = CreatePrimitive(PrimitiveType.Cylinder, "BootShaft", legParent);
            shaft.transform.localPosition = new Vector3(0, -0.48f, 0);
            shaft.transform.localScale = new Vector3(0.15f, 0.16f, 0.15f);
            SetMaterial(shaft, bootMat);

            // Foot Base
            GameObject foot = CreatePrimitive(PrimitiveType.Cube, "BootFoot", legParent);
            foot.transform.localPosition = new Vector3(0, -0.66f, 0.05f);
            foot.transform.localScale = new Vector3(0.13f, 0.10f, 0.24f);
            SetMaterial(foot, bootMat);

            // Sole & Heel
            GameObject sole = CreatePrimitive(PrimitiveType.Cube, "BootSole", legParent);
            sole.transform.localPosition = new Vector3(0, -0.72f, 0.05f);
            sole.transform.localScale = new Vector3(0.14f, 0.03f, 0.25f);
            SetMaterial(sole, bootMat);

            GameObject heel = CreatePrimitive(PrimitiveType.Cube, "BootHeel", legParent);
            heel.transform.localPosition = new Vector3(0, -0.72f, -0.04f);
            heel.transform.localScale = new Vector3(0.14f, 0.05f, 0.08f);
            SetMaterial(heel, bootMat);
        }

        private static void BuildHat(HatType hatType, Transform head, Material hatMat, Material accentMat)
        {
            switch (hatType)
            {
                case HatType.ChefToque:
                {
                    // Headband
                    GameObject band = CreatePrimitive(PrimitiveType.Cylinder, "ChefBand", head);
                    band.transform.localPosition = new Vector3(0, 0.18f, 0);
                    band.transform.localScale = new Vector3(0.38f, 0.05f, 0.38f);
                    SetMaterial(band, hatMat);

                    // Golden Star Cockade Badge
                    GameObject star = CreatePrimitive(PrimitiveType.Cube, "StarBadge", head);
                    star.transform.localPosition = new Vector3(0, 0.18f, 0.20f);
                    star.transform.localRotation = Quaternion.Euler(0, 0, 45f);
                    star.transform.localScale = new Vector3(0.04f, 0.04f, 0.02f);
                    SetMaterial(star, accentMat);

                    // Pleated Puffy Crown
                    GameObject puff = CreatePrimitive(PrimitiveType.Cylinder, "ChefCrown", head);
                    puff.transform.localPosition = new Vector3(0, 0.34f, 0);
                    puff.transform.localScale = new Vector3(0.42f, 0.16f, 0.42f);
                    SetMaterial(puff, hatMat);

                    GameObject topPuff = CreatePrimitive(PrimitiveType.Sphere, "ChefTop", head);
                    topPuff.transform.localPosition = new Vector3(0, 0.44f, 0);
                    topPuff.transform.localScale = new Vector3(0.46f, 0.14f, 0.46f);
                    SetMaterial(topPuff, hatMat);
                    break;
                }
                case HatType.SovietUshanka:
                {
                    // Cap Base
                    GameObject cap = CreatePrimitive(PrimitiveType.Sphere, "UshankaBase", head);
                    cap.transform.localPosition = new Vector3(0, 0.16f, 0);
                    cap.transform.localScale = new Vector3(0.40f, 0.26f, 0.38f);
                    SetMaterial(cap, hatMat);

                    // Front Fur Flap with Soviet Red Star
                    GameObject frontFlap = CreatePrimitive(PrimitiveType.Cube, "FrontFlap", head);
                    frontFlap.transform.localPosition = new Vector3(0, 0.19f, 0.17f);
                    frontFlap.transform.localScale = new Vector3(0.26f, 0.14f, 0.06f);
                    SetMaterial(frontFlap, hatMat);

                    GameObject redStar = CreatePrimitive(PrimitiveType.Cube, "RedStar", head);
                    redStar.transform.localPosition = new Vector3(0, 0.20f, 0.21f);
                    redStar.transform.localRotation = Quaternion.Euler(0, 0, 45f);
                    redStar.transform.localScale = new Vector3(0.05f, 0.05f, 0.02f);
                    SetMaterial(redStar, accentMat);

                    // Left & Right Ear Flaps (tied up)
                    GameObject earFlapL = CreatePrimitive(PrimitiveType.Cube, "EarFlapL", head);
                    earFlapL.transform.localPosition = new Vector3(-0.19f, 0.16f, 0);
                    earFlapL.transform.localScale = new Vector3(0.06f, 0.18f, 0.22f);
                    SetMaterial(earFlapL, hatMat);

                    GameObject earFlapR = CreatePrimitive(PrimitiveType.Cube, "EarFlapR", head);
                    earFlapR.transform.localPosition = new Vector3(0.19f, 0.16f, 0);
                    earFlapR.transform.localScale = new Vector3(0.06f, 0.18f, 0.22f);
                    SetMaterial(earFlapR, hatMat);
                    break;
                }
                case HatType.SafetyHelmet:
                {
                    // Domed Helmet
                    GameObject helmet = CreatePrimitive(PrimitiveType.Sphere, "HelmetDome", head);
                    helmet.transform.localPosition = new Vector3(0, 0.18f, 0);
                    helmet.transform.localScale = new Vector3(0.42f, 0.26f, 0.44f);
                    SetMaterial(helmet, hatMat);

                    // Visor Brim
                    GameObject brim = CreatePrimitive(PrimitiveType.Cylinder, "HelmetBrim", head);
                    brim.transform.localPosition = new Vector3(0, 0.12f, 0.06f);
                    brim.transform.localScale = new Vector3(0.44f, 0.02f, 0.46f);
                    SetMaterial(brim, hatMat);

                    // Headlamp
                    GameObject lamp = CreatePrimitive(PrimitiveType.Cylinder, "HeadLamp", head);
                    lamp.transform.localPosition = new Vector3(0, 0.20f, 0.22f);
                    lamp.transform.localRotation = Quaternion.Euler(90f, 0, 0);
                    lamp.transform.localScale = new Vector3(0.08f, 0.04f, 0.08f);
                    SetMaterial(lamp, accentMat);
                    break;
                }
                case HatType.WorkerCap:
                {
                    // Peaked Flat Cap (Кепка-восьмиклинка)
                    GameObject cap = CreatePrimitive(PrimitiveType.Cylinder, "FlatCap", head);
                    cap.transform.localPosition = new Vector3(0, 0.18f, -0.02f);
                    cap.transform.localRotation = Quaternion.Euler(10f, 0, 0);
                    cap.transform.localScale = new Vector3(0.40f, 0.10f, 0.42f);
                    SetMaterial(cap, hatMat);

                    // Visor
                    GameObject visor = CreatePrimitive(PrimitiveType.Cube, "CapVisor", head);
                    visor.transform.localPosition = new Vector3(0, 0.14f, 0.19f);
                    visor.transform.localRotation = Quaternion.Euler(15f, 0, 0);
                    visor.transform.localScale = new Vector3(0.24f, 0.02f, 0.10f);
                    SetMaterial(visor, accentMat);
                    break;
                }
            }
        }

        private static void BuildFacial(FacialType facial, Transform head, Material hairMat, Material metalMat)
        {
            switch (facial)
            {
                case FacialType.CurledMustache:
                {
                    // Cossack curled handlebar mustache
                    GameObject musL = CreatePrimitive(PrimitiveType.Cylinder, "Mustache_L", head);
                    musL.transform.localPosition = new Vector3(-0.07f, -0.05f, 0.17f);
                    musL.transform.localRotation = Quaternion.Euler(0, 25f, -30f);
                    musL.transform.localScale = new Vector3(0.025f, 0.065f, 0.025f);
                    SetMaterial(musL, hairMat);

                    GameObject musR = CreatePrimitive(PrimitiveType.Cylinder, "Mustache_R", head);
                    musR.transform.localPosition = new Vector3(0.07f, -0.05f, 0.17f);
                    musR.transform.localRotation = Quaternion.Euler(0, -25f, 30f);
                    musR.transform.localScale = new Vector3(0.025f, 0.065f, 0.025f);
                    SetMaterial(musR, hairMat);
                    break;
                }
                case FacialType.BushyMustache:
                {
                    // Solid Soviet worker's mustache
                    GameObject mustache = CreatePrimitive(PrimitiveType.Cube, "BushyMustache", head);
                    mustache.transform.localPosition = new Vector3(0, -0.05f, 0.175f);
                    mustache.transform.localScale = new Vector3(0.15f, 0.04f, 0.03f);
                    SetMaterial(mustache, hairMat);
                    break;
                }
                case FacialType.WeldingGoggles:
                {
                    // Forehead Welding Goggles
                    GameObject strap = CreatePrimitive(PrimitiveType.Cylinder, "GoggleStrap", head);
                    strap.transform.localPosition = new Vector3(0, 0.10f, 0);
                    strap.transform.localScale = new Vector3(0.38f, 0.02f, 0.36f);
                    SetMaterial(strap, hairMat);

                    GameObject goggleL = CreatePrimitive(PrimitiveType.Cylinder, "Goggle_L", head);
                    goggleL.transform.localPosition = new Vector3(-0.09f, 0.10f, 0.17f);
                    goggleL.transform.localRotation = Quaternion.Euler(90f, 0, 0);
                    goggleL.transform.localScale = new Vector3(0.075f, 0.03f, 0.075f);
                    SetMaterial(goggleL, metalMat);

                    GameObject goggleR = CreatePrimitive(PrimitiveType.Cylinder, "Goggle_R", head);
                    goggleR.transform.localPosition = new Vector3(0.09f, 0.10f, 0.17f);
                    goggleR.transform.localRotation = Quaternion.Euler(90f, 0, 0);
                    goggleR.transform.localScale = new Vector3(0.075f, 0.03f, 0.075f);
                    SetMaterial(goggleR, metalMat);
                    break;
                }
                default:
                    break;
            }
        }

        public static GameObject BuildToolProp(int toolId, Transform parent)
        {
            GameObject prop = new GameObject($"ToolProp_{toolId}");
            prop.transform.SetParent(parent, false);

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
            Material metalMat = CreateMaterial(shader, new Color(0.65f, 0.68f, 0.70f), 0.40f, 0.75f);
            Material darkMat = CreateMaterial(shader, new Color(0.20f, 0.20f, 0.20f), 0.05f);
            Material woodMat = CreateMaterial(shader, new Color(0.50f, 0.32f, 0.16f), 0.05f);

            switch (toolId)
            {
                case 1: // Bucket
                {
                    GameObject b = CreatePrimitive(PrimitiveType.Cylinder, "BucketBody", prop.transform);
                    b.transform.localPosition = new Vector3(0, -0.15f, 0);
                    b.transform.localScale = new Vector3(0.26f, 0.16f, 0.26f);
                    SetMaterial(b, metalMat);
                    break;
                }
                case 2: // Vacuum
                {
                    GameObject pipe = CreatePrimitive(PrimitiveType.Cylinder, "VacuumPipe", prop.transform);
                    pipe.transform.localPosition = new Vector3(0, 0, 0.25f);
                    pipe.transform.localRotation = Quaternion.Euler(90f, 0, 0);
                    pipe.transform.localScale = new Vector3(0.08f, 0.35f, 0.08f);
                    SetMaterial(pipe, darkMat);

                    GameObject nozzle = CreatePrimitive(PrimitiveType.Cube, "Nozzle", prop.transform);
                    nozzle.transform.localPosition = new Vector3(0, 0, 0.60f);
                    nozzle.transform.localScale = new Vector3(0.24f, 0.06f, 0.12f);
                    SetMaterial(nozzle, metalMat);
                    break;
                }
                case 3: // Flamethrower
                {
                    GameObject body = CreatePrimitive(PrimitiveType.Cylinder, "FlameBody", prop.transform);
                    body.transform.localPosition = new Vector3(0, 0, 0.2f);
                    body.transform.localRotation = Quaternion.Euler(90f, 0, 0);
                    body.transform.localScale = new Vector3(0.12f, 0.28f, 0.12f);
                    SetMaterial(body, darkMat);

                    GameObject tank = CreatePrimitive(PrimitiveType.Sphere, "GasTank", prop.transform);
                    tank.transform.localPosition = new Vector3(0, -0.12f, 0.15f);
                    tank.transform.localScale = new Vector3(0.16f, 0.20f, 0.16f);
                    SetMaterial(tank, CreateMaterial(shader, new Color(0.85f, 0.25f, 0.15f))); // Red tank
                    break;
                }
                case 10: // Holding Potato in hands
                {
                    GameObject potato = CreatePrimitive(PrimitiveType.Sphere, "HeldPotato", prop.transform);
                    potato.transform.localPosition = new Vector3(0, 0, 0.12f);
                    potato.transform.localScale = new Vector3(0.16f, 0.22f, 0.16f);
                    SetMaterial(potato, CreateMaterial(shader, new Color(0.68f, 0.50f, 0.28f), 0.04f));
                    break;
                }
                case 11: // Carrying Sack of potatoes
                {
                    GameObject sack = CreatePrimitive(PrimitiveType.Cylinder, "HeldSack", prop.transform);
                    sack.transform.localPosition = new Vector3(0, -0.05f, 0.20f);
                    sack.transform.localRotation = Quaternion.Euler(75f, 0, 0);
                    sack.transform.localScale = new Vector3(0.30f, 0.42f, 0.30f);
                    SetMaterial(sack, CreateMaterial(shader, new Color(0.58f, 0.46f, 0.32f), 0.04f));
                    break;
                }
                case 4: // Peeling Knife / Y-Peeler
                default:
                {
                    GameObject handle = CreatePrimitive(PrimitiveType.Cylinder, "KnifeHandle", prop.transform);
                    handle.transform.localPosition = new Vector3(0, 0, 0.05f);
                    handle.transform.localRotation = Quaternion.Euler(90f, 0, 0);
                    handle.transform.localScale = new Vector3(0.035f, 0.09f, 0.035f);
                    SetMaterial(handle, woodMat);

                    GameObject blade = CreatePrimitive(PrimitiveType.Cube, "KnifeBlade", prop.transform);
                    blade.transform.localPosition = new Vector3(0, 0, 0.20f);
                    blade.transform.localScale = new Vector3(0.01f, 0.045f, 0.15f);
                    SetMaterial(blade, metalMat);
                    break;
                }
            }

            return prop;
        }

        private static GameObject CreatePrimitive(PrimitiveType type, string name, Transform parent)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(parent, false);

            // Strip colliders so remote puppet never interferes with physics
            Collider col = obj.GetComponent<Collider>();
            if (col != null)
                GameObject.Destroy(col);

            return obj;
        }

        private static Material CreateMaterial(Shader shader, Color color, float smoothness = 0.04f, float metallic = 0.0f)
        {
            Material mat = new Material(shader);
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            return mat;
        }

        private static void SetMaterial(GameObject obj, Material mat)
        {
            MeshRenderer rend = obj.GetComponent<MeshRenderer>();
            if (rend != null)
                rend.material = mat;
        }
    }
}
