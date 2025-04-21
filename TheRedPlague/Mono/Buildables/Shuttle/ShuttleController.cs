using Nautilus.Utility;
using TheRedPlague.Mono.Util;
using UnityEngine;

namespace TheRedPlague.Mono.Buildables.Shuttle;

public class ShuttleController : MonoBehaviour
{
    public Rigidbody rb;
    public FakeRigidbody fakeRigidbody;
    public float accelerationFactor = 75f;
    public float leaveStationAcceleration = 20f;
    public float cancelVelocityForce = 30;
    public float landRotateSpeed = 10;
    public float rotateSpeed = 35;
    public float catchFallDistance = 10;
    public float catchFallForce = 12;
    public FMOD_CustomEmitter emitter;
    
    private ShuttlePath _path;
    private bool _hasPath;
    private int _currentPathIndex;
    private ShuttlePadBehavior _assignedPad;

    private bool _slowingDown;

    private Vector3 _targetDirection;
    private bool _rotate;
    private Quaternion _rotation;

    private bool _determinedLandingDirection;
    private Vector3 _landingDirection;

    private static readonly FMODAsset AscendSound = AudioUtils.GetFmodAsset("ShuttleAscend");
    private static readonly FMODAsset DescendSound = AudioUtils.GetFmodAsset("ShuttleDescend");
    private static readonly FMODAsset TakeOffSound = AudioUtils.GetFmodAsset("ShuttleTakeOff");
    
    public bool HasLanded { get; private set; }

    private int TargetPathIndex => _currentPathIndex + 1;
    
    public void SetPath(ShuttlePath path, ShuttlePadBehavior pad)
    {
        _path = path;
        _assignedPad = pad;
        _hasPath = true;
        _currentPathIndex = 0;
        transform.position = _path.Points[0].Position;
        transform.forward = _path.Points[0].DirectionForStartAndEnd;
        _rotation = transform.rotation;
        fakeRigidbody.enabled = true;
        rb.isKinematic = true;
        _determinedLandingDirection = false;
        
        HasLanded = false;
        
        if (_path.Points[0].Transition == ShuttlePath.TransitionType.Ground)
        {
            OnLeaveGround();
        }
    }

    private void Land()
    {
        if (_path != null && _path.Points[^1].DestroyWhenReached)
        {
            Destroy(gameObject);
        }
        _path = null;
        _hasPath = false;
        _rotate = false;
        fakeRigidbody.enabled = false;
        fakeRigidbody.Velocity = default;
        if (Vector3.Distance(transform.position, Player.main.transform.position) < 15)
        {
            MainCameraControl.main.ShakeCamera(0.4f, 2f);
        }

        transform.forward = _landingDirection;

        HasLanded = true;
    }

    private void OnDescend()
    {
        PlaySound(DescendSound);
    }
    
    private void OnLeaveGround()
    {
        PlaySound(AscendSound);
    }
    
    private void OnTakeOff()
    {
        PlaySound(TakeOffSound);
    }

    private void PlaySound(FMODAsset sound)
    {
        emitter.Stop();
        emitter.SetAsset(sound);
        emitter.Play();
    }

