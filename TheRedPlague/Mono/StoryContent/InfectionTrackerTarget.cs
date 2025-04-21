using UnityEngine;

namespace TheRedPlague.Mono.StoryContent;

public class InfectionTrackerTarget : MonoBehaviour
{
    public static InfectionTrackerTarget main;

    private void Awake()
    {
        main = this;
    }
}