﻿using HarmonyLib;
using TheRedPlague.Managers;
using TheRedPlague.Mono.CreatureBehaviour;
using TheRedPlague.Mono.FleshBlobs;
using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;

namespace TheRedPlague.Patches.Infection;

[HarmonyPatch(typeof(LiveMixin))]
public class LiveMixinPatcher
{
    [HarmonyPatch(nameof(LiveMixin.Awake))]
    [HarmonyPostfix]
    public static void AwakePostfix(LiveMixin __instance)
    {
        var rb = __instance.GetComponent<Rigidbody>();
        if (rb == null || rb.isKinematic) return;
        var creature = __instance.GetComponent<Creature>();
        var vehicle = __instance.GetComponent<Vehicle>();
        var subRoot = __instance.GetComponent<SubRoot>();
        if (creature == null && vehicle == null && subRoot == null &&
            (Player.main == null || __instance.gameObject != Player.main.gameObject)) return;
        __instance.gameObject.AddComponent<FleshBlobInfluenced>();
    }
    
    [HarmonyPatch(nameof(LiveMixin.TakeDamage))]
    [HarmonyPrefix]
    public static bool TakeDamagePrefix(LiveMixin __instance, float originalDamage, GameObject dealer, DamageType type)
    {
        if (originalDamage <= 0 || dealer == null)
            return true;
        if (__instance == Player.main.liveMixin)
            return true;
        if (__instance.GetComponent<Creature>() == null)
            return true;
        var damagedByZombie = ZombieManager.IsZombie(dealer);
        if (!damagedByZombie || dealer.GetComponent<FriendlyWarper>() != null)
            return true;
        // filter out invalid infection targets
        var infectionTarget = __instance.gameObject.GetComponent<RedPlagueHost>();
        if (infectionTarget != null && !infectionTarget.CanBeInfected())
            return true;
        if (ZombieManager.IsZombie(__instance.gameObject)) // am I already a zombie?
            return false;
        var calculatedDamage = DamageSystem.CalculateDamage(originalDamage, type, __instance.gameObject, dealer);
        // If the creature does not die from the attack and the random roll doesn't hit, take normal damage
        if (calculatedDamage < __instance.health && Random.value > ZombieManager.GetBiteConversionChance(__instance))
        {
            return true;
        }
        ZombieManager.Zombify(__instance.gameObject);
        return false;
    }
}