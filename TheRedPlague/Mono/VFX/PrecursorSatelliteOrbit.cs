using Story;
using UnityEngine;

namespace TheRedPlague.Mono.VFX;

public class PrecursorSatelliteOrbit : MonoBehaviour
{
    private static PrecursorSatelliteOrbit _main;

    public Transform ringPivot;
    public float ringRotationAnglesPerSecond = 3;
    public Vector3 offset = new(300, 3100, -1000);

    private const string SpawnAnimationStoryGoal = "PrecursorSatelliteSpawn";
    private const float SpawnAnimationStartDistance = 16000;
    private const float SpawnAnimationDuration = 4f;
    private const float SpawnAnimationCurveExponent = 8;

    private bool _spawnAnimationPlaying;
    private float _spawnAnimationStartTime;

    private void Start()
    {
        if (_main != null)
        {
            Plugin.Logger.LogWarning("Two PrecursorSatellites found in scene. Destroying the duplicate one.");
            Destroy(_main.gameObject);
        }

        _main = this;
        if (StoryGoalManager.main && StoryGoalManager.main.OnGoalComplete(SpawnAnimationStoryGoal))
        {
            PlaySpawnAnimation();
        }
    }

    private void PlaySpawnAnimation()
    {
        _spawnAnimationPlaying = true;
        _spawnAnimationStartTime = Time.time;
    }

    private void Update()
    {
        ringPivot.localEulerAngles += Vector3.up * (ringRotationAnglesPerSecond * Time.deltaTime);
    }

    private void LateUpdate()
    {
        var pos = MainCamera.camera.transform.position + offset;
        pos.y = Mathf.Max(pos.y, Ocean.GetOceanLevel() + offset.y);
        if (_spawnAnimationPlaying)
        {
            if (Time.time > _spawnAnimationStartTime + SpawnAnimationDuration)
            {
                _spawnAnimationPlaying = false;
            }
            else
            {
                pos.x += Mathf.Pow(Mathf.Clamp01(1f - (Time.time - _spawnAnimationStartTime) / SpawnAnimationDuration),
                    SpawnAnimationCurveExponent) * SpawnAnimationStartDistance;
            }
        }

        transform.position = pos;
    }
}