using UnityEngine;

public class Laser : MonoBehaviour
{
	public float Lifespan = 3.0f;

	private float LifespanRemaining;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		LifespanRemaining = Lifespan;
	}

    // Update is called once per frame
    void Update()
    {
		LifespanRemaining -= Time.deltaTime;

		transform.localScale = new Vector3(LifespanRemaining / Lifespan, transform.localScale.y, transform.localScale.z);

		if (LifespanRemaining <= 0.0f) {
			Destroy(gameObject);
		}
	}
}
