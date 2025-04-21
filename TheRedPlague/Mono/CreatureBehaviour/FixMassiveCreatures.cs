using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour;

public class FixMassiveCreatures : MonoBehaviour
{
    public float desiredLossyScale;

    private void Update()
    {
        if (transform.lossyScale.x > desiredLossyScale)
        {
            var parentScale = GetParentScale();
            transform.localScale = Vector3.one * (desiredLossyScale / parentScale);
        }
    }

    private float GetParentScale()
    {
        if (transform.parent == null) return 1;
        var scale = transform.parent.lossyScale;
        return (scale.x + scale.y + scale.z) / 3;
    }
}