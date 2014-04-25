using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerManager : MonoBehaviour{
	public GameObject otherPlayer; // Don't change it. It is set in the editor

	protected Dictionary<string, Player> remotePlayers;

	public float otherPlayerSpeed = 0.1f;

	private bool paused = false;

	// Initialization
	void Start () {
		remotePlayers = new Dictionary<string, Player>();
	}

	// Called every fixed framerate frame, if the MonoBehaviour is enabled.
	void FixedUpdate () {
		if (paused) {
			return ;
		}

		//Debug.Log ("Update all players locally");
		foreach (KeyValuePair<string, Player> pair in remotePlayers) {
			pair.Value.UpdateLocally(Time.fixedTime);
		}
	}

	public bool AddNewPlayer(string id, Vector3 startPos, float h, float v, Quaternion startRotation, int logicTime) {
		if (remotePlayers.ContainsKey(id)) {
			return false;
		}

		// Instantiate prefabs
		GameObject playerPrefab = Instantiate(otherPlayer, startPos, Quaternion.identity) as GameObject;
		//playerPrefab.transform.rotation = Quaternion.LookRotation(startDirection);
		playerPrefab.AddComponent<CapsuleCollider>();
		playerPrefab.AddComponent<TrailRenderer>();
		Debug.Log("Instantiate prefab for player " + id + " complete");

		// Create player
		float curTime = Time.time;
		Player newPlayer = new Player();
		newPlayer.SetStartState(playerPrefab, otherPlayerSpeed, id, h, v, startPos, logicTime, Time.fixedTime);
		remotePlayers.Add(id, newPlayer);
		Debug.Log("Initate player " + id + " complete");

		return true;
	}

	public bool RemovePlayer(string id) {
		if (!remotePlayers.ContainsKey(id)) {
			return false;
		}

		// Delete player
		remotePlayers[id].DestroyAllGameObject();
		remotePlayers.Remove(id);
		Debug.Log("Remove player " + id + " complete");

		return true;
	}

	public bool updatePlayerBasedOnNetwork(string id, Vector3 pos, Vector3 movement,
	                                       float h, float v, Quaternion rotation, int curLogicTime) {
		if (paused) {
			return false;
		}

		if (!remotePlayers.ContainsKey(id)){
			return false;
		}

		remotePlayers[id].UpdateBasedOnNetwork(pos, movement, h, v, rotation, curLogicTime, Time.time);
		return true;
	}

	public bool isPaused() {
		return paused;
	}

	public void setPauseState(bool state) {
		paused = state;
	}
}
