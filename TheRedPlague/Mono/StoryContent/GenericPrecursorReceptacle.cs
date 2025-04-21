using Nautilus.Utility;
using UnityEngine;

namespace TheRedPlague.Mono.StoryContent;

public abstract class GenericPrecursorReceptacle : HandTarget, IHandTarget
{
    private PlayerCinematicController _cinematicController;

    private Animator _animator;

    private static readonly FMODAsset UseSound = AudioUtils.GetFmodAsset("event:/player/cube terminal_use");

    private static readonly FMODAsset OpenSound = AudioUtils.GetFmodAsset("event:/player/cube terminal_open");

    private static readonly FMODAsset CloseSound = AudioUtils.GetFmodAsset("event:/player/cube terminal_close");

    private GameObject _insertedItem;
    private int _restoreQuickSlot = -1;
    
    private static readonly int OpenAnimationParameter = Animator.StringToHash("Open");

    public override void Awake()
    {
        base.Awake();
        
        _animator = GetComponent<Animator>();
        _cinematicController = GetComponent<PlayerCinematicController>();
        _cinematicController.informGameObject = gameObject;
    }

    public void OnHandHover(GUIHand hand)
    {
        if (!IsAcceptingItems()) return;
        
        HandReticle.main.SetText(HandReticle.TextType.Hand, GetHandUseText(), true,
            GameInput.Button.LeftHand);

        HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
        HandReticle.main.SetIcon(HandReticle.IconType.Hand);
    }

    public void OnHandClick(GUIHand hand)
    {
        if (!IsAcceptingItems()) return;

        var techType = TechTypeToRemove;
        var removedItem = Inventory.main.container.RemoveItem(techType);

        if (removedItem == null) return;

        _restoreQuickSlot = Inventory.main.quickSlots.activeSlot;
        Inventory.main.ReturnHeld(true);
        _insertedItem = removedItem.gameObject;
        Destroy(_insertedItem.GetComponent<PlagueHeartBehavior>());
        _insertedItem.transform.SetParent(Inventory.main.toolSocket);
        OnCinematicStarted(_insertedItem.transform, techType);
        _insertedItem.SetActive(true);
        var component = _insertedItem.GetComponent<Rigidbody>();
        if (component != null)
        {
            UWE.Utils.SetIsKinematicAndUpdateInterpolation(component, true);
        }

        _cinematicController.StartCinematicMode(Player.main);
        Utils.PlayFMODAsset(UseSound, transform);
    }

    public void OpenDeck()
    {
        if (IsAcceptingItems())
        {
            return;
        }

        _animator.SetBool(OpenAnimationParameter, true);
        Utils.PlayFMODAsset(OpenSound, base.transform);
    }

    public void CloseDeck()
    {
        if (!_animator.GetBool(OpenAnimationParameter)) return;
        _animator.SetBool(OpenAnimationParameter, false);
        Utils.PlayFMODAsset(CloseSound, base.transform);
    }

    public void OnPlayerCinematicModeEnd(PlayerCinematicController controller)
    {
        if (_insertedItem)
        {
            Destroy(_insertedItem);
        }

        CloseDeck();
        OnCinematicEnded();

        if (_restoreQuickSlot != -1)
        {
            Inventory.main.quickSlots.Select(_restoreQuickSlot);
        }
    }

    protected abstract TechType TechTypeToRemove { get; }
    protected abstract bool IsAcceptingItems();
    protected abstract void OnCinematicStarted(Transform itemTransform, TechType itemTechType);
    protected abstract void OnCinematicEnded();
    protected abstract string GetHandUseText();
}