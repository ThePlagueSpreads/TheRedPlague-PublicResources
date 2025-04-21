using UnityEngine;

namespace TheRedPlague.PrefabFiles.StoryProps.Bases;

public static class AbandonedBaseUtils
{
    private static readonly int SpecInt = Shader.PropertyToID("_SpecInt");

    public static void StripComponents(GameObject gameObject, Color emissionColor, bool darkenSpecular)
    {
        foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>())
        {
            var materials = renderer.materials;
            foreach (var m in materials)
            {
                m.SetColor(ShaderPropertyID._GlowColor, emissionColor);
            }
        }

        foreach (var ghostBase in gameObject.GetComponentsInChildren<DeactivateInGhostBase>())
        {
            ghostBase.gameObject.SetActive(false);
        }
        
        foreach (var baseSurfaceModel in gameObject.GetComponentsInChildren<BaseSurfaceModel>())
        {
            baseSurfaceModel.aboveWaterModel?.SetActive(false);
            baseSurfaceModel.underWaterModel?.SetActive(true);
            Object.DestroyImmediate(baseSurfaceModel);
        }

        var addedTechTag = false;

        switch (gameObject.name)
        {
            case "BaseCorridorCoverIShapeBottomIntOpened(Clone)":
            case "BaseCorridorCoverTShapeBottomIntOpened(Clone)":
                gameObject.transform.Find("collisions").gameObject.SetActive(false);
                break;
            case "BaseCorridorLadderBottom(Clone)":
                gameObject.transform.Find("logic").gameObject.SetActive(false);
                break;
            case "BaseMoonpool(Clone)":
                gameObject.transform.Find("entrance").gameObject.SetActive(false);
                gameObject.transform.Find("Flood_BaseMoonPool").gameObject.SetActive(false);
                var launchBay = gameObject.transform.Find("Launchbay_cinematic");
                foreach (Transform child in launchBay)
                {
                    if (child.gameObject.name != "moon_pool_anim") child.gameObject.SetActive(false);
                }
                break;
            case "BaseObservatory(Clone)":
                Object.DestroyImmediate(gameObject.GetComponent<ObservatoryAmbientSound>());
                gameObject.AddComponent<TechTag>().type = TechType.BaseObservatory;
                addedTechTag = true;
                break;
            case "BaseCorridorHatch(Clone)":
                gameObject.transform.Find("underWater/collisions").gameObject.SetActive(false);
                break;
            case "BaseRoomBioReactor(Clone)":
                gameObject.AddComponent<TechTag>().type = TechType.BaseBioReactorFragment;
                addedTechTag = true;
                break;
            case "BaseRoomNuclearReactor(Clone)":
                gameObject.AddComponent<TechTag>().type = TechType.BaseNuclearReactorFragment;
                addedTechTag = true;
                break;
            case "BaseWaterParkTop(Clone)":
            case "BaseWaterParkBottom(Clone)":
                gameObject.AddComponent<TechTag>().type = TechType.BaseWaterPark;
                addedTechTag = true;
                break;
        }

        if (!addedTechTag && gameObject.name.StartsWith("BaseRoom"))
        {
            gameObject.AddComponent<TechTag>().type = TechType.BaseRoom;
            addedTechTag = true;
        }
        
        Object.DestroyImmediate(gameObject.GetComponentInChildren<BaseLadder>());

        // Remove sky appliers
        
        var skyApplierComponents = gameObject.GetComponents<SkyApplier>();
        foreach (var skyApplier in skyApplierComponents)
        {
            Object.DestroyImmediate(skyApplier);
        }

        if (darkenSpecular)
        {
            var renderers = gameObject.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    if (material.HasProperty(SpecInt))
                        material.SetFloat(SpecInt, material.GetFloat(SpecInt) * 0.05f);
                }
            }
        }
    }
}