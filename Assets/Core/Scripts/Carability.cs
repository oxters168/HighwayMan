using UnityEngine;
using System.Collections.Generic;
using System;

public class Carability : MonoBehaviour
{
    private AbstractDriver self;

    public const uint MAX_LICENSE_INDEX = 2176782335;
    public string license;

    private readonly float[] scanDistances = new float[] { 15, 15, 25 };
    [Space(10)]
    public EffectWrapper scanLineEffect;
    public EffectWrapper scanSphereEffect;
    public bool scan;
    public int scanLevel;
    public float scansPerSecond = 0.25f;
    private float lastScan = float.MinValue;
    public bool CanScan { get { return Time.time - lastScan >= (1 / scansPerSecond); } }

    private readonly float[] sirenDistances = new float[] { -1, 15, 30, 60 };
    [Space(10)]
    public bool siren;
    public int sirenLevel;
    public float sirenScansPerSecond = 0.25f;
    private float lastSirenScan = float.MinValue;
    private SirenLightController[] sirenLights;

    private readonly float[] captureDistances = new float[] { 15, 30, 60, 75 };
    private readonly float[] captureTimes = new float[] { 8, 5, 3, 1 };
    [Space(10)]
    public EffectWrapper captureEffect;
    public bool capture;
    public bool cycleNextTarget, cyclePreviousTarget;
    //private bool cycledTarget;
    private AbstractDriver currentTarget;
    public int captureDistanceLevel;
    public int captureTimeLevel;
    private float startCaptureTime = -1;
    public bool IsCapturing { get; private set; }

    [Space(10)]
    public float targettingDistance = 75;

    private void Awake()
    {
        self = GetComponent<AbstractDriver>();
        sirenLights = GetComponentsInChildren<SirenLightController>(true);
    }
    private void Update()
    {
        ManageEffects();

        LicenseScanner();
        Siren();
        SirenLights();
        CycleTargets();
        IsCapturing = Capture();
    }

    private void ManageEffects()
    {
        Transform expectedParent = self.GetVehicle()?.transform;

        #region Scan
        EffectWrapper scanEffect = scanLineEffect;
        Vector3 scanScale = new Vector3(0.5f, 0.5f, GetScanDistance());
        if (scanLevel > 0)
        {
            scanEffect = scanSphereEffect;
            scanScale = Vector3.one * GetScanDistance();
            scanLineEffect?.SetShown(false);
        }
        else
            scanSphereEffect?.SetShown(false);
        if (scanEffect != null)
        {
            if (scanEffect.transform.parent != expectedParent)
                scanEffect.transform.SetParent(expectedParent, false);

            scanEffect.shownScale = scanScale;
            scanEffect.SetShown(scan);
            scanEffect.SetColor(CanScan ? Color.green : Color.red);
        }
        #endregion

        #region Capture
        if (captureEffect != null)
        {
            if (captureEffect.transform.parent != expectedParent)
                captureEffect.transform.SetParent(expectedParent, false);

            captureEffect.shownScale = Vector3.one * GetCaptureDistance();
            captureEffect.SetShown(capture);
            captureEffect.SetColor(IsCapturing ? Color.green : Color.red);
        }
        #endregion
    }
    public void SetTarget(AbstractDriver target)
    {
        if (currentTarget != null)
            currentTarget.GetCarHUD().showPointer = false;

        currentTarget = target;

        if (currentTarget != null)
            currentTarget.GetCarHUD().showPointer = true;
    }
    private void ShowLicense(AbstractDriver driver)
    {
        driver.GetCarHUD().showLicense = true;
    }

