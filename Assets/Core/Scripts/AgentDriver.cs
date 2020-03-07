using MLAgents;
using UnityEngine;
using UnityHelpers;

public class AgentDriver : Agent, AbstractDriver
{
    public VehicleSwitcher vehicles;

    public Vector3 spawnCenter, spawnSize;

    [Tooltip("Setting this value to anything not within the vehicles array will reset agent each time to random vehicle")]
    public int vehicleIndex = -1;
    private int currentVehicleIndex;

    public Vector3 targetCenter, targetSize;
    public Transform target;
    public float targetSpeed;
    [Tooltip("When to start rewarding speed")]
    public float targetSpeedOffset = 5;
    [Tooltip("Random range")]
    public float minSpeed = -100, maxSpeed = 100;
    [Tooltip("If the random speed generated is closer to 0 than this, then reset to this")]
    public float closestToZeroSpeed = 5;

    [Tooltip("The furthest the agent can be to the target before winning")]
    public float reachDistance = 2;
    [Tooltip("Minimum distance for another object to be to the agent's vehicle for the agent to fail")]
    public float collideDistance = 0.2f;

    //private float accumulatedStepReward;
    private float startSqrDistance;
    //private float distanceReward, speedReward;

    private float minSpeedOffset;
    private float minSqrDistance;

    void Start()
    {
        //heuristicPlayer = ReInput.players.GetPlayer(heuristicPlayerId);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(spawnCenter, spawnSize);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(targetCenter, targetSize);
    }

