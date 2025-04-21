using System;
using System.Collections;
using System.Collections.Generic;
using Nautilus.Handlers;
using Nautilus.Json;
using Nautilus.Json.Attributes;
using Nautilus.Utility;
using TheRedPlague.PrefabFiles.Items;
using UnityEngine;

namespace TheRedPlague.Mono.Buildables.PlagueNeutralizer;

public class PlagueNeutralizerMachine : MonoBehaviour
{
    private const float FoodUnitsPerItem = 25;
    private const float FoodUnitsPerSecond = 3f;
    private const float FoodCapacity = 100;
    private const float FoodUsedForIngots = 50;
    private const float IngotCraftDuration = 15;
    private const float CatalystsFoodConsumptionLimit = 100;

    public Animator animator;
    public Texture fabricatorEmissiveTexture;
    public PlagueNeutralizerLiquid liquid;
    public PlagueNeutralizerContainerInterface container;
    public PlagueNeutralizerCatalysts catalysts;
    public GameObject ingotModel;
    public ParticleSystem sparksParticles;
    public FMOD_CustomLoopingEmitter[] buildSounds;
    public GameObject[] beams;

    private InstanceData _data;

    private static readonly SaveData GlobalSaveData = SaveDataHandler.RegisterSaveDataCache<SaveData>();
    private static readonly int WorkingAnimParam = Animator.StringToHash("analyzing");

    private static readonly FMODAsset AddCatalystSound = AudioUtils.GetFmodAsset("PlagueNeutralizerAddCatalyst");

    private float _minBuildingPos = -0.1f;
    private float _maxBuildingPos = 0.3f;

    private float _lastTimeCatalystSoundPlayed;

    private bool _craftAnimationActive;

    private List<Material> _ingotMaterials;

    // Unity events

    private void Start()
    {
        _ingotMaterials = new List<Material>();
        foreach (var renderer in ingotModel.GetComponentsInChildren<Renderer>(true))
        {
            var materials = renderer.materials;
            renderer.materials = materials;
            _ingotMaterials.AddRange(materials);
        }

        foreach (var material in _ingotMaterials)
        {
            FixIngotMaterial(material);
        }

        var id = GetComponent<PrefabIdentifier>().Id;
        _data = GlobalSaveData.GetData(id);
        if (_data.catalystPowers == null || _data.catalystPowers.Length != catalysts.Capacity)
        {
            _data.catalystPowers = new float[catalysts.Capacity];
        }
        
        catalysts.UpdateDisplay(_data.catalystPowers);

        if (_data.craftingIngot)
        {
            StartCraftingAnimation();
        }
    }

    private void Update()
    {
        // Try to eat food
        if (_data.foodAmount < FoodCapacity && GetCatalystsCount() > 0)
        {
            if (TryConsumeFoodFromStorage(Time.deltaTime * FoodUnitsPerSecond, out var foodEaten))
            {
                _data.foodAmount += foodEaten;
                if (GetIndexOfSmallestActiveCatalyst(out var smallestCatalyst))
                {
                    _data.catalystPowers[smallestCatalyst] =
                        Mathf.Clamp01(_data.catalystPowers[smallestCatalyst] -
                                      foodEaten / CatalystsFoodConsumptionLimit);
                    catalysts.UpdateDisplay(_data.catalystPowers);
                }
            }
        }

        liquid.SetDisplayedAmount(_data.foodAmount / FoodCapacity);

        if (_data.craftingIngot)
        {
            if (_data.ingotCraftProgress >= 1f)
            {
                if (_craftAnimationActive)
                    EndCraftingAnimation();
                if (container.container.HasRoomFor(PlagueIngot.Info.TechType))
                    GiveIngot();
            }
            else
            {
                _data.foodAmount -= Time.deltaTime * (FoodUsedForIngots / IngotCraftDuration);
                _data.ingotCraftProgress =
                    Mathf.Clamp01(_data.ingotCraftProgress + Time.deltaTime / IngotCraftDuration);
                SetIngotCraftProgress(_data.ingotCraftProgress);
            }
        }
        else if (_data.foodAmount >= FoodUsedForIngots)
        {
            StartCraftingIngot();
        }
    }

    private void OnDestroy()
    {
        if (_ingotMaterials == null) return;
        foreach (var material in _ingotMaterials)
        {
            Destroy(material);
        }
    }

    // Public methods
    public void AddPlagueCatalyst()
    {
        _data.catalystPowers[GetIndexOfSmallestCatalyst()] = 1;

        catalysts.UpdateDisplay(_data.catalystPowers);

        if (Time.time > _lastTimeCatalystSoundPlayed + 2)
        {
            _lastTimeCatalystSoundPlayed = Time.time;
            Utils.PlayFMODAsset(AddCatalystSound, transform.position);
        }
    }

