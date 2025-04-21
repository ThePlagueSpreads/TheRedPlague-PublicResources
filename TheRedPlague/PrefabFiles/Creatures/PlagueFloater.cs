using System.Collections;
using System.Collections.Generic;
using ECCLibrary;
using ECCLibrary.Data;
using ECCLibrary.Mono;
using Nautilus.Assets;
using Nautilus.Extensions;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using TheRedPlague.Data;
using TheRedPlague.Mono.CreatureBehaviour;
using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Creatures;

public class PlagueFloater : CreatureAsset
{
    private static readonly int Shininess = Shader.PropertyToID("_Shininess");
    private static readonly int SpecInt = Shader.PropertyToID("_SpecInt");
    private static readonly int Fresnel = Shader.PropertyToID("_Fresnel");

    public PlagueFloater(PrefabInfo prefabInfo) : base(prefabInfo)
    {
    }

    protected override void PostRegister()
    {
        CreatureDataUtils.AddCreaturePDAEncyclopediaEntry(this, CustomPdaPaths.PlagueCreationsPath,
            null, null, 5, null, null);
    }

    protected override CreatureTemplate CreateTemplate()
    {
        var template = new CreatureTemplate(() => Plugin.CreaturesBundle.LoadAsset<GameObject>("PlagueFloaterPrefab"),
            BehaviourType.Shark, EcoTargetType.Shark, 2000)
        {
            CanBeInfected = false,
            LocomotionData = new LocomotionData(10, 0.05f, 3f, 1f, true),
            StayAtLeashData = new StayAtLeashData(0.4f, 1, 40),
            SwimBehaviourData = new SwimBehaviourData(0.5f),
            SwimRandomData = new SwimRandomData(0.2f, 3, new Vector3(40, 2, 40), 4, 1f, true),
            AvoidObstaclesData = new AvoidObstaclesData(0.45f, 1, false, 5f, 6f),
            BehaviourLODData = new BehaviourLODData(30, 60, 100),
            FleeOnDamageData = null,
            AnimateByVelocityData = new AnimateByVelocityData(3)
        };
        CreatureTemplateUtils.SetCreatureDataEssentials(template, LargeWorldEntity.CellLevel.Far, 800, 0,
            new BehaviourLODData(150, 500, 2000), 1000);
        return template;
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab, CreatureComponents components)
    {
        prefab.AddComponent<RedPlagueHost>().mode = RedPlagueHost.Mode.PlagueCreation;

        var meleeAttack = CreaturePrefabUtils.AddMeleeAttack<MeleeAttack>(prefab, components,
            prefab.transform.Find("AttackTrigger").gameObject, true, 45f, 2.5f);
        meleeAttack.eatHungerDecrement = 0.5f;
        meleeAttack.biteAggressionThreshold = 0.3f;
        var biteSound = prefab.AddComponent<FMOD_StudioEventEmitter>();
        biteSound.path = "MutantStalkerBite";
        biteSound.minInterval = 2;
        biteSound.startEventOnAwake = false;
        meleeAttack.attackSound = biteSound;

        /*
        var root = prefab.transform.Find("PlagueFloater/PlagueFloater/Root");

        foreach (Transform child in root)
        {
            if (!child.gameObject.name.StartsWith("Tendril")) continue;
            var trailManagerBuilder = new TrailManagerBuilder(components, child, 2f);
            trailManagerBuilder.SetTrailArrayToAllChildren();
            trailManagerBuilder.Apply();
        }
        */

        yield break;
    }

    protected override void ApplyMaterials(GameObject prefab)
    {
        MaterialUtils.ApplySNShaders(prefab, 7f, 1f, 1f,
            new DoubleSidedModifier(MaterialUtils.MaterialType.Transparent), new PlagueFloaterMaterialModifier());
    }

    private class PlagueFloaterMaterialModifier : MaterialModifier
    {
        public override void EditMaterial(Material material, Renderer renderer, int materialIndex,
            MaterialUtils.MaterialType materialType)
        {
            if (materialType == MaterialUtils.MaterialType.Transparent)
            {
                if (material.name.Contains("Tentacle"))
                {
                    material.SetFloat(SpecInt, 10);
                    material.SetFloat(Fresnel, 0.65f);
                    material.SetFloat(ShaderPropertyID._GlowStrength, 0.3f);
                    material.SetFloat(ShaderPropertyID._GlowStrengthNight, 0.3f);
                }
                else
                {
                    material.color = new Color(1f, 1f, 1f, 1.5f);
                    material.SetFloat(SpecInt, 30);
                    material.SetFloat(Fresnel, 0.74f);
                    material.SetFloat(ShaderPropertyID._GlowStrength, 0.3f);
                    material.SetFloat(ShaderPropertyID._GlowStrengthNight, 0.3f);
                    material.SetColor("_SpecColor", new Color(1f, 1f, 4f, 1f));
                }
            }
            else
            {
                if (material.name.Contains("Eye"))
                {
                    material.color = Color.black * 0.4f;
                    material.SetFloat(SpecInt, 5);
                    material.SetFloat(Fresnel, 0.64f);
                    material.SetFloat(Shininess, 6);
                }
                else if (material.name.Contains("Organ"))
                {
                    material.SetFloat(ShaderPropertyID._GlowStrength, 0.5f);
                    material.SetFloat(ShaderPropertyID._GlowStrengthNight, 0.3f);
                }
            }
        }
    }
}