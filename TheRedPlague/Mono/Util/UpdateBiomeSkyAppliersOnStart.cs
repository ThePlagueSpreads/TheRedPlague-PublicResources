using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheRedPlague.Mono.Util;

// This class re-applies Sky Appliers to all objects in the world with the given Sky when Start is called. Expensive, so do not overuse this!
public class UpdateBiomeSkyAppliersOnStart : MonoBehaviour
{
    // The positional offset from the center of the GameObject to be used for finding the skies to convert
    public Vector3 offsetForExteriorPosition;

    public bool updateEverySky;

    public void Start()
    {
        if (SkyApplierUpdater.main == null) return;

        IEnumerable<SkyApplier> skyAppliersToUpdate;
        if (updateEverySky)
        {
            var listOfAllSkyAppliers = new List<SkyApplier>();
            
            foreach (var biomeSky in WaterBiomeManager.main.biomeSkies)
            {
                if (!biomeSky) continue;
                foreach (var applier in biomeSky.skyAppliers)
                {
                    if (applier)
                        listOfAllSkyAppliers.Add(applier);
                }
            }

            skyAppliersToUpdate = listOfAllSkyAppliers;
        }
        else
        {
            var environmentSky =
                WaterBiomeManager.main.GetBiomeEnvironment(transform.position + offsetForExteriorPosition);
            skyAppliersToUpdate = environmentSky?.skyAppliers;
        }

        if (skyAppliersToUpdate == null)
        {
            Plugin.Logger.LogError("Warning: sky appliers are null (UpdateBiomeSkyAppliersOnStart)");
            return;
        }

        int countForDebugPurposes = 0;
        foreach (var applier in skyAppliersToUpdate)
        {
            if (applier == null) continue;
            if (applier.anchorSky != Skies.Auto) continue;
            UpdateSkyApplier(applier);
            countForDebugPurposes++;
        }

        Plugin.Logger.LogMessage($"Updated {countForDebugPurposes} SkyAppliers for " + this);
    }

    private void UpdateSkyApplier(SkyApplier applier)
    {
        applier.ApplySkybox();
    }
}