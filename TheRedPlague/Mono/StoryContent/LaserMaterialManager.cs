using Story;
using UnityEngine;

namespace TheRedPlague.Mono.StoryContent;

public class LaserMaterialManager : MonoBehaviour, IStoryGoalListener
{
    private Renderer _renderer;
    
    private void Start()
    {
        _renderer = GetComponentInChildren<Renderer>(true);

        if (!StoryGoalManager.main.IsGoalComplete(StoryUtils.DomeConstructionFinishedEvent.key))
        {
            _renderer.enabled = false;
        }
        
        StoryGoalManager.main.AddListener(this);
    }

    public void NotifyGoalComplete(string key)
    {
        if (key == StoryUtils.DomeConstructionFinishedEvent.key)
        {
            _renderer.enabled = true;
        }
    }

    private void DisableRenderer() => _renderer.enabled = false;
    
    private void SetColorYellow()
    {
        _renderer.material.color = new Color(3, 3, 0.977941f);
    }
    
    private void SetColorGreen()
    {
        _renderer.material.color = new Color(2, 4.5f, 1);
    }
    
    private void OnDestroy()
    {
        StoryGoalManager main = StoryGoalManager.main;
        if (main)
        {
            main.RemoveListener(this);
        }
    }
}