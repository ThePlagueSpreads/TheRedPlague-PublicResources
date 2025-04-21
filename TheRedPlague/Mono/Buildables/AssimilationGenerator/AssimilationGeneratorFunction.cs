using Nautilus.Utility;
using TheRedPlague.Mono.VFX;
using UnityEngine;

namespace TheRedPlague.Mono.Buildables.AssimilationGenerator;

public class AssimilationGeneratorFunction : MonoBehaviour
{
    public Constructable constructable;

    public Animator animator;
    public AnimateMaterialProperty glowAnimator;
    public FMOD_CustomEmitter activateEmitter;
    public FMOD_CustomLoopingEmitter loopEmitter;

    private static readonly FMODAsset BiteSound = AudioUtils.GetFmodAsset("AssimilationGeneratorBite");

    public float glowStrength = 1f;

    private bool _active;
    private float _timeDeactivate;
    private static readonly int Gobble = Animator.StringToHash("gobble");
    private static readonly int Working = Animator.StringToHash("working");

    public void EatItem(GameObject item, float bioreactorCharge)
    {
        var locomotion = item.GetComponent<Locomotion>();
        if (locomotion) locomotion.maxAcceleration = 0;
        var rb = item.GetComponent<Rigidbody>();
        if (rb) rb.velocity = (transform.position - item.transform.position).normalized * 10;
        var creature = item.GetComponent<Creature>();
        if (creature) creature.liveMixin.TakeDamage(100000);
        Destroy(item, 10);
        item.AddComponent<AssimilatorFood>();
        animator.SetTrigger(Gobble);
        animator.SetBool(Working, true);
        Utils.PlayFMODAsset(BiteSound, transform.position + transform.up * 5);
        _timeDeactivate = Time.time + 60;
        glowAnimator.SetTargetPropertyValue(glowStrength);
        Invoke(nameof(PlayLoopSound), 5);
        activateEmitter.Play();
        _active = true;
    }

    private void PlayLoopSound()
    {
        loopEmitter.Play();
    }

    private void Update()
    {
        if (_active && Time.time > _timeDeactivate)
        {
            Deactivate();
        }
    }

    private void Deactivate()
    {
        _active = false;

        glowAnimator.SetTargetPropertyValue(0);
        animator.SetBool(Working, false);
        animator.SetBool(Working, false);
        loopEmitter.Stop();
    }
}