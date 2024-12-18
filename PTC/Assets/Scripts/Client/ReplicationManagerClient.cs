using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;

public class ReplicationManagerClient : MonoBehaviour
{
    [SerializedDictionary ("ITEM ID", "ITEM PREFAB")]
    public SerializedDictionary<string, GameObject> worldObjects = new SerializedDictionary<string, GameObject>();

    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();

    private WorldPacket localWorldPacket;

    private void Awake()
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
    public WorldPacket GetClientWorldPacket()
    {
        return localWorldPacket;
    }

    public void ReceiveWorldPacket(WorldPacket packet)
    {
        switch (packet.worldAction)
        {
            case WorldActions.SPAWN:
                SpawnObject(packet);
                break;

            case WorldActions.DESTROY:
                DestroyObject(packet);
                break;

            case WorldActions.NONE:
                //DOES NOTHING - default state
                break;
        }
    }

    private void SpawnObject(WorldPacket packet)
    {
        if (!worldObjects.ContainsKey(packet.worldPacketID)) return;

        GameObject objToSpawn;
        worldObjects.TryGetValue(packet.worldPacketID, out objToSpawn);

        GameObject obj = Instantiate(objToSpawn, packet.powerUpPosition, Quaternion.identity);

        spawnedObjects.Add(packet.worldPacketID, obj);
    }

    private void DestroyObject(WorldPacket packet)
    {
        if (!spawnedObjects.ContainsKey(packet.worldPacketID)) return;

        GameObject objToDestroy;
        spawnedObjects.TryGetValue(packet.worldPacketID, out objToDestroy);
        spawnedObjects.Remove(packet.worldPacketID);
        spawnedObjects.TrimExcess();
        Destroy(objToDestroy);
    }
}
