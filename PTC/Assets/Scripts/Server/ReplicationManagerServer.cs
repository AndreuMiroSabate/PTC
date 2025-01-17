using AYellowpaper.SerializedCollections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum WorldActions
{
    NONE,
    SPAWN,
    DESTROY
}

[System.Serializable]
public struct WorldPacket
{
    public WorldActions worldAction;

    public string worldPacketID;

    public Vector3 powerUpPosition;
}
public class ReplicationManagerServer : MonoBehaviour
{
    [SerializedDictionary("ITEM ID", "ITEM PREFAB")]
    public SerializedDictionary<string, GameObject> worldObjects = new SerializedDictionary<string, GameObject>();

    public delegate void UpdateWorldPackages(WorldPacket wPackage);
    public UpdateWorldPackages worldUpdate;

    private WorldPacket localWorldPacket;

    private void Start()
    {
        //Initialize world packet
        localWorldPacket = new WorldPacket
        {
            worldAction = WorldActions.NONE,
            worldPacketID = "",
            powerUpPosition = Vector3.zero,
        };
    }

    // Warn the script when scene change
    public void ChangeToGameScene()
    {
        GameObject.Find("StartGameButton").GetComponent<Button>().onClick.AddListener(SpawnInitialPowerUp);
    }

    // Get packet function
    public WorldPacket GetServerWorldPacket()
    {
        if (localWorldPacket.worldAction == WorldActions.DESTROY)
            SpawnRandomPowerUp();

        return localWorldPacket;
    }

    public void ResetServerWorldPacket()
    {
        //Reset world packet
        localWorldPacket = new WorldPacket
        {
            worldAction = WorldActions.NONE,
            worldPacketID = "",
            powerUpPosition = Vector3.zero,
        };
    }

    public void SpawnInitialPowerUp()
    {
        Invoke("SpawnRandomPowerUp", 1f);
    }

    // Spawn power up function
    public void SpawnRandomPowerUp()
    {
        //if (GameObject.FindObjectOfType(typeof(PowerUpBehaviour))) return; //Revise when fix lag
        string randomID = worldObjects.ElementAt(Random.Range(0, worldObjects.Count)).Key;

        //Change en algun momento TODO
        Vector2 spawnPos = new Vector2(Random.Range(30, -30), Random.Range(30, -30));

        localWorldPacket = new WorldPacket
        {
            worldAction = WorldActions.SPAWN,
            worldPacketID = randomID,
            powerUpPosition = new Vector3(spawnPos.x, -3, spawnPos.y),
        };
    }
}
