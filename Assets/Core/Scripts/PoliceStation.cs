using UnityEngine;
using UnityHelpers;
using System.Collections.Generic;

public class PoliceStation : MonoBehaviour
{
    private ObjectPool<Transform> bountyOptionsPool;
    private List<BountyOption> shownBounties = new List<BountyOption>();

    public VehicleSwitcher viewedVehicle;

    void Awake()
    {
        bountyOptionsPool = PoolManager.GetPool("BountyOptions");
    }

    void Update()
    {
        if (!HasBountyOnBoard())
            UpdateShownBounties();

        ClearCaughtBounties();
    }

    private void ViewBountyPressed(BountyOption sender)
    {
        viewedVehicle.SetVehicle((int)sender.bounty.carData.vehicleIndex);
        var carAppearance = viewedVehicle.currentVehicle.GetComponentInParent<CarAppearance>();
        if (carAppearance != null)
        {
            carAppearance.color = sender.bounty.carData.vehicleColor;
            carAppearance.showSiren = false;
        }
    }

    private void ClearCaughtBounties()
    {
        for (int i = 0; i < shownBounties.Count; i++)
        {
            var currentShownBounty = shownBounties[i];
            if (currentShownBounty.bounty.caught)
                RemoveShownBounty(i);
        }
    }

    private void RemoveShownBounty(BountyData bounty)
    {
        for (int i = 0; i < shownBounties.Count; i++)
        {
            if (shownBounties[i].bounty == bounty)
            {
                RemoveShownBounty(i);
                break;
            }
        }
    }
    private void RemoveShownBounty(BountyOption shownBounty)
    {
        int shownBountyIndex = shownBounties.IndexOf(shownBounty);
        RemoveShownBounty(shownBountyIndex);
    }
    private void RemoveShownBounty(int index)
    {
        if (index >= 0 && index < shownBounties.Count)
        {
            var removedShownBounty = shownBounties[index];
            removedShownBounty.viewButton.onClick.RemoveAllListeners();
            bountyOptionsPool.Return(removedShownBounty.transform);
            shownBounties.Remove(removedShownBounty);
        }
        else
            Debug.LogError("PoliceStation(" + transform.name + "): Could not remove shown bounty with index " + index);
    }

    private bool HasBountyOnBoard()
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

    private void UpdateShownBounties()
    {
        var currentBounties = BountyTracker.bountyTrackerInScene.currentBounties;
        for (int i = 0; i < currentBounties.Length; i++)
        {
            var currentBounty = currentBounties[i];
            var bountyOption = bountyOptionsPool.Get<BountyOption>();
            bountyOption.bounty = currentBounty;
            bountyOption.viewButton.onClick.AddListener(() => { ViewBountyPressed(bountyOption); });
            shownBounties.Add(bountyOption);
        }
    }
}
