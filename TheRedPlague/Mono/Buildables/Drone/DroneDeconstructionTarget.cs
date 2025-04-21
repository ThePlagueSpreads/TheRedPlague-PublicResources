using System.Collections.Generic;
using UnityEngine;

namespace TheRedPlague.Mono.Buildables.Drone;

public class DroneDeconstructionTarget : MonoBehaviour
{
    public float deconstructDuration = 30f;
    public float percentDeconstructed;
    public Texture deconstructEmissiveTexture;
    public bool useCustomDeconstructDuration;

    private const float MinVolume = 200;
    private const float MaxVolume = 10000;
    private const float MinDeconstructDuration = 1.333f;
    private const float MaxDeconstructDuration = 350;

    public float deconstructRange = 60f;

    public Transform[] deconstructTransforms;

    private List<Material> _materials;

    private bool _vfxStarted;

    private Bounds _myBounds;
    
    public static float MaxDeconstructRange { get; private set; }
    
    private void Awake()
    {
        if (!useCustomDeconstructDuration)
            CalculateDeconstructDuration();
        if (deconstructTransforms == null)
            deconstructTransforms = new[] { transform };
    }

    private void Start()
    {
        if (deconstructRange > MaxDeconstructRange)
            MaxDeconstructRange = deconstructRange;
        
        ApplyMaterials();
    }

    private void CalculateDeconstructDuration()
    {
        _myBounds = new Bounds(transform.position, Vector3.one);
        var renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            _myBounds.Encapsulate(renderer.bounds);
        }

        var volume = _myBounds.size.x * _myBounds.size.y * _myBounds.size.z;

        var percent = Mathf.Clamp01(Mathf.InverseLerp(MinVolume, MaxVolume, volume));
        deconstructDuration = Mathf.Lerp(MinDeconstructDuration, MaxDeconstructDuration, percent);
    }

    public bool CanDeconstruct()
    {
        return percentDeconstructed < 1;
    }

    public void DeconstructThisFrame()
    {
        if (!_vfxStarted)
        {
            EnableDeconstructionVfx();
        }

        var oldPercent = percentDeconstructed;
        percentDeconstructed = Mathf.Clamp01(percentDeconstructed + Time.deltaTime / deconstructDuration);
        if (!Mathf.Approximately(oldPercent, percentDeconstructed))
        {
            UpdateMaterials();
        }
    }

    private void ApplyMaterials()
    {
        var renderers = gameObject.GetComponentsInChildren<Renderer>();
        _materials = new List<Material>();
        foreach (var renderer in renderers)
        {
            _materials.AddRange(renderer.materials);
        }

        foreach (var material in _materials)
        {
            if (material == null) continue;
            material.SetTexture(ShaderPropertyID._EmissiveTex, deconstructEmissiveTexture);
            material.SetFloat(ShaderPropertyID._Cutoff, 0.4f);
            // material.SetColor(ShaderPropertyID._BorderColor, new Color(0f, 1f, 2f, 1f));
            material.SetColor(ShaderPropertyID._BorderColor, new Color(0.7f, 0.1f, 0f));
            material.SetFloat(ShaderPropertyID._Built, 0f);
            material.SetFloat(ShaderPropertyID._Cutoff, 0.42f);
            material.SetVector(ShaderPropertyID._BuildParams, new Vector4(0.1f, 0.1f, 0.1f, -0.01f));
            material.SetFloat(ShaderPropertyID._NoiseStr, 1f);
            material.SetFloat(ShaderPropertyID._NoiseThickness, 0.50f);
            material.SetFloat(ShaderPropertyID._BuildLinear, 1f);
            material.SetFloat(ShaderPropertyID._MyCullVariable, 0f);
            material.SetFloat(ShaderPropertyID._minYpos, _myBounds.min.y);
            material.SetFloat(ShaderPropertyID._maxYpos, _myBounds.max.y);
        }
    }

    private void EnableDeconstructionVfx()
    {
        foreach (var material in _materials)
        {
            if (material == null) continue;
            material.EnableKeyword("FX_BUILDING");
        }

        _vfxStarted = true;
    }

    private void UpdateMaterials()
    {
        foreach (var material in _materials)
        {
            if (material == null) continue;
            material.SetFloat(ShaderPropertyID._Built, 1 - percentDeconstructed * 0.75f);
        }
    }

    private void OnDestroy()
    {
        if (_materials == null) return;
        foreach (var material in _materials)
        {
            Destroy(material);
        }
    }
}