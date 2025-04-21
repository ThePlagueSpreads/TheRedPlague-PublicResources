using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.Stabby;

public class StabbyTrigger : MonoBehaviour
{
    public StabbyMotion motion;
    public float cooldown = 1f;

    private float _timeCanStabAgain;
    
    private void OnTriggerEnter(Collider collider)
    {
        if (motion.Stabbing) return;
        if (Time.time < _timeCanStabAgain) return;
        _timeCanStabAgain = Time.time + cooldown;
        motion.StabTarget();
    }
}