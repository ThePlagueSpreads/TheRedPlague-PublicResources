using UnityEngine;

namespace TheRedPlague.Mono.VFX;

public class AdjustFarPlane : MonoBehaviour
{
    public float newFarClipPlane;
    public float transitionDuration;
    public float maxDepthToApply;

    private float _oldFarClipPlane;
    private Camera _camera;

    private float _changePerSecond;
    private float _currentFarClipPlane;

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
        _currentFarClipPlane = Mathf.MoveTowards(_currentFarClipPlane,
            newFarClipPlane, Time.deltaTime * _changePerSecond);
        _camera.farClipPlane = GetActualFarClipPlane();
    }

    private float GetActualFarClipPlane()
    {
        return Mathf.Lerp(newFarClipPlane, _oldFarClipPlane,
            Mathf.InverseLerp(0, maxDepthToApply, Ocean.GetDepthOf(Player.main.gameObject)));
    }
}