using TMPro;
using UnityEngine;
using UnityHelpers;

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
        var currentVehicle = self.GetVehicle();
        if (currentVehicle != null)
        {
            hudTransform.position = currentVehicle.vehicleRigidbody.transform.position;
            hudTransform.forward = Vector3.down;

            if (showPointer || showLicense)
            {
                GameObject cameraObject = GameObject.FindGameObjectWithTag("HighwayCam");
                if (cameraObject != null)
                {
                    var orbitController = cameraObject.GetComponent<OrbitCameraController>();
                    if (orbitController != null)
                    {
                        hudTransform.localRotation = Quaternion.Euler(orbitController.rightAngle, orbitController.upAngle, orbitController.forwardAngle);
                        //hudTransform.LookAt(cameraObject.transform, cameraObject.transform.up);
                        //hudTransform.forward = -hudTransform.forward;
                    }
                }
            }

            hudTransform.position -= hudDistance * hudTransform.forward;
        }
    }
}
