using UnityEngine;

public class LevelManager : MonoBehaviour
{
	#region Game Objects and Components

	[Header("Game Objects and Components")]
	public GameObject Player1Prefab;

	public GameObject Player1;

	public GameObject Player2Prefab;

	public GameObject Player2;

	public GameObject Spawnpoints;

	#endregion

	#region Scoring and Timers

	[Header("Scoring and Timers")]
	public int Player1Score = 0;

	public int Player2Score = 0;

	public int WinScore = 10;

	#endregion

	#region Music and SFX

	[Header("Music and SFX")]
	public AudioSource Music;

	private AudioClip[] MusicTracks;

	public AudioSource ScoreSFX;

	public AudioSource RespawnSFX;

	public AudioSource VictoryFX;

	#endregion

	private GameObject SpawnPlayer(GameObject prefab) {
		//Choose a random spawnpoint
		Vector2 spawnPosition = new Vector2(Random.Range(-10, 10), Random.Range(-10, 10));
		//TODO: Choose from a random node in the Spawnpoints object.

		//TODO: Add respawn animation at the spawn position

		//TODO: Add respawn SFX at the spawn position

		GameObject newPlayer = Instantiate(prefab);

		//Sets the player's position to the spawnpoint's position
		//TODO: There will be several spawn nodes. The player should spawn at a random one.
		//Rotates the player to a random angle
		newPlayer.transform.SetPositionAndRotation(spawnPosition, Quaternion.Euler(0, 0, Random.Range(0, 360)));

		return newPlayer;
	}

	void Start()
    {
		#region Music

		MusicTracks = Resources.LoadAll<AudioClip>("Audio/Music");

		if (MusicTracks.Length > 0) {
			AudioClip newTrack;

			do {
				//Randomize the track until it is different from the current track.
				newTrack = MusicTracks[Random.Range(0, MusicTracks.Length)];

				//If no music is playing, assign the new track.
				if (Music.clip == null) {
					Music.clip = newTrack;
					break;
				}
			}
			while (Music.clip == newTrack);

			Debug.Log($"[LevelManager] Now playing: {Music.clip.name}");
			Music.Play();
		}
		else {
			Debug.LogWarning("[LevelManager] No music tracks found.");
		}

		#endregion
	}

	void Update() {
		if (Player1 == null) {
			Player2Score++;

			//TODO: Add score SFX here

			//TODO: Update UI here

			Player1 = SpawnPlayer(Player1Prefab);
		}

		if (Player2 == null) {
			Player1Score++;

			//TODO: Add score SFX here

			//TODO: Update UI here

			Player2 = SpawnPlayer(Player2Prefab);
		}
	}
}
