using UnityEngine;

namespace TheRedPlague.Mono.Util;

public class DestroyPropsInRange : MonoBehaviour
{
    private const float UpdateInterval = 5f;
    
    public TechType[] techTypesToDestroy;
    public string[] classIdsToDestroy;
    public float radius;

    private float _radiusSqr;

    private void Start()
    {
        _radiusSqr = radius * radius;
        InvokeRepeating(nameof(LazyUpdate), Random.value, UpdateInterval);
    }

    private void LazyUpdate()
    {
        if (!isActiveAndEnabled) return;

        if (LargeWorld.main.streamer.cellManager.IsProcessing()) return;
        
        if (!LargeWorld.main.streamer.IsReady()) return;

        var allEntities = UniqueIdentifier.identifiers;
        
        foreach (var entity in allEntities.Values)
        {
            if (entity != null)
            {
                ProcessEntity(entity);
            }
        }
    }

    private void ProcessEntity(UniqueIdentifier identifier)
    {
        if (!identifier.gameObject.activeInHierarchy)
            return;
        if ((transform.position - identifier.transform.position).sqrMagnitude > _radiusSqr)
            return;
        foreach (var classId in classIdsToDestroy)
        {
            if (identifier.ClassId == classId)
            {
                SafeDestroy(identifier.gameObject);
                return;
            }
        }

        var techType = CraftData.GetTechType(identifier.gameObject);
        if (techType == TechType.None)
            return;
        
        foreach (var validType in techTypesToDestroy)
        {
            if (techType == validType)
            {
                SafeDestroy(identifier.gameObject);
                return;
            }
        }
    }

    private void SafeDestroy(GameObject go)
    {
        if (go.GetComponent<LiveMixin>())
            return;
        if (go.transform.parent != null && go.transform.parent.GetComponent<ChildObjectIdentifier>())
            return;
        var rb = go.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
            return;
        if (go.GetComponentInParent<Player>())
            return;
        Plugin.Logger.LogMessage("Safely destroying " + go);
        go.AddComponent<DestroyAfterDelay>();
    }
}