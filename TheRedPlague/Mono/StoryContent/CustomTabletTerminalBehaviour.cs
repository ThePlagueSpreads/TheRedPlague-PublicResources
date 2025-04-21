using System;
using UnityEngine;

namespace TheRedPlague.Mono.StoryContent;

public abstract class CustomTabletTerminalBehaviour : HandTarget, IHandTarget
{
	// Customizable data
	public PrecursorKeyTerminal.PrecursorKeyType acceptKeyType;
	
	public bool usesCustomKeyType;
	public Texture2D customKeyTexture;
	public TechType customKeyTechType;
	
	// Essential references
	public Material[] keyMats = new Material[5];
	public MeshRenderer keyFace;
	public Animator animator;
	public PlayerCinematicController cinematicController;

	// SFX
	public FMODAsset useSound;
	public FMODAsset openSound;
	public FMODAsset closeSound;
	
	private GameObject _keyObject;

	private int _restoreQuickSlot = -1;

	private bool Slotted
	{
		get
		{
			_slotted ??= LoadSavedSlottedState();
			return _slotted.Value;
		}
	}
	
	private bool? _slotted;
	private static readonly int OpenAnimatorParameter = Animator.StringToHash("Open");

	private const string InsertPrecursorKeyHandText = "Insert_Precursor_Key";

	protected abstract void OnCinematicModeStarted();
	protected abstract void OnActivation();
	protected abstract bool LoadSavedSlottedState();
	protected abstract void SaveStateAsSlotted();

	private void Start()
	{
		if (usesCustomKeyType)
		{
			keyFace.material = new Material(keyMats[0])
			{
				mainTexture = customKeyTexture
			};
			return;
		}
		keyFace.material = keyMats[(int)acceptKeyType];
	}

	private void OnEnable()
	{
		if (Slotted)
		{
			OnActivation();
		}
	}

	public void OpenDeck()
	{
		if (!Slotted)
		{
			Utils.PlayFMODAsset(openSound, transform);
			animator.SetBool(OpenAnimatorParameter, value: true);
		}
	}

	public void CloseDeck()
	{
		if (!animator.GetBool(OpenAnimatorParameter)) return;
		animator.SetBool(OpenAnimatorParameter, value: false);
		Utils.PlayFMODAsset(closeSound, base.transform);
	}

	private void DestroyKey()
	{
		if (_keyObject)
		{
			Destroy(_keyObject);
		}
	}

	public void OnPlayerCinematicModeEnd()
	{
		if ((bool)_keyObject)
		{
			_keyObject.transform.parent = null;
		}
		OnActivation();
		Invoke(nameof(CloseDeck), 4f);
		var animationLength = 0.25f;
		Invoke(nameof(CloseDeck), animationLength);
		Invoke(nameof(DestroyKey), animationLength + 0.2f);
		if (_restoreQuickSlot != -1)
		{
			Inventory.main.quickSlots.Select(_restoreQuickSlot);
		}
	}
	
	public void OnHandHover(GUIHand hand)
	{
		if (!Slotted)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "Insert_Precursor_Key", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (Slotted)
		{
			return;
		}
		var techType = usesCustomKeyType ? customKeyTechType : ConvertKeyTypeToTechType(acceptKeyType);
		var pickupable = Inventory.main.container.RemoveItem(techType);
		_restoreQuickSlot = -1;
		if (pickupable == null) return;
		OnCinematicModeStarted();
		_restoreQuickSlot = Inventory.main.quickSlots.activeSlot;
		Inventory.main.ReturnHeld();
		_keyObject = pickupable.gameObject;
		_keyObject.transform.SetParent(Inventory.main.toolSocket);
		_keyObject.transform.localPosition = Vector3.zero;
		_keyObject.transform.localRotation = Quaternion.identity;
		_keyObject.SetActive(value: true);
		Rigidbody component = _keyObject.GetComponent<Rigidbody>();
		if (component != null)
		{
			UWE.Utils.SetIsKinematicAndUpdateInterpolation(component, isKinematic: true);
		}
		cinematicController.StartCinematicMode(Player.main);
		Utils.PlayFMODAsset(useSound, base.transform);
		SaveStateAsSlotted();
		_slotted = true;
	}

	private static TechType ConvertKeyTypeToTechType(PrecursorKeyTerminal.PrecursorKeyType inputType)
	{
		return (TechType)Enum.Parse(typeof(TechType), inputType.ToString());
	}

	private void OnDestroy()
	{
		Destroy(keyFace.material);
	}
}