using UnityEngine;
using UnityEngine.UI;

namespace TheRedPlague.Mono.VFX;

public class ColorImageInLateUpdate : MonoBehaviour
{
    public Image image;
    public Color newColor;

    private void LateUpdate()
    {
        image.color = new Color(newColor.r, newColor.g, newColor.b, image.color.a);
    }
}