using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.Grabber;

public class GrabberVehicleTarget : GrabberTargetBase
{
    private const float GrabAnimDuration = 1;
    private const float LaunchVelocity = 14;

    public Vehicle vehicle;

    private Quaternion _vehicleInitialRotation;
    private Vector3 _vehicleInitialPosition;
    private float _timeVehicleGrabbed;

    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    
    public override void StartGrab(GrabberCreature grabber)
    {
        UWE.Utils.SetIsKinematicAndUpdateInterpolation(vehicle.GetComponent<Rigidbody>(), isKinematic: true);
        vehicle.collisionModel.SetActive(value: false);
        if (vehicle is Exosuit exoSuit)
        {
            SafeAnimator.SetBool(vehicle.mainAnimator, "reaper_attack", value: true);
            exoSuit.cinematicMode = true;
        }

        _vehicleInitialRotation = vehicle.transform.rotation;
        _vehicleInitialPosition = vehicle.transform.position;
        _timeVehicleGrabbed = Time.time;

        if (vehicle == Player.main.GetVehicle())
        {
            MainCameraControl.main.ShakeCamera(1f, 6, MainCameraControl.ShakeMode.BuildUp, 1.1f);
        }
    }

    public override void EndGrab(GrabberCreature grabber)
    {
        if (vehicle == null)
            return;
        
        if (vehicle is Exosuit exoSuit)
        {
            SafeAnimator.SetBool(vehicle.mainAnimator, "reaper_attack", value: false);
            exoSuit.cinematicMode = false;
        }

        var rb = vehicle.GetComponent<Rigidbody>();
        UWE.Utils.SetIsKinematicAndUpdateInterpolation(rb, isKinematic: false);
        vehicle.collisionModel.SetActive(value: true);
        
        rb.AddForce(grabber.grabTransform.forward * LaunchVelocity, ForceMode.VelocityChange);
    }

    public override void Damage(GrabberCreature grabber, float damage, DamageType type)
    {
        vehicle.liveMixin.TakeDamage(damage, grabber.transform.position, type, grabber.gameObject);
    }

    public override void SetTargetPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        _targetPosition = position;
        _targetRotation = rotation;
    }

    private void Update()
    {
        var graspPercent = Mathf.Clamp01((Time.time - _timeVehicleGrabbed) / GrabAnimDuration);
        if (graspPercent >= 1f)
        {
            transform.position = _targetPosition;
            transform.rotation = _targetRotation;
        }
        else
        {
            transform.position = (_targetPosition - _vehicleInitialPosition) * graspPercent + _vehicleInitialPosition;
            transform.rotation = Quaternion.Lerp(_vehicleInitialRotation, _targetRotation, graspPercent);
        }
    }
}