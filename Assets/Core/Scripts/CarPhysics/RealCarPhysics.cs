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

    [Tooltip("In meters per second")]
    public float maxSpeed = 20;
    [Tooltip("In meters per second squared")]
    public float maxAcceleration = 0.1f;
    [Tooltip("In degrees per second")]
    public float maxTurnRate = 1;
    [Tooltip("The x-axis represents current speed and the y-axis represents acceleration (both normalized to their max)")]
    /// <summary>
    /// The x-axis represents current speed and the y-axis represents acceleration (both normalized to their max)
    /// </summary>
    public AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 1, 1, 0);
    [Tooltip("The x-axis represents current speed and the y-axis represents turning rate (both normalized to their max)")]
    /// <summary>
    /// The x-axis represents current speed and the y-axis represents turning rate (both normalized to their max)
    /// </summary>
    public AnimationCurve handlingCurve = new AnimationCurve(new Keyframe(0, 0, 0, 10), new Keyframe(0.05f, 1), new Keyframe(1, 1));
    [Tooltip("The x-axis represents current speed and the y-axis represents how much friction to be applied (both axes are normalized to their max)")]
    /// <summary>
    /// The x-axis represents current speed and the y-axis represents how much friction to be applied (both axes are normalized to their max)
    /// </summary>
    public AnimationCurve frictionCurve = AnimationCurve.Linear(0, 1, 1, 0.8f);

    [Space(10)]
    public WheelInfo[] wheels;

    [Space(10)]
    public float gas;
    public float steer;

    [Space(10), Tooltip("How much distance from the ground the vehicle should be (in meters)")]
    public float groundHoverDistance = 1;

    [Space(10), Tooltip("The constant applied to the proportional part of the floatation pd controller")]
    public float Kp = 20000;
    [Tooltip("The constant applied to the derivative part of the floatation pd controller")]
    public float Kd = 800000;

    [Tooltip("How much distance beyond the minimum ground height before anti-gravity wears off")]
    public float antigravityFalloffDistance = 0.1f;
    public AnimationCurve antigravityFalloffCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Space(10), Tooltip("The maximum torque the pod can apply to fix its orientation (in newton meters)")]
    public float maxCorrectionTorque = 60000;
    [Tooltip("The dot value threshold between the vehicle up and calculated up before applying orientation torque")]
    public float orientationFixFalloff = 0.95f;

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
    private Quaternion CastedOrientation { get { return Quaternion.LookRotation(forward, up); } }
    /// <summary>
    /// Gets whether the vehicle is grounded based on whether anti gravity is being applied to the vehicle or not (antigravitymultiplier >? 0)
    /// </summary>
    private bool isGrounded;
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
    /// <summary>
    /// In case the calculated orientation
    /// </summary>
    private Quaternion prevOrientation;
    /// <summary>
    /// How sticky the vehicle is to the current surface based on gravity (0 not stuck at all, 1 completely stuck)
    /// </summary>
    private float surfaceCoefficient;
    
    void FixedUpdate()
    {
        vehicleBounds = transform.GetTotalBounds(Space.World);

        CalculateOrientationFromSurroundings();
        ApplyFloatation();

        float upOffset = Vector3.Dot(transform.up, up);
        if (isGrounded && upOffset >= orientationFixFalloff)
        {
            float currentForwardVelocity = Vector3.Dot(CarBody.velocity, forward);
            float accelerationMultiplier = accelerationCurve.Evaluate(Mathf.Abs(currentForwardVelocity) / maxSpeed);
            if (gas > float.Epsilon || gas < -float.Epsilon)
                CarBody.AddForce(PhysicsHelpers.CalculateRequiredForceForSpeed(CarBody.mass, currentForwardVelocity, Mathf.Clamp(currentForwardVelocity + gas * accelerationMultiplier * maxAcceleration, -maxSpeed, maxSpeed)) * forward, ForceMode.Force);
            
            float handlingMultiplier = handlingCurve.Evaluate(Mathf.Abs(currentForwardVelocity) / maxSpeed);
            if (steer > float.Epsilon || steer < -float.Epsilon)
                forward = Quaternion.AngleAxis(steer * maxTurnRate * handlingMultiplier * Mathf.Sign(currentForwardVelocity), up) * forward;
            
            // if (upOffset >= orientationFixFalloff && surfaceCoefficient >= 0.9f)
            // {
                ApplyOrientator(CastedOrientation);
            
                ApplyFriction();
                Steer();
            // }
        }
    }

    private void CalculateOrientationFromSurroundings()
    {
        var rayResults = PhysicsHelpers.CastRays(vehicleBounds.center, transform.forward, transform.right, -transform.up, showOrientationCasters, 15, 15, 10, groundMask, 180, 180);
        var rayHits = rayResults.Where(rayResult => rayResult.raycastHit);

        var nextUp = Vector3.up;
        if (rayHits.Count() > 0)
            nextUp = (rayHits.Select(rayInfo => rayInfo.hitData[0].normal).Aggregate((firstRay, secondRay) => firstRay + secondRay) / rayHits.Count()).normalized;
        up = Vector3.Lerp(up, nextUp, Time.fixedDeltaTime * 5);
        
        forward = Vector3.ProjectOnPlane(transform.forward, up).normalized;
        right = Quaternion.AngleAxis(90, up) * forward;

        surfaceCoefficient = Mathf.Clamp01(Vector3.Dot(Physics.gravity.normalized, -up));

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
    }
    private void Steer()
    {
        Vector3 steerTorque = CarBody.CalculateRequiredTorqueForRotation(CastedOrientation, Time.fixedDeltaTime, maxCorrectionTorque);
        float steerTorqueAmount = Vector3.Dot(steerTorque, up);
        CarBody.AddTorque(up * steerTorqueAmount, ForceMode.Force);
    }

    private void ApplyFloatation()
    {
        Vector3 expectedFloatingForce = CalculateFloatingForce();
        CarBody.AddForce(expectedFloatingForce * surfaceCoefficient, ForceMode.Force);
    }
    private Vector3 CalculateFloatingForce()
    {
        float vehicleSizeOnUpAxis = Mathf.Abs(Vector3.Dot(vehicleBounds.extents, up));

        float groundCastDistance = vehicleSizeOnUpAxis + groundHoverDistance * 5;
        RaycastHit hitInfo;
        float groundDistance = float.MaxValue;
        bool rayHitGround = Physics.Raycast(vehicleBounds.center, -up, out hitInfo, groundCastDistance, groundMask);
        if (rayHitGround)
            groundDistance = hitInfo.distance - vehicleSizeOnUpAxis;

        float groundOffset = groundHoverDistance - groundDistance;

        float antigravityMultiplier = 1;
        if (groundOffset < -float.Epsilon)
            antigravityMultiplier = antigravityFalloffCurve.Evaluate(Mathf.Max(antigravityFalloffDistance - Mathf.Abs(groundOffset), 0) / antigravityFalloffDistance);
        Vector3 antigravityForce = CarBody.CalculateAntiGravityForce() * antigravityMultiplier;

        isGrounded = antigravityMultiplier > Mathf.Epsilon;

        Vector3 floatingForce = Vector3.zero;
        if (groundDistance < float.MaxValue) //If the ground is within range
        {
            //Thanks to @bmabsout for a much better and more stable floatation method
            //based on pid but just the p and the d
            groundedPosition = hitInfo.point + up * groundHoverDistance;
            Vector3 err = up * Vector3.Dot(up, groundedPosition - CarBody.position);
            Vector3 proportional = Kp * err;
            Vector3 derivative = Kd * (err - prevErr);
            floatingForce = proportional + derivative;
            prevErr = err;
        }

        return antigravityForce + floatingForce;
    }
    private void ApplyOrientator(Quaternion orientation)
    {
        Vector3 correctionTorque = CalculateCorrectionTorque(orientation);
        CarBody.AddTorque(correctionTorque, ForceMode.Force);
    }
    private Vector3 CalculateCorrectionTorque(Quaternion orientation)
    {
        var correctionTorque = CarBody.CalculateRequiredTorqueForRotation(orientation, Time.fixedDeltaTime, maxCorrectionTorque);
        correctionTorque -= up * Vector3.Dot(correctionTorque, up);
        return correctionTorque;
    }

    [System.Serializable]
    public struct WheelInfo
    {
        public string name;
        public Vector3 position;
        public float radius;
    }
}
