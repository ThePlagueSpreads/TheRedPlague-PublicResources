using Nautilus.Handlers;

namespace TheRedPlague.Mono.Buildables.PlagueRefinery;

public class PlagueRefineryInterface : HandTarget, IHandTarget, ITreeActionReceiver
{
    public Constructable constructable;
    public PlagueRefineryMachine machine;

    public void OnHandHover(GUIHand hand)
    {
        if (!enabled || !constructable.constructed)
        {
            return;
        }

        var mainText = "PlagueRefinery";
        var subscript = string.Empty;
        bool showInput = true;
        if (machine.IsItemReadyForPickUp())
        {
            subscript = "PlagueRefineryOutputFull";
            showInput = false;
        }
        else if (machine.IsBusy())
        {
            mainText = "PlagueRefineryBusy";
            HandReticle.main.SetProgress(machine.GetProgress());
            HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1.5f);
            showInput = false;
        }
        else
        {
            if (!machine.HasSufficientPower())
            {
                subscript = "unpowered";
                showInput = false;
            }

            HandReticle.main.SetIcon(HandReticle.IconType.Hand);
        }

        HandReticle.main.SetText(HandReticle.TextType.Hand, mainText, translate: true,
            showInput ? GameInput.Button.LeftHand : GameInput.Button.None);
        HandReticle.main.SetText(HandReticle.TextType.HandSubscript, subscript, translate: true);
    }

    public void OnHandClick(GUIHand hand)
    {
        if (!constructable.constructed)
            return;
        if (machine.IsBusy())
            return;
        uGUI.main.craftingMenu.Open(PrefabFiles.Buildable.PlagueRefinery.CraftTreeType, this);
    }

    public bool PerformAction(TechType techType)
    {
        var recipe = CraftDataHandler.GetRecipeData(techType);

        if (recipe == null || recipe.Ingredients.Count != 1)
        {
            Plugin.Logger.LogError("Failed to find primary ingredient for " + techType +
                                   " (recipe must have only 1 ingredient)");
            ErrorMessage.AddMessage("Failure in loading/parsing recipe");
            return false;
        }

        if (machine.InsertItem(recipe.Ingredients[0].techType) == false) return false;
        uGUI.main.craftingMenu.Close(this);
        return true;
    }

    public bool inProgress => machine.IsBusy();
    public float progress => machine.GetProgress();
}