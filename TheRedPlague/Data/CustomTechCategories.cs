using Nautilus.Handlers;

namespace TheRedPlague.Data;

public static class CustomTechCategories
{
    public static TechGroup PlagueBiotechGroup { get; private set; }
    public static TechCategory PlagueBiotechCategory { get; private set; }

    public static void RegisterAll()
    {
        PlagueBiotechGroup = EnumHandler.AddEntry<TechGroup>("PlagueBiotech")
            .WithPdaInfo(null);

        PlagueBiotechCategory = EnumHandler.AddEntry<TechCategory>("PlagueBiotech")
            .WithPdaInfo(null)
            .RegisterToTechGroup(PlagueBiotechGroup);
    }
}