using HarmonyLib;
using Story;
using TheRedPlague.Compatibility;
using TheRedPlague.Data;
using TheRedPlague.Managers;
using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;

namespace TheRedPlague.Patches.Infection;

[HarmonyPatch(typeof(InfectedMixin))]
public static class InfectedMixinPatcher
{
    private static readonly int InfectionHeightStrength = Shader.PropertyToID("_InfectionHeightStrength");
    private static readonly int SpecColor = Shader.PropertyToID("_SpecColor");
    private static readonly int InfectionScale = Shader.PropertyToID("_InfectionScale");
    private static readonly int InfectionOffset = Shader.PropertyToID("_InfectionOffset");
    private const int MinInfectionValue = 1;
    
    private static readonly Texture2D PrecursorDroidRedIllumTexture =
        Plugin.CreaturesBundle.LoadAsset<Texture2D>("Precursor_Droid_illum_evil");


    [HarmonyPatch(nameof(InfectedMixin.Start))]
    [HarmonyPostfix]
    public static void StartPostfix(InfectedMixin __instance)
    {
        if (__instance.player != null)
            return;
        /* if we have a cure
        if (StoryGoalManager.main.IsGoalComplete(StoryUtils.EnzymeRainEnabled.key))
        {
            __instance.SetInfectedAmount(0);
            return;
        }
        */

        var techType = CraftData.GetTechType(__instance.gameObject);
        
        // Check if the creature has opted into immunity by another mod:
        if (ModCompatibilityManager.IsTechTypeImmune(techType))
        {
            __instance.gameObject.EnsureComponent<RedPlagueHost>().mode = RedPlagueHost.Mode.Immune;
            return;
        }

        // Disable infecting creatures in the ACU
        var waterParkCreature = __instance.gameObject.GetComponent<WaterParkCreature>();
        if (waterParkCreature && waterParkCreature.IsInsideWaterPark())
        {
            return;
        }

        // Disable infecting invalid targets
        var infectionTarget = __instance.GetComponent<RedPlagueHost>();
        if (infectionTarget != null && !infectionTarget.CanBeInfected())
        {
            return;
        }

        var shouldBeInfected = EvaluateShouldBeInfectedRandomly(techType);
        if (!shouldBeInfected)
        {
            if (CraftData.GetTechType(__instance.gameObject) == TechType.Warper)
            {
                shouldBeInfected = true;
            }
        }

        if (!shouldBeInfected)
        {
            if (__instance.gameObject.GetComponent<Pickupable>() == null)
            {
                var biomeName = WaterBiomeManager.main.GetBiome(__instance.transform.position);
                if (ZombieManager.IsBiomeHeavilyInfected(biomeName))
                {
                    shouldBeInfected = true;
                }
            }
        }

        if (shouldBeInfected)
            __instance.SetInfectedAmount(4);
    }

    private static bool EvaluateShouldBeInfectedRandomly(TechType techType)
    {
        if (techType == TechType.PrecursorDroid)
        {
            if (StoryUtils.IsAct1Complete())
                return true;
        }
        
        var random = Random.value;
        if (StoryUtils.IsHivemindReleased())
        {
            return random <= 0.20f;
        }

        if (StoryUtils.IsAct1Complete())
        {
            return random <= 0.05f;
        }

        if (StoryGoalManager.main.IsGoalComplete(StoryUtils.BiochemicalProtectionSuitUnlockEvent.key))
        {
            return random <= 0.012f;
        }

        return random <= 0.003f;
    }

