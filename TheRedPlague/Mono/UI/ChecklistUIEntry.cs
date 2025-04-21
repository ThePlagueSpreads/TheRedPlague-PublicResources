using TheRedPlague.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TheRedPlague.Mono.UI;

public class ChecklistUIEntry : MonoBehaviour
{
    public Image checkImage;
    public TextMeshProUGUI text;

    private ChecklistUI _checklistUI;
    private PdaChecklistAPI.ChecklistEntry _entry;

    public void SetUp(ChecklistUI checklistUI, PdaChecklistAPI.ChecklistEntry entry)
    {
        _checklistUI = checklistUI;
        _entry = entry;
        Refresh();
    }

    public void Refresh()
    {
        var titleText = _entry.FormatHandler != null
            ? _entry.FormatHandler.Invoke(_entry)
            : Language.main.Get(_entry.GetNameLanguageKey);
        
        var descriptionText = Language.main.Get(_entry.GetDescLanguageKey);
        
        text.text = $"<b><color=#25e6af>{titleText}</color></b>\n{descriptionText}";

        var isComplete = PdaChecklistAPI.IsEntryCompleted(_entry);
        checkImage.sprite = isComplete ? _checklistUI.checkedSprite : _checklistUI.uncheckedSprite;
    }
}