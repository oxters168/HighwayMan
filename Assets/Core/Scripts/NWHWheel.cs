using NWH.WheelController3D;
using UnityHelpers;

public class NWHWheel : AbstractWheel
{
    public WheelController wheel;

    public override bool IsGrounded()
    {
        return wheel.isGrounded;
    }

    public override void SetGrip(float value)
    {
        wheel.forwardFriction.forceCoefficient = value;
        wheel.forwardFriction.slipCoefficient = value;
        wheel.sideFriction.forceCoefficient = value;
        wheel.sideFriction.slipCoefficient = value;
    }
}
