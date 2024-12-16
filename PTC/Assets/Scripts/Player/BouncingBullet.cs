using UnityEngine;

public class BouncingBullet : MonoBehaviour
{
    public float speed = 10f; // Speed of the bullet
    public int damage = 1; // Damage of the bullet
    [Space]
    public GameObject explotionPref;

    private int maxBounces = 3; // Maximum number of bounces allowed
    private int currentBounces = 0; // Counter for bounces
    private Vector3 moveDirection; // Current movement direction

    private Rigidbody rb; // Reference to Rigidbody

    private GameObject myCreator;
    private PowerUps powerUps;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Vector3 velocity = moveDirection * speed;
        velocity.y = transform.position.y;
        rb.velocity = velocity; // Set the initial velocity
    }

    public void GetAllValues(GameObject myCreator, Vector3 moveDirection, int maxBounces, int damage, PowerUps powerUps)
    {
        this.damage = damage;
        this.myCreator = myCreator;
        this.maxBounces = maxBounces;
        this.moveDirection = moveDirection;
        this.powerUps = powerUps;
    }

    void OnCollisionEnter(Collision collision)
    {
        //Check collision with other players
        if (collision.collider.CompareTag("Player") && collision.gameObject != myCreator)
        {
            collision.gameObject.GetComponent<PlayerScript>().BulletHit();

            //TODO: power up de explosion
            if (HasPowerUp(PowerUps.EXPLOTION_BULLETS))
                Destroy(Instantiate(explotionPref, transform.position, Quaternion.identity), 1f);

            Destroy(gameObject); // Destroy bullet after max bounces
            return;
        }

        // Check if the bullet has exceeded the allowed number of bounces
        if (currentBounces >= maxBounces)
        {
            //TODO: power up de explosion
            if (HasPowerUp(PowerUps.EXPLOTION_BULLETS))
                Destroy(Instantiate(explotionPref, transform.position, Quaternion.identity), 1f);

            Destroy(gameObject); // Destroy bullet after max bounces
            return;
        }

        // Calculate reflection direction using the collision normal
        Vector3 reflectDirection = Vector3.Reflect(moveDirection, collision.contacts[0].normal);
        moveDirection = reflectDirection; // Update the movement direction
        rb.velocity = reflectDirection * speed; // Apply the new velocity

        currentBounces++; // Increment bounce count
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, Vector3.zero) > 100f)
        {
            Destroy(gameObject);
        }
    }

    public bool HasPowerUp(PowerUps powerUp)
    {
        return (powerUps & powerUp) == powerUp;
    }
}
