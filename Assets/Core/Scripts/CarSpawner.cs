using UnityEngine;
using UnityHelpers;

public class CarSpawner : MonoBehaviour
{
    private ObjectPool<Transform> BotCars { get { if (_botCars == null) _botCars = PoolManager.GetPool("Bots"); return _botCars; } }
    private ObjectPool<Transform> _botCars;

    [Tooltip("Will check speed and spawn traffic based on this driver (should implement AbstractDriver)")]
    public GameObject adjustTrafficTo;
    private AbstractDriver driverToAdjustTrafficTo;

    [Space(10), Tooltip("Max spawned vehicles at once")]
    public int trafficCount = 10;
    [Tooltip("Vehicles/Second")]
    public float spawnRate = 1;
    private float lastSpawn = float.MinValue;
    [Tooltip("No touchie")]
    public int currentSpawnedVehicleCount;

    public CarStats[] whiteListedVehicles;
    public ColorPercentage[] randomVehicleColorOptions;

    [Tooltip("In m/s")]
    public float speedLimit = 27.78f;
    [Tooltip("How much deviation is allowed from speed limit")]
    public float maxLimitDeviation = 5;

    [Space(10), Tooltip("The number of lanes of each road")]
    public int halfRoadLaneCount = 5;
    [Tooltip("rearSpawn1, rearSpawn2 .. rearSpawn(rightRoadLaneCount), frontSpawn1, frontSpawn2 .. frontSpawn(rightRoadLaneCount), target1, target2 .. target(rightRoadLaneCount)")]
    public Transform[] rightRoadPoints;

    //public int leftRoadLaneCount = 5;
    [Tooltip("rearSpawn1, rearSpawn2 .. rearSpawn(leftRoadLaneCount), frontSpawn1, frontSpawn2 .. frontSpawn(leftRoadLaneCount), target1, target2 .. target(leftRoadLaneCount)")]
    public Transform[] leftRoadPoints;

    private void Awake()
    {
        //botCars = PoolManager.GetPool("Bots");
        driverToAdjustTrafficTo = adjustTrafficTo.GetComponent<AbstractDriver>();
    }
    private void Update()
    {
        SpawnTraffic();
    }

    private void SpawnTraffic()
    {
        if (Time.time - lastSpawn >= 1 / spawnRate && currentSpawnedVehicleCount < trafficCount)
        {
            int index = Random.Range(0, halfRoadLaneCount * 2);

            bool spawnOnRight = true;
            if (index >= halfRoadLaneCount)
                spawnOnRight = false;

            //Spawn vehicle
            int wrappedIndex = index % halfRoadLaneCount;
            SpawnBot(spawnOnRight, wrappedIndex);
        }
    }
    public BotDriver SpawnBot(bool spawnOnRight, int spawnIndex, bool setRandomSpeed = true, float speed = 0, CarData carData = null)
    {
        var roadPoints = rightRoadPoints;
        if (!spawnOnRight)
            roadPoints = leftRoadPoints;

        //Get vehicle speed relative to highway
        float vehicleToAdjustTrafficToHighwaySpeed = 0;
        if (driverToAdjustTrafficTo != null)
        {
            var vehicleToAdjustTrafficTo = driverToAdjustTrafficTo.GetVehicle();
            if (vehicleToAdjustTrafficTo != null)
            {
                float percentDirection = vehicleToAdjustTrafficTo.vehicleRigidbody.velocity.normalized.PercentDirection(Vector3.forward);
                vehicleToAdjustTrafficToHighwaySpeed = vehicleToAdjustTrafficTo.currentTotalSpeed * percentDirection;
            }
        }

        //Switch spawn side depending on vehicle speed relative to highway
        int offsetIndex = 0;
        if (vehicleToAdjustTrafficToHighwaySpeed > speedLimit - maxLimitDeviation && spawnOnRight)
            offsetIndex = halfRoadLaneCount;
        else if (vehicleToAdjustTrafficToHighwaySpeed < -speedLimit + maxLimitDeviation && !spawnOnRight)
            offsetIndex = halfRoadLaneCount;

        Transform spawnPoint = roadPoints[offsetIndex + spawnIndex];
        Transform[] targets = GetTargets(roadPoints, halfRoadLaneCount);

        if (setRandomSpeed)
            speed = Random.Range(speedLimit - maxLimitDeviation, speedLimit + maxLimitDeviation);

        currentSpawnedVehicleCount++;
        lastSpawn = Time.time;

        return BotCars.Get<BotDriver>((bot) =>
        {
            Color vehicleColor;
            int vehicleIndex;

            if (carData != null)
            {
                vehicleIndex = (int)carData.vehicleIndex;
                bot.GetCarability().license = carData.vehicleLicense;
                vehicleColor = carData.vehicleColor;
            }
            else
            {
                vehicleIndex = whiteListedVehicles[Random.Range(0, whiteListedVehicles.Length)].index;
                bot.GetCarability().RandomizeLicense();
                vehicleColor = ColorPercentage.PickColor(randomVehicleColorOptions).color;
            }

            bot.Respawn(spawnPoint.position, spawnPoint.rotation, vehicleIndex, speed);

            var carAppearance = bot.GetCarAppearance();
            if (carAppearance != null)
                carAppearance.color = vehicleColor;

            bot.GetVehicle().vehicleHealth.SetPercent(1);
            bot.GetCarHUD().showLicense = false;
            bot.GetCarHUD().showPointer = false;
            bot.targets = targets;
            bot.currentTargetIndex = spawnIndex;
            bot.onFall += Bot_onFall;
        });
    }
    private void Bot_onFall(BotDriver caller)
    {
        currentSpawnedVehicleCount--;
        caller.onFall -= Bot_onFall;
        BotCars.Return(caller.transform);
    }

    private static Transform[] GetTargets(Transform[] roadPoints, int laneCount)
    {
        int startIndex = laneCount * 2;
        Transform[] targets = new Transform[laneCount];
        for (int i = 0; i < targets.Length; i++)
            targets[i] = roadPoints[startIndex + i];

        return targets;
    }
}
