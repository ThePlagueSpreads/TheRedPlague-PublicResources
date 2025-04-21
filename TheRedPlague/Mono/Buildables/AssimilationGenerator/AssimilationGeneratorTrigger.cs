using UnityEngine;

namespace TheRedPlague.Mono.Buildables.AssimilationGenerator;

public class AssimilationGeneratorTrigger : MonoBehaviour
{
    public AssimilationGeneratorFunction function;
    
    public void OnTriggerEnter(Collider other)
    {
        if (!function.constructable.constructed) return;
        if (other.gameObject == Player.main.gameObject)
        {
            Player.main.liveMixin.TakeDamage(3);
            return;
        }
        var root = UWE.Utils.GetEntityRoot(other.gameObject);
        if (root == null) return;
        if (root.GetComponent<AssimilatorFood>() != null) return;
        var techType = CraftData.GetTechType(root);
        if (techType == TechType.None) return;
        if (!BaseBioReactor.charge.TryGetValue(techType, out var chargeValue)) return;
        var creature = root.GetComponent<Creature>();
        if (creature && creature.liveMixin.maxHealth > 500) return;

        function.EatItem(other.gameObject, chargeValue);
    }
}