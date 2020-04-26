using UnityEngine;
using UnityHelpers;

public class Garage : MonoBehaviour
{
    [Tooltip("Must have abstract driver attached")]
    public GameObject modifyingDriver;
    private AbstractDriver modifying;

    public GameObject lockedImage;

    public VehicleSwitcher shownVehicles;
    public CarStats[] whitelist;
    private int currentWhitelistIndex;

    private void Start()
    {
        modifying = modifyingDriver.GetComponent<AbstractDriver>();
    }
    void Update()
    {
        lockedImage.SetActive(!PlayerData.playerDataInScene.unlockedVehicles.Exists((carData) => { return carData.vehicleIndex == whitelist[currentWhitelistIndex].index; }));
    }

    public void SetModifyingToShown()
    {
        modifying.GetVehicleSwitcher().SetVehicle(GetShownVehicleIndex());
    }
    public void SetShownToModifying()
    {
        currentWhitelistIndex = System.Array.IndexOf(whitelist, modifying.GetVehicle().vehicleStats);
        SetShownVehicle(GetShownVehicleIndex());
    }

    public void ShowNext()
    {
        currentWhitelistIndex += 1;
        SetShownVehicle(GetShownVehicleIndex());
    }
    public void ShowPrev()
    {
        currentWhitelistIndex -= 1;
        SetShownVehicle(GetShownVehicleIndex());
    }

    public void SetShownVehicle(int vehicleIndex)
    {
        shownVehicles.SetVehicle(vehicleIndex);
    }
    public int GetShownVehicleIndex()
    {
        currentWhitelistIndex = (currentWhitelistIndex + whitelist.Length) % whitelist.Length;
        return whitelist[currentWhitelistIndex].index;
    }
}
