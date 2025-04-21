using Nautilus.Handlers;
using TheRedPlague.PrefabFiles.Decorations;
using TheRedPlague.PrefabFiles.Equipment;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Fragments;

public class PlagueArmorFragment : FloatingCorpsePrefab
{
    public PlagueArmorFragment() : base("PlagueArmorFragment", "PlagueArmorFragment")
    {
        PDAHandler.AddCustomScannerEntry(Info.TechType, BoneArmor.Info.TechType,
            true, 3, 2.5f, false);
    }

    protected override void ModifyPrefab(GameObject prefab)
    {
        var material = BoneArmor.GetMaterial();

        var renderers = prefab.transform.Find("BoneArmor_Prefab").GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.material = material;
        }

        var light = prefab.AddComponent<Light>();
        light.intensity = 2;
        light.range = 5;
        light.color = Color.red;
    }
}