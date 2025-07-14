using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheRedPlague.StructureFormat;
using UnityEngine;

namespace TheRedPlague;

public static class FleshCaveFix
{
    public static void FixFleshCave()
    {
        Plugin.Logger.LogMessage("Loading flesh cave");
        var realFleshCave = Structure.LoadFromFile(Path.Combine(Path.GetDirectoryName(Plugin.Assembly.Location), "Structures", "Flesh cave decorations.structure"));
        Plugin.Logger.LogMessage("Loading flesh cave history folder");
        string folderPath = Path.Combine(Path.GetDirectoryName(Plugin.Assembly.Location), "OldFleshCaves");
        Plugin.Logger.LogMessage("Loading flesh cave history");
        var files = Directory.GetFiles(folderPath);
        var filesList = new List<string>(files);
        var sortedFiles = filesList.OrderBy(f => int.Parse(f.Split(Path.DirectorySeparatorChar).Last().Split('(', ')')[1]));
        
        var historicalEntities = new List<Entity>();
        foreach (var file in sortedFiles)
        {
            Plugin.Logger.LogMessage("Loading " + file);
            var structureFile = Structure.LoadFromFile(file);
            foreach (var entity in structureFile.Entities)
            {
                historicalEntities.Add(entity);
            }
            Plugin.Logger.LogMessage("Loaded " + structureFile.Entities.Length + " entities");
        }

        int found = 0;
        int successes = 0;
        for (int i = 0; i < realFleshCave.Entities.Length; i++)
        {
            if (realFleshCave.Entities[i] == null)
            {
                Plugin.Logger.LogError("Null entity detected!");
                continue;
            }
            if (!string.IsNullOrEmpty(realFleshCave.Entities[i].classId))
                continue;
            Plugin.Logger.LogMessage("Null entity found! Attempting to recover");
            found++;
            if (TryGetCloseMatch(historicalEntities, realFleshCave.Entities[i], out var match))
            {
                realFleshCave.Entities[i] = match;
                successes++;
            }
            else
            {
                Plugin.Logger.LogWarning($"Failed to find match for entity at position {realFleshCave.Entities[i].position.ToVector3()}!");
            }
        }
        Plugin.Logger.LogMessage($"Found {found} problematic entities! Fixed {successes}/{found} ({(float)successes/found * 100}%) of the entities.");
        realFleshCave.SaveToFile(Path.Combine(Path.GetDirectoryName(Plugin.Assembly.Location), "Structures", "Flesh cave decorations FIXED.structure"));
    }

    private static bool TryGetCloseMatch(List<Entity> all, Entity original, out Entity match)
    {
        var entityPosition = Round(original.position.ToVector3(), 4);
        var entityRotation = Round(original.rotation.ToQuaternion(), 4);
        bool foundMatch = false;
        match = null;

        foreach (var entity in all)
        {
            if (string.IsNullOrEmpty(entity.classId) || string.IsNullOrEmpty(entity.id))
                continue;
            
            var comparedPosition = Round(entity.position.ToVector3(), 4);
            var comparedRotation = Round(entity.rotation.ToQuaternion(), 4);

            bool positionMatches = entityPosition.Equals(comparedPosition);
            bool rotationMatches = entityRotation.Equals(comparedRotation);

            if (positionMatches && rotationMatches)
            {
                match = entity;
                foundMatch = true;
            }
            else if (positionMatches)
            {
                Plugin.Logger.LogWarning("Entity position matches but rotation does not!");
            }
        }

        if (foundMatch)
            return true;
        
        return false;
    }
    
    private static Vector3 Round(this Vector3 vector3, int decimalPlaces = 2)
    {
        float multiplier = 1;
        for (int i = 0; i < decimalPlaces; i++)
        {
            multiplier *= 10f;
        }
        return new Vector3(
            Mathf.Round(vector3.x * multiplier) / multiplier,
            Mathf.Round(vector3.y * multiplier) / multiplier,
            Mathf.Round(vector3.z * multiplier) / multiplier);
    }
    
    private static Quaternion Round(this Quaternion quaternion, int decimalPlaces = 2)
    {
        float multiplier = 1;
        for (int i = 0; i < decimalPlaces; i++)
        {
            multiplier *= 10f;
        }
        return new Quaternion(
            Mathf.Round(quaternion.x * multiplier) / multiplier,
            Mathf.Round(quaternion.y * multiplier) / multiplier,
            Mathf.Round(quaternion.z * multiplier) / multiplier,
            Mathf.Round(quaternion.w * multiplier) / multiplier);
    }
}