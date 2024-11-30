using System.Collections;
using System.Collections.Generic;
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

    public string worlPacketID;

    public Vector3 powerUpPosition;
}
public class ReplicationManagerServer : MonoBehaviour
{
    //public delegate void UpdateWorldPackages(WorldPacket wPackage);

    //public UpdateWorldPackages worldUpdate;
    //private void Start()
    //{
    //    worldUpdate += GameObject.Find("UDP_Server").GetComponent<Server>
    //}

    //private void Send(WorldPacket packet)
    //{
        
    //}
}

//public class ReplicationManagerServer : MonoBehaviour
//{
    //private Dictionary<string, GameObject> worldObjects = new Dictionary<string, GameObject>();

    //public Server server; // Referencia al script UDP del servidor

    //private void Start()
    //{
        //if (server == null)
        //{
            //server = FindObjectOfType<Server>();
        //}
    //}

    //// Registrar un objeto en el mundo con un paquete SPWAN
    //public void RegisterObject(GameObject obj, string packetID)
    //{
        //if (worldObjects.ContainsKey(packetID)) return;

        //worldObjects[packetID] = obj;

        //WorldPacket spawnPacket = new WorldPacket
        //{
            //worldAction = WorldActions.SPAWN,
            //worlPacketID = packetID,
            //powerUpPosition = obj.transform.position
        //};

        //SendWorldPacket(spawnPacket);
    //}

    // Actualizar la posición de un objeto
    //public void UpdateObject(string packetID, Vector3 newPosition)
    //{
        //if (!worldObjects.ContainsKey(packetID)) return;

        //GameObject obj = worldObjects[packetID];
        //obj.transform.position = newPosition;

        //WorldPacket updatePacket = new WorldPacket
        //{
            //worldAction = WorldActions.NONE, // No se requiere acción explícita para actualización simple
            //worlPacketID = packetID,
            //powerUpPosition = newPosition
        //};

        //SendWorldPacket(updatePacket);
    //}

    // Eliminar un objeto del mundo con un paquete DESTROY
    //public void RemoveObject(string packetID)
    //{
        //if (!worldObjects.ContainsKey(packetID)) return;

        //GameObject obj = worldObjects[packetID];
        //worldObjects.Remove(packetID);
        //Destroy(obj);

        //WorldPacket destroyPacket = new WorldPacket
        //{
            //worldAction = WorldActions.DESTROY,
            //worlPacketID = packetID,
        //};

        //SendWorldPacket(destroyPacket);
    //}

    // Enviar un paquete al servidor UDP
    //private void SendWorldPacket(WorldPacket packet)
    //{
        //if (server != null)
        //{
            //server.SendWorldPacket(packet);
        //}
    //}
//}
