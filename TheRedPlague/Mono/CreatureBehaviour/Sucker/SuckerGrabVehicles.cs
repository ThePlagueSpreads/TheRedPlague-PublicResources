﻿using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheRedPlague.Mono.CreatureBehaviour.Sucker;

public class SuckerGrabVehicles : MonoBehaviour, IOnTakeDamage
{
    public float grabCooldown = 1.6f;
    public float changeDirectionMinDelay = 0.8f;
    public float changeDirectionMaxDelay = 3f;
    public float vehicleAcceleration = 5f;
    public float exosuitAcceleration = 10f;
    public float vehicleRotationSpeed = 15f;
    public float chanceToMovePerSecond = 0.7f;

    public Collider mainCollider;
    public Rigidbody rigidbody;
    public LiveMixin liveMixin;
    public Animator animator;

    private Vehicle _currentVehicle;
    private SuckerControllerTarget _target;

    private float _timeCanGrabAgain;
    private float _timeChangeDirectionAgain;
    private float _timeDecideToMoveAgain;
    private Quaternion _targetRotation;
    private bool _acceleratingVehicle;
    private bool _grabbingExosuit;

    public bool isPrimaryControllerOfTarget;
    private static readonly int GrabParamId = Animator.StringToHash("latch");

    private bool Grabbing => _currentVehicle != null;

    private void OnCollisionEnter(Collision other)
    {
        if (Time.time < _timeCanGrabAgain) return;
        if (Grabbing) return;
        if (!liveMixin.IsAlive()) return;

        var vehicle = other.gameObject.GetComponent<Vehicle>();
        if (vehicle != null)
        {
            GrabOntoVehicle(vehicle);
        }
    }

    private void GrabOntoVehicle(Vehicle vehicle)
    {
        transform.parent = vehicle.transform;
        _currentVehicle = vehicle;
        SetCollisionsIgnore(_currentVehicle, false);
        rigidbody.isKinematic = true;
        LargeWorldStreamer.main.cellManager.UnregisterEntity(gameObject);
        transform.LookAt(_currentVehicle.transform);
        if (Physics.Raycast(transform.position, transform.forward, out var hit, 3, -1, QueryTriggerInteraction.Ignore))
            transform.position = hit.point;
        else
            transform.position += transform.forward * 0.5f;

        _target = vehicle.GetComponent<SuckerControllerTarget>();
        if (_target != null)
        {
            _target.AttachSucker(this);
        }
        else
        {
            isPrimaryControllerOfTarget = true;
        }

        animator.SetBool(GrabParamId, true);
        _grabbingExosuit = vehicle is Exosuit;
        if (_grabbingExosuit)
        {
            try
            {
                var newParent = vehicle.transform.Find("exosuit_01/root/geoChildren");
                transform.parent = newParent;
            }
            catch (Exception e)
            {
                Plugin.Logger.LogWarning("Unexpected transform hierarchy in Exosuit: " + e);
            }
        }
    }

    private void ReleaseFromVehicle()
    {
        if (!Grabbing) return;
        SetCollisionsIgnore(_currentVehicle, false);
        rigidbody.isKinematic = false;
        transform.parent = null;
        LargeWorldStreamer.main.cellManager.RegisterEntity(gameObject);
        _timeCanGrabAgain = Time.time + grabCooldown;
        _currentVehicle = null;
        if (_target != null)
        {
            _target.RemoveSucker(this);
        }

        _target = null;
        animator.SetBool(GrabParamId, false);
    }

    private void OnEnable()
    {
        if (_target != null)
        {
            _target.AttachSucker(this);
        }
    }

    private void OnDisable()
    {
        if (_target != null)
        {
            _target.RemoveSucker(this);
        }
    }

    private void SetCollisionsIgnore(Vehicle vehicle, bool ignore)
    {
        foreach (var collider in vehicle.collisionModel.GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(mainCollider, collider, ignore);
        }
    }

    public void OnTakeDamage(DamageInfo damageInfo)
    {
        if (damageInfo.damage > 10)
        {
            if (damageInfo.dealer != null && damageInfo.dealer.GetComponent<Vehicle>()) return;
            ReleaseFromVehicle();
        }
    }

    private void FixedUpdate()
    {
        if (!Grabbing) return;
        if (!isPrimaryControllerOfTarget) return;

        var vehicleRb = _currentVehicle.useRigidbody;

        if (_acceleratingVehicle && Ocean.GetDepthOf(gameObject) > 0)
            vehicleRb.AddRelativeForce(Vector3.forward * (_grabbingExosuit ? exosuitAcceleration : vehicleAcceleration),
                ForceMode.Acceleration);

        if (Time.time > _timeChangeDirectionAgain)
        {
            _timeChangeDirectionAgain = Time.time + Random.Range(changeDirectionMinDelay, changeDirectionMaxDelay);
            _targetRotation = GetRandomRotation();
        }

        if (Time.time > _timeDecideToMoveAgain)
        {
            _acceleratingVehicle = Random.value <= chanceToMovePerSecond;
            _timeDecideToMoveAgain = Time.time + 1;
        }
    }

    private Quaternion GetRandomRotation()
    {
        var direction = Random.onUnitSphere;
        if (_grabbingExosuit)
        {
            direction.y = 0;

            // Fix in case we were super unlucky and got an invalid rotation (if it was looking UP or DOWN before)
            if (direction.sqrMagnitude < 0.001f)
                direction = Vector3.forward;
        }

        return Quaternion.LookRotation(direction);
    }

    private void Update()
    {
        if (Grabbing)
            _currentVehicle.useRigidbody.MoveRotation(Quaternion.RotateTowards(_currentVehicle.useRigidbody.rotation,
                _targetRotation, vehicleRotationSpeed * Time.deltaTime));
    }

    public void OnKill()
    {
        ReleaseFromVehicle();
    }
}