using UnityEngine;

public class Carability : MonoBehaviour
{
    public const uint MAX_LICENSE_INDEX = 2176782335;
    public string license;

    private void Start()
    {
        license = GetRandomLicense();
    }

    public static string GetRandomLicense()
    {
        uint licenseIndex = (uint)(Random.value * MAX_LICENSE_INDEX);
        return CalculateLicensePlate(licenseIndex);
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
}
