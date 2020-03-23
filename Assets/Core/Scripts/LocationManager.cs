using UnityEngine;

public class LocationManager : MonoBehaviour
{
    private static LocationManager locationManagerInScene;

    public GameObject[] locations;
    public int locationIndex;
    private int currentIndex = -1;

    private void Awake()
    {
        locationManagerInScene = this;
    }
    private void Update()
    {
        if (locationIndex != currentIndex)
            SetLocation(locationIndex);
    }

    public static void SetLocationStatic(int index)
    {
        locationManagerInScene.SetLocation(index);
    }
    public void SetLocation(int index)
    {
        locationIndex = index;
        currentIndex = index;
        for (int i = 0; i < locations.Length; i++)
            locations[i].SetActive(false);

        locations[locationIndex].SetActive(true);
    }
}
