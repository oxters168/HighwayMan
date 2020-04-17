using System.Collections.Generic;
using UnityEngine;
using UnityHelpers;

public class BountyList : MonoBehaviour
{
    public string bountyOptionsPoolName = "BountyOptions";
    public VehicleSwitcher viewedVehicle;
    public CarStats defaultShownVehicle;

    private ObjectPool<Transform> bountyOptionsPool;
    private List<BountyOption> shownBounties = new List<BountyOption>();

    private BountyOption currentlySelected;

    public BountyOption.Options shownOptions;

    void Awake()
    {
        bountyOptionsPool = PoolManager.GetPool(bountyOptionsPoolName);
        ShowDefaultVehicle();
    }
    void Update()
    {
        if (currentlySelected == null)
            ShowDefaultVehicle();
        
        foreach (var bountyOption in shownBounties)
            if (bountyOption != null)
                bountyOption.shownOptions = shownOptions;
    }
    private void ViewBountyPressed(BountyOption sender)
    {
        SetSelection(sender);
    }

    public void SetSelection(BountyOption bountyOption)
    {
        if (HasBounty(bountyOption))
        {
            currentlySelected = bountyOption;
            ShowVehicle((int)bountyOption.bounty.carData.vehicleIndex, bountyOption.bounty.carData.vehicleColor);
        }
        else
            Debug.LogError("BountyList: Cannot show bounty option when not on list");
    }
    public BountyOption GetSelection()
    {
        return currentlySelected;
    }

    public BountyOption[] GetShownBounties()
    {
        return shownBounties.ToArray();
    }

    public bool HasBounty(BountyOption bountyOption)
    {
        return shownBounties.IndexOf(bountyOption) >= 0;
    }
    public bool HasBounty(BountyData bountyData)
    {
        return GetBountyOptionIndex(bountyData) >= 0;
    }
    public void ShowVehicle(CarStats carType, Color color)
    {
        if (carType != null)
            ShowVehicle(carType.index, color);
        else
            viewedVehicle?.gameObject.SetActive(false);
    }
    public void ShowVehicle(int index, Color color)
    {
        if (viewedVehicle != null)
        {
            viewedVehicle.gameObject.SetActive(true);
            viewedVehicle.SetVehicle(index);
            var carAppearance = viewedVehicle.currentVehicle.GetComponentInParent<CarAppearance>();
            if (carAppearance != null)
            {
                carAppearance.color = color;
                carAppearance.showSiren = false;
            }
        }
        else
            Debug.LogWarning("BountyList: No vehicle viewer provided");
    }
    public void ShowDefaultVehicle()
    {
        ShowVehicle(defaultShownVehicle, Color.white);
    }

    public bool HasAnyBountyOnBoard()
    {
        bool aBountyExists = false;
        for (int i = 0; i < shownBounties.Count; i++)
        {
            if (shownBounties != null)
            {
                aBountyExists = true;
                break;
            }
        }
        return aBountyExists;
    }

    public int GetBountyOptionIndex(BountyData bounty)
    {
        int optionIndex = -1;
        for (int i = 0; i < shownBounties.Count; i++)
        {
            if (shownBounties[i].bounty == bounty)
            {
                optionIndex = i;
                break;
            }
        }
        return optionIndex;
    }
    public int GetBountyOptionIndex(BountyOption shownBounty)
    {
        return shownBounties.IndexOf(shownBounty);
    }
    public BountyOption GetBountyOption(BountyData bounty)
    {
        BountyOption optionInList = null;
        var optionIndex = GetBountyOptionIndex(bounty);
        if (optionIndex >= 0 && optionIndex < shownBounties.Count)
            optionInList = shownBounties[optionIndex];
        else
            Debug.LogError("BountyList: Bounty data does not exist in list");

        return optionInList;
    }

    public void AddBountyOption(BountyData bounty)
    {
            var bountyOption = bountyOptionsPool.Get<BountyOption>();
            bountyOption.bounty = bounty;
            bountyOption.viewButton.onClick.AddListener(() => { ViewBountyPressed(bountyOption); });
            shownBounties.Add(bountyOption);
    }

    public void RemoveShownBounty(BountyData bounty)
    {
        RemoveShownBounty(GetBountyOptionIndex(bounty));
    }
    public void RemoveShownBounty(BountyOption shownBounty)
    {
        RemoveShownBounty(GetBountyOptionIndex(shownBounty));
    }
    public void RemoveShownBounty(int index)
    {
        if (index >= 0 && index < shownBounties.Count)
        {
            var removedShownBounty = shownBounties[index];
            removedShownBounty.viewButton.onClick.RemoveAllListeners();
            bountyOptionsPool.Return(removedShownBounty.transform);
            shownBounties.Remove(removedShownBounty);

            if (currentlySelected == removedShownBounty)
                currentlySelected = null;
        }
        else
            Debug.LogError("BountyList: Could not remove bounty from list, given index was invalid " + index);
    }
}
