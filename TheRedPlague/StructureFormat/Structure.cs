﻿using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace TheRedPlague.StructureFormat;

/// <summary>
/// Represents a group of entities.
/// </summary>
[Serializable]
public class Structure
{
    public string Name { get; set; }
    public Entity[] Entities { get; private set; }
    
    public bool IsSorted { get; private set; }

    /// <summary>
    /// Creates a new Entity Group with the given entities.
    /// </summary>
    /// <seealso cref="Structure.LoadFromFile"/>
    public Structure(string name, Entity[] entities)
    {
        Name = name;
        Entities = entities;
    }
    
    public static Structure LoadFromFile(string jsonFilePath)
    {
        var structure = JsonConvert.DeserializeObject<Structure>(File.ReadAllText(jsonFilePath));
        structure.Name = Path.GetFileNameWithoutExtension(jsonFilePath);
        return structure;
    }
    
    public void SaveToFile(string jsonFilePath)
    {
        File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
    }

    public void SortByPriority()
    {
        Entities = Entities.OrderBy(entity => entity.priority).ToArray();
        IsSorted = true;
    }
}