using Nautilus.Utility;
using Story;

namespace TheRedPlague.Mono.StoryContent;

public class IslandElevatorKeyTerminalBehaviour : CustomTabletTerminalBehaviour
{
    protected override void OnCinematicModeStarted()
    {
        if (IslandElevatorBehaviour.Main)
        {
            Utils.PlayFMODAsset(AudioUtils.GetFmodAsset("IslandElevatorActivation"),
                IslandElevatorBehaviour.Main.transform.position);
        }
    }

    protected override void OnActivation()
    {
        StoryUtils.IslandElevatorActivatedGoal.Trigger();
    }

    protected override bool LoadSavedSlottedState()
    {
        return StoryGoalManager.main.IsGoalComplete(StoryUtils.IslandElevatorActivatedGoal.key);
    }

    protected override void SaveStateAsSlotted()
    {
        
    }
}