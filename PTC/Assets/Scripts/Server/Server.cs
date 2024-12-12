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

[System.Serializable]
public struct ThePacket
{
    public PlayerPacket playerPacket;
    public WorldPacket worldPacket;
}

public class Server : MonoBehaviour
{
    private Socket socket;
    private string serverText;
    private readonly List<EndPoint> endPoints = new List<EndPoint>();
    private List<ThePacket> playerInLobbyPacket = new List<ThePacket>();
    private ConcurrentQueue<ThePacket> receivedPackets = new ConcurrentQueue<ThePacket>();

    private readonly object lockObject = new object();

    private bool gameStarted = false;
    private bool newPlayerJoined = false;
    private Button startGameButton = null;

    private ReplicationManagerServer replicationManagerServer;

    public void StartServer()
    {
        serverText = "Starting UDP Server...";

        replicationManagerServer = GetComponent<ReplicationManagerServer>();

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
        while (receivedPackets.TryDequeue(out ThePacket packet))
        {
            Broadcast(packet);
        }
    }

    private void Receive()
    {
        byte[] data = new byte[2048];
        ThePacket receivedPacket;

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
                    XmlSerializer serializer = new XmlSerializer(typeof(ThePacket));
                    receivedPacket = (ThePacket)serializer.Deserialize(stream);
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

                Debug.Log("Received packet from client: Player ID - " + receivedPacket.playerPacket.playerID);
                Debug.Log("Received packet from client with world instance of - " + receivedPacket.worldPacket.worldPacketID);

                // Enqueue the received packet to all connected clients
                receivedPackets.Enqueue(receivedPacket);
            }
            catch (Exception e)
            {
                Debug.LogError("Error in receiving data: " + e.Message);
            }
        }
    }

    private ThePacket SpawnPointPositionForPlayer(ThePacket packet)
    {
        string objSpawnPos = "Player_" + endPoints.Count.ToString() + "_SpawnPoint";

        if (!startGameButton)
        {
            GameObject.Find("WarningForPlayer").SetActive(false);
            startGameButton = GameObject.Find("StartGameButton").GetComponent<Button>();

            packet.playerPacket.playerPosition = GameObject.Find(objSpawnPos).transform.position;

            startGameButton.onClick.AddListener(StartGame);
        }

        playerInLobbyPacket.Add(packet);
        return packet;
    }

    private void StartGame()
    {
        startGameButton.gameObject.SetActive(false);
        gameStarted = true;

        for (int i = 0; i < playerInLobbyPacket.Count; i++)
        {
            int x = i + 1;
            string objSpawnPos = "Player_" + x + "_SpawnPoint";

            //Create packet
            PlayerPacket packet = new PlayerPacket
            {
                playerPosition = GameObject.Find(objSpawnPos).transform.position,
                playerAction = PlayerAction.START_GAME,
                playerID = playerInLobbyPacket[i].playerPacket.playerID,
                playerName = playerInLobbyPacket[i].playerPacket.playerName,
            };

            ThePacket thePacket = new ThePacket
            {
                playerPacket = packet,
                worldPacket = new WorldPacket
                {
                    worldAction = WorldActions.NONE,
                    worldPacketID = "",
                    powerUpPosition = Vector3.zero,
                },
            };

            Broadcast(thePacket);
        }
    }

    private void Broadcast(ThePacket packet)
    {
        byte[] sendBytes;

        if (newPlayerJoined)
        {
            packet = SpawnPointPositionForPlayer(packet);
            newPlayerJoined = false;
        }

        // Get Server World Packet
        if(packet.worldPacket.worldPacketID.Equals("")) 
            packet.worldPacket = replicationManagerServer.GetServerWorldPacket();
        
        replicationManagerServer.ResetServerWorldPacket();

        // Serialize the packet
        using (MemoryStream stream = new MemoryStream())
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ThePacket));
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