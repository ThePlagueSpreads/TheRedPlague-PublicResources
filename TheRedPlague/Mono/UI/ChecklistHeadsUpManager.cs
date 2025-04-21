using System;
using System.Collections;
using Story;
using UnityEngine;

namespace TheRedPlague.Mono.UI;

public class ChecklistHeadsUpManager : MonoBehaviour, IStoryGoalListener
{
    public GameObject notificationObject;

    private bool _shown;
    private bool _disabled;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => StoryGoalManager.main != null && StoryGoalManager.main.initialized);
        _shown = StoryGoalManager.main.IsGoalComplete(StoryUtils.ChecklistUnlockGoalKey);
        _disabled = StoryGoalManager.main.IsGoalComplete(StoryUtils.ChecklistViewedFirstTimeGoalKey);
        if (!_shown || !_disabled)
        {
            StoryGoalManager.main.AddListener(this);
        }
        UpdateState();
    }

    public void NotifyGoalComplete(string key)
    {
        if (_shown && _disabled) return;
        if (!_shown && key.Equals(StoryUtils.ChecklistUnlockGoalKey, StringComparison.OrdinalIgnoreCase))
            _shown = true;
        if (!_disabled && key.Equals(StoryUtils.ChecklistViewedFirstTimeGoalKey, StringComparison.OrdinalIgnoreCase))
            _disabled = true;
        UpdateState();
    }

    private void UpdateState()
    {
        notificationObject.SetActive(_shown && !_disabled);
    }
}