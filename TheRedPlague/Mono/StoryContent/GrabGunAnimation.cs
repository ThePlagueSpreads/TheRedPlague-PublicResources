using Story;
using UnityEngine;

namespace TheRedPlague.Mono.StoryContent;

public class GrabGunAnimation : MonoBehaviour
{
    private bool _isGunDisabled;

    private bool _checkedStoryGoal;

    public Animator gunAnimator;
    public Animator tentacle1Animator;
    public Animator tentacle2Animator;
    public GameObject tentacleRoot;
    public RuntimeAnimatorController gunOverrideAnimatorController;
    
    private static readonly int InstantAnimatorParameter = Animator.StringToHash("instant");

    private void Start()
    {
        if (GetIsGunDisabled())
        {
            StartGunAnimation(true);
        }
    }
    
    public bool GetIsGunDisabled()
    {
        if (_checkedStoryGoal) return _isGunDisabled;
            
        _isGunDisabled = StoryGoalManager.main.IsGoalComplete(StoryUtils.GrabGun.key);
        _checkedStoryGoal = true;

        return _isGunDisabled;
    }

    public void DisableGun()
    {
        _isGunDisabled = true;
        StoryUtils.GrabGun.Trigger();
        StartGunAnimation(false);
    }

    private void StartGunAnimation(bool instant)
    {
        tentacleRoot.SetActive(true);
        gunAnimator.runtimeAnimatorController = gunOverrideAnimatorController;
        if (!instant) return;
        gunAnimator.SetTrigger(InstantAnimatorParameter);
        tentacle1Animator.SetTrigger(InstantAnimatorParameter);
        tentacle2Animator.SetTrigger(InstantAnimatorParameter);
    }
}