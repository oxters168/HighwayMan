using Rewired;
using TMPro;
using UnityEngine;
using UnityHelpers;

public class HumanDriver : MonoBehaviour
{
    public OrbitCameraController followCamera;
    public TextMeshProUGUI speedGauge;
    public VehicleSwitcher vehicles;
    public int playerId;
    private Player player;

    private void Start()
    {
        player = ReInput.players.GetPlayer(playerId);
    }
    void Update()
    {
        if (vehicles.currentVehicle != null)
        {
            followCamera.target = vehicles.currentVehicle.transform;
            vehicles.currentVehicle.gas = player.GetAxis("Gas");
            vehicles.currentVehicle.brake = player.GetAxis("Brake");
            vehicles.currentVehicle.steer = player.GetAxis("Steer");
            speedGauge.text = vehicles.currentVehicle.currentSpeed.ToString();
        }
    }
}
