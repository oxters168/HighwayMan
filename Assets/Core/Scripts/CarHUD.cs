using TMPro;
using UnityEngine;

public class CarHUD : MonoBehaviour
{
    private AbstractDriver self;

    public Transform hudTransform;

    //public Transform lookAtTarget;
    public float hudDistance = 5;

    public bool showPointer;
    public GameObject pointer;

    public bool showLicense;
    public GameObject license;
    public TextMeshProUGUI licenseLabel;

    private void Awake()
    {
        self = GetComponent<AbstractDriver>();
    }
    void Update()
    {
        Reposition();

        if (showLicense)
            licenseLabel.text = self.GetCarability().license;

        pointer.SetActive(showPointer);
        license.SetActive(showLicense);
    }

    private void Reposition()
    {
        hudTransform.position = self.GetVehicle().vehicleRigidbody.transform.position;
        hudTransform.forward = Vector3.down;

        if (showPointer || showLicense)
        {
            GameObject lookAtTarget = GameObject.FindGameObjectWithTag("HighwayCam");
            if (lookAtTarget)
            {
                hudTransform.LookAt(lookAtTarget.transform, lookAtTarget.transform.up);
                hudTransform.forward = -hudTransform.forward;
            }
        }

        hudTransform.position -= hudDistance * hudTransform.forward;
    }
}
