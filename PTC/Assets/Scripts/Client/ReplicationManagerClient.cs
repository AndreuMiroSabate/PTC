using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplicationManagerClient : MonoBehaviour
{
    //private Dictionary<string, GameObject> worldObjects = new Dictionary<string, GameObject>();

    //public GameObject powerUpPrefab;

    //public void ReceiveWorldPacket(WorldPacket packet)
    //{
        //switch (packet.worldAction)
        //{
            //case WorldActions.SPAWN:
                //SpawnObject(packet);
                //break;

            //case WorldActions.DESTROY:
                //DestroyObject(packet);
                //break;

            //case WorldActions.NONE:
                //UpdateObject(packet);
                //break;
        //}
    //}

    //private void SpawnObject(WorldPacket packet)
    //{
        //if (worldObjects.ContainsKey(packet.worlPacketID)) return;

        //GameObject obj = Instantiate(powerUpPrefab, packet.powerUpPosition, Quaternion.identity);
        //worldObjects[packet.worlPacketID] = obj;
    //}

    //private void UpdateObject(WorldPacket packet)
    //{
    //    if (!worldObjects.ContainsKey(packet.worlPacketID)) return;

        //GameObject obj = worldObjects[packet.worlPacketID];
        //obj.transform.position = packet.powerUpPosition;
    //}

    //private void DestroyObject(WorldPacket packet)
    //{
        //if (!worldObjects.ContainsKey(packet.worlPacketID)) return;

        //GameObject obj = worldObjects[packet.worlPacketID];
        //worldObjects.Remove(packet.worlPacketID);
        //Destroy(obj);
    //}
}
