using UnityEngine;

namespace TheRedPlague.Mono.SFX;

public class WhispersOfTheDead : MonoBehaviour
{
    public float minDelay;
    public float maxDelay;
    public float maxDistance;

    public FMOD_CustomEmitter emitter;

    private float _timePlayAgain;

    private void Update()
    {
        if (Time.time < _timePlayAgain) return;
        if (Vector3.SqrMagnitude(MainCamera.camera.transform.position - transform.position) < maxDistance * maxDistance)
        {
            emitter.Play();
        }
        _timePlayAgain = Time.time + Random.Range(minDelay, maxDelay);
    }
}