using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerScript : MonoBehaviour
{
    public Rigidbody playerRb;
    public Transform playerTrans;
    public float speed = 17;

    [Space]
    public Transform canonTransform;
    public LayerMask floorLayer;
    [Space]
    public Transform canonBarrelTransform;
    public GameObject bulletPref;
    [Space]
    public TextMeshPro playerNameTXT;

    public delegate void UpdatePackages(Packet package);

    public UpdatePackages playerUpdate;

    [HideInInspector]
    public Packet playerPacket;

    [HideInInspector]
    public string playerID;

    [HideInInspector]
    public string playerName;

    [HideInInspector]
    public float rotation = 60f;

    private void Start()
    {
        playerUpdate?.Invoke(playerPacket);
    }

    private void Update()
    {
        // Shoot (Cambiar luego para que mande msg al server)
        if (Input.GetMouseButtonUp(0))
        {
            playerPacket.playerAction = PlayerAction.SHOOT;
            updatePacket();
        }
    }

    void FixedUpdate()
    {
        // Canon behaviour
        AimCannonAtMouse();

        // Get the input from the Horizontal and Vertical axes
        float verticalInput = Input.GetAxis("Vertical");   // Forward/Backward (-1 to 1)
        float horizontalInput = Input.GetAxis("Horizontal"); // Left/Right (-1 to 1)

        // Move the player based on vertical input
        if (Mathf.Abs(verticalInput) > 0.01f) // Add a small threshold to avoid unnecessary updates
        {
            Vector3 movement = transform.right * verticalInput * speed * Time.deltaTime;
            transform.Translate(movement, Space.World);
            updatePacket();
        }

        // Rotate the player based on horizontal input
        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            Quaternion deltaRotation = Quaternion.Euler(Vector3.up * horizontalInput * rotation * Time.deltaTime);
            transform.rotation *= deltaRotation;
            updatePacket();
        }
    }

    void AimCannonAtMouse()
    {
        // Get the mouse position in screen space
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Perform the raycast
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorLayer))
        {
            // Get the point where the raycast hits the floor
            Vector3 targetPoint = hit.point;
            targetPoint.y = canonTransform.position.y;

            // Calculate the direction from the cannon to the target point
            Vector3 direction = (targetPoint - canonTransform.position).normalized;

            // Calculate the angle between the target direction and the global right vector
            float angle = Vector3.SignedAngle(Vector3.right, direction, Vector3.up);

            // Apply rotation to the canon
            canonTransform.rotation = Quaternion.Euler(0, angle, -90);

            Debug.DrawLine(canonTransform.position, targetPoint, Color.red, 0.1f);
        }
    }

    //Funcion para lanzar un proyectil
    public void FireProjectile()
    {
        //Check cooldown

        // Shoot particle and effects

        // Instance bullet
        GameObject projectile = Instantiate(bulletPref, canonBarrelTransform.position, canonBarrelTransform.rotation);
        projectile.GetComponent<BouncingBullet>().GetAllValues(gameObject, canonBarrelTransform.forward, 1, 1);
    }

    //Funcion para recibir da�o
    public void ReceiveDamage()
    {
    
    }

    //Funci�n para actualizar valores
    public void GetPlayerValues(Packet playerPacket)
    {
        //postion
        transform.position = playerPacket.playerPosition;
        
        //rotation
        transform.rotation = playerPacket.playerRotation;

        //canon rotation
        canonTransform.rotation = playerPacket.playerCanonRotation;

        //actions 
        switch (playerPacket.playerAction)
        {
            case PlayerAction.GET_DAMAGE:
                ReceiveDamage();
                break;
            case PlayerAction.SHOOT:
                FireProjectile();
                break;
            case PlayerAction.DIE:
                //TODO
                break;
        }

    }

    void updatePacket()
    {
        playerPacket.playerPosition = transform.position;
        playerPacket.playerRotation = transform.rotation;
        playerPacket.playerCanonRotation = canonTransform.rotation;

        playerUpdate?.Invoke(playerPacket);

        //Reinicia la accion
        playerPacket.playerAction = PlayerAction.NONE;
    }

    public void SetInitialValues(string PlayerID, string PlayerName)
    {
        playerPacket = new Packet();
        playerPacket.playerID = playerID = PlayerID;
        playerPacket.playerPosition = transform.position;
        playerPacket.playerRotation = transform.rotation;
        playerPacket.playerName = playerName = playerNameTXT.text = PlayerName;
    }
}
