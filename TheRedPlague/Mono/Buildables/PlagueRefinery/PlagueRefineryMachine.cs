using System.Collections;
using System.Collections.Generic;
using Nautilus.Json;
using Nautilus.Json.Attributes;
using Nautilus.Utility;
using TheRedPlague.Data.BucketLootTable;
using TheRedPlague.PrefabFiles.Items;
using UnityEngine;
using UWE;

namespace TheRedPlague.Mono.Buildables.PlagueRefinery;

public class PlagueRefineryMachine : MonoBehaviour
{
    // Constants
    private const int Version = 1;
    private const float BloodFadeRate = 0.04f;
    private const float InputFallSpeed = 0.08f;
    private const float InputShrinkRate = 0.08f;
    private const float ConveyorBeltMoveSpeed = 0.2f;
    private const float PowerConsumption = 60;
    private const float CraftDurationEstimate = 14;

    private static readonly Color[] EffectColors =
    {
        new(2.4f, 5.5f, 2f),
        new(1.2f, 1, 1),
        new(1, 5, 1),
        new(0.1f, 0.1f, 0.4f)
    };

    // Global save data
    private static SaveData _globalData = new();
    private static Dictionary<TechType, Bucket<TechType>> _defaultGlobalData;

    private static bool IsSaveDataInvalid => _globalData.Buckets != null &&
        _globalData.Buckets.Count != _defaultGlobalData.Count || Version > _globalData.Version;

    // Cached IDs
    private static readonly int Working = Animator.StringToHash("working");
    private static readonly int Open = Animator.StringToHash("open");
    private static readonly int SpecColor = Shader.PropertyToID("_SpecColor");
    private static readonly int SpecTex = Shader.PropertyToID("_SpecTex");

    // Public assignable fields
    public Animator animator;
    public Renderer bloodOverlayRenderer;
    public FMOD_CustomLoopingEmitter workingSound;
    public Renderer[] grinderRenderers;
    public Texture2D grinderBloodDiffuse;
    public Texture2D grinderBloodSpecular;
    public Transform inputSpawnPoint;
    public Transform lootSpawnPoint;
    public Transform lootStopPoint;
    public Transform elevator;

    // Private instance fields
    private bool _busy;
    private PowerRelay _powerRelay;
    private Constructable _constructable;

    private Material _bloodOverlayMaterial;
    private Material[] _grinderMaterials;
    private bool _initialized;
    private float _bloodOverlayStrength;
    private GameObject _inputItemInstance;
    private GameObject _lootItemInstance;
    private bool _lootOnElevator;
    private bool _lootReadyForPickUp;
    private Collider[] _disabledCollidersFromLoot;
    private TechType _currentLootTechType;
    private ItemEffect _currentEffectType;
    private float _startTime;

    #region Saving

    private static void LoadDefaultData()
    {
        if (_defaultGlobalData != null) return;
        _defaultGlobalData = new Dictionary<TechType, Bucket<TechType>>
        {
            {
                TechType.SeaTreaderPoop, new Bucket<TechType>(
                    new List<BucketEntry<TechType>>
                    {
                        new(TechType.Lubricant, 2),
                        new(TechType.Salt, 3),
                        new(TechType.DisinfectedWater, 1),
                        new(TechType.WhiteMushroomSpore, 1),
                        new(TechType.MembrainTreeSeed, 1),
                        new(TechType.PurpleStalkSeed, 1),
                        new(TechType.PurpleBranchesSeed, 1),
                        new(TechType.WhiteMushroomSpore, 1),
                        new(TechType.RedRollPlantSeed, 1),
                        new(new[] { TechType.WhiteMushroom, TechType.AcidMushroom, TechType.SeaCrownSeed }, 1)
                    })
            },
            {
                MysteriousRemains.Info.TechType, new Bucket<TechType>(
                    new List<BucketEntry<TechType>>
                    {
                        new(new[] { ModPrefabs.AmalgamatedBone.TechType, PlagueCatalyst.Info.TechType }, 6),
                        new(DormantNeuralMatter.Info.TechType, 5),
                        new(ModPrefabs.WarperHeart.TechType, 2),
                        new(new[] { TechType.Silver, TechType.Gold, TechType.Copper }, 4),
                        new(new[] { TechType.AluminumOxide, TechType.Diamond, TechType.Lithium }, 2),
                        new(new[] { TechType.Peeper, TechType.Oculus, TechType.Bladderfish }, 3),
                        new(RedPlagueSample.Info.TechType, 1),
                        new(TechType.StalkerTooth, 1)
                    })
            },
            {
                TechType.ScrapMetal, new Bucket<TechType>(
                    new List<BucketEntry<TechType>>
                    {
                        new(new[] { TechType.TitaniumIngot, TechType.PlasteelIngot }, 2),
                        new(TechType.Titanium, 4),
                        new(TechType.CopperWire, 2),
                        new(TechType.Copper, 1),
                        new(TechType.Gold, 3),
                        new(TechType.ComputerChip, 2),
                        new(TechType.WiringKit, 3),
                        new(TechType.AdvancedWiringKit, 1)
                    })
            },
            {
                TechType.BloodOil, new Bucket<TechType>(
                    new List<BucketEntry<TechType>>
                    {
                        new(TechType.Benzene, 1),
                        new(TechType.Lubricant, 1),
                        new(TechType.Silicone, 1)
                    })
            }
        };
    }

