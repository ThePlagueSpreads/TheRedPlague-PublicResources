using Nautilus.Assets;

namespace TheRedPlague.Utilities.Gadgets;

public static class CustomGadgetExtensions
{
    public static void SetBackgroundType(this ICustomPrefab prefab, CraftData.BackgroundType backgroundType)
    {
        prefab.AddGadget(new BackgroundTypeGadget(prefab, backgroundType));
    }
}