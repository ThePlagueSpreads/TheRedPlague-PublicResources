using System.Collections;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using TheRedPlague.MaterialModifiers;
using TheRedPlague.Mono.VFX;
using TheRedPlague.PrefabFiles.Creatures;
using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.Mutants;

public class DeathScare : MonoBehaviour
{
    private static readonly FMODAsset _sound = AudioUtils.GetFmodAsset("CloseJumpScare");
    private float _xOffset = 0f;
    private float _yOffset = 0f;
    private float _zOffset = 0.5f;
    private static DeathScare _current;
    private float _timeSpawned;
    
    public static void PlayDeathScare()
    {
        if (_current != null)
            return;
        UWE.CoroutineHost.StartCoroutine(SpawnDeathScare());
    }
    
    public static void PlayMutantDeathScare(string prefabName, Mutant.Settings settings)
    {
        if (_current != null)
            return;
        var obj = Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>(prefabName));
        MaterialUtils.ApplySNShaders(obj, 4, 2f, 1,
            new DoubleSidedModifier(MaterialUtils.MaterialType.Opaque),
            new ColorMultiplierModifier(2));
        obj.AddComponent<SkyApplier>().renderers = obj.GetComponentsInChildren<Renderer>();
        obj.AddComponent<InfectAnything>().infectionHeightStrength = 0.001f;
        var scare = obj.AddComponent<DeathScare>();
        scare._yOffset = -0.1f;
        if (settings.HasFlag(Mutant.Settings.Large))
        {
            scare._zOffset = 1f;
        }
        else
        {
            scare._zOffset = 0.2f;
        }
        obj.transform.Find("MutatedDiverModel").transform.localEulerAngles = new Vector3(-20, 0, 0);
        var animator = obj.GetComponentInChildren<Animator>();
        if (animator) animator.SetTrigger("jumpscare");
        foreach (var collider in obj.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }
    }

    private static IEnumerator SpawnDeathScare()
    {
        /*
        GameObject obj;
        if (Random.value < 0.5f)
        {
            obj = Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("DeathScare"));
            var scare = obj.AddComponent<DeathScare>();
            MaterialUtils.ApplySNShaders(obj);
            obj.AddComponent<SkyApplier>().renderers = obj.GetComponentsInChildren<Renderer>();
            scare.yOffset = 0f;
            scare.zOffset = 1.2f;
        }
        else
        {
            var task = CraftData.GetPrefabForTechTypeAsync(TechType.Warper);
            yield return task;
            obj = Instantiate(task.GetResult());
            obj.GetComponent<Creature>().enabled = false;
            var scare = obj.AddComponent<DeathScare>();
            scare.yOffset = -1.4f;
            scare.zOffset = 1.3f;
        }
        */
        
        var obj = Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("DeathScare"));
        var scare = obj.AddComponent<DeathScare>();
        MaterialUtils.ApplySNShaders(obj, 4, 2f, 1,
            new DoubleSidedModifier(MaterialUtils.MaterialType.Opaque),
            new ColorMultiplierModifier(2));
        obj.AddComponent<SkyApplier>().renderers = obj.GetComponentsInChildren<Renderer>();
        scare._yOffset = -0.1f;
        scare._zOffset = 1f;
        obj.transform.Find("SharkJumpscare/MutatedDiver3").transform.localEulerAngles = new Vector3(-20, 180, 0);
        // 1% chance
        if (Random.value <= 0.001f)
        {
            // enable shark
            obj.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
            obj.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
        }

        yield break;
    }

    private void Awake()
    {
        _current = this;
        _timeSpawned = Time.time;
    }
    
    private void Start()
    {
        Player.main.liveMixin.invincible = true;
        Invoke(nameof(Kill), 2f);
        Utils.PlayFMODAsset(_sound, Player.main.transform.position);
        MainCameraControl.main.ShakeCamera(0.5f, 6, MainCameraControl.ShakeMode.Quadratic, 1.5f);
        FadingOverlay.PlayFX(new Color(0.5f, 0f, 0f), 0.1f, 0.2f, 0.1f, 0.1f);
    }

    private void LateUpdate()
    {
        var camera = MainCamera.camera.transform;
        var offset = camera.forward * Mathf.Lerp(5, _zOffset, (Time.time - _timeSpawned) * 4)
                     + camera.up * _yOffset
                     + camera.right * _xOffset;
        transform.position = camera.position + offset;
        transform.forward = -camera.forward;
    }

    private void Kill()
    {
        Player.main.liveMixin.invincible = false;
        var damage = Random.Range(60f, 75f);
        var willKillPlayer = Player.main.liveMixin.health <= DamageSystem.CalculateDamage(
            damage, DamageType.Normal, Player.main.gameObject);
        Player.main.liveMixin.TakeDamage(damage);
        FadingOverlay.PlayFX(new Color(0.1f, 0f, 0f), 0.1f, 0.2f, 0.1f);
        Destroy(gameObject, willKillPlayer ? 2f : 0.2f);
    }
}