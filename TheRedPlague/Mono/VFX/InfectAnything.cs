using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Story;

namespace TheRedPlague.Mono.VFX;

// add this script to any object to infect it lol
public class InfectAnything : MonoBehaviour
{
    public Renderer[] renderers;
    private List<Material> _materials;

    public bool infectedAtStart = true;
    public float infectionHeightStrength = 0.1f;

    public float infectionAmount = 4;
    public Vector3 infectionScale = Vector3.one;

    public float infectChance = 1f;
    public float infectChanceWhenHiveMindIsReleased = 1f;
    
    private const string ShaderKeyWord = "UWE_INFECTION";
    
    private static readonly int InfectionHeightStrengthParameter = Shader.PropertyToID("_InfectionHeightStrength");
    private static readonly int InfectionScaleParameter = Shader.PropertyToID("_InfectionScale");

    public bool isAppliedToPlayer;

    private void Start()
    {
        if (infectChance < 1 || infectChanceWhenHiveMindIsReleased < 1)
        {
            var goalManager = StoryGoalManager.main;
            var hivemindIsReleased =
                goalManager != null && goalManager.IsGoalComplete(StoryUtils.HiveMindReleasedGoal.key);
            var random = Random.value;
            if (!hivemindIsReleased && random > infectChance)
            {
                return;
            }
            if (hivemindIsReleased && random > infectChanceWhenHiveMindIsReleased)
            {
                return;
            }
        }
        isAppliedToPlayer = gameObject.GetComponent<Player>() != null;
        ApplyShading(infectedAtStart);
    }

    public void ApplyShading(bool infected)
    {
        if (renderers == null || renderers.Length == 0)
        {
            if (isAppliedToPlayer)
            {
                renderers = transform.Find("body").GetComponentsInChildren<Renderer>(true).Where((r) => !r.gameObject.name.Contains("BoneArmor")).ToArray();
            }
            else
            {
                renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            }
        }
        if (_materials == null)
        {
            _materials = new List<Material>();
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    _materials.AddRange(renderer.materials);
                }
            }
        }
        foreach (var material in _materials)
        {
            if (material == null) continue;
            
            material.SetFloat(ShaderPropertyID._InfectionAmount, infectionAmount);
            material.SetVector(ShaderPropertyID._ModelScale, base.transform.localScale);
            if (infected)
            {
                material.EnableKeyword(ShaderKeyWord);
                material.SetTexture(ShaderPropertyID._InfectionAlbedomap, Plugin.ZombieInfectionTexture);
                material.SetFloat(InfectionHeightStrengthParameter, infectionHeightStrength);
                material.SetVector(InfectionScaleParameter, infectionScale);
            }
            else
            {
                material.DisableKeyword(ShaderKeyWord);
            }
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