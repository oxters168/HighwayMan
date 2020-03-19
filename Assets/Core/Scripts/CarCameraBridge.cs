using UnityEngine;
using UnityHelpers;
using UB;
using TMPro;

public class CarCameraBridge : MonoBehaviour
{
    [Tooltip("Must implement the AbstractDriver interface")]
    public GameObject targetVehicleGO;
    private AbstractDriver targetVehicle;
    private GameObject prevVehicleGO;

    public OrbitCameraController followCamera;
    public TextMeshProUGUI speedGauge;

    public MosaicsPE cameraMosaic;
    public float closeMosaicValue = 15, farMosaicValue = 5;
    public float orthoLerp = 5;
    public float orthoMinSize = 10, orthoMaxSize = 50;

    void Update()
    {
        CheckTarget();

        var vehicle = targetVehicle?.GetVehicle();
        if (vehicle != null)
        {
            float speedPercent = vehicle.currentTotalSpeed / vehicle.vehicleStats.maxForwardSpeed;
            var camerasCamera = followCamera.GetComponent<Camera>();
            float lerpedOrtho = Mathf.Lerp(camerasCamera.orthographicSize, Mathf.Lerp(orthoMinSize, orthoMaxSize, speedPercent), Time.deltaTime * orthoLerp);
            camerasCamera.orthographicSize = lerpedOrtho;
            float orthoPercent = (lerpedOrtho - orthoMinSize) / (orthoMaxSize - orthoMinSize);
            cameraMosaic.BlockSize = Mathf.Lerp(closeMosaicValue, farMosaicValue, orthoPercent);

            followCamera.target = vehicle.transform;
            speedGauge.text = MathHelpers.SetDecimalPlaces(vehicle.GetSpeedInKMH(), 2).ToString();
        }
    }

    private void CheckTarget()
    {
        if (targetVehicleGO != prevVehicleGO)
            targetVehicle = targetVehicleGO.GetComponent<AbstractDriver>();
        prevVehicleGO = targetVehicleGO;
    }
}
