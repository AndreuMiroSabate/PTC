using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using System.IO;
using System.Xml.Serialization;
using System;
using System.Collections.Generic;

public class Server : MonoBehaviour
{
    Socket socket;
    string serverText;
    List<EndPoint> endPoints = new List<EndPoint>();

    public void startServer()
    {
        serverText = "Starting UDP Server...";

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ipep);

        Thread newConnection = new Thread(Receive);
        newConnection.Start();

        //
        DontDestroyOnLoad(gameObject);

        Debug.Log(serverText);
    }

    void Receive()
    {
        int recv;
        byte[] data = new byte[1024];
        Packet t = new Packet();

        serverText = "\n" + "Waiting for new Client...";
        Debug.Log(serverText);

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 9050);
        EndPoint Remote = (EndPoint)(sender);

        recv = socket.ReceiveFrom(data, ref Remote);
        if (!endPoints.Contains(Remote))
            endPoints.Add(Remote);

        while (true)
        {
            try
            {
                if (data.Length > 0)
                {
                    Debug.Log("Received data from client");

                    XmlSerializer serializer = new XmlSerializer(typeof(Packet));

                    MemoryStream stream = new MemoryStream();
                    stream.Write(data, 0, data.Length);
                    stream.Seek(0, SeekOrigin.Begin);
                    t = (Packet)serializer.Deserialize(stream);

                    Debug.Log("Packet deserialized successfully from client: Player ID - " + t.playerID);
                    
                    foreach (var ipep in endPoints)
                    {
                        Thread sendPing = new Thread(() => Send(t, ipep));
                        sendPing.Start();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error in receiving data: " + e.Message);
            }
        }
    }

    void Send(Packet paquete, EndPoint Remote)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(Packet));
        MemoryStream stream = new MemoryStream();

        serializer.Serialize(stream, paquete);
        byte[] sendBytes = stream.ToArray();

        socket.SendTo(sendBytes, Remote);
    }
}
