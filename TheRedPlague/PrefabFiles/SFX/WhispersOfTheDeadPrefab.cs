using Nautilus.Assets;
using Nautilus.Utility;
using TheRedPlague.Mono.SFX;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.SFX;

public class WhispersOfTheDeadPrefab
{
    public PrefabInfo Info { get; private set; }
    
    private FMODAsset SoundAsset { get; }
    private float MinDelay { get; }
    private float MaxDelay { get; }
    private float MaxDistance { get; }

    public WhispersOfTheDeadPrefab(PrefabInfo info, FMODAsset soundAsset, float minDelay, float maxDelay, float maxDistance)
    {
        Info = info;
        SoundAsset = soundAsset;
        MinDelay = minDelay;
        MaxDelay = maxDelay;
        MaxDistance = maxDistance;
    }

    public void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetPrefab);
        prefab.Register();
    }

    private GameObject GetPrefab()
    {
        var obj = new GameObject(Info.ClassID);
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Near);
        var behaviour = obj.AddComponent<WhispersOfTheDead>();
        behaviour.minDelay = MinDelay;
        behaviour.maxDelay = MaxDelay;
        behaviour.maxDistance = MaxDistance;
        var emitter = obj.AddComponent<FMOD_CustomEmitter>();
        emitter.SetAsset(SoundAsset);
        emitter.playOnAwake = false;
        emitter.SetAsset(SoundAsset);
        behaviour.emitter = emitter;
        return obj;
    }
}