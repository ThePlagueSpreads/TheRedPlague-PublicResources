using Story;
using UnityEngine;

namespace TheRedPlague.Mono.Triggers;

public class ConditionalStoryGoalTrigger : PlayerTrigger
{
    public StoryGoal requiredGoal;
    public StoryGoal goal;

    public bool destroyOnActivate = true;

    private void Start()
    {
        if (goal.key == null)
        {
            Plugin.Logger.LogError($"No key assigned to StoryGoalTrigger '{this}'!");
            return;
        }
        if (destroyOnActivate && StoryGoalManager.main.IsGoalComplete(goal.key))
        {
            Destroy(this);
            Destroy(GetComponent<Collider>());
        }
    }


    protected override void OnTriggerActivated()
    {
        if (!StoryGoalManager.main.IsGoalComplete(requiredGoal.key))
            return;
        
        goal.Trigger();
        
        if (destroyOnActivate)
        {
            Destroy(this);
            Destroy(GetComponent<Collider>());
        }
    }
}