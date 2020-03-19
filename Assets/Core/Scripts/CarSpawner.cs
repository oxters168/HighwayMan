using UnityEngine;
using UnityHelpers;

public class CarSpawner : MonoBehaviour
{
    private ObjectPool<Transform> botCars;

    [Tooltip("Will check speed and spawn traffic based on this driver (should implement AbstractDriver)")]
    public GameObject adjustTrafficTo;
    private AbstractDriver adjustTrafficToDriver;

    [Space(10), Tooltip("Max spawned vehicles at once")]
    public int trafficCount = 10;
    [Tooltip("Vehicles/Second")]
    public float spawnRate = 1;
    private float lastSpawn = float.MinValue;
    [Tooltip("No touchie")]
    public int currentSpawnedVehicleCount;

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

    private void Start()
    {
        botCars = PoolManager.GetPool("Bots");
        adjustTrafficToDriver = adjustTrafficTo.GetComponent<AbstractDriver>();
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

            var roadPoints = rightRoadPoints;
            if (index >= halfRoadLaneCount)
                roadPoints = leftRoadPoints;

            //Get vehicle speed relative to highway
            float percentDirection = adjustTrafficToDriver.GetVehicle().vehicleRigidbody.velocity.normalized.PercentDirection(Vector3.forward);
            float vehicleHighwaySpeed = adjustTrafficToDriver.GetVehicle().currentTotalSpeed * percentDirection;

            //Switch spawn side depending on vehicle speed relative to highway
            int offsetIndex = 0;
            if (vehicleHighwaySpeed > speedLimit - maxLimitDeviation && index < halfRoadLaneCount)
                offsetIndex = halfRoadLaneCount;
            else if (vehicleHighwaySpeed < -speedLimit + maxLimitDeviation && index >= halfRoadLaneCount)
                offsetIndex = halfRoadLaneCount;

            //Spawn vehicle
            int wrappedIndex = index % halfRoadLaneCount;
            Transform spawnPoint = roadPoints[offsetIndex + wrappedIndex];
            float speed = Random.Range(speedLimit - maxLimitDeviation, speedLimit + maxLimitDeviation);
            Transform[] targets = GetTargets(roadPoints, halfRoadLaneCount);
            SpawnBot(spawnPoint, speed, targets, wrappedIndex);
            currentSpawnedVehicleCount++;
            lastSpawn = Time.time;
        }
    }
    private void SpawnBot(Transform spawnPoint, float speed, Transform[] targets, int targetIndex)
    {
        botCars.Get<BotDriver>((bot) =>
        {
            bot.RespawnRandomVehicle(spawnPoint, speed);
            bot.GetVehicle().vehicleHealth.HealPercent(1);
            //bot.GetCarability().RandomizeLicense();
            bot.targets = targets;
            bot.currentTargetIndex = targetIndex;
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
