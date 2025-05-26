using UnityEngine;

namespace TheRedPlague.Mono.StoryContent.Precursor;

public class RoombaBehavior : MonoBehaviour, IScheduledUpdateBehaviour
{
    public Rigidbody rb;

    public float groundCheckDistance = 0.5f;
    public float forwardCheckDistance = 5;
    public float clearCheckDistance = 7;
    public float rampAcceleration = 11;
    public float acceleration = 7;
    public float stuckAcceleration = 20f;
    public float reverseAcceleration = 12;
    [Range(-1, 1)] public float upThreshold = 0.6f;
    public int turnAttempts = 12;
    public float turnOffsetThresholdDegrees = 10;
    public float startTurnAcceleration = 50;
    public float turnAcceleration = 50;
    public float sideScannersDistance = 1.02f;
    public float turnFailedStuckTimer = 5f;
    public float unstuckDuration = 0.7f;

    private bool _onGround;
    private bool _upright;
    private bool _turningAway;
    private float _targetAngle;
    private Vector3 _groundNormal;
    private float _timeStartedTurningAway;

    public int scheduledUpdateIndex { get; set; }

    private void OnEnable()
    {
        UpdateSchedulerUtils.Register(this);
    }

    private void OnDisable()
    {
        UpdateSchedulerUtils.Deregister(this);
    }

    public string GetProfileTag()
    {
        return "Roomba";
    }

    public void ScheduledUpdate()
    {
        _onGround = Physics.Raycast(transform.position, Vector3.down, out var groundHit, groundCheckDistance, -1,
            QueryTriggerInteraction.Ignore);
        if (_onGround) _groundNormal = groundHit.normal;

        _upright = _onGround && Vector3.Dot(transform.up, _groundNormal) > upThreshold;

        var centerScannerPos = transform.position;
        var leftScannerPos = centerScannerPos + transform.right * -sideScannersDistance;
        var rightScannerPos = centerScannerPos + transform.right * sideScannersDistance;

        if (!_turningAway &&
            (Physics.Raycast(centerScannerPos, transform.forward, forwardCheckDistance,
                 -1, QueryTriggerInteraction.Ignore) ||
             Physics.Raycast(leftScannerPos, transform.forward, forwardCheckDistance,
                 -1, QueryTriggerInteraction.Ignore) ||
             Physics.Raycast(rightScannerPos, transform.forward, forwardCheckDistance,
                 -1, QueryTriggerInteraction.Ignore))
           )
        {
            TryTurnAway();
        }
    }

    private void TryTurnAway()
    {
        bool flip = Random.value < 0.5f;
        for (var i = 0; i < turnAttempts; i++)
        {
            var angleRadians = Mathf.PI * 2f * (flip ? turnAttempts - i : i) / turnAttempts;
            var direction = new Vector3(Mathf.Sin(angleRadians), 0, Mathf.Cos(angleRadians));

            if (Physics.Raycast(transform.position, direction, clearCheckDistance, -1,
                    QueryTriggerInteraction.Ignore)) continue;

            _turningAway = true;
            _targetAngle = angleRadians * Mathf.Rad2Deg;
            _timeStartedTurningAway = Time.time;
            return;
        }
    }

    private void FixedUpdate()
    {
        if (!_onGround || !_upright)
            return;

        if (!_turningAway)
        {
            // main movement behavior
            var force = _groundNormal.y > 0.95f ? acceleration : rampAcceleration;
            if (rb.velocity.sqrMagnitude < 0.1f)
                force = stuckAcceleration;
            rb.AddRelativeForce(Vector3.forward * force, ForceMode.Acceleration);
        }
        else if (Time.time > _timeStartedTurningAway + turnFailedStuckTimer)
        {
            if (Time.time > _timeStartedTurningAway + turnFailedStuckTimer + unstuckDuration)
            {
                _turningAway = false;
                return;
            }

            rb.AddRelativeForce(Vector3.back * reverseAcceleration, ForceMode.Acceleration);
        }
        else
        {
            // turn behavior
            if (Mathf.Abs(transform.eulerAngles.y - _targetAngle) < turnOffsetThresholdDegrees)
            {
                _turningAway = false;
                rb.angularVelocity = Vector3.zero;
                return;
            }

            // var turnDirection = Mathf.Sign(transform.eulerAngles.y - _targetAngle);
            rb.AddTorque(
                new Vector3(0, rb.angularVelocity.sqrMagnitude <= 0.1f ? startTurnAcceleration : turnAcceleration, 0),
                ForceMode.Acceleration);
        }
    }
}