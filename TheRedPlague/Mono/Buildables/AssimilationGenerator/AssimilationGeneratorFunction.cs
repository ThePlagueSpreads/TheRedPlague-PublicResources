using System.Diagnostics.CodeAnalysis;
using Nautilus.Utility;
using TheRedPlague.Mono.VFX;
using UnityEngine;

namespace TheRedPlague.Mono.Buildables.AssimilationGenerator;

public class AssimilationGeneratorFunction : MonoBehaviour, IHandTarget, IConstructable
{
    private static readonly bool Debug = false;
    
    public PowerSource powerSource;
    public PowerRelay relay;

    public Constructable constructable;

    public Animator animator;
    public AnimateMaterialProperty glowAnimator;
    public FMOD_CustomEmitter activateEmitter;
    public FMOD_CustomLoopingEmitter loopEmitter;

    private static readonly FMODAsset BiteSound = AudioUtils.GetFmodAsset("AssimilationGeneratorBite");

    public float glowStrength = 1f;

    public float powerPerSecond = 2f;

    public float minDurationPerFood = 30f;
    public float maxDurationPerFood = 195f;
    public float minExpectedBioreactorCharge = 100f;
    public float maxExpectedBioreactorCharge = 1000f;
    
    public float maxReserveTime = 600;

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
        var duration = GetChargeDuration(bioreactorCharge);
        if (Debug)
            Plugin.Logger.LogInfo("Consuming " + item.name + ": " + duration + " seconds");
        if (_active)
        {
            _timeDeactivate = Mathf.Min(_timeDeactivate + duration, Time.time + maxReserveTime);
        }
        else
        {
            _timeDeactivate = DayNightCycle.main.timePassedAsFloat + duration;
        }
        glowAnimator.SetTargetPropertyValue(glowStrength);
        Invoke(nameof(PlayLoopSound), 5);
        activateEmitter.Play();
        _active = true;
    }

    private float GetChargeDuration(float bioreactorCharge)
    {
        return Mathf.Lerp(minDurationPerFood, maxDurationPerFood,
            Mathf.InverseLerp(minExpectedBioreactorCharge, maxExpectedBioreactorCharge, bioreactorCharge));
    }

    private void PlayLoopSound()
    {
        loopEmitter.Play();
    }

    private void Update()
    {
        if (!_active) return;
        
        if (DayNightCycle.main.timePassedAsFloat > _timeDeactivate)
        {
            Deactivate();
            return;
        }
            
        relay.ModifyPower(powerPerSecond * DayNightCycle.main.deltaTime, out _);
    }

    private void Deactivate()
    {
        _active = false;

        glowAnimator.SetTargetPropertyValue(0);
        animator.SetBool(Working, false);
        animator.SetBool(Working, false);
        loopEmitter.Stop();
    }

    public void OnHandHover(GUIHand hand)
    {
        if (constructable.constructed)
        {
            HandReticle.main.SetText(HandReticle.TextType.Hand,
                Language.main.GetFormat("AssimilationGeneratorStatus",
                    Mathf.RoundToInt(powerSource.GetPower()),
                    Mathf.RoundToInt(powerSource.GetMaxPower())),
                false);
            HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
            HandReticle.main.SetIcon(HandReticle.IconType.Hand);
        }
    }

    public void OnHandClick(GUIHand hand)
    {
    }

    public bool IsDeconstructionObstacle()
    {
        return true;
    }

    public bool CanDeconstruct([UnscopedRef] out string reason)
    {
        reason = null;
        return true;
    }

    public void OnConstructedChanged(bool constructed)
    {
    }
}