using UnityEngine;
using UnityEngine.SceneManagement;

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

	public AudioSource VictorySFX;

	#endregion

	private GameObject SpawnPlayer(GameObject prefab) {
		//Choose a random spawnpoint
		Vector2 spawnPosition = new Vector2(Random.Range(-10, 10), Random.Range(-10, 10));
		//TODO: Choose from a random node in the Spawnpoints object.

		//TODO: Add respawn explosionAnimation at the spawn position

		//TODO: Add respawn SFX at the spawn position

		GameObject newPlayer = Instantiate(prefab);

		//Sets the player's position to the spawnpoint's position
		//TODO: There will be several spawn nodes. The player should spawn at a random one.
		//Rotates the player to a random angle
		newPlayer.transform.SetPositionAndRotation(spawnPosition, Quaternion.Euler(0, 0, Random.Range(0, 360)));

		return newPlayer;
	}

	private void PlayMusic() {
		if (!Music.enabled) return;

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

			Debug.Log($"[LevelManager] Now playing: Ben Prunty - {Music.clip.name}");

			Music.Play();
		} else {
			Debug.LogWarning("[LevelManager] No music tracks found.");
		}
	}

	private void Win(GameObject winner) {
		gameOver = true;

		//TODO: Add victory screen here
		//Use winner.name

		Music.Stop();

		//Disables the music component so that it doesn't play again
		Music.enabled = false; 

		VictorySFX.Play();
	}

	private void Restart() {
		Debug.Log("[LevelManager] Restarting level...");

		gameOver = false;

		SceneManager.LoadScene(SceneManager.GetActiveScene().name);

		Music.enabled = true; //Re-enables the music component after the level is reloaded.

		Music.Stop(); //Stops the music when the level is reloaded.
					  //When the level is reloaded, new music will play.
	}

	void Start()
    {
		MusicTracks = Resources.LoadAll<AudioClip>("Music");
	}

	void Update() {
		#region Player Scoring

		if (Player1 == null && !gameOver) {
			Player2Score++;

			//TODO: Add score SFX here

			//TODO: Update UI here

			//Spawn a new player if the game is not over
			if (Player2Score < WinScore) Player1 = SpawnPlayer(Player1Prefab);
			else Win(Player2);
		}

		if (Player2 == null && !gameOver) {
		Player1Score++;

			//TODO: Add score SFX here

			//TODO: Update UI here

			//Spawn a new player if the game is not over
			if (Player1Score < WinScore) Player2 = SpawnPlayer(Player2Prefab);
			else Win(Player1);
		}

		#endregion

		if (!Music.isPlaying && !gameOver) PlayMusic();

		#region Universal Controls

		//Volume up
		if (Input.GetKeyDown(KeyCode.Plus)) {
			Music.volume += 0.1f;
			if (Music.volume > 1) Music.volume = 1;
			Debug.Log($"[LevelManager] Music volume: {Music.volume}");
		}

		//Volume down
		else if (Input.GetKeyDown(KeyCode.Minus)) {
			Music.volume -= 0.1f;
			if (Music.volume < 0) Music.volume = 0;
			Debug.Log($"[LevelManager] Music volume: {Music.volume}");
		}

		//Quit game
		if (Input.GetKey(KeyCode.Escape)) {
			if (holdTimeRemaining > 0) {
				holdTimeRemaining -= Time.deltaTime;

				//Timer reached zero; exit the game
				if (holdTimeRemaining <= 0) {
					Debug.Log("[LevelManager] Quitting game...");
					Application.Quit(); //Doesn't work in editor mode
				}
			}
		}

		//Cancel quit game
		else if (Input.GetKeyUp(KeyCode.Escape)) {
			Debug.Log("[LevelManager] Cancelled quit game.");
			holdTimeRemaining = holdKeyDuration; //Reset timer
		}

		//Restart level
		if (Input.GetKey(KeyCode.R)) {
			if (holdTimeRemaining > 0) {
				holdTimeRemaining -= Time.deltaTime;

				//Timer reached zero; restart the level
				if (holdTimeRemaining <= 0) Restart();
			}
		}

		//Cancel restart level
		else if (Input.GetKeyUp(KeyCode.R)) {
			Debug.Log("[LevelManager] Cancelled restart level.");
			holdTimeRemaining = holdKeyDuration; //Reset timer
		}

		#endregion
	}

	/// <summary>
	/// How long the player has to press the exit button to quit the game.
	/// </summary>
	private float holdKeyDuration = 3;

	/// <summary>
	/// The remaining time the player has to press the exit button to quit the game.
	/// <br/><br/>
	/// Resets when the player releases the exit button.
	/// </summary>
	private float holdTimeRemaining = 3;

	private bool gameOver = false;
}
