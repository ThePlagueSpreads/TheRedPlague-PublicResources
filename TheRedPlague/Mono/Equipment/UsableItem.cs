using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheRedPlague.Mono.Equipment;

public class UsableItem : MonoBehaviour
{
    [SerializeField]
    private string cachedId;
    
    private static readonly Dictionary<string, Action> CachedActions = new();

    private Action _uniqueAction;
    private bool _useUniqueAction;

    public void SetOnUseAction(string classId, Action action)
    {
        cachedId = classId;
        CachedActions[classId] = action;
    }
    
    public void SetUniqueUseAction(Action action)
    {
        _uniqueAction = action;
        _useUniqueAction = true;
    }

    public void ExecuteOnUseAction()
    {
        if (_useUniqueAction)
        {
            _uniqueAction.Invoke();
            return;
        }
        
        if (CachedActions.TryGetValue(cachedId, out var action))
        {
            action?.Invoke();
        }
        else
        {
            Plugin.Logger.LogError($"Failed to find on use action for item {gameObject} (id: {cachedId})!");
        }
    }
}