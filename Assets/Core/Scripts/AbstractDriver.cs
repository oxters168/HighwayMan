using UnityHelpers;

public interface AbstractDriver
{
    CarPhysics GetVehicle();
    Carability GetCarability();
    CarAppearance GetCarAppearance();
    CarHUD GetCarHUD();
    VehicleSwitcher GetVehicleSwitcher();
}
