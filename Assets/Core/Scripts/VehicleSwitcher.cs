using UnityEngine;
using UnityHelpers;

public class VehicleSwitcher : MonoBehaviour
{
    public CarPhysics[] allVehicles;
    public CarPhysics currentVehicle { get; private set; }
    public int index;
    private int prevIndex = -1;

    private void Update()
    {
        if (index != prevIndex)
            SetVehicle(index);
    }

    public void SetVehicle(int vehicleIndex)
    {
        index = Mathf.Clamp(vehicleIndex, 0, allVehicles.Length - 1);
        if (allVehicles.Length > 0)
        {
            CarPhysics prevVehicle = null;
            if (prevIndex >= 0 && prevIndex < allVehicles.Length)
            {
                prevVehicle = allVehicles[prevIndex];
                if (prevVehicle != null)
                    prevVehicle.SetVisible(false);
            }

            currentVehicle = allVehicles[index];
            currentVehicle.Match(prevVehicle);
            currentVehicle.SetVisible(true);
        }
        prevIndex = index;
    }
}