    private void CycleTargets()
    {
        //if (!cycledTarget && (cycleNextTarget || cyclePreviousTarget))
        if (cycleNextTarget || cyclePreviousTarget)
        {
            //cycledTarget = true;

            //int currentCaptureDistanceLevel = Mathf.Clamp(captureDistanceLevel, 0, captureDistances.Length - 1);
            AbstractDriver[] surroundingDrivers = GetSurroundingDrivers(targettingDistance);

            if (surroundingDrivers != null && surroundingDrivers.Length > 0)
            {
                List<Tuple<AbstractDriver, float>> orderingSet = new List<Tuple<AbstractDriver, float>>();
                for (int i = 0; i < surroundingDrivers.Length; i++)
                {
                    Vector3 targetDirection = (surroundingDrivers[i].GetVehicle().vehicleRigidbody.transform.position - self.GetVehicle().vehicleRigidbody.transform.position).normalized;
                    float targetAngle = Vector3.SignedAngle(-Vector3.right, targetDirection, Vector3.up);
                    orderingSet.Add(new Tuple<AbstractDriver, float>(surroundingDrivers[i], targetAngle));
                }

                orderingSet.Sort((current, next) => Mathf.RoundToInt(Mathf.Sign(current.Item2 - next.Item2)));
                int currentTargetIndex = -1;
                if (currentTarget != null)
                {
                    for (int i = 0; i < orderingSet.Count; i++)
                    {
                        if (orderingSet[i].Item1 == currentTarget)
                        {
                            currentTargetIndex = i;
                            break;
                        }
                    }
                }
                if (cycleNextTarget)
                    currentTargetIndex += 1;
                else
                    currentTargetIndex -= 1;

                currentTargetIndex = (currentTargetIndex + surroundingDrivers.Length) % surroundingDrivers.Length;

                SetTarget(orderingSet[currentTargetIndex].Item1);
            }
        }
        //else if (!cycleNextTarget && !cyclePreviousTarget)
        //    cycledTarget = false;
    }
    public float GetCaptureDistance()
    {
        int currentCaptureDistanceLevel = Mathf.Clamp(captureDistanceLevel, 0, captureDistances.Length - 1);
        return captureDistances[currentCaptureDistanceLevel];
    }
    private bool Capture()
    {
        bool isCapturing = false;

        if (capture)
        {
            //There's a bug where you can capture unspawned vehicles, should probably check if active in heirarchy. Needs more testing
            if (currentTarget != null)
            {
                float targetDistance = (currentTarget.GetVehicle().vehicleRigidbody.transform.position - self.GetVehicle().vehicleRigidbody.transform.position).magnitude;
                if (targetDistance <= GetCaptureDistance() / 2)
                {
                    isCapturing = true;

                    if (startCaptureTime < 0)
                        startCaptureTime = Time.time;

                    int currentCaptureTimeLevel = Mathf.Clamp(captureTimeLevel, 0, captureTimes.Length - 1);
                    if (Time.time - startCaptureTime >= captureTimes[currentCaptureTimeLevel])
                    {
                        BountyTracker.bountyTrackerInScene.Catch(currentTarget.GetCarability().license);
                        Debug.Log("Captured " + currentTarget.GetCarability().license); //Caught
                        isCapturing = false;
                    }
                }
            }
        }

        if (!isCapturing)
            startCaptureTime = -1;

        return isCapturing;
    }
    private void SirenLights()
    {
        if (sirenLights != null && sirenLights.Length > 0)
        {
            foreach (var sirenLight in sirenLights)
                sirenLight.onOff = siren;
        }
    }
    private void Siren()
    {
        if (siren && Time.time - lastSirenScan >= (1 / sirenScansPerSecond))
        {
            int currentSirenScanLevel = Mathf.Clamp(sirenLevel, 0, sirenDistances.Length - 1);
            float scanDistance = sirenDistances[currentSirenScanLevel];
            if (scanDistance > 0)
            {
                var driver = GetDriverInFront(scanDistance);
                if (driver != null && driver is BotDriver)
                    ((BotDriver)driver).ChangeLane();
            }
            lastSirenScan = Time.time;
        }
    }
    public float GetScanDistance()
    {
        int currentScanLevel = Mathf.Clamp(scanLevel, 0, scanDistances.Length - 1);
        return scanDistances[currentScanLevel];
    }
    private void LicenseScanner()
    {
        if (scan && CanScan)
        {
            float scanDistance = GetScanDistance();
            if (scanLevel > 0)
            {
                if (ScanSurroundingLicenses(scanDistance) > 0)
                    lastScan = Time.time;
            }
            else
                if (ScanFrontLicense(scanDistance))
                    lastScan = Time.time;
        }
    }

    public void RandomizeLicense()
    {
        license = GetRandomLicense();
    }

    private bool ScanFrontLicense(float distance = 15)
    {
        bool hit = false;

        var driver = GetDriverInFront(distance);
        if (driver != null)
        {
            ShowLicense(driver);
            //Debug.Log(driver.GetCarability().license);
            hit = true;
        }

        return hit;
    }
    private int ScanSurroundingLicenses(float diameter = 15)
    {
        var nearbyVehicles = GetSurroundingDrivers(diameter);

        if (nearbyVehicles.Length > 0)
        {
            //string debugString = "Found " + nearbyVehicles.Length + " license(s):\n";
            foreach (var vehicle in nearbyVehicles)
                ShowLicense(vehicle);
            //    debugString += vehicle.GetCarability().license + "\n";
            //Debug.Log(debugString);
        }

        return nearbyVehicles.Length;
    }

    public AbstractDriver GetDriverInFront(float distance = 15)
    {
        AbstractDriver driver = null;
        Vector3 centerPoint = self.GetVehicle().GetPointOnBoundsBorder(0, 0, 0);
        RaycastHit hitInfo;
        if (Physics.Raycast(centerPoint, self.GetVehicle().vehicleRigidbody.transform.forward, out hitInfo, distance))
        {
            driver = hitInfo.transform.GetComponentInParent<AbstractDriver>();
        }
        return driver;
    }
    public AbstractDriver[] GetSurroundingDrivers(float diameter = 15)
    {
        List<AbstractDriver> nearbyVehicles = new List<AbstractDriver>();

        Vector3 spherecastPosition = self.GetVehicle().vehicleRigidbody.transform.position;
        Collider[] hits = Physics.OverlapSphere(spherecastPosition, diameter / 2);
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                var driver = hits[i].transform.GetComponentInParent<AbstractDriver>();
                if (driver != null && driver.GetCarability() != this && !nearbyVehicles.Contains(driver))
                    nearbyVehicles.Add(driver);
            }
        }

        return nearbyVehicles.ToArray();
    }

    /// <summary>
    /// Given a value, will calculate an alpha-numeric license plate.
    /// </summary>
    /// <param name="index">A value between 0 and 2176782335 (both inclusive).</param>
    /// <returns>An alpha-numeric license plate.</returns>
    public static string CalculateLicensePlate(uint index)
    {
        //65-90=A-Z
        string license = "";
        for (int i = 0; i < 6; i++)
        {
            //Go from base10 to base36
            var currentLicenseValue = (byte)(index % 36);
            index = index / 36;

            //Get alpha-numeric value based on base36 value
            if (currentLicenseValue > 9)
                license += (char)(65 + (currentLicenseValue - 10));
            else
                license += currentLicenseValue;
        }

        return license;
    }
    public static string GetRandomLicense()
    {
        uint licenseIndex = (uint)(UnityEngine.Random.value * MAX_LICENSE_INDEX);
        return CalculateLicensePlate(licenseIndex);
    }
}
