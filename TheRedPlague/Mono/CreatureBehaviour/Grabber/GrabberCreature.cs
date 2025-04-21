using System.Collections.Generic;
using Nautilus.Utility;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.Grabber;

public class GrabberCreature : MonoBehaviour
{
    public float revealDuration = 2.8f;
    public float attackDuration = 7.4f;
    public float grabCooldown = 8f;
    public float initialDamageInterval = 2f;
    public float damageInterval = 0.4f;
    public float damageValue = 5;
    public float revealAllowGrabDelay = 0.9f;

    public Animator animator;
    public Renderer renderer;
    public Transform pivotTransform;
    public Transform grabTransform;
    public Collider mainCollider;
    public GameObject[] triggers;
    public GameObject bloodPrefab;

    public FMOD_CustomLoopingEmitter grabEmitter;

    public GrabMode CurrentGrabMode { get; private set; } = GrabMode.Burrowed;
    
    private static List<GrabberCreature> _allGrabbers = new();

    private static bool _previewActive;
    
    private static readonly int RevealedAnimParam = Animator.StringToHash("revealed");
    private static readonly int AttackAnimParam = Animator.StringToHash("attack");
    private static readonly int PreviewAnimParam = Animator.StringToHash("EDITOR_PREVIEW");
    private static readonly int UpAnimatorLayer = 1;
    
    private GrabberTargetBase _grabTarget;
    private bool _grabbing;
    private float _timeCanGrabAgain;
    private float _timeTakeDamageAgain;
    private float _timeEndReveal;
    private float _timeAllowGrabAfterReveal;

    private static readonly FMODAsset FleshSound = AudioUtils.GetFmodAsset("GrabberFlesh");
    private static readonly FMODAsset GrowlSound = AudioUtils.GetFmodAsset("GrabberGrowl");

    private void Start()
    {
        mainCollider.enabled = false;
        SetTriggersActive(false);
        if (!_previewActive)
            renderer.enabled = false;
    }

    public bool IsTargetValid(GameObject target)
    {
        if (target == Player.main.gameObject)
            return true;
        var vehicle = target.GetComponent<Vehicle>();
        if (vehicle != null)
        {
            return vehicle.liveMixin.IsAlive() && !vehicle.docked && !vehicle.useRigidbody.isKinematic;
        }
        return false;
    }

    public bool CanGrab()
    {
        return !_grabbing && Time.time > _timeCanGrabAgain && Time.time > _timeAllowGrabAfterReveal;
    }

    public bool IsRevealed()
    {
        return Time.time < _timeEndReveal;
    }

    public void Reveal()
    {
        animator.SetBool(RevealedAnimParam, true);
        _timeEndReveal = Time.time + revealDuration;
        _timeAllowGrabAfterReveal = Time.time + revealAllowGrabDelay;
        Invoke(nameof(FinishRevealing), revealAllowGrabDelay);
        Invoke(nameof(Hide), revealDuration);
        mainCollider.enabled = true;
        renderer.enabled = true;
        PlaySound(GrowlSound);
        PlayBloodParticle();
    }

    public void OrientTowardsTarget(Transform target)
    {
        pivotTransform.LookAt(target);
        pivotTransform.localEulerAngles = new Vector3(0, pivotTransform.localEulerAngles.y, 0);
        CurrentGrabMode = GetGrabMode(target);
        animator.SetLayerWeight(UpAnimatorLayer, CurrentGrabMode == GrabMode.Above ? 1 : 0);
    }

    public void GrabTarget(GameObject target)
    {
        var grabbed = false;
        if (target.TryGetComponent<Vehicle>(out var vehicle))
        {
            var grabVehicle = target.AddComponent<GrabberVehicleTarget>();
            grabVehicle.vehicle = vehicle;
            _grabTarget = grabVehicle;
            grabbed = true;
        }

        if (grabbed)
        {
            BeginGrab();
        }
        else
        {
            Plugin.Logger.LogWarning("GrabberCreature: Invalid target passed to GrabTarget!");
        }
    }

    // expects grab target to exist
    private void BeginGrab()
    {
        animator.SetBool(AttackAnimParam, true);
        _grabbing = true;
        _timeCanGrabAgain = Time.time + attackDuration + grabCooldown;
        _grabTarget.StartGrab(this);
        _timeTakeDamageAgain = Time.time + initialDamageInterval;
        _timeEndReveal = Time.time + attackDuration + 1f;
        grabEmitter.Play();
        
        Invoke(nameof(EndGrab), attackDuration);
    }

    // the GrabTarget can be null when this is called so be careful
    private void EndGrab()
    {
        animator.SetBool(AttackAnimParam, false);
        if (_grabTarget)
            _grabTarget.EndGrab(this);
        _grabbing = false;
        _timeCanGrabAgain = Time.time + grabCooldown;
        grabEmitter.Stop();
        
        Destroy(_grabTarget);
        _grabTarget = null;
        Hide();
    }

    private void SetTriggersActive(bool active)
    {
        foreach (var trigger in triggers)
        {
            trigger.SetActive(active);
        }
    }

    private GrabMode GetGrabMode(Transform target)
    {
        return Vector3.Dot(transform.up, (target.position - transform.position).normalized) > 0.75f ? GrabMode.Above : GrabMode.InFront;
    }

    private void FinishRevealing()
    {
        SetTriggersActive(true);
        PlaySound(FleshSound);
    }
    
    private void Hide()
    {
        if (_grabbing)
            return;
        
        animator.SetBool(RevealedAnimParam, false);
        CurrentGrabMode = GrabMode.Burrowed;
        mainCollider.enabled = false;
        Invoke(nameof(FinishHiding), 5f);
        PlaySound(FleshSound);
        PlayBloodParticle();
    }

    private void FinishHiding()
    {
        SetTriggersActive(false);
        renderer.enabled = false;
    }

    private void Update()
    {
        if (!_grabbing)
            return;
        
        if (_grabTarget == null)
        {
            EndGrab();
            return;
        }
        
        _grabTarget.SetTargetPositionAndRotation(grabTransform.position, grabTransform.rotation);

        if (Time.time > _timeTakeDamageAgain)
        {
            _grabTarget.Damage(this, damageValue, DamageType.Normal);
            _timeTakeDamageAgain = Time.time + damageInterval;
        }
    }

    private void OnEnable()
    {
        _allGrabbers.Add(this);
        UpdatePreviewDisplay();
    }

    private void OnDisable()
    {
        _allGrabbers.Remove(this);
        if (_grabTarget != null)
        {
            EndGrab();
        }
    }

    private void UpdatePreviewDisplay()
    {
        animator.SetBool(PreviewAnimParam, _previewActive);
        if (_previewActive)
            renderer.enabled = true;
    }

    private void PlaySound(FMODAsset sound)
    {
        Utils.PlayFMODAsset(sound, transform.position);
    }

    private void PlayBloodParticle()
    {
        if (bloodPrefab == null)
        {
            Plugin.Logger.LogError("Grabber bloodPrefab not found!");
            return;
        }
        var particle = Instantiate(bloodPrefab, transform.position, transform.rotation);
        particle.SetActive(true);
        Destroy(particle, 6);
    }

    public static void TogglePreviewMode()
    {
        _previewActive = !_previewActive;
        foreach (var grabber in _allGrabbers)
        {
            if (grabber)
                grabber.UpdatePreviewDisplay();
        }
    }
}