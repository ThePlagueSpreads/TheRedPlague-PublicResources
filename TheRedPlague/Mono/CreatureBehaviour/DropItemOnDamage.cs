using System.Collections;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour;

public class DropItemOnDamage : MonoBehaviour, IOnTakeDamage
{
    public TechType techType;
    [Range(0f, 1f)]
    public float percentage;

    public float spawnRadius;
    
    public void OnTakeDamage(DamageInfo damageInfo)
    {
        if (Random.value < percentage)
        {
            StartCoroutine(DropItem());
        }
    }

    private IEnumerator DropItem()
    {
        var task = CraftData.GetPrefabForTechTypeAsync(techType);
        yield return task;
        var prefab = task.GetResult();
        if (prefab == null)
        {
            Plugin.Logger.LogWarning("Failed to find prefab for TechType " + techType);
            yield break;
        }

        var position = transform.position;
        if (spawnRadius > Mathf.Epsilon)
        {
            position += Random.onUnitSphere * spawnRadius;
        }
        Instantiate(prefab, position, Random.rotation);
    }
}