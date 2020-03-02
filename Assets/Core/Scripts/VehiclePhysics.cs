using NWH.WheelController3D;
using TMPro;
using UnityEngine;
using UnityHelpers;

public class VehiclePhysics : MonoBehaviour
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

    public float strivedSpeed;

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

        float forwardPercent = -(Vector3.Angle(transform.forward, physics.velocity.normalized) / 90 - 1);
        float actualSpeed = physics.velocity.magnitude * forwardPercent;
        speedGauge.text = actualSpeed.ToString();

        if (wheelRL.isGrounded || wheelRR.isGrounded)
        {
            gas = Mathf.Clamp(gas, -1, 1);
            brake = Mathf.Clamp(brake, 0, 1);
            steer = Mathf.Clamp(steer, -1, 1);

            float gasAmount = gas * (acceleration + (gas > 0 && actualSpeed < 0 || gas < 0 && actualSpeed > 0 ? brakeleration : 0));
            float brakeAmount = brake * brakeleration * (actualSpeed >= 0 ? -1 : 1);
            float totalAcceleration = gasAmount + brakeAmount;
            if (totalAcceleration > -float.Epsilon && totalAcceleration < float.Epsilon && !(strivedSpeed > -float.Epsilon && strivedSpeed < float.Epsilon))
                totalAcceleration = deceleration * (actualSpeed >= 0 ? -1 : 1);
            float deltaSpeed = totalAcceleration * Time.fixedDeltaTime;

            float differenceInSpeed = Mathf.Abs(Mathf.Abs(strivedSpeed) - Mathf.Abs(actualSpeed)); //This is used to
            float speedRatio = Mathf.Abs(actualSpeed) / Mathf.Abs(strivedSpeed);                   //reset the strivedSpeed
            if (differenceInSpeed > speedRatio * maxReverseSpeed)                                  //value when it's too
                strivedSpeed = actualSpeed;                                                        //far from actual speed

            strivedSpeed = Mathf.Clamp(strivedSpeed + deltaSpeed, -maxReverseSpeed, maxForwardSpeed);

            Vector3 nonForwardVelocity = physics.velocity - actualSpeed * transform.forward;
            physics.AddForce(physics.CalculateRequiredForceForSpeed(strivedSpeed * transform.forward + (actualSpeed > 0.01f ? nonForwardVelocity : Vector3.zero)), ForceMode.Force);
        }
        //else
        //    strivedSpeed = actualSpeed;
    }
}
