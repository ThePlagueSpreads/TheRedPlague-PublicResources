using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheRedPlague.Mono.CreatureBehaviour.Neural;

public class NeuralGifting : MonoBehaviour, IScheduledUpdateBehaviour
{
    public Transform giftParent;
    public Collider[] colliders;
    public Animator animator;
    public Pickupable pickupable;
    public float findGiftIntervalMin = 60 * 6;
    public float findGiftIntervalMax = 60 * 10;
    // 0: Must be directly behind the camera, 1: must be horizontal to the camera 
    public float offScreenTolerance = 0.7f;

    private bool _loadingGift;
    private Pickupable _gift;

    private double _timeTryAgain;

    private static readonly TechType[] Items =
    {
        TechType.NutrientBlock,
        TechType.DisinfectedWater,
        TechType.FilteredWater,
        TechType.WaterFiltrationSuitWater,
        TechType.Snack1,
        TechType.Snack2,
        TechType.Snack3,
        TechType.SmallMelon
    };

    private static readonly int Holding = Animator.StringToHash("holding");

    private void Start()
    {
        ResetTimer();
    }

    private void OnEnable()
    {
        UpdateSchedulerUtils.Register(this);
        SetHoldingState(_gift != null);
    }

    private void OnDisable()
    {
        UpdateSchedulerUtils.Deregister(this);
    }

    private void GrabRandomGift()
    {
        StartCoroutine(GrabGiftCoroutine(Items[Random.Range(0, Items.Length)]));
    }

    private void ResetTimer()
    {
        _timeTryAgain = DayNightCycle.main.timePassed + Random.Range(findGiftIntervalMin, findGiftIntervalMax);
    }

    private IEnumerator GrabGiftCoroutine(TechType giftType)
    {
        _loadingGift = true;
        var task = CraftData.GetPrefabForTechTypeAsync(giftType);
        yield return task;
        if (task.GetResult() == null)
        {
            Plugin.Logger.LogError($"Failed to load prefab by TechType {giftType}!");
            yield break;
        }
        _loadingGift = false;
        var item = Instantiate(task.GetResult(), giftParent, true);
        item.transform.localPosition = Vector3.zero;
        item.transform.localEulerAngles = new Vector3(-90, 0, 0);

        foreach (var collider in item.GetComponentsInChildren<Collider>())
        {
            foreach (var myCollider in colliders)
            {
                Physics.IgnoreCollision(collider, myCollider);
            }
        }
        
        SetHoldingState(true);
        
        _gift = item.GetComponent<Pickupable>();
        
        var rb = item.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
    }

    private bool IsOffScreen()
    {
        return Vector3.Dot(MainCamera.camera.transform.forward,
            (transform.position - MainCamera.camera.transform.position).normalized) < -1 + offScreenTolerance;
    }

    public string GetProfileTag()
    {
        return "NeuralGifting";
    }

    public void ScheduledUpdate()
    {
        if (_gift != null && _gift.transform.parent != giftParent)
        {
            // Reset the gift reference in case it was referring to one that is now in the player's inventory
            _gift = null;
            SetHoldingState(false);
        }
        
        if (DayNightCycle.main.timePassed < _timeTryAgain)
        {
            return;
        }
        
        // Grab a gift if possible
        if (!_loadingGift && _gift == null && IsOffScreen())
        {
            GrabRandomGift();
            ResetTimer();
        }
    }

    private void SetHoldingState(bool holdingItem)
    {
        animator.SetBool(Holding, holdingItem);
        pickupable.isPickupable = !holdingItem;
    }

    public int scheduledUpdateIndex { get; set; }
}