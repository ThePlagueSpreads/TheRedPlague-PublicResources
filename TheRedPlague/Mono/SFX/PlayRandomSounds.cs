using UnityEngine;

namespace TheRedPlague.Mono.SFX;

public class PlayRandomSounds : MonoBehaviour
{
    public float minDelay;
    public float maxDelay;

    public FMOD_CustomEmitter emitter;

    private float _timePlayAgain;

    private void Update()
    {
        if (Time.time > _timePlayAgain)
        {
            emitter.Play();
            _timePlayAgain = Time.time + Random.Range(minDelay, maxDelay);
        }
    }
}