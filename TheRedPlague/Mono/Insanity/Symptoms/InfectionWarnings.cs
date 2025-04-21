using System.Collections;

namespace TheRedPlague.Mono.Insanity.Symptoms;

public class InfectionWarnings : InsanitySymptom
{
    private const float MinInsanity = 30;

    protected override IEnumerator OnLoadAssets()
    {
        yield break;
    }

    protected override void OnActivate()
    {
        StoryUtils.InsanityWarning.Trigger();
    }

    protected override void OnDeactivate()
    {
        
    }

    protected override bool ShouldDisplaySymptoms()
    {
        return InsanityPercentage > MinInsanity;
    }
}