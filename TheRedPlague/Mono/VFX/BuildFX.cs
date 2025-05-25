using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheRedPlague.Mono.VFX;

public class BuildFX : MonoBehaviour, IManagedUpdateBehaviour
{
    private const string ConstructingKeyword = "FX_BUILDING";

    public Renderer[] renderers;
    public Transform centerTransform;

    private List<Material> _materials;
    private Texture _fabricatorEmissiveTexture;
    private Settings _activeSettings;

    private bool _registered;
    private bool _busySettingUp;
    private bool _fxActive;

    private bool _interpolating;
    private float _interpolationEndTime;
    private float _interpolationStartValue;
    private float _interpolationEndValue;
    private float _interpolationDuration;

    public bool Ready { get; private set; }

    public float CurrentConstructPercent { get; set; }

    public void StartSetUp(Settings settings)
    {
        StartCoroutine(SetUp(settings));
    }

    private IEnumerator SetUp(Settings settings)
    {
        if (_busySettingUp) yield break;
        _busySettingUp = true;
        _activeSettings = settings;
        var fabricatorTask = CraftData.GetPrefabForTechTypeAsync(TechType.Fabricator);
        yield return fabricatorTask;
        _fabricatorEmissiveTexture = fabricatorTask.GetResult().GetComponent<CrafterGhostModel>()._EmissiveTex;
        SetUpMaterials(settings);
        Ready = true;
        _busySettingUp = false;
    }

    private void SetUpMaterials(Settings settings)
    {
        _materials = new List<Material>();
        foreach (var renderer in renderers)
        {
            _materials.AddRange(renderer.materials);
        }

        foreach (var material in _materials)
        {
            material.SetTexture(ShaderPropertyID._EmissiveTex, _fabricatorEmissiveTexture);
            material.SetFloat(ShaderPropertyID._Cutoff, settings.Cutoff);
            material.SetColor(ShaderPropertyID._BorderColor, settings.BorderColor);
            material.SetVector(ShaderPropertyID._BuildParams, settings.BuildParams);
            material.SetFloat(ShaderPropertyID._NoiseStr, settings.NoiseStrength);
            material.SetFloat(ShaderPropertyID._NoiseThickness, settings.NoiseThickness);
        }
    }

    public void StartFxWhenReady(float startBuildPercent)
    {
        if (!Ready && !_busySettingUp)
        {
            Plugin.Logger.LogWarning("BuildFX will not start because it hasn't been given instructions! Canceling...");
            return;
        }

        StartCoroutine(StartAnyTimeCoroutine(startBuildPercent));
    }

    private IEnumerator StartAnyTimeCoroutine(float startBuildPercent)
    {
        while (!Ready)
        {
            yield return null;
        }

        StartFx(startBuildPercent);
    }

    public void StartFx(float startBuildPercent)
    {
        if (!Ready)
        {
            Plugin.Logger.LogWarning("Starting BuildFX when not ready!");
            return;
        }

        if (_fxActive)
            return;

        CurrentConstructPercent = startBuildPercent;

        foreach (var material in _materials)
        {
            if (material == null) continue;
            material.SetFloat(ShaderPropertyID._BuildLinear, 1f);
            material.SetFloat(ShaderPropertyID._MyCullVariable, 0f);
            material.SetFloat(ShaderPropertyID._Built, CurrentConstructPercent);
            material.SetFloat(ShaderPropertyID._minYpos, centerTransform.position.y + _activeSettings.MinYPos);
            material.SetFloat(ShaderPropertyID._maxYpos, centerTransform.position.y + _activeSettings.MaxYPos);
            material.EnableKeyword(ConstructingKeyword);
        }

        Shader.SetGlobalFloat(ShaderPropertyID._SubConstructProgress, 0f);

        if (!_registered)
        {
            BehaviourUpdateUtils.Register(this);
            _registered = true;
        }

        _fxActive = true;
    }

    public void StopFx()
    {
        if (!_fxActive)
        {
            return;
        }

        foreach (var material in _materials)
        {
            if (material == null) continue;
            material.SetFloat(ShaderPropertyID._Built, 1f);
            material.DisableKeyword(ConstructingKeyword);
        }

        TryDeregister();

        _fxActive = false;
    }

    public void StartConstructionEffectInterpolation(float targetValue, float duration)
    {
        if (!Ready)
        {
            Plugin.Logger.LogWarning("Starting BuildFX interpolation when not ready!");
        }

        _interpolating = true;
        _interpolationStartValue = CurrentConstructPercent;
        _interpolationEndValue = targetValue;
        _interpolationEndTime = Time.time + duration;
        _interpolationDuration = duration;
    }

    public void StopConstructionEffectInterpolation()
    {
        _interpolating = false;
    }

    private void TryDeregister()
    {
        if (!_registered) return;
        BehaviourUpdateUtils.Deregister(this);
        _registered = false;
    }

    public string GetProfileTag()
    {
        return "TRP:BuildFX";
    }

    private void OnDestroy()
    {
        StopFx();
        
        foreach (var material in _materials)
        {
            Destroy(material);
        }
    }

    public void ManagedUpdate()
    {
        if (!isActiveAndEnabled)
            return;

        if (_interpolating)
        {
            CurrentConstructPercent = Mathf.Lerp(_interpolationStartValue, _interpolationEndValue,
                1f - (_interpolationEndTime - Time.time) / _interpolationDuration);
            if (Mathf.Approximately(CurrentConstructPercent, _interpolationEndValue))
            {
                CurrentConstructPercent = _interpolationEndValue;
                _interpolating = false;
            }
        }

        foreach (var material in _materials)
        {
            material.SetFloat(ShaderPropertyID._Built, CurrentConstructPercent);
        }
    }

    public int managedUpdateIndex { get; set; }


    public struct Settings
    {
        public Color BorderColor { get; }
        public float Cutoff { get; }
        public Vector4 BuildParams { get; }
        public float NoiseStrength { get; }
        public float NoiseThickness { get; }
        public float MinYPos { get; }
        public float MaxYPos { get; }

        public Settings(Color borderColor, Vector4 buildParams, float minYPos, float maxYPos, float cutoff = 0.42f,
            float noiseStrength = 0.25f, float noiseThickness = 0.49f)
        {
            BorderColor = borderColor;
            Cutoff = cutoff;
            BuildParams = buildParams;
            NoiseStrength = noiseStrength;
            NoiseThickness = noiseThickness;
            MinYPos = minYPos;
            MaxYPos = maxYPos;
        }
    }
}