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
            Debug.Log("Changed color");
        }
    }
}
