using UnityEngine;

public class Missile : MonoBehaviour
{
	#region Game Objects and Components

	[Header("Game Objects and Components")]
	public GameObject Target;

	public GameObject Explosion;

	public Rigidbody2D Body;

	public CircleCollider2D Collider;

	private Player player;

	#endregion

	#region Values

	public float Acceleration = 10;

	public float MaxSpeed = 1;

	public float TurnRate = 360;

	#endregion

	void SelfDestruct() {
		if (player != null) player.MissileWarningSFX.Stop();

		//Make big boom!
		GameObject boom = Instantiate(Explosion, transform.position, transform.rotation);
		boom.GetComponent<Explosion>().ExplosionSize = 3;

		Destroy(gameObject);
	}

	#region Unity

	void Start() {
		//Destroy self if there's nothing to target.
		if (Target == null) {
			SelfDestruct();
			return;
		}

        Body = GetComponent<Rigidbody2D>();
		Collider = GetComponent<CircleCollider2D>();

		//If the target is a player...
		player = Target.GetComponent<Player>();
		if (player != null && !player.MissileWarningSFX.isPlaying) player.MissileWarningSFX.Play();
	}

    void FixedUpdate() {
		//Destroy self if there's nothing to target.
		if (Target == null) {
			SelfDestruct();
			return;
		}

		// Rotate towards the target
		Vector2 direction = (Target.transform.position - transform.position).normalized;
		float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		float currentAngle = transform.rotation.eulerAngles.z;
		float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, TurnRate * Time.fixedDeltaTime);
		transform.rotation = Quaternion.Euler(0, 0, newAngle - 90);

		//Accelerate towards the target
		Body.AddForce(Acceleration * Time.fixedDeltaTime * transform.up, ForceMode2D.Force);
		if (Body.linearVelocity.magnitude > MaxSpeed) Body.linearVelocity = Body.linearVelocity.normalized * MaxSpeed;
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		if (collision == null) return;

		//If collided with the target, which is a player...
		if (collision.gameObject == Target && player != null) {
			player.Kill();
			SelfDestruct();
		}

		//If collided with the target, which is not a player...
		else if (collision.gameObject == Target && player == null) {
			Destroy(Target);
			SelfDestruct();
		}
	}

	#endregion
}
