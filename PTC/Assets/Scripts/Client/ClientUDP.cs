using TMPro;
using System;
using System.IO;
using System.Net;
using UnityEngine;
using System.Net.Sockets;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using System.Collections;

public enum Message
{
    CLIENT,
    SERVER
}

[System.Serializable]
public struct Packet
{
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    public Quaternion playerCanonRotation;

    // Player health
    public float life;
    // Player ID
    public string playerID;
    // Player name
    public string playerName;
}

public class ClientUDP : MonoBehaviour
{
    UdpClient udpClient;
    IPEndPoint ipep;

    [HideInInspector]
    public string playerID;

    [Space]
    public TextMeshProUGUI serverIP1;

    [Header("PLAYER PREFAB")]
    public GameObject tankPref;

    // Stores references to all players in the lobby/scene
    List<PlayerScript> currentLobbyPlayers = new List<PlayerScript>();

    // Thread-safe queue for incoming packets
    private ConcurrentQueue<Packet> receivedPackets = new ConcurrentQueue<Packet>();

    // Flag to indicate if the client is disposed
    private bool isDisposed = false;

    // Initialize the client
    void Start()
    {
        // Uncomment this for testing if you need to verify tankPref setup
        // TestInstantiatePlayer();
    }

    public void StartUDPClient()
    {
        string finalIP = serverIP1.text.Trim();
        ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);
        udpClient = new UdpClient(ipep);

        playerID = Guid.NewGuid().ToString();

        Packet packet = new Packet();
        packet.playerID = playerID;
        packet.playerName = "jiji";
        packet.playerPosition = new Vector3(0, 5, 0);

        //No se destruya 
        DontDestroyOnLoad(gameObject);

        // Start receiving data asynchronously
        //udpClient.BeginReceive(Receive, null);
        //XmlSerializer serializer = new XmlSerializer(typeof(Packet));
        //MemoryStream stream = new MemoryStream();
        //serializer.Serialize(stream, packet);
        //byte[] sendBytes = stream.ToArray();
        //udpClient.Send(sendBytes, sendBytes.Length, ipep);

        udpClient.BeginReceive(Receive, udpClient);
        Send(packet);

    }

    void Update()
    {
        // Process all received packets in the queue
        while (receivedPackets.TryDequeue(out Packet packet))
        {
            ProcessPacket(packet);
        }
    }

    // Send a packet to the server
    void Send(Packet packet)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(Packet));
        MemoryStream stream = new MemoryStream();
        serializer.Serialize(stream, packet);
        byte[] sendBytes = stream.ToArray();

        // Send the message to the server
        udpClient.Send(sendBytes, sendBytes.Length, ipep);
    }

    // Receive messages from the server
    private void Receive(IAsyncResult result)
    {
        if (isDisposed) return; // Stop execution if the client is disposed

        try
        {
            byte[] bytes = udpClient.EndReceive(result, ref ipep);
            if (bytes.Length > 0)
            {
                Debug.Log("Received data from server");

                Packet packet;
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Packet));
                    packet = (Packet)serializer.Deserialize(stream);
                    Debug.Log("Deserialized packet successfully: Player ID - " + packet.playerID);
                }

                // Enqueue the packet for processing on the main thread
                receivedPackets.Enqueue(packet);
            }
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

        // Continue listening for incoming data if not disposed
        if (!isDisposed)
        {
            try
            {
                udpClient.BeginReceive(Receive, null);
            }
            catch (ObjectDisposedException)
            {
                Debug.LogWarning("Attempted to BeginReceive on a disposed UdpClient.");
            }
        }
    }

    // Process each packet on the main thread
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
        if (udpClient != null)
        {
            udpClient.Close();
            udpClient.Dispose();
        }
    }
}
