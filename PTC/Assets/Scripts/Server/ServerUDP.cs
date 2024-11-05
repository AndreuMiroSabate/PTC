using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;

public class ServerUDP : MonoBehaviour
{
    Socket socket;

    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    string serverText;

    // Función llamada al inicio del juego para inicializar el UI
    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();  // Obtener el componente TextMeshProUGUI del objeto UI
    }

    // Función para iniciar el servidor UDP
    public void startServer()
    {
        serverText = "Starting UDP Server...";  // Establecer texto inicial en el UI

        // Crear un punto de enlace (IPEndPoint) que escuche en el puerto 9050 de cualquier dirección IP
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);  // Crear un socket UDP
        socket.Bind(ipep);  // Enlazar el socket al puerto 9050

        // Crear un hilo que se encargará de recibir los mensajes de los clientes
        Thread newConnection = new Thread(Receive);
        newConnection.Start();  // Iniciar el hilo para recibir mensajes
    }

    // Actualización del UI con el texto del servidor
    void Update()
    {
        UItext.text = serverText;  // Actualizar el texto mostrado en el UI
    }

    // Función que maneja la recepción de mensajes desde los clientes
    void Receive()
    {
        int recv;
        byte[] data = new byte[1024];  // Buffer para almacenar los datos recibidos

        serverText = serverText + "\n" + "Waiting for new Client...";  // Informar que estamos esperando nuevos clientes

        // Crear un endpoint que pueda recibir mensajes desde cualquier dirección IP
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint Remote = (EndPoint)(sender);  // Convertir el IPEndPoint en un EndPoint genérico

        // Bucle infinito para recibir mensajes de cualquier cliente
        while (true)
        {
            recv = socket.ReceiveFrom(data, ref Remote);  // Recibir datos del cliente y almacenar la dirección remota
            serverText = serverText + "\n" + string.Format("Message received from {0}:", Remote.ToString());  // Mostrar en el UI la dirección del cliente
            serverText = serverText + "\n" + Encoding.ASCII.GetString(data, 0, recv);  // Mostrar el contenido del mensaje recibido

            // Después de recibir el mensaje, enviamos un ping al cliente
            Thread sendPing = new Thread(() => Send(Remote));  // Crear un hilo para enviar el ping
            sendPing.Start();  // Iniciar el hilo de envío
        }
    }

    // Función que envía un mensaje de "Ping" al cliente
    void Send(EndPoint Remote)
    {
        string welcome = "Ping";  // Mensaje de saludo
        byte[] data = Encoding.ASCII.GetBytes(welcome);  // Convertir el mensaje a bytes
        socket.SendTo(data, Remote);  // Enviar el mensaje "Ping" al cliente utilizando su dirección remota
    }
}
