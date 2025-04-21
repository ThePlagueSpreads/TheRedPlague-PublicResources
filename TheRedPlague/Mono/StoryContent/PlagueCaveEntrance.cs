using System.Collections.Generic;
using Nautilus.Utility;
using Story;
using UnityEngine;

namespace TheRedPlague.Mono.StoryContent;

public class PlagueCaveEntrance : MonoBehaviour
{
    private static List<PlagueCaveEntrance> _all = new();
    
    private static readonly int InstantParam = Animator.StringToHash("instant");
    private static readonly int OpenParam = Animator.StringToHash("open");

    public Animator animator;
    public GameObject blocker;

    private bool _isOpen;

    private static readonly FMODAsset OpenSound = AudioUtils.GetFmodAsset("PlagueCaveOpenSFX");

    private void OnEnable()
    {
        _all.Add(this);
    }

    private void OnDisable()
    {
        _all.Remove(this);
    }

    private void Start()
    {
        if (StoryGoalManager.main.IsGoalComplete(StoryUtils.OpenPlagueCave.key))
        {
            Open(true);
        }
    }

    public void Open(bool instant = false)
    {
        if (instant)
        {
            animator.SetBool(InstantParam, true);
        }
        else
        {
            StoryUtils.OpenPlagueCave.Trigger();
            Utils.PlayFMODAsset(OpenSound, transform.position);
            MainCameraControl.main.ShakeCamera(0.4f, 10, MainCameraControl.ShakeMode.Linear, 0.3f);
        }
        animator.SetBool(OpenParam, true);
        _isOpen = true;
        blocker.SetActive(false);
    }
    
    public void Close()
    {
        StoryGoalManager.main.completedGoals.Remove(StoryUtils.OpenPlagueCave.key);
        animator.SetBool(OpenParam, false);
        animator.SetBool(InstantParam, false);
        _isOpen = false;
        blocker.SetActive(true);
    }
    
    public bool GetIsOpen() => _isOpen;

    public static void OpenAll()
    {
        foreach (var entrance in _all)
        {
            if (entrance) entrance.Open();
        }
    }

    public static void CloseAll()
    {
        foreach (var entrance in _all)
        {
            if (entrance) entrance.Close();
        }
    }
}