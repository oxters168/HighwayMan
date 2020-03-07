using Rewired;
using TMPro;
using UB;
using UnityEngine;
using UnityHelpers;

public class HumanDriver : MonoBehaviour, AbstractDriver
{
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
            vehicles.currentVehicle.gas = player.GetAxis("Gas");
            vehicles.currentVehicle.brake = player.GetAxis("Brake");
            vehicles.currentVehicle.steer = player.GetAxis("Steer");
        }
    }

    public CarPhysics GetVehicle()
    {
        return vehicles.currentVehicle;
    }
}
