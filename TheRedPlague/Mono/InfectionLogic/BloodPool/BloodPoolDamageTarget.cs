using TheRedPlague.Data;
using UnityEngine;

namespace TheRedPlague.Mono.InfectionLogic.BloodPool;

public class BloodPoolDamageTarget : MonoBehaviour
{
    private int _numTriggers;
    private LiveMixin _liveMixin;
    private Player _player;

    private void Start()
    {
        _liveMixin = GetComponent<LiveMixin>();
        _player = GetComponent<Player>();
        InvokeRepeating(nameof(ApplyDamage), 0f, 1f);
    }

    public void Increment()
    {
        _numTriggers++;
    }

    public void Decrement()
    {
        _numTriggers--;
        if (_numTriggers <= 0)
        {
            Destroy(this);
        }
    }

    private void ApplyDamage()
    {
        if ((_player == null || !_player.cinematicModeActive) && _liveMixin != null)
        {
            _liveMixin.TakeDamage(1f, transform.position, CustomDamageTypes.PenetrativePlagueDamage);
        }
    }
}