using UnityEngine;

namespace TheRedPlague.Mono.CreatureBehaviour.Stabby;

public class StabbyMotion : MonoBehaviour
{
    public Transform ikTarget;
    public float retractSpeed = 4.5f;
    public float retractDuration = 0.03333f;
    public float stabSpeed = 18f;
    public float stabDuration = 0.1f;
    public float lerpSpeed = 4f;
    public float rotateSpeedAnglesPerSecond = 50;
    public float defaultMaxDistance = 1.3f;
    public FMOD_CustomEmitter emitter;

    private Vector3 _defaultTargetPosition;
    private Vector3 _targetLocalPosition;
    private Vector3 _targetDirection;

    public bool Stabbing { get; private set; }
    private float _timeStartedStabbing;
    
    public void StabTarget()
    {
        if (Stabbing) return;
        Stabbing = true;
        _timeStartedStabbing = Time.time;
        emitter.Play();
    }

    public bool IsThrustingForward() => Time.time < _timeStartedStabbing + retractDuration + stabDuration;
    
    private void Start()
    {
        _defaultTargetPosition = ikTarget.localPosition;
    }

    private Transform GetLookAtTransform()
    {
        return MainCamera.camera.transform;
    }
    
    private void Update()
    {
        if (Stabbing)
        {
            if (Time.time < _timeStartedStabbing + retractDuration)
            {
                ikTarget.transform.position -= ikTarget.up * (retractSpeed * Time.deltaTime * transform.localScale.y);
            }   
            else if (IsThrustingForward())
            {
                ikTarget.transform.position += ikTarget.up * (stabSpeed * Time.deltaTime * transform.localScale.y);
            }
            else
            {
                Stabbing = false;
            }
            return;
        }

        var lookAtTransform = GetLookAtTransform();
        if (lookAtTransform == null)
        {
            _targetLocalPosition = _defaultTargetPosition;
            _targetDirection = transform.up;
        }
        else
        {
            var directionToLookAtTransform = (lookAtTransform.position - transform.position).normalized;
            _targetLocalPosition = ikTarget.parent.InverseTransformPoint(transform.position + directionToLookAtTransform).normalized
                                   * defaultMaxDistance;
            _targetDirection = directionToLookAtTransform;
        }

        ikTarget.localPosition =
            Vector3.Lerp(ikTarget.localPosition, _targetLocalPosition, Time.deltaTime * lerpSpeed);
        ikTarget.up = Vector3.RotateTowards(ikTarget.up, _targetDirection,
            Time.deltaTime * rotateSpeedAnglesPerSecond, 0);
    }
}