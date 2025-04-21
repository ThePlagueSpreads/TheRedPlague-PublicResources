using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Decorations;

public class FleshBlockPrefab
{
    private readonly string _cloneClassId;
    private readonly TextureSet _side;
    private readonly TextureSet _cap;
    private PrefabInfo Info { get; }

    private static readonly TextureSet[] TextureSets =
    {
        new("Cysts"),
        new("Eyes", m => { m.SetFloat(SideScale, 0.08f); },
            m =>
            {
                m.SetFloat(CapScale, 0.08f);
                m.SetFloat(CapBorderBlendRange, 1f);
                m.SetFloat(CapBorderBlendOffset, -0.716f);
                m.SetFloat(CapBorderBlendAngle, 2.658f);
            }),
        new("Flesh"),
        new("Pustules", m => { m.SetFloat(SideScale, 0.5f); },
            m => { m.SetFloat(CapScale, 0.5f); }),
        new("Skin", m => { m.SetFloat(SideScale, 0.06f); },
            m => { m.SetFloat(CapScale, 0.06f); })
    };

    // Shader property IDs
    private static readonly int CapBorderBlendAngle = Shader.PropertyToID("_CapBorderBlendAngle");
    private static readonly int CapBorderBlendOffset = Shader.PropertyToID("_CapBorderBlendOffset");
    private static readonly int CapBorderBlendRange = Shader.PropertyToID("_CapBorderBlendRange");
    private static readonly int CapSIGMap = Shader.PropertyToID("_CapSIGMap");
    private static readonly int CapBumpMap = Shader.PropertyToID("_CapBumpMap");
    private static readonly int CapTexture = Shader.PropertyToID("_CapTexture");
    private static readonly int SideSIGMap = Shader.PropertyToID("_SideSIGMap");
    private static readonly int SideBumpMap = Shader.PropertyToID("_SideBumpMap");
    private static readonly int SideTexture = Shader.PropertyToID("_SideTexture");
    private static readonly int CapScale = Shader.PropertyToID("_CapScale");
    private static readonly int SideScale = Shader.PropertyToID("_SideScale");

    private FleshBlockPrefab(string classId, string cloneClassId, TextureSet side, TextureSet cap)
    {
        _cloneClassId = cloneClassId;
        _side = side;
        _cap = cap;
        Info = PrefabInfo.WithTechType(classId);
    }

    public static void RegisterAll()
    {
        LoadAllTextureSets();
        // I : side texture index
        for (var i = 0; i < TextureSets.Length; i++)
        {
            // J : cap texture index
            for (var j = 0; j < TextureSets.Length + 1; j++)
            {
                if (i == j) continue;
                var noCap = j == TextureSets.Length;
                var classId = "FleshBlock_" + TextureSets[i] + "_" + (noCap ? "NoCap" : TextureSets[j]);
                var prefab = new FleshBlockPrefab(classId, "fa986d5a-0cf8-4c63-af9f-8c36acd5bea4",
                    TextureSets[i],
                    noCap ? null : TextureSets[j]);
                prefab.Register();
            }
        }
    }

    private void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(new CloneTemplate(Info, _cloneClassId)
        {
            ModifyPrefab = obj =>
            {
                var mesh = obj.GetComponentInChildren<MeshRenderer>();
                var material = mesh.material;
                material.SetTexture(SideTexture, _side.Diffuse);
                material.SetTexture(SideBumpMap, _side.Normal);
                material.SetTexture(SideSIGMap, _side.Sig);
                if (_cap != null)
                {
                    material.SetTexture(CapTexture, _cap.Diffuse);
                    material.SetTexture(CapBumpMap, _cap.Normal);
                    material.SetTexture(CapSIGMap, _cap.Sig);
                    _cap.ApplyCapMaterialOverrides?.Invoke(material);
                }
                else
                {
                    material.SetFloat(CapBorderBlendRange, 0f);
                    material.SetFloat(CapBorderBlendOffset, 0f);
                    material.SetFloat(CapBorderBlendAngle, 0f);
                    material.SetFloat(CapScale, 0f);
                }

                _side.ApplySideMaterialOverrides?.Invoke(material);
                
                obj.transform.GetChild(1).gameObject.SetActive(false);
                var meshColliderObject = new GameObject("MeshCollider");
                meshColliderObject.transform.SetParent(obj.transform);
                var meshCollider = meshColliderObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = Plugin.AssetBundle.LoadAsset<GameObject>("FleshBlockCollision")
                    .GetComponent<MeshFilter>().mesh;
                meshColliderObject.transform.localPosition = Vector3.zero;
                meshColliderObject.transform.localRotation = Quaternion.identity;
                meshColliderObject.transform.localScale = Vector3.one * 100;
                meshColliderObject.layer = LayerID.TerrainCollider;

                obj.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Far;

                obj.EnsureComponent<ConstructionObstacle>();
            }
        });
        prefab.Register();
    }

    private static void LoadAllTextureSets()
    {
        foreach (var t in TextureSets)
        {
            t.LoadTextures();
        }
    }

    private class TextureSet
    {
        public string Name { get; }
        public Texture2D Diffuse { get; private set; }
        public Texture2D Normal { get; private set; }
        public Texture2D Sig { get; private set; }

        public delegate void ApplyMaterialOverrides(Material material);

        public ApplyMaterialOverrides ApplySideMaterialOverrides { get; private set; }
        public ApplyMaterialOverrides ApplyCapMaterialOverrides { get; private set; }

        public TextureSet(string name, ApplyMaterialOverrides applySideMaterialOverrides = null,
            ApplyMaterialOverrides applyCapMaterialOverrides = null)
        {
            Name = name;
            ApplySideMaterialOverrides = applySideMaterialOverrides;
            ApplyCapMaterialOverrides = applyCapMaterialOverrides;
        }

        public void LoadTextures()
        {
            var textures = new[]
            {
                Plugin.AssetBundle.LoadAsset<Texture2D>("FB_" + Name + "Diffuse"),
                Plugin.AssetBundle.LoadAsset<Texture2D>("FB_" + Name + "Normal"),
                Plugin.AssetBundle.LoadAsset<Texture2D>("FB_" + Name + "SIG"),
            };
            foreach (var texture in textures)
            {
                if (texture == null)
                {
                    Plugin.Logger.LogWarning(
                        $"Failed to load one or more texture maps for texture set '{this}'!");
                }
            }

            Diffuse = textures[0];
            Normal = textures[1];
            Sig = textures[2];
        }

        public override string ToString()
        {
            return Name;
        }
    }
}