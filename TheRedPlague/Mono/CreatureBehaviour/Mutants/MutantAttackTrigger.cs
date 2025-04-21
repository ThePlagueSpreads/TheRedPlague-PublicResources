using Nautilus.Utility;
using TheRedPlague.PrefabFiles.Creatures;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.Mutants;

public class MutantAttackTrigger : MonoBehaviour
{
    public string prefabFileName;
    public Mutant.Settings settings;
    public float damage;
    public float instantKillChance = 0.1f;
    public float attackDelay = 1f;

    private GameObject _model;
    private float _timeCanAttackAgain;
    private static readonly FMODAsset NormalMutantBite = AudioUtils.GetFmodAsset("NormalMutantBite");
    private static readonly FMODAsset LargeMutantBite = AudioUtils.GetFmodAsset("LargeMutantBite");

    private DisableRigidbodyWhileOnScreen _disableRigidbodyWhileOnScreen;

    private void Start()
    {
        _model = transform.parent.GetChild(0)?.gameObject;
        if (IsHeavilyMutated)
            _disableRigidbodyWhileOnScreen = GetComponentInParent<DisableRigidbodyWhileOnScreen>();
    }

    private bool IsHeavilyMutated => settings.HasFlag(Mutant.Settings.HeavilyMutated);

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time < _timeCanAttackAgain) return;
        var player = GetTarget(other).GetComponent<Player>();
        if (player == null)
        {
            return;
        }
        DamagePlayer(player);
        _timeCanAttackAgain = Time.time + attackDelay;
    }

    private void DamagePlayer(Player player)
    {
        var calculatedDamage = DamageSystem.CalculateDamage(damage, DamageType.Normal, player.gameObject);
        if (calculatedDamage >= player.liveMixin.health && !Plugin.Options.DisableJumpScares)
        {
            JumpScare();
            return;
        }

        if (Random.value < instantKillChance && !Plugin.Options.DisableJumpScares)
        {
            JumpScare();
        }
        else
        {
            player.liveMixin.TakeDamage(calculatedDamage, transform.position);
            Utils.PlayFMODAsset(IsHeavilyMutated ? LargeMutantBite : NormalMutantBite, transform.position);
            if (_disableRigidbodyWhileOnScreen)
                _disableRigidbodyWhileOnScreen.UnfreezeForDuration(3);
        }
    }

    private void JumpScare()
    {
        DeathScare.PlayMutantDeathScare(prefabFileName, settings);
        if (_model) _model.SetActive(false);
        Invoke(nameof(ReEnableModel), 5);
    }
    
    private GameObject GetTarget(Collider collider)
    {
        var other = collider.gameObject;
        if (other.GetComponent<LiveMixin>() == null && collider.attachedRigidbody != null)
        {
            other = collider.attachedRigidbody.gameObject;
        }
        return other;
    }

    private void ReEnableModel()
    {
        if (_model) _model.SetActive(true);
    }
}