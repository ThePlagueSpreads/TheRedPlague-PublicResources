using UnityEngine;

namespace TheRedPlague.Mono.StoryContent;

public class PlagueHeartBehavior : MonoBehaviour
{
    public static PlagueHeartBehavior main;

    private void Awake()
    {
        main = this;
    }
}