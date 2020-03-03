using UnityEngine;

public class Repeater : MonoBehaviour
{
    public Transform target;
    public Transform extraFront, front, mid, rear, extraRear;

    public float size = 50;
    public float offset = 25;

    void Update()
    {
        int index = Mathf.CeilToInt((target.position.z - offset) / size);
        extraFront.position = new Vector3(front.position.x, front.position.y, (index + 2) * size);
        front.position = new Vector3(front.position.x, front.position.y, (index + 1) * size);
        mid.position = new Vector3(mid.position.x, mid.position.y, index * size);
        rear.position = new Vector3(rear.position.x, rear.position.y, (index - 1) * size);
        extraRear.position = new Vector3(rear.position.x, rear.position.y, (index - 2) * size);
    }
}
