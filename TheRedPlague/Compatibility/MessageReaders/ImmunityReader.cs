using System;
using Nautilus.Utility.ModMessages;

namespace TheRedPlague.Compatibility.MessageReaders;

public class ImmunityReader : ModMessageReader
{
    protected override void OnReceiveMessage(ModMessage message)
    {
        if (message.Subject != "SetTechTypeImmune") return;
        try
        {
            var techType = (TechType)message.Contents[0];
            ModCompatibilityManager.AddImmuneTechType(techType);
            Plugin.Logger.LogDebug($"Added '{techType}' to list of immune tech types");
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Failed to handle message '{message}'!\nException: {e}");
        }
    }
}