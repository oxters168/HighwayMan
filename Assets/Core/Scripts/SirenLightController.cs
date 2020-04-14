using UnityEngine;

public class SirenLightController : MonoBehaviour
{
    public Light sirenLight;
    public Renderer sirenRenderer;
    public string shaderColorName = "_Color";
    public string shaderEmissionColorName = "_EmissionColor";

    [Space(10)]
    public bool onOff;
    [Tooltip("The mainTex color when on")]
    public Color texOn = Color.red;
    [Tooltip("The mainTex color when off")]
    public Color texOff = Color.black;
    [Tooltip("The emmision color when on")]
    public Color emOn = Color.red;
    [Tooltip("The emission color when off")]
    public Color emOff = Color.black;
    [Tooltip("The amount of time in seconds it takes to go on")]
    public float onTime = 0.1f;
    [Tooltip("The amount of time in seconds it takes to go off")]
    public float offTime = 0.1f;
    [Range(0, 1)]
    public float value;

    private bool turningOn;

    private bool errored;

    void Update()
    {
        float deltaValue = Time.deltaTime / -offTime;
        if (onOff && turningOn)
            deltaValue = Time.deltaTime / onTime;
        value += deltaValue;
        if (value >= 1)
            turningOn = false;
        else if (value <= 0)
            turningOn = true;
        value = Mathf.Clamp01(value);

        if (sirenLight != null)
            sirenLight.intensity = value;
        if (sirenRenderer != null)
        {
            errored = false;

            sirenRenderer.material.SetColor(shaderColorName, Color.Lerp(texOff, texOn, value));
            sirenRenderer.material.SetColor(shaderEmissionColorName, Color.Lerp(emOff, emOn, value));
        }
        else if (!errored)
        {
            errored = true;
            Debug.LogError("SirenLightController(" + name + "): Must provide renderer");
        }
    }
}
