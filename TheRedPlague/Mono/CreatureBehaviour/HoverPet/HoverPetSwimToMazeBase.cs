using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.HoverPet;

public class HoverPetSwimToMazeBase : CreatureAction
{
    private const float SwimAtSurfaceDistance = 150f;

    private HoverPetBehavior _behavior;

    private void Start()
    {
        _behavior = gameObject.GetComponent<HoverPetBehavior>();
    }

    public override float Evaluate(Creature creature, float time)
    {
        if (_behavior.ShouldSwimTowardsMazeBase)
        {
            return 0.7f;
        }

        return 0f;
    }

    public override void Perform(Creature creature, float time, float deltaTime)
    {
        var closeToPlayer = Vector3.Distance(transform.position, Player.main.transform.position) < 25;
        swimBehaviour.SwimTo(GetTargetPos(), closeToPlayer ? 10 : 3);
    }

    private Vector3 GetTargetPos()
    {
        if (ShouldSwimAtSurface())
        {
            var target2D = new Vector2(HoverPetSpawner.MazeBasePosition.x, HoverPetSpawner.MazeBasePosition.z);
            var direction2D =
                (target2D - new Vector2(transform.position.x, transform.position.z)).normalized * 12;
            var linearTarget = new Vector3(direction2D.x + transform.position.x, -7, direction2D.y + transform.position.z);
            return RandomizePath(linearTarget);
        }

        return HoverPetSpawner.MazeBasePosition;
    }

    private bool ShouldSwimAtSurface()
    {
        var horizontalDistance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z),
            new Vector2(HoverPetSpawner.MazeBasePosition.x, HoverPetSpawner.MazeBasePosition.z));
        return horizontalDistance > SwimAtSurfaceDistance;
    }

    private Vector3 RandomizePath(Vector3 originalPathTarget)
    {
        var angle = Mathf.PerlinNoise(Time.time * AngleRandomizationSpeed + AngleRandomizationOffset,
            Time.time * AngleRandomizationSpeed + AngleRandomizationOffset - 310) * Mathf.PI * 2f;
        var distanceScale = Mathf.Lerp(MinRandomDistance, MaxRandomDistance,
            Mathf.PerlinNoise(Time.time * DistanceRandomizationSpeed + DistanceRandomizationOffset,
                Time.time * DistanceRandomizationSpeed + DistanceRandomizationOffset + 15));
        return originalPathTarget + new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * distanceScale;
    }

    private const float MaxRandomDistance = 8;

    private const float MinRandomDistance = 2;

    private const float AngleRandomizationOffset = -1340;

    private const float AngleRandomizationSpeed = 5.4f;

    private const float DistanceRandomizationSpeed = 2f;

    private const float DistanceRandomizationOffset = -705;
}