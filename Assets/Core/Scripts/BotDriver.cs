using UnityEngine;
using UnityHelpers;

public class BotDriver : MonoBehaviour
{
    public VehicleSwitcher vehicles;
    public TreeCollider treeCollider;
    public HumanDriver keepUpWith;

    [Space(10), Tooltip("In m/s")]
    public float speedLimit = 27.78f;

    [Space(10)]
    public Transform frontSpawn;
    public Transform rearSpawn;

    private bool spawnedFromRear;

    private float spawnSpeed;

    private void Awake()
    {
        treeCollider.onTriggerEnter.AddListener(OnTreeTriggerEnter);
    }
    private void Start()
    {
        Respawn();
    }
    private void Update()
    {
        if (vehicles.currentVehicle != null)
        {
            if (vehicles.currentVehicle.transform.position.y < -5)
                Respawn();

            vehicles.currentVehicle.gas = vehicles.currentVehicle.currentSpeed < spawnSpeed ? 1 : 0;
        }
    }
    private void OnTreeTriggerEnter(TreeCollider.CollisionInfo colInfo)
    {
        if (!spawnedFromRear && colInfo.collidedWith.CompareTag("Rear") || spawnedFromRear && colInfo.collidedWith.CompareTag("Front"))
            Respawn();
    }

    public void Respawn()
    {
        vehicles.SetVehicle(Random.Range(0, vehicles.allVehicles.Length));

        spawnSpeed = Random.Range(speedLimit - 5, speedLimit + 5);

        spawnedFromRear = true;
        Transform spawnFrom = rearSpawn;
        if (keepUpWith.vehicles.currentVehicle != null)
        {
            float keepUpDirection = Vector3.Dot(keepUpWith.vehicles.currentVehicle.transform.forward, Vector3.forward);
            float keepUpSpeed = keepUpWith.vehicles.currentVehicle.currentSpeed;
            if ((keepUpDirection < 0 && keepUpSpeed > 0) || keepUpDirection > 0 && (keepUpSpeed < 0 || spawnSpeed < keepUpSpeed))
            {
                spawnFrom = frontSpawn;
                spawnedFromRear = false;
            }
        }
        vehicles.currentVehicle.Teleport(spawnFrom.position, spawnFrom.rotation, spawnSpeed);
    }
}
