using TMPro;
using UnityEngine;
using UnityHelpers;

public class VehicleDriver : MonoBehaviour
{
    public TextMeshProUGUI speedGauge;

    public Rigidbody physics;

    [Tooltip("In m/s^2")]
    public float acceleration = 3.4f;
    [Tooltip("In m/s^2")]
    public float deceleration = 1.2f;
    [Tooltip("In m/s^2")]
    public float brakeleration = 10f;
    [Tooltip("In m/s")]
    public float maxSpeed = 57.2f;

    public float currentSpeed;

    [Range(0, 1)]
    public float gas;
    [Range(0, 1)]
    public float brake;

    void FixedUpdate()
    {
        float gasAmount = gas * acceleration * (currentSpeed >= 0 ? 1 : -1);
        float brakeAmount = brake * brakeleration * (currentSpeed >= 0 ? -1 : 1);
        float totalAcceleration = gasAmount + brakeAmount;
        if (totalAcceleration > -float.Epsilon && totalAcceleration < float.Epsilon && !(currentSpeed > -float.Epsilon && currentSpeed < float.Epsilon))
            totalAcceleration = deceleration * (currentSpeed >= 0 ? -1 : 1);
        float deltaSpeed = totalAcceleration * Time.fixedDeltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed + deltaSpeed, 0, maxSpeed);

        float actualSpeed = physics.velocity.magnitude;
        //if (!(currentSpeed > -float.Epsilon && currentSpeed < float.Epsilon) && actualSpeed < currentSpeed)
        //{
            Vector3 nextCalculatedPosition = physics.position + transform.forward * currentSpeed;
            physics.AddForce(physics.CalculateRequiredForce(nextCalculatedPosition), ForceMode.Force);
        //}
        speedGauge.text = actualSpeed.ToString();
    }
}
