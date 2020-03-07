using UnityEngine;
using UnityHelpers;

public class BotDriver : MonoBehaviour, AbstractDriver
{
    public VehicleSwitcher vehicles;
    [Tooltip("Has to implement the interface AbstractDriver")]
    public GameObject keepUpWithObject;
    private AbstractDriver keepUpWith;

    [Space(10), Tooltip("In m/s")]
    public float speedLimit = 27.78f;

    [Space(10)]
    public Transform frontSpawn;
    public Transform rearSpawn;

    [Space(10), Tooltip("The default target when respawned")]
    public int defaultTargetIndex;
    [Tooltip("In seconds")]
    public float targetSwitchTime = 3;
    [Tooltip("Every targetSwitchTime seconds, this is the percent chance the bot will switch targets.")]
    public float targetSwitchPercent = 0.2f;
    [Tooltip("Max difference in degrees from target")]
    public float maxAngle = 5;
    [Tooltip("Min difference in degrees from forward")]
    public float minAngle = 0.1f;
    [Range(0, 1), Tooltip("The max value to give to steer in CarPhysics")]
    public float maxSteer = 0.2f;
    [Tooltip("The dead zone")]
    public float minDistance = 0.1f;
    [Tooltip("The distance between the lanes in meters")]
    public float laneDistance = 5;
    private float lastTargetSwitchTime;


    public Transform[] targets;
    public int currentTargetIndex;

    //private bool spawnedFromRear;

    private float spawnSpeed;

    private void Awake()
    {
        //treeCollider.onTriggerEnter.AddListener(OnTreeTriggerEnter);
    }
    private void Start()
    {
        RespawnRandom();
        keepUpWith = keepUpWithObject.GetComponent<AbstractDriver>();
    }
    private void Update()
    {
        var currentVehicle = vehicles.currentVehicle;
        if (currentVehicle != null)
        {
            RespawnCheck(currentVehicle);
            Drive(currentVehicle);
            TrySwitchTarget();
        }
    }
    //private void OnTreeTriggerEnter(TreeCollider.CollisionInfo colInfo)
    //{
    //    if (!spawnedFromRear && colInfo.collidedWith.CompareTag("Rear") || spawnedFromRear && colInfo.collidedWith.CompareTag("Front"))
    //        RespawnRandom();
    //}

    private void RespawnCheck(CarPhysics currentVehicle)
    {
        if (currentVehicle.transform.position.y < -5)
            RespawnRandom();
    }
    private void Drive(CarPhysics currentVehicle)
    {
        //Calculate steer value to reach target
        float nextSteer = 0;
        Vector3 vehicleFrontPoint = currentVehicle.GetPointOnBoundsBorder(0, -0.5f, 0.9f);
        Vector3 hypotenuse = targets[currentTargetIndex].position - vehicleFrontPoint;
        float targetAngle = Vector3.SignedAngle(hypotenuse.normalized, targets[currentTargetIndex].forward, currentVehicle.transform.up);
        float vehicleAngle = -Vector3.SignedAngle(currentVehicle.transform.forward, hypotenuse.normalized, currentVehicle.transform.up);
        //Debug.Log(vehicleAngle.ToString());
        float oppSide = -Mathf.Sin(targetAngle * Mathf.Deg2Rad) * hypotenuse.magnitude;
        Debug.DrawRay(vehicleFrontPoint, hypotenuse, Color.yellow);
        Debug.DrawRay(vehicleFrontPoint, currentVehicle.transform.right * oppSide, Color.yellow);
        if (Mathf.Abs(oppSide) >= minDistance && (Mathf.Abs(vehicleAngle) < maxAngle || Mathf.Sign(vehicleAngle) != Mathf.Sign(oppSide)))
        {
            nextSteer = Mathf.Clamp(oppSide / laneDistance, -1, 1) * maxSteer;
        }
        else if (Mathf.Abs(vehicleAngle) >= maxAngle)
            nextSteer = maxSteer * -Mathf.Sign(vehicleAngle);
        else if (Mathf.Abs(oppSide) < minDistance && Mathf.Abs(vehicleAngle) > minAngle)
            nextSteer = maxSteer * -Mathf.Sign(vehicleAngle) / 8;

        //Calculating gas value to conform to set speed
        float nextGas = currentVehicle.currentSpeed < spawnSpeed ? 1 : 0;

        var forwardHitInfo = currentVehicle.GetForwardHitInfo();
        var leftHitInfo = currentVehicle.GetLeftHitInfo();
        var rightHitInfo = currentVehicle.GetRightHitInfo();
        if (forwardHitInfo.hit || leftHitInfo.hit || rightHitInfo.hit)
        {
            //Distance from obstacle to gas percent
            if (forwardHitInfo.hit)
            {
                float distanceToObstacle = forwardHitInfo.info.distance;
                float predictedDistanceTravel = currentVehicle.currentSpeed * Time.fixedDeltaTime;
                nextGas = -(1 - Mathf.Clamp01(predictedDistanceTravel / distanceToObstacle));
                //nextGas = -(1 - forwardHitInfo.distance / forwardDistanceObstacleCheck);
            }

            //Steer from obstacle
            if (forwardHitInfo.hit)
            {
                float obstacleAngle = currentVehicle.GetHitAngle(forwardHitInfo);
                if (obstacleAngle > 0 && currentVehicle.currentSpeed > 0 || obstacleAngle < 0 && currentVehicle.currentSpeed < 0)
                    nextSteer = -1;
                else
                    nextSteer = 1;
            }
            else if (leftHitInfo.hit && currentVehicle.currentSpeed > 0 || rightHitInfo.hit && currentVehicle.currentSpeed < 0)
                nextSteer = 1;
            else if (rightHitInfo.hit && currentVehicle.currentSpeed > 0 || leftHitInfo.hit && currentVehicle.currentSpeed < 0)
                nextSteer = -1;
        }

        //Apply values to car
        currentVehicle.gas = nextGas;
        currentVehicle.steer = nextSteer;
    }
    private void TrySwitchTarget()
    {
        if (Time.time - lastTargetSwitchTime >= targetSwitchTime)
        {
            float rng = Random.Range(0, 1f);
            if (rng <= targetSwitchPercent)
                currentTargetIndex = Random.Range(0, targets.Length);

            lastTargetSwitchTime = Time.time;
        }
    }

    public void RespawnRandom()
    {
        Respawn(Random.Range(0, vehicles.allVehicles.Length));
    }
    public void Respawn(int vehicleIndex)
    {
        currentTargetIndex = defaultTargetIndex;

        vehicles.SetVehicle(Mathf.Clamp(vehicleIndex, 0, vehicles.allVehicles.Length));
        vehicles.currentVehicle.castRays = true;

        spawnSpeed = Random.Range(speedLimit - 5, speedLimit + 5);

        //spawnedFromRear = true;
        Transform spawnFrom = rearSpawn;
        var followVehicle = keepUpWith?.GetVehicle();
        if (followVehicle != null)
        {
            float keepUpDirection = Vector3.Dot(followVehicle.transform.forward, Vector3.forward);
            float keepUpSpeed = followVehicle.currentSpeed;
            if ((keepUpDirection < 0 && keepUpSpeed > 0) || keepUpDirection > 0 && (keepUpSpeed < 0 || spawnSpeed < keepUpSpeed))
            {
                spawnFrom = frontSpawn;
                //spawnedFromRear = false;
            }
        }
        vehicles.currentVehicle.Teleport(spawnFrom.position, spawnFrom.rotation, spawnSpeed);
    }

    public CarPhysics GetVehicle()
    {
        return vehicles.currentVehicle;
    }
}
