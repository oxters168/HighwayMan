using TMPro;
using UnityEngine;
using UnityHelpers;

public class CarHUD : MonoBehaviour
{
    private AbstractDriver self;

    public Transform hudTransform;
    public Vector3 closeScale = new Vector3(0.01f, 0.01f, 0.01f);
    public Vector3 farScale = new Vector3(0.03f, 0.03f, 0.03f);

    //public Transform lookAtTarget;
    public float hudDistance = 5;

    public bool showPointer;
    public GameObject pointer;

    public bool showLicense;
    public GameObject license;
    public TextMeshProUGUI licenseLabel;

    private CarCameraBridge cameraBridge;

    private void Awake()
    {
        self = GetComponent<AbstractDriver>();
    }
    void Update()
    {
        if (cameraBridge == null)
            cameraBridge = GameObject.FindGameObjectWithTag("HighwayCam")?.GetComponent<CarCameraBridge>();
        if (cameraBridge != null)
            hudTransform.localScale = Vector3.Lerp(closeScale, farScale, cameraBridge.orthoPercent);

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
