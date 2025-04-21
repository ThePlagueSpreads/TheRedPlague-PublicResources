using UnityEngine;

namespace TheRedPlague.Mono.Systems;

public class NpcSurvivorManager : MonoBehaviour
{
    public static NpcSurvivorManager main;

    private void Awake()
    {
        main = this;
    }
}