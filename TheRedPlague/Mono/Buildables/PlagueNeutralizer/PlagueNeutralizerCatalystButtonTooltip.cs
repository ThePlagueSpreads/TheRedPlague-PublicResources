using UnityEngine;

namespace TheRedPlague.Mono.Buildables.PlagueNeutralizer;

public class PlagueNeutralizerCatalystButtonTooltip : MonoBehaviour, ITooltip
{
    public PlagueNeutralizerMachine machine;

    public void GetTooltip(TooltipData tooltip)
    {
        if (machine == null) return;
        tooltip.prefix.Append(GetText());
        ;
    }

    private string GetText()
    {
        if (machine.HasSpaceForCatalysts())
        {
            return Language.main.GetFormat("PlagueNeutralizerInsertCatalystTooltip", machine.GetCatalystsCount(),
                machine.catalysts.Capacity);
        }

        return Language.main.Get("PlagueNeutralizerInsertCatalystFullTooltip");
    }

    public bool showTooltipOnDrag => false;
}