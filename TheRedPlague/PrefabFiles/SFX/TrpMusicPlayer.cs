using Nautilus.Assets;
using Nautilus.Utility;
using TheRedPlague.Mono.SFX;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.SFX;

public class TrpMusicPlayer
{
    private PrefabInfo Info { get; }
    private LargeWorldEntity.CellLevel CellLevel { get; }
    private string EventName { get; }
    private float StartDistance { get; }
    private float EndDistance { get; }

    public TrpMusicPlayer(string classId, LargeWorldEntity.CellLevel cellLevel, string eventName, float startDistance, float endDistance)
    {
        Info = PrefabInfo.WithTechType(classId);
        CellLevel = cellLevel;
        EventName = eventName;
        StartDistance = startDistance;
        EndDistance = endDistance;
        if (StartDistance > EndDistance)
        {
            Plugin.Logger.LogWarning($"TrpMusicPlayer: StartDistance greater than EndDistance for {classId}");
        }
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
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, CellLevel);
        var emitter = obj.AddComponent<FMOD_CustomLoopingEmitter>();
        emitter.SetAsset(AudioUtils.GetFmodAsset(EventName));
        emitter.followParent = false;
        emitter.restartOnPlay = false;
        emitter.playOnAwake = false;
        var player = obj.AddComponent<GenericMusicPlayer>();
        player.startRange = StartDistance;
        player.endRange = EndDistance;
        player.emitter = emitter;
        return obj;
    }
}