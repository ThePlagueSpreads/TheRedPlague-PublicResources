using Story;
using TheRedPlague.Data;
using TheRedPlague.Managers;
using TheRedPlague.Mono.Equipment;
using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;

namespace TheRedPlague.Mono.Systems;

public class PlayerInfectedBiomeDamage : MonoBehaviour
{
    private float _timeTryDamageAgain;

    private static readonly SafePocket[] SafePockets =
    {
        new(new Vector3(-1253, -228, 711), 50)
    };

    private void Update()
    {
        if (!(Time.time > _timeTryDamageAgain)) return;
        _timeTryDamageAgain = Time.time + 3f;
        if (PlagueArmorBehavior.IsPlagueArmorEquipped() ||
            !ZombieManager.IsBiomeHeavilyInfected(WaterBiomeManager.main.GetBiome(Player.main.transform.position)))
        {
            return;
        }

        foreach (var safePocket in SafePockets)
        {
            if (Vector3.SqrMagnitude(Player.main.transform.position - safePocket.Center) <
                safePocket.Radius * safePocket.Radius)
            {
                return;
            }
        }

        if (ShouldAddInsanityInsteadOfDamage())
        {
            PlagueDamageStat.main.TakeInfectionDamage(1.25f);
        }
        else
        {
            Player.main.liveMixin.TakeDamage(3, MainCamera.camera.transform.position + Random.onUnitSphere,
                CustomDamageTypes.PenetrativePlagueDamage);
        }
    }

    private bool ShouldAddInsanityInsteadOfDamage()
    {
        if (Player.main.GetCurrentSub() != null)
            return true;
        return StoryGoalManager.main &&
               StoryGoalManager.main.IsGoalComplete(StoryUtils.UseBiochemicalProtectionSuitEvent.key);
    }

    private struct SafePocket
    {
        public readonly Vector3 Center;
        public readonly float Radius;

        public SafePocket(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }
    }
}