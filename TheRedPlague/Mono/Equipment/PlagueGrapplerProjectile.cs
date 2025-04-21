using Nautilus.Utility;
using TheRedPlague.Mono.StoryContent;
using TheRedPlague.Utilities;
using UnityEngine;

namespace TheRedPlague.Mono.Equipment;

public class PlagueGrapplerProjectile : MonoBehaviour
{
    private PlagueGrapplerTool _tool;

    private const float MaxPullMass = 1700;
    private const float MinLifetime = 0.1f;
    private const float MaxLifetime = 300;
    private const float LaunchVelocity = 40;
    private const float ReelInMaxForce = 90;
    private const float ReelInMinForce = 20;
    private const float ReelInMinMass = 200;
    private const float ReelInSnapDistance = 1.4f;
    private const float ManipulateModeSnapDistance = 2f;
    private const float MinSizeForLargeGrabAnim = 1.2f;
    private const float PlayerPullAcceleration = 100f;
    private const float GrabDelay = 0.02f;
    private const float MinVelocity = 0.3f;
    private const float CancelVelocity = 21;
    private const float MaxLooseTime = 6;
    private const float MaxDragAccelerationMagnitude = 100f;
    private const float MaxDistance = 100f;
    private const float AboveWaterAccelerationMultiplier = 0.6f;
    private const float MaintainAccelerationDuration = 0.7f;
    private const float TensionAcceleration = 30;

    private const float MinMassForMirrorForce = 1000f;
    private const float MaxMassForMirrorForce = 5000f;
    private const float MinMirrorForcePercent = 0.03f;
    private const float MaxMirrorForcePercent = 0.25f;

    private bool _manipulateMode;
    private bool _reelingIn;
    private Transform _lineAttachPoint;
    private Rigidbody _rb;
    private Collider _collider;
    private Animator _animator;
    private Rigidbody _grabbedRb;
    private bool _grabbing;
    private float _spawnTime;
    private bool _recalling;
    private Pickupable _grabbedPickupable;
    private Vector3 _lastFrameVelocity;
    private bool _playedDestroyVfx;
    private Vector3 _lastPlayerAcceleration;
    private float _lastTimePlayerAccelerated;
    private float _temporaryLengthLimit = -1f;
    private bool _grabbedOntoPlagueCave;
    private PlagueCaveHatch _caveHatch;
    private bool _busyOpeningPlagueCave;

    private bool _launched;
    private static readonly int GrabAnim = Animator.StringToHash("grab");
    private static readonly int LargeGrabAnim = Animator.StringToHash("large");
    
    private static readonly FMODAsset FireSound = AudioUtils.GetFmodAsset("PlagueGrapplerFire");
    private static readonly FMODAsset TraverseSound = AudioUtils.GetFmodAsset("PlagueGrapplerTraverse");
    private static readonly FMODAsset GrabSound = AudioUtils.GetFmodAsset("PlagueGrapplerGrab");
    private static readonly FMODAsset CollideSound = AudioUtils.GetFmodAsset("PlagueGrapplerCollide");

    public void Launch(PlagueGrapplerTool tool)
    {
        if (_launched)
        {
            Plugin.Logger.LogWarning("Plague grappler projectile was already launched! Cannot launch twice.");
        }

        _tool = tool;
        _lineAttachPoint = transform.Find("PlagueGrapplerProjectileArmature/LineAttachPoint");
        UpdateLinePositions();
        _collider = gameObject.GetComponent<Collider>();
        _collider.enabled = true;
        _animator = GetComponent<Animator>();
        _rb = gameObject.AddComponent<Rigidbody>();
        _rb.mass = 100;
        _rb.useGravity = false;
        _rb.angularDrag = 1f;
        var wf = gameObject.AddComponent<WorldForces>();
        wf.useRigidbody = _rb;
        wf.underwaterGravity = 0f;
        wf.underwaterDrag = 0.05f;
        wf.lockInterpolation = true;
        wf.aboveWaterGravity = 9.8f;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.AddForce(transform.forward * LaunchVelocity, ForceMode.VelocityChange);
        Destroy(gameObject, MaxLifetime);
        _launched = true;
        _spawnTime = Time.time;
        IgnoreCollisionsWithPlayer();
        PlaySound(FireSound);
    }

