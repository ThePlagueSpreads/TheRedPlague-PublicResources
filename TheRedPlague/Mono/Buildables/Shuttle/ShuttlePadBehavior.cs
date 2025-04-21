using System;
using System.Collections;
using Story;
using TheRedPlague.PrefabFiles.Items;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheRedPlague.Mono.Buildables.Shuttle;

public class ShuttlePadBehavior : MonoBehaviour
{
    public ShuttlePadStorageContainer container;
    public GameObject shuttlePrefab;
    public Transform landingPoint;
    public float spawnHeight = 2200;
    private Vector3 _domeLandingPosition = new Vector3(111, 2112, 1);
    private Vector3 _domeDodgePosition = new Vector3(227, 2138, 14);

    private TechType[] _plagueResourceTechTypes;

    public VoiceNotification shuttleFromSpaceVoiceNotification;
    public VoiceNotification shuttleToSpaceVoiceNotification;
    public VoiceNotification shuttleDestroyedVoiceNotification;
    public VoiceNotification warnPlayerVoiceNotification;

    public VoiceNotification contractTerminatedVoiceNotification;

    // For act 2 and onward (less harsh)
    public VoiceNotification itemNotWantedVoiceNotification;

    private ShuttleController _currentShuttle;

    private bool _warnedPlayer;

    public bool IsShuttleActive { get; private set; }

    private void Awake()
    {
        _plagueResourceTechTypes = new[]
        {
            ModPrefabs.AmalgamatedBone.TechType,
            ModPrefabs.WarperHeart.TechType,
            PlagueCatalyst.Info.TechType,
            DormantNeuralMatter.Info.TechType,
            MysteriousRemains.Info.TechType,
            PlagueIngot.Info.TechType
        };
    }

    public void PlayShuttleAnimation(ShuttlePath path)
    {
        if (_currentShuttle == null)
        {
            _currentShuttle = Instantiate(shuttlePrefab).GetComponent<ShuttleController>();
            _currentShuttle.gameObject.SetActive(true);
        }

        _currentShuttle.SetPath(path, this);
        IsShuttleActive = true;
    }

    public void DeliverCargoToAlterra()
    {
        StartCoroutine(PlayFullShuttleAnimationCoroutine());
    }

    private IEnumerator PlayFullShuttleAnimationCoroutine()
    {
        yield return CallShuttleFromSpace();
        yield return new WaitForSeconds(8);
        CommentOnCargo();
        yield return new WaitUntil(() => _currentShuttle == null || _currentShuttle.HasLanded);
        yield return new WaitForSeconds(5);
        if (_currentShuttle == null)
        {
            yield break;
        }

        PackageItemsIntoCargo();

        yield return SendShuttleBack();
    }

    private IEnumerator CallShuttleFromSpace()
    {
        PlayShuttleAnimation(DomeIsPresent() ? GetPathFromDomeToPad() : GetPathFromSpaceToPad());
        yield return new WaitForSeconds(2);
        shuttleFromSpaceVoiceNotification.Play();
    }

    private IEnumerator SendShuttleBack()
    {
        PlayShuttleAnimation(DomeIsPresent() ? GetPathFromPadToDome() : GetPathFromPadToSpace());

        if (StoryGoalManager.main.IsGoalComplete(StoryUtils.SendShuttleFirstTime.key))
        {
            shuttleToSpaceVoiceNotification.Play();
        }
        else
        {
            StoryUtils.SendShuttleFirstTime.Trigger();
        }

        IsShuttleActive = false;
        yield break;
    }

    private bool DomeIsPresent()
    {
        return StoryGoalManager.main.IsGoalComplete(StoryUtils.DomeConstructionEvent.key);
    }

    public void PackageItemsIntoCargo()
    {
        var itemsContainer = container.container;
        var itemTypes = itemsContainer.GetItemTypes();

        if (itemTypes.Contains(RedPlagueSample.Info.TechType))
        {
            StoryUtils.SendPlagueSampleViaShuttleEvent.Trigger();
        }

        if (itemTypes.Contains(ModPrefabs.BananaInfo.TechType))
        {
            StoryUtils.BananaphobiaGoal.Trigger();
        }

        foreach (var plagueResource in _plagueResourceTechTypes)
        {
            if (itemTypes.Contains(plagueResource))
            {
                StoryGoalManager.main.OnGoalComplete(
                    StoryUtils.GetStoryGoalKeyForShuttleDelivery(plagueResource.ToString()));
            }
        }

        itemsContainer.Clear();
    }

    private void CommentOnCargo()
    {
        if (HasNoDesiredItems(container.container))
        {
            PlayInvalidItemEvent();
        }
    }

