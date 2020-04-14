using UnityHelpers;

public class NoWheel : AbstractWheel
{
    public override bool IsGrounded()
    {
        return transform.IsGrounded();
    }

    public override void SetGrip(float value)
    {
        
    }
}
