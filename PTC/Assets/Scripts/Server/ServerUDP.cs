using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using System;
using System.Xml.Serialization;
using System.IO;

public class ServerUDP : MonoBehaviour
{
    private UdpClient udpServer;
    private IPEndPoint remoteEndPoint;

    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    string serverText;

    // Función llamada al inicio del juego para inicializar el UI
    void Start()
    {
        //UItext = UItextObj.GetComponent<TextMeshProUGUI>();  // Obtener el componente TextMeshProUGUI del objeto UI
    }

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
        byte[] bytes = new byte[1024];

        XmlSerializer serializer = new XmlSerializer(typeof(Packet));
        var t = new Packet();

        MemoryStream stream = new MemoryStream();

        stream.Write(bytes, 0, bytes.Length);
        stream.Seek(0, SeekOrigin.Begin);

        t = (Packet)serializer.Deserialize(stream);

        Debug.Log("Received from client: " + t);

        // Process the received data




        // Continue receiving data asynchronously
        udpServer.BeginReceive(Receive, null);
    }

    // Función que envía un mensaje de "Ping" al cliente
    void Send(Packet paquete, IPEndPoint Remote)
    {
        var t = new Packet();
        t.playerPosition = paquete.playerPosition;
        t.playerRotation = paquete.playerRotation;
        t.playerCanonRotation = paquete.playerCanonRotation;
        XmlSerializer serializer = new XmlSerializer(typeof(Packet));
        MemoryStream stream = new MemoryStream();
        serializer.Serialize(stream, t);
        byte[] sendBytes = stream.ToArray();

        // Send the message to the server
        udpServer.Send(sendBytes, sendBytes.Length, Remote);
    }
}