    public override void AgentReset()
    {
        ResetVehicle();
        ResetTarget();

        float randomSpeed = Mathf.Clamp(Random.Range(minSpeed, maxSpeed), -vehicles.currentVehicle.vehicleStats.maxReverseSpeed, vehicles.currentVehicle.vehicleStats.maxForwardSpeed);
        if (randomSpeed > -closestToZeroSpeed && randomSpeed < closestToZeroSpeed)
            randomSpeed = closestToZeroSpeed * Mathf.Sign(randomSpeed);
        targetSpeed = randomSpeed;

        startSqrDistance = (vehicles.currentVehicle.transform.position - target.position).sqrMagnitude;
        //distanceReward = 0;
        //speedReward = 0;

        minSqrDistance = startSqrDistance;
        minSpeedOffset = targetSpeed;
    }
    public override void CollectObservations()
    {
        AddVectorObs(target.position);
        AddVectorObs(targetSpeed);
        AddVectorObs(vehicles.currentVehicle.transform.position);
        AddVectorObs(vehicles.currentVehicle.transform.rotation);
        AddVectorObs(vehicles.currentVehicle.currentSpeed);
        //AddVectorObs(vehicles.currentVehicle.vehicleRigidbody.velocity);
        //AddVectorObs(vehicles.currentVehicle.vehicleRigidbody.angularVelocity);

        var hitInfos = GetHitInfos();
        foreach (var hitInfo in hitInfos)
        {
            //Vector3 hitPoint = hitInfo.info.point;
            //if (!hitInfo.hit)
            //    hitPoint = hitInfo.rayStart + hitInfo.rayStartDirection * hitInfo.rayMaxDistance;
            float hitDistance = hitInfo.hit ? hitInfo.info.distance : -1;
            AddVectorObs(hitDistance);
        }

        //base.CollectObservations(sensor);
    }
    public override void AgentAction(float[] vectorAction)
    {
        vehicles.currentVehicle.gas = vectorAction[0];
        vehicles.currentVehicle.steer = vectorAction[1];
        //Debug.Log("ActionVertical: " + vectorAction[0]);
        //Debug.Log("ActionHorizontal: " + vectorAction[1]);
        //base.AgentAction(vectorAction);

        float currentSqrDistance = (target.position - vehicles.currentVehicle.transform.position).sqrMagnitude;

        //Give distance reward
        float distanceReward = 0;
        //if (currentSqrDistance < minSqrDistance)
        //{
            distanceReward = CalculateDistanceReward(currentSqrDistance);
            float prevDistanceReward = CalculateDistanceReward(minSqrDistance);
            distanceReward = distanceReward - prevDistanceReward;
            minSqrDistance = currentSqrDistance;
        //}

        //Give speed reward
        float speedReward = 0;
        float currentSpeedOffset = targetSpeed - vehicles.currentVehicle.currentSpeed;
        //if (currentSpeedOffset < minSpeedOffset)
        //{
            speedReward = CalculateSpeedReward(vehicles.currentVehicle.currentSpeed);
            float prevSpeedReward = CalculateSpeedReward(targetSpeed - minSpeedOffset);
            speedReward = speedReward - prevSpeedReward;
            minSpeedOffset = currentSpeedOffset;
        //}

        //Win on reach target
        if (currentSqrDistance <= reachDistance * reachDistance)
        {
            //Debug.Log("We did it WE DID IT");
            Debug.Log("FinalReward: " + distanceReward + " + " + speedReward + " + " + 0.333f + " = " + (distanceReward + speedReward + 0.333f));
            SetReward(distanceReward + speedReward + 0.333f);
            Done();
        }
        else
        {
            Debug.Log("SubRewarded: " + distanceReward + " + " + speedReward + " = " + (distanceReward + speedReward));
            SetReward(distanceReward + speedReward);
        }

        //Reset on 'collision'
        float closestDistance = float.MaxValue;
        var hitInfos = GetHitInfos();
        foreach (var hitInfo in hitInfos)
            closestDistance = Mathf.Min(hitInfo.hit ? hitInfo.info.distance : float.MaxValue, closestDistance);
        if (closestDistance < collideDistance)
        {
            //Debug.Log("Got too close to something");
            SetReward(-2);
            Done();
        }

        //Reset on flip
        if (Vector3.Dot(vehicles.currentVehicle.transform.up, Vector3.up) < 0)
        {
            //Debug.Log("Flipped over");
            SetReward(-2);
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
        //Debug.Log("Vertical: " + vertical);
        //Debug.Log("Horizontal: " + horizontal);

        return new float[] { vertical, horizontal };
    }

    private float CalculateDistanceReward(float sqrDistance)
    {
        float reachSqrDistance = Mathf.Min(reachDistance * reachDistance, startSqrDistance);
        float distancePercent = sqrDistance / (startSqrDistance - reachSqrDistance);
        distancePercent = 1 - distancePercent;
        return Mathf.Clamp(distancePercent, -1, 1) * 0.333f;
    }
    private float CalculateSpeedReward(float speed)
    {
        return Mathf.Clamp(speed / targetSpeed, -1, 1) * 0.333f;
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
        var randomPosition = new Vector3(spawnCenter.x + spawnSize.x / 2 * GetRandomValue(), spawnCenter.y + spawnSize.y / 2 * GetRandomValue(), spawnCenter.z + spawnSize.z / 2 * GetRandomValue());
        var randomRotation = Quaternion.Euler(0, 360 * Random.value, 0);
        vehicles.currentVehicle.Teleport(randomPosition, randomRotation);
    }
    private void ResetTarget()
    {
        var randomPosition = new Vector3(targetCenter.x + targetSize.x / 2 * GetRandomValue(), targetCenter.y + targetSize.y / 2 * GetRandomValue(), targetCenter.z + targetSize.z / 2 * GetRandomValue());
        target.position = randomPosition;
    }

    private RaycastHitInfo[] GetHitInfos()
    {
        return new RaycastHitInfo[] { vehicles.currentVehicle.GetForwardHitInfo(), vehicles.currentVehicle.GetRightHitInfo(), vehicles.currentVehicle.GetLeftHitInfo(), vehicles.currentVehicle.GetRearHitInfo() };
    }

    public CarPhysics GetVehicle()
    {
        return vehicles.currentVehicle;
    }

    private float GetRandomValue()
    {
        return (Random.value * 2) - 1;
    }
}
