using UnityEngine;

public class MimicCamera : MonoBehaviour
{
    public Camera mimicked;
    private Camera self;

    private void Awake()
    {
        self = GetComponent<Camera>();
    }
    private void Update()
    {
        self.orthographic = mimicked.orthographic;
        self.orthographicSize = mimicked.orthographicSize;
    }
}
