using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using TheRedPlague.Mono.InfectionLogic.BloodPool;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.VFX;

public class BloodPool
{
    private readonly PrefabInfo _info;
    private readonly string _originalClassId;

    public BloodPool(string classId, string originalClassId)
    {
        _info = PrefabInfo.WithTechType(classId);
        _originalClassId = originalClassId;
    }

    public void Register()
    {
        var prefab = new CustomPrefab(_info);
        prefab.SetGameObject(new CloneTemplate(_info, _originalClassId)
        {
            ModifyPrefab = obj =>
            {
                obj.GetComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.VeryFar;
                var surface = obj.transform.Find("Surface").gameObject.GetComponent<Renderer>();
                var newColor = new Color(0.338076f, 0, 0, 1);
                surface.material.color = newColor;
                var underSurface = obj.transform.Find("UnderSurface").gameObject.GetComponent<Renderer>();
                underSurface.material.color = newColor;
                var trigger = obj.transform.Find("Trigger").gameObject;
                var brineTrigger = trigger.GetComponent<AcidicBrineDamageTrigger>();
                brineTrigger.enabled = false;
                trigger.AddComponent<BloodPoolDamageTrigger>().box = brineTrigger.box;
                foreach (var atmosphere in obj.GetComponentsInChildren<AtmosphereVolume>())
                {
                    atmosphere.overrideBiome = "fleshcave_bloodpool";
                    atmosphere.priority = 65;
                }
            }
        });
        prefab.Register();
    }
}