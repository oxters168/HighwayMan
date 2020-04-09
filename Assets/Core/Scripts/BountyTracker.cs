using UnityEngine;

public class BountyTracker : MonoBehaviour
{
    [Tooltip("An object that has Abstract Driver attached")]
    public GameObject bountyHunterObject;
    private AbstractDriver bountyHunter;

    public Repeater highway;
    public CarSpawner carSpawner;

    public Color[] randomVehicleColorOptions;
    public int carTypes;
    public int activeBountyCount = 5;
    public int minBounty = 50, maxBounty = 100;

    public BountyData[] currentBounties;

    void Start()
    {
        bountyHunter = bountyHunterObject.GetComponent<AbstractDriver>();

        currentBounties = new BountyData[activeBountyCount];
        for (int i = 0; i < currentBounties.Length; i++)
            currentBounties[i] = GenerateRandomBounty();
    }
    void Update()
    {
        for (int i = 0; i < currentBounties.Length; i++)
        {
            var currentBounty = currentBounties[i];
            if (currentBounty.driver != null)
            {
                if (((BotDriver)currentBounty.driver).fallen)
                    currentBounty.driver = null;
                else
                    currentBounty.position = currentBounty.driver.GetVehicle().transform.position.z;
            }
            else
            {
                var bountyHunterVehicle = bountyHunter.GetVehicle();
                float bountyHunterPosition = 0;
                if (bountyHunterVehicle != null)
                    bountyHunterPosition = bountyHunterVehicle.transform.position.z;

                if (Mathf.Abs(bountyHunterPosition - currentBounty.position) <= GetMinDistance())
                    currentBounty.driver = carSpawner.SpawnBot(currentBounty.headingNorth, currentBounty.lane, true, 0, currentBounty.carData);
                else
                    currentBounty.position = Mathf.Clamp(currentBounty.position + (currentBounty.headingNorth ? 1 : -1) * 10 * Time.deltaTime, bountyHunterPosition - GetMaxDistance(), bountyHunterPosition + GetMaxDistance());
            }
        }
    }

    public int Catch(string license)
    {
        int bountyIndex = GetBountyIndex(license);
        int value = 0;
        if (bountyIndex >= 0)
        {
            value = (int)currentBounties[bountyIndex].reward;
            currentBounties[bountyIndex] = GenerateRandomBounty();
        }
        return value;
    }
    public bool HasBounty(string license)
    {
        return GetBountyIndex(license) >= 0;
    }

    private int GetBountyIndex(string license)
    {
        int index = -1;
        for (int i = 0; i < currentBounties.Length; i++)
            if (currentBounties[i].carData.vehicleLicense.Equals(license))
            {
                index = i;
                break;
            }

        return index;
    }

    private BountyData GenerateRandomBounty()
    {
        return new BountyData
        {
            carData = new CarData
            {
                vehicleColor = randomVehicleColorOptions[Random.Range(0, randomVehicleColorOptions.Length)],
                vehicleIndex = (uint)Random.Range(0, carTypes),
                vehicleLicense = Carability.GetRandomLicense(),
            },
            reward = (uint)Random.Range(minBounty, maxBounty + 1),
            headingNorth = (Random.value * 2 - 1) >= 0,
            position = highway.target.position.z + Mathf.Sign(Random.value * 2 - 1) * Random.Range(GetMinDistance(), GetMaxDistance()),
            lane = Random.Range(0, carSpawner.halfRoadLaneCount)
        };
    }

    private float GetMinDistance()
    {
        return highway.size / 2;
    }
    private float GetMaxDistance()
    {
        return highway.size * 5;
    }
}

[System.Serializable]
public class BountyData
{
    public bool isSpooked;
    public CarData carData;
    public uint reward;
    public bool headingNorth;
    public float position;
    public int lane;

    public AbstractDriver driver;
}
