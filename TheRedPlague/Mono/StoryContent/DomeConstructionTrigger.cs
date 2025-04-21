using System.Collections;
using Story;
using TheRedPlague.PrefabFiles.StoryProps;
using UnityEngine;

namespace TheRedPlague.Mono.StoryContent;

public class DomeConstructionTrigger : MonoBehaviour
{
    private bool _triggered;
    
    private void Update()
    {
        if (!_triggered && Vector3.Distance(transform.position, Player.main.transform.position) < 20)
        {
            Trigger();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        
        if (other.gameObject != Player.main.gameObject)
        {
            return;
        }
        
        Plugin.Logger.LogInfo("Dome construction trigger entered...");

        var story = StoryGoalManager.main;
        
        if (story == null)
        {
            Plugin.Logger.LogError("StoryGoalManager is not initialized!");
            return;
        }

        if (story.IsGoalComplete(StoryUtils.DomeConstructionEvent.key))
        {
            Plugin.Logger.LogError("DomeConstructionEvent was already triggered!");
            return;
        }

        Trigger();
    }

    private void Trigger()
    {
        UWE.CoroutineHost.StartCoroutine(SpawnDome());
        Destroy(gameObject);
        _triggered = true;
    }

    public static IEnumerator SpawnDome()
    {
        yield return new WaitForSeconds(6);
        Plugin.Logger.LogInfo("Beginning dome construction.");
        var domeTask = CraftData.GetPrefabForTechTypeAsync(NewInfectionDome.Info.TechType);
        yield return domeTask;
        var dome = Instantiate(domeTask.GetResult(), Vector3.up * 50, Quaternion.identity);
        dome.SetActive(true);
        dome.transform.localScale = Vector3.one * 900;
    }
}