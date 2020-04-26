using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityHelpers;

public class BountyOption : MonoBehaviour
{
    [System.Flags]
    public enum Options { viewButton = 0x1, valueLabel = 0x2, licenseLabel = 0x4, viewCompass = 0x8, viewType = 0x10, }

    public Options shownOptions;
    [HideInInspector]
    public VehicleSwitcher vehicles;

    public TextMeshProUGUI licenseLabel;
    public TextMeshProUGUI priceLabel;
    public TextMeshProUGUI companyLabel;
    public TextMeshProUGUI typeLabel;
    public Button viewButton;
    public BountyData bounty;
    public GameObject compassGameObject;
    public RectTransform compass;
    public AbstractDriver compassRelativeTo;
    public GameObject[] distanceGauge;

    private void Update()
    {
        viewButton.gameObject.SetActive((shownOptions & Options.viewButton) != 0);
        priceLabel.gameObject.SetActive((shownOptions & Options.valueLabel) != 0);
        licenseLabel.gameObject.SetActive((shownOptions & Options.licenseLabel) != 0);
        companyLabel.gameObject.SetActive((shownOptions & Options.viewType) != 0);
        typeLabel.gameObject.SetActive((shownOptions & Options.viewType) != 0);
        compassGameObject.SetActive((shownOptions & Options.viewCompass) != 0);

        priceLabel.text = "$" + bounty.reward;
        licenseLabel.text = bounty.carData.vehicleLicense;

        var bountyVehicleIndex = bounty.carData.vehicleIndex;
        var vehicle = vehicles.allVehicles[bountyVehicleIndex];
        if (vehicle != null)
        {
            companyLabel.text = vehicle.vehicleStats.companyName;
            typeLabel.text = vehicle.vehicleStats.modelName;
        }

        if (compassRelativeTo != null)
        {
            var otherVehicle = compassRelativeTo.GetVehicle();
            if (otherVehicle != null)
            {
                var minBountyDistance = BountyTracker.bountyTrackerInScene.GetMinDistance();
                var maxBountyDistance = BountyTracker.bountyTrackerInScene.GetMaxDistance();
                float bountyDistance = Mathf.Abs(bounty.position - otherVehicle.vehicleRigidbody.position.z);
                float percentDistance = (Mathf.Clamp(bountyDistance, minBountyDistance, maxBountyDistance) - minBountyDistance) / (maxBountyDistance - minBountyDistance);
                int signalIndex = Mathf.RoundToInt((1 - percentDistance) * (distanceGauge.Length - 1));
                SetDistanceGaugeIndex(signalIndex);
                //Vector2 bountyPosition = new Vector2(0, bounty.position);
                //Vector2 compassDirection = bountyPosition - otherVehicle.vehicleRigidbody.position.xz();
                //compass.localRotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, compassDirection));
            }
        }
    }

    private void SetDistanceGaugeIndex(int index)
    {
        for (int i = 0; i < distanceGauge.Length; i++)
            distanceGauge[i].SetActive(i == index);
    }
}
