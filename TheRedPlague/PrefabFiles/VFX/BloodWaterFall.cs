using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.VFX;

public class BloodWaterfall
{
    private readonly PrefabInfo _info;
    private readonly string _originalClassId;

    public BloodWaterfall(string classId, string originalClassId)
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
                foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
                {
                    renderer.material.color = new Color(0.619048f, 0, 0, 1);
                }
            }
        });
        prefab.Register();
    }
}