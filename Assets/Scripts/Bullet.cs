using UnityEngine;

public class Bullet : MonoBehaviour
{
	public Player shooter;

	[Range(0, 10000)]
	public float Speed = 50; //Units per second

	[Range(0, 10)]
	public float BaseLifespan = 5;

	private float LifespanRemaining;

	public Rigidbody2D body;

	public CircleCollider2D trigger;

	void Start()
    {
		body = GetComponent<Rigidbody2D>();
		trigger = GetComponent<CircleCollider2D>();
		LifespanRemaining = BaseLifespan;
	}
    
    void Update()
    {
        LifespanRemaining -= Time.deltaTime;

		if (LifespanRemaining <= 0) Destroy(gameObject);

		//body.AddForce(Speed * Time.deltaTime * transform.up, ForceMode2D.Impulse);
		body.linearVelocity = Speed * transform.up;
	}

	void OnTriggerEnter2D(Collider2D other) {
		//If the other object is the shooter, ignore it.
		if (other.gameObject == shooter.gameObject) return;

		//If the other object is neither a player nor a bullet, destroy the bullet.
		if (other.gameObject.GetComponent<Player>() is null && other.gameObject.GetComponent<Bullet>() is null) {
			Debug.Log("[Bullet] Impact!");
			DestroySelf();
		}

		//If the other object is a player, kill the player and destroy the bullet.
		if (other.gameObject.GetComponent<Player>() is Player player) {
			Debug.Log("[Bullet] Hit player!");

			player.Kill();

			DestroySelf();
		}

		//If the other object is a bullet, destroy both bullets.
		if (other.gameObject.GetComponent<Bullet>() is not null) {
			Debug.Log("[Bullet] Hit bullet!");

			DestroySelf();

			//Instantly destroys the other bullet to avoid duplicate effects.
			Destroy(other.gameObject);
		}
	}

	void DestroySelf() {

		//TODO: Insert animation here

		//TODO: Insert sound effect here

		Destroy(gameObject);
	}
}
