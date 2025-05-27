using TheRedPlague.PrefabFiles.Equipment;
using UnityEngine;

namespace TheRedPlague.Mono.Equipment;

// This class manages the plague armor model & equip state
public class PlagueArmorBehavior : MonoBehaviour
{
    private static PlagueArmorBehavior _main;
    
    private Transform _parentBone;
    private GameObject _currentArmor;

    private bool _equipped;

    private void Start()
    {
        _parentBone = transform.Find("body/player_view/export_skeleton/head_rig/neck/chest/spine_3/spine_2");
        _main = this;
    }

    public void SetArmorActive(bool state)
    {
        if (state && _currentArmor == null && !GameOptions.GetVrAnimationMode())
        {
            SpawnArmor();
        }
        else if (!state)
        {
            Destroy(_currentArmor);
        }

        _equipped = state;
    }

    private void SpawnArmor()
    {
        _currentArmor = Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("BoneArmor_PlayerModel"), _parentBone, true);
        _currentArmor.transform.localPosition = new Vector3(-0.06f, 0, 0.03f);
        _currentArmor.transform.localEulerAngles = new Vector3(0, 10, 86);
        _currentArmor.transform.localScale = Vector3.one * 0.8f;
        var material = BoneArmor.GetMaterial();

        var renderers = _currentArmor.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.material = material;
        }

        _currentArmor.EnsureComponent<SkyApplier>().renderers = renderers;
    }

    public static bool IsPlagueArmorEquipped()
    {
        if (_main == null) return false;
        return _main._equipped;
    }
}