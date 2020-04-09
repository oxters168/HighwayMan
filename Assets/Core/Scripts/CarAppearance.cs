using UnityEngine;

public class CarAppearance : MonoBehaviour
{
    public Color color = Color.white;
    private Color prevColor = Color.white;
    public Renderer affectedRenderer;

    private void Update()
    {
        if (color != prevColor)
        {
            affectedRenderer.material.color = color;
            prevColor = color;
        }
    }
}

[System.Serializable]
public struct ColorPercentage
{
    public Color color;
    [Range(0, 1)]
    public float percent;

    public static ColorPercentage PickColor(params ColorPercentage[] choices)
    {
        ColorPercentage choice = default;
        if (choices != null && choices.Length > 0)
        {
            choice = choices[0];
            if (choices.Length > 1)
            {
                float rng = Random.value;
                foreach (var possiblity in choices)
                    if (possiblity.percent >= rng && possiblity.percent <= choice.percent)
                        choice = possiblity;
            }
        }
        return choice;
    }
}
