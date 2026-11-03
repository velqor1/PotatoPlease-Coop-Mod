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
        Beret,
        SafetyHelmet,
        BakerCap
    }

    public enum FacialType
    {
        CurledMustache,
        Freckles,
        WeldingGoggles,
        CleanShaven
    }

    public static class PuppetModelBuilder
    {
        public static readonly CharacterVisualConfig[] Presets = new CharacterVisualConfig[]
        {
            // Player 1: Master Chef (White coat, royal navy apron, tall chef toque, mustache)
            new CharacterVisualConfig
            {
                Title = "ШЕФ-ПОВАР",
                CoatColor = new Color(0.95f, 0.95f, 0.95f),
                ApronColor = new Color(0.12f, 0.24f, 0.45f),
                HatColor = new Color(0.98f, 0.98f, 0.98f),
                HatAccentColor = new Color(0.85f, 0.65f, 0.15f), // Gold
                GloveColor = new Color(0.85f, 0.85f, 0.85f),
                BootColor = new Color(0.15f, 0.15f, 0.15f),
                Hat = HatType.ChefToque,
                Facial = FacialType.CurledMustache
            },
            // Player 2: Peeler Specialist (Warm beige coat, crimson red apron, red beret)
            new CharacterVisualConfig
            {
                Title = "МАСТЕР ОЧИСТКИ",
                CoatColor = new Color(0.85f, 0.80f, 0.70f),
                ApronColor = new Color(0.75f, 0.18f, 0.15f),
                HatColor = new Color(0.70f, 0.12f, 0.12f),
                HatAccentColor = new Color(0.2f, 0.2f, 0.2f),
                GloveColor = new Color(0.95f, 0.45f, 0.15f), // Orange grip gloves
                BootColor = new Color(0.25f, 0.18f, 0.12f),
                Hat = HatType.Beret,
                Facial = FacialType.Freckles
            },
            // Player 3: Press Mechanic (Dark work shirt, forest green heavy apron, yellow helmet & goggles)
            new CharacterVisualConfig
            {
                Title = "МЕХАНИК ПРЕССА",
                CoatColor = new Color(0.35f, 0.38f, 0.42f),
                ApronColor = new Color(0.18f, 0.42f, 0.25f),
                HatColor = new Color(0.95f, 0.80f, 0.10f), // Yellow hardhat
                HatAccentColor = new Color(0.1f, 0.1f, 0.1f),
                GloveColor = new Color(0.25f, 0.25f, 0.25f), // Heavy black gloves
                BootColor = new Color(0.18f, 0.18f, 0.18f),
                Hat = HatType.SafetyHelmet,
                Facial = FacialType.WeldingGoggles
            },
            // Player 4: Kitchen Apprentice (Light slate coat, royal purple apron, paper baker cap)
            new CharacterVisualConfig
            {
                Title = "ПОДМАСТЕРЬЕ",
                CoatColor = new Color(0.90f, 0.90f, 0.92f),
                ApronColor = new Color(0.40f, 0.20f, 0.55f),
                HatColor = new Color(0.95f, 0.95f, 0.95f),
                HatAccentColor = new Color(0.7f, 0.15f, 0.2f),
                GloveColor = new Color(0.3f, 0.6f, 0.9f), // Blue latex gloves
                BootColor = new Color(0.2f, 0.2f, 0.2f),
                Hat = HatType.BakerCap,
                Facial = FacialType.CleanShaven
            }
        };

        public static GameObject BuildPuppet(int skinIndex, out Transform headTransform, out Transform rightHandAnchor, out Transform leftArmTransform, out Transform rightArmTransform, out Transform leftLegTransform, out Transform rightLegTransform)
        {
            skinIndex = Mathf.Clamp(skinIndex, 0, Presets.Length - 1);
            CharacterVisualConfig config = Presets[skinIndex];

            GameObject root = new GameObject($"Puppet_{config.Title}");

            // Standard unlit / simple lit shader for clean readable visuals
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");

            Material coatMat = CreateMaterial(shader, config.CoatColor);
            Material apronMat = CreateMaterial(shader, config.ApronColor);
            Material hatMat = CreateMaterial(shader, config.HatColor);
            Material hatAccentMat = CreateMaterial(shader, config.HatAccentColor);
            Material skinMat = CreateMaterial(shader, new Color(0.96f, 0.78f, 0.64f)); // Healthy skin
            Material eyeWhiteMat = CreateMaterial(shader, Color.white);
            Material eyePupilMat = CreateMaterial(shader, new Color(0.1f, 0.1f, 0.15f));
            Material gloveMat = CreateMaterial(shader, config.GloveColor);
            Material bootMat = CreateMaterial(shader, config.BootColor);
            Material beltMat = CreateMaterial(shader, new Color(0.25f, 0.15f, 0.08f)); // Leather
            Material buckleMat = CreateMaterial(shader, new Color(0.85f, 0.75f, 0.25f)); // Brass
            Material hairMat = CreateMaterial(shader, new Color(0.3f, 0.2f, 0.12f));

            // --- PELVIS & TORSO ---
            GameObject torso = CreatePrimitive(PrimitiveType.Capsule, "Torso", root.transform);
            torso.transform.localPosition = new Vector3(0, 1.05f, 0);
            torso.transform.localScale = new Vector3(0.55f, 0.45f, 0.45f);
            SetMaterial(torso, coatMat);

            // Apron front plate
            GameObject apron = CreatePrimitive(PrimitiveType.Cube, "Apron", root.transform);
            apron.transform.localPosition = new Vector3(0, 1.02f, 0.22f);
            apron.transform.localScale = new Vector3(0.48f, 0.65f, 0.05f);
            SetMaterial(apron, apronMat);

            // Belt & Brass Buckle
            GameObject belt = CreatePrimitive(PrimitiveType.Cylinder, "Belt", root.transform);
            belt.transform.localPosition = new Vector3(0, 0.85f, 0);
            belt.transform.localScale = new Vector3(0.58f, 0.04f, 0.48f);
            SetMaterial(belt, beltMat);

            GameObject buckle = CreatePrimitive(PrimitiveType.Cube, "Buckle", root.transform);
            buckle.transform.localPosition = new Vector3(0, 0.85f, 0.26f);
            buckle.transform.localScale = new Vector3(0.12f, 0.08f, 0.04f);
            SetMaterial(buckle, buckleMat);

            // --- HEAD HIERARCHY ---
            GameObject headObj = new GameObject("HeadAnchor");
            headObj.transform.SetParent(root.transform, false);
            headObj.transform.localPosition = new Vector3(0, 1.55f, 0);
            headTransform = headObj.transform;

            GameObject headMesh = CreatePrimitive(PrimitiveType.Sphere, "Head", headTransform);
            headMesh.transform.localPosition = Vector3.zero;
            headMesh.transform.localScale = new Vector3(0.42f, 0.45f, 0.42f);
            SetMaterial(headMesh, skinMat);

            // Eyes
            GameObject leftEye = CreatePrimitive(PrimitiveType.Sphere, "LeftEye", headTransform);
            leftEye.transform.localPosition = new Vector3(-0.10f, 0.05f, 0.19f);
            leftEye.transform.localScale = new Vector3(0.08f, 0.08f, 0.04f);
            SetMaterial(leftEye, eyeWhiteMat);

            GameObject leftPupil = CreatePrimitive(PrimitiveType.Sphere, "LeftPupil", headTransform);
            leftPupil.transform.localPosition = new Vector3(-0.10f, 0.05f, 0.21f);
            leftPupil.transform.localScale = new Vector3(0.04f, 0.04f, 0.02f);
            SetMaterial(leftPupil, eyePupilMat);

            GameObject rightEye = CreatePrimitive(PrimitiveType.Sphere, "RightEye", headTransform);
            rightEye.transform.localPosition = new Vector3(0.10f, 0.05f, 0.19f);
            rightEye.transform.localScale = new Vector3(0.08f, 0.08f, 0.04f);
            SetMaterial(rightEye, eyeWhiteMat);

            GameObject rightPupil = CreatePrimitive(PrimitiveType.Sphere, "RightPupil", headTransform);
            rightPupil.transform.localPosition = new Vector3(0.10f, 0.05f, 0.21f);
            rightPupil.transform.localScale = new Vector3(0.04f, 0.04f, 0.02f);
            SetMaterial(rightPupil, eyePupilMat);

            // Nose
            GameObject nose = CreatePrimitive(PrimitiveType.Sphere, "Nose", headTransform);
            nose.transform.localPosition = new Vector3(0, 0.01f, 0.22f);
            nose.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
            SetMaterial(nose, skinMat);

            // --- HAT BUILDER ---
            BuildHat(config.Hat, headTransform, hatMat, hatAccentMat);

            // --- FACIAL FEATURES ---
            BuildFacial(config.Facial, headTransform, hairMat, buckleMat);

            // --- ARMS ---
            // Left Arm
            GameObject leftArm = new GameObject("LeftArm");
            leftArm.transform.SetParent(root.transform, false);
            leftArm.transform.localPosition = new Vector3(-0.35f, 1.35f, 0);
            leftArmTransform = leftArm.transform;

            GameObject lUpperArm = CreatePrimitive(PrimitiveType.Cylinder, "L_UpperArm", leftArm.transform);
            lUpperArm.transform.localPosition = new Vector3(0, -0.22f, 0);
            lUpperArm.transform.localScale = new Vector3(0.12f, 0.20f, 0.12f);
            SetMaterial(lUpperArm, coatMat);

            GameObject lHand = CreatePrimitive(PrimitiveType.Sphere, "L_Hand", leftArm.transform);
            lHand.transform.localPosition = new Vector3(0, -0.45f, 0);
            lHand.transform.localScale = new Vector3(0.14f, 0.14f, 0.14f);
            SetMaterial(lHand, gloveMat);

            // Right Arm
            GameObject rightArm = new GameObject("RightArm");
            rightArm.transform.SetParent(root.transform, false);
            rightArm.transform.localPosition = new Vector3(0.35f, 1.35f, 0);
            rightArmTransform = rightArm.transform;

            GameObject rUpperArm = CreatePrimitive(PrimitiveType.Cylinder, "R_UpperArm", rightArm.transform);
            rUpperArm.transform.localPosition = new Vector3(0, -0.22f, 0);
            rUpperArm.transform.localScale = new Vector3(0.12f, 0.20f, 0.12f);
            SetMaterial(rUpperArm, coatMat);

            GameObject rHand = CreatePrimitive(PrimitiveType.Sphere, "R_Hand", rightArm.transform);
            rHand.transform.localPosition = new Vector3(0, -0.45f, 0);
            rHand.transform.localScale = new Vector3(0.14f, 0.14f, 0.14f);
            SetMaterial(rHand, gloveMat);

            // Tool mount socket on right hand
            GameObject toolSocket = new GameObject("RightHandAnchor");
            toolSocket.transform.SetParent(rightArm.transform, false);
            toolSocket.transform.localPosition = new Vector3(0, -0.48f, 0.15f);
            rightHandAnchor = toolSocket.transform;

            // --- LEGS ---
            // Left Leg
            GameObject leftLeg = new GameObject("LeftLeg");
            leftLeg.transform.SetParent(root.transform, false);
            leftLeg.transform.localPosition = new Vector3(-0.16f, 0.75f, 0);
            leftLegTransform = leftLeg.transform;

            GameObject lThigh = CreatePrimitive(PrimitiveType.Cylinder, "L_Leg", leftLeg.transform);
            lThigh.transform.localPosition = new Vector3(0, -0.32f, 0);
            lThigh.transform.localScale = new Vector3(0.15f, 0.32f, 0.15f);
            SetMaterial(lThigh, beltMat);

            GameObject lBoot = CreatePrimitive(PrimitiveType.Cube, "L_Boot", leftLeg.transform);
            lBoot.transform.localPosition = new Vector3(0, -0.68f, 0.05f);
            lBoot.transform.localScale = new Vector3(0.17f, 0.15f, 0.26f);
            SetMaterial(lBoot, bootMat);

            // Right Leg
            GameObject rightLeg = new GameObject("RightLeg");
            rightLeg.transform.SetParent(root.transform, false);
            rightLeg.transform.localPosition = new Vector3(0.16f, 0.75f, 0);
            rightLegTransform = rightLeg.transform;

            GameObject rThigh = CreatePrimitive(PrimitiveType.Cylinder, "R_Leg", rightLeg.transform);
            rThigh.transform.localPosition = new Vector3(0, -0.32f, 0);
            rThigh.transform.localScale = new Vector3(0.15f, 0.32f, 0.15f);
            SetMaterial(rThigh, beltMat);

            GameObject rBoot = CreatePrimitive(PrimitiveType.Cube, "R_Boot", rightLeg.transform);
            rBoot.transform.localPosition = new Vector3(0, -0.68f, 0.05f);
            rBoot.transform.localScale = new Vector3(0.17f, 0.15f, 0.26f);
            SetMaterial(rBoot, bootMat);

            return root;
        }

        private static void BuildHat(HatType hatType, Transform head, Material hatMat, Material accentMat)
        {
            switch (hatType)
            {
                case HatType.ChefToque:
                {
                    // Cylindrical headband
                    GameObject band = CreatePrimitive(PrimitiveType.Cylinder, "ChefBand", head);
                    band.transform.localPosition = new Vector3(0, 0.22f, 0);
                    band.transform.localScale = new Vector3(0.44f, 0.05f, 0.44f);
                    SetMaterial(band, accentMat);

                    // Puffy crown
                    GameObject puff = CreatePrimitive(PrimitiveType.Sphere, "ChefPuff", head);
                    puff.transform.localPosition = new Vector3(0, 0.42f, 0);
                    puff.transform.localScale = new Vector3(0.55f, 0.32f, 0.55f);
                    SetMaterial(puff, hatMat);
                    break;
                }
                case HatType.Beret:
                {
                    GameObject beret = CreatePrimitive(PrimitiveType.Cylinder, "Beret", head);
                    beret.transform.localPosition = new Vector3(0.04f, 0.24f, 0);
                    beret.transform.localRotation = Quaternion.Euler(12f, 0, -15f);
                    beret.transform.localScale = new Vector3(0.52f, 0.08f, 0.52f);
                    SetMaterial(beret, hatMat);

                    GameObject stem = CreatePrimitive(PrimitiveType.Sphere, "BeretStem", head);
                    stem.transform.localPosition = new Vector3(0.05f, 0.32f, 0);
                    stem.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
                    SetMaterial(stem, accentMat);
                    break;
                }
                case HatType.SafetyHelmet:
                {
                    GameObject helmet = CreatePrimitive(PrimitiveType.Sphere, "Helmet", head);
                    helmet.transform.localPosition = new Vector3(0, 0.20f, 0);
                    helmet.transform.localScale = new Vector3(0.48f, 0.35f, 0.50f);
                    SetMaterial(helmet, hatMat);

                    GameObject lamp = CreatePrimitive(PrimitiveType.Cube, "HeadLamp", head);
                    lamp.transform.localPosition = new Vector3(0, 0.24f, 0.25f);
                    lamp.transform.localScale = new Vector3(0.08f, 0.06f, 0.06f);
                    SetMaterial(lamp, accentMat);
                    break;
                }
                case HatType.BakerCap:
                {
                    GameObject cap = CreatePrimitive(PrimitiveType.Cube, "BakerCap", head);
                    cap.transform.localPosition = new Vector3(0, 0.26f, 0);
                    cap.transform.localScale = new Vector3(0.24f, 0.16f, 0.42f);
                    SetMaterial(cap, hatMat);

                    GameObject stripe = CreatePrimitive(PrimitiveType.Cube, "CapStripe", head);
                    stripe.transform.localPosition = new Vector3(0, 0.22f, 0);
                    stripe.transform.localScale = new Vector3(0.25f, 0.03f, 0.43f);
                    SetMaterial(stripe, accentMat);
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
                    GameObject mustacheL = CreatePrimitive(PrimitiveType.Cylinder, "Mustache_L", head);
                    mustacheL.transform.localPosition = new Vector3(-0.08f, -0.06f, 0.21f);
                    mustacheL.transform.localRotation = Quaternion.Euler(0, 20f, -25f);
                    mustacheL.transform.localScale = new Vector3(0.03f, 0.07f, 0.03f);
                    SetMaterial(mustacheL, hairMat);

                    GameObject mustacheR = CreatePrimitive(PrimitiveType.Cylinder, "Mustache_R", head);
                    mustacheR.transform.localPosition = new Vector3(0.08f, -0.06f, 0.21f);
                    mustacheR.transform.localRotation = Quaternion.Euler(0, -20f, 25f);
                    mustacheR.transform.localScale = new Vector3(0.03f, 0.07f, 0.03f);
                    SetMaterial(mustacheR, hairMat);
                    break;
                }
                case FacialType.WeldingGoggles:
                {
                    GameObject gogglesL = CreatePrimitive(PrimitiveType.Cylinder, "Goggle_L", head);
                    gogglesL.transform.localPosition = new Vector3(-0.11f, 0.14f, 0.20f);
                    gogglesL.transform.localRotation = Quaternion.Euler(90f, 0, 0);
                    gogglesL.transform.localScale = new Vector3(0.09f, 0.03f, 0.09f);
                    SetMaterial(gogglesL, metalMat);

                    GameObject gogglesR = CreatePrimitive(PrimitiveType.Cylinder, "Goggle_R", head);
                    gogglesR.transform.localPosition = new Vector3(0.11f, 0.14f, 0.20f);
                    gogglesR.transform.localRotation = Quaternion.Euler(90f, 0, 0);
                    gogglesR.transform.localScale = new Vector3(0.09f, 0.03f, 0.09f);
                    SetMaterial(gogglesR, metalMat);
                    break;
                }
                default:
                    break;
            }
        }

        public static GameObject BuildToolProp(int toolId, Transform parent)
        {
            // 0: Hand, 1: Bucket, 2: Vacuum, 3: Flamethrower, 4: Knife/Peeler
            GameObject prop = new GameObject($"ToolProp_{toolId}");
            prop.transform.SetParent(parent, false);

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
            Material metalMat = CreateMaterial(shader, new Color(0.7f, 0.72f, 0.75f));
            Material darkMat = CreateMaterial(shader, new Color(0.2f, 0.2f, 0.2f));
            Material woodMat = CreateMaterial(shader, new Color(0.55f, 0.35f, 0.18f));

            switch (toolId)
            {
                case 1: // Bucket
                {
                    GameObject b = CreatePrimitive(PrimitiveType.Cylinder, "BucketBody", prop.transform);
                    b.transform.localPosition = new Vector3(0, -0.15f, 0);
                    b.transform.localScale = new Vector3(0.28f, 0.18f, 0.28f);
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
                case 4: // Peeling Knife / Y-Peeler
                default:
                {
                    GameObject handle = CreatePrimitive(PrimitiveType.Cylinder, "KnifeHandle", prop.transform);
                    handle.transform.localPosition = new Vector3(0, 0, 0.05f);
                    handle.transform.localRotation = Quaternion.Euler(90f, 0, 0);
                    handle.transform.localScale = new Vector3(0.04f, 0.10f, 0.04f);
                    SetMaterial(handle, woodMat);

                    GameObject blade = CreatePrimitive(PrimitiveType.Cube, "KnifeBlade", prop.transform);
                    blade.transform.localPosition = new Vector3(0, 0, 0.22f);
                    blade.transform.localScale = new Vector3(0.01f, 0.05f, 0.16f);
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

            // Strip colliders so remote puppet doesn't collide with the local player or physics
            Collider col = obj.GetComponent<Collider>();
            if (col != null)
                GameObject.Destroy(col);

            return obj;
        }

        private static Material CreateMaterial(Shader shader, Color color)
        {
            Material mat = new Material(shader);
            mat.color = color;
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
