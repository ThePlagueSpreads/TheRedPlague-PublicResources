using Story;
using UnityEngine;

namespace TheRedPlague.Mono.StoryContent.Precursor;

public class GenericForceFieldDoor : MonoBehaviour, IStoryGoalListener
{
    public string storyGoalKey;
    public Renderer[] renderers;
    public VFXLerpColor colorControl;
    public GameObject colliderObject;
    public FMOD_CustomLoopingEmitter soundEmitter;

    private bool _isOpen;
    
    private void OnEnable()
    {
        NoCostConsoleCommand.main.UnlockDoorsEvent += OnUnlockDoorsCheat;
    }

    private void OnDisable()
    {
        NoCostConsoleCommand.main.UnlockDoorsEvent -= OnUnlockDoorsCheat;
    }
    
    private void Start()
    {
        if (StoryGoalManager.main.IsGoalComplete(storyGoalKey))
        {
            ToggleDoor(true);
            return;
        }
        StoryGoalManager.main.AddListener(this);
        soundEmitter.Play();
    }

    private void OnUnlockDoorsCheat()
    {
        ToggleDoor(NoCostConsoleCommand.main.unlockDoors);
    }
    
    private void ToggleDoor(bool open)
    {
        if (open != _isOpen)
        {
            SetDoorState(open);
        }
    }
    
    private void SetDoorState(bool open)
    {
        if (open)
        {
            DisableDoor();
        }
        else
        {
            EnableDoor();
        }
        _isOpen = open;
    }
    
    private void EnableDoor()
    {
        colliderObject.SetActive(true);
        soundEmitter.Play();
        colorControl.gameObject.SetActive(value: true);
        colorControl.ResetColor();
        CancelInvoke(nameof(DisableVisual));
        foreach (var renderer in renderers)
        {
            renderer.enabled = true;
        }
    }

    private void DisableDoor()
    {
        colorControl.Play();
        soundEmitter.Stop();
        Invoke(nameof(DisableVisual), colorControl.duration);
        colliderObject.SetActive(false); 
    }

    public void NotifyGoalComplete(string key)
    {
        if (_isOpen) return;
        if (key.Equals(storyGoalKey))
        {
            ToggleDoor(true);
        }
    }
    
    private void DisableVisual()
    {
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }
    }

    private void OnDestroy()
    {
        StoryGoalManager.main.RemoveListener(this);
    }
}