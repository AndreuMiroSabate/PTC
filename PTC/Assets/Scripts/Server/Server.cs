using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using System.IO;
using System.Xml.Serialization;
using System;
using System.Collections.Generic;

public class Server : MonoBehaviour
{
    private Socket socket;
    private string serverText;
    private readonly List<EndPoint> endPoints = new List<EndPoint>();
    private readonly object lockObject = new object();

    private bool gameStarted = false;

    public void StartServer()
    {
        serverText = "Starting UDP Server...";

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ipep);

        Thread receiveThread = new Thread(Receive);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        DontDestroyOnLoad(gameObject);

        Debug.Log(serverText);
    }

    private void Receive()
    {
        byte[] data = new byte[1024];
        Packet receivedPacket;

        string objSpawnPos = "";

        Debug.Log("\nWaiting for new clients...");

        while (true)
        {
            try
            {
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                int recv = socket.ReceiveFrom(data, ref remoteEndPoint);

                // Deserialize received data
                using (MemoryStream stream = new MemoryStream(data, 0, recv))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Packet));
                    receivedPacket = (Packet)serializer.Deserialize(stream);
                }

                // Add new clients to the list
                lock (lockObject)
                {
                    if (!endPoints.Contains(remoteEndPoint))
                    {
                        endPoints.Add(remoteEndPoint);
                        Debug.Log("New client connected: " + remoteEndPoint);
                        objSpawnPos = "Player_" + endPoints.Count.ToString() + "_SpawnPoint";

                        receivedPacket.playerPosition = GameObject.Find(objSpawnPos).transform.position;
                    }
                }

                Debug.Log("Received packet from client: Player ID - " + receivedPacket.playerID);

                // Broadcast the received packet to all connected clients
                Broadcast(receivedPacket);
            }
            catch (Exception e)
            {
                Debug.LogError("Error in receiving data: " + e.Message);
            }
        }
    }

    private void Broadcast(Packet packet)
    {
        byte[] sendBytes;

        // Serialize the packet
        using (MemoryStream stream = new MemoryStream())
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Packet));
            serializer.Serialize(stream, packet);
            sendBytes = stream.ToArray();
        }

        // Send to all connected clients
        lock (lockObject)
        {
            foreach (var endPoint in endPoints)
            {
                try
                {
                    socket.SendTo(sendBytes, endPoint);
                    Debug.Log("Sent data to: " + endPoint);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error in sending data to " + endPoint + ": " + e.Message);
                }
            }
        }
    }
}