using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.UI;
using UnityEngine;
using System.Net;
using System.IO;
using System;

public class Server : MonoBehaviour
{
    private Socket socket;
    private string serverText;
    private readonly List<EndPoint> endPoints = new List<EndPoint>();
    private List<string> playersInLobbyIDs = new List<string>();
    private ConcurrentQueue<Packet> receivedPackets = new ConcurrentQueue<Packet>();

    private readonly object lockObject = new object();

    private bool gameStarted = false;
    private bool newPlayerJoined = false;
    private Button startGameButton = null;

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

    private void Update()
    {
        // Process received packets
        while (receivedPackets.TryDequeue(out Packet packet))
        {
            Broadcast(packet);
        }
    }

    private void Receive()
    {
        byte[] data = new byte[1024];
        Packet receivedPacket;

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
                        newPlayerJoined = true;
                        Debug.Log("New client connected: " + remoteEndPoint);
                    }
                }

                Debug.Log("Received packet from client: Player ID - " + receivedPacket.playerID);

                // Enqueue the received packet to all connected clients
                receivedPackets.Enqueue(receivedPacket);
            }
            catch (Exception e)
            {
                Debug.LogError("Error in receiving data: " + e.Message);
            }
        }
    }

    private Packet SpawnPointPositionForPlayer(Packet packet)
    {
        string objSpawnPos = "Player_" + endPoints.Count.ToString() + "_SpawnPoint";
        packet.playerPosition = GameObject.Find(objSpawnPos).transform.position;

        if (!startGameButton)
        {
            GameObject.Find("WarningForPlayer").SetActive(false);
            startGameButton = GameObject.Find("StartGameButton").GetComponent<Button>();
            startGameButton.onClick.AddListener(StartGame);
        }

        playersInLobbyIDs.Add(packet.playerID);
        return packet;
    }

    private void StartGame()
    {
        startGameButton.gameObject.SetActive(false);
        gameStarted = true;

        for (int i = 0; i < playersInLobbyIDs.Count; i++)
        {
            int x = i + 1;
            string objSpawnPos = "Player_" + x + "_SpawnPoint";

            //Create packet
            Packet packet = new Packet
            {
                playerPosition = GameObject.Find(objSpawnPos).transform.position,
                playerAction = PlayerAction.START_GAME,
                playerID = playersInLobbyIDs[i],
            };

            Broadcast(packet);
        }
    }

    private void Broadcast(Packet packet)
    {
        byte[] sendBytes;

        if (newPlayerJoined)
        {
            packet = SpawnPointPositionForPlayer(packet);
            newPlayerJoined = false;
        }

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