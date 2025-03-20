using UnityEngine;

public class Explosion : MonoBehaviour
{
	#region Values

	/// <summary>
	/// The lifespan of the Explosion in seconds.
	/// </summary>
	[Header("Values")]
	public float Lifespan;

	/// <summary>
	/// The remaining lifespan of the Explosion in seconds.
	/// </summary>
	private float LifespanRemaining;

	/// <summary>
	/// The size of the Explosion.
	/// </summary>
	[Range(0, 3)]
	public int ExplosionSize = 0;

	#endregion

	#region Animations and SFX

	/// <summary>
	/// The animator of the Explosion.
	/// </summary>
	[Header("Animations and SFX")]
	public Animator explosionAnimator;

	/// <summary>
	/// The small explosion sound effect.
	/// </summary>
	public AudioSource smallExplosionSFX;

	/// <summary>
	/// The medium explosion sound effect.
	/// </summary>
	public AudioSource mediumExplosionSFX;

	/// <summary>
	/// The large explosion sound effect.
	/// </summary>
	public AudioSource largeExplosionSFX;

	#endregion

	#region Methods

	/// <summary>
	/// Spawns an explosion object but does nothing.
	/// </summary>
	public void HideExplosion() {
		explosionAnimator.SetInteger("Size", 0);
	}

	private void Start() {
		LifespanRemaining = Lifespan;

		explosionAnimator.SetInteger("Size", ExplosionSize);

		switch (ExplosionSize) {

			//None
			case 0:
				Debug.LogWarning("[Explosion] No Explosion size specified.");
				break;

			//Small
			case 1:
				transform.localScale = new Vector3(5, 5, 5);
				smallExplosionSFX.Play();
				break;

			//Medium
			case 2:
				transform.localScale = new Vector3(10, 10, 10);
				mediumExplosionSFX.Play();
				break;

			//Large
			case 3:
				transform.localScale = new Vector3(20, 20, 20);
				largeExplosionSFX.Play();
				break;
		}
	}

	void Update()
    {
		LifespanRemaining -= Time.deltaTime;

		if (LifespanRemaining <= 0) {
			Destroy(gameObject);
		}
	}

	#endregion
}
