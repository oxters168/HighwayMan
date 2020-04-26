using UnityEngine;
using System.Collections.Generic;

public class PlayerData : MonoBehaviour
{
    public static PlayerData playerDataInScene;

    public CarData defaultCar = new CarData { vehicleIndex = 0, vehicleColor = Color.gray, vehicleLicense = string.Empty, };
    public List<CarData> unlockedVehicles = new List<CarData>();

    void Awake()
    {
        playerDataInScene = this;
    }
    void Start()
    {
        unlockedVehicles.Add(defaultCar);
    }
}
