using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BountyOption : MonoBehaviour
{
    [System.Flags]
    public enum Options { viewButton = 0x1, valueLabel = 0x2, licenseLabel = 0x4 }

    public Options shownOptions;

    public TextMeshProUGUI licenseLabel;
    public TextMeshProUGUI priceLabel;
    public Button viewButton;
    public BountyData bounty;

    private void Update()
    {
        viewButton.gameObject.SetActive((shownOptions & Options.viewButton) != 0);
        priceLabel.gameObject.SetActive((shownOptions & Options.valueLabel) != 0);
        licenseLabel.gameObject.SetActive((shownOptions & Options.licenseLabel) != 0);

        priceLabel.text = "$" + bounty.reward;
        licenseLabel.text = bounty.carData.vehicleLicense;
    }
}
