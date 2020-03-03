using Rewired;
using TMPro;
using UB;
using UnityEngine;
using UnityHelpers;

public class HumanDriver : MonoBehaviour
{
    public OrbitCameraController followCamera;
    public TextMeshProUGUI speedGauge;
    public VehicleSwitcher vehicles;
    public int playerId;
    private Player player;

    public MosaicsPE cameraMosaic;
    public float closeMosaicValue, farMosaicValue;
    public float orthoLerp = 5;
    public float orthoMinSize = 10, orthoMaxSize = 50;

    private void Start()
    {
        player = ReInput.players.GetPlayer(playerId);
    }
    void Update()
    {
        if (vehicles.currentVehicle != null)
        {
            float speedPercent = vehicles.currentVehicle.currentSpeed / vehicles.currentVehicle.vehicleStats.maxForwardSpeed;
            var camerasCamera = followCamera.GetComponent<Camera>();
            float lerpedOrtho = Mathf.Lerp(camerasCamera.orthographicSize, Mathf.Lerp(orthoMinSize, orthoMaxSize, speedPercent), Time.deltaTime * orthoLerp);
            camerasCamera.orthographicSize = lerpedOrtho;
            float orthoPercent = (lerpedOrtho - orthoMinSize) / (orthoMaxSize - orthoMinSize);
            cameraMosaic.BlockSize = Mathf.Lerp(closeMosaicValue, farMosaicValue, orthoPercent);

            followCamera.target = vehicles.currentVehicle.transform;
            vehicles.currentVehicle.gas = player.GetAxis("Gas");
            vehicles.currentVehicle.brake = player.GetAxis("Brake");
            vehicles.currentVehicle.steer = player.GetAxis("Steer");
            speedGauge.text = MathHelpers.SetDecimalPlaces(vehicles.currentVehicle.currentSpeed, 2).ToString();
        }
    }
}
