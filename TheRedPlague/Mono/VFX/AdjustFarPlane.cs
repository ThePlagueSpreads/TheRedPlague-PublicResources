using UnityEngine;

namespace TheRedPlague.Mono.VFX;

public class AdjustFarPlane : MonoBehaviour
{
    public float newFarClipPlane;
    public float transitionDuration;
    
    private float _oldFarClipPlane;
    private Camera _camera;
    
    private float _changePerSecond;

    private void Start()
    {
        _camera = MainCamera.camera;
        _oldFarClipPlane = _camera.farClipPlane;
        _changePerSecond = Mathf.Abs(newFarClipPlane - _oldFarClipPlane) / transitionDuration;
    }

    private void OnDestroy()
    {
        if (_camera)
        {
            _camera.farClipPlane = _oldFarClipPlane;
        }
    }

    private void LateUpdate()
    {
        _camera.farClipPlane =
            Mathf.MoveTowards(_camera.farClipPlane, newFarClipPlane, Time.deltaTime * _changePerSecond);
    }
}