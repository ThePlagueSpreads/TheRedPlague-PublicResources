using UnityEngine;

namespace TheRedPlague.Mono.Buildables.PlagueNeutralizer;

public class PlagueNeutralizerCatalysts : MonoBehaviour
{
    private const float MaxAlpha = 0.43f;
    
    public GameObject[] models;
    
    private Material[] _materials;
    private bool _isInitialized;

    public int Capacity => models.Length;
    
    private void Start()
    {
        if (!_isInitialized)
            Init();
    }

    private void Init()
    {
        _materials = new Material[models.Length];
        for (var i = 0; i < models.Length; i++)
        {
            _materials[i] = models[i].transform.Find("PlagueCrystal/Crystal").GetComponent<MeshRenderer>().material;
            // models should be deactivated by default
            models[i].SetActive(false);
        }
        _isInitialized = true;
    }

    public void UpdateDisplay(float[] powerData)
    {
        if (!_isInitialized)
            Init();
        for (var i = 0; i < models.Length; i++)
        {
            UpdateMaterial(_materials[i], powerData[i]);
            if (powerData[i] <= Mathf.Epsilon) continue;
            models[i].SetActive(true);
        }
    }

    private void UpdateMaterial(Material material, float power)
    {
        material.color = material.color.WithAlpha(power * MaxAlpha);
    }

    private void OnDestroy()
    {
        if (_materials == null) return;
        foreach (var m in _materials)
        {
            Destroy(m);
        }
    }
}