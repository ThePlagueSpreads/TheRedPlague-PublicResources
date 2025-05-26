using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TheRedPlague.Mono.InfectionLogic;
using TheRedPlague.Patches;
using UnityEngine;

namespace TheRedPlague.Mono.Insanity;

public class InsanityManager : MonoBehaviour
{
    public static InsanityManager Main { get; private set; }

    public float Insanity { get; private set; }

    private const float UpdateRate = 0.5f;

    private readonly List<InsanitySymptom> _symptoms = new();

    private static readonly Dictionary<string, float> BiomeInfectionPercentages = new()
    {
        { "skyisland", 30f },
        { "dunes", 40f },
        { "mazebase", 50f },
        { "shrinebase_hallway", 55f },
        { "shrinebase_mainroom", 65f },
        { "fleshcave_upper", 60f },
        { "fleshcave_chamber", 60f },
        { "crashedShip", 10f }
    };

    public static void RegisterBiomeInfectionPercentage(string biome, float percentage)
    {
        BiomeInfectionPercentages.Add(biome, percentage);
    }

    public IEnumerator RegisterSymptom(System.Type symptomType)
    {
        var symptomObject = new GameObject(symptomType.Name);
        symptomObject.transform.parent = transform;
        var symptomComponent = (InsanitySymptom)symptomObject.AddComponent(symptomType);
        yield return symptomComponent.LoadAssetsCoroutine(this);
        _symptoms.Add(symptomComponent);
    }

    private IEnumerator Start()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        yield return RegisterDefaultInsanitySymptoms();

        stopwatch.Stop();
        Plugin.Logger.LogDebug($"Loaded {_symptoms.Count} insanity symptoms in {stopwatch.ElapsedMilliseconds}ms.");

        if (_symptoms.Count == 0)
        {
            Plugin.Logger.LogWarning("No insanity symptoms found!");
        }

        InvokeRepeating(nameof(LazyUpdate), Random.value, UpdateRate);

        Main = this;
    }

    private IEnumerator RegisterDefaultInsanitySymptoms()
    {
        var assembly = Plugin.Assembly;
        var insanitySymptomTypes = assembly.GetTypes().Where(t => typeof(InsanitySymptom).IsAssignableFrom(t));
        foreach (var type in insanitySymptomTypes)
        {
            if (!type.IsAbstract)
                yield return RegisterSymptom(type);
        }
    }

    private void LazyUpdate()
    {
        Insanity = CalculatePlayerInsanity();

        var isInsanitySystemActive = IsInsanitySystemActive();

        // Update active symptoms
        foreach (var symptom in _symptoms)
        {
            try
            {
                var active = symptom.ShouldActivate() && isInsanitySystemActive;
                if (active)
                {
                    if (!symptom.IsSymptomActive)
                        symptom.Activate();
                    symptom.DoLazyUpdate(UpdateRate);
                }
                else
                {
                    if (symptom.IsSymptomActive)
                        symptom.Deactivate();
                }
            }
            catch (System.Exception e)
            {
                Plugin.Logger.LogError($"Exception thrown with Symptom '{symptom}': " + e);
            }
        }
    }

    private static bool IsInsanitySystemActive()
    {
        if (Player.main.justSpawned)
            return false;
        return true;
        // This stupid check is reversed
        // return GameModeUtils.IsCheatActive(GameModeOption.Creative);
    }

    // Has a typical range of 0 to 100, but out-of-bounds values are expected
    private float CalculatePlayerInsanity()
    {
        if (PlagueDamageStat.main == null) return 0;
        if (!Plugin.Options.EnableInsanityInCreative && GameModeUtils.IsInvisible()) return 0;
        if (InsanityDeterrenceZone.GetIsInZoneOfDeterrence(Player.main.transform.position, Player.main.IsInBase()))
            return 0;
        return PlagueDamageStat.main.InfectionPercent + GetBiomeInsanityPercentage();
    }

    // Range of 0 to 100
    private float GetBiomeInsanityPercentage()
    {
        var playerBiome = Player.main.GetBiomeString();
        return BiomeInfectionPercentages.TryGetValue(playerBiome, out var value) ? value : 0f;
    }
}