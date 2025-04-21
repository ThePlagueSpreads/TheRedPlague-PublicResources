using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;

namespace TheRedPlague.Mono.Buildables.PlagueAltar;

public class PlagueAltarCrafter : GhostCrafter
{
    private const float FabricatorPowerConsumption = 5f;
    private const float InfectionDamage = 9;
    
    private static readonly int CraftingParamId = Animator.StringToHash("crafting");
    
    public Animator animator;
    public FMODAsset interactSound;
    public FMOD_CustomLoopingEmitter craftSoundEmitter;
    public GameObject sparksPrefab;
    public Transform[] beamEndPoints;
    public Renderer[] beams;
    
    private ParticleSystem[] _craftingParticles;

    public float extraPowerConsumption = 25f;
    
    public override void Start()
    {
        base.Start();
        spawnAnimationDuration = 6f;
        _craftingParticles = new ParticleSystem[beamEndPoints.Length];
        for (var i = 0; i < _craftingParticles.Length; i++)
        {
            var particle = Instantiate(sparksPrefab, beamEndPoints[i], true);
            particle.transform.localPosition = Vector3.up * 0.006f;
            particle.transform.localRotation = Quaternion.identity;
            particle.SetActive(true);
            _craftingParticles[i] = particle.GetComponent<ParticleSystem>();
        }
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
        if (logic == null || !logic.inProgress)
            return;
        var highestTransform = ghost != null && ghost.itemSpawnPoint != null ? ghost.itemSpawnPoint : transform;
        Shader.SetGlobalFloat(ShaderPropertyID._FabricatorPosY, highestTransform.position.y - 0.3f);
    }

    public override void OnOpenedChanged(bool opened)
    {
        base.OnOpenedChanged(opened);
        if (opened)
        {
            Utils.PlayFMODAsset(interactSound, transform.position);
        }
    }

    public override void OnStateChanged(bool crafting)
    {
        base.OnStateChanged(crafting);
        animator.SetBool(CraftingParamId, crafting);
        if (crafting)
        {
            craftSoundEmitter.Play();
            PlagueDamageStat.main.TakeInfectionDamage(InfectionDamage);
            StoryUtils.PlagueAltarFirstUse.Trigger();
        }
        else
        {
            craftSoundEmitter.Stop();
            Utils.PlayFMODAsset(interactSound, transform.position);
        }

        foreach (var particle in _craftingParticles)
        {
            particle.SetPlaying(crafting && MiscSettings.flashes);
        }

        foreach (var beam in beams)
        {
            beam.enabled = crafting;
        }
    }

    public override void Craft(TechType techType, float duration)
    {
        var totalRequiredPower = extraPowerConsumption + FabricatorPowerConsumption;
        
        if (GameModeUtils.RequiresPower() && powerRelay.GetPower() < totalRequiredPower)
        {
            ErrorMessage.AddMessage(Language.main.GetFormat("TrpMachineInsufficientPower", totalRequiredPower));
            return;
        }

        powerRelay.ConsumeEnergy(extraPowerConsumption, out _);
        
        base.Craft(techType, duration);
    }
}