    [HarmonyPatch(nameof(InfectedMixin.UpdateInfectionShading))]
    [HarmonyPostfix]
    public static void UpdateInfectionShadingPostfix(InfectedMixin __instance)
    {
        if (__instance.materials == null)
            return;

        var techType = CraftData.GetTechType(__instance.gameObject);
        var hasOverridenInfectionSettings =
            InfectionSettingsDatabase.InfectionSettingsList.TryGetValue(techType, out var infectionSettings);

        if (__instance.infectedAmount >= MinInfectionValue && __instance.gameObject != Player.main.gameObject)
        {
            if (!ZombieManager.IsZombie(__instance.gameObject))
            {
                ZombieManager.AddZombieBehaviour(__instance.gameObject.EnsureComponent<RedPlagueHost>());
            }
        }

        for (int i = 0 ; i < __instance.materials.Count; i++)
        {
            var material = __instance.materials[i];
            
            material.SetTexture(ShaderPropertyID._InfectionAlbedomap, Plugin.ZombieInfectionTexture);
            if (material.HasProperty(InfectionHeightStrength))
                material.SetFloat(InfectionHeightStrength,
                    hasOverridenInfectionSettings ? infectionSettings.InfectionHeight : -0.1f);
            if (hasOverridenInfectionSettings) material.SetVector(InfectionScale, infectionSettings.InfectionScale);
            if (hasOverridenInfectionSettings)
                material.SetVector(InfectionOffset, infectionSettings.InfectionOffset);
            if (hasOverridenInfectionSettings && __instance.infectedAmount >= MinInfectionValue)
                material.SetColor(ShaderPropertyID._Color, infectionSettings.InfectedBodyColor);

            if (__instance.infectedAmount < MinInfectionValue)
                continue;

            switch (techType)
            {
                case TechType.GhostLeviathan or TechType.GhostLeviathanJuvenile:
                    material.SetColor(SpecColor, Color.black);
                    material.SetColor(ShaderPropertyID._GlowColor, Color.black);
                    break;
                case TechType.HoopfishSchool:
                    material.color = new Color(1, 0.3f, 0.44f);
                    material.SetColor(SpecColor, new Color(1, 0.76f, 0.66f));
                    material.SetColor(ShaderPropertyID._GlowColor, Color.red);
                    material.SetFloat(ShaderPropertyID._InfectionAmount, 0);
                    break;
                case TechType.Crash:
                    // A hacky method to determine if the material is the eye of a crash fish
                    var isCrashFishEye = i % 2 == 1;
                    if (isCrashFishEye)
                    {
                        material.SetColor(ShaderPropertyID._Color, Color.black);
                        material.SetColor(SpecColor, Color.black);
                        material.SetColor(ShaderPropertyID._GlowColor, Color.black);
                    }
                    else
                    {
                        material.SetColor(ShaderPropertyID._Color, new Color(0, 1, 0.9f));
                        material.SetColor(SpecColor, new Color(0, 1, 0.38f));
                        material.SetColor(ShaderPropertyID._GlowColor, new Color(0, 1, 1));
                    }
                    break;
                case TechType.Mesmer:
                    material.SetColor(ShaderPropertyID._Color, Color.black);
                    material.SetColor(SpecColor, new Color(1, 0.571429f, 1));
                    material.SetColor(ShaderPropertyID._GlowColor, new Color(3, 0, 0));
                    break;
                case TechType.SpineEel when material.name.Contains("eye"):
                    material.SetColor(ShaderPropertyID._Color, new Color(5, 5, 5));
                    material.SetColor(SpecColor, new Color(0, 5, 5));
                    break;
                case TechType.Warper when (material.name.Contains("entrails") || material.name.Contains("alpha")):
                    material.SetColor(ShaderPropertyID._Color, Color.red);
                    material.SetColor(ShaderPropertyID._SpecColor, Color.red);
                    material.SetColor(ShaderPropertyID._GlowColor, Color.red);
                    break;
                case TechType.PrecursorDroid:
                    material.SetTexture(ShaderPropertyID._Illum, PrecursorDroidRedIllumTexture);
                    material.SetFloat(ShaderPropertyID._InfectionAmount, 0);
                    break;
            }
        }
        
        if (techType == TechType.SpineEel && __instance.infectedAmount >= MinInfectionValue)
            __instance.transform.Find("model/spine_eel_geo/Spine_eel_geo/Spine_eel_eye_L").gameObject.SetActive(false);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(InfectedMixin.SetInfectedAmount))]
    public static void SetInfectedAmountPostfix(InfectedMixin __instance, ref bool __result)
    {
        if (__instance.infectedAmount > 0 && __result)
        {
            ZombieManager.TryConversion(__instance.gameObject);
        }
    }
}