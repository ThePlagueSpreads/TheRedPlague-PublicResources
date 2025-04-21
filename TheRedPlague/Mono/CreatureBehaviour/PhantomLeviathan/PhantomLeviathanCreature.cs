namespace TheRedPlague.Mono.CreatureBehaviour.PhantomLeviathan;

public class PhantomLeviathanCreature : Creature
{
    public PhantomMeleeAttack meleeAttack;
    public PhantomPoisonAttack poisonAttack;
    public SwimBehaviour swimBehaviour;
    
    public bool CanAttack()
    {
        return !meleeAttack.IsAttacking() && !poisonAttack.IsAttacking();
    }
}