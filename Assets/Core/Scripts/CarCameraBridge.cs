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
    public GaugeController speedGauge;
    //public TextMeshProUGUI speedGauge;

    [Space(10)]
    public MosaicsPE cameraMosaic;
    public float closeMosaicValue = 15, farMosaicValue = 5;
    [Space(10)]
    public float orthoMinSize = 10;
    public float orthoMaxSize = 50;
    public float orthoLerp = 5;
    [Space(10)]
    public bool rotateWithVehicle;
    public float rightAngle;
    public float upAngleOffset;

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
            followCamera.rightAngle = rightAngle;
            followCamera.upAngle = (rotateWithVehicle ? vehicle.vehicleRigidbody.transform.rotation.eulerAngles.y : 0) + upAngleOffset;

            if (speedGauge != null)
            {
                float rawDiff = vehicle.vehicleStats.maxForwardSpeed * MathHelpers.MPS_TO_KMH / (speedGauge.markCount + 0);
                int diff = Mathf.CeilToInt(rawDiff / 10) * 10;
                speedGauge.markDiff = diff;
                speedGauge.value = Mathf.Abs(vehicle.GetSpeedInKMH()) / speedGauge.lastMarkValue;
                //speedGauge.text = MathHelpers.SetDecimalPlaces(vehicle.GetSpeedInKMH(), 2).ToString();
            }
        }
    }

    private void CheckTarget()
    {
        if (targetVehicleGO != prevVehicleGO)
            targetVehicle = targetVehicleGO.GetComponent<AbstractDriver>();
        prevVehicleGO = targetVehicleGO;
    }
}
