using UnityEngine;
using UnityHelpers;

public class BuildingController : MonoBehaviour
{
    public enum BuildingType { none, policeStation, shops, garage, }
    [Tooltip("Should follow enum")]
    public GameObject[] buildings;
    public BuildingType buildingType;

    private GameObject building;
    private bool inBuilding;

    public event CarEnteredHandler onCarParked;
    public delegate void CarEnteredHandler(CarPhysics car, BuildingType buildingType);

    private void Update()
    {
        SetBuildingType(buildingType);
    }
    private void OnTriggerEnter(Collider other)
    {
        var car = other.GetComponentInParent<CarPhysics>();
        if (car != null && buildingType != BuildingType.none && !inBuilding)
        {
            building = other.gameObject;
            inBuilding = true;
            onCarParked?.Invoke(car, buildingType);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (building == other.gameObject)
        {
            inBuilding = false;
            building = null;
        }
    }

    public void SetBuildingType(BuildingController.BuildingType value)
    {
        for (int i = 0; i < buildings.Length; i++)
            buildings[i].SetActive(i == ((int)value - 1));

        buildingType = value;
    }
}
