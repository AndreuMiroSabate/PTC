using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public Rigidbody playerRb;
    public Transform playerTrans;
    public float speed = 17;

    public delegate void UpdatePackages(Packet package);

    public UpdatePackages playerUpdate;

    Packet playerPacket;

    [HideInInspector]
    public string playerID;

    [HideInInspector]
    public string playerName;

    Vector3 rotationRight = new Vector3(0, 60, 0);
    Vector3 rotationLeft = new Vector3(0, -60, 0);

    Vector3 forward = new Vector3(1, 0, 0);
    Vector3 backward = new Vector3(-1, 0, 0);

    private void Start()
    {
        playerUpdate?.Invoke(playerPacket);
    }

    void FixedUpdate()
    {
        if (Input.GetKey("w")) //Vertical (tienen valores entre 1, -1)
        {
            transform.Translate(forward * speed * Time.deltaTime);
            updatePacket();
        }
        if (Input.GetKey("s")) //Quitar
        {
            transform.Translate(backward * speed * Time.deltaTime);
            updatePacket();
        }

        if (Input.GetKey("d")) //Horizontal (tienen valores entre 1, -1)
        {
            Quaternion deltaRotationRight = Quaternion.Euler(rotationRight * Time.deltaTime);
            playerRb.MoveRotation(playerRb.rotation * deltaRotationRight);
            updatePacket();
        }

        if (Input.GetKey("a")) //Quitar
        {
            Quaternion deltaRotationLeft = Quaternion.Euler(rotationLeft * Time.deltaTime);
            playerRb.MoveRotation(playerRb.rotation * deltaRotationLeft);
            updatePacket();
        }
    }

    void updatePacket()
    {
        playerPacket.playerPosition = transform.position;
        playerPacket.playerRotation = transform.rotation;

        playerUpdate?.Invoke(playerPacket);
    }

    public void SetInitialValues(string PlayerID, string PlayerName)
    {
        playerPacket = new Packet();
        playerPacket.playerID = playerID = PlayerID;
        playerPacket.playerPosition = transform.position;
        playerPacket.playerRotation = transform.rotation;
        playerPacket.playerName = playerName = PlayerName;
    }
}
