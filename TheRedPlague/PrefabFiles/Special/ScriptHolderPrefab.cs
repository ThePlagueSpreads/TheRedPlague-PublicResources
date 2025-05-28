using System;
using Nautilus.Assets;
using Nautilus.Utility;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Special;

public class ScriptHolderPrefab
{
    public PrefabInfo Info { get; }

    private LargeWorldEntity.CellLevel _cellLevel;
    private Action<GameObject> _modifyPrefab;

    public ScriptHolderPrefab(PrefabInfo info, LargeWorldEntity.CellLevel cellLevel, Action<GameObject> modifyPrefab)
    {
        _modifyPrefab = modifyPrefab;
        _cellLevel = cellLevel;
        Info = info;
    }

    public void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GetPrefab);
        prefab.Register();
    }

    private GameObject GetPrefab()
    {
        var obj = new GameObject(Info.ClassID);
        obj.SetActive(false);
        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, _cellLevel);
        _modifyPrefab.Invoke(obj);
        return obj;
    }
}