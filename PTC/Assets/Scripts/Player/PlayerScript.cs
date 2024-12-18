using UnityEngine;
using TMPro;

public enum PlayerState
{
    WAIT,
    PLAYING,
    DEAD,
}
[System.Flags]
public enum PowerUps
{
    None = 0,                   // No power-ups
    SHIELD = 1 << 0,            // 0001
    TRIPLE_SHOT = 1 << 1,       // 0010
    EXPLOTION_BULLETS = 1 << 2, // 0100
    BOUNCING_BULLET = 1 << 3,   // 1000
}

public class PlayerScript : MonoBehaviour
{
    public Rigidbody playerRb;
    public Transform playerTrans;

    [Space]
    public Transform canonTransform;
    public LayerMask floorLayer;
    [Space]
    public Transform canonBarrelTransform;
    public GameObject bulletPref;
    public GameObject smokeParticlePref;
    [Space]
    public TextMeshPro playerNameTXT;
    
    [Space]
    public PowerUps activePowerUps = PowerUps.None;

    public delegate void UpdatePackages(ThePacket package);

    public UpdatePackages playerUpdate;

    [HideInInspector]
    public PlayerState playerState;

    [HideInInspector]
    public PlayerPacket playerPacket;

    [HideInInspector]
    public string playerID;

    [HideInInspector]
    public string playerName;

    [HideInInspector]
    public float speed;

    [HideInInspector]
    public float rotation;

    [Space]
    [Header("HEALTH")]
    public int playerHealth = 3;

    [HideInInspector]
    public int bulletBounceNum = 1;
    [HideInInspector]
    public TrajectoryVisualizer trajectoryVisualizer;

    public float shootCoolDown;
    [HideInInspector]
    public float shootCoolDownTimer;

    [HideInInspector]
    public WorldPacket localWorldPacket;

    [Space]
    [Header("POWER UP PREFABS")]
    public GameObject shieldPref;
    [HideInInspector]
    public GameObject shieldSpawned;

    private void Start()
    {
        if (GetComponent<TrajectoryVisualizer>())
            trajectoryVisualizer = GetComponent<TrajectoryVisualizer>();

        // Def values for player packet
        playerPacket.life = playerHealth;

        shootCoolDownTimer = Time.time;

        // Def values for world packet
        localWorldPacket = new WorldPacket
        {
            worldAction = WorldActions.NONE,
            worldPacketID = "",
            powerUpPosition = Vector3.zero,
        };

        ThePacket thePacket = new ThePacket
        {
            playerPacket = playerPacket,
            worldPacket = localWorldPacket,
        };

        playerUpdate?.Invoke(thePacket);
        playerState = PlayerState.WAIT;
        speed = 10;
        rotation = 90f;
    }

    private void Update()
    {
        if (playerState != PlayerState.PLAYING) return;

        // Shoot (Cambiar luego para que mande msg al server)
        if (Input.GetMouseButtonUp(0))
        {
            playerPacket.playerAction = PlayerAction.SHOOT;
            UpdatePacket();
        }
    }

    void FixedUpdate()
    {
        if (playerState != PlayerState.PLAYING) return;

        // Canon behaviour
        AimCannonAtMouse();

        // Get the input from the Horizontal and Vertical axes
        float verticalInput = Input.GetAxis("Vertical");   // Forward/Backward (-1 to 1)
        float horizontalInput = Input.GetAxis("Horizontal"); // Left/Right (-1 to 1)

        // Move the player based on vertical input using Rigidbody
        if (Mathf.Abs(verticalInput) > 0.01f) // Add a small threshold to avoid unnecessary updates
        {
            Vector3 movement = transform.right * verticalInput * speed * Time.deltaTime;

            RaycastHit hit;
            if (!Physics.Raycast(transform.position, movement.normalized, out hit, movement.magnitude))
            {
                transform.Translate(movement, Space.World);
            }
        }

        // Rotate the player based on horizontal input
        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            Quaternion deltaRotation = Quaternion.Euler(Vector3.up * horizontalInput * rotation * Time.deltaTime);
            transform.rotation *= deltaRotation;
        }

        UpdatePacket();
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

    //Fire a projectile
    public void FireProjectile()
    {
        if (playerState != PlayerState.PLAYING) return;
        if (Time.time - shootCoolDownTimer <= shootCoolDown) return;

        // Shoot particle and effects
        Destroy(Instantiate(smokeParticlePref, canonBarrelTransform.position, canonBarrelTransform.rotation), .5f);

        // Instance bullet
        GameObject projectile = Instantiate(bulletPref, canonBarrelTransform.position, canonBarrelTransform.rotation);
        projectile.GetComponent<BouncingBullet>().GetAllValues(gameObject, canonBarrelTransform.forward, bulletBounceNum, 1, activePowerUps);

        if (HasPowerUp(PowerUps.TRIPLE_SHOT)) TripleShotFunction();

        //Reset Timer
        shootCoolDownTimer = Time.time;
    }

    public void TripleShotFunction() //ONly when power up is active
    {
        GameObject projectile;

        // Bullet angled to the left
        projectile = Instantiate(bulletPref, canonBarrelTransform.position, canonBarrelTransform.rotation);
        Vector3 leftDirection = Quaternion.Euler(0, -45, 0) * canonBarrelTransform.forward; // 15-degree angle to the left
        Vector3 leftEndPoint = canonTransform.position + leftDirection * 10f; // Extend the ray
        Debug.DrawLine(canonTransform.position, leftEndPoint, Color.green, 0.1f);

        projectile.GetComponent<BouncingBullet>().GetAllValues(gameObject, leftDirection, bulletBounceNum, 1, activePowerUps);

        // Bullet angled to the right
        projectile = Instantiate(bulletPref, canonBarrelTransform.position, canonBarrelTransform.rotation);
        Vector3 rightDirection = Quaternion.Euler(0, 45, 0) * canonBarrelTransform.forward; // 15-degree angle to the right
        Vector3 rightEndPoint = canonTransform.position + rightDirection * 10f; // Extend the ray
        Debug.DrawLine(canonTransform.position, rightEndPoint, Color.blue, 0.1f);

        projectile.GetComponent<BouncingBullet>().GetAllValues(gameObject, rightDirection, bulletBounceNum, 1, activePowerUps);
    }

