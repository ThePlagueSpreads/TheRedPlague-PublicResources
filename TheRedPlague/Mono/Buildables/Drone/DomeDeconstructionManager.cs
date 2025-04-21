using System.Collections.Generic;
using Nautilus.Utility;
using Story;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TheRedPlague.Mono.Buildables.Drone;

// Saves progress and follows the player
public class DomeDeconstructionManager : MonoBehaviour
{
    public static DomeDeconstructionManager main;

    private bool _useSpecialOverride;

    public BoidController boidController;

    public float requiredSecondsOfDeconstruction = 1500f;
    
    public Texture deconstructVfxEmissiveTexture;

    public bool Active { get; private set; }

    private GameObject _progressBarObject;

    private HashSet<DroneDeconstructionTarget> _deconstructionTargets = new();

    private readonly DroneDeconstructionSaveData _saveData = new();

    private const string FolderPathForWrecks = "WorldEntities/Environment/Wrecks";

    private float _timeCheckWrecksAgain;
    private readonly float _checkWreckInterval = 7.5f;

    private static readonly List<CustomWreckage> CustomWreckageClassIds = new();

    public static void RegisterCustomWreckageProp(CustomWreckage data)
    {
        CustomWreckageClassIds.Add(data);
    }

    public float CurrentProgress
    {
        get => _saveData.progress;
        private set => _saveData.progress = value;
    }

    public IEnumerable<DroneDeconstructionTarget> GetDeconstructionTargets()
    {
        return _deconstructionTargets;
    }

    public void ContributeProgress()
    {
        if (_useSpecialOverride) return;
        CurrentProgress = Mathf.Clamp01(CurrentProgress + Time.deltaTime / requiredSecondsOfDeconstruction);
        if (CurrentProgress >= 1f && Active)
        {
            OnFinishConstruction();
        }
    }

    public void ApplySpecialOverride()
    {
        _useSpecialOverride = true;
        _progressBarObject.SetActive(false);
    }

    private void Awake()
    {
        main = this;
        _saveData.Load();
        SaveUtils.RegisterOnSaveEvent(OnSave);
    }

    private void Start()
    {
        var goalManager = StoryGoalManager.main;
        if (goalManager.IsGoalComplete(StoryUtils.DronesReadyForDomeConstruction.key))
        {
            return;
        }

        Active = true;
        CreateProgressBar();
        MakeWrecksDeconstructable(false);
        
        Invoke(nameof(TriggerStoryGoal), 1f);
    }

    private void CreateProgressBar()
    {
        _progressBarObject = Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("TadpoleProgressBar"));
        FontUtils.SetFontInChildren(_progressBarObject, FontUtils.Aller_Rg);
        var rectTransform = _progressBarObject.GetComponent<RectTransform>();
        rectTransform.SetParent(uGUI.main.transform.Find("ScreenCanvas"));
        rectTransform.localPosition = new Vector3(0, 350);
        rectTransform.localScale = Vector3.one;
        var bar = _progressBarObject.AddComponent<DroneProgressBar>();
        bar.maskImage = _progressBarObject.transform.Find("Bar").GetComponent<Image>();
        bar.text = _progressBarObject.transform.Find("ProgressPercentText").GetComponent<TextMeshProUGUI>();
        _progressBarObject.transform.Find("Description").GetComponent<TextMeshProUGUI>().text =
            Language.main.Get("DomeDroneDeconstructDescription");
    }

    private void Update()
    {
        if (!Active)
        {
            boidController.transform.position = new Vector3(0, 0, 3000);
            return;
        }

        if (!_useSpecialOverride)
        {
            var targetPos = Player.main.transform.position;
            targetPos.y += 5;
            boidController.transform.position = targetPos;
        }
        
        if (Time.time > _timeCheckWrecksAgain)
        {
            MakeWrecksDeconstructable(true);
        }
    }

    private void OnFinishConstruction()
    {
        Active = false;
        Destroy(_progressBarObject);
        StoryUtils.DronesReadyForDomeConstruction.Trigger();
    }

    private void MakeWrecksDeconstructable(bool repeatsArePossible)
    {
        foreach (var identifier in UniqueIdentifier.identifiers.Values)
        {
            ProcessIdentifierAsPotentiallyDeconstructable(identifier, repeatsArePossible);
        }

        _timeCheckWrecksAgain = Time.time + _checkWreckInterval;
    }

    public void ProcessIdentifierAsPotentiallyDeconstructable(UniqueIdentifier identifier, bool isPossibleRepeat)
    {
        if (identifier == null) return;
        if (identifier is not PrefabIdentifier prefabIdentifier) return;

        var key = prefabIdentifier.prefabKey;

        if (string.IsNullOrEmpty(key)) return;

        var customDeconstructTime = -1f;
        bool hasCustomEntry = false;
        
        foreach (var custom in CustomWreckageClassIds)
        {
            if (custom.PrefabKey == key)
            {
                hasCustomEntry = true;
                customDeconstructTime = custom.DeconstructTime;
                break;
            }
        }
        
        if (!hasCustomEntry && !key.StartsWith(FolderPathForWrecks)) return;

        DroneDeconstructionTarget deconstructionTarget;
        if (isPossibleRepeat)
        {
            deconstructionTarget = identifier.gameObject.EnsureComponent<DroneDeconstructionTarget>();
        }
        else
        {
            deconstructionTarget = identifier.gameObject.AddComponent<DroneDeconstructionTarget>();
        }

        if (customDeconstructTime > 0)
        {
            deconstructionTarget.useCustomDeconstructDuration = true;
            deconstructionTarget.deconstructDuration = customDeconstructTime;
        }
        
        _deconstructionTargets.Add(deconstructionTarget);
        deconstructionTarget.deconstructEmissiveTexture = deconstructVfxEmissiveTexture;
    }

    private void TriggerStoryGoal()
    {
        StoryUtils.OnBuildDrones.Trigger();
    }

    private void OnSave()
    {
        _saveData.Save();
    }

    private void OnDestroy()
    {
        if (CurrentProgress >= 1f)
        {
            Destroy(_progressBarObject, 3f);
        }
        else
        {
            Destroy(_progressBarObject);
        }
        main = null;
    }

    public class CustomWreckage
    {
        public string PrefabKey { get; }
        public float DeconstructTime { get; }

        public CustomWreckage(string prefabKey, float deconstructTime = -1)
        {
            PrefabKey = prefabKey;
            DeconstructTime = deconstructTime;
        }
    }
}