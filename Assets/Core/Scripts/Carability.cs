using UnityEngine;
using System.Collections.Generic;

public class Carability : MonoBehaviour
{
    public VehicleSwitcher self;

    public const uint MAX_LICENSE_INDEX = 2176782335;
    public string license;

    private readonly float[] scanDistances = new float[] { 15, 15, 25 };
    public bool scan;
    public int scanLevel;
    public float scansPerSecond = 0.25f;
    private float lastScan = float.MinValue;

    private void Start()
    {
        license = GetRandomLicense();
    }
    private void Update()
    {
        LicenseScanner();
    }

    private void LicenseScanner()
    {
        if (scan && Time.time - lastScan >= (1 / scansPerSecond))
        {
            int currentScanLevel = Mathf.Clamp(scanLevel, 0, scanDistances.Length - 1);
            float scanDistance = scanDistances[currentScanLevel];
            if (currentScanLevel > 0)
            {
                if (ScanSurroundingLicenses(scanDistance) > 0)
                    lastScan = Time.time;
            }
            else
                if (ScanFrontLicense(scanDistance))
                lastScan = Time.time;
        }
    }
    private bool ScanFrontLicense(float distance = 15)
    {
        bool hit = false;

        Vector3 centerPoint = self.currentVehicle.GetPointOnBoundsBorder(0, 0, 0);
        RaycastHit hitInfo;
        if (Physics.Raycast(centerPoint, self.currentVehicle.vehicleRigidbody.transform.forward, out hitInfo, distance))
        {
            var driver = hitInfo.transform.GetComponentInParent<AbstractDriver>();
            if (driver != null)
            {
                Debug.Log(driver.GetCarability().license);
                hit = true;
            }
        }

        return hit;
    }
    private int ScanSurroundingLicenses(float diameter = 15)
    {
        var nearbyVehicles = GetSurroundingDrivers(diameter);

        if (nearbyVehicles.Length > 0)
        {
            string debugString = "Found " + nearbyVehicles.Length + " license(s):\n";
            foreach (var vehicle in nearbyVehicles)
                debugString += vehicle.GetCarability().license + "\n";
            Debug.Log(debugString);
        }

        return nearbyVehicles.Length;
    }
    public AbstractDriver[] GetSurroundingDrivers(float diameter = 15)
    {
        List<AbstractDriver> nearbyVehicles = new List<AbstractDriver>();

        Vector3 spherecastPosition = self.currentVehicle.vehicleRigidbody.transform.position;
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
        uint licenseIndex = (uint)(Random.value * MAX_LICENSE_INDEX);
        return CalculateLicensePlate(licenseIndex);
    }
}
