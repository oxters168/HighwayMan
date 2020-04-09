using UnityEngine;
using UnityHelpers;

public class BotDriver : MonoBehaviour, AbstractDriver
{
    public VehicleSwitcher vehicles;
    public Carability carability;

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

    public event FallenHandler onFall;
    public delegate void FallenHandler(BotDriver caller);
    public bool fallen { get; private set; }

    private float spawnSpeed;

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

    private void RespawnCheck(CarPhysics currentVehicle)
    {
        if (currentVehicle.transform.position.y < -0.5f)
        {
            fallen = true;
            onFall?.Invoke(this);
        }
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
        float nextGas = currentVehicle.currentForwardSpeed < spawnSpeed ? 1 : 0;

        var forwardHitInfo = currentVehicle.GetForwardHitInfo();
        var leftHitInfo = currentVehicle.GetLeftHitInfo();
        var rightHitInfo = currentVehicle.GetRightHitInfo();
        if (forwardHitInfo.hit || leftHitInfo.hit || rightHitInfo.hit)
        {
            //Distance from obstacle to gas percent
            if (forwardHitInfo.hit)
            {
                float distanceToObstacle = forwardHitInfo.info.distance;
                float predictedDistanceTravel = currentVehicle.currentForwardSpeed * Time.fixedDeltaTime;
                nextGas = -(1 - Mathf.Clamp01(predictedDistanceTravel / distanceToObstacle));
                //nextGas = -(1 - forwardHitInfo.distance / forwardDistanceObstacleCheck);
            }

            //Steer from obstacle
            if (forwardHitInfo.hit)
            {
                float obstacleAngle = currentVehicle.GetHitAngle(forwardHitInfo);
                if (obstacleAngle > 0 && currentVehicle.currentForwardSpeed > 0 || obstacleAngle < 0 && currentVehicle.currentForwardSpeed < 0)
                    nextSteer = -1;
                else
                    nextSteer = 1;
            }
            else if (leftHitInfo.hit && currentVehicle.currentForwardSpeed > 0 || rightHitInfo.hit && currentVehicle.currentForwardSpeed < 0)
                nextSteer = 1;
            else if (rightHitInfo.hit && currentVehicle.currentForwardSpeed > 0 || leftHitInfo.hit && currentVehicle.currentForwardSpeed < 0)
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
                ChangeLane();

            lastTargetSwitchTime = Time.time;
        }
    }
    public void ChangeLane()
    {
        float randomValue = Mathf.Pow(Random.value * 2 - 1, 9);
        int lanesToShift = Mathf.RoundToInt((randomValue + Mathf.Sign(randomValue) * 1) * (targets.Length - 1));
        int nextTargetIndex = Mathf.Clamp(currentTargetIndex + lanesToShift, 0, targets.Length - 1);
        if (nextTargetIndex == currentTargetIndex)
            nextTargetIndex = Mathf.Clamp(currentTargetIndex - lanesToShift, 0, targets.Length - 1);

        currentTargetIndex = nextTargetIndex;
    }

    public void RespawnRandomVehicle(Vector3 spawnPosition, Quaternion spawnRotation, float speed)
    {
        Respawn(spawnPosition, spawnRotation, Random.Range(0, vehicles.allVehicles.Length), speed);
    }
    public void Respawn(Vector3 spawnPosition, Quaternion spawnRotation, int vehicleIndex, float speed)
    {
        currentTargetIndex = defaultTargetIndex;

        vehicles.SetVehicle(Mathf.Clamp(vehicleIndex, 0, vehicles.allVehicles.Length));
        vehicles.currentVehicle.castRays = true;

        spawnSpeed = speed;

        vehicles.currentVehicle.Teleport(spawnPosition, spawnRotation, spawnSpeed);

        fallen = false;
    }

    public CarPhysics GetVehicle()
    {
        return vehicles.currentVehicle;
    }
    public Carability GetCarability()
    {
        return carability;
    }
    public CarAppearance GetCarAppearance()
    {
        //return GetComponent<CarAppearance>();
        return vehicles.currentVehicle.GetComponent<CarAppearance>();
    }
    public CarHUD GetCarHUD()
    {
        return GetComponent<CarHUD>();
    }
}
