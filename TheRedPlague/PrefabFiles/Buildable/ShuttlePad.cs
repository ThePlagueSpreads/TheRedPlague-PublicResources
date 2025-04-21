using System;
using System.Collections;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using TheRedPlague.MaterialModifiers;
using TheRedPlague.Mono.Buildables.Shuttle;
using TheRedPlague.Mono.Util;
using UnityEngine;
using UnityEngine.UI;
using UWE;
using Object = UnityEngine.Object;

namespace TheRedPlague.PrefabFiles.Buildable;

public static class ShuttlePad
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("ShuttlePad")
        .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("ShuttlePadIcon"));

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(CreatePrefab);
        prefab.SetPdaGroupCategory(TechGroup.ExteriorModules, TechCategory.ExteriorModule);
        prefab.SetRecipe(new RecipeData(new CraftData.Ingredient(
                TechType.TitaniumIngot),
            new CraftData.Ingredient(TechType.ComputerChip),
            new CraftData.Ingredient(TechType.Lubricant)));
        prefab.Register();
        KnownTechHandler.SetAnalysisTechEntry(Info.TechType, Array.Empty<TechType>(),
            KnownTechHandler.DefaultUnlockData.BasicUnlockSound,
            Plugin.AssetBundle.LoadAsset<Sprite>("ShuttlePadPopup"));
    }

    private static IEnumerator CreatePrefab(IOut<GameObject> obj)
    {
        var pad = Object.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("ShuttlePad_Prefab"));
        var padComponent = pad.AddComponent<ShuttlePadBehavior>();
        
        pad.SetActive(false);
        PrefabUtils.AddBasicComponents(pad, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.Global);
        MaterialUtils.ApplySNShaders(pad, 7, 1, 3,
            new DoubleSidedModifier(MaterialUtils.MaterialType.Cutout),
            new IgnoreParticleSystemsModifier());
        var modelTransform = pad.transform.Find("Model");
        var constructable = PrefabUtils.AddConstructable(pad, Info.TechType,
            ConstructableFlags.Outside | ConstructableFlags.Rotatable | ConstructableFlags.AllowedOnConstructable
            | ConstructableFlags.Ground, modelTransform.gameObject);
        constructable.forceUpright = true;
        constructable.placeDefaultDistance = 10;
        constructable.placeMinDistance = 2;
        constructable.placeMaxDistance = 20;
        constructable.allowedUnderwater = false;
        pad.AddComponent<ConstructableBounds>().bounds =
            new OrientedBounds(Vector3.up, Quaternion.identity, new Vector3(9, 0.45f, 9));
        var consolePrefabTask = PrefabDatabase.GetPrefabAsync("0dbfbfbf-23f4-4506-9c49-5db80299d072");
        yield return consolePrefabTask;
        if (!consolePrefabTask.TryGetPrefab(out var consolePrefab))
        {
            throw new Exception("Failed to find console prefab ('0dbfbfbf-23f4-4506-9c49-5db80299d072')!");
        }
        
        var console = Object.Instantiate(consolePrefab, modelTransform);
        console.name = "ShuttleCargoConsole";
        console.transform.localPosition = new Vector3(0, 0.8f, 7);
        Object.DestroyImmediate(console.GetComponent<StoryHandTarget>());
        console.transform.Find("KeypadUI/Icon").GetComponent<Image>().sprite =
            Plugin.AssetBundle.LoadAsset<Sprite>("ShuttleCargoIcon");
        Object.DestroyImmediate(console.GetComponent<PrefabIdentifier>());
        Object.DestroyImmediate(console.GetComponent<LargeWorldEntity>());
        Object.DestroyImmediate(console.GetComponent<GenericConsole>());
        
        var containerObj = new GameObject("CargoStorageContainer");
        containerObj.transform.parent = console.transform;
        containerObj.AddComponent<BoxCollider>();
        containerObj.transform.localPosition = new Vector3(0, 1, 0);
        containerObj.transform.localRotation = Quaternion.identity;
        
        var storageRoot = new GameObject("ItemsRoot");
        storageRoot.transform.SetParent(pad.transform, false);

        var childObjectIdentifier = storageRoot.AddComponent<ChildObjectIdentifier>();
        childObjectIdentifier.ClassId = "ShuttlePadCargo";
    
        var container = containerObj.AddComponent<ShuttlePadStorageContainer>();
        container.prefabRoot = pad;
        container.width = 3;
        container.height = 3;
        container.storageRoot = childObjectIdentifier;
        container.preventDeconstructionIfNotEmpty = true;
        container.storageLabel = "ShuttleCargoScreen";
        container.hoverText = "ShuttleCargoContainerHover";
        container.pad = padComponent;
        
        var neptuneRocketTask = CraftData.GetPrefabForTechTypeAsync(TechType.RocketBase);
        yield return neptuneRocketTask;
        var result = neptuneRocketTask.GetResult();
        var ladderPrefab = result.transform.Find("Base/Triggers/outerLadders1").gameObject;
        var rightLadder = Object.Instantiate(ladderPrefab, modelTransform);
        rightLadder.transform.localPosition = new Vector3(-3.333f, -1, 9);
        rightLadder.transform.localScale = Vector3.one * 0.5f;
        rightLadder.transform.localEulerAngles = Vector3.up * 180;
        var leftLadder = Object.Instantiate(ladderPrefab, modelTransform);
        leftLadder.transform.localPosition = new Vector3(3.333f, -1, 9);
        leftLadder.transform.localScale = Vector3.one * 0.5f;
        leftLadder.transform.localEulerAngles = Vector3.up * 180;

        var shuttlePrefab = pad.transform.Find("ShuttlePrefab").gameObject;
        var shuttleRigidBody = shuttlePrefab.AddComponent<Rigidbody>();
        shuttleRigidBody.useGravity = true;
        shuttleRigidBody.drag = 0.4f;
        shuttleRigidBody.mass = 2000;
        shuttleRigidBody.interpolation = RigidbodyInterpolation.Interpolate;
        var fakeRb = shuttlePrefab.AddComponent<FakeRigidbody>();
        fakeRb.drag = 0.3f;
        var thrusterController = shuttlePrefab.transform.Find("Thrusters").gameObject.AddComponent<ShuttleThrusterController>();
        thrusterController.shuttleRoot = shuttlePrefab.transform;
        thrusterController.fakeRigidbody = fakeRb;
        thrusterController.backThrusters = new[]
        {
            thrusterController.transform.Find("BackThrusterL").gameObject.GetComponent<ParticleSystem>(),
            thrusterController.transform.Find("BackThrusterR").gameObject.GetComponent<ParticleSystem>()
        };
        thrusterController.bottomThrusters = new[]
        {
            thrusterController.transform.Find("BottomThrusterF").gameObject.GetComponent<ParticleSystem>(),
            thrusterController.transform.Find("BottomThrusterB").gameObject.GetComponent<ParticleSystem>()
        };

        var emitter = shuttlePrefab.AddComponent<FMOD_CustomEmitter>();
        emitter.playOnAwake = false;
        emitter.followParent = true;
        emitter.restartOnPlay = true;
        
        var shuttleController = shuttlePrefab.AddComponent<ShuttleController>();
        shuttleController.rb = shuttleRigidBody;
        shuttleController.fakeRigidbody = fakeRb;
        shuttleController.emitter = emitter;

        var explode = shuttlePrefab.AddComponent<ShuttleExplodeOnCollide>();
        explode.rb = fakeRb;
        explode.myCollider = shuttlePrefab.GetComponentInChildren<Collider>(true);

        var fromSpaceVoiceNotification = pad.AddComponent<VoiceNotification>();
        fromSpaceVoiceNotification.text = "PDAReceiveShuttle";
        fromSpaceVoiceNotification.sound = AudioUtils.GetFmodAsset("PDAReceiveShuttle");

        var toSpaceVoiceNotification = pad.AddComponent<VoiceNotification>();
        toSpaceVoiceNotification.text = "PDASendShuttle";
        toSpaceVoiceNotification.sound = AudioUtils.GetFmodAsset("PDASendShuttle");

        var shuttleDestroyedVoiceNotification = pad.AddComponent<VoiceNotification>();
        shuttleDestroyedVoiceNotification.text = "PDAShuttleContactLost";
        shuttleDestroyedVoiceNotification.sound = AudioUtils.GetFmodAsset("PDAShuttleContactLost");
        shuttleDestroyedVoiceNotification.minInterval = 30;
        
        var warnVoiceNotification = pad.AddComponent<VoiceNotification>();
        warnVoiceNotification.text = "ShuttleInvalidItem4";
        warnVoiceNotification.sound = AudioUtils.GetFmodAsset("ShuttleInvalidItem4");

        var terminateContractNotification = pad.AddComponent<VoiceNotification>();
        terminateContractNotification.text = "PDAShuttleContractTerminated";
        terminateContractNotification.sound = AudioUtils.GetFmodAsset("PDAShuttleContractTerminated");

        var itemNotWantedNotification = pad.AddComponent<VoiceNotification>();
        itemNotWantedNotification.text = "ShuttleInvalidItem3";
        itemNotWantedNotification.sound = AudioUtils.GetFmodAsset("PDAShuttleItemNotWanted");

        padComponent.container = container;
        padComponent.shuttlePrefab = shuttlePrefab;
        padComponent.landingPoint = pad.transform.Find("ShuttleLandingTransform");
        padComponent.shuttleToSpaceVoiceNotification = toSpaceVoiceNotification;
        padComponent.shuttleFromSpaceVoiceNotification = fromSpaceVoiceNotification;
        padComponent.shuttleDestroyedVoiceNotification = shuttleDestroyedVoiceNotification;
        padComponent.warnPlayerVoiceNotification = warnVoiceNotification;
        padComponent.contractTerminatedVoiceNotification = terminateContractNotification;
        padComponent.itemNotWantedVoiceNotification = itemNotWantedNotification;
        
        obj.Set(pad);
    }
}