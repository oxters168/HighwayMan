using UnityEngine;
using UnityHelpers;
using System.Linq;

public class RealCarPhysics : MonoBehaviour
{
    private Rigidbody _carBody;
    /// <summary>
    /// The default values were adjusted for a vehicle with a mass of 1395
    /// </summary>
    private Rigidbody CarBody { get { if (_carBody == null) _carBody = GetComponentInChildren<Rigidbody>(); return _carBody; } }

    public float maxSpeed = 20;
    public float acceleration = 0.1f;

    [Space(10), Tooltip("The minimum distance the pod keeps itself floating above the ground (in meters)")]
    public float minGroundDistance = 1;

    [Space(10), Tooltip("The constant applied to the proportional part of the floatation pd controller")]
    public float Kp = 20000;
    [Tooltip("The constant applied to the derivative part of the floatation pd controller")]
    public float Kd = 800000;

    [Tooltip("How much distance beyond the minimum ground height before anti-gravity wears off")]
    public float antigravityFalloffDistance = 20;
    public AnimationCurve antigravityFalloffCurve = AnimationCurve.Linear(0, 0, 1, 1);

    /// <summary>
    /// The x-axis represents current velocity and the y-axis represents how much friction to be applied (both axes are normalized to max speed and max friction)
    /// </summary>
    public AnimationCurve frictionCurve = AnimationCurve.Linear(0, 1, 1, 0.8f);


    [Space(10), Tooltip("The maximum torque the pod can apply to fix its orientation (in newton meters)")]
    public float maxCorrectionTorque = 60000;

    [Space(10), Tooltip("The layer(s) to be raycasted when looking for the ground")]
    public LayerMask groundMask = ~0;

    [Space(10)]
    public bool showOrientationCasters;
    public bool showCalculatedUp;
    public bool showCalculatedForward;
    public bool showCalculatedRight;

    /// <summary>
    /// The orientation the pod should be, calculated from the the up averaged from the surrounding face normals and the forward based on that up and the transform's forward
    /// </summary>
    private Quaternion castedOrientation;
    /// <summary>
    /// The up direction the pod should be
    /// </summary>
    private Vector3 up;
    /// <summary>
    /// The forward direction the pod should be
    /// </summary>
    private Vector3 forward;
    /// <summary>
    /// The right direction the pod should be
    /// </summary>
    private Vector3 right;
    /// <summary>
    /// The bounds of the vehicle calculated every fixed frame
    /// </summary>
    private Bounds vehicleBounds;
    /// <summary>
    /// The position the pod would be if left motionless
    /// </summary>
    private Vector3 groundedPosition;
    /// <summary>
    /// Used in the pd controller of the floatation
    /// </summary>
    private Vector3 prevErr;

    public bool control;
    
    void FixedUpdate()
    {
        vehicleBounds = transform.GetTotalBounds(Space.World);

        CalculateOrientationFromSurroundings();
        ApplyFloatation();
        ApplyOrientator();

        ApplyFriction();

        if (control)
        {
            float currentForwardVelocity = Vector3.Dot(CarBody.velocity, forward);
            if (Input.GetKey(KeyCode.W))
                CarBody.AddForce(PhysicsHelpers.CalculateRequiredForceForSpeed(CarBody.mass, currentForwardVelocity, Mathf.Clamp(currentForwardVelocity + acceleration, -maxSpeed, maxSpeed)) * forward, ForceMode.Force);
            if (Input.GetKey(KeyCode.S))
                CarBody.AddForce(PhysicsHelpers.CalculateRequiredForceForSpeed(CarBody.mass, currentForwardVelocity, Mathf.Clamp(currentForwardVelocity - acceleration, -maxSpeed, maxSpeed)) * forward, ForceMode.Force);
        }
    }

