using HarmonyLib;
using TheRedPlague.Data;
using TheRedPlague.Mono.CreatureBehaviour.HoverPet;
using TheRedPlague.Mono.Equipment;
using TheRedPlague.Mono.InfectionLogic;
using TheRedPlague.Mono.Insanity;
using TheRedPlague.Mono.Systems;
using TheRedPlague.Mono.VFX;
using TheRedPlague.PrefabFiles.Equipment;
using UnityEngine;

namespace TheRedPlague.Patches.Essential;

[HarmonyPatch(typeof(Player))]
public static class PlayerPatcher
{
    public const float InfectionDamageFromCreaturesPercent = 0.25f;
    public const float MaxInfectionDamageFromCreatures = 17f;
    
    [HarmonyPatch(nameof(Player.Start))]
    [HarmonyPostfix]
    public static void StartPostfix(Player __instance)
    {
        __instance.gameObject.EnsureComponent<PlayerInfectedBiomeDamage>();
        
        __instance.gameObject.EnsureComponent<PlayerInfectionDamageVisualization>();

        try
        {
            MainCamera.camera.gameObject.AddComponent<PlagueScreenFXController>();
        }
        catch (System.Exception exception)
        {
            Plugin.Logger.LogError(exception);
        }
        
        __instance.gameObject.AddComponent<PlagueDamageStat>();

        var trpManagersRoot = new GameObject("TRPManagers").transform;
        
        var insanityManager = new GameObject("InsanityManager");
        insanityManager.AddComponent<InsanityManager>();
        insanityManager.transform.SetParent(trpManagersRoot);

        var hoverPetSpawner = new GameObject("HoverPetSpawner");
        hoverPetSpawner.AddComponent<HoverPetSpawner>();
        hoverPetSpawner.transform.SetParent(trpManagersRoot);

        var eventMusicPlayer = new GameObject("EventMusicPlayer");
        eventMusicPlayer.AddComponent<TrpEventMusicPlayer>();
        eventMusicPlayer.transform.SetParent(trpManagersRoot);
    }
    
    [HarmonyPatch(nameof(Player.EquipmentChanged))]
    [HarmonyPostfix]
    public static void EquipmentChangedPostfix(Player __instance)
    {
        var equipment = Inventory.main.equipment;
        __instance.gameObject.EnsureComponent<PlagueArmorBehavior>()
            .SetArmorActive(equipment.GetTechTypeInSlot("Body") == BoneArmor.Info.TechType);
    }

    [HarmonyPatch(nameof(Player.OnTakeDamage))]
    [HarmonyPostfix]
    public static void OnTakeDamagePostfix(Player __instance, DamageInfo damageInfo)
    {
        // Give infection damage from creature attacks
        if (damageInfo.type == DamageType.Normal && damageInfo.dealer != null)
        {
            if (RedPlagueHost.IsGameObjectInfected(damageInfo.dealer))
            {
                PlagueDamageStat.main.TakeInfectionDamage(
                    Mathf.Min(damageInfo.damage * InfectionDamageFromCreaturesPercent, MaxInfectionDamageFromCreatures));
            }
        }

        // Give infection damage from penetrative plague attacks
        if (damageInfo.type == CustomDamageTypes.PenetrativePlagueDamage)
        {
            PlagueDamageStat.main.TakeInfectionDamage(damageInfo.damage);
        }
    }
}