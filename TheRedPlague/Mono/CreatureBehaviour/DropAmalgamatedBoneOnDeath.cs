using System.Collections;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour;

public class DropAmalgamatedBoneOnDeath : MonoBehaviour
{
    public void OnKill()
    {
        UWE.CoroutineHost.StartCoroutine(SpawnDropsCoroutine(transform.position, GetNumberOfBonesToDrop()));
    }

    private int GetNumberOfBonesToDrop()
    {
        var lm = gameObject.GetComponent<LiveMixin>();
        if (lm == null) return 1;
        return lm.maxHealth switch
        {
            < 1500 => 1,
            < 2500 => 2,
            < 4000 => 3,
            _ => 5
        };
    }

    private IEnumerator SpawnDropsCoroutine(Vector3 position, int number)
    {
        var task = CraftData.GetPrefabForTechTypeAsync(ModPrefabs.AmalgamatedBone.TechType);
        yield return task;
        for (int i = 0; i < number; i++)
        {
            var obj = Instantiate(task.GetResult(), position + Random.insideUnitSphere * Mathf.Min(number, 3), Quaternion.identity);
            obj.SetActive(true);
            obj.GetComponent<Rigidbody>().isKinematic = false;
            LargeWorld.main.streamer.cellManager.RegisterEntity(obj);
        }
    }
}