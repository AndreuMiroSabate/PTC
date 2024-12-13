using UnityEngine;

public class BouncingBullet : MonoBehaviour
{
    public float speed = 10f; // Speed of the bullet
    public int damage = 1; // Damage of the bullet
    private int maxBounces = 3; // Maximum number of bounces allowed
    private int currentBounces = 0; // Counter for bounces
    private Vector3 moveDirection; // Current movement direction

    private Rigidbody rb; // Reference to Rigidbody

    private GameObject myCreator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        moveDirection = transform.forward; // Initial direction
        rb.velocity = moveDirection * speed; // Set the initial velocity
    }

    public void GetAllValues(GameObject myCreator, Vector3 moveDirection, int maxBounces, int damage)
    {
        this.damage = damage;
        this.myCreator = myCreator;
        this.maxBounces = maxBounces;
        this.moveDirection = moveDirection;
    }

    void OnCollisionEnter(Collision collision)
    {
        //Check collision with other players
        if (collision.collider.CompareTag("Player") && collision.gameObject != myCreator)
        {
            collision.gameObject.GetComponent<PlayerScript>().BulletHit();
            Destroy(gameObject); // Destroy bullet after max bounces
            return;
        }

        // Check if the bullet has exceeded the allowed number of bounces
        if (currentBounces >= maxBounces)
        {
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
        // Optional: Destroy bullet if it moves too far (to prevent infinite movement)
        if (Vector3.Distance(transform.position, Vector3.zero) > 1000f)
        {
            Destroy(gameObject);
        }
    }
}
