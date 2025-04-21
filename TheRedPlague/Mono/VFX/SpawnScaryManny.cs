using Nautilus.Utility;
using UnityEngine;

namespace TheRedPlague.Mono.VFX;

public class SpawnScaryManny : MonoBehaviour
{
    private float _timeCanSpawnAgain;
    
    private void OnTriggerEnter(Collider other)
    {
        if (Time.time < _timeCanSpawnAgain) return;
        if (other.gameObject != Player.main.gameObject) return;
        if (!ScaryMannySpawns.TryGetClosestSpawnPoint(transform.position, out var location, out var rotation)) return;
        var shark = Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("MutantProp"));
        shark.transform.position = location;
        shark.transform.rotation = rotation;
        Destroy(shark, 2);
        shark.AddComponent<SkyApplier>().renderers = shark.GetComponentsInChildren<Renderer>();
        MaterialUtils.ApplySNShaders(shark);
        _timeCanSpawnAgain = Time.time + 100;
    }
}