using NWH.VehiclePhysics;
using NWH.WheelController3D;
using TMPro;
using UnityEngine;
using UnityHelpers;

public class VehicleDriver : MonoBehaviour
{
    public TextMeshProUGUI speedGauge;

    public Rigidbody physics;

    public Transform wheelFL, wheelFR;
    public WheelController wheelRL, wheelRR;

    [Tooltip("In m/s^2")]
    public float acceleration = 3.4f;
    [Tooltip("In m/s^2")]
    public float deceleration = 1.2f;
    [Tooltip("In m/s^2")]
    public float brakeleration = 10f;
    [Tooltip("In m/s")]
    public float maxForwardSpeed = 57.2f;
    [Tooltip("In m/s")]
    public float maxReverseSpeed = 28.6f;
    [Tooltip("In degrees")]
    public float maxWheelAngle = 33.33f;

    public float currentSpeed;

    [Range(-1, 1)]
    public float gas;
    [Range(0, 1)]
    public float brake;
    [Range(-1, 1)]
    public float steer;

    void FixedUpdate()
    {
        Quaternion wheelRotation = Quaternion.Euler(0, maxWheelAngle * steer, 0);
        wheelFL.localRotation = wheelRotation;
        wheelFR.localRotation = wheelRotation;

        float forwardPercent = Mathf.Clamp(Vector3.Angle(transform.forward, physics.velocity), 0, 90) / 90;
        float actualForwardSpeed = physics.velocity.magnitude * forwardPercent;
        speedGauge.text = actualForwardSpeed.ToString();

        if (wheelRL.isGrounded && wheelRR.isGrounded)
        {
            gas = Mathf.Clamp(gas, -1, 1);
            brake = Mathf.Clamp(brake, 0, 1);
            steer = Mathf.Clamp(steer, -1, 1);

            float gasAmount = gas * acceleration;
            float brakeAmount = brake * brakeleration * (currentSpeed >= 0 ? -1 : 1);
            float totalAcceleration = gasAmount + brakeAmount;
            if (totalAcceleration > -float.Epsilon && totalAcceleration < float.Epsilon && !(currentSpeed > -float.Epsilon && currentSpeed < float.Epsilon))
                totalAcceleration = deceleration * (currentSpeed >= 0 ? -1 : 1);
            float deltaSpeed = totalAcceleration * Time.fixedDeltaTime;

            currentSpeed = Mathf.Clamp(currentSpeed + deltaSpeed, -maxReverseSpeed, maxForwardSpeed);

            physics.AddForce(physics.CalculateRequiredForceForSpeed(currentSpeed * transform.forward), ForceMode.Force);
        }
        else
            currentSpeed = actualForwardSpeed;
    }
}
