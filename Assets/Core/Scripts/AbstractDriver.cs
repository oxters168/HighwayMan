using UnityEngine;
using UnityHelpers;

public interface AbstractDriver
{
    CarPhysics GetVehicle();
    Carability GetCarability();
}
