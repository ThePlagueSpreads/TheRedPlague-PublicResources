using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using Nautilus.Utility;
using TheRedPlague.PrefabFiles.Buildable;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Fragments;

public static class PlagueNeutralizerFragment
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("PlagueNeutralizerFragment");

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(new CloneTemplate(Info, "c9bdcc4d-a8c6-43c0-8f7a-f86841cd4493")
        {
            ModifyPrefab = obj =>
            {
                obj.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
                obj.transform.Find("model/Species_Analyzer/Species_Analyzer/Species_Analyzer_glass1").gameObject.SetActive(false);
                Object.DestroyImmediate(obj.GetComponent<SpecimenAnalyzer>());
                Object.DestroyImmediate(obj.GetComponent<Constructable>());
                Object.DestroyImmediate(obj.GetComponent<LiveMixin>());
                Object.DestroyImmediate(obj.GetComponentInChildren<StorageContainer>());
                PrefabUtils.AddResourceTracker(obj, TechType.Fragment);
            }
        });
        prefab.Register();
        WorldEntityDatabaseHandler.AddCustomInfo(Info.ClassID, Info.TechType, Vector3.one, false,
            LargeWorldEntity.CellLevel.Near, EntitySlot.Type.Medium);
        PDAHandler.AddCustomScannerEntry(Info.TechType, PlagueNeutralizer.Info.TechType,
            true, 3, 3, false);
    }
}