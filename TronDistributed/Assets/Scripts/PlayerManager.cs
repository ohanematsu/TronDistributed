using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerManager : MonoBehaviour{
	public GameObject otherPlayer; // Don't change it. It is set in the editor

	protected Dictionary<int, Player> remotePlayers;

	public float otherPlayerSpeed = 0.1f;

	// Initialization
	void Start () {
		remotePlayers = new Dictionary<int, Player>();
	}

	// Called every fixed framerate frame, if the MonoBehaviour is enabled.
	void FixedUpdate () {
		Debug.Log("Try to update all players locally");
		float curTime = Time.time;
		for (int i = 0; i < remotePlayers.Count; i++) {
			remotePlayers[i].UpdateLocally(curTime);
		}
	}

	public bool AddNewPlayer(int id, Vector3 startPos, Vector3 startDirection) {
		if (remotePlayers.ContainsKey(id)) {
			return false;
		}

		// Instantiate prefabs
		GameObject playerPrefab = Instantiate(otherPlayer, startPos, Quaternion.identity) as GameObject;
		playerPrefab.transform.rotation = Quaternion.LookRotation(startDirection);
		playerPrefab.AddComponent<CapsuleCollider>();
		playerPrefab.AddComponent<TrailRenderer>();
		Debug.Log("Instantiate prefab for player " + id + " complete");

		// Create player
		float curTime = Time.time;
		Player newPlayer = new Player();
		newPlayer.SetStartState(id, playerPrefab, startPos, startDirection, otherPlayerSpeed, curTime);
		remotePlayers.Add(id, newPlayer);
		Debug.Log("Initate player " + id + " complete");

		return true;
	}

	public bool RemovePlayer(int id) {
		if (!remotePlayers.ContainsKey(id)) {
			return false;
		}

		// Delete player
		remotePlayers[id].DestroyAllGameObject();
		remotePlayers.Remove(id);
		Debug.Log("Remove player " + id + " complete");

		return true;
	}

	public bool updatePlayerBasedOnNetwork(int id, Vector3 direction, Vector3 movement) {
		if (!remotePlayers.ContainsKey(id)) {
			return false;
		}

		float curTime = Time.time;
		remotePlayers[id].UpdateBasedOnNetwork (direction, movement, curTime);

		return true;
	}
}
