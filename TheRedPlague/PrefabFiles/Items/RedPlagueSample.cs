using Nautilus.Assets;
using Nautilus.Handlers;
using Nautilus.Utility;
using TheRedPlague.Data;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Items;

public static class RedPlagueSample
{
    private const string SuperEnormouslyLongNameForTheNameForOctoʼsThingThatOctoNamedBecauseHeExportedTheImageMultipleTimesAndNeededDifferentNamesForEachOneSoHeWouldntGetConfusedPlagueSampleSpriteImageAssetNameDotComSlashLogin = "PlagueSampleIconTransparent2RealLastRenderForRealThisTime_Final_2_Last_Finished";
    
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("RedPlagueSample")
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>(SuperEnormouslyLongNameForTheNameForOctoʼsThingThatOctoNamedBecauseHeExportedTheImageMultipleTimesAndNeededDifferentNamesForEachOneSoHeWouldntGetConfusedPlagueSampleSpriteImageAssetNameDotComSlashLogin))
        .WithSizeInInventory(new Vector2int(2, 2));

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetGameObject);
        prefab.Register();
        
        CraftDataHandler.SetBackgroundType(Info.TechType, CustomBackgroundTypes.PlagueItem);
        
        BaseBioReactor.charge[Info.TechType] = 300;
    }

    private static GameObject GetGameObject()
    {
        var obj = Object.Instantiate(
            Plugin.AssetBundle.LoadAsset<GameObject>("RedPlagueSamplePrefab"));
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Near);
        MaterialUtils.ApplySNShaders(obj, 6f);
        obj.AddComponent<Pickupable>();
        var rb = obj.AddComponent<Rigidbody>();
        rb.mass = 2;
        rb.useGravity = false;
        rb.isKinematic = true;
        var wf = obj.AddComponent<WorldForces>();
        wf.useRigidbody = rb;
        return obj;
    }
}