using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.XR;

public class Server : MonoBehaviour
{
    private Socket socket;
    private IPEndPoint remoteEndPoint;

    //Start Server
    public void StartServer()
    {
        remoteEndPoint = new IPEndPoint(IPAddress.Any, 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(remoteEndPoint);

        Thread firstConection = new Thread(Receive);
        firstConection.Start();
    }

    private void Receive()
    {
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint Remote = (EndPoint)(sender);
        Packet packet = new Packet();
        byte[] data = new byte[1500];

        try
        {
            int receivedData = socket.ReceiveFrom(data, ref Remote);

            using (MemoryStream stream = new MemoryStream(receivedData))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Packet));
                packet = (Packet)serializer.Deserialize(stream);

                Debug.Log("Packet deserialized successfully from client: Player ID - " + packet.playerID);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error in receiving data: " + e.Message);
        }

        Thread sendPing = new Thread(() => Send(Remote,packet));
        sendPing.Start();
    }

    private void Send(EndPoint remote, Packet packet)
    {
        var t = new Packet();
        t.playerPosition = packet.playerPosition;
        t.playerRotation = packet.playerRotation;
        t.playerCanonRotation = packet.playerCanonRotation;
        t.playerID = packet.playerID;
        t.playerName = packet.playerName;
        XmlSerializer serializer = new XmlSerializer(typeof(Packet));
        MemoryStream stream = new MemoryStream();
        serializer.Serialize(stream, t);
        byte[] sendBytes = stream.ToArray();

        socket.SendTo(sendBytes, remote);
    }

}
