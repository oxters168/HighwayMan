using Rewired;
using UnityEngine;
using UnityHelpers;

public class HumanDriver : MonoBehaviour, AbstractDriver
{
    public VehicleSwitcher vehicles;
    public Carability carability;
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
            vehicles.currentVehicle.gas = player.GetAxis("Gas");
            vehicles.currentVehicle.brake = player.GetAxis("Brake");
            vehicles.currentVehicle.steer = player.GetAxis("Steer");

            carability.scan = player.GetButton("Scan");
        }
    }

    public CarPhysics GetVehicle()
    {
        return vehicles.currentVehicle;
    }
    public Carability GetCarability()
    {
        return carability;
    }
}
