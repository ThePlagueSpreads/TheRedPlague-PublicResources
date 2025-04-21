using Nautilus.Utility;
using UnityEngine;

namespace TheRedPlague.Mono.UpgradeModules;

public class AntiPossessionModuleBehaviour : MonoBehaviour
{
    private static readonly FMODAsset RemoveSound = AudioUtils.GetFmodAsset("event:/env/damage/shock");
    
    private void Start()
    {
        InvokeRepeating(nameof(ScanForThreats), Random.value, 1f);
    }

    private void ScanForThreats()
    {
        var creatures = gameObject.GetComponentsInChildren<Creature>();
        foreach (var creature in creatures)
        {
            if (creature.IsFriendlyTo(Player.main.gameObject))
            {
                continue;
            }
            creature.liveMixin.TakeDamage(30, transform.position, DamageType.Electrical, gameObject);
            Utils.PlayFMODAsset(RemoveSound, creature.transform.position);
        }
    }
}