using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Nautilus.Extensions;
using UnityEngine;

namespace TheRedPlague.Utilities;

public static class GenerateBasePrefabCode
{
    private const string PathPrefix = "Assets/Prefabs/Base/GeneratorPieces/";
    
    public static void ConvertBaseToPrefabConstructionCode(Base @base)
    {
        var cells = @base.cellObjects;

        var spawnablePieces = new Dictionary<string, List<SpawnablePiece>>();
        foreach (var cell in cells)
        {
            if (cell == null) continue;
            foreach (Transform childObject in cell)
            {
                var name = childObject.name.TrimClone();
                if (!spawnablePieces.TryGetValue(name, out var piecesList))
                {
                    piecesList = new List<SpawnablePiece>();
                    spawnablePieces.Add(name, piecesList);
                }
                piecesList.Add(new SpawnablePiece(name, childObject.transform.localPosition + cell.localPosition, childObject.transform.localRotation));

            }
        }

        var sb = new StringBuilder();

        int requestIndex = 1;
        foreach (var entry in spawnablePieces)
        {
            var requestVariable = $"request{requestIndex}";
            sb.AppendLine($"var {requestVariable} = PrefabDatabase.GetPrefabForFilenameAsync(\"{PathPrefix}{entry.Key}.prefab\");");
            sb.AppendLine($"yield return {requestVariable};");
            sb.AppendLine($"if (!{requestVariable}.TryGetPrefab(out var {GetPrefabVariableName(entry.Key)}))");
            sb.AppendLine("{");
            sb.AppendLine($"    Plugin.Logger.LogError(\"Failed to load prefab with name {entry.Key}!\");");
            sb.AppendLine("    yield break;");
            sb.AppendLine("}");
            requestIndex++;
        }

        int objIndex = 1;
        foreach (var entry in spawnablePieces)
        {
            foreach (var piece in entry.Value)
            {
                var variableName = $"child{objIndex}";
                sb.AppendLine($"var {variableName} = Object.Instantiate({GetPrefabVariableName(entry.Key)}, obj.transform);");
                sb.AppendLine($"{variableName}.transform.localPosition = {FormatVector3(piece.LocalPosition)};");
                sb.AppendLine($"{variableName}.transform.localRotation = {FormatQuaternion(piece.LocalRotation)};");
                sb.AppendLine($"{variableName}.SetActive(true);");
                sb.AppendLine($"StripComponents({variableName});");
                objIndex++;
            }
        }

        var text = sb.ToString();
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "BaseGenerationCode.txt");
        File.WriteAllText(path, text);
        ErrorMessage.AddMessage($"Successfully created base generation code at path '{path}'.");
    }

    private static string FormatVector3(Vector3 vector3)
    {
        return $"new Vector3({vector3.x}f, {vector3.y}f, {vector3.z}f)";
    }
    
    private static string FormatQuaternion(Quaternion quaternion)
    {
        return $"new Quaternion({quaternion.x}f, {quaternion.y}f, {quaternion.z}f, {quaternion.w}f)";
    }
    
    private static string GetPrefabVariableName(string prefabName)
    {
        if (prefabName.Length <= 1) return prefabName;
        return char.ToLower(prefabName[0]) + prefabName.Substring(1);
    }
    
    private readonly struct SpawnablePiece
    {
        public string Name { get; }
        public string AddressablePath { get; }
        public Vector3 LocalPosition { get; }
        public Quaternion LocalRotation { get; }

        public SpawnablePiece(string name, Vector3 localPosition, Quaternion localRotation)
        {
            Name = name;
            AddressablePath = PathPrefix + name + ".prefab";
            LocalPosition = localPosition;
            LocalRotation = localRotation;
        }
    }
}