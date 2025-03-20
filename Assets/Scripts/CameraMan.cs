using UnityEngine;

public class CameraMan : MonoBehaviour
{
	#region Game Objects and Components

	[Header("Game Objects and Components")]
	public LevelManager LevelManager;

	public Camera SonyZV1;

	private GameObject player1;

	private GameObject player2;

	private Vector2 player1Pos;

	private Vector2 player2Pos;

	#endregion

	#region Values

	[Header("Values")]
	public float MaxDistance = 80;

	public float MinSize = 8;

	public float MaxSize = 50;

	private float size = 50;

	public float LerpSpeed = 5;

	#endregion

	void Start()
    {
		LevelManager = GameObject.Find("Level Manager").GetComponent<LevelManager>();
		SonyZV1 = GetComponent<Camera>();
		player1 = LevelManager.Player1;
		player2 = LevelManager.Player2;
    }

    void Update()
    {
		//Keeps track of the player positions
		if (player1 != null) player1Pos = player1.transform.position;
		else player1 = LevelManager.Player1;

		if (player2 != null) player2Pos = player2.transform.position;
		else player2 = LevelManager.Player2;

		//Moves the camera between both players
		Vector2 center = (player1Pos + player2Pos) / 2;
		Vector3 position = new Vector3(center.x, center.y, transform.position.z);
		transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * LerpSpeed);

		//Changes camera zoom based on player distance
		float distance = Vector2.Distance(player1Pos, player2Pos);
		size = Mathf.Clamp(distance / MaxDistance * MaxSize, MinSize, MaxSize);
		SonyZV1.orthographicSize = Mathf.Lerp(SonyZV1.orthographicSize, size, Time.deltaTime * LerpSpeed);
	}
}
