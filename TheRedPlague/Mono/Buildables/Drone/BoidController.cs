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

using System.Collections.Generic;
using UnityEngine;

namespace TheRedPlague.Mono.Buildables.Drone;

public class BoidController : MonoBehaviour
{
    [Range(0.1f, 20.0f)]
    public float velocity = 6.0f;

    [Range(0.0f, 0.9f)]
    public float velocityVariation = 0.5f;

    [Range(0.1f, 20.0f)]
    public float rotationCoeff = 4.0f;

    [Range(0.1f, 10.0f)]
    public float neighborDist = 2.0f;

    public float avoidanceScanRadius = 1f;
    public float avoidanceScanDistance = 8f;
    public float avoidanceScanInterval = 0.3f;
    public float avoidanceMinClearDistance = 10f;
    public float avoidanceDuration = 1.5f;
    public float avoidanceWeight = 10f;
    public int avoidObjectsAttempts = 12;

    public HashSet<BoidBehaviour> Boids { get; } = new();

    public int Register(BoidBehaviour behaviour)
    {
        Boids.Add(behaviour);
        return _numberRegistered++;
    }
    
    private int _numberRegistered;

    private void Update()
    {
        if (Boids.Count == 0)
        {
            Destroy(gameObject);
        }
    }
    
    
}
