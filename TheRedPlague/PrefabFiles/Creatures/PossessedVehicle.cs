using System.Collections;
using Nautilus.Assets;
using TheRedPlague.Mono.CreatureBehaviour.Sucker;
using TheRedPlague.Mono.InfectionLogic;
using UnityEngine;

namespace TheRedPlague.PrefabFiles.Creatures;

public class PossessedVehicle
{
    public PrefabInfo Info { get; }
    
    private readonly TechType _vehicleTechType;

    public PossessedVehicle(TechType vehicleTechType)
    {
        Info = PrefabInfo.WithTechType("Possessed" + vehicleTechType);
        _vehicleTechType = vehicleTechType;
    }

    public void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(BuildPrefab);
        prefab.Register();
    }

    private IEnumerator BuildPrefab(IOut<GameObject> prefab)
    {
        var task = CraftData.GetPrefabForTechTypeAsync(_vehicleTechType);
        yield return task;
        var obj = UWE.Utils.InstantiateDeactivated(task.GetResult());
        
        // lock vehicle
        var vehicle = obj.GetComponent<Vehicle>();
        if (vehicle is SeaMoth seaMoth)
        {
            var lights = seaMoth.toggleLights;
            lights.enabled = false;
            lights.lightsOffSound = null;
            lights.lightsOnSound = null;
        }
        var infectedVehicle = obj.AddComponent<InfectedVehicle>();
        obj.AddComponent<SuckerControllerTarget>();
        
        // remove signal/beacon
        Object.DestroyImmediate(obj.GetComponent<PingInstance>());
        
        // remove construct vfx
        Object.DestroyImmediate(obj.GetComponent<VFXConstructing>());
        
        // remove depth crushing
        Object.DestroyImmediate(obj.GetComponent<DepthAlarms>());
        Object.DestroyImmediate(obj.GetComponent<CrushDamage>());
        
        // fix rigidity
        obj.GetComponent<Rigidbody>().isKinematic = false;
        
        // explode when the player or a creature is close
        obj.AddComponent<PossessedVehicleExplode>().vehicle = vehicle;
        
        // remove eco target type (this used to be a 'shark')
        Object.DestroyImmediate(obj.GetComponent<EcoTarget>());

        obj.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Far;
        
        prefab.Set(obj);
    }
}