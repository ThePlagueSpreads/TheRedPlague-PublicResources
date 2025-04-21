using HarmonyLib;
using mset;
using Nautilus.Utility;
using TheRedPlague.Managers;
using UnityEngine;

namespace TheRedPlague.Patches.MainMenu;

[HarmonyPatch(typeof(uGUI_MainMenu))]
public class MainMenuPatches
{
    private static int _timesLoaded;

    private static GameObject _overlayLogoInstance;
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(uGUI_MainMenu.Start))]
    public static void StartPostfix()
    {
        if (!Plugin.Options.DisableRedPlagueMenuOverlay)
            SpawnOverlayLogo();
        if (!Plugin.Options.DisableRedPlagueMenuTitle)
            SpawnRedPlagueLogo();
        _timesLoaded++;
        if (_timesLoaded >= 2)
        {
            ErrorMessage.AddMessage("[<color=#FF0000>The Red Plague</color>] Warning: Save corruption is possible for" +
                                    " some mods if you load another save before quitting to desktop!");
        }
    }

    private static void SpawnOverlayLogo()
    {
        var logo = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("RedPlagueOverlayLogoPrefab"));
        var sa = logo.AddComponent<SkyApplier>(); 
        sa.renderers = logo.GetComponentsInChildren<Renderer>(true);
        sa.anchorSky = Skies.Custom;
        sa.SetCustomSky(Object.FindObjectOfType<Sky>());
        logo.transform.position = new Vector3(-25.5f, 1, 40);
        logo.transform.eulerAngles = Vector3.up * 180;
        MaterialUtils.ApplySNShaders(logo);
        UpdateOverlayLogoProgress(logo.transform);
        _overlayLogoInstance = logo;
    }

    private static void SpawnRedPlagueLogo()
    {
        var logo = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("RedPlagueLogoPrefab"));
        var sa = logo.AddComponent<SkyApplier>(); 
        sa.renderers = logo.GetComponentsInChildren<Renderer>();
        sa.anchorSky = Skies.Custom;
        sa.SetCustomSky(Object.FindObjectOfType<Sky>());
        logo.transform.position = new Vector3(5, 6f, 35f);
        logo.transform.eulerAngles = Vector3.up * 180;
        logo.transform.localScale = Vector3.one * 0.6f;
        MaterialUtils.ApplySNShaders(logo);
    }

    private static void UpdateOverlayLogoProgress(Transform parent)
    {
        int activeProgress = GlobalRedPlagueProgressTracker.GetCurrentProgressValue();
        int highestChildIndex = parent.childCount - 1;
        int activeChildIndex = Mathf.Min(activeProgress - 1, highestChildIndex);
        for (int i = 0; i < parent.childCount; i++)
        {
            parent.GetChild(i).gameObject.SetActive(activeChildIndex == i);
        }
    }

    public static void RefreshMainMenu()
    {
        if (_overlayLogoInstance != null)
        {
            UpdateOverlayLogoProgress(_overlayLogoInstance.transform);
        }
    }
}