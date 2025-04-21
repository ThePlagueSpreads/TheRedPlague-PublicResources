using Nautilus.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheRedPlague.Mono.VFX;

public class LifePodScare : MonoBehaviour
{
    public float minInterval = 4 * 60;
    public float maxInterval = 10 * 60;
    public float existDuration = 5f;
    
    private float _timeScareAgain;

    private readonly Vector3[] _startPositions = new Vector3[]
    {
        new Vector3(-0.140f, 4.500f, 2.757f),
        new Vector3(-0.140f, 4.500f, -2.481f),
        new Vector3(0.700f, 3.850f, 0.000f),
    };

    private Vector3[] _startAngles = new Vector3[]
    {
        new Vector3(270, 0, 0),
        new Vector3(270, 180, 0),
        new Vector3(5, 90, 0),
    };

    private void Start()
    {
        _timeScareAgain = Time.time + Random.Range(minInterval, maxInterval);
    }

    private void Update()
    {
        if (Time.time < _timeScareAgain)
            return;
        _timeScareAgain = Time.time + Random.Range(minInterval, maxInterval);
        if (EscapePod.main == null)
            return;
        if (!StoryUtils.IsAct1Complete())
            return;
        if (Player.main.currentEscapePod == null)
        {
            var playerDist = Vector3.Distance(Player.main.transform.position, EscapePod.main.transform.position);
            if (playerDist > 40 && playerDist < 500 && Random.value < 0.25f)
            {
                Scare(2);
            }
            return;
        }
        Scare(Random.Range(0, _startPositions.Length));
    }

    private void Scare(int index)
    {
        var obj = Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("MutantProp"), EscapePod.main.transform, true);
        obj.transform.localPosition = _startPositions[index];
        obj.transform.localEulerAngles = _startAngles[index];
        obj.AddComponent<SkyApplier>().renderers = obj.GetComponentsInChildren<Renderer>();
        MaterialUtils.ApplySNShaders(obj);
        Destroy(obj, existDuration);
    }
}