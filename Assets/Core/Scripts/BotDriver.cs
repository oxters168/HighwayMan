using UnityEngine;
using UnityHelpers;

public class BotDriver : MonoBehaviour
{
    public VehicleSwitcher vehicles;
    public TreeCollider treeCollider;
    public HumanDriver keepUpWith;

    [Space(10), Tooltip("In m/s")]
    public float speedLimit = 27.78f;

    [Space(10)]
    public Transform frontSpawn;
    public Transform rearSpawn;

    [Space(10), Tooltip("In meters")]
    public float forwardDistanceObstacleCheck = 10;
    [Tooltip("In meters")]
    public float sideDistanceObstacleCheck = 2;

    [Space(10), Tooltip("The default target when respawned")]
    public int defaultTargetIndex;
    [Tooltip("In seconds")]
    public float targetSwitchTime = 3;
    [Tooltip("Every targetSwitchTime seconds, this is the percent chance the bot will switch targets.")]
    public float targetSwitchPercent = 0.02f;
    private float lastTargetSwitchTime;


    public Transform[] targets;
    private int currentTargetIndex;

    private bool spawnedFromRear;

    private float spawnSpeed;

    private void Awake()
    {
        treeCollider.onTriggerEnter.AddListener(OnTreeTriggerEnter);
    }
    private void Start()
    {
        RespawnRandom();
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
    private void OnTreeTriggerEnter(TreeCollider.CollisionInfo colInfo)
    {
        if (!spawnedFromRear && colInfo.collidedWith.CompareTag("Rear") || spawnedFromRear && colInfo.collidedWith.CompareTag("Front"))
            RespawnRandom();
    }

    private void RespawnCheck(CarPhysics currentVehicle)
    {
        if (currentVehicle.transform.position.y < -5)
            RespawnRandom();
    }
    private void Drive(CarPhysics currentVehicle)
    {
        //Calculate steer value to reach target
        float nextSteer;
        float targetAngle = currentVehicle.transform.position.SignedAngle(targets[currentTargetIndex].position, currentVehicle.transform.forward, currentVehicle.transform.up);
        nextSteer = Mathf.Clamp(targetAngle / currentVehicle.vehicleStats.maxWheelAngle, -1, 1);

        //Calculating gas value to conform to set speed
        float nextGas = currentVehicle.currentSpeed < spawnSpeed ? 1 : 0;

        //Get obstacle ray start points
        Vector3 vehicleLowerCenter = currentVehicle.GetPointOnBoundsBorder(0, -0.5f, 0);

        Vector3 vehicleForwardRayStart = currentVehicle.GetPointOnBoundsBorder(0, -0.5f, 0.9f);
        Vector3 vehicleForwardRightRayStart = currentVehicle.GetPointOnBoundsBorder(0.5f, -0.5f, 0.9f);
        Vector3 vehicleForwardLeftRayStart = currentVehicle.GetPointOnBoundsBorder(-0.5f, -0.5f, 0.9f);
        Vector3 vehicleLeftRayStart = currentVehicle.GetPointOnBoundsBorder(-1, -0.5f, 0);
        Vector3 vehicleRightRayStart = currentVehicle.GetPointOnBoundsBorder(1, -0.5f, 0);

        Vector3 vehicleForwardRightDirection = (vehicleForwardRightRayStart - vehicleLowerCenter).normalized;
        Vector3 vehicleForwardLeftDirection = (vehicleForwardLeftRayStart - vehicleLowerCenter).normalized;

        //Draw rays around vehicle
        Debug.DrawRay(vehicleForwardRayStart, currentVehicle.transform.forward * forwardDistanceObstacleCheck, Color.blue);
        Debug.DrawRay(vehicleForwardRightRayStart, vehicleForwardRightDirection * forwardDistanceObstacleCheck, Color.blue);
        Debug.DrawRay(vehicleForwardLeftRayStart, vehicleForwardLeftDirection * forwardDistanceObstacleCheck, Color.blue);
        Debug.DrawRay(vehicleLeftRayStart, -currentVehicle.transform.right * sideDistanceObstacleCheck, Color.red);
        Debug.DrawRay(vehicleRightRayStart, currentVehicle.transform.right * sideDistanceObstacleCheck, Color.red);

        RaycastHit forwardHitInfo, forwardLeftHitInfo, forwardRightHitInfo, leftHitInfo, rightHitInfo;
        bool forwardHit = Physics.Raycast(vehicleForwardRayStart, currentVehicle.transform.forward, out forwardHitInfo, forwardDistanceObstacleCheck);
        bool forwardLeftHit = Physics.Raycast(vehicleForwardLeftRayStart, vehicleForwardLeftDirection, out forwardLeftHitInfo, forwardDistanceObstacleCheck * 0.5f);
        bool forwardRightHit = Physics.Raycast(vehicleForwardRightRayStart, vehicleForwardRightDirection, out forwardRightHitInfo, forwardDistanceObstacleCheck * 0.5f);
        bool leftHit = Physics.Raycast(vehicleLeftRayStart, -currentVehicle.transform.right, out leftHitInfo, sideDistanceObstacleCheck);
        bool rightHit = Physics.Raycast(vehicleRightRayStart, currentVehicle.transform.right, out rightHitInfo, sideDistanceObstacleCheck);
        if (forwardHit || forwardLeftHit || forwardRightHit || leftHit || rightHit)
        {
            //Distance from obstacle to gas percent
            if (forwardHit || forwardLeftHit || forwardRightHit)
                nextGas = -1;
                //nextGas = -(1 - forwardHitInfo.distance / forwardDistanceObstacleCheck);

            //Steer from obstacle
            if (forwardHit || forwardLeftHit || forwardRightHit)
            {
                if (forwardLeftHit && currentVehicle.currentSpeed > 0 || forwardRightHit && currentVehicle.currentSpeed < 0)
                    nextSteer = 1;
                else if (forwardRightHit && currentVehicle.currentSpeed > 0 || forwardLeftHit && currentVehicle.currentSpeed < 0)
                    nextSteer = -1;
                else
                {
                    float obstacleAngle = currentVehicle.transform.position.SignedAngle(forwardHitInfo.point, currentVehicle.transform.forward, currentVehicle.transform.up);
                    if (obstacleAngle > 0 && currentVehicle.currentSpeed > 0 || obstacleAngle < 0 && currentVehicle.currentSpeed < 0)
                        nextSteer = -1;
                    else
                        nextSteer = 1;
                }
            }
            else if (leftHit && currentVehicle.currentSpeed > 0 || rightHit && currentVehicle.currentSpeed < 0)
                nextSteer = 1;
            else if (rightHit && currentVehicle.currentSpeed > 0 || leftHit && currentVehicle.currentSpeed < 0)
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
            float rng = Random.Range(0, 1);
            if (rng <= targetSwitchPercent)
                currentTargetIndex = Random.Range(0, targets.Length);
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

        spawnSpeed = Random.Range(speedLimit - 5, speedLimit + 5);

        spawnedFromRear = true;
        Transform spawnFrom = rearSpawn;
        if (keepUpWith.vehicles.currentVehicle != null)
        {
            float keepUpDirection = Vector3.Dot(keepUpWith.vehicles.currentVehicle.transform.forward, Vector3.forward);
            float keepUpSpeed = keepUpWith.vehicles.currentVehicle.currentSpeed;
            if ((keepUpDirection < 0 && keepUpSpeed > 0) || keepUpDirection > 0 && (keepUpSpeed < 0 || spawnSpeed < keepUpSpeed))
            {
                spawnFrom = frontSpawn;
                spawnedFromRear = false;
            }
        }
        vehicles.currentVehicle.Teleport(spawnFrom.position, spawnFrom.rotation, spawnSpeed);
    }
}
