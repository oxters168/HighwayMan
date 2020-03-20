using MLAgents;
using UnityEngine;
using UnityHelpers;

public class AgentDriver : Agent, AbstractDriver
{
    public VehicleSwitcher vehicles;
    public Carability carability;

    public Vector3 vehicleSpawnCenter, vehicleSpawnSize;

    [Tooltip("Setting this value to anything not within the vehicles array will reset agent each time to random vehicle")]
    public int vehicleIndex = -1;
    private int currentVehicleIndex;

    public Vector3 targetSpawnCenter, targetSpawnSize;
    public Transform target;
    [Tooltip("The max angle the vehicle can face from the target. This spawns the vehicle facing the target randomly within this angle")]
    public float maxTargetOffsetAngle = 45;
    public float targetSpeed;
    [Tooltip("Random range")]
    public float minTargetSpeed = -100, maxTargetSpeed = 100;
    [Tooltip("If the random speed generated is less than this, then set to this (appropriated for negative or positive)")]
    public float closestToZeroTargetSpeed = 5;

    [Tooltip("The furthest the agent can be to the target before winning")]
    public float reachDistance = 2;
    [Tooltip("Minimum distance for another object to be to the agent's vehicle for the agent to fail")]
    public float collideDistance = 0.2f;

    [Space(10), Tooltip("The max the agent is allowed to steer")]
    public float maxSteer = 0.5f;

    [Space(10)]
    public float distanceHP = 1;
    public float speedHP = 1;
    public float reachHP = 5;
    public float collisionHP = 5;
    public float flipHP = 5;
    public float steerHP = 1000000;

    private float startDistance;
    //private float prevDistance;
    private int stepsPassed;
    private float accumulatedDistance;

    void Start()
    {
        //heuristicPlayer = ReInput.players.GetPlayer(heuristicPlayerId);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(vehicleSpawnCenter, vehicleSpawnSize);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(targetSpawnCenter, targetSpawnSize);
    }

