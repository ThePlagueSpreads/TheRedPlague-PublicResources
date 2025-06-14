using System;
using System.Collections;
using System.Data.Common;
using Nautilus.Assets;
using Nautilus.Utility;
using TheRedPlague.Mono.StoryContent;
using TheRedPlague.Mono.StoryContent.Precursor;
using TheRedPlague.Utilities;
using UnityEngine;
using UWE;
using Object = UnityEngine.Object;

namespace TheRedPlague.PrefabFiles.Precursor;

public class CustomForceFieldPrefab
{
    public PrefabInfo Info { get; }

    private readonly Func<GameObject> _modelPrefab;
    private readonly string _storyGoalKey;

    // The model prefab object is expected to be a basic prefab set up with a 1x1x1 parent,
    // a model child, and a collider child
    public CustomForceFieldPrefab(string classId, Func<GameObject> modelPrefab, string storyGoalKey)
    {
        Info = PrefabInfo.WithTechType(classId);
        _modelPrefab = modelPrefab;
        _storyGoalKey = storyGoalKey;
    }

    public CustomForceFieldPrefab(string classId, GameObject modelPrefab, string storyGoalKey) : this(classId,
        () => modelPrefab, storyGoalKey)
    {
    }

    public void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetPrefab);
        prefab.Register();
    }

    private IEnumerator GetPrefab(IOut<GameObject> prefab)
    {
        var referenceForceField = PrefabDatabase.GetPrefabAsync("18f2fbaa-78df-46a9-805a-79ac4d942125");
        yield return referenceForceField;
        if (!referenceForceField.TryGetPrefab(out var referencePrefab))
        {
            Plugin.Logger.LogWarning("Failed to find reference force field prefab! Using default model instead.");
            prefab.Set(TrpPrefabUtils.CreateLootCubePrefab(Info));
            yield break;
        }

        var obj = Object.Instantiate(_modelPrefab());
        obj.SetActive(false);

        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Medium);

        obj.AddComponent<ConstructionObstacle>();

        var forceFieldMaterial = new Material(MaterialUtils.ForceFieldMaterial);

        var renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.material = forceFieldMaterial;
        }

        var lerpColor = renderers[0].gameObject.AddComponent<VFXLerpColor>();
        lerpColor.PlayOnAwake = false;
        lerpColor.destroyMaterial = true;
        lerpColor.looping = false;
        lerpColor.reverse = false;
        lerpColor.duration = 5;
        lerpColor.randomAmount = 0;
        var referenceLerpColor = referencePrefab.GetComponentInChildren<VFXLerpColor>();
        lerpColor.blendCurve = referenceLerpColor.blendCurve;
        lerpColor.colorEnd = referenceLerpColor.colorEnd;

        var soundEmitter = obj.AddComponent<FMOD_CustomLoopingEmitter>();
        soundEmitter.restartOnPlay = false;
        soundEmitter.followParent = true;
        soundEmitter.SetAsset(AudioUtils.GetFmodAsset("event:/env/prec_forcefield_loop",
            "{b6524cc4-c7db-4700-aab3-610a8fa2fc04}"));
        soundEmitter.assetStop = AudioUtils.GetFmodAsset("event:/env/prec_foce_field_deactivate",
            "{00bc6b7c-3a07-47ad-bde2-c88a43ee5c4a}");

        var colliderObject = obj.GetComponentInChildren<Collider>().gameObject;

        var door = obj.AddComponent<GenericForceFieldDoor>();
        door.storyGoalKey = _storyGoalKey;
        door.renderers = renderers;
        door.colorControl = lerpColor;
        door.colliderObject = colliderObject;
        door.soundEmitter = soundEmitter;

        prefab.Set(obj);
    }
}