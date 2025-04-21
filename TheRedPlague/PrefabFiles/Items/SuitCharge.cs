using System.Collections;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using TheRedPlague.Mono.Equipment;
using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Items;

public static class SuitCharge
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("SuitCharge")
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("SuitChargeIcon"))
        .WithSizeInInventory(new Vector2int(1, 2));

    private static readonly FMODAsset UseSound = AudioUtils.GetFmodAsset("event:/tools/divereel/set_anchor");
    private const float UseSoundDuration = 2f;
    
    private static float _timeLastSoundPlayed;
    
    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetGameObject);
        prefab.SetPdaGroupCategoryAfter(TechGroup.Personal, TechCategory.Equipment, TechType.FirstAidKit);
        prefab.SetRecipe(new RecipeData(
                new CraftData.Ingredient(TechType.Battery), new CraftData.Ingredient(TechType.Lubricant)))
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab(CraftTreeHandler.Paths.FabricatorEquipment);
        prefab.Register();
        KnownTechHandler.SetAnalysisTechEntry(Info.TechType, System.Array.Empty<TechType>(),
            KnownTechHandler.DefaultUnlockData.BasicUnlockSound,
            Plugin.AssetBundle.LoadAsset<Sprite>("SuitChargePopup"));
    }

    private static GameObject GetGameObject()
    {
        var obj = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("SuitChargePrefab"));
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Near);
        MaterialUtils.ApplySNShaders(obj, 8f);
        PrefabUtils.AddWorldForces(obj, 25f, 1.4f, isKinematic: true);
        PrefabUtils.AddVFXFabricating(obj, "Suit Charge", 0f, 0.6f,
            new Vector3(-0.07f, -0.021f, 0f), 60);
        obj.AddComponent<UsableItem>().SetOnUseAction(Info.ClassID, OnUse);
        obj.AddComponent<Pickupable>();

        return obj;
    }
    
    private static void OnUse()
    {
        PlagueDamageStat.main.Charge(45);
        if (Time.time > _timeLastSoundPlayed + UseSoundDuration)
            UWE.CoroutineHost.StartCoroutine(PlayUseSound());
    }
    
    private static IEnumerator PlayUseSound()
    {
        _timeLastSoundPlayed = Time.time;
        var obj = new GameObject("SuitChargeUseSoundEmitter");
        var emitter = obj.AddComponent<FMOD_CustomEmitter>();
        emitter.SetAsset(UseSound);
        emitter.transform.position = Player.main.transform.position;
        emitter.Play();
        yield return new WaitForSeconds(UseSoundDuration);
        emitter.Stop();
        Object.Destroy(obj, 1);
    }
}