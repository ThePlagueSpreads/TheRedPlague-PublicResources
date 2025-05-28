using Story;

namespace TheRedPlague.Mono.Triggers;

public class StoryGoalTrigger : PlayerTrigger
{
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
        }
    }


    protected override void OnTriggerActivated()
    {
        goal.Trigger();
        
        if (destroyOnActivate)
        {
            Destroy(this);
        }
    }
}