    //Receive damage from other players
    public virtual void ReceiveDamage(PlayerPacket playerPacket)
    {
        //check if power up shield
        if (HasPowerUp(PowerUps.SHIELD))
        {
            Destroy(shieldSpawned);
            return;
        }

        //TODO: feedback animation


        playerHealth -= 1;

        //If player' health is bellow or equal to 0, dies
        if(playerPacket.life <= 0)
        {
            this.playerPacket.playerAction = PlayerAction.DIE;
            UpdatePacket();
        }
    }

    public void BulletHit()
    {
        playerPacket.playerAction = PlayerAction.GET_DAMAGE;
        UpdatePacket();
    }

    public void PlayerDie()
    {
        // TODO: meter corutine para hacerlo más juicy
        Destroy(gameObject, .1f);
    }

    //Update player values
    public void GetPlayerValues(PlayerPacket playerPacket)
    {
        //postion
        transform.position = playerPacket.playerPosition;
        
        //rotation
        transform.rotation = playerPacket.playerRotation;

        //canon rotation
        canonTransform.rotation = playerPacket.playerCanonRotation;

        //player health
        if (playerHealth > playerPacket.life)
            playerHealth = playerPacket.life;

        //actions 
        switch (playerPacket.playerAction)
        {
            case PlayerAction.GET_DAMAGE:
                ReceiveDamage(playerPacket);
                break;
            case PlayerAction.SHOOT:
                FireProjectile();
                break;
            case PlayerAction.DIE:
                PlayerDie();
                break;
            case PlayerAction.START_GAME:
                playerState = PlayerState.PLAYING;
                GameObject.Find("WarningForPlayer")?.SetActive(false);

                //TODO Play sound
                
                break;

            case PlayerAction.SHIELD:
                if (!HasPowerUp(PowerUps.SHIELD))
                {
                    AddPowerUp(PowerUps.SHIELD);
                    shieldSpawned = Instantiate(shieldPref, transform);
                }

                break;
            case PlayerAction.TRIPLE_SHOT:
                AddPowerUp(PowerUps.TRIPLE_SHOT);

                break;
            case PlayerAction.MORE_BOUNCING:
                bulletBounceNum++;
                if(trajectoryVisualizer)trajectoryVisualizer.maxBounces = bulletBounceNum;

                break;
            case PlayerAction.EXPLOTION_BULLETS:
                AddPowerUp(PowerUps.EXPLOTION_BULLETS);

                break;
        }
    }

    void UpdatePacket()
    {
        playerPacket.playerPosition = transform.position;
        playerPacket.playerRotation = transform.rotation;
        playerPacket.playerCanonRotation = canonTransform.rotation;
        playerPacket.life = playerHealth;

        ThePacket thePacket = new ThePacket
        {
            playerPacket = playerPacket,
            // Reset the world packet, the connection should only be server-player for this
            worldPacket = localWorldPacket,
        };

        playerUpdate?.Invoke(thePacket);

        //Restart Action
        playerPacket.playerAction = PlayerAction.NONE;

        //Restart World Packet
        localWorldPacket = new WorldPacket
        {
            worldAction = WorldActions.NONE,
            worldPacketID = "",
            powerUpPosition = Vector3.zero,
        };
    }

    // POWER UPS FUNCTIONS -- START

    public void AddPowerUp(PowerUps powerUp)
    {
        activePowerUps |= powerUp;
    }
    public void RemovePowerUp(PowerUps powerUp)
    {
        activePowerUps &= ~powerUp;
    }
    public bool HasPowerUp(PowerUps powerUp)
    {
        return (activePowerUps & powerUp) == powerUp;
    }
    public void ClearPowerUps()
    {
        activePowerUps = PowerUps.None;
    }

    // POWER UPS FUNCTIONS -- END

    public void SetInitialValues(string PlayerID, string PlayerName)
    {
        playerPacket = new PlayerPacket();
        playerPacket.playerID = playerID = PlayerID;
        playerPacket.playerPosition = transform.position;
        playerPacket.playerRotation = transform.rotation;
        playerPacket.playerName = playerName = playerNameTXT.text = PlayerName;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PowerUp"))
        {
            localWorldPacket = new WorldPacket
            {
                worldAction = WorldActions.DESTROY,
                worldPacketID = other.name,
            };
            switch (other.GetComponentInParent<PowerUpBehaviour>()?.GetPowerUp())
            {
                case PowerUps.SHIELD:
                    playerPacket.playerAction = PlayerAction.SHIELD;
                    break;
                case PowerUps.TRIPLE_SHOT:
                    playerPacket.playerAction = PlayerAction.TRIPLE_SHOT;
                    break;
                case PowerUps.EXPLOTION_BULLETS:
                    playerPacket.playerAction = PlayerAction.EXPLOTION_BULLETS;
                    break;
                case PowerUps.BOUNCING_BULLET:
                    playerPacket.playerAction = PlayerAction.MORE_BOUNCING;
                    break;
                default:
                    break;
            }
        }
    }
}
