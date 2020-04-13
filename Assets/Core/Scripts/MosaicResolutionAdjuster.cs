using UB;
using UnityEngine;

public class MosaicResolutionAdjuster : MonoBehaviour
{
    public MosaicsPE cameraMosaic;
    public float mosaicPercent = 0.01f;

    private void Update()
    {
        cameraMosaic.BlockSize = CalculateMosaicValue(mosaicPercent);
    }
    private float CalculateMosaicValue(float percent)
    {
        return Mathf.Clamp(Mathf.Min(Screen.width, Screen.height) * percent, 1, 100);
    }
}
