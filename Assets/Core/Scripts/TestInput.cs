using UnityEngine;
using Rewired;

public class TestInput : MonoBehaviour
{
    public int playerId;
    private Player player;
    public RealCarPhysics testVehicle;

    void Start()
    {
        player = ReInput.players.GetPlayer(playerId);
    }
    void Update()
    {
        testVehicle.gas = player.GetAxis("Gas");
        testVehicle.steer = player.GetAxis("Steer");
    }
}
