using UnityEngine;

namespace TheRedPlague.Mono.SFX;

public class PlayIslandMusic : MonoBehaviour
{
    public FMOD_CustomLoopingEmitter emitter;

    public float playRadius = 100f;

    private float _timeReenable;

    private static PlayIslandMusic _instance;

    private void Start()
    {
        _instance = this;
        InvokeRepeating(nameof(LazyUpdate), Random.value, 0.5f);
    }

    private void LazyUpdate()
    {
        if (!isActiveAndEnabled) return;
        if (Time.time < _timeReenable) return;
        if (Vector3.SqrMagnitude(MainCamera.camera.transform.position - transform.position) < playRadius * playRadius)
        {
            if (!emitter.playing)
                emitter.Play();
        }
        else
        {
            emitter.Stop();
        }
    }

    public static void DisableForTime(float seconds)
    {
        if (_instance == null)
        {
            return;
        }

        _instance._timeReenable = Time.time + seconds;
        _instance.emitter.Stop();
    }
}