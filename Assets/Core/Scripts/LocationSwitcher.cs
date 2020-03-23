using UnityEngine;
using UnityHelpers;

public class LocationSwitcher : MonoBehaviour
{
    public LocationManager locationManager;
    public string[] locationDoozyEvents;

    public void OnBuildingEntered(CarPhysics vehicle, BuildingController.BuildingType buildingType)
    {
        var driver = vehicle.GetComponentInParent<AbstractDriver>();
        if (driver != null && driver is HumanDriver)
        {
            locationManager.SetLocation((int)buildingType);
            Doozy.Engine.GameEventMessage.SendEvent(locationDoozyEvents[(int)buildingType]);
        }
    }
}
