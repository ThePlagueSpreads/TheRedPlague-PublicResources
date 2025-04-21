using ECCLibrary.Mono;
using Nautilus.Utility;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.PhantomLeviathan;

public class PhantomAttackLastTarget : AttackLastTarget
{
    private static readonly FMODAsset ChargeSound = AudioUtils.GetFmodAsset("PhantomLeviathanAttack");
    
    public CreatureVoice voice;
    
    public override void StartPerform(Creature creature, float time)
    {
        base.StartPerform(creature, time);
        if (Time.time > voice.TimeLastPlayed + 8)
        {
            voice.emitter.SetAsset(ChargeSound);
            voice.emitter.Play();
            voice.BlockIdleSoundsForTime(10);
        }
    }
}