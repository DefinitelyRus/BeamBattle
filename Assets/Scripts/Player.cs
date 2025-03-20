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
	/// The laser guide sprite that indicates the deltaGap of the laser beam.
	/// </summary>
	public SpriteRenderer LaserGuide;

	/// <summary>
	/// The sprite renderer of the player.
	/// </summary>
	public SpriteRenderer PlayerSprite;

	/// <summary>
	/// The animator for the player's animations.
	/// <br/><br/>
	/// The player catches on fire on their last hitpoint--that's
	/// the only time an explosionAnimation is needed.
	/// </summary>
	public Animator PlayerAnimator;

	/// <summary>
	/// The animator for the thrust explosionAnimation.
	/// </summary>
	public Animator ThrustAnimator;

	/// <summary>
	/// The Explosion prefab to instantiate when the player is damaged or killed.
	/// </summary>
	public GameObject Explosion;

	/// <summary>
	/// The laser beam prefab containing the mask and sprite.
	/// </summary>
	public GameObject LaserSprite;

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

	/// <summary>
	/// Whether to allow firing the gun.
	/// </summary>
	private bool allowGun = true;

	/// <summary>
	/// Fires the gun.
	/// </summary>
	public void FireGun() {

		//Spawn bullet
		GameObject bullet = Instantiate(BulletPrefab, transform.position, transform.rotation);
		bullet.GetComponent<Bullet>().Shooter = this;

		CancelFireSFX.Stop();
		GunSFX.Play();

		RemainingGunCooldown = GunCooldown;
		//The hold duration reset is put here to avoid firing the gun after the laser.
	}

	/// <summary>
	/// Fires the laser.
	/// </summary>
	public void FireLaser() {

		//Restore the player's speed
		MaxForwardSpeed = MaxForwardSpeedCopy;
		TurnRate = TurnRateCopy;

		//Disable the laser guide
		LaserGuide.color = new Color(LaserGuide.color.r, LaserGuide.color.g, LaserGuide.color.b, 0);

		LaserSFX.Play();
		ChargingSFX.Stop();

		#region Raycasts and Effects

		//Perform a raycast scan
		Vector2 laserDirection = transform.up;
		RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, laserDirection, LaserRange);

		//For every object hit by the laser beam...
		foreach (RaycastHit2D hit in hits) {

			//If the object is not this player...
			if (hit.collider.gameObject != gameObject) {
				Debug.Log($"[{gameObject.name}] Hit {hit.collider.gameObject.name} at {hit.distance:F2} units.");

				//Explodes where the laser hits
				GameObject boom = Instantiate(Explosion, hit.point, transform.rotation);
				boom.GetComponent<Explosion>().ExplosionSize = 3;

				//Spawns the laser sprite between the shooter and the hit object
				Vector2 deltaGap = Vector2.Lerp(transform.position, hit.centroid, 0.5f);
				GameObject laser = Instantiate(LaserSprite, deltaGap, transform.rotation);
				laser.transform.localScale = new Vector3(1, hit.distance, 1);

				//If the object is a player, kill it.
				if (hit.collider.gameObject.GetComponent<Player>() is Player player) player.Kill();

				break;
			}

			//If the object is this player, ignore it.
			else continue;
		}

		//If the laser beam reaches its max range...
		if (hits.Length == 1) {
			Debug.Log($"[{gameObject.name}] Laser beam reached max range.");

			//Spawns the laser sprite between the shooter and the laser's max range.
			Vector2 deltaGap = Vector2.Lerp(transform.position, transform.position + transform.up * LaserRange, 0.5f);
			GameObject laser = Instantiate(LaserSprite, deltaGap, transform.rotation);
			laser.transform.localScale = new Vector3(1, LaserRange, 1);
		}

		//Draws the laser beam in the Scene view for debugging.
		Debug.DrawRay(transform.position, laserDirection * LaserRange, Color.red, 10f);

		#endregion

		//Recoil
		Body.AddForce(-RecoilForce * Time.deltaTime * transform.up, ForceMode2D.Impulse);

		RemainingLaserCooldown = LaserCooldown;
		RemainingHoldDuration = 0;
		allowGun = false;
	}

	/// <summary>
	/// Charges the laser.
	/// </summary>
	public void ChargeLaser() {
		RemainingHoldDuration += Time.deltaTime;

		//Slows down the player while spooling up.
		MaxForwardSpeed = Mathf.Lerp(MaxForwardSpeed, MaxSpeedWhileSpooling, RemainingHoldDuration / HoldDuration);
		TurnRate = Mathf.Lerp(TurnRate, TurnRateWhileSpooling, RemainingHoldDuration / HoldDuration);

		//Fades in the laser guide sprite while spooling up.
		LaserGuide.color = new Color(LaserGuide.color.r, LaserGuide.color.g, LaserGuide.color.b, Mathf.Lerp(0, 1, RemainingHoldDuration / HoldDuration));

		if (!ChargingSFX.isPlaying) ChargingSFX.PlayDelayed(0.15f);
		//Delayed because it's audible when firing the guns.
	}

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

		//Update the player's health sprite/explosionAnimation
		PlayerAnimator.SetInteger("Health", Hitpoints);

		//Boom!
		GameObject boom = Instantiate(Explosion, transform.position, transform.rotation);
		boom.GetComponent<Explosion>().ExplosionSize = 2;

		if (Hitpoints == 0) Kill();
		//Set to exactly 0 only so Kill() won't be activated more than once.
	}

	/// <summary>
	/// Disables player movement and initiates the death sequence.
	/// </summary>
	public void Kill() {
		Debug.Log($"[{gameObject.name}] was killed!");

		//Just for animations--the player is already dead.
		Hitpoints = 0;
		PlayerAnimator.SetInteger("Health", Hitpoints);
		ThrustAnimator.SetBool("IsThrusting", false);
		ThrustSFX.Stop();

		//Disables player movement and allows drifting
		Body.linearDamping = 0.5f;
		Body.angularDamping = 10000;
		ForwardAcceleration = 0;
		ReverseAcceleration = 0;
		TurnRate = 0;
		allowGun = false;
		//It's fine to leave these values as is because a new player instance will be created when the player respawns.

		StartCoroutine(ExecuteDeathSequence(1));
	}

	/// <summary>
	/// Executes the death sequence for the player.
	/// </summary>
	/// <param name="initialDelay">How many seconds to wait before exploding.</param>
	private IEnumerator ExecuteDeathSequence(float initialDelay = 1) {
		yield return new WaitForSeconds(initialDelay);

		//Boom!
		GameObject boom = Instantiate(Explosion, transform.position, transform.rotation);
		boom.GetComponent<Explosion>().ExplosionSize = 3;

		PlayerSprite.enabled = false;

		yield return new WaitForSeconds(2);

		Destroy(gameObject);
	}

	#endregion

	#region Audio

	[Header("Audio")]
	public AudioSource ThrustSFX;

	public AudioSource GunSFX;

	public AudioSource ChargingSFX;

	public AudioSource LaserSFX;

	public AudioSource MissileWarningSFX;

	public AudioSource OnCooldownSFX;

	public AudioSource CancelFireSFX;

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
		PlayerSprite = GetComponent<SpriteRenderer>();
		PlayerAnimator = GetComponent<Animator>();
		ThrustAnimator = transform.GetChild(1).GetComponent<Animator>();

		MaxForwardSpeedCopy = MaxForwardSpeed;
		TurnRateCopy = TurnRate;
		LaserGuide.enabled = true;
		LaserGuide.color = new Color(LaserGuide.color.r, LaserGuide.color.g, LaserGuide.color.b, 0);
	}

    void Update()
    {
		#region Weapons
		//Fire gun
		if (Input.GetKeyUp(FireKey) && RemainingGunCooldown == 0 && RemainingHoldDuration < 0.2 && allowGun) FireGun();

		//Re-enable gun if the player isn't dead.
		else if (Input.GetKeyUp(FireKey) && Hitpoints > 0) allowGun = true;

		//Spool up laser beam if the player isn't dead.
		if (Input.GetKey(FireKey) && RemainingLaserCooldown == 0 && Hitpoints > 0) ChargeLaser();

		//Did not hold long enough to fire
		else if (Input.GetKeyUp(FireKey) && RemainingHoldDuration < HoldDuration) {

			MaxForwardSpeed = MaxForwardSpeedCopy;
			TurnRate = TurnRateCopy;

			//Disables the laser guide sprite when not spooling up.
			LaserGuide.color = new Color(LaserGuide.color.r, LaserGuide.color.g, LaserGuide.color.b, 0);

			ChargingSFX.Stop();

			if (RemainingHoldDuration > 0.2f) CancelFireSFX.Play();

			RemainingHoldDuration = 0;
		}

		//Fire laser beam
		if (RemainingHoldDuration >= HoldDuration) FireLaser();

		//Update cooldowns
		RemainingGunCooldown = Mathf.Max(RemainingGunCooldown - Time.deltaTime, 0);
		RemainingLaserCooldown = Mathf.Max(RemainingLaserCooldown - Time.deltaTime, 0);

		#endregion

		#region Thrusting Animations and SFX

		//Enable
		if (Input.GetKey(ForwardKey) && Hitpoints > 0) {
			ThrustAnimator.SetBool("IsThrusting", true);
			if (!ThrustSFX.isPlaying) ThrustSFX.Play();
		}

		//Disable
		else if (Input.GetKeyUp(ForwardKey) && Hitpoints > 0) {
			ThrustAnimator.SetBool("IsThrusting", false);
			ThrustSFX.Stop();
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
