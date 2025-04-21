using System;
using System.Collections;
using Nautilus.Handlers;
using Nautilus.Utility;
using Story;
using TheRedPlague.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TheRedPlague.Mono.UI;

public class ChecklistUI : MonoBehaviour, IStoryGoalListener
{
    private const string OpenTrpChecklistButtonText = "OpenTrpChecklistButtonText";
    private const string CloseTrpChecklistButtonText = "CloseTrpChecklistButtonText";

    private TextMeshProUGUI _logTabNameText;
    private string _defaultLogTabName;

    private GameObject[] _logModeObjects;
    private GameObject[] _checklistModeObjects;
    private GameObject _checklistButton;
    private TextMeshProUGUI _checklistButtonText;
    private GameObject _entryPrefab;
    private RectTransform _layoutParent;

    public Sprite checkedSprite;
    public Sprite uncheckedSprite;

    private bool _checklistModeActive;

    private bool _shouldRefreshDirty;

    public static void CreateChecklistUI(uGUI_LogTab logTab)
    {
        try
        {
            var checklistUi = logTab.gameObject.AddComponent<ChecklistUI>();
            var contentParent = logTab.transform.Find("Content");

            // Create button
            var checklistButton = Instantiate(contentParent.Find("ButtonSortByDate").gameObject, contentParent);
            checklistButton.SetActive(true);
            checklistButton.name = "TRPChecklistButton";
            checklistButton.GetComponent<Image>().color = new Color(0.5f, 1f, 1f);
            var checklistButtonTransform = checklistButton.GetComponent<RectTransform>();
            var checklistButtonSize = checklistButtonTransform.sizeDelta;
            checklistButtonSize.x = 200;
            checklistButtonTransform.sizeDelta = checklistButtonSize;
            checklistButtonTransform.localPosition = new Vector3(400, 320);
            var buttonTextObject = checklistButtonTransform.GetChild(0).gameObject;
            buttonTextObject.name = "ChecklistButtonText";
            DestroyImmediate(buttonTextObject.GetComponent<Image>());
            var buttonText = buttonTextObject.AddComponent<TextMeshProUGUI>();
            buttonText.text = Language.main.Get("OpenTrpChecklistButtonText");
            buttonText.font = FontUtils.Aller_Rg;
            buttonText.horizontalAlignment = HorizontalAlignmentOptions.Center;
            buttonText.verticalAlignment = VerticalAlignmentOptions.Middle;
            buttonText.fontSize = 20;
            var buttonTextTransform = buttonTextObject.GetComponent<RectTransform>();
            buttonTextTransform.anchorMin = Vector2.zero;
            buttonTextTransform.anchorMax = Vector2.one;
            buttonTextTransform.offsetMin = Vector2.zero;
            buttonTextTransform.offsetMax = Vector2.zero;
            var buttonComponent = checklistButton.AddComponent<Button>();
            buttonComponent.onClick.AddListener(checklistUi.OnChecklistButtonPressed);

            // Create checklist panel
            var checklistPanel = new GameObject("ChecklistPanel");
            checklistPanel.transform.parent = contentParent;
            var checklistPanelTransform = checklistPanel.EnsureComponent<RectTransform>();
            var scrollViewTransform = contentParent.Find("ScrollView").GetComponent<RectTransform>();
            checklistPanelTransform.localScale = scrollViewTransform.localScale;
            checklistPanelTransform.localPosition = scrollViewTransform.localPosition;
            checklistPanelTransform.anchorMin = scrollViewTransform.anchorMin;
            checklistPanelTransform.anchorMax = scrollViewTransform.anchorMax;
            checklistPanelTransform.offsetMin = scrollViewTransform.offsetMin;
            checklistPanelTransform.offsetMax = scrollViewTransform.offsetMax;
            var panelLayoutGroup = checklistPanel.AddComponent<VerticalLayoutGroup>();
            panelLayoutGroup.spacing = 20;
            panelLayoutGroup.padding = new RectOffset(20, 20, 20, 20);
            panelLayoutGroup.childForceExpandHeight = false;
            panelLayoutGroup.childControlHeight = false;
            panelLayoutGroup.childAlignment = TextAnchor.UpperCenter;

            // Create checklist entry prefab
            // Create entry root
            var entryPrefab = new GameObject("ChecklistEntry");
            entryPrefab.SetActive(false);
            var entryTransform = entryPrefab.AddComponent<RectTransform>();
            var entryImage = entryPrefab.AddComponent<Image>();
            entryImage.type = Image.Type.Sliced;
            entryImage.pixelsPerUnitMultiplier = 2;
            entryImage.sprite = checklistButton.GetComponent<Image>().sprite;
            entryTransform.anchorMin = new Vector2(0, 0.5f);
            entryTransform.anchorMax = new Vector2(1, 0.5f);
            entryTransform.offsetMin = new Vector2(0, 50);
            entryTransform.offsetMax = new Vector2(0, 50);
            entryTransform.sizeDelta = new Vector2(500, 100);

            // Create check icon (and its background)
            var checkBackground = Instantiate(logTab.prefabEntry.transform.Find("ButtonContainer").gameObject,
                entryTransform);
            checkBackground.name = "CheckContainer";
            var checkBackgroundTransform = checkBackground.gameObject.EnsureComponent<RectTransform>();
            checkBackgroundTransform.anchorMin = new Vector2(0.07f, 0.5f);
            checkBackgroundTransform.anchorMax = new Vector2(0.07f, 0.5f);
            checkBackgroundTransform.sizeDelta = new Vector2(40, 40);
            DestroyImmediate(checkBackground.GetComponentInChildren<Button>());
            checkBackgroundTransform.Find("Button/Background").GetComponent<Image>().color = new Color(1, 1, 1, 0.3f);
            var check = checkBackgroundTransform.Find("Button/Image").gameObject.GetComponent<Image>();

            // Create text
            var entryTextObject = new GameObject("ChecklistEntryText");
            var entryTextTransform = entryTextObject.AddComponent<RectTransform>();
            entryTextTransform.SetParent(entryTransform);
            entryTextTransform.localScale = Vector3.one;
            entryTextTransform.anchorMin = new Vector2(0.1f, 0);
            entryTextTransform.anchorMax = Vector2.one;
            entryTextTransform.offsetMin = Vector2.zero;
            entryTextTransform.offsetMax = Vector2.zero;

            var entryText = entryTextObject.AddComponent<TextMeshProUGUI>();
            entryText.text = "Placeholder";
            entryText.font = FontUtils.Aller_Rg;
            entryText.horizontalAlignment = HorizontalAlignmentOptions.Left;
            entryText.verticalAlignment = VerticalAlignmentOptions.Middle;
            entryText.fontSize = 20;
            entryText.enableWordWrapping = true;

            var entryComponent = entryPrefab.AddComponent<ChecklistUIEntry>();
            entryComponent.text = entryText;
            entryComponent.checkImage = check;

            // Assign references
            checklistUi._logModeObjects = new[]
                { contentParent.Find("ScrollView").gameObject, contentParent.Find("Scrollbar").gameObject };

            checklistUi._checklistModeObjects = new[] { checklistPanel };

            checklistUi._checklistButton = checklistButton;
            checklistUi._checklistButtonText = buttonText;

            checklistUi._entryPrefab = entryPrefab;
            checklistUi.checkedSprite = Plugin.AssetBundle.LoadAsset<Sprite>("ChecklistChecked");
            checklistUi.uncheckedSprite = Plugin.AssetBundle.LoadAsset<Sprite>("ChecklistUnchecked");
            checklistUi._layoutParent = checklistPanelTransform;

            checklistUi._logTabNameText = contentParent.Find("LogLabel").GetComponent<TextMeshProUGUI>();

            // Ensure proper state
            checklistButton.SetActive(false);

            foreach (var obj in checklistUi._checklistModeObjects)
            {
                obj.SetActive(false);
            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Error when creating checklist UI: " + e);
        }
    }

    public void ForceRefresh()
    {
        _shouldRefreshDirty = true;
    }

    private void RefreshMenu()
    {
        foreach (Transform child in _layoutParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var entry in PdaChecklistAPI.GetChecklistEntries())
        {
            AddEntryContainer(entry);
        }
    }

    private void AddEntryContainer(PdaChecklistAPI.ChecklistEntry entry)
    {
        var entryContainer = Instantiate(_entryPrefab, _layoutParent, false);
        entryContainer.GetComponent<ChecklistUIEntry>().SetUp(this, entry);
        entryContainer.SetActive(true);
    }

    private void OnChecklistButtonPressed()
    {
        _checklistModeActive = !_checklistModeActive;
        foreach (var obj in _logModeObjects)
        {
            obj.SetActive(!_checklistModeActive);
        }

        foreach (var obj in _checklistModeObjects)
        {
            obj.SetActive(_checklistModeActive);
        }

        _checklistButtonText.text =
            Language.main.Get(_checklistModeActive ? CloseTrpChecklistButtonText : OpenTrpChecklistButtonText);

        if (string.IsNullOrEmpty(_defaultLogTabName))
        {
            _defaultLogTabName = _logTabNameText.text;
        }

        _logTabNameText.text = _checklistModeActive ? Language.main.Get("TrpChecklistTabHeader") : _defaultLogTabName;

        StoryGoalManager.main.OnGoalComplete(StoryUtils.ChecklistViewedFirstTimeGoalKey);
        
        if (_checklistModeActive)
        {
            RefreshMenu();
        }
    }

    private IEnumerator Start()
    {
        StoryGoalManager.main.AddListener(this);
        _shouldRefreshDirty = true;
        yield return new WaitUntil(() => StoryGoalManager.main != null && StoryGoalManager.main.initialized);
        if (StoryGoalManager.main.IsGoalComplete(StoryUtils.ChecklistUnlockGoalKey))
        {
            _checklistButton.SetActive(true);
        }

        StoryUtils.ProcessAct2FollowUpMessages();
        RefreshMenu();
    }

    private void Update()
    {
        if (_shouldRefreshDirty)
        {
            _shouldRefreshDirty = false;
            RefreshMenu();
        }
    }

    public void NotifyGoalComplete(string key)
    {
        if (key.Equals(StoryUtils.ChecklistUnlockGoalKey, StringComparison.OrdinalIgnoreCase))
        {
            _checklistButton.SetActive(true);
        }

        AddStoryGoalsFromChecklist();

        if (!StoryUtils.InCreativeMode())
        {
            foreach (var checklistEntry in PdaChecklistAPI.GetChecklistEntries())
            {
                if (checklistEntry.Key == key)
                {
                    DisplayEntryCompletionNotification(checklistEntry);
                }
            }
        }

        _shouldRefreshDirty = true;
    }

    private void AddStoryGoalsFromChecklist()
    {
        foreach (var checklistEntry in PdaChecklistAPI.GetChecklistEntries())
        {
            if (StoryGoalManager.main.IsGoalComplete(checklistEntry.Key))
                continue;

            bool isComplete;
            if (checklistEntry.CustomEntryRequirementsHandler != null)
            {
                isComplete = checklistEntry.CustomEntryRequirementsHandler.Invoke(checklistEntry);
            }
            else
            {
                isComplete = true;
                foreach (var requirement in checklistEntry.RequiredGoals)
                {
                    if (StoryGoalManager.main.IsGoalComplete(requirement)) continue;
                    isComplete = false;
                    break;
                }
            }

            if (isComplete)
            {
                StoryGoalManager.main.OnGoalComplete(checklistEntry.Key);
            }
        }
    }

    private static void DisplayEntryCompletionNotification(PdaChecklistAPI.ChecklistEntry entry)
    {
        var notification = new uGUI_PopupNotification.Entry
        {
            duration = 5,
            title = Truncate(
                entry.FormatHandler != null
                    ? entry.FormatHandler.Invoke(entry)
                    : Language.main.Get(entry.GetNameLanguageKey), 60),
            text = Language.main.Get("TrpChecklistEntryCompletion"),
            sprite = null,
            sound = KnownTechHandler.DefaultUnlockData.BasicUnlockSound
        };
        uGUI_PopupNotification.main.Enqueue(notification);
    }

    private static string Truncate(string value, int maxChars)
    {
        return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
    }
}