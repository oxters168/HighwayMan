using UnityEngine;
using UnityHelpers;

public class EffectWrapper : MonoBehaviour
{
    public Transform objectRenderer;
    public PSMeshRendererUpdater effect;
    public Vector3 shownScale = Vector3.one;
    public Vector3 hiddenScale = Vector3.zero;
    public float showTime = 1, hideTime = 1;

    private Coroutine currentRoutine;
    public bool isShown { get; private set; }
    private Vector3 currentScale;

    private void OnEnable()
    {
        effect.UpdateMeshEffect();
        currentScale = hiddenScale;
        objectRenderer.localScale = currentScale;
        objectRenderer.gameObject.SetActive(false);
    }

    public void SetShown(bool onOff)
    {
        if (onOff && !isShown)
            Show();
        else if (!onOff && isShown)
            Hide();
    }
    public void Show()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        Vector3 startScale = currentScale;
        currentRoutine = StartCoroutine(CommonRoutines.TimedAction(
            (percentShown) =>
            {
                currentScale = Vector3.Lerp(startScale, shownScale, percentShown);
                objectRenderer.localScale = currentScale;
            },
            showTime,
            () =>
            {
                objectRenderer.gameObject.SetActive(true);
                effect.UpdateMeshEffect();
            }, null));

        isShown = true;
    }
    public void Hide()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        Vector3 startScale = currentScale;
        currentRoutine = StartCoroutine(CommonRoutines.TimedAction(
            (percentHidden) =>
            {
                currentScale = Vector3.Lerp(startScale, hiddenScale, percentHidden);
                objectRenderer.localScale = currentScale;
            }, hideTime, null,
            () =>
            {
                objectRenderer.gameObject.SetActive(false);
            }));

        isShown = false;
    }
    public void SetColor(Color color)
    {
        effect.UpdateColor(color);
    }
}
