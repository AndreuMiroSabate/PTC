using TMPro;
using System;
using System.IO;
using System.Net;
using UnityEngine;
using System.Net.Sockets;
using System.Xml.Serialization;
using System.Collections.Generic;

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

    // vida
    public float life;
    // id player
    public string playerID;
    //nombre del player
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

    //Guarda una lista de referencias de todos los players en la lobby/escena
    List<PlayerScript> currentLobbyPlayers = new List<PlayerScript>();

    // Función llamada al inicio del juego para inicializar el UI
    void Start()
    {
        //UItext = UItextObj.GetComponent<TextMeshProUGUI>();  // Obtener el componente TextMeshProUGUI del objeto UI
    }

    public void StartUDPClient()
    {
        string finalIP = serverIP1.text.Trim();
        udpClient = new UdpClient();
        ipep = new IPEndPoint(IPAddress.Parse("192.168.1.101"), 9050);

        playerID = Guid.NewGuid().ToString();

        Packet packet = new Packet();
        packet.playerID = playerID;

        DontDestroyOnLoad(gameObject);

        // Start receiving data asynchronously
        Debug.Log("Starting to receive data from server...");
        udpClient.BeginReceive(Receive, null);

        Debug.Log("Sending initial packet to server...");
        Send(packet);  // Enviar el primer paquete al servidor como handshake
    }


    // Función que actualiza el texto mostrado en la UI
    void Update()
    {
        //UItext.text = clientText;  // Actualizar el texto en el UI con el mensaje recibido
    }

    // Función que envía mensajes al servidor
    void Send(Packet paquete)
    {
        //Serializa paquete --START--
        var t = new Packet();
        t.playerPosition = paquete.playerPosition;
        t.playerRotation = paquete.playerRotation;
        t.playerCanonRotation = paquete.playerCanonRotation;
        t.playerID = paquete.playerID;
        t.playerName = paquete.playerName;
        XmlSerializer serializer = new XmlSerializer(typeof(Packet));
        MemoryStream stream = new MemoryStream();
        serializer.Serialize(stream, t);
        byte[] sendBytes = stream.ToArray();
        //Serializa paquete --END--

        sendBytes = System.Text.Encoding.UTF8.GetBytes("Hello from client");

        // Send the message to the server
        udpClient.Send(sendBytes, sendBytes.Length, ipep);
    }

    // Función que permite enviar un mensaje personalizado desde la UI al servidor
    //public void SendMessage()
    //{
    //    // Obtener el mensaje desde el campo de texto de la UI
    //    string handshake = message.text;

    //    byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(handshake);

    //    // Send the message to the server
    //    udpClient.Send(sendBytes, sendBytes.Length, ipep);

    //    // Actualizar el texto del cliente con el mensaje enviado
    //    clientText += "\n" + message.text;
    //    Debug.Log("Sent to server: " + message);
    //}

    // Función que recibe los mensajes enviados por el servidor y los muestra en la UI
    private void Receive(IAsyncResult result)
    {
        try
        {
            // Recibe los datos del servidor
            byte[] bytes = udpClient.EndReceive(result, ref ipep);

            if (bytes.Length > 0)
            {
                Debug.Log("Received data from server!");

                // Deserializar el paquete recibido
                XmlSerializer serializer = new XmlSerializer(typeof(Packet));
                MemoryStream stream = new MemoryStream(bytes);
                Packet t = (Packet)serializer.Deserialize(stream);

                Debug.Log("Packet deserialized successfully!");
                Debug.Log("Received packet ID: " + t.playerID);

                // Aquí puedes procesar el paquete y actualizar el cliente
            }
            else
            {
                Debug.LogWarning("Received empty packet from server.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving data: " + e.Message);
        }

        // Continúa recibiendo datos de manera asíncrona
        udpClient.BeginReceive(Receive, null);
    }

    void OnApplicationQuit()
    {
        udpClient.Close();
    }
}