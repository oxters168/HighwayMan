using UnityEngine;

public class Repeater : MonoBehaviour
{
    public Transform target;
    public Transform front, mid, back;

    public float size = 50;
    public float offset = 25;

    void Update()
    {
        int index = Mathf.CeilToInt((target.position.z - offset) / size);
        mid.position = new Vector3(mid.position.x, mid.position.y, index * size);
        front.position = new Vector3(front.position.x, front.position.y, (index + 1) * size);
        back.position = new Vector3(back.position.x, back.position.y, (index - 1) * size);
    }
}
