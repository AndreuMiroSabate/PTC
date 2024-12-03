using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    // Get packet function
    public WorldPacket GetServerWorldPacket()
    {
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

    private void Update()
    {
        // -- DEBUG KEY --
        if (Input.GetKeyDown(KeyCode.V))
        {
            SpawnRandomPowerUp();
            Debug.Log("Spawn Object");
        }
    }

    // Spawn power up function
    void SpawnRandomPowerUp()
    {
        string randomID = worldObjects.ElementAt(Random.Range(0, worldObjects.Count)).Key;

        //Change en algun momento TODO
        Vector2 spawnPos = new Vector2(Random.Range(15, -15), Random.Range(15, -15));

        localWorldPacket = new WorldPacket
        {
            worldAction = WorldActions.SPAWN,
            worldPacketID = randomID,
            powerUpPosition = new Vector3(spawnPos.x, -3, spawnPos.y),
        };
    }
}
