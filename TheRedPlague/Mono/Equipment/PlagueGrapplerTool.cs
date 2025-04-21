using UnityEngine;

namespace TheRedPlague.Mono.Equipment;

public class PlagueGrapplerTool : PlayerTool
{
    private const bool RightHandIsManipulateMode = false;
    private const bool RightHandIsRecall = false;
    private const float MinTravelDuration = 0.5f;
    private const float EnergyConsumptionPerUse = 1f;

    public GameObject projectileModel;
    public LineRenderer line;
    public Transform lineAttachPoint;

    private PlagueGrapplerProjectile _projectile;
    private bool _manipulateModeActive;
    private float _timeLastDeployed;
    
    public override string animToolName => "stasisrifle";

    public override bool OnLeftHandDown()
    {
        return HandleInput(false);
    }

    public override bool OnRightHandDown()
    {
        return HandleInput(true);
    }

    public override string GetCustomUseText()
    {
        var addCutInput = _projectile != null && CanCut();
        var baseText = GetBaseUseText();
        if (!addCutInput) return baseText;
        if (baseText.Length > 0) baseText += " | ";
        baseText += LanguageCache.GetButtonFormat("PlagueGrapplerCutWireInput", GameInput.Button.AltTool);
        return baseText;
    }

    public override bool OnAltDown()
    {
        if (_projectile != null && CanCut())
        {
            Destroy(_projectile.gameObject);
            _projectile.SpawnDestroyVfx();
        }

        return true;
    }

    private string GetBaseUseText()
    {
        if (!_projectile) return LanguageCache.GetButtonFormat("PlagueGrapplerFireInput", GameInput.Button.RightHand);

        if (_projectile.IsReelingIn())
        {
            return Language.main.GetFormat("PlagueGrapplerInputFormat",
                LanguageCache.GetButtonFormat("PlagueGrapplerCancelInput", GameInput.Button.LeftHand),
                LanguageCache.GetButtonFormat("PlagueGrapplerUnreelInput", GameInput.Button.RightHand));
        }

        if (_projectile.GrabbedOntoPlagueCave())
        {
            return LanguageCache.GetButtonFormat("PlagueGrapplerOpenCaveInput", GameInput.Button.RightHand);
        }

        if (!_projectile.HasGrabbed())
        {
            return string.Empty;
        }

        if (_projectile.UnableToGrabTarget())
        {
            return LanguageCache.GetButtonFormat("PlagueGrapplerTraverseInput", GameInput.Button.RightHand);
        }

        return Language.main.GetFormat("PlagueGrapplerInputFormat",
            LanguageCache.GetButtonFormat("PlagueGrapplerGrabInput", GameInput.Button.LeftHand),
            LanguageCache.GetButtonFormat("PlagueGrapplerFireInput", GameInput.Button.RightHand));
    }

    private bool HandleInput(bool rightHand)
    {
        if (!CanFire())
            return false;
        
        if (!CanSpawnProjectile())
        {
            return HandleReelInInput(rightHand);
        }

        if (rightHand)
        {
            return DeployProjectile();
        }

        return false;
    }

    private bool HandleReelInInput(bool rightHand)
    {
        if (Time.time < _timeLastDeployed + MinTravelDuration)
            return true;
        if (_projectile == null)
            return true;
        if (_projectile.GrabbedOntoPlagueCave())
        {
            _projectile.OpenPlagueCave();
            return true;
        }
        if (_projectile.IsReelingIn())
        {
            if (rightHand == RightHandIsRecall)
            {
                _projectile.Recall();
            }
            else
            {
                _projectile.CancelReel();
            }

            return true;
        }

        return _projectile.ReelIn(rightHand == RightHandIsManipulateMode);
    }

    private bool CanCut()
    {
        if (_projectile && _projectile.GrabbedOntoPlagueCave()) return false;
        return true;
    }

    private bool CanFire()
    {
        return !Player.main.IsInside();
    }

    private bool CanSpawnProjectile()
    {
        if (_projectile != null)
            return false;
        return Time.time > _timeLastDeployed + MinTravelDuration;
    }

    private bool DeployProjectile()
    {
        if (energyMixin.IsDepleted())
            return false;

        energyMixin.ConsumeEnergy(EnergyConsumptionPerUse);
        
        projectileModel.SetActive(false);
        var projectile = Instantiate(projectileModel);
        projectile.transform.position = projectileModel.transform.position;
        projectile.transform.rotation = projectileModel.transform.rotation;
        var projectileSkyApplier = projectile.AddComponent<SkyApplier>();
        projectileSkyApplier.renderers = projectile.GetComponentsInChildren<Renderer>();
        projectileSkyApplier.dynamic = true;
        projectile.SetActive(true);
        _projectile = projectile.AddComponent<PlagueGrapplerProjectile>();
        _projectile.Launch(this);
        _timeLastDeployed = Time.time;
        line.enabled = true;
        return true;
    }

    public override void OnHolster()
    {
        if (_projectile)
            Destroy(_projectile.gameObject);
    }

    private void OnDisable()
    {
        if (_projectile)
            Destroy(_projectile.gameObject);
    }

    public void OnProjectileDestroyed()
    {
        line.enabled = false;
        projectileModel.SetActive(true);
    }
}