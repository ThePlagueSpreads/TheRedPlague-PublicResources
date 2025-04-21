using System;
using UnityEngine;

namespace TheRedPlague.Mono.Util;

public class DestroyIfIdMatches : MonoBehaviour
{
    public string[] ids = Array.Empty<string>();
    
    private void Start()
    {
        if (ShouldIKillMyself())
        {
            Destroy(gameObject);
        }
    }

    private bool ShouldIKillMyself()
    {
        var identifier = gameObject.GetComponent<UniqueIdentifier>();
        if (identifier == null) return false;
        
        var myId = identifier.Id;
        
        foreach (var candidate in ids)
        {
            if (candidate == myId) return true;
        }

        return false;
    }
}