    private void InitializeSaveData()
    {
        // If the data exists and is valid, return
        if (!IsSaveDataInvalid)
        {
            return;
        }

        // Load the data
        _globalData.Load();

        // Fix and reset the data if it is still invalid
        if (IsSaveDataInvalid)
        {
            Plugin.Logger.LogWarning("Plague Refinery data was invalid or outdated. Updating and resetting.");
            _globalData.Buckets = _defaultGlobalData;
            _globalData.Version = Version;
        }
    }

    #endregion

    private void Start()
    {
        _bloodOverlayMaterial = bloodOverlayRenderer.material;
        _bloodOverlayStrength = 0;
        _grinderMaterials = new Material[grinderRenderers.Length];
        for (var i = 0; i < grinderRenderers.Length; i++)
            _grinderMaterials[i] = grinderRenderers[i].material;
        LoadDefaultData();
        InitializeSaveData();
        SaveUtils.RegisterOnQuitEvent(OnGameExited);
        SaveUtils.RegisterOnSaveEvent(OnGameSaved);
        _powerRelay = GetComponentInParent<PowerRelay>();
        _constructable = GetComponent<Constructable>();
        _initialized = true;
    }

    private void OnDestroy()
    {
        Destroy(_bloodOverlayMaterial);
        if (_grinderMaterials != null)
        {
            foreach (var material in _grinderMaterials)
            {
                Destroy(material);
            }
        }

        if (!_initialized) return;
        SaveUtils.UnregisterOnQuitEvent(OnGameExited);
        SaveUtils.UnregisterOnSaveEvent(OnGameSaved);
    }

    private void OnGameExited()
    {
        _globalData = new SaveData();
    }

    private void OnGameSaved()
    {
        _globalData.Save();
    }

    public bool IsBusy()
    {
        return _busy;
    }

    public bool InsertItem(TechType itemType)
    {
        if (!_constructable.constructed)
        {
            return false;
        }
        
        if (!_initialized)
        {
            Plugin.Logger.LogWarning("Plague refinery is not yet initialized! Cannot use now.");
            return false;
        }

        if (IsBusy())
            return false;

        if (!HasSufficientPower())
        {
            ErrorMessage.AddMessage(Language.main.GetFormat("TrpMachineInsufficientPower", PowerConsumption));
            return false;
        }
        
        if (TryGetItemFromInventory(itemType))
        {
            if (_powerRelay != null)
                _powerRelay.ConsumeEnergy(PowerConsumption, out _);
            StartCoroutine(UsageCoroutine(itemType));
        }
        else
        {
            ErrorMessage.AddMessage(Language.main.Get("TrpMachineNotEnoughItems"));
        }

        return false;
    }

    public bool HasSufficientPower()
    {
        return !GameModeUtils.RequiresPower() || (_powerRelay && _powerRelay.GetPower() >= PowerConsumption);
    }

    public bool IsItemReadyForPickUp()
    {
        return _lootItemInstance != null && _lootReadyForPickUp;
    }

    public float GetProgress()
    {
        if (!IsBusy())
            return 0f;
        
        if (IsItemReadyForPickUp())
            return 1f;

        return Mathf.Clamp01((Time.time - _startTime) / CraftDurationEstimate) * 0.99f;
    }

    private bool TryGetItemFromInventory(TechType itemType)
    {
        if (!GameModeUtils.RequiresIngredients())
            return true;
        var inventory = Inventory.main;
        var itemsWithTechType = inventory.container.GetItems(itemType);
        return itemsWithTechType is { Count: > 0 } && inventory.TryRemoveItem(itemsWithTechType[0].item);
    }

    private static TechType GetNextItemDrop(TechType inputItem)
    {
        var bucket = _globalData.Buckets[inputItem];
        if (bucket.RemainingDraws == 0)
        {
            bucket.Reset();
        }

        return bucket.DrawEntry();
    }

