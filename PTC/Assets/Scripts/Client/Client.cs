using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System;
using System.Xml.Serialization;
using System.IO;

public class Client : MonoBehaviour
{
    Socket socket;
    string playerID;

    [Header("PLAYER PREFAB")]
    public GameObject tankPref;

    // Stores references to all players in the lobby/scene
    List<PlayerScript> currentLobbyPlayers = new List<PlayerScript>();

    // Thread-safe queue for incoming packets
    private ConcurrentQueue<Packet> receivedPackets = new ConcurrentQueue<Packet>();

    // Flag to indicate if the client is disposed
    private bool isDisposed = false;

    public void StartClient()
    {
        playerID = Guid.NewGuid().ToString();

        Packet initial = new Packet();
        initial.playerID = playerID;
        initial.playerName = "jiji";
        initial.playerPosition = new Vector3(0, 5, 0);

        Thread mainThread = new Thread(() => Send(initial));
        mainThread.Start();

        Thread receive = new Thread(Receive);
        receive.Start();

        //
        DontDestroyOnLoad(gameObject);

        Debug.Log("Client Started");
    }

    void Update()
    {
        while (receivedPackets.TryDequeue(out Packet packet))
        {
            ProcessPacket(packet);
        }
    }

    void Send(Packet packet)
    {
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("10.0.53.21"), 9050);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        XmlSerializer serializer = new XmlSerializer(typeof(Packet));
        MemoryStream stream = new MemoryStream();
        serializer.Serialize(stream, packet);
        byte[] sendBytes = stream.ToArray();
        socket.SendTo(sendBytes, ipep);
    }

    void Receive()
    {
        int recv;
        byte[] data = new byte[1024];
        Packet t = new Packet();

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 9050);
        EndPoint Remote = (EndPoint)(sender);

        while (true)
        {
            try
            {
                recv = socket.ReceiveFrom(data, ref Remote);

                if (recv > 0)
                {
                    Debug.Log("Received data from Server");

                    XmlSerializer serializer = new XmlSerializer(typeof(Packet));

                    MemoryStream stream = new MemoryStream();
                    stream.Write(data, 0, recv);
                    stream.Seek(0, SeekOrigin.Begin);
                    t = (Packet)serializer.Deserialize(stream);

                    receivedPackets.Enqueue(t);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error in receiving data: " + e.Message);
            }
        }
    }

    private void ProcessPacket(Packet packet)
    {
        // Check if the player already exists
        bool playerExists = false;
        foreach (var player in currentLobbyPlayers)
        {
            if (packet.playerID.Equals(player.playerID))
            {
                // Update existing player's position and rotation
                player.transform.position = packet.playerPosition;
                player.transform.rotation = packet.playerRotation;

                // Update cannon rotation if needed
                playerExists = true;
                break;
            }
        }

        // If player does not exist, instantiate a new player
        if (!playerExists)
        {
            StartCoroutine(InstancePlayer(packet));
        }
    }

    IEnumerator InstancePlayer(Packet packet)
    {
        yield return new WaitForSeconds(1);
        Debug.Log("Instantiating new player...");
        GameObject instantiatedObj = Instantiate(tankPref, packet.playerPosition, packet.playerRotation);
        PlayerScript playerScript = instantiatedObj.GetComponent<PlayerScript>();
        if (playerScript != null)
        {
            // Add the player to the lobby list and set attributes
            currentLobbyPlayers.Add(playerScript);
            playerScript.SetInitialValues(packet.playerID, packet.playerName);
            playerScript.playerUpdate += Send;
            Debug.Log("Player instantiated and added to lobby: " + playerScript.playerID);

        }
        else
        {
            Debug.LogError("Failed to get PlayerScript component from instantiated object.");
        }
    }

}