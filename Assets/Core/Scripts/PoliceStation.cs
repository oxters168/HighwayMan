using UnityEngine;

public class PoliceStation : MonoBehaviour
{
    public BountyList stationBountyList;
    public BountyOption.Options stationShownOptions;
    public BountyList acceptedBountyList;
    public BountyOption.Options acceptedShownOptions;

    void Update()
    {
        if (!stationBountyList.HasAnyBountyOnBoard())
            UpdateShownBounties();

        ClearCaughtBounties();

        stationBountyList.shownOptions = stationShownOptions;
        acceptedBountyList.shownOptions = acceptedShownOptions;
    }

    public void ShowDefaultVehicle()
    {
        stationBountyList.ShowDefaultVehicle();
    }

    public void DeclineCurrentlyShownBounty()
    {
        var currentlySelected = stationBountyList.GetSelection();
        if (currentlySelected != null)
        {
            BountyTracker.bountyTrackerInScene.ReplaceBounty(currentlySelected.bounty);
            stationBountyList.RemoveShownBounty(currentlySelected);
            acceptedBountyList.RemoveShownBounty(currentlySelected.bounty);
        }
        else
            Debug.LogWarning("PoliceStation: Tried to decline a bounty that does not exist");
    }
    public void AcceptCurrentlyShownBounty()
    {
        var currentlySelected = stationBountyList.GetSelection();
        if (currentlySelected != null)
        {
            if (!acceptedBountyList.HasBounty(currentlySelected.bounty))
                acceptedBountyList.AddBountyOption(currentlySelected.bounty);
            else
                Debug.LogWarning("PoliceStation: Cannot accept already accepted bounty");
        }
        else
            Debug.LogWarning("PoliceStation: Tried to accept a bounty that does not exist");
    }

    private void ClearCaughtBounties()
    {
        ClearCaughtBounties(stationBountyList);
        ClearCaughtBounties(acceptedBountyList);
    }
    private void ClearCaughtBounties(BountyList bountyList)
    {
        var shownBounties = bountyList.GetShownBounties();
        for (int i = 0; i < shownBounties.Length; i++)
        {
            var currentShownBounty = shownBounties[i];
            if (currentShownBounty.bounty.caught)
                bountyList.RemoveShownBounty(i);
        }
    }

    private void UpdateShownBounties()
    {
        var currentBounties = BountyTracker.bountyTrackerInScene.currentBounties;
        for (int i = 0; i < currentBounties.Length; i++)
        {
            var currentBounty = currentBounties[i];
            stationBountyList.AddBountyOption(currentBounty);
        }
    }
}
