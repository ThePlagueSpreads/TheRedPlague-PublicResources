using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheRedPlague.Mono.VFX;

public class FadeOutOnApproach : MonoBehaviour
{
    public float fadeOutRange = 10f;
    public float fadeDuration = 0.5f;

    private bool _fading;
    private List<Renderer> _fadeRenderers;

    private float _fadeAmount;
    
    private void Start()
    {
        InvokeRepeating(nameof(CheckFadeOut), Random.value, 0.1f);
    }

    private void CheckFadeOut()
    {
        if (_fading) return;
        if (Vector3.SqrMagnitude(MainCamera.camera.transform.position - transform.position) <
            fadeOutRange * fadeOutRange)
        {
            StartFadeOut();
        }
    }

    private void StartFadeOut()
    {
        _fading = true;
        CancelInvoke();
        _fadeRenderers = new List<Renderer>();
        GetComponentsInChildren(_fadeRenderers);
        _fadeAmount = 1f;
        Destroy(gameObject, fadeDuration);
    }

    private void Update()
    {
        if (!_fading)
            return;
        _fadeAmount = Mathf.Clamp01(_fadeAmount - Time.deltaTime / fadeDuration);
        foreach (var renderer in _fadeRenderers)
        {
            if (renderer)
                renderer.SetFadeAmount(_fadeAmount);
        }
    }
}