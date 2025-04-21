using System.Collections.Generic;
using System.Linq;
using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;

namespace TheRedPlague.Mono.VFX;

public class PlayerInfectionDamageVisualization : MonoBehaviour
{
    private Renderer[] _renderers;
    private List<Material> _materials;

    private float _timeNextUpdate;

    private float _damageForMinEffect = 10f;
    private float _damageForMaxEffect = 90f;
    private float _updateDelay = 0.2f;
    private float _maxInfectionAmount = 5;
    private float _interpolateSpeed = 15f;
    private float _changeTolerance = 0.01f;

    private bool _renderingPlayerAsInfected;

    private float _lastDisplayedDamage = -1f;
    private float _renderedDamage;

    private static readonly int InfectionHeightStrengthParameter = Shader.PropertyToID("_InfectionHeightStrength");
    private static readonly int InfectionScale = Shader.PropertyToID("_InfectionScale");

    private void Start()
    {
        _renderers = transform.Find("body").GetComponentsInChildren<Renderer>(true)
            .Where(r => !r.gameObject.name.Contains("BoneArmor") &&
                        !r.transform.parent.name.Contains("PDA")).ToArray();
        _materials = new List<Material>();
        foreach (var renderer in _renderers)
        {
            if (renderer != null)
            {
                _materials.AddRange(renderer.materials);
            }
        }

        foreach (var material in _materials)
        {
            if (material == null) continue;
            material.SetFloat(InfectionHeightStrengthParameter, 0.02f);
            material.EnableKeyword("UWE_INFECTION");
            material.SetTexture(ShaderPropertyID._InfectionAlbedomap, Plugin.ZombieInfectionTexture);
            material.SetVector(ShaderPropertyID._ModelScale, Vector4.one);
            material.SetVector(InfectionScale, new Vector3(2, 2, 2));
        }
    }


    private void Update()
    {
        if (Time.time < _timeNextUpdate) return;
        _timeNextUpdate = Time.time + _updateDelay;

        _renderedDamage = Mathf.MoveTowards(_renderedDamage, PlagueDamageStat.main.InfectionPercent,
            Time.deltaTime * _interpolateSpeed);

        if (Mathf.Abs(_lastDisplayedDamage - _renderedDamage) < _changeTolerance)
            return;

        _lastDisplayedDamage = _renderedDamage;

        if (_renderedDamage <= _damageForMinEffect)
        {
            if (_renderingPlayerAsInfected)
            {
                UpdateInfectionStrength();
            }

            return;
        }

        _renderingPlayerAsInfected = true;
        UpdateInfectionStrength();
    }

    private void UpdateInfectionStrength()
    {
        if (_materials == null) return;
        foreach (var material in _materials)
        {
            if (material == null) continue;
            var percent =
                Mathf.Clamp01(Mathf.InverseLerp(_damageForMinEffect, _damageForMaxEffect, _renderedDamage));
            material.SetFloat(ShaderPropertyID._InfectionAmount, Mathf.Lerp(0, _maxInfectionAmount, percent));
        }
    }
}