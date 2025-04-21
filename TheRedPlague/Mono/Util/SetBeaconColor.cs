using UnityEngine;

namespace TheRedPlague.Mono.Util;

public class SetBeaconColor : MonoBehaviour
{
    public int colorIndex;
    
    private void Start()
    {
        GetComponent<PingInstance>().SetColor(colorIndex);
    }
}