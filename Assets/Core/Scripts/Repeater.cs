using UnityEngine;

public class Repeater : MonoBehaviour
{
    public Transform target;
    public RoadPart extraFront, front, mid, rear, extraRear;

    public float size = 50;
    public float offset = 25;

    [Tooltip("Every Nth road part place a parking")]
    public int parkingEveryNth = 5;

    public Transform pointsOfInterest;

    void Update()
    {
        int index = Mathf.CeilToInt((target.position.z - offset) / size);
        int extraFrontIndex = index + 2;
        int frontIndex = index + 1;
        int midIndex = index;
        int rearIndex = index - 1;
        int extraRearIndex = index - 2;

        SetRoadType(extraFront, extraFrontIndex);
        SetRoadType(front, frontIndex);
        SetRoadType(mid, midIndex);
        SetRoadType(rear, rearIndex);
        SetRoadType(extraRear, extraRearIndex);

        extraFront.transform.position = new Vector3(extraFront.transform.position.x, extraFront.transform.position.y, extraFrontIndex * size);
        front.transform.position = new Vector3(front.transform.position.x, front.transform.position.y, frontIndex * size);
        mid.transform.position = new Vector3(mid.transform.position.x, mid.transform.position.y, midIndex * size);
        rear.transform.position = new Vector3(rear.transform.position.x, rear.transform.position.y, rearIndex * size);
        extraRear.transform.position = new Vector3(extraRear.transform.position.x, extraRear.transform.position.y, extraRearIndex * size);

        pointsOfInterest.position = new Vector3(pointsOfInterest.position.x, pointsOfInterest.position.y, midIndex * size);
    }

    private void SetRoadType(RoadPart road, int roadIndex)
    {
        var roadType = RoadPart.RoadType.normal;
        var buildingType = BuildingController.BuildingType.none;

        if (roadIndex % parkingEveryNth == 0)
        {
            if ((roadIndex / parkingEveryNth) % 2 == 0)
                roadType = RoadPart.RoadType.parkingOnRight;
            else
                roadType = RoadPart.RoadType.parkingOnLeft;

            buildingType = BuildingController.BuildingType.policeStation;
        }

        road.SetRoadType(roadType);
        road.SetBuilding(buildingType);
    }
}
