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
            carability.siren = player.GetButton("Siren");
            carability.capture = player.GetButton("Capture");
            carability.cycleNextTarget = player.GetButtonDown("CycleNext") || player.GetButtonShortPress("CycleNext");
            carability.cyclePreviousTarget = player.GetButtonDown("CyclePrev") || player.GetButtonShortPress("CyclePrev");
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
    public CarAppearance GetCarAppearance()
    {
        //return GetComponent<CarAppearance>();
        return vehicles.currentVehicle.GetComponent<CarAppearance>();
    }
    public CarHUD GetCarHUD()
    {
        return GetComponent<CarHUD>();
    }
}