    public override void AgentReset()
    {
        ResetTarget();
        ResetVehicle();
        RandomizeTargetSpeed();

        startDistance = (target.position - vehicles.currentVehicle.transform.position).magnitude;
        //prevDistance = startDistance;
        stepsPassed = 0;
        accumulatedDistance = 0;
    }
    public override void CollectObservations()
    {
        AddVectorObs(vehicles.currentVehicle.transform.position.x);
        AddVectorObs(vehicles.currentVehicle.transform.position.z);
        //AddVectorObs(vehicles.currentVehicle.transform.rotation.eulerAngles.y);
        AddVectorObs(target.position.x);
        AddVectorObs(target.position.z);
        //Vector3 targetDirection = (target.position - vehicles.currentVehicle.vehicleRigidbody.transform.position).normalized;
        //float targetAngle = Vector3.SignedAngle(targetDirection, vehicles.currentVehicle.vehicleRigidbody.transform.forward, vehicles.currentVehicle.vehicleRigidbody.transform.up);
        //AddVectorObs(targetAngle);
        AddVectorObs(vehicles.currentVehicle.transform.rotation);

        AddVectorObs(vehicles.currentVehicle.currentForwardSpeed);
        //AddVectorObs(targetSpeed);

        AddVectorObs(vehicles.index);

        var hitInfos = GetHitInfos();
        foreach (var hitInfo in hitInfos)
        {
            float hitDistance = hitInfo.hit ? hitInfo.info.distance : -1;
            AddVectorObs(hitDistance);
        }
    }
    public override void AgentAction(float[] vectorAction)
    {
        vehicles.currentVehicle.gas = vectorAction[0];
        vehicles.currentVehicle.steer = vectorAction[1] * maxSteer;

        var vehicleRigidbody = vehicles.currentVehicle.vehicleRigidbody;

        stepsPassed++;

        float currentDistance = (target.position - vehicleRigidbody.transform.position).magnitude;

        //Reset on reach target
        if (currentDistance <= reachDistance)
        {
            SetReward(reachHP);
            //Debug.Log("Reached target");
            Done();
        }
        else
        {
            //float closestDistance = float.MaxValue;
            //var hitInfos = GetHitInfos();
            //foreach (var hitInfo in hitInfos)
            //    closestDistance = Mathf.Min(hitInfo.hit ? hitInfo.info.distance : float.MaxValue, closestDistance);

            //Reset on 'collision'
            //if (closestDistance < collideDistance)
            //{
                //int stepsLeft = maxStep - stepsPassed;
                //float averageDistanceError = Mathf.Sqrt(accumulatedDistance);
                //SetReward(-(averageDistanceError * stepsLeft));
            //    Done();
            //}
            //Reset on flip
            //else if (Vector3.Dot(vehicles.currentVehicle.transform.up, Vector3.up) < 0)
            //{
                //int stepsLeft = maxStep - stepsPassed;
                //float averageDistanceError = Mathf.Sqrt(accumulatedDistance);
                //SetReward(-(averageDistanceError * stepsLeft));
            //    Done();
            //}
            //Reset on fall
            if (vehicleRigidbody.transform.position.y < -2)
            {
                Done();
            }
            else
            {
                /*Vector3 targetDirection = (target.position - vehicleRigidbody.transform.position).normalized;
                float targetDirectionPercent = vehicleRigidbody.velocity.PercentDirection(targetDirection);
                var currentTargetDirectionSpeed = vehicles.currentVehicle.currentTotalSpeed * targetDirectionPercent;
                if (currentTargetDirectionSpeed < 0.05f && currentTargetDirectionSpeed > -0.05f)
                    currentTargetDirectionSpeed = 0;

                float reward = currentTargetDirectionSpeed;
                if (currentTargetDirectionSpeed <= 0)
                    //reward = -speedHP;
                    reward = -Mathf.Pow(currentTargetDirectionSpeed - 1, 2);

                float targetAngle = Vector3.Angle(targetDirection, vehicleRigidbody.transform.forward);*/
                //Debug.Log("Current reward: " + reward + " current target directional speed: " + currentTargetDirectionSpeed);
                //accumulatedDistance += currentDistance * currentDistance;
                //if (targetAngle > maxTargetOffsetAngle || currentTargetDirectionSpeed < -speedHP)
                //    Done();
                //else
                //Debug.Log(reward);
                    //SetReward(reward);
                //SetReward(-1);

                //float steerPenalty = 0;
                //Vector3 targetDirection = (target.position - vehicles.currentVehicle.vehicleRigidbody.transform.position).normalized;
                //float targetAngle = Vector3.SignedAngle(targetDirection, vehicles.currentVehicle.vehicleRigidbody.transform.forward, vehicles.currentVehicle.vehicleRigidbody.transform.up);
                //if (targetAngle > 0 && (vehicles.currentVehicle.currentForwardSpeed > 0 && vehicles.currentVehicle.steer > 0) || (vehicles.currentVehicle.currentForwardSpeed < 0 && vehicles.currentVehicle.steer < 0))
                //    steerPenalty = -steerHP; //penalize
                //else if (targetAngle < 0 && (vehicles.currentVehicle.currentForwardSpeed > 0 && vehicles.currentVehicle.steer < 0) || (vehicles.currentVehicle.currentForwardSpeed < 0 && vehicles.currentVehicle.steer > 0))
                //    steerPenalty = -steerHP; //penalize

                float distancePenalty = -(currentDistance / (targetSpawnSize.magnitude / 2));
                if (Application.isEditor)
                    Debug.Log(distancePenalty);
                SetReward(distancePenalty / maxStep);
            }
        }
    }
    public override float[] Heuristic()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        return new float[] { vertical, horizontal };
    }

    private void ResetVehicle()
    {
        //Pick vehicle
        currentVehicleIndex = vehicleIndex;
        if (currentVehicleIndex < 0 || currentVehicleIndex >= vehicles.allVehicles.Length)
            currentVehicleIndex = Random.Range(0, vehicles.allVehicles.Length);
        vehicles.SetVehicle(currentVehicleIndex);

        //Make sure we're casting collision rays
        vehicles.currentVehicle.castRays = true;

        //Teleport vehicle to random position and rotation
        var randomPosition = new Vector3(vehicleSpawnCenter.x + vehicleSpawnSize.x / 2 * GetRandomValue(), vehicleSpawnCenter.y + vehicleSpawnSize.y / 2 * GetRandomValue(), vehicleSpawnCenter.z + vehicleSpawnSize.z / 2 * GetRandomValue());

        Vector3 targetDirection = (target.position - randomPosition).normalized;
        float targetAngle = Quaternion.LookRotation(targetDirection, Vector3.up).eulerAngles.y;
        var randomRotation = Quaternion.Euler(0, targetAngle + maxTargetOffsetAngle * (Random.value * 2 - 1), 0);
        //float randomSpeed = Random.Range(-vehicles.currentVehicle.vehicleStats.maxReverseSpeed, vehicles.currentVehicle.vehicleStats.maxForwardSpeed);
        float randomSpeed = 0;
        vehicles.currentVehicle.Teleport(transform.TransformPoint(randomPosition), transform.TransformRotation(randomRotation), randomSpeed);
    }
    private void ResetTarget()
    {
        var randomPosition = new Vector3(targetSpawnCenter.x + targetSpawnSize.x / 2 * GetRandomValue(), targetSpawnCenter.y + targetSpawnSize.y / 2 * GetRandomValue(), targetSpawnCenter.z + targetSpawnSize.z / 2 * GetRandomValue());
        target.position = transform.TransformPoint(randomPosition);
    }
    private void RandomizeTargetSpeed()
    {
        float randomSpeed = Mathf.Clamp(Random.Range(minTargetSpeed, maxTargetSpeed), -vehicles.currentVehicle.vehicleStats.maxReverseSpeed, vehicles.currentVehicle.vehicleStats.maxForwardSpeed);
        if (randomSpeed > -closestToZeroTargetSpeed && randomSpeed < closestToZeroTargetSpeed)
            randomSpeed = closestToZeroTargetSpeed * Mathf.Sign(randomSpeed);
        targetSpeed = randomSpeed;
    }

    private RaycastHitInfo[] GetHitInfos()
    {
        return new RaycastHitInfo[] { vehicles.currentVehicle.GetForwardHitInfo(), vehicles.currentVehicle.GetRightHitInfo(), vehicles.currentVehicle.GetLeftHitInfo(), vehicles.currentVehicle.GetRearHitInfo() };
    }
    private float GetRandomValue()
    {
        return (Random.value * 2) - 1;
    }

    public CarPhysics GetVehicle()
    {
        return vehicles.currentVehicle;
    }

    public Carability GetCarability()
    {
        return carability;
    }
}
