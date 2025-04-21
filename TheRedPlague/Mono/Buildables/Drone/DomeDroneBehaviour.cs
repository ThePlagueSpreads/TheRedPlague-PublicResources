using UnityEngine;

namespace TheRedPlague.Mono.Buildables.Drone;

public class DomeDroneBehaviour : MonoBehaviour
{
    private static DomeDeconstructionManager _manager;

    public BoidBehaviour boidBehaviour;
    public Animator animator;
    public Texture deconstructVfxEmissiveTexture;

    public GameObject vfx;
    public FMOD_CustomLoopingEmitter deconstructSoundEmitter;
    
    private static readonly int OffsetAnimParam = Animator.StringToHash("offset");

    private readonly float _reevaluateBestTargetDelay = 0.5f;
    private float _timeReevaluateAgain;
    private DroneDeconstructionTarget _deconstructionTarget;

    private bool _deconstructing;
    private float _timeLastDeconstructed;

    // A unique ID ranging between 0 (inclusive) and the number of drones (exclusive)
    private int _uniqueId;

    private Vector3 _constructionReadyPosition;

    private bool IsGettingReadyForDomeConstruction => true;

    private void Start()
    {
        animator.SetFloat(OffsetAnimParam, Random.value);
        _uniqueId = boidBehaviour.RegisterIntoController(GetManager(this).boidController);
        _timeReevaluateAgain = Time.time + Random.value;
        _constructionReadyPosition = GetConstructionPositionForDrone(_uniqueId);
    }

    private static DomeDeconstructionManager GetManager(DomeDroneBehaviour drone)
    {
        if (_manager == null)
        {
            var controller = new GameObject("DroneBoidController").AddComponent<BoidController>();
            controller.neighborDist = 9;
            controller.velocity = 15;
            controller.rotationCoeff = 8;
            _manager = controller.gameObject.AddComponent<DomeDeconstructionManager>();
            _manager.boidController = controller;
            _manager.deconstructVfxEmissiveTexture = drone.deconstructVfxEmissiveTexture;
        }

        return _manager;
    }

    private void Update()
    {
        if (_deconstructing)
        {
            if (Time.time > _timeLastDeconstructed + 0.3f)
            {
                _deconstructing = false;
                vfx.SetActive(false);
                deconstructSoundEmitter.Stop();
            }
        }
        
        if (_manager != null && _manager.Active)
        {
            boidBehaviour.enabled = true;
            if (_deconstructionTarget == null || Time.time > _timeReevaluateAgain)
            {
                _timeReevaluateAgain = Time.time + _reevaluateBestTargetDelay;
                _deconstructionTarget = GetBestDeconstructTarget();
            }

            if (_deconstructionTarget != null)
            {
                if (_deconstructionTarget.CanDeconstruct())
                {
                    _deconstructionTarget.DeconstructThisFrame();
                    _manager.ContributeProgress();
                    if (!_deconstructing)
                    {
                        _deconstructing = true;
                        vfx.SetActive(true);
                        deconstructSoundEmitter.Play();
                    }

                    vfx.transform.up = -(_deconstructionTarget.transform.position - transform.position).normalized;
                    _timeLastDeconstructed = Time.time;
                }
            }
        } 
        else if (IsGettingReadyForDomeConstruction)
        {
            boidBehaviour.enabled = false;
            var controller = boidBehaviour.GetController();
            var directionToPoint = _constructionReadyPosition - transform.position;
            var distToPoint = Vector3.Magnitude(directionToPoint);
            if (distToPoint > 3f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, 
                    Quaternion.LookRotation(directionToPoint), Time.deltaTime * 60);
            }
            transform.position += transform.forward * (controller.velocity * Time.deltaTime);
        }
    }

    private DroneDeconstructionTarget GetBestDeconstructTarget()
    {
        DroneDeconstructionTarget best = null;
        var closest = DroneDeconstructionTarget.MaxDeconstructRange * DroneDeconstructionTarget.MaxDeconstructRange;
        foreach (var deconstructTarget in _manager.GetDeconstructionTargets())
        {
            if (deconstructTarget == null) continue;
            if (!deconstructTarget.CanDeconstruct()) continue;
            
            foreach (var deconstructPoint in deconstructTarget.deconstructTransforms)
            {
                var distance = Vector3.SqrMagnitude(deconstructPoint.transform.position - transform.position);
                if (distance > deconstructTarget.deconstructRange * deconstructTarget.deconstructRange) continue;
                if (distance > closest) continue;
                best = deconstructTarget;
                closest = distance;
            }
        }

        return best;
    }

    private static Vector3 GetConstructionPositionForDrone(int id)
    {
        var angle = Mathf.PI * 2f * id / 16;
        const float distance = 2000f;
        return new Vector3(Mathf.Cos(angle) * distance, 15, Mathf.Sin(angle) * distance);
    }
}