using UnityEngine;

namespace TheRedPlague.Mono.VFX;

public class AnimateMaterialProperty : MonoBehaviour
{
    public int[] propertyIds;
    public Renderer[] renderers;
    public float defaultValue;
    public float interpolationSpeed = 1f;
    public float updateInterval = 0.1f;

    private float _value;
    private float _targetValue;
    private float _timeUpdateAgain;

    private Material[] _materials;

    public void SetTargetPropertyValue(float value)
    {
        _targetValue = value;
    }
    
    private void Start()
    {
        var totalMaterials = 0;
        foreach (var renderer in renderers)
        {
            totalMaterials += renderer.sharedMaterials.Length;
        }

        _materials = new Material[totalMaterials];
        
        var index = 0;
        for (var i = 0; i < renderers.Length; i++)
        {
            var materials = renderers[i].materials;
            for (var j = 0; j < materials.Length; j++)
            {
                _materials[index] = materials[j];
                index++;
            }
        }

        _value = defaultValue;
        _targetValue = defaultValue;
        UpdateMaterials(true);
    }

    private void UpdateMaterials(bool first)
    {
        if (!first && Mathf.Approximately(_value, _targetValue))
        {
            return;
        }
        _value = Mathf.MoveTowards(_value, _targetValue,
            Time.deltaTime * interpolationSpeed / updateInterval);
        foreach (var material in _materials)
        {
            if (material == null) continue;
            foreach (var id in propertyIds)
            {
                material.SetFloat(id, _value);
            }
        }
    }

    private void Update()
    {
        if (Time.time > _timeUpdateAgain)
        {
            _timeUpdateAgain = Time.time + updateInterval;
            UpdateMaterials(false);
        }
    }
}