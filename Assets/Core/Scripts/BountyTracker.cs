using UnityEngine;
using UnityHelpers;

public class BountyTracker : MonoBehaviour
{
    public static BountyTracker bountyTrackerInScene;

    [Tooltip("An object that has Abstract Driver attached")]
    public GameObject bountyHunterObject;
    private AbstractDriver bountyHunter;

    public Repeater highway;
    public CarSpawner carSpawner;

    public CarStats[] whiteListedVehicles;
    public ColorPercentage[] randomVehicleColorOptions;
    public int activeBountyCount = 5;
    public int minBounty = 50, maxBounty = 100;

    public BountyData[] currentBounties;

    void Awake()
    {
        bountyTrackerInScene = this;

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
            currentBounties[bountyIndex].caught = true;
            currentBounties[bountyIndex].driver.GetVehicle().Teleport(Vector3.up * -5, Quaternion.identity);
            ReplaceBounty(bountyIndex);
        }
        return value;
    }
    public void ReplaceBounty(string license)
    {
        ReplaceBounty(GetBountyIndex(license));
    }
    public void ReplaceBounty(BountyData bounty)
    {
        ReplaceBounty(GetBountyIndex(bounty));
    }
    private void ReplaceBounty(int index)
    {
        if (index >= 0 && index < currentBounties.Length)
            currentBounties[index] = GenerateRandomBounty();
        else
            Debug.LogError("BountyTracker: Could not replace bounty, given index is invalid");
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
    private int GetBountyIndex(BountyData bounty)
    {
        return System.Array.IndexOf(currentBounties, bounty);
    }

    private BountyData GenerateRandomBounty()
    {
        return new BountyData
        {
            carData = new CarData
            {
                vehicleColor = ColorPercentage.PickColor(randomVehicleColorOptions).color,
                vehicleIndex = (uint)whiteListedVehicles[Random.Range(0, whiteListedVehicles.Length)].index,
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
    public bool caught;
    public CarData carData;
    public uint reward;
    public bool headingNorth;
    public float position;
    public int lane;

    public AbstractDriver driver;
}
