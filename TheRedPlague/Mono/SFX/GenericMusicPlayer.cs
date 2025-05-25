using UnityEngine;

namespace TheRedPlague.Mono.SFX;

public class GenericMusicPlayer : MonoBehaviour
{
    public float startRange;
    public float endRange;
    public FMOD_CustomLoopingEmitter emitter;
    
    private void Start()
    {
        InvokeRepeating(nameof(UpdatePlayState), Random.value, 0.5f);
    }

    private void UpdatePlayState()
    {
        var distance = Vector3.Distance(MainCamera.camera.transform.position, transform.position);
        if (emitter.playing)
        {
            if (distance > endRange)
            {
                emitter.Stop();
                return;
            }
        }
        if (distance < startRange)
        {
            emitter.Play();
        }
    }
}