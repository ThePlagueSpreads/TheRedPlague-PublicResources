using System;
using System.Collections;
using Nautilus.Assets;
using Nautilus.Utility;
using TheRedPlague.Mono.StoryContent.Precursor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheRedPlague.PrefabFiles.StoryProps;

public class CustomItemReceptacle<T> where T : GenericPrecursorReceptacle
{
    private PrefabInfo Info { get; }

    public Action<GameObject, T> ModifyPrefab { get; set; }
    
    public CustomItemReceptacle(PrefabInfo info)
    {
        Info = info;
    }

    public void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(CreatePrefab);
        prefab.Register();
    }
    
    private IEnumerator CreatePrefab(IOut<GameObject> prefab)
    {
        var request = UWE.PrefabDatabase.GetPrefabAsync("63e69987-7d34-41f0-aab9-1187ea06e740");
        yield return request;
        request.TryGetPrefab(out var reference);
        var go = Object.Instantiate(reference.transform.Find("Precursor_Teleporter_Activation_Terminal").gameObject);
        go.SetActive(false);
        PrefabUtils.AddBasicComponents(go, Info.ClassID, Info.TechType,
            LargeWorldEntity.CellLevel.Near);
        var boxCollider = go.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(1.2f, 2f, 1.3f);
        boxCollider.center = Vector3.up;
        var receptacle = go.AddComponent<T>();
        var trigger = new GameObject("Trigger");
        trigger.transform.parent = go.transform;
        trigger.transform.localPosition = Vector3.zero;
        var collider = trigger.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 5;
        trigger.AddComponent<PrecursorKeyTerminalTrigger>();
        trigger.AddComponent<Rigidbody>().isKinematic = true;
        
        ModifyPrefab?.Invoke(go, receptacle);
        
        prefab.Set(go);
    }
}