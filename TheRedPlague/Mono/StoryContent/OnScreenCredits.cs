using Nautilus.Utility;
using UnityEngine;

namespace TheRedPlague.Mono.StoryContent;

public class OnScreenCredits : MonoBehaviour
{
    public float speed = 40f;
    
    public static void Play()
    {
        var credits = Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("ScrollingCredits"));
        var rectTransform = credits.GetComponent<RectTransform>();
        rectTransform.SetParent(uGUI.main.transform.Find("ScreenCanvas"));
        rectTransform.localScale = Vector3.one;
        rectTransform.localPosition = new Vector3(-300, -550);
        credits.AddComponent<OnScreenCredits>();
        Utils.PlayFMODAsset(AudioUtils.GetFmodAsset("RedPlagueThemeMusic"), Player.main.transform.position);
    }

    private void Update()
    {
        transform.localPosition += Vector3.up * speed * Time.deltaTime;
    }
}