    private void IgnoreCollisionsWithPlayer()
    {
        var playerColliders = Player.main.GetComponentsInChildren<Collider>();
        foreach (var collider in playerColliders)
        {
            Physics.IgnoreCollision(_collider, collider);
        }
    }

    private void Update()
    {
        UpdateLinePositions();

        if (_recalling)
        {
            var positionDifference = _tool.transform.position - transform.position;
            var directionToTool = positionDifference.normalized;
            transform.position += directionToTool * (Time.deltaTime * CancelVelocity);
            transform.forward = -directionToTool;
            TryDestroy(positionDifference);
        }
    }

    private void FixedUpdate()
    {
        if (_tool == null)
        {
            Destroy(gameObject);
            return;
        }

        _lastPlayerAcceleration = Vector3.zero;

        if (Time.time > _spawnTime + MinLifetime && !_recalling && _rb && !_rb.isKinematic &&
            (_rb.velocity.sqrMagnitude < MinVelocity * MinVelocity || Time.time > _spawnTime + MaxLooseTime ||
             (transform.position - _tool.transform.position).sqrMagnitude > MaxDistance * MaxDistance))
        {
            Recall();
        }

        if (_recalling)
        {
            return;
        }

        var positionDifference = _tool.transform.position - transform.position;
        var directionToTool = positionDifference.normalized;
        var reelInForce = GetReelInForce();

        AddPlayerDragMovement(Time.fixedDeltaTime);

        if (_temporaryLengthLimit > Mathf.Epsilon &&
            positionDifference.sqrMagnitude > _temporaryLengthLimit * _temporaryLengthLimit)
        {
            AcceleratePlayer(directionToTool * -TensionAcceleration);
        }

        // The primary action occurs when reeling
        if (!_reelingIn) return;

        if (_grabbing)
        {
            if (_manipulateMode)
            {
                if (_grabbedRb == null)
                {
                    Detach();
                    return;
                }

                _grabbedRb.AddForceAtPosition(directionToTool * reelInForce, transform.position,
                    ForceMode.Acceleration);
                var mirrorForce = GetMirrorForcePercent();
                if (mirrorForce > Mathf.Epsilon)
                {
                    AcceleratePlayer(directionToTool * (-mirrorForce * reelInForce));
                }
            }
            else
            {
                AcceleratePlayer(directionToTool * -PlayerPullAcceleration);
            }

            TryDestroy(positionDifference);
        }
        else
        {
            if (TryDestroy(positionDifference))
            {
                return;
            }

            if (_rb)
                _rb.AddForce(directionToTool * reelInForce, ForceMode.Acceleration);
        }
    }

    private void AddPlayerDragMovement(float deltaTime)
    {
        if (!_grabbedRb || _reelingIn)
        {
            _lastFrameVelocity = default;
            return;
        }

        var velocity = _grabbedRb.velocity;
        var acceleration = (velocity - _lastFrameVelocity) / deltaTime;
        acceleration = Vector3.ClampMagnitude(acceleration, MaxDragAccelerationMagnitude);
        AcceleratePlayer(acceleration);

        _lastFrameVelocity = velocity;
    }

    private bool TryDestroy(Vector3 positionDifference)
    {
        var distanceThreshold = ReelInSnapDistance * ReelInSnapDistance;
        if (_manipulateMode)
        {
            distanceThreshold = ManipulateModeSnapDistance * ManipulateModeSnapDistance;
        }

        if (positionDifference.sqrMagnitude < distanceThreshold)
        {
            Destroy(gameObject);
            if (_manipulateMode)
            {
                if (_grabbedPickupable && _grabbedPickupable.AllowedToPickUp())
                {
                    Inventory.main.container.UnsafeAdd(new InventoryItem(_grabbedPickupable));
                    _grabbedPickupable.Pickup();
                }

                if (_grabbedRb && _grabbedRb.TryGetComponent<BreakableResource>(out var breakable))
                {
                    breakable.BreakIntoResources();
                }
            }

            return true;
        }

        return false;
    }

