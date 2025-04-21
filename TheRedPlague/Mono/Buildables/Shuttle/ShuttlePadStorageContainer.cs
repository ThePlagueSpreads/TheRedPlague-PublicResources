using Nautilus.Extensions;
using Nautilus.Utility;
using Story;
using TheRedPlague.Compatibility;
using TheRedPlague.PrefabFiles.Items;
using UnityEngine;
using UnityEngine.UI;

namespace TheRedPlague.Mono.Buildables.Shuttle;

public class ShuttlePadStorageContainer : StorageContainer
{
    public ShuttlePadBehavior pad;

    private const string ShuttleInterfaceParentName = "ShuttleCargo";

    private ResourceHintData[] _hintData;

    private ResourceHintData[] GetResourceHintTypes()
    {
        return new[]
        {
            new ResourceHintData(ModPrefabs.AmalgamatedBone.TechType,
                Plugin.AssetBundle.LoadAsset<Sprite>("AmalgamatedBone")),
            new ResourceHintData(MysteriousRemains.Info.TechType,
                Plugin.AssetBundle.LoadAsset<Sprite>("MysteriousRemainsIcon")),
            new ResourceHintData(ModPrefabs.WarperHeart.TechType,
                Plugin.AssetBundle.LoadAsset<Sprite>("WarperHeartIcon")),
            new ResourceHintData(PlagueCatalyst.Info.TechType,
                Plugin.AssetBundle.LoadAsset<Sprite>("PlagueCrystalIcon")),
            new ResourceHintData(DormantNeuralMatter.Info.TechType,
                Plugin.AssetBundle.LoadAsset<Sprite>("DormantPlagueMatterIcon")),
            new ResourceHintData(PlagueIngot.Info.TechType,
                Plugin.AssetBundle.LoadAsset<Sprite>("PlagueIngotIcon"))
        };
    }

    private void Start()
    {
        _hintData = GetResourceHintTypes();
    }

