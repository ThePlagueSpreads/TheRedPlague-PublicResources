using UnityEngine;

namespace TheRedPlague.Mono.Util;

public class AcceleratePlayer : MonoBehaviour
{
    public Vector3 force;
    public float duration;

    private float _spawnTime;

    private void Start()
    {
        Destroy(this, duration);
        _spawnTime = Time.time;
    }

    private void FixedUpdate()
    {
        var applied = force;
        applied *= 1f - (Time.time - _spawnTime) / duration;
        switch (Player.main.motorMode)
        {
            case Player.MotorMode.Walk:
            case Player.MotorMode.Run:
                Player.main.groundMotor.SetVelocity(((IGroundMoveable)Player.main.groundMotor).GetVelocity() +
                                                    applied * (Time.deltaTime / 2f));
                break;
            case Player.MotorMode.Dive:
            case Player.MotorMode.Seaglide:
                Player.main.rigidBody.AddForce(applied, ForceMode.Acceleration);
                break;
            default:
                Plugin.Logger.LogWarning("Unexpected motor mode: " + Player.main.motorMode);
                break;
        }
    }
}