    public int GetIndexOfSmallestCatalyst()
    {
        var bestIndex = 0;
        var powers = _data.catalystPowers;
        for (var i = 0; i < powers.Length; i++)
        {
            if (powers[i] <= Mathf.Epsilon)
            {
                bestIndex = i;
                break;
            }

            if (powers[i] < powers[bestIndex])
            {
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    public bool GetIndexOfSmallestActiveCatalyst(out int smallestIndex)
    {
        smallestIndex = -1;
        var powers = _data.catalystPowers;
        for (var i = 0; i < powers.Length; i++)
        {
            if (powers[i] <= Mathf.Epsilon)
                continue;

            if (smallestIndex == -1 || powers[i] < powers[smallestIndex])
            {
                smallestIndex = i;
            }
        }

        return smallestIndex >= 0;
    }

    public bool HasSpaceForCatalysts()
    {
        foreach (var power in _data.catalystPowers)
        {
            if (power <= Mathf.Epsilon)
                return true;
        }

        return false;
    }

    public int GetCatalystsCount()
    {
        var count = 0;
        foreach (var power in _data.catalystPowers)
        {
            if (power >= Mathf.Epsilon)
                count++;
        }

        return count;
    }

    // Private methods
    private void FixIngotMaterial(Material material)
    {
        material.EnableKeyword("FX_BUILDING");
        material.SetTexture(ShaderPropertyID._EmissiveTex, fabricatorEmissiveTexture);
        material.SetFloat(ShaderPropertyID._Cutoff, 0.4f);
        material.SetColor(ShaderPropertyID._BorderColor, new Color(2, 0.4f, 0.4f));
        material.SetFloat(ShaderPropertyID._Built, 0f);
        material.SetFloat(ShaderPropertyID._Cutoff, 0.42f);
        material.SetVector(ShaderPropertyID._BuildParams, new Vector4(2f, 0.7f, 3f, -0.25f));
        material.SetFloat(ShaderPropertyID._NoiseStr, 0.25f);
        material.SetFloat(ShaderPropertyID._NoiseThickness, 0.49f);
        material.SetFloat(ShaderPropertyID._BuildLinear, 1f);
        material.SetFloat(ShaderPropertyID._MyCullVariable, 0f);
    }

    private bool TryConsumeFoodFromStorage(float maxFoodToEat, out float foodEaten)
    {
        var storageRoot = container.storageRoot.transform;
        foodEaten = 0f;
        foreach (Transform child in storageRoot)
        {
            var pickupable = child.gameObject.GetComponent<Pickupable>();
            if (pickupable.GetTechType() != PlagueIngot.Info.TechType)
            {
                var scale = child.localScale.x;
                var amountToUse = Mathf.Min(scale, maxFoodToEat - foodEaten);
                foodEaten += amountToUse;
                child.transform.localScale = Vector3.one * (scale - amountToUse / FoodUnitsPerItem);
                if (child.transform.localScale.x <= 0.001f)
                {
                    container.container.RemoveItem(pickupable, true);
                    Destroy(child.gameObject);
                }
            }
        }

        return foodEaten > Mathf.Epsilon;
    }

    private void StartCraftingIngot()
    {
        _data.craftingIngot = true;
        if (!_craftAnimationActive)
            StartCraftingAnimation();
        StoryUtils.PlagueNeutralizerFirstUse.Trigger();
    }

    private void StartCraftingAnimation()
    {
        animator.SetBool(WorkingAnimParam, true);
        ingotModel.SetActive(true);
        SetIngotCraftProgress(0);
        if (MiscSettings.flashes)
        {
            sparksParticles.Play();
        }

        foreach (var sfx in buildSounds)
            sfx.Play();
        
        foreach (var beam in beams)
        {
            beam.SetActive(true);
        }

        _craftAnimationActive = true;
    }

    private void EndCraftingAnimation()
    {
        animator.SetBool(WorkingAnimParam, false);
        ingotModel.SetActive(false);
        sparksParticles.Stop();
        foreach (var sfx in buildSounds)
            sfx.Stop();
        foreach (var beam in beams)
        {
            beam.SetActive(false);
        }

        _craftAnimationActive = false;
    }

    private void GiveIngot()
    {
        StartCoroutine(AddIngotToStorage());
        _data.craftingIngot = false;
        _data.ingotCraftProgress = 0;
    }

    private void SetIngotCraftProgress(float progress)
    {
        foreach (var material in _ingotMaterials)
        {
            material.SetFloat(ShaderPropertyID._Built, progress);
            material.SetFloat(ShaderPropertyID._minYpos, _minBuildingPos + ingotModel.transform.position.y);
            material.SetFloat(ShaderPropertyID._maxYpos, _maxBuildingPos + ingotModel.transform.position.y);
        }
    }

    private IEnumerator AddIngotToStorage()
    {
        var ingotTask = CraftData.GetPrefabForTechTypeAsync(PlagueIngot.Info.TechType);
        yield return ingotTask;
        if (!container.container.HasRoomFor(PlagueIngot.Info.TechType))
        {
            Plugin.Logger.LogWarning("Failed to find room for Plague Ingot in neutralizer! Deleting item.");
            yield break;
        }

        var pickupable = Instantiate(ingotTask.GetResult().GetComponent<Pickupable>());
        var item = new InventoryItem(pickupable);
        pickupable.Pickup();
        container.container.UnsafeAdd(item);
    }

    [FileName("PlagueNeutralizers")]
    private class SaveData : SaveDataCache
    {
        public Dictionary<string, InstanceData> Data { get; set; }

        public InstanceData GetData(string id)
        {
            if (Data == null)
            {
                Load();
                Data ??= new Dictionary<string, InstanceData>();
            }

            if (Data.TryGetValue(id, out var data))
                return data;
            data = new InstanceData();
            Data.Add(id, data);
            return data;
        }
    }

    [Serializable]
    private class InstanceData
    {
        public float foodAmount;
        public float[] catalystPowers;
        public bool craftingIngot;
        public float ingotCraftProgress;

        public InstanceData()
        {
            catalystPowers = Array.Empty<float>();
        }

        public InstanceData(float foodAmount, float[] catalystPowers)
        {
            this.foodAmount = foodAmount;
            this.catalystPowers = catalystPowers;
        }
    }
}