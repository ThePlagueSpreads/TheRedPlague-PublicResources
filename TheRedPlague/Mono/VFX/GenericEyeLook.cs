using UnityEngine;

namespace TheRedPlague.Mono.VFX;

public class GenericEyeLook : MonoBehaviour, IManagedLateUpdateBehaviour
{
    public float degreesPerSecond = 180;

    public bool useLimits;

    public float dotLimit;
    
    private Transform _eyeLookDummy;

    private Vector3 _defaultLocalUp;

    private void OnEnable()
    {
        CreateDummy();
        if (_defaultLocalUp.sqrMagnitude < 1 && transform.parent != null)
        {
            _defaultLocalUp = transform.parent.InverseTransformDirection(transform.up);
        }
        BehaviourUpdateUtils.Register(this);
    }
    
    private void CreateDummy()
    {
        if (_eyeLookDummy)
            return;
        _eyeLookDummy = new GameObject("EyeLookDummy").transform;
    }
    
    private void OnDisable()
    {
        if (_eyeLookDummy) Destroy(_eyeLookDummy.gameObject);
        BehaviourUpdateUtils.Deregister(this);
    }

    public string GetProfileTag()
    {
        return "GenericEyeLook";
    }

    public void ManagedLateUpdate()
    {
        if (_eyeLookDummy == null)
        {
            Plugin.Logger.LogWarning("Eye look dummy not found! Creating new instance.");
            CreateDummy();
        }

        var direction = Vector3.Normalize(MainCamera.camera.transform.position - transform.position);
        if (useLimits)
        {
            var defaultUpVector = transform.parent.TransformDirection(_defaultLocalUp);
            // if out of range, use default look direction
            if (Vector3.Dot(direction, defaultUpVector) < dotLimit)
                direction = defaultUpVector;
        }
        _eyeLookDummy.rotation = Quaternion.RotateTowards(_eyeLookDummy.rotation,
            Quaternion.LookRotation(direction), Time.deltaTime * degreesPerSecond);
        transform.up = _eyeLookDummy.forward;
    }

    public int managedLateUpdateIndex { get; set; }
}