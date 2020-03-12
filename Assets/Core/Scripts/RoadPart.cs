using UnityEngine;

public class RoadPart : MonoBehaviour
{
    private readonly float[] sidedness = new float[] { 0, 0, 180, };
    public enum RoadType { normal, parkingOnRight, parkingOnLeft };
    [Tooltip("Should follow the enum order")]
    public GameObject[] roadTypes;
    public RoadType roadType;

    public BuildingController buildings;

    private void Update()
    {
        SetRoadType(roadType);
    }

    public void SetRoadType(RoadType value)
    {
        for (int i = 0; i < roadTypes.Length; i++)
            roadTypes[i].SetActive(i == (int)value);

        buildings.transform.rotation = Quaternion.Euler(0, sidedness[(int)value], 0);

        roadType = value;
    }
    public void SetBuilding(BuildingController.BuildingType value)
    {
        buildings.SetBuildingType(value);
    }
}
