using TheRedPlague.Mono.Util;
using UnityEngine;

namespace TheRedPlague.Mono.Buildables.Shuttle;

public class ShuttleThrusterController : MonoBehaviour
{
    public Transform shuttleRoot;

    public FakeRigidbody fakeRigidbody;

    public ParticleSystem[] backThrusters;
    public ParticleSystem[] bottomThrusters;

    public float minVerticalForce = 0.5f;
    public float maxVerticalForce = 10f;

    public float minHorizontalForce = 1f;
    public float maxHorizontalForce = 40f;

    private float[] _defaultThrusterStrengths;
    private float _horizontalStrength;
    private float _verticalStrength;

    private float _renderedHorizontalStrength;
    private float _renderedVerticalStrength;

    private void Start()
    {
        _defaultThrusterStrengths = new[]
        {
            backThrusters[0].main.startSizeMultiplier,
            backThrusters[1].main.startSizeMultiplier,
            bottomThrusters[0].main.startSizeMultiplier,
            bottomThrusters[1].main.startSizeMultiplier
        };
    }

    private void Update()
    {
        var localVelocity = !fakeRigidbody.enabled ?
            Vector3.zero : shuttleRoot.InverseTransformVector(fakeRigidbody.LastAppliedForce);

        var horizontalMagnitudeSqr = localVelocity.x * localVelocity.x + localVelocity.z * localVelocity.z;

        _horizontalStrength = Mathf.Clamp01(Mathf.InverseLerp(minHorizontalForce, maxHorizontalForce,
            Mathf.Sqrt(horizontalMagnitudeSqr)));
        _verticalStrength =
            Mathf.Clamp01(Mathf.InverseLerp(minVerticalForce, maxVerticalForce, Mathf.Abs(localVelocity.y)));

        _renderedHorizontalStrength =
            Mathf.MoveTowards(_renderedHorizontalStrength, _horizontalStrength, Time.deltaTime);
        _renderedVerticalStrength =
            Mathf.MoveTowards(_renderedVerticalStrength, _verticalStrength, Time.deltaTime);

        for (int i = 0; i < backThrusters.Length; i++)
        {
            
            var main = backThrusters[i].main;
            main.startSizeMultiplier = _defaultThrusterStrengths[i] * _renderedHorizontalStrength;
        }

        for (int i = 0; i < bottomThrusters.Length; i++)
        {
            var main = bottomThrusters[i].main;
            main.startSizeMultiplier = _defaultThrusterStrengths[i + 2] * _renderedVerticalStrength;
        }
    }
}