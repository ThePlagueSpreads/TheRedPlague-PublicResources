using Nautilus.Assets;
using TheRedPlague.Utilities;

namespace TheRedPlague.PrefabFiles.Items;

public static class BioBattery
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("BioBattery");

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(() => TrpPrefabUtils.CreateLootCubePrefab(Info));
        prefab.Register();
    }
}