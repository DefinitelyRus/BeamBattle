using UnityEngine;

/// <summary>
/// No, this was not written using ChatGPT--why do you think it works?
/// Author: DefinitelyRus
/// </summary>
public class Player : MonoBehaviour
{
	#region Physics

	/// <summary>
	/// The acceleration applied when moving forwards.
	/// </summary>
	[Header("Physics")]
	[Range(0, 10000)]
	public float ForwardAcceleration = 1500;

	/// <summary>
	/// The maximum speed the player can move forwards.
	/// <br/><br/>
	/// This is calculated as the magnitude of the player's velocity vector.
	/// The velocity is clamped to this value.
	/// </summary>
	[Range(0, 100)]
	public float MaxForwardSpeed = 15;

	/// <summary>
	/// The maximum speed the player can move forwards while spooling the laser.
	/// </summary>
	[Range(0, 10000)]
	public float MaxSpeedWhileSpooling = 300;

	/// <summary>
	/// A copy of the maximum forward speed to restore it after spooling the laser.
	/// </summary>
	private float MaxForwardSpeedCopy;

	/// <summary>
	/// The acceleration applied when moving backwards.
	/// </summary>
	[Range(0, 10000)]
	public float ReverseAcceleration = 500;

	/// <summary>
	/// The maximum speed the player can move backwards.
	/// <br/><br/>
	/// This is calculated as the magnitude of the player's velocity vector.
	/// The velocity is clamped to this value.
	/// </summary>
	[Range(0, 10000)]
	public float MaxReverseSpeed = 5;

	/// <summary>
	/// How fast the player can turn in degrees per second.
	/// </summary>
	[Range(0, 1000)]
	public float TurnRate = 180;

	/// <summary>
	/// The turn rate while spooling the laser.
	/// </summary>
	[Range(0, 360)]
	public float TurnRateWhileSpooling = 30;

	/// <summary>
	/// A copy of the turn rate to restore it after spooling the laser.
	/// </summary>
	private float TurnRateCopy;

	/// <summary>
	/// The force applied to the player when firing the laser.
	/// </summary>
	[Range(0, 10000)]
	public float RecoilForce = 5000;

	#endregion

	#region

	[Header("Timers and Cooldowns")]
	[Range(0.0f, 5.0f)]
	public float GunCooldown = 0.25f;
	private float RemainingGunCooldown = 0;

	[Range(0.0f, 60.0f)]
	public float LaserCooldown = 5;
	private float RemainingLaserCooldown = 0;

	[Range(0.0f, 60.0f)]
	public float HoldDuration = 1;
	private float RemainingHoldDuration = 0;

	#endregion

	#region Keybinds

	[Header("Keybinds")]
	public KeyCode ForwardKey = KeyCode.W;

	public KeyCode ReverseKey = KeyCode.S;

	public KeyCode TurnLeftKey = KeyCode.A;

	public KeyCode TurnRightKey = KeyCode.D;

	public KeyCode FireKey = KeyCode.Space;

	#endregion

	#region GameObjects and Components

	[Header("GameObjects and Components")]
	public GameObject BulletPrefab;

	public Rigidbody2D body;

	#endregion

	#region Weapons and Hull

	[Header("Weapons")]
	public float LaserRange = 100;

	public void Kill() {
		Debug.Log($"[{gameObject.name}] Killed!");

		//Disables player movement and allows drifting
		body.linearDamping = 0.5f;
		body.angularDamping = 0.5f;
		ForwardAcceleration = 0;
		ReverseAcceleration = 0;
		TurnRate = 0;
		//It's fine to leave these values as is because a new player instance will be created when the player respawns.
		//I thought of also disabling firing weapons, but let's allow some martyrdom. :)

		//TODO: Delay by 1s

		//TODO: Add explosion SFX

		//TODO: Add explosion animation

		//TODO: Disable player sprite

		//TODO: In SpawnManager, automatically spawn a new player instance after the gameObject is destroyed.

		//TODO: Delay by 2s

		//Destroy(gameObject);
	}

	#endregion

	#region Unity

	void Start()
    {
        body = GetComponent<Rigidbody2D>();

		MaxForwardSpeedCopy = MaxForwardSpeed;
		TurnRateCopy = TurnRate;
	}

