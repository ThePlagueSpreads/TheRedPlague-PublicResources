using System;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.Sucker;

public class SuckerLook : MonoBehaviour
{
    private Transform _suckerLookDummy;
    
    private void LateUpdate()
    {
        if (_suckerLookDummy == null)
        {
            Plugin.Logger.LogWarning("Sucker look dummy not found! Creating new instance.");
            CreateDummy();
        }
        _suckerLookDummy.rotation = Quaternion.RotateTowards(_suckerLookDummy.rotation,
            Quaternion.LookRotation(MainCamera.camera.transform.position - transform.position), Time.deltaTime * 180);
        transform.up = _suckerLookDummy.forward;
    }

    private void OnEnable()
    {
        CreateDummy();
    }
    
    private void CreateDummy()
    {
        if (_suckerLookDummy)
            return;
        _suckerLookDummy = new GameObject("SuckerEyeLookDummy").transform;
    }
    
    private void OnDisable()
    {
        if (_suckerLookDummy) Destroy(_suckerLookDummy.gameObject);
    }
}