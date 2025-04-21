using System.Collections.Generic;
using Nautilus.Utility.ModMessages;
using TheRedPlague.Compatibility.MessageReaders;

namespace TheRedPlague.Compatibility;

internal static class ModCompatibilityManager
{
    private static readonly HashSet<TechType> ImmuneTechTypes = new();
    
    public static void RegisterAllCompatibility()
    {
        AmalgamationCompatibility.PatchCompatibility();

        var inbox = new RedPlagueInbox();
        inbox.AddMessageReader(new ImmunityReader());
        ModMessageSystem.RegisterInbox(inbox);
        inbox.ReadAnyHeldMessages();
    }

    public static void AddImmuneTechType(TechType techType)
    {
        if (techType == TechType.None)
        {
            Plugin.Logger.LogWarning("Trying to add TechType.None to ImmuneTechTypes!");
            return;
        }
        ImmuneTechTypes.Add(techType);
    }

    public static bool IsTechTypeImmune(TechType techType)
    {
        return techType != TechType.None && ImmuneTechTypes.Contains(techType);
    }
    
    public static bool HasAdvancedInventory() =>
        BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("sn.advancedinventory.mod");
}