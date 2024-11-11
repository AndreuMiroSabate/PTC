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
        udpServer = new UdpClient(port);
        remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
       
        Debug.Log("Server started. Waiting for messages...");
 
        DontDestroyOnLoad(gameObject);

        // Start receiving data asynchronously
        udpServer.BeginReceive(Receive, null);
    }

    // Actualización del UI con el texto del servidor
    void Update()
    {
        //UItext.text = serverText;  // Actualizar el texto mostrado en el UI
    }

    // Función que maneja la recepción de mensajes desde los clientes
    void Receive(IAsyncResult result)
    {
        try
        {
            byte[] bytes = udpServer.EndReceive(result, ref remoteEndPoint);

            if (bytes.Length > 0)
            {
                Debug.Log("Received data from client at IP: " + remoteEndPoint.Address.ToString());

                // Deserializar el paquete recibido
                XmlSerializer serializer = new XmlSerializer(typeof(string));
                MemoryStream stream = new MemoryStream(bytes);
                string t = (string)serializer.Deserialize(stream);

                Debug.Log("Packet deserialized successfully from client!");
                //Debug.Log("Client packet ID: " + t.playerID);

                // Responder al cliente para confirmar la conexión
                //Send(t, remoteEndPoint);
            }
            else
            {
                Debug.LogWarning("Received empty packet from client.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving data: " + e.Message);
        }

        // Continúa recibiendo datos de manera asíncrona
        udpServer.BeginReceive(Receive, null);
    }


    // Función que envía un mensaje de "Ping" al cliente
    void Send(Packet paquete, IPEndPoint Remote)
    {
        //Serializa paquete --START--

        var t = new Packet();
        t.playerPosition = paquete.playerPosition;
        t.playerRotation = paquete.playerRotation;
        t.playerCanonRotation = paquete.playerCanonRotation;
        XmlSerializer serializer = new XmlSerializer(typeof(Packet));
        MemoryStream stream = new MemoryStream();
        serializer.Serialize(stream, t);
        byte[] sendBytes = stream.ToArray();

        //Serializa paquete --END--

        // Send the message to the server
        udpServer.Send(sendBytes, sendBytes.Length, Remote);
    }
}
