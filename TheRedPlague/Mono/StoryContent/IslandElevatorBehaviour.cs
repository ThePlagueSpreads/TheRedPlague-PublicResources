using Story;
using UnityEngine;

namespace TheRedPlague.Mono.StoryContent;

public class IslandElevatorBehaviour : MonoBehaviour, IStoryGoalListener
{
    public static IslandElevatorBehaviour Main { get; private set; }
    
    private void Start()
    {
        Main = this;
        StoryGoalManager.main.AddListener(this);
        SetElevatorActive(StoryGoalManager.main.IsGoalComplete(StoryUtils.IslandElevatorActivatedGoal.key));
    }

    public void NotifyGoalComplete(string key)
    {
        if (key == StoryUtils.IslandElevatorActivatedGoal.key)
        {
            SetElevatorActive(true);
        }
    }

    private void SetElevatorActive(bool state)
    {
        transform.Find("elevator_bot_trigger").gameObject.SetActive(state);
        transform.Find("FX").gameObject.SetActive(state);
    }

    private void OnDestroy()
    {
        StoryGoalManager.main.RemoveListener(this);
    }
}