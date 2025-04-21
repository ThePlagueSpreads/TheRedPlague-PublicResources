using UnityEngine;

namespace TheRedPlague.Mono.Buildables.PlagueAltar;

public class PlagueAltarEye : MonoBehaviour
{
    public bool flip;
    
    private void Update()
    {
        var vector = (MainCamera.camera.transform.position - transform.position).normalized;
        if (flip) vector *= -1;
        transform.up = vector;
    }
}