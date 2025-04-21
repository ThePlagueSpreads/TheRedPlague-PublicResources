using System.Collections;
using System.Collections.Generic;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using Story;
using TheRedPlague.MaterialModifiers;
using TheRedPlague.Mono.SFX;
using TheRedPlague.Mono.Systems;
using TheRedPlague.PrefabFiles.DomeDrones;
using UnityEngine;

namespace TheRedPlague.Mono.VFX;

public class DomeConstructionVfx : MonoBehaviour
{
    public Renderer domeShieldRenderer;
    public Texture emissiveTex;
    
    private Renderer[] _renderers;
    private List<Material> _materials;

    private float _constructionTimeStarted;
    private bool _constructing;
    
    private const float KConstructionDuration = 45;

    private void SetUpGhostMaterials()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _materials = new List<Material>();
        
        foreach (var renderer in _renderers)
        {
            _materials.AddRange(renderer.materials);
        }

        foreach (var material in _materials)
        {
            if (material.shader == null || material.shader.name == "DontRender") continue;
            
            material.EnableKeyword("FX_BUILDING");
            material.SetTexture(ShaderPropertyID._EmissiveTex, emissiveTex);
            material.SetFloat(ShaderPropertyID._Cutoff, 0.4f);
            material.SetColor(ShaderPropertyID._BorderColor, new Color(0f, 1f, 2f, 1f));
            material.SetFloat(ShaderPropertyID._Built, 0f);
            material.SetFloat(ShaderPropertyID._Cutoff, 0.42f);
            material.SetVector(ShaderPropertyID._BuildParams, new Vector4(0.05f, 0.05f, 0.01f, -0.01f));
            material.SetFloat(ShaderPropertyID._NoiseStr, 0.25f);
            material.SetFloat(ShaderPropertyID._NoiseThickness, material.name.ToLower().Contains("glass") ? 0.25f : 0.40f);
            material.SetFloat(ShaderPropertyID._BuildLinear, 1f);
            material.SetFloat(ShaderPropertyID._MyCullVariable, 0f);
            material.SetFloat(ShaderPropertyID._minYpos, 0);
            material.SetFloat(ShaderPropertyID._maxYpos, 3000);
        }
    }

    private IEnumerator Start()
    {
        var goalManager = StoryGoalManager.main;
        if (goalManager && goalManager.IsGoalComplete(StoryUtils.DomeConstructionEvent.key))
        {
            yield break;
        }
        
        domeShieldRenderer.enabled = false;
        SetUpGhostMaterials();

        yield return new WaitForSeconds(1);
        
        yield return BeginConstruction();
    }

    private IEnumerator BeginConstruction()
    {
        StoryUtils.PrepareForDomeConstruction.Trigger();
        yield return new WaitForSeconds(5);
        StoryUtils.DomeConstructionEvent.Trigger();
        _constructionTimeStarted = Time.time;
        Utils.PlayFMODAsset(AudioUtils.GetFmodAsset("DomeConstruction"), Vector3.zero);
        _constructing = true;
        yield return new WaitForSeconds(2);
        SpawnDrones();
    }

    private void Update()
    {
        if (!_constructing) return;
        if (Time.time > _constructionTimeStarted + KConstructionDuration)
        {
            OnConstructionEnded();
            _constructing = false;
            return;
        }

        var progress = (Time.time - _constructionTimeStarted) / KConstructionDuration;
        
        foreach (var material in _materials)
        {
            if (material.shader.name != "DontRender")
            {
                material.SetFloat(ShaderPropertyID._Built, progress);
            }
        }
    }

    private void SpawnDrones()
    {
        SpawnDrone(new Vector3[]
        {
            new(-57.37f, 258.67f, 7.99f),
            new(-23.17f, 332.27f, 46.29f),
            new(-58.73f, 346.56f, -51.16f),
            new(-77.12f, 335.15f, -140.09f),
            new(-75.94f, 303.23f, -149.67f),
            new(-62.55f, 270.11f, -100.13f)
        });
        SpawnDrone(new Vector3[]
        {
            new(-9.22f, 292.04f, -100.27f),
            new(3.94f, 381.22f, -96.99f),
            new(-118.36f, 348.43f, -89.98f),
            new(-132.57f, 303.18f, -91.61f),
            new(-134.58f, 267.36f, -91.84f)
        });
        SpawnDrone(new Vector3[]
        {
            new(-66.26f, 267.82f, -112.02f),
            new(-98.09f, 307.17f, -306.63f),
            new(-95.88f, 331.38f, -60.06f),
            new(-53.86f, 352.57f, 194.95f),
            new(-71.51f, 162.21f, -40.66f)
        });
    }

    private void SpawnDrone(Vector3[] path)
    {
        var drone = Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("TadpolePrefab"));
        drone.SetActive(false);
        drone.AddComponent<SkyApplier>().renderers = drone.GetComponentsInChildren<Renderer>();
        MaterialUtils.ApplySNShaders(drone, 6, 1f, 2f,
            new IgnoreParticleSystemsModifier(),
            new DomeDronePrefab.DomeDroneMaterialModifier());
        var followPath = drone.AddComponent<BasicPathFollower>();
        drone.transform.position = path[0];
        followPath.path = path;
        followPath.velocity = 20;
        followPath.rotateSpeed = 120;
        drone.SetActive(true);
    }

    private void OnConstructionEnded()
    {
        foreach (var material in _materials)
        {
            material.DisableKeyword("FX_BUILDING");
        }

        domeShieldRenderer.enabled = true;
        
        PlayIslandMusic.DisableForTime(600);
        TrpEventMusicPlayer.PlayMusic(AudioUtils.GetFmodAsset("RedPlagueThemeMusic"), 120);
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