using System.Collections;
using UnityEngine;

namespace TheRedPlague.Mono.Insanity;

public abstract class InsanitySymptom : MonoBehaviour
{
    public bool IsSymptomActive { get; private set; }
    
    protected bool AssetsLoaded { get; private set; }

    private bool _loadingAssets;
    private InsanityManager _manager;
    protected float InsanityPercentage => _manager.Insanity;

    public IEnumerator LoadAssetsCoroutine(InsanityManager manager)
    {
        if (AssetsLoaded)
        {
            Plugin.Logger.LogWarning($"Assets for '{this}' are already loaded!");
            yield break;
        }
        
        if (_loadingAssets)
        {
            Plugin.Logger.LogWarning("Attempting to call InsanitySymptom.LoadAssets while assets are already loading!");
            yield break;
        }
        
        _manager = manager;
        _loadingAssets = true;
        yield return OnLoadAssets();
        AssetsLoaded = true;
        _loadingAssets = false;
    }

    public void Activate()
    {
        IsSymptomActive = true;
        OnActivate();
    }

    public void Deactivate()
    {
        IsSymptomActive = false;
        OnDeactivate();
    }


    public bool ShouldActivate() => ShouldDisplaySymptoms();
    public void DoLazyUpdate(float dt) => PerformSymptoms(dt);
    
    protected abstract IEnumerator OnLoadAssets();
    protected abstract void OnActivate();
    protected abstract void OnDeactivate();
    protected abstract bool ShouldDisplaySymptoms();
    protected virtual void PerformSymptoms(float dt) { }
}