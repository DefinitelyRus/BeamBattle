using UnityEngine;

/// <summary>
/// No, this was not written using ChatGPT--why do you think it works?
/// Author: DefinitelyRus
/// </summary>
public class Player : MonoBehaviour
{
	#region Physics

	//Forward/Backward Movement
	[Header("Physics")]
	[Range(0.0f, 10000.0f)]
	public float ForwardAcceleration = 2500;

	[Range(0.0f, 10000.0f)]
	public float MaxForwardSpeed = 25;

	[Range(0.0f, 10000.0f)]
	public float ReverseAcceleration = 1000;

	[Range(0.0f, 10000.0f)]
	public float MaxReverseSpeed = 10;

	//Turning
	[Range(0.0f, 1000.0f)]
	public float TurnRate = 180;

	//Laser Beam
	[Range(0.0f, 100000.0f)]
	public float RecoilForce = 50;

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

	private Rigidbody2D body;

	#endregion

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        body = GetComponent<Rigidbody2D>();
	}

    // Update is called once per frame
    void Update()
    {
		//Debug.Log($"Velocity: {body.linearVelocity.magnitude}");

		//Fire gun
		if (Input.GetKeyDown(FireKey) && RemainingGunCooldown == 0) {
			Debug.Log("[Player] Firing gun!");
			RemainingGunCooldown = GunCooldown;
		}

		//Gun still cooling down
		else if (Input.GetKeyDown(FireKey) && RemainingGunCooldown > 0) {
			Debug.Log($"[Player] Gun cooling down: {RemainingGunCooldown:F2}s");
		}

		//Spool up laser beam
		if (Input.GetKey(FireKey) && RemainingLaserCooldown == 0) {
			Debug.Log($"[Player] {RemainingHoldDuration:F2}s / {HoldDuration}s");
			RemainingHoldDuration += Time.deltaTime;
		}

		//Fire laser beam
		else if (Input.GetKeyUp(FireKey) && RemainingHoldDuration >= HoldDuration) {
			Debug.Log("[Player] Firing laser beam!");

			//Fire laser beam

			body.AddForce(-RecoilForce * Time.deltaTime * transform.up, ForceMode2D.Impulse);

			RemainingLaserCooldown = LaserCooldown;
			RemainingHoldDuration = 0;
		}

		//Did not hold long enough to fire
		else if (RemainingHoldDuration < HoldDuration) {
			RemainingHoldDuration = 0;
		}

		//Laser beam still cooling down
		else if (Input.GetKeyDown(FireKey) && RemainingLaserCooldown > 0) {
			Debug.Log($"[Player] Laser beam cooling down: {RemainingLaserCooldown:F2}s");
		}

		//Update cooldowns
		RemainingGunCooldown = Mathf.Max(RemainingGunCooldown - Time.deltaTime, 0);
		RemainingLaserCooldown = Mathf.Max(RemainingLaserCooldown - Time.deltaTime, 0);
	}

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
}
