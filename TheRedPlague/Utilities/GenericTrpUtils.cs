using UnityEngine;

namespace TheRedPlague.Utilities;

public static class GenericTrpUtils
{
    public static bool IsPositionOnScreen(Vector3 position, float threshold = -0.1f)
    {
        var camTransform = MainCamera.camera.transform;
        return Vector3.Dot(camTransform.forward, (position - camTransform.position).normalized) > threshold;
    }
    
    public static bool TryGetSpawnPositionBehindPlayer(out Vector3 spawnLocation, float spawnDistance, int maxTries)
    {
        for (var i = 0; i < maxTries; i++)
        {
            spawnLocation = Player.main.transform.position + Random.onUnitSphere * spawnDistance;
            if (spawnLocation.y > Ocean.GetOceanLevel())
                continue;
            if (IsPositionOnScreen(spawnLocation))
                continue;
            var camToSpawnPos = spawnLocation - MainCamera.camera.transform.position;
            var distance = camToSpawnPos.magnitude;
            if (!Physics.Raycast(MainCamera.camera.transform.position, camToSpawnPos / distance, distance,
                    Voxeland.GetTerrainLayerMask()))
            {
                return true;
            }
        }
        spawnLocation = default;
        return false;
    }
    
    public static bool TryGetSpawnPositionOnGround(out Vector3 pos, float radius, int maxIterations, float minDistanceFromPlayer)
    {
        if (Player.main.IsInBase())
        {
            for (int i = 0; i < maxIterations; i++)
            {
                // not gonna fix this typo sorry
                var angel = Random.value * 2 * Mathf.PI;
                var raycastStartPos =
                    Player.main.transform.position + new Vector3(Mathf.Cos(angel), 0, Mathf.Sin(angel)) * Random.Range(5, 25) + Vector3.up;
                if (Physics.Raycast(raycastStartPos, Vector3.down, out var hit, 3, -1, QueryTriggerInteraction.Ignore))
                {
                    if (!IsPositionOnScreen(hit.point))
                    {
                        pos = hit.point;
                        return true;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < maxIterations; i++)
            {
                var raycastStartPos =
                    Player.main.transform.position + (Random.onUnitSphere * radius) + Vector3.up * radius;
                if (Physics.Raycast(raycastStartPos, Vector3.down, out var hit, 60, -1, QueryTriggerInteraction.Ignore))
                {
                    if (!IsPositionOnScreen(hit.point) && Vector3.SqrMagnitude(MainCamera.camera.transform.position - hit.point) > minDistanceFromPlayer * minDistanceFromPlayer)
                    {
                        pos = hit.point;
                        return true;
                    }
                }
            }
        }

        pos = default;
        return false;
    }
    
    public static GameObject GetTargetRoot(Collider childCollider, bool separateLiving = true, bool checkForIdentifiers = false)
    {
        if (separateLiving)
        {
            var liveMixin = childCollider.GetComponent<LiveMixin>();
            if (liveMixin != null)
                return childCollider.gameObject;   
        }
        if (childCollider.attachedRigidbody)
            return childCollider.attachedRigidbody.gameObject;
        if (checkForIdentifiers)
        {
            var identifier = childCollider.gameObject.GetComponentInParent<UniqueIdentifier>();
            if (identifier)
                return identifier.gameObject;
        }
        return childCollider.gameObject;
    }

    public static Bounds GetObjectBounds(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(obj.transform.position, Vector3.zero);
        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }
}