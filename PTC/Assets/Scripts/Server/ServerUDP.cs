using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using System;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;

public class ServerUDP : MonoBehaviour
{
    private UdpClient udpServer;
    private IPEndPoint remoteEndPoint;

    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    string serverText;


    // Función para iniciar el servidor UDP
    public void startServer(int port = 9050)
    {
        remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
        udpServer = new UdpClient(remoteEndPoint);
       
        Debug.Log("Server started. Waiting for messages...");
 
        DontDestroyOnLoad(gameObject);

        // Start receiving data asynchronously
        udpServer.BeginReceive(Receive, udpServer);
    }

    // Actualización del UI con el texto del servidor
    void Update()
    {
        //UItext.text = serverText;  // Actualizar el texto mostrado en el UI
    }

    // Función que maneja la recepción de mensajes desde los clientes
    void Receive(IAsyncResult result)
    {
        Packet t = new Packet();
        try
        {
            byte[] bytes = udpServer.EndReceive(result, ref remoteEndPoint);

            if (bytes.Length > 0)
            {
                Debug.Log("Received data from client");

                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Packet));
                    t = (Packet)serializer.Deserialize(stream);

                    Debug.Log("Packet deserialized successfully from client: Player ID - " + t.playerID);
                    // Aquí puedes procesar el paquete
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error in receiving data: " + e.Message);
        }

        Send(t, remoteEndPoint);

        udpServer.BeginReceive(Receive, udpServer);
    }

    // Función que envía un mensaje de "Ping" al cliente
    void Send(Packet paquete, IPEndPoint Remote)
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
        udpServer.Send(sendBytes, sendBytes.Length, Remote);
    }
}
