using System;
using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using TheRedPlague.Mono.StoryContent;
using TheRedPlague.Mono.StoryContent.Precursor;
using TheRedPlague.Mono.Util;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheRedPlague.PrefabFiles.StoryProps;

public class CustomTabletTerminalPrefab<T> where T : CustomTabletTerminalBehaviour
{
    private PrefabInfo Info { get; }

    private PrecursorKeyTerminal.PrecursorKeyType KeyType { get; }
    
    private bool UsesCustomKeyType { get; }
    private Texture2D CustomKeyTexture { get; }
    private TechType CustomKeyTechType { get; }
    
    public Action<T> ModifyComponent { get; init; }

    public CustomTabletTerminalPrefab(string classId, PrecursorKeyTerminal.PrecursorKeyType acceptKeyType)
    {
        Info = PrefabInfo.WithTechType(classId);
        KeyType = acceptKeyType;
    }
    
    public CustomTabletTerminalPrefab(string classId, Texture2D customKeyTexture, TechType customKeyTechType)
    {
        Info = PrefabInfo.WithTechType(classId);
        UsesCustomKeyType = true;
        CustomKeyTexture = customKeyTexture;
        CustomKeyTechType = customKeyTechType;
    }

    public void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(new CloneTemplate(Info, "c718547d-fe06-4247-86d0-efd1e3747af0")
        {
            ModifyPrefab = ModifyPrefab
        });
        prefab.Register();
    }

    private void ModifyPrefab(GameObject prefab)
    {
        var terminalComponent = prefab.GetComponent<PrecursorKeyTerminal>();

        var behaviour = prefab.AddComponent<T>();
        
        behaviour.acceptKeyType = KeyType;
        behaviour.usesCustomKeyType = UsesCustomKeyType;
        behaviour.customKeyTexture = CustomKeyTexture;
        behaviour.customKeyTechType = CustomKeyTechType;
        
        behaviour.keyMats = terminalComponent.keyMats;
        behaviour.keyFace = terminalComponent.keyFace;
        behaviour.animator = terminalComponent.animator;
        behaviour.cinematicController = terminalComponent.cinematicController;
        behaviour.useSound = terminalComponent.useSound;
        behaviour.openSound = terminalComponent.openSound;
        behaviour.closeSound = terminalComponent.closeSound;

        if (Info.ClassID == "IslandElevatorTerminal")
        {
            prefab.AddComponent<DestroyIfIdMatches>().ids = new[] { "b3e8e534-8363-4bac-8875-847d5b3cd032" };
        }

        ModifyComponent?.Invoke(behaviour);
            
        Object.DestroyImmediate(terminalComponent);
    }
}