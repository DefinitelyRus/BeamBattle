using UnityEngine;

public class Bullet : MonoBehaviour {

	#region Values

	/// <summary>
	/// The speed of this bullet.
	/// </summary>
	[Header("Values")]
	[Range(0, 500)]
	public float Speed = 30; //Units per second

	/// <summary>
	/// How long this bullet will last (in seconds) before being destroyed.
	/// </summary>
	[Range(0, 10)]
	public float BaseLifespan = 5;

	/// <summary>
	/// The remaining lifespan of this bullet.
	/// </summary>
	private float LifespanRemaining;

	#endregion

	#region Game Objects and Components

	/// <summary>
	/// The player that shot this bullet.
	/// </summary>
	[Header("Game Objects and Components")]
	public Player shooter;

	/// <summary>
	/// The rigidbody of this bullet.
	/// </summary>
	public Rigidbody2D body;

	/// <summary>
	/// The trigger collider of this bullet.
	/// </summary>
	public CircleCollider2D trigger;

	#endregion

	#region Methods

	/// <summary>
	/// Destroys this bullet.
	/// </summary>
	void DestroySelf(bool silent = false) {
		if (!silent) {
			//TODO: Insert small explosion animation here

			//TODO: Insert SFX here
		}

		Destroy(gameObject);
	}

	#endregion

	#region Unity

	void Start()
    {
		body = GetComponent<Rigidbody2D>();
		trigger = GetComponent<CircleCollider2D>();
		LifespanRemaining = BaseLifespan;
	}
    
    void Update()
    {
        LifespanRemaining -= Time.deltaTime;

		if (LifespanRemaining <= 0) DestroySelf(true); //Silently destroys self

		//Body.AddForce(Speed * Time.deltaTime * transform.up, ForceMode2D.Impulse);
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

		//If the other object is a player, hit the player and destroy the bullet.
		if (other.gameObject.GetComponent<Player>() is Player player) {
			Debug.Log("[Bullet] Hit player!");

			player.TakeHit();

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

	#endregion
}
