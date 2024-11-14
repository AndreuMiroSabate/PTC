using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using System.IO;
using System.Xml.Serialization;
using System;

public class Server : MonoBehaviour
{
    Socket socket;
    string serverText;

    public void startServer()
    {
        serverText = "Starting UDP Server...";

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ipep);

        //TO DO 3
        //Our client is sending a handshake, the server has to be able to recieve it
        //It's time to call the Receive thread
        Thread newConnection = new Thread(Receive);
        newConnection.Start();
        Debug.Log(serverText);
    }

    void Receive()
    {
        int recv;
        byte[] data = new byte[1024];
        Packet t = new Packet();

        serverText = "\n" + "Waiting for new Client...";
        Debug.Log(serverText);

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint Remote = (EndPoint)(sender);

        while (true)
        {
            try
            {
                recv = socket.ReceiveFrom(data, ref Remote);

                if (data.Length > 0)
                {
                    Debug.Log("Received data from client");

                    using (MemoryStream stream = new MemoryStream(data))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(Packet));
                        t = (Packet)serializer.Deserialize(stream);

                        Debug.Log("Packet deserialized successfully from client: Player ID - " + t.playerID);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error in receiving data: " + e.Message);
            }

            Thread sendPing = new Thread(() => Send(t,Remote));
            sendPing.Start();
        }
    }

    void Send(Packet paquete ,EndPoint Remote)
    {
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
        socket.SendTo(sendBytes, Remote);
    }
}
