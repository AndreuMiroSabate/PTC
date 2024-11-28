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
