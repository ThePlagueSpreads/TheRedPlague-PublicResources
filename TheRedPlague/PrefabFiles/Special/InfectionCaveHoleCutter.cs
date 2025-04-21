using Nautilus.Assets;
using Nautilus.Utility;
using TheRedPlague.Mono.Util;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Special;

public static class InfectionCaveHoleCutter
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("InfectionCaveHole");

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetGameObject);
        prefab.Register();
    }

    private static GameObject GetGameObject()
    {
        var gameObject = new GameObject(Info.ClassID);
        gameObject.SetActive(false);
        PrefabUtils.AddBasicComponents(gameObject, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Far);
        var holeCutter = gameObject.AddComponent<CutHoleInTerrain>();
        holeCutter.holeRadius = 35;
        holeCutter.disableRenderers = true;
        holeCutter.disableColliders = true;
        holeCutter.disableGrass = true;
        return gameObject;
    }
}