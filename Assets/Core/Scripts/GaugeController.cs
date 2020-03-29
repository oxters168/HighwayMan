using UnityEngine;
using UnityEngine.UI;
using UnityHelpers;

public class GaugeController : MonoBehaviour
{
    public GaugeMarkController gaugeMarkPrefab;
    public Transform gaugeMarksParent;
    private ObjectPool<GaugeMarkController> gaugeMarksPool;

    [Range(0, 1), Tooltip("What percentage to fill the gauge")]
    public float value;

    [Tooltip("The image on the gauge that will act as the bar")]
    public Image gaugeFillBar;
    [Range(0, 1), Tooltip("This is the minimum value the bar can be filled, it will act as the relative zero of the bar")]
    public float fillBarMin = 0;
    [Range(0, 1), Tooltip("This is the maximum value the bar can be filled, it will act as the relative one of the bar")]
    public float fillBarMax = 1;

    /// <summary>
    /// The value displayed at the end of the gauge
    /// </summary>
    public float lastMarkValue { get { return GetValueAt(markCount - 1); } }

    [Tooltip("The step difference between each mark")]
    public float markDiff = 20;
    private float previousMarkDiff = float.MinValue;
    [Tooltip("How many marks the gauge should have in total")]
    public int markCount = 10;
    private int previousMarkCount = int.MinValue;

    public float radius = 1;
    public float minAngle, maxAngle;

    void Start()
    {
        gaugeMarksPool = new ObjectPool<GaugeMarkController>(gaugeMarkPrefab, markCount > 5 ? markCount : 5, false, true, gaugeMarksParent, false);
    }

    void Update()
    {
        FillBar();
        CheckRedraw();
    }

    private void FillBar()
    {
        float totalFill = fillBarMax - fillBarMin;
        gaugeFillBar.fillAmount = value * totalFill + fillBarMin;
    }

    private void CheckRedraw()
    {
        if (markDiff != previousMarkDiff || markCount != previousMarkCount)
        {
            ResetMarks();
            previousMarkDiff = markDiff;
            previousMarkCount = markCount;
        }
    }
    private void ResetMarks()
    {
        gaugeMarksPool.ReturnAll();

        float deltaAngle = (maxAngle - minAngle) / (markCount - 1);

        for (int i = 0; i < markCount; i++)
        {
            float currentAngle = deltaAngle * i + minAngle;
            float rads = (currentAngle - 90) * Mathf.Deg2Rad;

            float xPos = radius * Mathf.Cos(rads);
            float yPos = radius * Mathf.Sin(rads);

            gaugeMarksPool.Get((gaugeMark) => { gaugeMark.transform.localPosition = new Vector2(xPos, yPos); gaugeMark.SetAngle(currentAngle); gaugeMark.label.text = GetValueAt(i).ToString(); });
        }
    }
    private float GetValueAt(int index)
    {
        return Mathf.RoundToInt(markDiff * index);
    }
}
