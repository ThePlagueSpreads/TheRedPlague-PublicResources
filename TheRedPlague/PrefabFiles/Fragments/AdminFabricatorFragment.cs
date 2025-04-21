using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using Nautilus.Utility;
using TheRedPlague.PrefabFiles.Buildable;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Fragments;

public class AdminFabricatorFragment
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("AdminFabricatorFragment");

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(new CloneTemplate(Info, TechType.Fabricator)
        {
            ModifyPrefab = ModifyPrefab
        });
        prefab.Register();
        PDAHandler.AddCustomScannerEntry(Info.TechType, AdminFabricator.Info.TechType, true, 1,
            5f, false);
    }

    private static void ModifyPrefab(GameObject prefab)
    {
        Object.DestroyImmediate(prefab.GetComponent<Constructable>());
        Object.DestroyImmediate(prefab.GetComponent<ConstructableBounds>());
        Object.DestroyImmediate(prefab.GetComponent<PreventDeconstruction>());
        Object.DestroyImmediate(prefab.GetComponent<Fabricator>());
        prefab.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
        AdminFabricator.ApplyAdminFabricatorMaterials(prefab);
        PrefabUtils.AddResourceTracker(prefab, TechType.Fragment);
    }
}