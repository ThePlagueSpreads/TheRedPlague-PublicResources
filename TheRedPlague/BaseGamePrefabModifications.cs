using System.Collections;
using TheRedPlague.Mono.Util;
using UWE;

namespace TheRedPlague;

public static class BaseGamePrefabModifications
{
    public static void ModifyBaseGamePrefabs()
    {
        CoroutineHost.StartCoroutine(MainProcess());
    }

    private static IEnumerator MainProcess()
    {
        yield return ModifyCoralTubes();
        yield return ModifyWreckProps();
        yield return ModifyJellyShrooms();
    }

    // Modify coral tubes to remove the two around the elevator platform
    private static IEnumerator ModifyCoralTubes()
    {
        var coralTube = PrefabDatabase.GetPrefabAsync("f0295655-8f4f-4b18-b67d-925982a472d7");
        yield return coralTube;
        if (coralTube.TryGetPrefab(out var coralTubePrefab))
        {
            coralTubePrefab.AddComponent<DestroyIfIdMatches>().ids = new[]
            {
                "32b32bec-4665-4735-9c1a-7c2c5291a0ee",
                "679e863d-69dc-48b7-af0e-a7c6d311020b"
            };
        }
    }
    
    // Modify wreck props to remove the one clipping into the maze base
    private static IEnumerator ModifyWreckProps()
    {
        var wreckDunes6 = PrefabDatabase.GetPrefabAsync("38f4a1d4-7cbc-4a21-a953-02b3f667975f");
        yield return wreckDunes6;
        if (wreckDunes6.TryGetPrefab(out var wreckDunes6Prefab))
        {
            wreckDunes6Prefab.transform.Find("Interactable/Starship_exploded_debris_41(Placeholder)").gameObject.SetActive(false);
        }
    }
    
    // Modify jelly shrooms to replace with flesh plant 1
    private static IEnumerator ModifyJellyShrooms()
    {
        var jellyShroom2 = PrefabDatabase.GetPrefabAsync("400fa668-152d-4b81-ad8f-a3cef16efed8");
        yield return jellyShroom2;
        if (jellyShroom2.TryGetPrefab(out var jellyShroom2Prefab))
        {
            jellyShroom2Prefab.AddComponent<DestroyIfIdMatches>().ids = new[]
            {
                "11f224f8-13f2-4f92-b571-16e2386dc368",
                "598cd9ce-8d85-40c0-8af6-197fdfdd9a0a",
                "f40792f9-4a72-4a6f-af07-f1c011b2445c",
                "443026f6-3f48-42d7-b714-e10213d74474",
                "3c78d3be-897a-4f1b-918e-82126e1aeffc",
                "e6f6353c-9857-499e-98b6-b52f1ba5bdc0",
                "7e0b887a-db70-490d-8072-cf51d5db6ee3"
            };
        }
        
        var jellyShroom3 = PrefabDatabase.GetPrefabAsync("8d0b24b7-c71f-42ab-8df9-7bfe05616ab4");
        yield return jellyShroom3;
        if (jellyShroom3.TryGetPrefab(out var jellyShroom3Prefab))
        {
            jellyShroom3Prefab.AddComponent<DestroyIfIdMatches>().ids = new[]
            {
                "f8751fb6-6996-4935-a203-c1345e131f05"
            };
        }
    }
}