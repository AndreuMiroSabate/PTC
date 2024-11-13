using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;


public class Client : MonoBehaviour
{
    Socket socket;
    IPEndPoint ipep;

    [Header("PLAYER PREFAB")]
    public GameObject tankPref;

    List<PlayerScript> currentLobbyPlayers = new List<PlayerScript>();

    // Thread-safe queue for incoming packets
    private ConcurrentQueue<Packet> receivedPackets = new ConcurrentQueue<Packet>();

    // Flag to indicate if the client is disposed
    private bool isDisposed = false;


    public void StartClient()
    {
        ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        //playerID = Guid.NewGuid().ToString();

        Packet packet = new Packet();
        //packet.playerID = playerID;
        packet.playerName = "jiji";
        packet.playerPosition = new Vector3(0, 5, 0);

        //No se destruya 
        DontDestroyOnLoad(gameObject);
        
        //Send(packet);
        Thread mainThread = new Thread(() => Send(packet));
        mainThread.Start();
    }

    private void Send(Packet packet)
    {
        ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);
        XmlSerializer serializer = new XmlSerializer(typeof(Packet));
        MemoryStream stream = new MemoryStream();
        serializer.Serialize(stream, packet);
        byte[] sendBytes = stream.ToArray();
        socket.SendTo(sendBytes,ipep);
    }

    private void Receive()
    {
        if (isDisposed) return; // Stop execution if the client is disposed

        byte[] data = new byte[1500];
        EndPoint Remote = (EndPoint)(ipep);

        try
        { 
            int receivedData = socket.ReceiveFrom(data, ref Remote);

                Debug.Log("Received data from server");

                Packet packet = new Packet();
                using (MemoryStream stream = new MemoryStream(receivedData))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Packet));
                    packet = (Packet)serializer.Deserialize(stream);
                    Debug.Log("Deserialized packet successfully: Player ID - " + packet.playerID);
                }

                // Enqueue the packet for processing on the main thread
                receivedPackets.Enqueue(packet);
        }
        catch (ObjectDisposedException)
        {
            Debug.LogWarning("UdpClient is disposed, stopping Receive.");
            return;
        }
        catch (Exception e)
        {
            Debug.LogError("Error in receiving data: " + e.Message);
        }
    }

    void Update()
    {
        // Process all received packets in the queue
        while (receivedPackets.TryDequeue(out Packet packet))
        {
            ProcessPacket(packet);
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
    // Safely close the UDP client on application exit
    void OnApplicationQuit()
    {
        isDisposed = true;
        if (socket != null)
        {
            socket.Close();
            socket.Dispose();
        }
    }
}