    void Update()
    {
		#region Weapons
		//Fire gun
		if (Input.GetKeyDown(FireKey) && RemainingGunCooldown == 0) {
			Debug.Log($"[{gameObject.name}] Firing gun!");

			//Spawn bullet
			GameObject bullet = Instantiate(BulletPrefab, transform.position, transform.rotation);
			bullet.GetComponent<Bullet>().shooter = this;

			RemainingGunCooldown = GunCooldown;
		}

		//Spool up laser beam
		if (Input.GetKey(FireKey) && RemainingLaserCooldown == 0) {
			RemainingHoldDuration += Time.deltaTime;

			//Slows down the player while spooling up.
			MaxForwardSpeed = Mathf.Lerp(MaxForwardSpeed, MaxSpeedWhileSpooling, RemainingHoldDuration / HoldDuration);
			TurnRate = Mathf.Lerp(TurnRate, TurnRateWhileSpooling, RemainingHoldDuration / HoldDuration);

			//TODO: Enable spool-up SFX
		}

		//Did not hold long enough to fire
		else if (RemainingHoldDuration < HoldDuration) {
			RemainingHoldDuration = 0;

			MaxForwardSpeed = MaxForwardSpeedCopy;
			TurnRate = TurnRateCopy;

			//TODO: Disable and reset spool-up SFX

			//TODO: Play SFX indicating that it was not held long enough to fire the laser.
		}

		//Laser beam still cooling down
		else if (Input.GetKeyDown(FireKey) && RemainingLaserCooldown > 0) {
			Debug.Log($"[{gameObject.name}] Laser beam cooling down: {RemainingLaserCooldown:F2}s");

			//TODO: Play SFX indicating the laser is still cooling down.
		}

		//Fire laser beam
		if (RemainingHoldDuration >= HoldDuration) {
			Debug.Log($"[{gameObject.name}] Firing laser beam!");

			//Restores the player's speed after firing the laser.
			MaxForwardSpeed = MaxForwardSpeedCopy;
			TurnRate = TurnRateCopy;

			#region SFX

			//TODO: Add laser beam firing SFX

			//TODO: Disable and reset spool-up SFX

			#endregion

			#region Raycasts and Effects

			//Perform a raycast scan
			Vector2 laserDirection = transform.up;
			RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, laserDirection, LaserRange);

			//For every object hit by the laser beam...
			foreach (RaycastHit2D hit in hits) {

				//If the object is not this player...
				if (hit.collider.gameObject != gameObject) {
					Debug.Log($"[{gameObject.name}] Hit {hit.collider.gameObject.name} at {hit.distance:F2} units.");

					//TODO: Explode at hit.point if hit.collider is not a player

					//TODO: Render the laser sprite at hit.centroid

					if (hit.collider.gameObject.GetComponent<Player>() is Player player) player.Kill();

					break;
				}

				//If the object is this player, ignore it.
				else continue;
			}

			//Draws the laser beam in the Scene view for debugging.
			Debug.DrawRay(transform.position, laserDirection * LaserRange, Color.red, 10f);

			#endregion

			//Applies recoil force to the player
			body.AddForce(-RecoilForce * Time.deltaTime * transform.up, ForceMode2D.Impulse);

			//Reset cooldowns and timers
			RemainingLaserCooldown = LaserCooldown;
			RemainingHoldDuration = 0;
		}

		//Update cooldowns
		RemainingGunCooldown = Mathf.Max(RemainingGunCooldown - Time.deltaTime, 0);
		RemainingLaserCooldown = Mathf.Max(RemainingLaserCooldown - Time.deltaTime, 0);

		#endregion

		#region Movement Animations and SFX

		if (Input.GetKey(ForwardKey)) {
			//TODO: Add thrust animation

			//TODO: Add thrust SFX
		}
		/*
		 * Animations are handles separately from physics calculations.
		 * It's critical to minimize non-essential calculations
		 * in FixedUpdate to avoid performance issues.
		 */

		#endregion
	}

	//Reserved for physics calculations
	void FixedUpdate() {
		//Forward
		if (Input.GetKey(ForwardKey)) {
			body.AddForce(ForwardAcceleration * Time.fixedDeltaTime * transform.up, ForceMode2D.Force);
			if (body.linearVelocity.magnitude > MaxForwardSpeed) body.linearVelocity = body.linearVelocity.normalized * MaxForwardSpeed;
		}

		//Reverse
		if (Input.GetKey(ReverseKey)) {
			body.AddForce(-ReverseAcceleration * Time.fixedDeltaTime * transform.up, ForceMode2D.Force);
			if (body.linearVelocity.magnitude > MaxReverseSpeed) body.linearVelocity = body.linearVelocity.normalized * MaxReverseSpeed;
		}

		//Turn Left
		if (Input.GetKey(TurnLeftKey)) {
			body.AddTorque(TurnRate * Time.fixedDeltaTime);
		}

		//Turn Right
		if (Input.GetKey(TurnRightKey)) {
			body.AddTorque(-TurnRate * Time.fixedDeltaTime);
		}
	}

	#endregion
}