    private void Update()
    {
        if (_assignedPad == null)
        {
            enabled = false;
            fakeRigidbody.enabled = false;
            rb.isKinematic = false;
            rb.velocity = fakeRigidbody.Velocity;
            return;
        }
        
        if (!_hasPath) return;
        
        if (_rotate)
        {
            RotateTowardsDirection(_targetDirection);
            transform.rotation = _rotation;
        }
        
        var currentPoint = _path.Points[_currentPathIndex];
        var targetPoint = _path.Points[TargetPathIndex];
        
        var direction = targetPoint.Position - transform.position;
        var directionNormalized = direction.normalized;

        var sqrDistanceToNextPoint = Vector3.SqrMagnitude(targetPoint.Position - transform.position);

        // If completing the docking sequence...
        if (targetPoint.Transition == ShuttlePath.TransitionType.Ground && (sqrDistanceToNextPoint <= 0.25f
                || transform.position.y < targetPoint.Position.y))
        {
            if (sqrDistanceToNextPoint > 3 * 3)
            {
                transform.position = new Vector3(targetPoint.Position.x, transform.position.y, targetPoint.Position.z);
            }
            
            fakeRigidbody.enabled = false;
            
            if (sqrDistanceToNextPoint <= 0.0001f)
            {
                AdvanceToNextPoint();
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPoint.Position, Time.deltaTime);
            if (_determinedLandingDirection)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    Quaternion.LookRotation(_landingDirection), Time.deltaTime * 2000);
            }
        }
        // If moving to another point at all...
        else if (sqrDistanceToNextPoint > Mathf.Epsilon)
        {
            // If entering space...
            if (targetPoint.Transition == ShuttlePath.TransitionType.Space &&
                Vector3.SqrMagnitude(transform.position - targetPoint.Position) < 2500)
            {
                Destroy(gameObject);
                return;
            }
            
            // If transitioning towards another point or entering a port...
            if (_slowingDown || targetPoint.Transition == ShuttlePath.TransitionType.Ground ||
                sqrDistanceToNextPoint < fakeRigidbody.Velocity.sqrMagnitude * 4)
            {
                var cancelVelocity = (direction - fakeRigidbody.Velocity).normalized * cancelVelocityForce;
                // If landing...
                if (targetPoint.Transition == ShuttlePath.TransitionType.Ground)
                {
                    _rotate = true;
                    _targetDirection = GetLandingDirection(targetPoint.DirectionForStartAndEnd);
                    cancelVelocity.y = fakeRigidbody.Velocity.y > -2 ? 0 :
                        Mathf.Clamp01(catchFallDistance / Mathf.Sqrt(sqrDistanceToNextPoint)) * catchFallForce;
                }
                // If transitioning to a nearby point...
                else
                {
                    _rotate = false;
                }
                fakeRigidbody.AddForce(cancelVelocity);
                _slowingDown = true;
            }
            // If leaving a port...
            else if (currentPoint.Transition == ShuttlePath.TransitionType.Ground)
            {
                _rotate = false;
                var accelerationVector = direction.normalized * leaveStationAcceleration;
                fakeRigidbody.AddForce(accelerationVector);
            }
            // If traveling normally...
            else
            {
                _rotate = true;
                _targetDirection = directionNormalized;
                
                fakeRigidbody.AddForce(transform.forward * accelerationFactor);
            }
        }
        
        // If close to the next point, advance
        if (targetPoint.Transition != ShuttlePath.TransitionType.Ground && sqrDistanceToNextPoint <= 200)
        {
            AdvanceToNextPoint();
        }
    }

    private Vector3 GetLandingDirection(Vector3 defaultLandingDirection)
    {
        if (_determinedLandingDirection)
        {
            return _landingDirection;
        }

        var direction = defaultLandingDirection;
        var dot = Vector3.Dot(transform.forward, defaultLandingDirection);
        if (dot < 0) direction = -defaultLandingDirection;
        _landingDirection = direction;
        _determinedLandingDirection = true;
        return direction;
    }

    private void RotateTowardsDirection(Vector3 direction)
    {
        _rotation = Quaternion.RotateTowards(_rotation, Quaternion.LookRotation(direction),
            (_path.Points[TargetPathIndex].Transition == ShuttlePath.TransitionType.Ground
            ? landRotateSpeed : rotateSpeed) * Time.deltaTime);
    }

    private void AdvanceToNextPoint()
    {
        _currentPathIndex++;
        _slowingDown = false;
        
        if (TargetPathIndex >= _path.Points.Count)
        {
            Land();
            return;
        }
        
        if (_currentPathIndex >= 1 && _path.Points[_currentPathIndex - 1].Transition == ShuttlePath.TransitionType.Ground
            && _path.Points[_currentPathIndex].Transition == ShuttlePath.TransitionType.Default)
        {
            OnTakeOff();
        } 
        else if (_path.Points[TargetPathIndex].Transition == ShuttlePath.TransitionType.Ground)
        {
            Invoke(nameof(OnDescend), 4);
        }
        
        fakeRigidbody.enabled = true;
    }
}