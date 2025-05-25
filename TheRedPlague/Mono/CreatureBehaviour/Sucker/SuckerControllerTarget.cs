using System.Collections.Generic;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.Sucker;

public class SuckerControllerTarget : MonoBehaviour
{
    private static List<SuckerControllerTarget> _all = new();

    private readonly List<SuckerGrabVehicles> _attachedSuckers = new();

    private Exosuit _exosuit;
    private bool _isExosuit;

    private void Start()
    {
        _exosuit = GetComponent<Exosuit>();
        _isExosuit = _exosuit != null;
    }

    private void OnEnable()
    {
        _all.Add(this);
    }

    private void OnDisable()
    {
        _all.Remove(this);
        FixExosuitIssues();
    }

    public void AttachSucker(SuckerGrabVehicles sucker)
    {
        _attachedSuckers.Add(sucker);
        RecalculatePrimarySucker();
        if (_isExosuit && _attachedSuckers.Count == 1)
        {
            _exosuit.mainAnimator.SetBool("player_in", true);
            InvokeRepeating(nameof(FixExosuitIssues), 1, 1);
        }
    }

    public void RemoveSucker(SuckerGrabVehicles sucker)
    {
        _attachedSuckers.Remove(sucker);
        RecalculatePrimarySucker();
        if (_isExosuit && _attachedSuckers.Count == 0)
        {
            _exosuit.mainAnimator.SetBool("player_in", Player.main.GetVehicle() == _exosuit);
            CancelInvoke(nameof(FixExosuitIssues));
            FixExosuitIssues();
        }
    }

    private void FixExosuitIssues()
    {
        if (!_isExosuit)
            return;
        
        if (Player.main.GetVehicle() == _exosuit && _attachedSuckers.Count > 0 && !_exosuit._playerFullyEntered)
        {
            _exosuit.playerFullyEntered = true;
        }

        if (!_exosuit.playerFullyEntered && _attachedSuckers.Count > 0)
        {
            _exosuit.mainAnimator.SetBool("player_in", true);
        }
    }

    public void RecalculatePrimarySucker()
    {
        bool foundPrimary = false;
        foreach (var sucker in _attachedSuckers)
        {
            if (sucker == null) continue;
            if (!foundPrimary)
            {
                sucker.isPrimaryControllerOfTarget = true;
                foundPrimary = true;
                continue;
            }

            sucker.isPrimaryControllerOfTarget = false;
        }

        _attachedSuckers.RemoveAll(e => e == null);
    }

    public static bool TryGetClosest(out SuckerControllerTarget result, Vector3 referencePos, float maxRange)
    {
        result = null;

        if (_all.Count == 0) return false;

        var closestSqrDistance = float.MaxValue;
        foreach (var target in _all)
        {
            var sqrDist = Vector3.SqrMagnitude(target.transform.position - referencePos);
            if (sqrDist > closestSqrDistance) continue;
            if (sqrDist > maxRange * maxRange) continue;
            closestSqrDistance = sqrDist;
            result = target;
        }

        return result != null;
    }
}