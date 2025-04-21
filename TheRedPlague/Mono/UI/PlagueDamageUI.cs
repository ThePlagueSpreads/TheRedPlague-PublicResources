using TheRedPlague.Patches;
using TheRedPlague.Patches.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TheRedPlague.Mono.UI;

public class PlagueDamageUI : MonoBehaviour
{
    public static PlagueDamageUI Main
    {
        get
        {
            if (_main == null)
                Create();
            return _main;
        }
    }

    private static PlagueDamageUI _main;

    public Image image;
    public float progressInterpolationRate = 0.3f;
    public GameObject tripleStatsBackground;
    public GameObject quintupleStatsBackground;
    public SuitChargeBar suitChargeBar;

    private Sprite[] _droopProgressSprites;

    private float _progress;
    private float _displayedProgress;

    private void Start()
    {
        LoadSprites();
        SetActiveProgress(0);
        SetActiveFrame(-1);
    }

    public void Refresh()
    {
        var sceneHud = SuitChargeDisplayPatcher.SceneHud;
        if (sceneHud)
        {
            sceneHud.UpdateElements();
        }
        else
        {
            Plugin.Logger.LogWarning("Scene Hud reference not found!");
        }
    }

    public static PlagueDamageUI Create()
    {
        if (uGUI.main == null)
        {
            Plugin.Logger.LogError("PlagueDamageUI.Create: uGUI.main not found!");
            return null;
        }
        var hudParent = uGUI.main.transform.Find("ScreenCanvas/HUD");
        var parent = hudParent.Find("Content/BarsPanel/OxygenBar");

        var root = new GameObject("PlagueDamageUI");
        var rootRect = root.AddComponent<RectTransform>();
        rootRect.SetParent(parent);
        rootRect.localScale = Vector3.one;
        rootRect.localPosition = new Vector3(0, 0, 0);
        rootRect.localRotation = Quaternion.identity;

        var plagueDamageUI = hudParent.gameObject.AddComponent<PlagueDamageUI>();

        var droopObject = new GameObject("Droop");
        var droopImage = droopObject.AddComponent<Image>();
        var droopRect = droopObject.GetComponent<RectTransform>();
        droopRect.SetParent(root.transform);
        droopRect.localScale = Vector3.one * 1.3f;
        droopRect.localPosition = new Vector3(0, -68, 0);
        droopRect.localRotation = Quaternion.identity;

        plagueDamageUI.image = droopImage;

        var barsPanel = hudParent.Find("Content/BarsPanel");

        // Survival stats backgrounds with extra slot for suit charge bar
        plagueDamageUI.tripleStatsBackground =
            CreateNewBarsPanelBackground(barsPanel, "StatsBackgroundTriple", "StatsOutlineTriple");

        plagueDamageUI.quintupleStatsBackground =
            CreateNewBarsPanelBackground(barsPanel, "StatsBackgroundQuintuple", "StatsOutlineQuintuple");
        
        // Suit charge bar
        var waterBar = barsPanel.transform.Find("WaterBar").gameObject;
        var suitChargeBarObject = Instantiate(waterBar, barsPanel);
        suitChargeBarObject.SetActive(false);

        suitChargeBarObject.name = "SuitChargeBar";
        var chargeIcon = Plugin.AssetBundle.LoadAsset<Sprite>("BiochemicalChargeIcon");
        suitChargeBarObject.transform.Find("Icon/Icon").gameObject.GetComponent<Image>().sprite = chargeIcon;
        var suitChargeBar = suitChargeBarObject.AddComponent<SuitChargeBar>();
        plagueDamageUI.suitChargeBar = suitChargeBar;
        DestroyImmediate(suitChargeBarObject.GetComponent<uGUI_WaterBar>());
        // DestroyImmediate(suitChargeBarObject.GetComponent<Animation>());
        var circularBar = suitChargeBarObject.GetComponentInChildren<uGUI_CircularBar>();
        circularBar.color = SuitChargeBar.NormalBarColor;
        circularBar.borderColor = SuitChargeBar.NormalBorderColor;
        suitChargeBar.transform.localPosition += new Vector3(0, -50, 0);
        suitChargeBar.bar = circularBar;

        var waterBarComponent = waterBar.GetComponent<uGUI_WaterBar>();
        suitChargeBar.icon = suitChargeBarObject.transform.Find("Icon").gameObject.GetComponent<RectTransform>();
        suitChargeBar.iconImage = suitChargeBar.icon.GetComponentInChildren<Image>();
        suitChargeBar.text = suitChargeBarObject.transform.Find("Icon/Text").gameObject.GetComponent<TextMeshProUGUI>();
        suitChargeBar.pulseDelayCurve = waterBarComponent.pulseDelayCurve;
        suitChargeBar.pulseTimeCurve = waterBarComponent.pulseTimeCurve;
        suitChargeBar.pulseAnimation = suitChargeBarObject.GetComponent<Animation>();
        suitChargeBar.normalSprite = chargeIcon;
        suitChargeBar.insaneSprite = Plugin.AssetBundle.LoadAsset<Sprite>("InsanityEyeIcon");
        
        _main = plagueDamageUI;
        return plagueDamageUI;
    }

    private static GameObject CreateNewBarsPanelBackground(Transform barsPanelParent, string backgroundName,
        string outlineName)
    {
        var originalBackground = barsPanelParent.Find("BackgroundQuad").gameObject;
        var background = Instantiate(originalBackground, barsPanelParent);
        background.transform.SetSiblingIndex(0);
        background.name = backgroundName;
        background.GetComponent<Image>().sprite = Plugin.AssetBundle.LoadAsset<Sprite>(backgroundName);
        background.transform.GetChild(0).GetComponent<Image>().sprite =
            Plugin.AssetBundle.LoadAsset<Sprite>(outlineName);
        
        var bg = background.GetComponent<RectTransform>();
        var center = background.transform.GetChild(1).GetComponent<RectTransform>();
        var xScale = 1.216f;
        var bgScale = bg.localScale;
        bgScale.Scale(new Vector3(xScale, 1, 1));
        bg.localScale = bgScale;
        bg.localPosition = new Vector2(152f, bg.localPosition.y);
        var centerScale = center.localScale;
        centerScale.Scale(new Vector3(1 / xScale, 1, 1));
        center.localScale = centerScale;
        center.localPosition = new Vector2(11.5f, center.localPosition.y);
        
        background.SetActive(false);
        return background;
    }

    private void LoadSprites()
    {
        _droopProgressSprites = new Sprite[30];
        for (var i = 0; i < _droopProgressSprites.Length; i++)
        {
            _droopProgressSprites[i] = Plugin.AssetBundle.LoadAsset<Sprite>($"PD_{i + 1:00}");
        }
    }

    public void SetActiveProgress(float progress)
    {
        _progress = progress;
    }

    private void Update()
    {
        if (Mathf.Approximately(_progress, _displayedProgress))
        {
            return;
        }

        _displayedProgress =
            Mathf.MoveTowards(_displayedProgress, _progress, progressInterpolationRate * Time.deltaTime);
        SetActiveFrame((int)(_droopProgressSprites.Length * _displayedProgress) - 1);
    }

    private void SetActiveFrame(int frame)
    {
        if (frame < 0)
        {
            image.enabled = false;
        }
        else
        {
            image.enabled = true;
            image.sprite = _droopProgressSprites[frame];
        }
    }
}