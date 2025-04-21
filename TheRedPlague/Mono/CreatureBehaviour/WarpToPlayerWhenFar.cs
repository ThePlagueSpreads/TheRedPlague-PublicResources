using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour;

public class WarpToPlayerWhenFar : MonoBehaviour
{
    public float warpDistance = 40f;

    private void Start()
    {
        InvokeRepeating(nameof(WarpToPlayer), Random.value * 5, 5);
    }
    
    private void WarpToPlayer()
    {
        if (Player.main.GetBiomeString().StartsWith("precursor", System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        var difference = Player.main.transform.position - transform.position;
        if (!(difference.magnitude > warpDistance))
        {
            return;
        }
        var position = Player.main.transform.position - difference.normalized * warpDistance;
        position.y = Mathf.Min(position.y, Ocean.GetOceanLevel() - 1f);
        var num = UWE.Utils.OverlapSphereIntoSharedBuffer(base.transform.position, 5f);
        for (var i = 0; i < num; i++)
        {
            if ((bool)UWE.Utils.sharedColliderBuffer[i].GetComponentInParent<SubRoot>())
            {
                return;
            }
        }
        transform.position = position;
    }
}