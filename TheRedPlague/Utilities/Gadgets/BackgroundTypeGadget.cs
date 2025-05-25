using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;

namespace TheRedPlague.Utilities.Gadgets;

public class BackgroundTypeGadget : Gadget
{
    private readonly CraftData.BackgroundType _backgroundType;
    
    public BackgroundTypeGadget(ICustomPrefab prefab, CraftData.BackgroundType backgroundType) : base(prefab)
    {
        _backgroundType = backgroundType;
    }

    protected override void Build()
    {
        CraftDataHandler.SetBackgroundType(prefab.Info.TechType, _backgroundType);
    }
}