    private void CalculateOrientationFromSurroundings()
    {
        var rayResults = PhysicsHelpers.CastRays(vehicleBounds.center, showOrientationCasters, 15, 15, 10, groundMask, Space.World);
        var rayHits = rayResults.Where(rayResult => rayResult.raycastHit);

        var nextUp = Vector3.up;
        if (rayHits.Count() > 0)
            nextUp = (rayHits.Select(rayInfo => rayInfo.hitData[0].normal).Aggregate((firstRay, secondRay) => firstRay + secondRay) / rayHits.Count()).normalized;
        up = Vector3.Lerp(up, nextUp, Time.fixedDeltaTime * 5);
        forward = Vector3.ProjectOnPlane(transform.forward, up).normalized;
        right = Quaternion.AngleAxis(90, up) * forward;
        castedOrientation = Quaternion.LookRotation(forward, up);

        if (showCalculatedUp)
            Debug.DrawRay(vehicleBounds.center, up * 5, Color.green);
        if (showCalculatedForward)
            Debug.DrawRay(vehicleBounds.center, forward * 5, Color.blue);
        if (showCalculatedRight)
            Debug.DrawRay(vehicleBounds.center, right * 5, Color.red);
    }

    private void ApplyFriction()
    {
        float currentHorVelocity = Vector3.Dot(CarBody.velocity, right);
        float velocityFrictionMultiplier = frictionCurve.Evaluate(currentHorVelocity / maxSpeed);

        float forceToStop = PhysicsHelpers.CalculateRequiredForceForSpeed(CarBody.mass, currentHorVelocity, 0);
        CarBody.AddForce(right * forceToStop * velocityFrictionMultiplier, ForceMode.Force);

        Vector3 correctionTorque = CarBody.CalculateRequiredTorqueForRotation(castedOrientation, Time.fixedDeltaTime, maxCorrectionTorque);
        float maxTorque = 20;
        float correctionTorqueAmount = Vector3.Dot(correctionTorque, up);
        float torqueFrictionMultiplier = frictionCurve.Evaluate(correctionTorqueAmount / maxTorque);
        CarBody.AddTorque(up * correctionTorqueAmount, ForceMode.Force);
    }

    private void ApplyFloatation()
    {
        Vector3 expectedFloatingForce = CalculateFloatingForce();
        CarBody.AddForce(expectedFloatingForce, ForceMode.Force);
    }
    private Vector3 CalculateFloatingForce()
    {
        float vehicleSizeOnUpAxis = Mathf.Abs(Vector3.Dot(vehicleBounds.extents, up));

        float groundCastDistance = vehicleSizeOnUpAxis + minGroundDistance * 5;
        RaycastHit hitInfo;
        float groundDistance = float.MaxValue;
        bool rayHitGround = Physics.Raycast(vehicleBounds.center, -up, out hitInfo, groundCastDistance, groundMask);
        if (rayHitGround)
            groundDistance = hitInfo.distance - vehicleSizeOnUpAxis;

        float groundOffset = minGroundDistance - groundDistance;

        float antigravityMultiplier = 1;
        if (groundOffset < -float.Epsilon)
            antigravityMultiplier = antigravityFalloffCurve.Evaluate(Mathf.Max(antigravityFalloffDistance - Mathf.Abs(groundOffset), 0) / antigravityFalloffDistance);
        Vector3 antigravityForce = CarBody.CalculateAntiGravityForce() * antigravityMultiplier;

        Vector3 floatingForce = Vector3.zero;
        if (groundDistance < float.MaxValue) //If the ground is within range
        {
            //Thanks to @bmabsout for a much better and more stable floatation method
            //based on pid but just the p and the d
            groundedPosition = hitInfo.point + up * minGroundDistance;
            Vector3 err = up * Vector3.Dot(up, groundedPosition - CarBody.position);
            Vector3 proportional = Kp * err;
            Vector3 derivative = Kd * (err - prevErr);
            floatingForce = proportional + derivative;
            prevErr = err;
        }

        return antigravityForce + floatingForce;
    }
    private void ApplyOrientator()
    {
        Vector3 correctionTorque = CalculateCorrectionTorque();
        CarBody.AddTorque(correctionTorque, ForceMode.Force);
    }
    private Vector3 CalculateCorrectionTorque()
    {
        var correctionTorque = CarBody.CalculateRequiredTorqueForRotation(castedOrientation, Time.fixedDeltaTime, maxCorrectionTorque);
        correctionTorque -= up * Vector3.Dot(correctionTorque, up);
        return correctionTorque;
    }
}
