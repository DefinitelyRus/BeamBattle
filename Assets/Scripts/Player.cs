using System.Collections;
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

	#region GameObjects and Components

	/// <summary>
	/// The bullet prefab to instantiate when firing the gun.
	/// </summary>
	[Header("GameObjects and Components")]
	public GameObject BulletPrefab;

	/// <summary>
	/// The Rigidbody2D component of the player.
	/// </summary>
	public Rigidbody2D Body;

	/// <summary>
	/// The laser guide sprite that indicates the direction of the laser beam.
	/// </summary>
	public SpriteRenderer LaserGuide;

	/// <summary>
	/// The sprite renderer of the player.
	/// </summary>
	public SpriteRenderer PlayerSprite;

	#endregion

	#region Weapons

	/// <summary>
	/// The range of the laser beam.
	/// </summary>
	[Header("Weapons & Hull")]
	public float LaserRange = 100;

	/// <summary>
	/// The cooldown time for the gun. This limits the firerate.
	/// </summary>
	[Range(0.0f, 5.0f)]
	public float GunCooldown = 0.25f;
	private float RemainingGunCooldown = 0;

	/// <summary>
	/// The cooldown time for the laser beam.
	/// </summary>
	[Range(0.0f, 60.0f)]
	public float LaserCooldown = 5;
	private float RemainingLaserCooldown = 0;

	/// <summary>
	/// The duration the player must hold the fire key to fire the laser.
	/// </summary>
	[Range(0.0f, 60.0f)]
	public float HoldDuration = 1;
	private float RemainingHoldDuration = 0;

	#endregion

	#region Hull

	/// <summary>
	/// The hitpoints of the player. When this reaches 0, the player is killed.
	/// <br/><br/>
	/// It's not accompanied by a "CurrentHitpoints" field because
	/// the player instance is destroyed entirely and replaced by a new one when killed.
	/// </summary>
	public int Hitpoints { get; private set; } = 3;

	/// <summary>
	/// Damages the player and kills them if hitpoints reach 0.
	/// </summary>
	public void TakeHit() {
		Hitpoints--;
		Debug.Log($"[{gameObject.name}] was hit! HP: {Hitpoints}");

		//TODO: Spawn an explosion animation as its own object

		//TODO: Add explosion SFX

		switch (Hitpoints) {
			case 2:
				//TODO: Apply damaged sprite
				break;
			case 1:
				//TODO: Apply heavily damaged sprite
				break;
			case 0:
				Kill();
				break;
		}
	}

	/// <summary>
	/// Disables player movement and initiates the death sequence.
	/// </summary>
	public void Kill() {
		Debug.Log($"[{gameObject.name}] was killed!");

		//Disables player movement and allows drifting
		Body.linearDamping = 0.5f;
		Body.angularDamping = 0.5f;
		ForwardAcceleration = 0;
		ReverseAcceleration = 0;
		TurnRate = 0;
		//It's fine to leave these values as is because a new player instance will be created when the player respawns.
		//I thought of also disabling firing weapons, but let's allow some martyrdom. :)

		StartCoroutine(ExecuteDeathSequence(1));
	}

	/// <summary>
	/// Executes the death sequence for the player.
	/// </summary>
	/// <param name="initialDelay">How many seconds to wait before exploding.</param>
	private IEnumerator ExecuteDeathSequence(float initialDelay = 1) {
		yield return new WaitForSeconds(initialDelay);

		//TODO: Add explosion SFX

		//TODO: Add large explosion animation

		PlayerSprite.enabled = false;

		yield return new WaitForSeconds(2);

		Destroy(gameObject);

		//TODO: In LevelManager, automatically spawn a new player instance after the gameObject is destroyed.
	}

	#endregion

	#region Keybinds

	[Header("Keybinds")]
	public KeyCode ForwardKey = KeyCode.W;

	public KeyCode ReverseKey = KeyCode.S;

	public KeyCode TurnLeftKey = KeyCode.A;

	public KeyCode TurnRightKey = KeyCode.D;

	public KeyCode FireKey = KeyCode.Space;

	#endregion

	#region Unity

	void Start()
    {
        Body = GetComponent<Rigidbody2D>();
		LaserGuide = transform.GetChild(0).GetComponent<SpriteRenderer>();

		MaxForwardSpeedCopy = MaxForwardSpeed;
		TurnRateCopy = TurnRate;
		LaserGuide.color = new Color(1, 1, 1, 0);
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

			//Fades in the laser guide sprite while spooling up.
			LaserGuide.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, RemainingHoldDuration / HoldDuration));

			//TODO: Enable spool-up SFX
		}

		//Did not hold long enough to fire
		else if (RemainingHoldDuration < HoldDuration) {
			RemainingHoldDuration = 0;

			MaxForwardSpeed = MaxForwardSpeedCopy;
			TurnRate = TurnRateCopy;

			//Disables the laser guide sprite when not spooling up.
			LaserGuide.color = new Color(1, 1, 1, 0);

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

			//Disables the laser guide sprite after firing the laser.
			LaserGuide.color = new Color(1, 1, 1, 0);

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

					//TODO: Render the laser sprite at `hit.centroid`

					//If the object is a player, kill it.
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
			Body.AddForce(-RecoilForce * Time.deltaTime * transform.up, ForceMode2D.Impulse);

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
		 * Animations are handled separately from physics calculations.
		 * It's critical to minimize non-essential calculations
		 * in FixedUpdate to avoid performance issues.
		 */

		#endregion
	}

	//Reserved for physics calculations
	void FixedUpdate() {
		//Forward
		if (Input.GetKey(ForwardKey)) {
			Body.AddForce(ForwardAcceleration * Time.fixedDeltaTime * transform.up, ForceMode2D.Force);
			if (Body.linearVelocity.magnitude > MaxForwardSpeed) Body.linearVelocity = Body.linearVelocity.normalized * MaxForwardSpeed;
		}

		//Reverse
		if (Input.GetKey(ReverseKey)) {
			Body.AddForce(-ReverseAcceleration * Time.fixedDeltaTime * transform.up, ForceMode2D.Force);
			if (Body.linearVelocity.magnitude > MaxReverseSpeed) Body.linearVelocity = Body.linearVelocity.normalized * MaxReverseSpeed;
		}

		//Turn Left
		if (Input.GetKey(TurnLeftKey)) {
			Body.AddTorque(TurnRate * Time.fixedDeltaTime);
		}

		//Turn Right
		if (Input.GetKey(TurnRightKey)) {
			Body.AddTorque(-TurnRate * Time.fixedDeltaTime);
		}
	}

	#endregion
}
