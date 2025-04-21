using UnityEngine;

namespace TheRedPlague.Mono.VFX;

public class BasicPathFollower : MonoBehaviour
{
    public Vector3[] path;
    public float velocity;
    public float rotateSpeed;
    public float maxDistanceToNextPathNode = 2f;
    
    private int _nextIndex = 1;

    private void Start()
    {
        if (path.Length <= 1) Destroy(gameObject);
    }

    private void Update()
    {
        var nextPointVector = path[_nextIndex] - transform.position;
        var sqrMagnitude = Vector3.SqrMagnitude(nextPointVector);
        if (sqrMagnitude < maxDistanceToNextPathNode * maxDistanceToNextPathNode)
        {
            Advance();
            return;
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(nextPointVector.normalized),
            Time.deltaTime * rotateSpeed);
        transform.position += transform.forward * (velocity * Time.deltaTime);
    }

    private void Advance()
    {
        _nextIndex++;
        if (_nextIndex >= path.Length)
        {
            Destroy(gameObject);
        }
    }
}