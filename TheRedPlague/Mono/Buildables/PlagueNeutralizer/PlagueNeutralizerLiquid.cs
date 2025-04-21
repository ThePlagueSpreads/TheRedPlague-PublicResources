using UnityEngine;

namespace TheRedPlague.Mono.Buildables.PlagueNeutralizer;

public class PlagueNeutralizerLiquid : MonoBehaviour
{
    public MeshRenderer renderer;

    private readonly Color _normalColor = new(0.2f, 0.309f, 0.40666f, 0.5f);
    private readonly Color _redColor = new(0.666f, 0.095f, 0.047f, 0.61f);

    private Material _material;

    private float _currentAmount;
    private float _targetAmount;
    
    private void Awake()
    {
        _material = new Material(renderer.material);
        _material.color = _normalColor;
        renderer.material = _material;
    }

    public void SetDisplayedAmount(float percentage)
    {
        _targetAmount = percentage;
    }

    private void OnDestroy()
    {
        Destroy(_material);
    }

    private void Update()
    {
        _currentAmount = Mathf.MoveTowards(_currentAmount, _targetAmount, Time.deltaTime);
        _material.color = Color.Lerp(_normalColor, _redColor, _currentAmount);
    }
}