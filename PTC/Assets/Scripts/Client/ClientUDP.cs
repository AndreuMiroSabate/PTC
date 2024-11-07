﻿using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using System;
using System.Xml.Serialization;
using System.IO;

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
}

public class ClientUDP : MonoBehaviour
{
    UdpClient udpClient;
    IPEndPoint ipep;

    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    string clientText = "";
    public TextMeshProUGUI message;

    // Función llamada al inicio del juego para inicializar el UI
    void Start()
    {
        //UItext = UItextObj.GetComponent<TextMeshProUGUI>();  // Obtener el componente TextMeshProUGUI del objeto UI
        DontDestroyOnLoad(gameObject);
    }

    public void StartUDPClient(TextMeshProUGUI ipAddress)
    {
        Debug.Log(ipAddress.text);
        udpClient = new UdpClient();
        ipep = new IPEndPoint(IPAddress.Parse(ipAddress.text.Trim()), 9050);

        // Start receiving data asynchronously
        udpClient.BeginReceive(Receive, null);
    }


    // Función que actualiza el texto mostrado en la UI
    void Update()
    {
        //UItext.text = clientText;  // Actualizar el texto en el UI con el mensaje recibido
    }

    // Función que envía mensajes al servidor
    void Send(Packet paquete)
    {
        var t = new Packet();
        t.playerPosition = paquete.playerPosition;
        t.playerRotation = paquete.playerRotation;
        t.playerCanonRotation = paquete.playerCanonRotation;
        t.playerID = paquete.playerID;
        XmlSerializer serializer = new XmlSerializer(typeof(Packet));
        MemoryStream stream = new MemoryStream();
        serializer.Serialize(stream, t);
        byte[] sendBytes = stream.ToArray();

        // Send the message to the server
        udpClient.Send(sendBytes, sendBytes.Length, ipep);
    }

    // Función que permite enviar un mensaje personalizado desde la UI al servidor
    public void SendMessage()
    {
        // Obtener el mensaje desde el campo de texto de la UI
        string handshake = message.text;

        byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(handshake);

        // Send the message to the server
        udpClient.Send(sendBytes, sendBytes.Length, ipep);

        // Actualizar el texto del cliente con el mensaje enviado
        clientText += "\n" + message.text;
        Debug.Log("Sent to server: " + message);
    }

    // Función que recibe los mensajes enviados por el servidor y los muestra en la UI
    private void Receive(IAsyncResult result)
    {
        byte[] bytes = new byte[1024];

        XmlSerializer serializer = new XmlSerializer(typeof(Packet));
        var t = new Packet();

        MemoryStream stream = new MemoryStream();

        stream.Write(bytes, 0, bytes.Length);
        stream.Seek(0, SeekOrigin.Begin);

        t = (Packet)serializer.Deserialize(stream);

        Debug.Log("Received from server: " + t);

        // Process the received data

        // Continue receiving data asynchronously
        udpClient.BeginReceive(Receive, null);
    }
}