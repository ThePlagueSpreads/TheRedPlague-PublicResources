using Nautilus.Handlers;
using UnityEngine;

namespace TheRedPlague.Data;

public static class CustomBackgroundTypes
{
    public static CraftData.BackgroundType PlagueItem { get; private set; }

    public static void RegisterCustomBackgroundTypes()
    {
        var itemBackground = new Atlas.Sprite(Plugin.AssetBundle.LoadAsset<Sprite>("PlagueItemBackground"));
        FixItemBackgroundSprite(itemBackground);
        PlagueItem = EnumHandler.AddEntry<CraftData.BackgroundType>("PlagueItem")
            .WithBackground(itemBackground);
    }

    private static void FixItemBackgroundSprite(Atlas.Sprite sprite)
    {
        sprite.slice9Grid = true;
    }
}