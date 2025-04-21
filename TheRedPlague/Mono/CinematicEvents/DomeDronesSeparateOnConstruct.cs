using System.Collections;
using UnityEngine;

namespace TheRedPlague.Mono.CinematicEvents;

public class DomeDronesSeparateOnConstruct : MonoBehaviour
{
    public VFXConstructing vfxConstructing;
    public TechType spawnTechType;
    
    private GameObject _dronePrefab;
    
    private IEnumerator Start()
    {
        var droneTask = CraftData.GetPrefabForTechTypeAsync(spawnTechType);
        yield return droneTask;
        _dronePrefab = droneTask.GetResult();
        yield return null;
        yield return new WaitUntil(() => vfxConstructing.IsConstructed());
        SeparateDrones();
    }

    public void SeparateDrones()
    {
        foreach (Transform child in transform)
        {
            Instantiate(_dronePrefab, child.transform.position, child.transform.rotation);
            Destroy(child.gameObject);
        }
        Destroy(gameObject);
    }
}