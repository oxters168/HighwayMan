using UnityEngine;
using UnityHelpers;

public class CarSpawner : MonoBehaviour
{
    private ObjectPool<Transform> botCars;

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

    public Color[] randomVehicleColorOptions;

    [Tooltip("In m/s")]
    public float speedLimit = 27.78f;
    [Tooltip("How much deviation is allowed from speed limit")]
    public float maxLimitDeviation = 5;

    [Space(10)]
    public bool debugNoise;
    public int seed = 1337;
    public float posY = 10;
    public float offsetX = 0, offsetY = 0;
    public int columns = 10, rows = 10;
    public float size = 1;
    public float frequency = 1;
    public FastNoise.NoiseType noiseType;
    public FastNoise.FractalType fractalType;
    public float fractalGain = 0;
    public float fractalLacunarity = 0;
    public int fractalOctaves = 0;

    [Space(10), Tooltip("The number of lanes of each road")]
    public int halfRoadLaneCount = 5;
    [Tooltip("rearSpawn1, rearSpawn2 .. rearSpawn(rightRoadLaneCount), frontSpawn1, frontSpawn2 .. frontSpawn(rightRoadLaneCount), target1, target2 .. target(rightRoadLaneCount)")]
    public Transform[] rightRoadPoints;

    //public int leftRoadLaneCount = 5;
    [Tooltip("rearSpawn1, rearSpawn2 .. rearSpawn(leftRoadLaneCount), frontSpawn1, frontSpawn2 .. frontSpawn(leftRoadLaneCount), target1, target2 .. target(leftRoadLaneCount)")]
    public Transform[] leftRoadPoints;

    private void Awake()
    {
        botCars = PoolManager.GetPool("Bots");
        driverToAdjustTrafficTo = adjustTrafficTo.GetComponent<AbstractDriver>();
    }
    private void Update()
    {
        SpawnTraffic();
    }
    private void OnDrawGizmos()
    {
        if (debugNoise)
        {
            var fn = GetTrafficNoise();
            for (int col = 0; col < columns; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    float posX = col * size;
                    float posZ = row * size;
                    float colorLerp = fn.GetNoise(posX + offsetX, posZ + offsetY);
                    Gizmos.color = new Color(colorLerp, colorLerp, colorLerp);
                    Gizmos.DrawCube(new Vector3(posX, posY, posZ), Vector3.one * size);
                }
            }
        }
    }

    private FastNoise GetTrafficNoise()
    {
        FastNoise fn = new FastNoise(seed);
        fn.SetFrequency(frequency);
        fn.SetNoiseType(noiseType);
        fn.SetFractalType(fractalType);
        fn.SetFractalGain(fractalGain);
        fn.SetFractalLacunarity(fractalLacunarity);
        fn.SetFractalOctaves(fractalOctaves);
        return fn;
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

        return botCars.Get<BotDriver>((bot) =>
        {
            Color vehicleColor;

            if (carData != null)
            {
                bot.Respawn(spawnPoint.position, spawnPoint.rotation, (int)carData.vehicleIndex, speed);
                bot.GetCarability().license = carData.vehicleLicense;
                vehicleColor = carData.vehicleColor;
            }
            else
            {
                bot.RespawnRandomVehicle(spawnPoint.position, spawnPoint.rotation, speed);
                bot.GetCarability().RandomizeLicense();
                vehicleColor = randomVehicleColorOptions[Random.Range(0, randomVehicleColorOptions.Length)];
            }

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
        botCars.Return(caller.transform);
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
