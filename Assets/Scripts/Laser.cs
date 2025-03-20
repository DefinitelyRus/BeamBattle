using UnityEngine;

public class Laser : MonoBehaviour
{
	public float Lifespan = 3.0f;

	private float LifespanRemaining;

	void Start()
    {
		LifespanRemaining = Lifespan;
	}

    void Update()
    {
		LifespanRemaining -= Time.deltaTime;

		//Makes the beam thinner as the lifespan decays.
		transform.localScale = new Vector3(LifespanRemaining / Lifespan, transform.localScale.y, transform.localScale.z);

		if (LifespanRemaining <= 0.0f) {
			Destroy(gameObject);
		}
	}
}
