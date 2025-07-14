using BepInEx;
using mset;
using Nautilus.Handlers.TitleScreen;
using Nautilus.Utility;
using TheRedPlague.Managers;
using UnityEngine;

namespace TheRedPlague;

public static class TrpTitleScreen
{
    public const string TitleScreenLocalizationName = "RedPlagueTitleScreenName";
    
    private static GameObject _overlayLogoInstance;

    public static void RegisterTitleScreenCompatibility(BaseUnityPlugin plugin)
    {
        TitleScreenHandler.RegisterTitleScreenObject(plugin,
            new TitleScreenHandler.CustomTitleData(TitleScreenLocalizationName,
                new WorldObjectTitleAddon(SpawnOverlayLogo),
                new WorldObjectTitleAddon(SpawnRedPlagueLogo),
                new MusicTitleAddon(AudioUtils.GetFmodAsset("ProjectTRP")),
                new SkyChangeTitleAddon(3f, new SkyChangeTitleAddon.Settings(6, 0.2f)))
        );
    }

    private static GameObject SpawnOverlayLogo()
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
        return logo;
    }

    private static GameObject SpawnRedPlagueLogo()
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
        return logo;
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