using Nautilus.Utility;
using TheRedPlague.Mono.InfectionLogic;
using TheRedPlague.PrefabFiles.Items;
using UnityEngine;

namespace TheRedPlague.Mono.Equipment;

public class TransfuserTool : PlayerTool
{
    public override string animToolName => "transfuser";

    private static readonly FMODAsset FailSound = AudioUtils.GetFmodAsset("event:/tools/transfuser/fail");
    private static readonly FMODAsset TakeSampleSound = AudioUtils.GetFmodAsset("event:/tools/transfuser/take_sample");

    private bool _busy;

    private float _lastFailTime = -100;
    
    public override string GetCustomUseText()
    {
        if (_busy) return Language.main.Get("UseInfectionSamplerBusy");
        if (Time.time < _lastFailTime + 2) return Language.main.Get("UseInfectionSamplerFail");
        return LanguageCache.GetButtonFormat("UseInfectionSampler", GameInput.Button.RightHand);
    }
    
    public override bool OnRightHandDown()
    {
        if (_busy) return false;
        if (Targeting.GetTarget(Player.main.gameObject, 3, out var target, out var distance))
        {
            bool givesSample = false;
            
            // Check if it is infected
            var infectedMixin = target.GetComponent<InfectedMixin>();
            if (infectedMixin == null)
            {
                infectedMixin = target.GetComponentInParent<InfectedMixin>();
            }
            if (infectedMixin != null)
            {
                givesSample = infectedMixin.IsInfected();
            }

            // Check if it is a "plague creation"
            var infectionTarget = target.GetComponent<RedPlagueHost>();
            if (infectionTarget == null)
            {
                infectionTarget = target.GetComponentInParent<RedPlagueHost>();
            }
            if (infectionTarget != null && infectionTarget.mode == RedPlagueHost.Mode.PlagueCreation)
            {
                givesSample = true;
            }

            if (givesSample)
            {
                Utils.PlayFMODAsset(TakeSampleSound, transform.position);
                Invoke(nameof(AddSampleDelayed), 6);
                _busy = true;
                return true;
            }
        }
        Utils.PlayFMODAsset(FailSound, transform.position);
        _lastFailTime = Time.time;
        return true;
    }

    private void AddSampleDelayed()
    {
        _busy = false;
        CraftData.AddToInventory(RedPlagueSample.Info.TechType);
        StoryUtils.TransfuserSampleTakenEvent.Trigger();
    }
}