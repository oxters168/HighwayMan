using UnityEngine;
using UnityHelpers;

[System.Serializable]
public class OnCarEnteredBuildingEvent : UnityEngine.Events.UnityEvent<CarPhysics, BuildingController.BuildingType> { }

public class Repeater : MonoBehaviour
{
    public Transform target;
    public GameObject extraFront;
    public RoadPart front, mid, rear;
    public GameObject extraRear;

    public float size = 50;
    public float offset = 25;

    [Space(10)]
    public Transform pointsOfInterest;

    [Space(10), Tooltip("Every Nth road part place a parking")]
    public int parkingEveryNth = 5;
    [Tooltip("Probability of building to appear, index corresponds to BuildingController.BuildingType enum")]
    public float[] buildingOdds;

    private FastNoise buildingsNoise;

    private int previousIndex = int.MinValue;

    [Space(10)]
    public OnCarEnteredBuildingEvent onCarEnteredBuilding;

    private void Start()
    {
        buildingsNoise = new FastNoise((int)System.DateTime.UtcNow.Ticks);

        front.buildings.onCarParked += Buildings_onCarParked; //Shouldn't actually need to do this one since car can't get out of mid anyways
        mid.buildings.onCarParked += Buildings_onCarParked;
        rear.buildings.onCarParked += Buildings_onCarParked; //Shouldn't actually need to do this one since car can't get out of mid anyways
    }

    private void Buildings_onCarParked(CarPhysics car, BuildingController.BuildingType buildingType)
    {
        onCarEnteredBuilding?.Invoke(car, buildingType);
    }

    void Update()
    {
        int index = Mathf.CeilToInt((target.position.z - offset) / size);

        if (index != previousIndex)
        {
            previousIndex = index;

            int extraFrontIndex = index + 2;
            int frontIndex = index + 1;
            int midIndex = index;
            int rearIndex = index - 1;
            int extraRearIndex = index - 2;

            SetRoadType(front, frontIndex);
            SetRoadType(mid, midIndex);
            SetRoadType(rear, rearIndex);

            extraFront.transform.position = new Vector3(extraFront.transform.position.x, extraFront.transform.position.y, extraFrontIndex * size);
            front.transform.position = new Vector3(front.transform.position.x, front.transform.position.y, frontIndex * size);
            mid.transform.position = new Vector3(mid.transform.position.x, mid.transform.position.y, midIndex * size);
            rear.transform.position = new Vector3(rear.transform.position.x, rear.transform.position.y, rearIndex * size);
            extraRear.transform.position = new Vector3(extraRear.transform.position.x, extraRear.transform.position.y, extraRearIndex * size);

            if (pointsOfInterest != null)
                pointsOfInterest.position = new Vector3(pointsOfInterest.position.x, pointsOfInterest.position.y, midIndex * size);

        }
    }

    private void SetRoadType(RoadPart road, int roadIndex)
    {
        var roadType = RoadPart.RoadType.normal;
        var buildingType = BuildingController.BuildingType.none;

        if (roadIndex % parkingEveryNth == 0)
        {
            int parkingIndex = roadIndex / parkingEveryNth;
            if (parkingIndex % 2 == 0)
                roadType = RoadPart.RoadType.parkingOnRight;
            else
                roadType = RoadPart.RoadType.parkingOnLeft;

            #region Pick building
            float buildingRng = (buildingsNoise.GetWhiteNoiseInt(parkingIndex, 199) + 1) / 2;
            buildingType = BuildingController.BuildingType.none;
            float lowestPossibleOdd = float.MaxValue;
            if (buildingOdds != null && buildingOdds.Length == System.Enum.GetNames(typeof(BuildingController.BuildingType)).Length)
            {
                for (int i = 0; i < buildingOdds.Length; i++)
                {
                    if (buildingRng <= buildingOdds[i] && buildingOdds[i] < lowestPossibleOdd)
                    {
                        buildingType = (BuildingController.BuildingType)i;
                        lowestPossibleOdd = buildingOdds[i];
                    }
                }
            }
            else
                Debug.LogError("Repeater(" + gameObject.name + "): Building odds array length should equal to BuildingController.BuildingType length");
            #endregion
        }

        road.SetRoadType(roadType);
        road.SetBuilding(buildingType);
    }
}
