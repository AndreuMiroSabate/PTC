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

public class Client_Host : MonoBehaviour
{
    private Socket socket;
    private string playerID;
    private bool isDisposed = false;

    [Header("PLAYER PREFAB")]
    public GameObject playerTankPref;
    public GameObject tankPref;

    private List<PlayerScript> currentLobbyPlayers = new List<PlayerScript>();
    private ConcurrentQueue<ThePacket> receivedPackets = new ConcurrentQueue<ThePacket>();

    private Thread receiveThread;

    private string serverIP;

    public void StartClient(TMP_InputField playerNameTextMesh)
    {
        playerID = Guid.NewGuid().ToString();
        serverIP = GetLocalIPAddress().Trim();

        // Initialize socket
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(serverIP), 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        // Send initial packet
        PlayerPacket initialPacket = new PlayerPacket
        {
            playerID = playerID,
            playerName = playerNameTextMesh.text,
            playerPosition = new Vector3(0, 5, 0)
        };

        ThePacket thePacket = new ThePacket
        {
            playerPacket = initialPacket,
            worldPacket = new WorldPacket 
            {
                worldAction = WorldActions.NONE,
                worlPacketID = "",
                powerUpPosition = Vector3.zero,
            },
        };

        Send(thePacket);

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
        while (receivedPackets.TryDequeue(out ThePacket packet))
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

    private void Send(ThePacket packet)
    {
        try
        {
            IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Parse(serverIP), 9050);

            XmlSerializer serializer = new XmlSerializer(typeof(ThePacket));
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, packet);
                byte[] sendBytes = stream.ToArray();
                socket.SendTo(sendBytes, serverEndpoint);
            }

            Debug.Log("Sent packet to server: Player ID - " + packet.playerPacket.playerID);
        }
        catch (Exception e)
        {
            Debug.LogError("Error in sending data: " + e.Message);
        }
    }

    private void Receive()
    {
        byte[] buffer = new byte[2048];

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
                        XmlSerializer serializer = new XmlSerializer(typeof(ThePacket));
                        ThePacket receivedPacket = (ThePacket)serializer.Deserialize(stream);
                        receivedPackets.Enqueue(receivedPacket);
                        Debug.Log("Received packet from server: Player ID - " + receivedPacket.playerPacket.playerID);
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

    private void ProcessPacket(ThePacket packet)
    {
        // Check if the player already exists
        if (isPlayerInGame(packet, out PlayerScript existingPlayer))
        {
            // Update player's values
            existingPlayer.GetPlayerValues(packet.playerPacket);

            return;
        }

        // Instantiate new player
        StartCoroutine(InstancePlayer(packet));
    }

    private bool isPlayerInGame(ThePacket packet, out PlayerScript myPlayer)
    {
        foreach (var player in currentLobbyPlayers)
        {
            if (packet.playerPacket.playerID.Equals(player.playerID))
            {
                myPlayer = player;
                return true;
            }
        }

        myPlayer = null;
        return false;
    }

    private IEnumerator InstancePlayer(ThePacket packet)
    {
        yield return null; // Wait until the next frame to instantiate

        if (isPlayerInGame(packet, out PlayerScript existingPlayer))
            yield break;

        Debug.Log("Instantiating new player...");
        GameObject instantiatedObj = Instantiate(playerID == packet.playerPacket.playerID ? playerTankPref : tankPref,
                                                packet.playerPacket.playerPosition, packet.playerPacket.playerRotation);

        PlayerScript playerScript = instantiatedObj.GetComponent<PlayerScript>();

        if (playerScript != null)
        {
            currentLobbyPlayers.Add(playerScript);
            playerScript.SetInitialValues(packet.playerPacket.playerID, packet.playerPacket.playerName);
            playerScript.playerUpdate += Send;
            Debug.Log("Player instantiated and added to lobby: " + playerScript.playerID);
        }
        else
        {
            Debug.LogError("Failed to get PlayerScript component from instantiated object.");
        }
    }

    string GetLocalIPAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                // Ensure the IP address is IPv4 and not a loopback address
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error fetching local IP address: " + ex.Message);
        }
        return null;
    }
}
