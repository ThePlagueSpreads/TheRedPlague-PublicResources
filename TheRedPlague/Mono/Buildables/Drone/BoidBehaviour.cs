//
// Boids - Flocking behavior simulation.
//
// Copyright (C) 2014 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using UnityEngine;

namespace TheRedPlague.Mono.Buildables.Drone;

public class BoidBehaviour : MonoBehaviour
{
    private BoidController _controller;

    private float _noiseOffset;
    private float _timeScanAgain;
    private bool _avoiding;
    private Vector3 _lastAvoidanceDirection;
    private float _timeAvoidanceStarted;
    
    public BoidController GetController() => _controller;

    // Calculates the separation vector with a target.
    private Vector3 GetSeparationVector(Transform target)
    {
        var diff = transform.position - target.transform.position;
        var diffLen = diff.magnitude;
        var scaler = Mathf.Clamp01(1.0f - diffLen / _controller.neighborDist);
        return diff * (scaler / diffLen);
    }

    private void Start()
    {
        _noiseOffset = Random.value * 10.0f;
        if (_controller == null)
        {
            enabled = false;
        }
    }

    public int RegisterIntoController(BoidController controller)
    {
        _controller = controller;
        enabled = true;
        return _controller.Register(this);
    }

    public void UnregisterFromController()
    {
        if (_controller)
        {
            _controller.Boids.Remove(this);   
        }
        enabled = false;
    }

    private void OnDestroy()
    {
        UnregisterFromController();
    }

    private void Update()
    {
        var currentPosition = transform.position;
        var currentRotation = transform.rotation;

        // Current velocity randomized with noise.
        var noise = Mathf.PerlinNoise(Time.time, _noiseOffset) * 2.0f - 1.0f;
        var velocity = _controller.velocity * (1.0f + noise * _controller.velocityVariation);

        // Initializes the vectors.
        var separation = Vector3.zero;
        var alignment = _controller.transform.forward;
        var cohesion = _controller.transform.position;
        
        // Accumulates the vectors.
        foreach (var boid in _controller.Boids)
        {
            if (!boid.isActiveAndEnabled) continue;
            if (boid.gameObject == gameObject) continue;
            var t = boid.transform;
            separation += GetSeparationVector(t);
            alignment += t.forward;
            cohesion += t.position;
        }

        var avg = 1.0f / _controller.Boids.Count;
        alignment *= avg;
        cohesion *= avg;
        cohesion = (cohesion - currentPosition).normalized;

        if (Time.time > _timeScanAgain)
        {
            if (ScanForObstacles() && TryGetClearDirection(out var clearDirection))
            {
                _lastAvoidanceDirection = clearDirection;
                _timeAvoidanceStarted = Time.time;
                _avoiding = true;
            }

            _timeScanAgain = Time.time + _controller.avoidanceScanInterval;
        }

        var avoidanceVector = Vector3.zero;

        if (_avoiding)
        {
            if (Time.time > _timeAvoidanceStarted + _controller.avoidanceDuration)
            {
                _avoiding = false;
            }
            else
            {
                avoidanceVector = _lastAvoidanceDirection * _controller.avoidanceWeight;
            }
        }

        // Calculates a rotation from the vectors.
        var direction = separation + alignment + cohesion + avoidanceVector;
        var rotation = Quaternion.FromToRotation(Vector3.forward, direction.normalized);

        // Applies the rotation with interpolation.
        if (rotation != currentRotation)
        {
            var ip = Mathf.Exp(-_controller.rotationCoeff * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(rotation, currentRotation, ip);
        }

        // Moves forward.
        transform.position = currentPosition + transform.forward * (velocity * Time.deltaTime);
    }

    private bool ScanForObstacles()
    {
        return Physics.SphereCast(new Ray(transform.position, transform.forward), _controller.avoidanceScanRadius,
            _controller.avoidanceScanDistance, Voxeland.GetTerrainLayerMask(), QueryTriggerInteraction.Ignore);
    }

    private bool TryGetClearDirection(out Vector3 direction)
    {
        for (var i = 0; i < _controller.avoidObjectsAttempts; i++)
        {
            direction = Random.onUnitSphere;
            if (!Physics.Raycast(transform.position, direction, _controller.avoidanceMinClearDistance,
                    Voxeland.GetTerrainLayerMask(), QueryTriggerInteraction.Ignore))
            {
                return true;
            }
        }

        direction = default;
        return false;
    }
}
