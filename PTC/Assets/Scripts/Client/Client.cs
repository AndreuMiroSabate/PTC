using TMPro;
using System;
using System.IO;
using System.Net;
using UnityEngine;
using System.Threading;
using System.Collections;
using System.Net.Sockets;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections.Concurrent;

[System.Serializable]
public enum PlayerAction
{
    NONE,
    START_GAME,
    GET_DAMAGE,
    SHOOT,
    DIE,

    //TODO pick ups
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

    // Possible player actions
    [XmlElement("PlayerAction")]
    public PlayerAction playerAction;
}

public class Client : MonoBehaviour
{
    private Socket socket;
    private string playerID;
    private bool isDisposed = false;

    [Header("PLAYER PREFAB")]
    public GameObject playerTankPref;
    public GameObject tankPref;

    [Header("INPUT FIELDS")]
    public TMP_InputField serverIPTextMesh;
    public TMP_InputField playerNameTextMesh;

    private List<PlayerScript> currentLobbyPlayers = new List<PlayerScript>();
    private ConcurrentQueue<Packet> receivedPackets = new ConcurrentQueue<Packet>();

    private Thread receiveThread;

    private string serverIP;

    public void StartClient()
    {
        playerID = Guid.NewGuid().ToString();
        serverIP = serverIPTextMesh.text.Trim();

        // Initialize socket
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(serverIP), 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        // Send initial packet
        Packet initialPacket = new Packet
        {
            playerID = playerID,
            playerName = playerNameTextMesh.text,
            playerPosition = new Vector3(0, 5, 0)
        };
        Send(initialPacket);

        // Start receiving thread
        receiveThread = new Thread(Receive);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        DontDestroyOnLoad(gameObject);
        Debug.Log("Client Started with Player ID: " + playerID);
    }

    private void Update()
    {
        // Process received packets
        while (receivedPackets.TryDequeue(out Packet packet))
        {
            ProcessPacket(packet);
        }
    }

    private void OnApplicationQuit()
    {
        DisposeClient();
    }

    private void DisposeClient()
    {
        if (isDisposed) return;

        isDisposed = true;
        Debug.Log("Disposing client...");

        try
        {
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error while closing socket: " + e.Message);
        }

        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
    }

    private void Send(Packet packet)
    {
        try
        {
            IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Parse(serverIP), 9050);

            XmlSerializer serializer = new XmlSerializer(typeof(Packet));
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, packet);
                byte[] sendBytes = stream.ToArray();
                socket.SendTo(sendBytes, serverEndpoint);
            }

            Debug.Log("Sent packet to server: Player ID - " + packet.playerID);
        }
        catch (Exception e)
        {
            Debug.LogError("Error in sending data: " + e.Message);
        }
    }

    private void Receive()
    {
        byte[] buffer = new byte[1024];

        while (!isDisposed)
        {
            try
            {
                EndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
                int receivedBytes = socket.ReceiveFrom(buffer, ref remoteEndpoint);

                if (receivedBytes > 0)
                {
                    using (MemoryStream stream = new MemoryStream(buffer, 0, receivedBytes))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(Packet));
                        Packet receivedPacket = (Packet)serializer.Deserialize(stream);
                        receivedPackets.Enqueue(receivedPacket);
                        Debug.Log("Received packet from server: Player ID - " + receivedPacket.playerID);
                    }
                }
            }
            catch (Exception e)
            {
                if (!isDisposed) // Suppress errors after disposal
                {
                    Debug.LogError("Error in receiving data: " + e.Message);
                }
            }
        }
    }

    private void ProcessPacket(Packet packet)
    {
        // Check if the player already exists
        if (isPlayerInGame(packet, out PlayerScript existingPlayer))
        {
            // Update player's values
            existingPlayer.GetPlayerValues(packet);

            return;
        }

        // Instantiate new player
        StartCoroutine(InstancePlayer(packet));
    }

    private bool isPlayerInGame(Packet packet, out PlayerScript myPlayer)
    {
        foreach (var player in currentLobbyPlayers)
        {
            if (packet.playerID.Equals(player.playerID))
            {
                myPlayer = player;
                return true;
            }
        }

        myPlayer = null;
        return false;
    }

    private IEnumerator InstancePlayer(Packet packet)
    {
        yield return null; // Wait until the next frame to instantiate

        if (isPlayerInGame(packet, out PlayerScript existingPlayer))
            yield break;

        if (GameObject.Find("StartGameButton"))
            GameObject.Find("StartGameButton").SetActive(false);

        Debug.Log("Instantiating new player...");
        GameObject instantiatedObj = Instantiate(playerID == packet.playerID ? playerTankPref : tankPref, 
                                                packet.playerPosition, packet.playerRotation);

        PlayerScript playerScript = instantiatedObj.GetComponent<PlayerScript>();

        if (playerScript != null)
        {
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