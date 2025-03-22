using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
	#region Game Objects and Components

	[Header("Game Objects and Components")]
	public GameObject Player1Prefab;

	/// <summary>
	/// The exact Player 1 instance.
	/// </summary>
	public GameObject Player1;

	/// <summary>
	/// The Player 1 prefab to spawn.
	/// </summary>
	public GameObject Player2Prefab;

	/// <summary>
	/// The exact Player 2 instance.
	/// </summary>
	public GameObject Player2;

	/// <summary>
	/// The Player 2 prefab to spawn.
	/// </summary>
	public GameObject Spawnpoints;

	/// <summary>
	/// The missile prefab to spawn when a player goes out of bounds.
	/// </summary>
	public GameObject MissilePrefab;

    public TextMeshProUGUI WinnerText;

	public GameObject EndScreen;

	#endregion

	#region Scoring and Timers

	[Header("Scoring and Timers")]
	public int Player1Score = 0;

	public int Player2Score = 0;

	public int WinScore = 10;

	public float OutOfBoundsTimer = 5;

	private float player1BoundsCountdown = 0;

	private float player2BoundsCountdown = 0;

	/// <summary>
	/// How long the player has to press the button to activate.
	/// </summary>
	private float holdKeyDuration = 3;

	/// <summary>
	/// The remaining time the player has to press the exit button to quit the game.
	/// <br/><br/>
	/// Resets when the player releases the exit button.
	/// </summary>
	private float holdTimeRemaining = 3;

	private bool gameOver = false;

	#endregion

	#region Music and SFX

	[Header("Music and SFX")]
	public AudioSource Music;

	private AudioClip[] MusicTracks;

	public AudioSource ScoreSFX;

	public AudioSource RespawnSFX;

	public AudioSource VictorySFX;

	public AudioSource OutOfBoundsSFX;

	#endregion

	#region Methods

	/// <summary>
	/// It spawns the player at a random location.
	/// </summary>
	/// <param name="prefab"></param>
	/// <returns></returns>
	private GameObject SpawnPlayer(GameObject prefab) {
		LayerMask mask = LayerMask.GetMask("Default");
		float checkRadius = 2f;
		Vector2 spawnPosition;

		//Ensure the spawn position is not inside another object
		do {
			spawnPosition = new Vector2(Random.Range(-40, 40), Random.Range(-40, 40));
		} while (Physics2D.OverlapCircle(spawnPosition, checkRadius, mask));

		//TODO: Add respawn animation at the spawn position
		//TODO: Add respawn SFX at the spawn position

		GameObject newPlayer = Instantiate(prefab);

		//Rotates the player to a random angle
		newPlayer.transform.SetPositionAndRotation(spawnPosition, Quaternion.Euler(0, 0, Random.Range(0, 360)));

		//Removes the "(Clone)" string postfixed to objects spawned from prefabs.
		newPlayer.name = newPlayer.name.Replace("(Clone)", "");

		return newPlayer;
	}

	/// <summary>
	/// Plays a random track that isn't the same as the previous,
	/// if there isn't already one playing.
	/// </summary>
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
			while (Music.clip == newTrack || Music.clip.name == "Space Cruise");
			//Select a random track except for "Space Cruise". That's for the main menu.

			//They're all from FTL lmao
			Debug.Log($"[LevelManager] Now playing: Ben Prunty - {Music.clip.name}");

			Music.Play();
		}
		
		else Debug.LogWarning("[LevelManager] No music tracks found.");
	}

	/// <summary>
	/// Displays the winner screen with SFX and prevents further respawns.
	/// </summary>
	/// <param name="winner"></param>
	private void Win(GameObject winner) {
		gameOver = true;

        EndScreen.SetActive(true);
        WinnerText.text = winner.name + " Wins!";

        Music.Stop();

		//Disables the music component so that it doesn't play again
		Music.enabled = false; 

		VictorySFX.Play();
	}

	/// <summary>
	/// Reloads the scene.
	/// </summary>
	public void Restart() {
		Debug.Log("[LevelManager] Restarting level...");

		holdTimeRemaining = 3;

		gameOver = false;

		SceneManager.LoadScene(SceneManager.GetActiveScene().name);

		Music.enabled = true; //Re-enables the music component after the level is reloaded.

		Music.Stop(); //Stops the music when the level is reloaded.
					  //When the level is reloaded, new music will play.
	}

	/// <summary>
	/// Goes back to the main menu.
	/// </summary>
    public void Menu()
    {
        StartCoroutine(DelayedSceneLoad("Main Screen"));
	}

	#endregion

	#region Unity

	void Start() {
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

		//Music
		if (!Music.isPlaying && !gameOver) PlayMusic();

		#region Out of Bounds

		if (player1BoundsCountdown == 0) { } else if (player1BoundsCountdown > 0) player1BoundsCountdown -= Time.deltaTime;
		else if (player1BoundsCountdown < 0) {
			//Spawn missile
			GameObject missile = Instantiate(MissilePrefab);
			missile.transform.position = (Vector2) transform.position + Random.insideUnitCircle * 100;
			missile.GetComponent<Missile>().Target = Player1;
			player1BoundsCountdown = 0;
		}

		if (player2BoundsCountdown == 0) { } else if (player2BoundsCountdown > 0) player2BoundsCountdown -= Time.deltaTime;
		else if (player2BoundsCountdown < 0) {
			//Spawn missile
			GameObject missile = Instantiate(MissilePrefab);
			missile.transform.position = (Vector2) transform.position + Random.insideUnitCircle * 100;
			missile.GetComponent<Missile>().Target = Player2;
			player2BoundsCountdown = 0;
		}

		#endregion

		#region Universal Controls

		//Volume up
		if (Input.GetKeyDown(KeyCode.Equals)) {
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
					holdTimeRemaining = 3;
					Application.Quit(); //Doesn't work in editor mode
				}
			}
		}

		//Cancel quit game, or if it doesn't work.
		else if (Input.GetKeyUp(KeyCode.Escape)) {
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
			holdTimeRemaining = holdKeyDuration; //Reset timer
		}

		//Player 1 self-kill
		if (Input.GetKey(KeyCode.LeftBracket)) {
			if (holdTimeRemaining > 0) {
				holdTimeRemaining -= Time.deltaTime;

				//Timer reached zero; kill player.
				if (holdTimeRemaining <= 0) {
					Player1.GetComponent<Player>().Kill();
				}
			}
		}

		//Cancel Player 1 self-kill
		else if (Input.GetKeyUp(KeyCode.LeftBracket)) {
			holdTimeRemaining = holdKeyDuration; //Reset timer
		}

		//Player 2 self-kill
		if (Input.GetKey(KeyCode.RightBracket)) {
			if (holdTimeRemaining > 0) {
				holdTimeRemaining -= Time.deltaTime;

				//Timer reached zero; kill player.
				if (holdTimeRemaining <= 0) {
					Player2.GetComponent<Player>().Kill();
				}
			}
		}

		//Cancel Player 1 self-kill
		else if (Input.GetKeyUp(KeyCode.RightBracket)) {
			holdTimeRemaining = holdKeyDuration; //Reset timer
		}

		#endregion
	}

	private IEnumerator DelayedSceneLoad(string sceneName)
	{
		yield return new WaitForSeconds(0.1f);
		SceneManager.LoadScene(sceneName);
	}

	#region Out of bounds triggers

	private void OnTriggerExit2D(Collider2D collision) {
		//Player 1
		if (collision.gameObject == Player1) {
			if (Player1.GetComponent<Player>().Hitpoints <= 0) return;
			player1BoundsCountdown = OutOfBoundsTimer;

			if (!OutOfBoundsSFX.isPlaying) OutOfBoundsSFX.Play();
		}
		
		//Player 2
		else if (collision.gameObject == Player2) {
			if (Player2.GetComponent<Player>().Hitpoints <= 0) return;

			player2BoundsCountdown = OutOfBoundsTimer;

			if (!OutOfBoundsSFX.isPlaying) OutOfBoundsSFX.Play();
		}
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		//Player 1
		if (collision.gameObject == Player1) {
			player1BoundsCountdown = 0;

			OutOfBoundsSFX.Stop();
			//NOTE: This will deactivate the SFX if ONE player re-enters the area.
		}
		
		//Player 2
		else if (collision.gameObject == Player2) {
			player2BoundsCountdown = 0;

			OutOfBoundsSFX.Stop();
			//NOTE: This will deactivate the SFX if ONE player re-enters the area.
		}
	}

	#endregion

	#endregion
}
