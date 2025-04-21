using Nautilus.Assets;
using Nautilus.Utility;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.VFX;

public class BloodParticle
{
    private static readonly int Smoothness = Shader.PropertyToID("_Smoothness");
    private static readonly int SpecColor = Shader.PropertyToID("_SpecColor");
    private static readonly int NormalMap = Shader.PropertyToID("_NormalMap");
    private static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
    private readonly PrefabInfo _prefabInfo;
    private readonly string _prefabName;

    public BloodParticle(PrefabInfo prefabInfo, string prefabName)
    {
        _prefabInfo = prefabInfo;
        _prefabName = prefabName;
    }

    public void Register()
    {
        var prefab = new CustomPrefab(_prefabInfo);
        prefab.SetGameObject(GetGameObject);
        prefab.Register();
    }

    private GameObject GetGameObject()
    {
        var obj = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>(_prefabName));
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, _prefabInfo.ClassID, _prefabInfo.TechType, LargeWorldEntity.CellLevel.Medium);
        foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
        {
            var material = renderer.material;
            ConvertShader(material);
        }
        return obj;
    }

    public static void ConvertShader(Material material)
    {
        /* SHADER OVERVIEW:
         * _Color: color
         * _ColorIntensity float
         * _Smoothness: float (0f to 1f)
         * _MainTex tex
         * _NormalMap tex
         */
        
        var smoothness = material.GetFloat(Smoothness);
        material.SetColor(SpecColor, Color.white);
        var normalMap = material.GetTexture(NormalMap);
        MaterialUtils.ApplyUBERShader(material, 1f, 1f, 1f, MaterialUtils.MaterialType.Transparent);
        material.SetTexture(BumpMap, normalMap);
        material.EnableKeyword("MARMO_NORMALMAP");
        material.SetTexture(ShaderPropertyID._SpecTex, material.mainTexture);
        material.SetFloat("_SpecInt", 5);
        material.EnableKeyword("_ZWRITE_ON");
        material.EnableKeyword("MARMO_SPECMAP");
        material.SetColor(ShaderPropertyID._SpecColor, new Color(1f, 1f, 1f, 1f));
        material.SetFloat("_Fresnel", 0.1f);
        material.SetFloat("_IBLreductionAtNight", 0.875f);
        material.SetFloat("_Shininess", smoothness * 8);
    }
}