    private IEnumerator UsageCoroutine(TechType inputItem)
    {
        _busy = true;
        _currentEffectType = GetItemEffect(inputItem);
        _startTime = Time.time;
        yield return SpawnInputObjectModel(inputItem);
        yield return new WaitForSeconds(0.1f);
        animator.SetBool(Working, true);
        workingSound.Play();
        _bloodOverlayStrength = 1;
        SetGrindersBloody();
        yield return new WaitForSeconds(6f);
        animator.SetBool(Working, false);
        workingSound.Stop();
        yield return new WaitForSeconds(0.1f);
        _bloodOverlayStrength = 1;
        yield return new WaitForSeconds(0.1f);
        Destroy(_inputItemInstance);
        var drop = GetNextItemDrop(inputItem);
        StartCoroutine(SpawnLootOutputModel(drop));
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => _lootOnElevator);
        animator.SetBool(Open, true);
        yield return new WaitForSeconds(2.2f);
        AllowLootPickUp();
        yield return new WaitUntil(WasLootPickedUp);
        _lootOnElevator = false;
        _lootItemInstance = null;
        _lootReadyForPickUp = false;
        animator.SetBool(Open, false);
        _busy = false;
    }

    private IEnumerator SpawnInputObjectModel(TechType techType)
    {
        var task = CraftData.GetPrefabForTechTypeAsync(techType);
        yield return task;
        var prefab = task.GetResult();
        var model = UWE.Utils.InstantiateDeactivated(prefab, inputSpawnPoint, Vector3.zero, Quaternion.identity);
        DestroyImmediate(model.GetComponent<PrefabIdentifier>());
        DestroyImmediate(model.GetComponent<TechTag>());
        DestroyImmediate(model.GetComponent<LargeWorldEntity>());
        DestroyImmediate(model.GetComponent<Pickupable>());
        DestroyImmediate(model.GetComponent<WorldForces>());
        if (model.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;
        foreach (var collider in model.GetComponentsInChildren<Collider>())
            collider.enabled = false;
        if (techType == TechType.ScrapMetal)
        {
            model.transform.GetChild(0).localScale *= 0.39f;
        }
        model.SetActive(true);
        _inputItemInstance = model;
    }

    private IEnumerator SpawnLootOutputModel(TechType techType)
    {
        _currentLootTechType = techType;
        var task = CraftData.GetPrefabForTechTypeAsync(techType);
        yield return task;
        var prefab = task.GetResult();
        var model = UWE.Utils.InstantiateDeactivated(prefab, transform, Vector3.zero, Quaternion.identity);
        model.transform.localPosition = lootSpawnPoint.localPosition;
        if (model.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;
        if (model.TryGetComponent<Pickupable>(out var pickupable))
            pickupable.isPickupable = false;
        _disabledCollidersFromLoot = model.GetComponentsInChildren<Collider>();
        foreach (var collider in _disabledCollidersFromLoot)
            collider.enabled = false;
        model.SetActive(true);
        if (model.TryGetComponent<LargeWorldEntity>(out var lwe))
            LargeWorld.main.streamer.cellManager.UnregisterEntity(lwe);
        if (model.TryGetComponent<LiveMixin>(out var lm))
            lm.TakeDamage(1000);
        model.transform.localScale = Vector3.one * 0.5f;
        string classId = null;
        if (model.TryGetComponent<PrefabIdentifier>(out var prefabIdentifier))
            classId = prefabIdentifier.ClassId;
        if (!string.IsNullOrEmpty(classId) && WorldEntityDatabase.TryGetInfo(classId, out var info) && info.prefabZUp)
            model.transform.forward = model.transform.up;
        _lootItemInstance = model;
        
        // lazy fixes for positions
        if (_currentLootTechType == RedPlagueSample.Info.TechType)
            _lootItemInstance.transform.localPosition += Vector3.up * 0.11f;
        else if (_currentLootTechType == DormantNeuralMatter.Info.TechType)
            _lootItemInstance.transform.localPosition += Vector3.up * 0.24f;
        else if (_currentLootTechType == PlagueCatalyst.Info.TechType)
            _lootItemInstance.transform.localScale *= 0.7f;
        else if (_currentLootTechType == TechType.Benzene)
            _lootItemInstance.transform.localScale *= 2f;

        // lazy fix for flying fish
        yield return null;
        if (rb != null)
            rb.isKinematic = true;
    }

    private void AllowLootPickUp()
    {
        if (_lootItemInstance == null)
        {
            Plugin.Logger.LogWarning("Loot item instance was not found!");
            return;
        }

        if (_lootItemInstance.TryGetComponent<Pickupable>(out var pickupable))
            pickupable.isPickupable = true;
        foreach (var collider in _disabledCollidersFromLoot)
        {
            collider.enabled = true;
        }

        _lootReadyForPickUp = true;
    }

    private bool WasLootPickedUp()
    {
        return _lootItemInstance == null || !_lootItemInstance.activeInHierarchy;
    }

    private void SetGrindersBloody()
    {
        foreach (var material in _grinderMaterials)
        {
            material.mainTexture = grinderBloodDiffuse;
            material.SetTexture(SpecTex, grinderBloodSpecular);
            var color = EffectColors[(int)_currentEffectType];
            color = new Color(Mathf.Clamp01(color.r), color.g, color.b);
            material.color = color;
            material.SetColor("_SpecColor", color);
        }
    }

    private void PlaceItemOnElevator()
    {
        _lootItemInstance.transform.parent = elevator;
        _lootItemInstance.transform.localPosition = Vector3.zero;
        _lootOnElevator = true;
        
        if (_currentLootTechType == RedPlagueSample.Info.TechType)
            _lootItemInstance.transform.localPosition += Vector3.forward * 0.002f;
        else if (_currentLootTechType == DormantNeuralMatter.Info.TechType)
            _lootItemInstance.transform.localPosition += Vector3.forward * 0.0018f;
        else if (_currentLootTechType == ModPrefabs.AmalgamatedBone.TechType)
        {
            _lootItemInstance.transform.localPosition += Vector3.up * 0.0002f;
            _lootItemInstance.transform.localEulerAngles = new Vector3(38, 90, 90);
        }
        else switch (_currentLootTechType)
        {
            case TechType.DisinfectedWater:
                _lootItemInstance.transform.localPosition += Vector3.forward * 0.0015f;
                break;
            case TechType.Lubricant:
                _lootItemInstance.transform.localPosition += Vector3.forward * 0.0012f;
                break;
            case TechType.AdvancedWiringKit:
            case TechType.WiringKit:
                _lootItemInstance.transform.localPosition += Vector3.forward * 0.0004f;
                break;
            case TechType.TitaniumIngot:
            case TechType.PlasteelIngot:
                _lootItemInstance.transform.localPosition += Vector3.forward * 0.0013f;
                break;
            case TechType.CopperWire:
                _lootItemInstance.transform.localPosition += Vector3.forward * 0.001f;
                break;
            case TechType.Benzene:
                _lootItemInstance.transform.localPosition += Vector3.forward * 0.0003f;
                break;
            case TechType.Silicone:
                _lootItemInstance.transform.localPosition += Vector3.forward * 0.001f;
                break;
            case TechType.ComputerChip:
                _lootItemInstance.transform.localPosition += Vector3.forward * 0.0008f;
                break;
            default:
            {
                var techTypeName = _currentLootTechType.ToString();
                if (techTypeName.EndsWith("Seed") || techTypeName.EndsWith("Spore"))
                {
                    _lootItemInstance.transform.localPosition += Vector3.forward * 0.0005f;
                }

                break;
            }
        }
    }

    private void Update()
    {
        if (_bloodOverlayStrength > 0)
        {
            _bloodOverlayStrength = Mathf.Clamp01(_bloodOverlayStrength - Time.deltaTime * BloodFadeRate);
            var color = EffectColors[(int)_currentEffectType].WithAlpha(_bloodOverlayStrength);
            _bloodOverlayMaterial.color = color;
            _bloodOverlayMaterial.SetColor(SpecColor, color);
            bloodOverlayRenderer.enabled = true;
        }
        else
        {
            bloodOverlayRenderer.enabled = false;
        }

        if (!_busy)
            return;

        if (_inputItemInstance != null)
        {
            _inputItemInstance.transform.localPosition +=
                new Vector3(0, -InputFallSpeed * Time.deltaTime, 0);
            _inputItemInstance.transform.localScale -= Vector3.one * (InputShrinkRate * Time.deltaTime);
            if (_inputItemInstance.transform.localScale.x <= 0.01f)
                Destroy(_inputItemInstance);
        }

        if (_lootItemInstance != null && !_lootOnElevator)
        {
            _lootItemInstance.transform.localPosition += Vector3.right * (ConveyorBeltMoveSpeed * Time.deltaTime);
            if (_lootItemInstance.transform.localPosition.x >= lootStopPoint.transform.localPosition.x)
            {
                PlaceItemOnElevator();
            }
        }
    }

    private static ItemEffect GetItemEffect(TechType techType)
    {
        if (techType == TechType.ScrapMetal)
            return ItemEffect.Metal;
        if (techType == MysteriousRemains.Info.TechType || techType == TechType.BloodOil)
            return ItemEffect.RedBlood;
        return ItemEffect.Dirty;
    }

    [FileName("PlagueRefineryData")]
    private class SaveData : SaveDataCache
    {
        public Dictionary<TechType, Bucket<TechType>> Buckets { get; set; }
        public int Version { get; set; }
    }

    private enum ItemEffect
    {
        Dirty,
        RedBlood,
        CreatureBlood,
        Metal
    }
}