    private void PlayInvalidItemEvent()
    {
        var story = StoryGoalManager.main;
        if (!story.IsGoalComplete(StoryUtils.ShuttleInvalidItem1.key))
        {
            StoryUtils.ShuttleInvalidItem1.Trigger();
        }
        else if (!story.IsGoalComplete(StoryUtils.ShuttleInvalidItem2.key))
        {
            StoryUtils.ShuttleInvalidItem2.Trigger();
        }
        else if (!story.IsGoalComplete(StoryUtils.ShuttleInvalidItem3.key))
        {
            StoryUtils.ShuttleInvalidItem3.Trigger();
        }
        // For the beginning of the game only, troll the player a bit
        else if (!StoryGoalManager.main.IsGoalComplete(StoryUtils.SendPlagueSampleViaShuttleEvent.key)
                 && !StoryUtils.IsAct1Complete())
        {
            if (!_warnedPlayer)
            {
                warnPlayerVoiceNotification.Play();
                _warnedPlayer = true;
            }
            else
            {
                contractTerminatedVoiceNotification.Play();
                Invoke(nameof(CutToCredits), 6);
            }
        }
        // For act 2 and onward, just say something along the lines of "I don't think anyone wanted that"
        else
        {
            itemNotWantedVoiceNotification.Play();
        }
    }

    private void CutToCredits()
    {
        AddressablesUtility.LoadSceneAsync("EndCreditsSceneCleaner", LoadSceneMode.Single);
    }

    private bool HasNoDesiredItems(ItemsContainer itemsContainer)
    {
        var itemsList = itemsContainer.GetItemTypes();
        if (itemsList == null || itemsList.Count == 0) return true;

        var result = true;

        foreach (var type in itemsList)
        {
            if (type == RedPlagueSample.Info.TechType)
            {
                result = false;
            }
            
            if (type == ModPrefabs.BananaInfo.TechType)
            {
                result = false;
            }

            foreach (var plagueResource in _plagueResourceTechTypes)
            {
                if (type == plagueResource)
                {
                    result = false;
                }
            }
        }

        return result;
    }

    private ShuttlePath GetPathFromSpaceToPad()
    {
        return new ShuttlePath(new ShuttlePath.Point[]
        {
            new(new Vector3(0, spawnHeight, 0), ShuttlePath.TransitionType.Space, Vector3.down),
            new(landingPoint.position + new Vector3(0, 100, 0), ShuttlePath.TransitionType.Default),
            new(landingPoint.position, ShuttlePath.TransitionType.Ground, landingPoint.forward)
        });
    }

    private ShuttlePath GetPathFromPadToSpace()
    {
        return new ShuttlePath(new ShuttlePath.Point[]
        {
            new(landingPoint.position, ShuttlePath.TransitionType.Ground, _currentShuttle.transform.forward),
            new(landingPoint.position + new Vector3(0, 60, 0), ShuttlePath.TransitionType.Default),
            new(new Vector3(0, spawnHeight, 0), ShuttlePath.TransitionType.Space, Vector3.up)
        });
    }

    private ShuttlePath GetPathFromDomeToPad()
    {
        return new ShuttlePath(new ShuttlePath.Point[]
        {
            new(_domeLandingPosition, ShuttlePath.TransitionType.Ground, Vector3.right),
            new(_domeDodgePosition, ShuttlePath.TransitionType.Default, Vector3.down),
            new(landingPoint.position + new Vector3(0, 100, 0), ShuttlePath.TransitionType.Default),
            new(landingPoint.position, ShuttlePath.TransitionType.Ground, landingPoint.forward)
        });
    }

    private ShuttlePath GetPathFromPadToDome()
    {
        return new ShuttlePath(new ShuttlePath.Point[]
        {
            new(landingPoint.position, ShuttlePath.TransitionType.Ground, _currentShuttle.transform.forward),
            new(landingPoint.position + new Vector3(0, 60, 0), ShuttlePath.TransitionType.Default),
            new(_domeDodgePosition, ShuttlePath.TransitionType.Default, Vector3.left),
            new(_domeLandingPosition, ShuttlePath.TransitionType.Ground, Vector3.left)
            {
                DestroyWhenReached = true
            }
        });
    }

    public void DestroyShuttle()
    {
        IsShuttleActive = false;
        Destroy(_currentShuttle);
    }

    private void Update()
    {
        if (IsShuttleActive && _currentShuttle == null)
        {
            IsShuttleActive = false;
            shuttleDestroyedVoiceNotification.Play();
        }
    }
}