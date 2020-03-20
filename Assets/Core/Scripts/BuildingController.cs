using UnityEngine;
using UnityHelpers;

public class BuildingController : MonoBehaviour
{
    public enum BuildingType { none, policeStation, shops, }
    [Tooltip("Should follow enum")]
    public GameObject[] buildings;
    public BuildingType buildingType;

    public event CarEnteredHandler onCarParked;
    public delegate void CarEnteredHandler(CarPhysics car, BuildingType buildingType);

    private void Update()
    {
        SetBuildingType(buildingType);
    }
    private void OnTriggerEnter(Collider other)
    {
        var car = other.GetComponentInParent<CarPhysics>();
        if (car != null && buildingType != BuildingType.none)
        {
            onCarParked?.Invoke(car, buildingType);
            //Debug.Log(car.name + " entered the building");
        }
    }

    public void SetBuildingType(BuildingController.BuildingType value)
    {
        for (int i = 0; i < buildings.Length; i++)
            buildings[i].SetActive(i == ((int)value - 1));

        buildingType = value;
    }
}
