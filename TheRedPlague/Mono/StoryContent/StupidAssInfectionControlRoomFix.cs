using UnityEngine;

namespace TheRedPlague.Mono.StoryContent;

public class StupidAssInfectionControlRoomFix : MonoBehaviour
{
    private void Start()
    {
        if (transform.position.y < 100)
        {
            Destroy(gameObject);

            Plugin.Logger.LogError("why did this stupid thing end up somewhere it isn't supposed to be?");
            Plugin.Logger.LogError("scene time: " + Time.timeSinceLevelLoad);
            Plugin.Logger.LogError("day/night time: " + DayNightCycle.main.timePassed);
        }
    }
}