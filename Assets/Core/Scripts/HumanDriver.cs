using Rewired;
using UnityEngine;

public class HumanDriver : MonoBehaviour
{
    public VehiclePhysics vehicle;
    public int playerId;
    private Player player;

    private void Start()
    {
        player = ReInput.players.GetPlayer(playerId);
    }
    void Update()
    {
        vehicle.gas = player.GetAxis("Gas");
        vehicle.brake = player.GetAxis("Brake");
        vehicle.steer = player.GetAxis("Steer");
    }
}