    public override void Open(Transform useTransform)
    {
        if (pad.IsShuttleActive) return;
        base.Open(useTransform);
        if (open)
        {
            EnableUI();
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        DisableUI();
    }

    private void OnDisable()
    {
        DisableUI();
    }

    private void OnButtonPressed()
    {
        if (container.IsEmpty())
        {
            ErrorMessage.AddMessage(Language.main.Get("ShuttleCargoEmptyWarning"));
            return;
        }

        pad.DeliverCargoToAlterra();
        Player.main.pda.Close();
    }

    private void EnableUI()
    {
        if (!TryGetStorageContainerInventoryTab(out var tabRoot)) return;

        // create interface parent
        var shuttleInterfaceParent = new GameObject(ShuttleInterfaceParentName).EnsureComponent<RectTransform>();
        shuttleInterfaceParent.SetParent(tabRoot);
        shuttleInterfaceParent.localPosition = Vector3.zero;
        shuttleInterfaceParent.localEulerAngles = Vector3.zero;
        shuttleInterfaceParent.localScale = Vector3.one;
        shuttleInterfaceParent.anchorMin = new Vector2(0.5f, 0);
        shuttleInterfaceParent.anchorMax = new Vector2(0.5f, 1f);

        // create the button
        var sendButton = new GameObject("SendCargoButton").AddComponent<Button>();
        var sendButtonRectTransform = sendButton.gameObject.EnsureComponent<RectTransform>();
        sendButtonRectTransform.SetParent(shuttleInterfaceParent);
        sendButtonRectTransform.localEulerAngles = Vector3.zero;
        sendButtonRectTransform.anchorMin = Vector2.zero;
        sendButtonRectTransform.anchorMax = Vector2.zero;
        sendButtonRectTransform.sizeDelta = new Vector2(100, 100);
        sendButtonRectTransform.localScale = Vector3.one;
        sendButtonRectTransform.localPosition =
            new Vector3(0, ModCompatibilityManager.HasAdvancedInventory() ? -300 : -180);
        var sendButtonIcon = sendButton.gameObject.AddComponent<Image>();
        sendButtonIcon.sprite = Plugin.AssetBundle.LoadAsset<Sprite>("UploadCargoIcon");
        sendButton.onClick.AddListener(OnButtonPressed);
        sendButton.image = sendButtonIcon;
        sendButton.image.maskable = false;
        sendButton.gameObject.SetActive(false);
        sendButton.gameObject.SetActive(true);
        var tooltip = sendButton.gameObject.AddComponent<SimpleTooltip>();
        tooltip.text = "PackageShuttleCargoTooltip";

        // create the item hints
        if (ShouldDisplayRequiredItems())
        {
            var itemHintGridObj = new GameObject("ItemHintGrid");
            var itemHintGridTransform = itemHintGridObj.EnsureComponent<RectTransform>();
            itemHintGridTransform.SetParent(shuttleInterfaceParent);
            itemHintGridTransform.localEulerAngles = Vector3.zero;
            itemHintGridTransform.anchorMin = Vector2.zero;
            itemHintGridTransform.anchorMax = Vector2.zero;
            itemHintGridTransform.sizeDelta = new Vector2(200, 200);
            itemHintGridTransform.localScale = Vector3.one;
            itemHintGridTransform.localPosition =
                new Vector3(0, ModCompatibilityManager.HasAdvancedInventory() ? 60 : 180);
            var gridLayout = itemHintGridObj.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(60, 60);
            gridLayout.spacing = new Vector2(5, 5);
        
            foreach (var hint in _hintData)
            {
                var hintObj = new GameObject("ResourceHint-" + hint.ItemType);
                var hintTransform = hintObj.EnsureComponent<RectTransform>();
                hintTransform.SetParent(itemHintGridTransform);
                hintTransform.localEulerAngles = Vector3.zero;
                hintTransform.anchorMin = Vector2.zero;
                hintTransform.anchorMax = Vector2.zero;
                hintTransform.sizeDelta = new Vector2(80, 80);
                hintTransform.localScale = Vector3.one;
                hintTransform.localPosition = Vector3.zero;
                var hintImage = hintObj.AddComponent<Image>();
                hintImage.sprite = hint.Sprite;
                hintImage.raycastTarget = false;
                var obscured =
                    !StoryGoalManager.main.IsGoalComplete(
                        StoryUtils.GetStoryGoalKeyForShuttleDelivery(hint.ItemType.ToString()));
                hintImage.color = obscured ? new Color(0.02f, 0, 0) : Color.white;

                LayoutRebuilder.ForceRebuildLayoutImmediate(itemHintGridTransform);
            }
        }

        if (ModCompatibilityManager.HasAdvancedInventory())
        {
            var mask = tabRoot.GetComponentInParent<RectMask2D>();
            if (mask)
            {
                mask.enabled = false;
            }
        }
    }

    private void DisableUI()
    {
        if (!TryGetStorageContainerInventoryTab(out var tabRoot)) return;
        var shuttleInterface = tabRoot.Find(ShuttleInterfaceParentName);
        if (shuttleInterface != null)
        {
            Destroy(shuttleInterface.gameObject);
        }

        if (ModCompatibilityManager.HasAdvancedInventory())
        {
            var mask = tabRoot.GetComponentInParent<RectMask2D>();
            if (mask)
            {
                mask.enabled = true;
            }
        }
    }

    private bool TryGetStorageContainerInventoryTab(out Transform tabRoot)
    {
        var pdaScreen = uGUI_PDA.main;
        if (pdaScreen == null)
        {
            tabRoot = null;
            return false;
        }

        tabRoot = pdaScreen.transform.SearchChild("StorageContainer");
        return tabRoot != null;
    }

    private bool ShouldDisplayRequiredItems()
    {
        return StoryGoalManager.main.IsGoalComplete(StoryUtils.ChecklistUnlockGoalKey) &&
               !StoryGoalManager.main.IsGoalComplete(StoryUtils.PlagueResearchMission.key);
    }

    private struct ResourceHintData
    {
        public TechType ItemType { get; }
        public Sprite Sprite { get; }

        public ResourceHintData(TechType itemType, Sprite sprite)
        {
            ItemType = itemType;
            Sprite = sprite;
        }
    }
}