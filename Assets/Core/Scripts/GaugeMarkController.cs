using UnityEngine;

public class GaugeMarkController : MonoBehaviour
{
    public TMPro.TextMeshProUGUI label;

    public void SetAngle(float value)
    {
        transform.localRotation = Quaternion.Euler(0, 0, value);
        label.transform.rotation = Quaternion.identity;
    }
}
