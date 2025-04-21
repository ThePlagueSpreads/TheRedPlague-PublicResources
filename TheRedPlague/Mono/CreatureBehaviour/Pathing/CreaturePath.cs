using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.Pathing;

public class CreaturePath : ScriptableObject
{
    [SerializeField]
    private Vector3[] points;

    [SerializeField]
    private float maxDistanceToPoints;
    
    public static CreaturePath Create(Vector3[] points, float maxDistanceToPoints)
    {
        var path = CreateInstance<CreaturePath>();
        path.points = points;
        path.maxDistanceToPoints = maxDistanceToPoints;
        return path;
    }

    public bool GetClosestPoint(Vector3 point, out Vector3 closestPoint)
    {
        var closestDistance = maxDistanceToPoints * maxDistanceToPoints;
        var closestPointIndex = -1;
        
        for (var i = 0; i < points.Length; i++)
        {
            var distance = Vector3.SqrMagnitude(point - points[i]);
            if (distance < closestDistance)
            {
                closestPointIndex = i;
                closestDistance = distance;
            }
        }

        if (closestPointIndex == -1)
        {
            closestPoint = default;
            return false;
        }
        
        closestPoint = points[closestPointIndex];
        return true;
    }

    public bool IsPointInRangeOfPath(Vector3 point)
    {
        var sqr = maxDistanceToPoints * maxDistanceToPoints;
        foreach (var p in points)
        {
            var distance = Vector3.SqrMagnitude(p - point);
            if (distance < sqr) return true;
        }

        return false;
    }
}