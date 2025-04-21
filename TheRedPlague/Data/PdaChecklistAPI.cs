using System.Collections.Generic;
using Story;
using TheRedPlague.PrefabFiles.Items;

namespace TheRedPlague.Data;

public static class PdaChecklistAPI
{
    private static readonly Dictionary<string, ChecklistEntry> Entries = new();
    private static readonly Dictionary<string, List<ChecklistEntry>> EntriesByRequiredGoal = new();

    public delegate string FormatChecklistEntryHandler(ChecklistEntry entry);

    public delegate bool CustomEntryRequirementsHandler(ChecklistEntry entry);

    internal static void RegisterTrpEntries()
    {
        RegisterChecklistEntry(new ChecklistEntry(StoryUtils.OfferingToThePlagueMission.key,
            StoryUtils.ScanPlagueAltarGoal.key));
        RegisterChecklistEntry(new ChecklistEntry(StoryUtils.MrTeethMission.key,
            StoryUtils.UnlockPlagueGrapplerGoal.key));
        RegisterChecklistEntry(new ChecklistEntry(StoryUtils.JohnKyleMission.key,
            StoryUtils.UnlockPlagueArmorGoal.key));
        RegisterChecklistEntry(new ChecklistEntry(StoryUtils.PlagueResearchMission.key,
            StoryUtils.GetStoryGoalKeyForShuttleDelivery(ModPrefabs.AmalgamatedBone.ClassID),
            StoryUtils.GetStoryGoalKeyForShuttleDelivery(ModPrefabs.WarperHeart.ClassID),
            StoryUtils.GetStoryGoalKeyForShuttleDelivery(PlagueCatalyst.Info.ClassID),
            StoryUtils.GetStoryGoalKeyForShuttleDelivery(PlagueIngot.Info.ClassID),
            StoryUtils.GetStoryGoalKeyForShuttleDelivery(DormantNeuralMatter.Info.ClassID),
            StoryUtils.GetStoryGoalKeyForShuttleDelivery(MysteriousRemains.Info.ClassID))
        {
            CustomEntryRequirementsHandler = e =>
            {
                var completedGoals = 0;
                foreach (var requirement in e.RequiredGoals)
                {
                    if (StoryGoalManager.main.IsGoalComplete(requirement))
                        completedGoals++;
                }

                return completedGoals >= 6;
            },
            FormatHandler = e =>
            {
                var completedGoals = 0;
                foreach (var requirement in e.RequiredGoals)
                {
                    if (StoryGoalManager.main.IsGoalComplete(requirement))
                        completedGoals++;
                }

                return Language.main.GetFormat(e.GetNameLanguageKey, completedGoals);
            }
        });
    }

    public static void RegisterChecklistEntry(ChecklistEntry entry)
    {
        Entries.Add(entry.Key, entry);

        foreach (var requiredGoal in entry.RequiredGoals)
        {
            if (EntriesByRequiredGoal.TryGetValue(requiredGoal, out var list))
            {
                list.Add(entry);
            }
            else
            {
                list = new List<ChecklistEntry> { entry };
                EntriesByRequiredGoal.Add(requiredGoal, list);
            }
        }
    }

    public static bool TryGetChecklistEntryByKey(string key, out ChecklistEntry entry)
    {
        return Entries.TryGetValue(key, out entry);
    }

    public static bool TryGetChecklistEntriesForRequiredStoryGoal(string requiredStoryGoal,
        out IReadOnlyCollection<ChecklistEntry> entries)
    {
        var hasValue = EntriesByRequiredGoal.TryGetValue(requiredStoryGoal, out var list);
        if (!hasValue)
        {
            entries = null;
            return false;
        }

        entries = list;
        return true;
    }

    public static bool IsEntryCompleted(string key)
    {
        if (!TryGetChecklistEntryByKey(key, out var entry))
        {
            Plugin.Logger.LogWarning("Failed to find checklist entry by key " + key);
            return false;
        }

        return IsEntryCompleted(entry);
    }

    public static bool IsEntryCompleted(ChecklistEntry entry)
    {
        if (entry.CustomEntryRequirementsHandler != null)
        {
            return entry.CustomEntryRequirementsHandler.Invoke(entry);
        }

        var goalManager = StoryGoalManager.main;

        if (goalManager.IsGoalComplete(entry.Key))
            return true;

        foreach (var requiredGoal in entry.RequiredGoals)
        {
            if (!goalManager.IsGoalComplete(requiredGoal))
                return false;
        }

        return true;
    }

    public static IReadOnlyCollection<ChecklistEntry> GetChecklistEntries() => Entries.Values;

    public readonly struct ChecklistEntry
    {
        // Doubles as the StoryGoal that is used on completion
        public string Key { get; init; }
        public string[] RequiredGoals { get; init; }
        public FormatChecklistEntryHandler FormatHandler { get; init; }
        public CustomEntryRequirementsHandler CustomEntryRequirementsHandler { get; init; }

        public ChecklistEntry(string key, params string[] requiredGoals)
        {
            Key = key;
            RequiredGoals = requiredGoals;
        }

        public ChecklistEntry(string key, FormatChecklistEntryHandler formatHandler, params string[] requiredGoals) :
            this(key, requiredGoals)
        {
            FormatHandler = formatHandler;
        }

        public string GetNameLanguageKey => "Checklist_" + Key;
        public string GetDescLanguageKey => "ChecklistDesc_" + Key;
    }
}