using System.Collections;
using System.Collections.Generic;
using Nautilus.Handlers;
using Nautilus.Json;
using Nautilus.Json.Attributes;
using Newtonsoft.Json;
using UnityEngine;
using UWE;

namespace TheRedPlague.Mono.Util;

public class ChildObjectSpawner : MonoBehaviour, IScheduledUpdateBehaviour
{
    public UniqueIdentifier identifier;
    public ChildObjectIdentifier childObjectIdentifier;
    public float respawnDuration;
    public string spawnClassId;
    public float minimumRespawnDistance;
    public float spawnScale = 1f;
    public bool useParentScale = true;

    private static readonly SaveData _saveData = SaveDataHandler.RegisterSaveDataCache<SaveData>();

    public int scheduledUpdateIndex { get; set; }

    private bool _busy;

    private bool _itemExisted;

    private GameObject GetCurrentChildObject()
    {
        return childObjectIdentifier.transform.childCount == 0
            ? null
            : childObjectIdentifier.transform.GetChild(0).gameObject;
    }

    private bool ContainsItem()
    {
        return childObjectIdentifier.transform.childCount != 0;
    }

    private bool IsInRespawningRange()
    {
        if (minimumRespawnDistance < 1) return true;
        return Vector3.SqrMagnitude(Player.main.transform.position - transform.position) >=
               minimumRespawnDistance * minimumRespawnDistance;
    }

    private IEnumerator SpawnAsync()
    {
        _busy = true;
        var task = PrefabDatabase.GetPrefabAsync(spawnClassId);
        yield return task;
        _busy = false;
        if (!task.TryGetPrefab(out var prefab))
        {
            Plugin.Logger.LogWarning($"Failed to load prefab by Class ID {spawnClassId}!");
            yield break;
        }

        var obj = UWE.Utils.InstantiateDeactivated(prefab);
        obj.transform.parent = childObjectIdentifier.transform;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale *= spawnScale;
        if (useParentScale)
        {
            var parentScale = transform.lossyScale;
            obj.transform.localScale *= (parentScale.x + parentScale.y + parentScale.z) / 3;
        }
        obj.SetActive(true);
        _itemExisted = true;
    }
    
    public string GetProfileTag()
    {
        return "TheRedPlague:ChildObjectSpawner";
    }

    private void OnEnable()
    {
        UpdateSchedulerUtils.Register(this);
    }

    private void OnDisable()
    {
        UpdateSchedulerUtils.Deregister(this);
    }

    private void Start()
    {
        _itemExisted = ContainsItem();
    }

    public void ScheduledUpdate()
    {
        if (_busy) return;

        if (ContainsItem())
        {
            _itemExisted = true;
            return;
        }

        // If the item existed earlier but was removed, add a respawn timer (and save it)
        if (_itemExisted)
        {
            _saveData.SaveRespawnTime(identifier.id, DayNightCycle.main.timePassed + respawnDuration);
            _itemExisted = false;
            return;
        }

        var hasRespawnTimer = _saveData.TryGetRespawnTime(identifier.id, out var respawnTime);
        if (!hasRespawnTimer || DayNightCycle.main.timePassed > respawnTime)
        {
            // Don't consider spawning range unless respawning
            if (!hasRespawnTimer || IsInRespawningRange())
                StartCoroutine(SpawnAsync());
        }
    }

    [FileName("ChildObjectSpawners")]
    public class SaveData : SaveDataCache
    {
        // Key: Owner unique id, Value: respawn time (DayNightCycle.main.timePassed)
        [JsonProperty] private Dictionary<string, double> _respawnTimes;

        public bool TryGetRespawnTime(string ownerId, out double respawnTime)
        {
            if (_respawnTimes != null)
                return _respawnTimes.TryGetValue(ownerId, out respawnTime);

            respawnTime = 0;
            return false;
        }

        public void SaveRespawnTime(string ownerId, double respawnTime)
        {
            _respawnTimes ??= new Dictionary<string, double>();
            _respawnTimes[ownerId] = respawnTime;
        }
    }
}