using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using TheRedPlague.Mono.VFX;
using UnityEngine;
using UWE;

namespace TheRedPlague.PrefabFiles.Plants;

public static class InfectedBrainCoral
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("InfectedBrainCoral");

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(new CloneTemplate(Info, "171c6a5b-879b-4785-be7a-6584b2c8c442")
        {
            ModifyPrefab = obj =>
            {
                var infect = obj.AddComponent<InfectAnything>();
                infect.infectionHeightStrength = 0;
            }
        });
        prefab.SetSpawns(new WorldEntityInfo
        {
            localScale = Vector3.one * 1.8f,
            cellLevel = LargeWorldEntity.CellLevel.Medium,
            slotType = EntitySlot.Type.Small,
            prefabZUp = true,
            techType = Info.TechType,
            classId = Info.ClassID
        });
        prefab.Register();
    }
}