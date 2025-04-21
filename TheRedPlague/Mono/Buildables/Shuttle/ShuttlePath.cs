using System.Collections.Generic;
using UnityEngine;

namespace TheRedPlague.Mono.Buildables.Shuttle;

public class ShuttlePath
{
    public ShuttlePath(Point[] points)
    {
        _points = points;
    }

    public IReadOnlyList<Point> Points => _points;

    private readonly Point[] _points;

    public readonly struct Point
    {
        public Vector3 Position { get; }
        public Vector3 DirectionForStartAndEnd { get; }
        public TransitionType Transition { get; }
        public bool DestroyWhenReached { get; init; }

        public Point(Vector3 position, TransitionType transition, Vector3 directionForStartAndEnd = default)
        {
            Position = position;
            Transition = transition;
            DirectionForStartAndEnd = directionForStartAndEnd;
        }
    }

    public enum TransitionType
    {
        Default,
        Ground,
        Space
    }
}