    public void Recall()
    {
        _recalling = true;
        _grabbedRb = null;
        _manipulateMode = false;
        _collider.enabled = false;
        _grabbedPickupable = null;
        RemovePhysics();
    }

    public bool UnableToGrabTarget()
    {
        return _grabbing && !_manipulateMode;
    }

    public void CancelReel()
    {
        if (!_reelingIn) return;
        _reelingIn = false;
        SetCurrentLengthLimit();
    }

    private void OnDisable()
    {
        if (_tool)
            _tool.OnProjectileDestroyed();
        if (!_playedDestroyVfx && (_tool == null || !_tool.isActiveAndEnabled))
            SpawnDestroyVfx();
        if (Time.time < _lastTimePlayerAccelerated + 0.2f)
        {
            PreservePlayerVelocity();
        }
    }

    private void PreservePlayerVelocity()
    {
        var accel = Player.main.gameObject.AddComponent<Util.AcceleratePlayer>();
        accel.force = _lastPlayerAcceleration;
        accel.duration = MaintainAccelerationDuration;
    }

    public void SpawnDestroyVfx()
    {
        if (_playedDestroyVfx) return;
        var vfx = Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("DrifterCannonProjectileShatterPrefab"),
            transform.position, Quaternion.identity);
        MaterialUtils.ApplySNShaders(vfx);
        vfx.AddComponent<SkyApplier>().renderers = new Renderer[] { vfx.GetComponent<Renderer>() };
        _playedDestroyVfx = true;
    }

    public bool ReelIn(bool tryGrab)
    {
        if (_reelingIn) return false;
        if (tryGrab && !_manipulateMode) return false;
        if (!tryGrab) _manipulateMode = false;
        _reelingIn = true;
        PlaySound(_manipulateMode ? GrabSound : TraverseSound);
        return true;
    }

    public bool IsReelingIn()
    {
        return _reelingIn;
    }
    
    public bool HasGrabbed()
    {
        return _grabbing;
    }

    public bool GrabbedOntoPlagueCave()
    {
        return _grabbedOntoPlagueCave;
    }

    public void OpenPlagueCave()
    {
        if (_busyOpeningPlagueCave)
            return;
        
        if (_caveHatch)
        {
            _caveHatch.entrance.Open();
            _busyOpeningPlagueCave = true;
            Invoke(nameof(OnDoneOpeningPlagueCave), 12);
        }
        else
        {
            Plugin.Logger.LogWarning("Cave hatch reference is missing!");
        }
    }

    private void OnDoneOpeningPlagueCave()
    {
        _busyOpeningPlagueCave = false;
        Destroy(gameObject);
        SpawnDestroyVfx();
    }

    public bool IsMovingObject()
    {
        return _manipulateMode && _grabbedRb != null;
    }

    private void RemovePhysics()
    {
        Destroy(_rb);
        Destroy(GetComponent<WorldForces>());
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!CanGrab())
            return;

        // Plague cave logic
        if (CheckCollisionForPlagueCaveHatch(other, out var hatch))
        {
            GrabOnto(hatch.gameObject);
            _grabbedOntoPlagueCave = true;
            _caveHatch = hatch;
        }
        // Normal grab logic
        else
        {
            var root = GenericTrpUtils.GetTargetRoot(other.collider, false, true);
            if (!IsValidTarget(root))
                return;

            GrabOnto(root);
        }

        Vector3 direction = default;
        for (var i = 0; i < other.contactCount; i++)
        {
            direction += other.contacts[i].normal;
        }

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.forward = -direction.normalized;
        }
        
        PlaySound(CollideSound);
    }

    private bool CanGrab()
    {
        return !_recalling && !_grabbing && !_reelingIn && Time.time > _spawnTime + GrabDelay;
    }

    private bool IsValidTarget(GameObject root)
    {
        if (root == Player.main.gameObject)
            return false;
        return true;
    }

    private float GetReelInForce()
    {
        if (_recalling)
        {
            return ReelInMaxForce;
        }

        if (!_manipulateMode || _grabbedRb == null)
        {
            return ReelInMaxForce;
        }

        return Mathf.Lerp(ReelInMaxForce, ReelInMinForce,
            Mathf.InverseLerp(ReelInMinMass, MaxPullMass, _grabbedRb.mass));
    }

    private float GetMirrorForcePercent()
    {
        if (_grabbedRb.mass <= MinMassForMirrorForce)
            return 0f;
        return Mathf.Lerp(MaxMirrorForcePercent, MinMirrorForcePercent,
            Mathf.InverseLerp(MinMassForMirrorForce, MaxMassForMirrorForce, _grabbedRb.mass));
    }

    private void GrabOnto(GameObject target)
    {
        var grabMode = GetPreferredGrabMode(target);
        if (grabMode == GrabMode.None)
        {
            return;
        }

        UpdateBiteAnim(target);
        RemovePhysics();
        _collider.enabled = false;
        transform.SetParent(target.transform, true);
        _grabbedRb = target.GetComponent<Rigidbody>();
        if (grabMode == GrabMode.MoveObject)
        {
            _grabbedRb.isKinematic = false;
            _grabbedPickupable = target.GetComponent<Pickupable>();
        }

        _rb.interpolation = RigidbodyInterpolation.None;

        _manipulateMode = grabMode == GrabMode.MoveObject;

        _grabbing = true;

        SetCurrentLengthLimit();
    }

    private void SetCurrentLengthLimit()
    {
        _temporaryLengthLimit = Vector3.Distance(_tool.transform.position, transform.position) + 1f;
    }

    private void Detach()
    {
        _grabbing = false;
    }

    private void AcceleratePlayer(Vector3 force)
    {
        switch (Player.main.motorMode)
        {
            case Player.MotorMode.Walk:
            case Player.MotorMode.Run:
                Player.main.groundMotor.SetVelocity(((IGroundMoveable)Player.main.groundMotor).GetVelocity() +
                                                    force * (Time.deltaTime * AboveWaterAccelerationMultiplier));
                if (Player.main.IsUnderwaterForSwimming())
                    Player.main.rigidBody.AddForce(force, ForceMode.Acceleration);
                break;
            case Player.MotorMode.Dive:
            case Player.MotorMode.Seaglide:
                Player.main.rigidBody.AddForce(force, ForceMode.Acceleration);
                break;
            default:
                Plugin.Logger.LogWarning("Unexpected motor mode: " + Player.main.motorMode);
                break;
        }

        _lastPlayerAcceleration += force;
        _lastTimePlayerAccelerated = Time.time;
    }

    private bool CheckCollisionForPlagueCaveHatch(Collision collision, out PlagueCaveHatch hatch)
    {
        if (!collision.rigidbody)
        {
            hatch = null;
            return false;
        }
        return collision.rigidbody.TryGetComponent(out hatch) && !hatch.entrance.GetIsOpen();
    }

    private void UpdateBiteAnim(GameObject target)
    {
        var size = GenericTrpUtils.GetObjectBounds(target).size;
        var averageSize = (size.x + size.y + size.z) / 3;
        bool isLarge = averageSize > MinSizeForLargeGrabAnim;
        _animator.SetBool(GrabAnim, true);
        _animator.SetBool(LargeGrabAnim, isLarge);
    }

    private void UpdateLinePositions()
    {
        _tool.line.SetPosition(0, _tool.lineAttachPoint.position);
        _tool.line.SetPosition(1, _lineAttachPoint.position);
    }

    private void PlaySound(FMODAsset sound)
    {
        Utils.PlayFMODAsset(sound, transform.position);
    }

    private GrabMode GetPreferredGrabMode(GameObject root)
    {
        var rb = root.GetComponent<Rigidbody>();
        if (rb == null)
            return GrabMode.MovePlayer;

        if (rb.mass > MaxPullMass)
            return GrabMode.MovePlayer;

        if (rb.isKinematic && root.GetComponent<WorldForces>() == null)
            return GrabMode.MovePlayer;

        var creature = root.GetComponent<Creature>();
        if (creature == null)
        {
            var propulsionCannonTarget = root.GetComponent<IPropulsionCannonAmmo>();
            if (propulsionCannonTarget != null && !propulsionCannonTarget.GetAllowedToGrab())
                return GrabMode.MovePlayer;
        }

        return GrabMode.MoveObject;
    }

    private enum GrabMode
    {
        None,
        MovePlayer,
        MoveObject
    }
}