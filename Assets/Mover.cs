using UnityEngine;

public class Mover : MonoBehaviour
{
    public Vector3 direction = Vector3.back; // Movement direction
    public float speed;
    public Jumper jumper; // Reference to the Jumper

    void Start()
    {
        speed = Random.Range(4f, 6f);
    }

    void Update()
    {
        transform.Translate(direction.normalized * speed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("wall"))
        {
            Debug.Log(gameObject.name + " collided with a wall!");
            if (jumper != null)
            {
                jumper.RewardForWallCollision();
            }
            else
            {
                Debug.LogError("Jumper reference not set in Mover.");
            }
            Destroy(gameObject);
        }
    }
}
