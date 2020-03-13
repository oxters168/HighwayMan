using MLAgents;
using UnityEngine;
using UnityHelpers;

public class AgentDriver : Agent, AbstractDriver
{
    public VehicleSwitcher vehicles;

    public Vector3 vehicleSpawnCenter, vehicleSpawnSize;

    [Tooltip("Setting this value to anything not within the vehicles array will reset agent each time to random vehicle")]
    public int vehicleIndex = -1;
    private int currentVehicleIndex;

    public Vector3 targetSpawnCenter, targetSpawnSize;
    public Transform target;
    public float targetSpeed;
    [Tooltip("Random range")]
    public float minTargetSpeed = -100, maxTargetSpeed = 100;
    [Tooltip("If the random speed generated is less than this, then set to this (appropriated for negative or positive)")]
    public float closestToZeroTargetSpeed = 5;

    [Tooltip("The furthest the agent can be to the target before winning")]
    public float reachDistance = 2;
    [Tooltip("Minimum distance for another object to be to the agent's vehicle for the agent to fail")]
    public float collideDistance = 0.2f;

    [Space(10)]
    public float distanceHP = 1;
    public float speedHP = 1;
    public float reachHP = 5;
    public float collisionHP = 5;
    public float flipHP = 5;

    private float startDistance;
    private float prevDistance;

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
        ResetVehicle();
        ResetTarget();
        RandomizeTargetSpeed();

        startDistance = (target.position - vehicles.currentVehicle.transform.position).magnitude;
        prevDistance = startDistance;
    }
    public override void CollectObservations()
    {
        AddVectorObs(vehicles.currentVehicle.transform.position.x);
        AddVectorObs(vehicles.currentVehicle.transform.position.z);
        AddVectorObs(vehicles.currentVehicle.transform.rotation.eulerAngles.y);
        AddVectorObs(target.position.x);
        AddVectorObs(target.position.z);

        AddVectorObs(vehicles.currentVehicle.currentForwardSpeed);
        AddVectorObs(targetSpeed);

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
        vehicles.currentVehicle.steer = vectorAction[1];

        float currentDistance = (target.position - vehicles.currentVehicle.transform.position).magnitude;
        //SetReward(-currentDistance);
        SetReward(-1);
        //SetReward(prevDistance - currentDistance);

        //float currentDistanceReward = (currentDistance / startDistance) * distanceHP;
        //float prevDistanceReward = (prevDistance / startDistance) * distanceHP;
        //float changeInDistanceReward = Mathf.Clamp(prevDistanceReward - currentDistanceReward, -1, 1);
        //prevDistance = currentDistance;

        //float speedReward = -Mathf.Abs((targetSpeed - vehicles.currentVehicle.currentSpeed) / targetSpeed) * speedPunishHP;

        //SetReward(distanceReward + speedReward);

        //Reset on reach target
        if (currentDistance <= reachDistance)
        {
            //SetReward(reachHP + changeInDistanceReward);
            SetReward(reachHP);
            Done();
        }
        //else
        //    SetReward(changeInDistanceReward);

        //Reset on 'collision'
        float closestDistance = float.MaxValue;
        var hitInfos = GetHitInfos();
        foreach (var hitInfo in hitInfos)
            closestDistance = Mathf.Min(hitInfo.hit ? hitInfo.info.distance : float.MaxValue, closestDistance);
        if (closestDistance < collideDistance)
        {
            SetReward(-collisionHP);
            Done();
        }

        //Reset on flip
        if (Vector3.Dot(vehicles.currentVehicle.transform.up, Vector3.up) < 0)
        {
            //Debug.Log("Flipped over");
            SetReward(-flipHP);
            Done();
        }

        //Reset on fall
        if (vehicles.currentVehicle.transform.position.y < -2)
        {
            //Debug.Log("Fell down the rabbit hole");
            Done();
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
        var randomRotation = Quaternion.Euler(0, 360 * Random.value, 0);
        vehicles.currentVehicle.Teleport(transform.TransformPoint(randomPosition), transform.TransformRotation(randomRotation));
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
}
