using UnityEngine;

namespace TheRedPlague.Mono.Util;

public class FakeRigidbody : MonoBehaviour
{
    public float gravity = 9.8f;
    public float drag;
    
    public Vector3 Velocity { get; set; }
    
    public Vector3 LastAppliedForce { get; private set; }

    public void AddForce(Vector3 force)
    {
        Velocity += force * Time.deltaTime;
        LastAppliedForce = force;
    }

    private void Update()
    {
        // Add gravity force
        Velocity += new Vector3(0, -gravity * Time.deltaTime, 0);
        // Add drag force
        Velocity *= 1 - Time.deltaTime * drag;

        transform.position += Velocity * Time.deltaTime;
    }
}