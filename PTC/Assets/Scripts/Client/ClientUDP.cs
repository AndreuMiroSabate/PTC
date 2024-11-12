using TMPro;
using System;
using System.IO;
using System.Net;
using UnityEngine;
using System.Net.Sockets;
using System.Xml.Serialization;
using System.Collections.Generic;
//using System.IO.Ports;
using System.Text;

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
        ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);
        udpClient = new UdpClient(ipep);

        playerID = Guid.NewGuid().ToString();

        Packet packet = new Packet();
        packet.playerID = playerID;
        packet.playerName = "jiji";

        //No se destruya 
        DontDestroyOnLoad(gameObject);

        // Start receiving data asynchronously
        udpClient.BeginReceive(Receive, null);
        XmlSerializer serializer = new XmlSerializer(typeof(Packet));
        MemoryStream stream = new MemoryStream();
        serializer.Serialize(stream, packet);
        byte[] sendBytes = stream.ToArray();
        udpClient.Send(sendBytes, sendBytes.Length, ipep);
        //Send(packet);
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
        Packet t = new Packet();
        try
        {
            byte[] bytes = udpClient.EndReceive(result, ref ipep);

            if (bytes.Length > 0)
            {
                Debug.Log("Received data from server");

                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Packet));
                    t = (Packet)serializer.Deserialize(stream);

                    Debug.Log("Packet deserialized successfully: Player ID - " + t.playerID);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error in receiving data: " + e.Message);
        }

        Debug.Log("Received");

        udpClient.BeginReceive(Receive, null);

        // Process the received data
        foreach (var item in currentLobbyPlayers)
        {
            if (t.playerID.Equals(item.playerID))
            {
                item.transform.position = t.playerPosition;
                item.transform.rotation = t.playerRotation;
                //TODO

                return;
            }
        }

        {
            //Instancia un nuevo jugador
            PlayerScript ps = Instantiate(tankPref, t.playerPosition, t.playerRotation).GetComponent<PlayerScript>();
            Instantiate(tankPref, t.playerPosition, t.playerRotation);
            //Añadir el jugador a la lista de referencias
            currentLobbyPlayers.Add(ps);

            //Asignar los valores basicos al player (ID, nombre)
            ps.playerID = t.playerID;
            ps.playerName = t.playerName;

            //Asignar al delegado la funcion del cliente de enviar mensajes
            //ps.playerUpdate += Send;

            XmlSerializer serializer = new XmlSerializer(typeof(Packet));
            MemoryStream stream = new MemoryStream();
            serializer.Serialize(stream, ps);
            byte[] sendBytes = stream.ToArray();
            //Serializa paquete --END--

            // Send the message to the server
            udpClient.Send(sendBytes, sendBytes.Length, ipep);
        }
    }

    void OnApplicationQuit()
    {
        udpClient.Close();
    }
}