using UnityEngine;

public class Obstacle : MonoBehaviour
{
    void Start()
    {
		//Spawn at a random size, rotation, and color
		transform.rotation = Random.rotation;
		transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360)));

		float scale = Random.Range(0.5f, 10);
		transform.localScale = new Vector3(scale, scale, scale);

		GetComponent<SpriteRenderer>().color = Random.ColorHSV(0, 1, 0, 0.1f, 0.8f, 1, 1, 1);
	}
}
