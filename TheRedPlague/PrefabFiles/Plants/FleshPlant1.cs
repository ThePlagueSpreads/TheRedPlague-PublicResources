using System.Collections;
using Nautilus.Assets;
using Nautilus.Handlers;
using Nautilus.Utility;
using TheRedPlague.Data;
using TheRedPlague.Mono.Util;
using TheRedPlague.PrefabFiles.Items;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Plants;

public static class FleshPlant1
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("FleshPlant1");

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(CreatePrefab);
        prefab.Register();
        
        PDAHandler.AddEncyclopediaEntry(Info.ClassID, CustomPdaPaths.PlagueCreationsPath, null, null,
            null, null, PDAHandler.UnlockBasic);
        PDAHandler.AddCustomScannerEntry(Info.TechType, 4, false, Info.ClassID);
    }

    private static IEnumerator CreatePrefab(IOut<GameObject> prefab)
    {
        var obj = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("FleshPlant1"));
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Far);
        MaterialUtils.ApplySNShaders(obj, 6, 2f, 2f, new MaterialModifier());
        obj.AddComponent<ConstructionObstacle>();
        foreach (var collider in obj.GetComponentsInChildren<Collider>())
        {
            collider.gameObject.EnsureComponent<VFXSurface>().surfaceType = VFXSurfaceTypes.vegetation;
        }
        
        var childObjectIdentifier = obj.transform.Find("Pivot/ItemSpawnPoint").gameObject
            .AddComponent<ChildObjectIdentifier>();
        childObjectIdentifier.classId = "FleshPlant1RemainsContainer";

        var mysteriousRemainsSpawns = obj.AddComponent<ChildObjectSpawner>();
        mysteriousRemainsSpawns.identifier = obj.GetComponent<UniqueIdentifier>();
        mysteriousRemainsSpawns.childObjectIdentifier = childObjectIdentifier;
        mysteriousRemainsSpawns.respawnDuration = 60 * 15;
        mysteriousRemainsSpawns.spawnClassId = MysteriousRemains.Info.ClassID;
        mysteriousRemainsSpawns.minimumRespawnDistance = 15;
        mysteriousRemainsSpawns.spawnScale = 1.6f;
        prefab.Set(obj);
        yield break;
    }

    private class MaterialModifier : Nautilus.Utility.MaterialModifiers.MaterialModifier
    {
        public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
        {
            if (materialType == MaterialUtils.MaterialType.Transparent)
            {
                material.SetFloat("_SpecInt", 15);
                material.SetFloat(ShaderPropertyID._MyCullVariable, 0);
            }
            else if (renderer.gameObject.name.ToLower().Contains("stem"))
            {
                material.EnableKeyword("UWE_WAVING");
                material.SetVector("_Scale", new Vector4(0.12f, 0.05f, 0.238f, 0.01f));
                material.SetVector("_Frequency", new Vector4(0.6f, 0.5f, 0.75f, 0.8f));
                material.SetVector("_Speed", new Vector4(0.2f, 0.3f, 0, 0));
                material.SetFloat("_WaveUpMin", 1f);
                material.SetFloat("_Fallof", 1f);
            }
        }
    }
}