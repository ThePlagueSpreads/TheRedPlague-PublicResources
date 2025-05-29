using UnityEngine;

namespace TheRedPlague.Mono.Util;

public class SimpleLOD : MonoBehaviour, IScheduledUpdateBehaviour
{
    public float unloadDistance;
    public GameObject toggledObject;

    private float _unloadDistanceSqr;

    private void Start()
    {
        _unloadDistanceSqr = unloadDistance * unloadDistance;
    }

    private void OnEnable()
    {
        UpdateSchedulerUtils.Register(this);
    }
    
    private void OnDisable()
    {
        UpdateSchedulerUtils.Deregister(this);
    }

    public string GetProfileTag()
    {
        return "TRP:SimpleLOD";
    }

    public void ScheduledUpdate()
    {
        var closestSqrDistance = Mathf.Min(
            (Player.main.transform.position - transform.position).sqrMagnitude,
            (MainCamera.camera.transform.position - transform.position).sqrMagnitude);
        toggledObject.SetActive(closestSqrDistance < _unloadDistanceSqr);
    }

    public int scheduledUpdateIndex { get; set; }
}