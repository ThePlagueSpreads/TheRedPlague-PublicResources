using UnityEngine;

namespace TheRedPlague.Mono.UI;

public class UIBobAnimation : MonoBehaviour
{
    public float distance;
    public Vector3 axis;
    public float speed;

    private Vector3 _startPosition;
    private RectTransform _rt;
    
    private void Start()
    {
        _rt = GetComponent<RectTransform>();
        if (_rt == null)
        {
            Plugin.Logger.LogError("RectTransform not found on object " + gameObject);
        }
        _startPosition = _rt.localPosition;
    }

    private void Update()
    {
        _rt.localPosition = _startPosition + axis * (distance * Mathf.Sin(Time.time * speed));
    }
}